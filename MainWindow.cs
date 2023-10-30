using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace SmartMove {

	public partial class MainWindow : Form {

		public MainWindow() {
			InitializeComponent();
			this.Altered(AlteredFlags.LabelsChanged);
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			this.Text = Application.ProductName;
			this.UpdateState(new AlbertLinkPortState());
		}

		public AlbertLink Link {
			private get;
			set;
		}

		private TextInputWindow buildProcedureTextInput = null;

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.UserClosing && this.Link != null) {
				e.Cancel = true;
				this.Link.Quit();
			}
		}

		public void Print(string value) {

			StringBuilder parts = new StringBuilder(value.Length);

			foreach (char c in value) {
				switch (c) {
					case (char)1: // Token begin
					case (char)2: // Token end
						break;
					case (char)12: // Clear screen
						if (parts.Length > 0) {
							this.PrintOutput.AppendText(parts.ToString());
							parts.Clear();
						}
						this.PrintOutput.Clear();
						break;
					case '\r': // New line
						parts.Append(Environment.NewLine);
						break;
					default:
						parts.Append(c);
						break;
				}
			}

			if (parts.Length > 0) {
				this.PrintOutput.AppendText(parts.ToString());
				parts.Clear();
			}

			
			this.PrintOutput.ScrollToCaret();
		}

		private AskType? askType = null;

		public void EnableCommandMode(bool focus = true) {
			this.getKeyQueue.Clear();
			this.askType = null;
			this.CommandPanel.Enabled = true;
			this.CommandInput.Clear();
			if (focus) this.CommandInput.Focus();
		}

		public void Ask(AskType type, string prompt) {
			this.EnableCommandMode();
			this.askType = type;
			this.Print(prompt);
		}

		private void SendCommand() {
			if (this.CommandPanel.Enabled) {
				if (this.askType.HasValue) {
					this.PrintOutput.AppendText(this.CommandInput.Text + Environment.NewLine);
					this.PrintOutput.ScrollToCaret();
					this.PrintOutput.Focus();
					this.CommandPanel.Enabled = false;
					this.Link?.AskBack(this.CommandInput.Text);
					this.CommandInput.Clear();
				} else {
					this.PrintOutput.AppendText(": " + this.Link.SteadyLine(this.CommandInput.Text, AlbertLink.LabelUse.UseLblsSetting).Replace("\x01", "").Replace("\x02", "") + Environment.NewLine);
					this.PrintOutput.ScrollToCaret();
					this.PrintOutput.Focus();
					this.CommandPanel.Enabled = false;
					this.Link?.SendCmd(this.CommandInput.Text);
					this.CommandInput.Clear();
				}
			}
		}

		private void SendButton_Click(object sender, EventArgs e) {
			this.SendCommand();
		}

		private void CommandInput_KeyPress(object sender, KeyPressEventArgs e) {
			if (this.CommandPanel.Enabled) {
				// If the command panel is enabled, we're typing in a command.
				if (e.KeyChar == '\r') {
					e.Handled = true;
					this.SendCommand();
				}
			} else {
				// If the command panel is disabled, a procedure is running so store the value in the queue.
				e.Handled = true;
				this.getKeyQueue.Enqueue(e.KeyChar);
			}
		}

		private void MainWindow_Activated(object sender, EventArgs e) {
			if (this.CommandPanel.Enabled) {
				this.CommandInput.Focus();
			}
		}

		private readonly string[] motorDirections = new string[] { "", "↻", "↺", "" };

		static void UpdateAnalogueSensor(AlbertLinkAnalogueSensor sensor, ProgressBar progress, Label value) {
			if (SmartBox.SensorIsPresent(sensor.ID)) {
				progress.Value = sensor.ADC;
				value.Text = sensor.ADC.ToString();
			} else {
				progress.Value = 0;
				value.Text = "";
			}
		}

		public void UpdateState(AlbertLinkPortState state) {

			// Update analogue sensors

			UpdateAnalogueSensor(state.SensorA, this.AnalogueAProgress, this.AnalogueAValue);
			UpdateAnalogueSensor(state.SensorB, this.AnalogueBProgress, this.AnalogueBValue);
			UpdateAnalogueSensor(state.SensorC, this.AnalogueCProgress, this.AnalogueCValue);
			UpdateAnalogueSensor(state.SensorD, this.AnalogueDProgress, this.AnalogueDValue);


			// Update digital sensors

			this.Sensor0.Checked = (state.Inputs & (1 << 0)) != 0;
			this.Sensor1.Checked = (state.Inputs & (1 << 1)) != 0;
			this.Sensor2.Checked = (state.Inputs & (1 << 2)) != 0;
			this.Sensor3.Checked = (state.Inputs & (1 << 3)) != 0;
			this.Sensor4.Checked = (state.Inputs & (1 << 4)) != 0;
			this.Sensor5.Checked = (state.Inputs & (1 << 5)) != 0;
			this.Sensor6.Checked = (state.Inputs & (1 << 6)) != 0;
			this.Sensor7.Checked = (state.Inputs & (1 << 7)) != 0;

			// Update digital outputs

			this.Output0.Checked = (state.Outputs & (1 << 0)) != 0;
			this.Output1.Checked = (state.Outputs & (1 << 1)) != 0;
			this.Output2.Checked = (state.Outputs & (1 << 2)) != 0;
			this.Output3.Checked = (state.Outputs & (1 << 3)) != 0;
			this.Output4.Checked = (state.Outputs & (1 << 4)) != 0;
			this.Output5.Checked = (state.Outputs & (1 << 5)) != 0;
			this.Output6.Checked = (state.Outputs & (1 << 6)) != 0;
			this.Output7.Checked = (state.Outputs & (1 << 7)) != 0;

			// Update motors

			this.MotorAValue.Text = motorDirections[(state.Motors >> 0) & 3];
			this.MotorBValue.Text = motorDirections[(state.Motors >> 2) & 3];
			this.MotorCValue.Text = motorDirections[(state.Motors >> 4) & 3];
			this.MotorDValue.Text = motorDirections[(state.Motors >> 6) & 3];

			// Status bar
			this.RunningStatus.Text = state.RunMode ? "▶" : "";
			this.ClockStatus.Text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", state.ClockHours, state.ClockMinutes, state.ClockSeconds, state.ClockCentiseconds);

			// Menu state
			this.RunToolStripMenuItem.Enabled = !state.RunMode;
			this.StopToolStripMenuItem.Enabled = state.RunMode;
		}

		const int ProceduresMenuDefaultItemCount = 4;

		public void Altered(AlteredFlags flags) {

			// Has the procedure list changed?
			if ((flags & AlteredFlags.ProcedureListChanged) != 0) {

				// Remove old procedures
				while (this.ProceduresToolStripMenuItem.DropDownItems.Count > ProceduresMenuDefaultItemCount) {
					this.ProceduresToolStripMenuItem.DropDownItems.RemoveAt(ProceduresMenuDefaultItemCount);
				}
				this.RunToolStripMenuItem.DropDownItems.Clear();

				foreach (var procedure in this.Link.List()) {
					if (this.ProceduresToolStripMenuItem.DropDownItems.Count <= ProceduresMenuDefaultItemCount) {
						this.ProceduresToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
					}
					var procedureToolStripItem = new ToolStripMenuItem(procedure) {
						Tag = procedure
					};
					procedureToolStripItem.Click += ProcedureToolStripItem_Click;
					this.ProceduresToolStripMenuItem.DropDownItems.Add(procedureToolStripItem);

					var runProcedureToolStripItem = new ToolStripMenuItem(procedure) {
						Tag = procedure
					};
					runProcedureToolStripItem.Click += RunProcedureToolStripItem_Click; ;
					this.RunToolStripMenuItem.DropDownItems.Add(runProcedureToolStripItem);

				}

				this.RunToolStripMenuItem.Enabled = this.RunToolStripMenuItem.DropDownItems.Count > 0;
			}

			// Have the labels changed?
			if ((flags & AlteredFlags.LabelsChanged) != 0) {

				this.AnalogueAName.Text = "SENSORA";
				this.AnalogueBName.Text = "SENSORB";
				this.AnalogueCName.Text = "SENSORC";
				this.AnalogueDName.Text = "SENSORD";
				this.Sensor0.Text = "SENSOR0";
				this.Sensor1.Text = "SENSOR1";
				this.Sensor2.Text = "SENSOR2";
				this.Sensor3.Text = "SENSOR3";
				this.Sensor4.Text = "SENSOR4";
				this.Sensor5.Text = "SENSOR5";
				this.Sensor6.Text = "SENSOR6";
				this.Sensor7.Text = "SENSOR7";
				this.Output0.Text = "OUTPUT0";
				this.Output1.Text = "OUTPUT1";
				this.Output2.Text = "OUTPUT2";
				this.Output3.Text = "OUTPUT3";
				this.Output4.Text = "OUTPUT4";
				this.Output5.Text = "OUTPUT5";
				this.Output6.Text = "OUTPUT6";
				this.Output7.Text = "OUTPUT7";
				this.MotorAName.Text = "MOTORA";
				this.MotorBName.Text = "MOTORB";
				this.MotorCName.Text = "MOTORC";
				this.MotorDName.Text = "MOTORD";

				if (this.Link != null) {
					foreach (var label in this.Link.ReadLabels()) {
						switch (label.OldName) {
							case "SENSORA": this.AnalogueAName.Text = label.NewName; break;
							case "SENSORB": this.AnalogueBName.Text = label.NewName; break;
							case "SENSORC": this.AnalogueCName.Text = label.NewName; break;
							case "SENSORD": this.AnalogueDName.Text = label.NewName; break;
							case "SENSOR0": this.Sensor0.Text = label.NewName; break;
							case "SENSOR1": this.Sensor1.Text = label.NewName; break;
							case "SENSOR2": this.Sensor2.Text = label.NewName; break;
							case "SENSOR3": this.Sensor3.Text = label.NewName; break;
							case "SENSOR4": this.Sensor4.Text = label.NewName; break;
							case "SENSOR5": this.Sensor5.Text = label.NewName; break;
							case "SENSOR6": this.Sensor6.Text = label.NewName; break;
							case "SENSOR7": this.Sensor7.Text = label.NewName; break;
							case "OUTPUT0": this.Output0.Text = label.NewName; break;
							case "OUTPUT1": this.Output1.Text = label.NewName; break;
							case "OUTPUT2": this.Output2.Text = label.NewName; break;
							case "OUTPUT3": this.Output3.Text = label.NewName; break;
							case "OUTPUT4": this.Output4.Text = label.NewName; break;
							case "OUTPUT5": this.Output5.Text = label.NewName; break;
							case "OUTPUT6": this.Output6.Text = label.NewName; break;
							case "OUTPUT7": this.Output7.Text = label.NewName; break;
							case "MOTORA": this.MotorAName.Text = label.NewName; break;
							case "MOTORB": this.MotorBName.Text = label.NewName; break;
							case "MOTORC": this.MotorCName.Text = label.NewName; break;
							case "MOTORD": this.MotorDName.Text = label.NewName; break;
						}
					}
				}
			}
		}

		private void ProcedureToolStripItem_Click(object sender, EventArgs e) {
			if (sender is ToolStripItem procedureToolStripItem) {
				if (procedureToolStripItem.Tag is string procedure) {
					this.Link.Host.EditProcedure(false, procedure);
				}
			}
		}

		private void QuitToolStripMenuItem_Click(object sender, EventArgs e) {
			this.Link?.Quit();
		}

		private void DisconnectToolStripMenuItem_Click(object sender, EventArgs e) {
			this.Link?.Sleep();
		}
		private void RunProcedureToolStripItem_Click(object sender, EventArgs e) {
			if (sender is ToolStripItem procedureToolStripItem) {
				if (procedureToolStripItem.Tag is string procedure && this.CommandPanel.Enabled) {
					this.CommandPanel.Enabled = false;
					this.Link.SendCmd(procedure);
				}
			}
		}

		private readonly Queue<char> getKeyQueue = new Queue<char>(4);

		public char GetKey() {
			if (getKeyQueue.Count > 0) {
				return getKeyQueue.Dequeue();
			} else {
				return (char)0;
			}
		}

		public void LoadFile(string filename) {
			if (string.IsNullOrEmpty(filename)) {
				this.OpenDialog.ShowDialog();
			} else {
				// Append the default .txt extension if required
				if (!File.Exists(filename) && File.Exists(filename + ".txt")) {
					filename += ".txt";
				}
				// Check if the file exists
				if (!File.Exists(filename)) {
					this.Link.Error("File not found");
				} else {
					// Try to load the file
					try {
						var file = File.ReadAllLines(filename);
						
						bool readingProcedureName = true;

						string procedureName = null;
						List<string> procedureBody = new List<string>();

						foreach (var line in file) {
							if (line.Length == 0) {
								readingProcedureName = true;
								if (!string.IsNullOrEmpty(procedureName) && procedureBody.Count > 0) {
									switch (this.Link.PutProcedure(procedureName, string.Join("\r", procedureBody.ToArray()))) {
										case PutProcedureResult.OK:
											this.Print(string.Format("Loaded procedure '{0}'\r", procedureName));
											break;
									}
									procedureName = null;
									procedureBody.Clear();
								}
							} else if (readingProcedureName) {
								if (line.Contains("=")) {
									var labelParts = line.Split('=');
									var labelName = labelParts[0];
									var labelValue = labelParts[1];
									this.Link.WriteLabel(new AlbertLinkLabel(labelName, labelValue, false));
								} else {
									procedureName = line;
									readingProcedureName = false;
								}
							} else {
								procedureBody.Add(line);
							}
						}


						if (!readingProcedureName && !string.IsNullOrEmpty(procedureName) && procedureBody.Count > 0) {
							this.Link.PutProcedure(procedureName, string.Join("\r", procedureBody.ToArray()));
							procedureName = null;
							procedureBody.Clear();
						}

					} catch (Exception ex) { 
						this.Link.Error(ex.Message);
					}
				}
			}
		}

		public void SaveFile(string filename, string procedure) {
			if (string.IsNullOrEmpty(filename)) {
				this.SaveDialog.ShowDialog();
			} else {
				if (string.IsNullOrEmpty(Path.GetExtension(filename))) {
					filename += ".txt";
				}
				try {
					using (var saveFile = File.CreateText(filename)) {
						string[] procedures;

						if (string.IsNullOrEmpty(procedure)) {

							// Save the labels first
							var labels = this.Link.ReadLabels();
							if (labels.Length > 0) {
								foreach (var label in labels) {
									if (!label.Soft) {
										saveFile.WriteLine("{0}={1}", label.OldName, label.NewName);
									}
								}
								saveFile.WriteLine();
							}

							// Get the list of procedures to save.
							procedures = this.Link.List();
						} else {
							// We're only saving one procedure.
							procedures = new string[] { procedure };
						}
						foreach (var procedureToSave in procedures) {
							saveFile.WriteLine(procedureToSave);
							foreach (var lineToSave in this.Link.GetProcedure(procedureToSave).Split('\r')) {
								var trimmedLine = lineToSave.Replace("\x01", "").Replace("\x02", "").Trim();
								if (!string.IsNullOrEmpty(trimmedLine)) {
									saveFile.WriteLine(trimmedLine);
								}
							}
							saveFile.WriteLine();
						}
					}
				} catch (Exception ex) {
					this.Link.Error(ex.Message);
				}
			}
		}

		private void OpenDialog_FileOk(object sender, CancelEventArgs e) {
			if (!e.Cancel && sender is OpenFileDialog dialog) {
				this.LoadFile(dialog.FileName);
			}
		}

		private void NewToolStripMenuItem_Click(object sender, EventArgs e) {
			if (this.CommandPanel.Enabled && !this.askType.HasValue) {
				this.CommandPanel.Enabled = false;
				this.Link.SendCmd("NEW");
			}
		}

		private void OpenToolStripMenuItem_Click(object sender, EventArgs e) {
			this.LoadFile("");
		}

		private void SaveToolStripMenuItem_Click(object sender, EventArgs e) {
			this.SaveFile("", "");
		}

		private void SaveDialog_FileOk(object sender, CancelEventArgs e) {
			if (!e.Cancel && sender is SaveFileDialog dialog) {
				this.SaveFile(dialog.FileName, "");
			}
		}

		private void BuildToolStripMenuItem_Click(object sender, EventArgs e) {
			if (this.buildProcedureTextInput == null || this.buildProcedureTextInput.IsDisposed) {
				this.buildProcedureTextInput = new TextInputWindow {
					Description = "Enter a name for the new procedure:",
				};
				this.buildProcedureTextInput.TextInputField.MaxLength = 15;
				this.buildProcedureTextInput.FormClosing += BuildProcedureTextInput_FormClosing;
			}
			this.buildProcedureTextInput.Show();
			this.buildProcedureTextInput.Focus();
			this.buildProcedureTextInput.TextInputField.Focus();
		}

		private void BuildProcedureTextInput_FormClosing(object sender, FormClosingEventArgs e) {
			if (sender is TextInputWindow textInput) {
				if (e.CloseReason == CloseReason.UserClosing && textInput.DialogResult == DialogResult.OK && textInput.TextInputField.Text.Trim().Length > 0) {
					this.Link.Host.EditProcedure(true, textInput.TextInputField.Text.Trim());
				}
			}
		}


		private void Output_Click(object sender, EventArgs e) {
			if (sender is CheckBox outputCheckbox) {
				int outputIndex = int.Parse(outputCheckbox.Name[outputCheckbox.Name.Length - 1].ToString());
				var mask = (byte)(1 << outputIndex);
				this.Link.SetPort(outputCheckbox.Checked ? (byte)0 : mask, (byte)(~mask));
			}
        }

		private void StopToolStripMenuItem_Click(object sender, EventArgs e) {
			this.Link?.Escape();
		}

		protected override bool IsInputKey(Keys keyData) {
			if (keyData == Keys.Escape) return true;
			return base.IsInputKey(keyData);
		}

		private void MainWindow_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)0x1B) {
				e.Handled = true;
				this.CommandPanel.Enabled = false;
				this.Link?.Escape();
			}
		}
	}
}
