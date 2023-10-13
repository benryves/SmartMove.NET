using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace SmartBox {

	public partial class MainWindow : Form {

		public MainWindow() {
			this.Text = Application.ProductName;
			InitializeComponent();
		}

		public AlbertLink Link {
			private get;
			set;
		}

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
			if (this.Link != null) {
				e.Cancel = true;
				this.Link.Sleep();
			}
		}

		public void Print(char c) {
			switch (c) {
				case (char)1: // Token begin
				case (char)2: // Token end
					break;
				case (char)12: // Clear screen
					this.PrintOutput.Clear();
					break;
				case '\r': // New line
					this.PrintOutput.AppendText(Environment.NewLine);
					break;
				default:
					this.PrintOutput.AppendText(c.ToString());
					break;
			}
			this.PrintOutput.ScrollToCaret();
		}

		public void EnableCommandMode(bool focus = true) {
			this.CommandPanel.Enabled = true;
			this.CommandInput.Clear();
			if (focus) this.CommandInput.Focus();
		}

		private void SendCommand() {
			if (this.CommandPanel.Enabled) {
				this.PrintOutput.AppendText(": " + this.CommandInput.Text + Environment.NewLine);
				this.PrintOutput.ScrollToCaret();
				this.PrintOutput.Focus();
				this.CommandPanel.Enabled = false;
				this.Link?.SendCmd(this.CommandInput.Text);
				this.CommandInput.Clear();
			}
		}

		private void SendButton_Click(object sender, EventArgs e) {
			this.SendCommand();
		}

		private void CommandInput_KeyPress(object sender, KeyPressEventArgs e) {
			switch (e.KeyChar) {
				case (char)0x1B:
					e.Handled = true;
					this.Link?.Escape();
					break;
				case '\r':
					e.Handled = true;
					this.SendCommand();
					break;
			}
		}

		private void MainWindow_Activated(object sender, EventArgs e) {
			if (this.CommandPanel.Enabled) {
				this.CommandInput.Focus();
			}
		}

		private readonly string[] motorDirections = new string[] { "", "↻", "↺", "" };

		private readonly string[] sensorTypes = new string[] {
			"No sensor",
			"Sensor $",
			"Temp",
			"No sensor",
			"No sensor",
			"Volts",
			"Temp",
			"Volts",
			"Temp",
			"Sound",
			"PH",
			"Sensor $",
			"Position  ",
			"Sensor $",
			"Sensor $",
			"Sensor $",
			"Light",
			"Sensor $",
			"Sensor $",
			"Sensor $",
			"Humidity",
			"Sound",
			"Light",
			"Sound",
			"Sensor $",
			"Atmos",
			"Light",
			"User",
			"Adaptor",
			"Temp",
			"LGate",
			"Sensor $",
			"Temp",
		};

		public void UpdateState(AlbertLinkPortState state) {

			// Update analogue sensors

			if (state.SensorA.ID == 0) {
				this.AnalogueAProgress.Value = 0;
				this.AnalogueAValue.Text = "";
				this.AnalogueAName.Text = sensorTypes[0];
			} else {
				this.AnalogueAProgress.Value = state.SensorA.ADC;
				this.AnalogueAValue.Text = state.SensorA.ADC.ToString();
				this.AnalogueAName.Text = sensorTypes[state.SensorA.ID - 1].Replace('$', 'A');
			}

			if (state.SensorB.ID == 0) {
				this.AnalogueBProgress.Value = 0;
				this.AnalogueBValue.Text = "";
				this.AnalogueBName.Text = sensorTypes[0];
			} else {
				this.AnalogueBProgress.Value = state.SensorB.ADC;
				this.AnalogueBValue.Text = state.SensorB.ADC.ToString();
				this.AnalogueBName.Text = sensorTypes[state.SensorB.ID - 1].Replace('$', 'B'); ;
			}

			if (state.SensorC.ID == 0) {
				this.AnalogueCProgress.Value = 0;
				this.AnalogueCValue.Text = "";
				this.AnalogueCName.Text = sensorTypes[0];
			} else {
				this.AnalogueCProgress.Value = state.SensorC.ADC;
				this.AnalogueCValue.Text = state.SensorC.ADC.ToString();
				this.AnalogueCName.Text = sensorTypes[state.SensorC.ID - 1].Replace('$', 'C'); ;
			}

			if (state.SensorD.ID == 0) {
				this.AnalogueDProgress.Value = 0;
				this.AnalogueDValue.Text = "";
				this.AnalogueDName.Text = sensorTypes[0];
			} else {
				this.AnalogueDProgress.Value = state.SensorD.ADC;
				this.AnalogueDValue.Text = state.SensorD.ADC.ToString();
				this.AnalogueDName.Text = sensorTypes[state.SensorD.ID - 1].Replace('$', 'D'); ;
			}

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
		}
	}
}
