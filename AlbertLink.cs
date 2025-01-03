﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SmartMove {

	public struct AlbertLinkAnalogueSensor {
		public byte ADC;
		public byte ID;
	};

	public struct AlbertLinkPortState {
		public bool RunMode;
		public byte Inputs;
		public byte Outputs;
		public byte Motors;
		public AlbertLinkAnalogueSensor SensorA;
		public AlbertLinkAnalogueSensor SensorB;
		public AlbertLinkAnalogueSensor SensorC;
		public AlbertLinkAnalogueSensor SensorD;
		public byte ClockHours;
		public byte ClockMinutes;
		public byte ClockSeconds;
		public byte ClockCentiseconds;
	};

	public struct AlbertLinkLabel {
		public string OldName;
		public string NewName;
		public bool Soft;
		public AlbertLinkLabel(string oldName, string newName, bool soft) {
			this.OldName = oldName;
			this.NewName = newName;
			this.Soft = soft;
		}
	};

	[Flags]
	public enum SetupFlags : byte {
		None = 0,
		NewTraceSystem = 1 << 0,
		EnableProcedureLabelChangeChecking = 1 << 1,
		EnableCustomCommands = 1 << 2,
		EnablePrompt = 1 << 3,
		ShitComputer = 1 << 4,
		EnableGetLineCount = 1 << 5,
	};

	public enum AskType : byte {
		String = 0,
		Number = 1,
		Time = 2,
	};

	public enum PutProcedureResult : byte {
		OK = 0,
		BadName = 1,
		NoRoom = 2,
		BadData = 3,
	};

	public enum WriteLabelResult : byte {
		OK = 0,
		BadSourceLabel = 1,
		BadLabel = 2,
		LabelTooLong = 3,
		LabelExistsAsProcedure = 4,
		CannotOverwriteHardWithSoft = 5,
	};

	[Flags]
	public enum AlteredFlags : byte { 
		None = 0,
		ProcedureListChanged = 1 << 0,
		LabelsChanged = 1 << 1,
	};

	public class AlbertLink : IDisposable {

		private readonly SmartBox smartBox;
		private readonly IAlbertLinkHost host;

		public IAlbertLinkHost Host {
			get { return host; }
		}

		private bool running = false;

		private TimeSpan samplePortsInterval = TimeSpan.FromMilliseconds(50);
		private TimeSpan updateSensorsInterval = TimeSpan.FromMilliseconds(1000);
		private DateTime nextSampleTime = DateTime.Now;
		private DateTime lastUpdateSensorsTime = DateTime.MinValue;

		private readonly byte[] sensorIDs = new byte[4];
		
		private bool disposedValue;

		public AlbertLink(SmartBox smartBox, IAlbertLinkHost host) {
			this.smartBox = smartBox;
			this.host = host;
		}

		public void Run() {

			// Initialise the host
			this.host.Initialize(this);

			// Is AlbertLink installed?
			var albertLinkCall = this.smartBox.GetNameCode("AlbertLink");

			if (albertLinkCall == 0) {

				// Retrieve the program from the host.
				var program = this.host.GetAlbertLinkProgram();

				// No, so we need to load it.
				ushort lomem = this.smartBox.ReadLomem(), himem = this.smartBox.ReadHimem();

				if (program.Length > (himem - lomem) + 1) {
					return;
				}

				// Replace this.DownloadData(lomem, program); with loop to allow for progress reports
				int chunkSize = 128;
				for (int offset = 0; offset < program.Length; offset += chunkSize) {
					this.host.UpdateConnectionProgress(offset, program.Length);
					this.host.Idle();
					int length = Math.Min(chunkSize, program.Length - offset);
					this.smartBox.port.Write((byte)SmartBox.Command.DownloadData);
					this.smartBox.port.Write((ushort)(lomem + offset));
					this.smartBox.port.Write((ushort)length);
					this.smartBox.port.Write(program, offset, length);
				}
				this.host.UpdateConnectionProgress(program.Length, program.Length);
				this.host.Idle();

				ushort entrypoint = (ushort)(lomem + program[0] + program[1] * 256);

				this.smartBox.ExecuteCode(entrypoint, 0, (byte)entrypoint, (byte)(entrypoint / 256));
				this.smartBox.port.ReadByte();

				albertLinkCall = this.smartBox.GetNameCode("AlbertLink");

			}

			if (albertLinkCall == 0) {
				return;
			}

			// Invoke AlbertLink
			this.smartBox.port.Write(albertLinkCall);

			this.smartBox.port.ReadByte();
			this.smartBox.port.ReadByte();

			// Setup with appropriate flags
			this.smartBox.port.Write((byte)1);
			this.smartBox.port.Write((byte)(SetupFlags.EnableProcedureLabelChangeChecking));

			// Show the sign-on message
			this.host.ShowSignOn();

			// Trigger reading labels
			this.host.Altered(AlteredFlags.ProcedureListChanged | AlteredFlags.LabelsChanged);

			running = true;

			while (running) {
				var now = DateTime.Now;
				UpdateEvent evt;
				if (this.smartBox.port.BytesToRead < 0) {
					running = false;
					break;
				} else if (this.smartBox.port.BytesToRead > 0) {
					switch (evt = (UpdateEvent)this.smartBox.port.ReadByte()) {
						case UpdateEvent.File:
							this.smartBox.port.Write((byte)0);
							this.FileBack(this.host.File(this.smartBox.port.ReadByte(), this.smartBox.port.ReadString()));
							break;
						case UpdateEvent.Close:
							this.smartBox.port.Write((byte)0);
							this.host.Close(this.smartBox.port.ReadByte());
							break;
						case UpdateEvent.Store:
							this.smartBox.port.Write((byte)0);
							this.host.Store(this.smartBox.port.ReadByte(), this.smartBox.port.ReadString(0));
							break;
						case UpdateEvent.Trace:
							this.smartBox.port.Write((byte)0);
							this.host.Trace(this.smartBox.port.ReadString(13));
							break;
						case UpdateEvent.Print:
							this.smartBox.port.Write((byte)0);
							this.host.Print(this.smartBox.port.ReadString(0));
							break;
						case UpdateEvent.Error:
							this.smartBox.port.Write((byte)0);
							var procedure = this.smartBox.port.ReadString();
							var message = this.smartBox.port.ReadString();
							var line = this.smartBox.port.ReadString();
							if (!this.host.DisplayError(procedure, message, line)) {
								if (!string.IsNullOrEmpty(procedure)) {
									this.host.Print(procedure + " : ");
								}
								this.host.Print(message);
								if (!string.IsNullOrEmpty(line)) {
									this.host.Print(" (" + line + ")");
								}
								this.host.Print("\r");
							}
							break;
						case UpdateEvent.Ask:
							this.smartBox.port.Write((byte)0);
							this.host.Ask((AskType)this.smartBox.port.ReadByte(), this.smartBox.port.ReadString());
							break;
						case UpdateEvent.Inkey:
							this.smartBox.port.Write((byte)0);
							this.smartBox.port.Write((byte)this.host.GetKey());
							break;
						case UpdateEvent.Cmd:
							this.smartBox.port.Write((byte)0);
							this.host.EnableCommandMode();
							break;
						case UpdateEvent.Build:
							this.smartBox.port.Write((byte)0);
							this.host.EditProcedure(true, this.smartBox.port.ReadString());
							break;
						case UpdateEvent.Edit:
							this.smartBox.port.Write((byte)0);
							this.host.EditProcedure(false, this.smartBox.port.ReadString());
							break;
						case UpdateEvent.Quit:
							this.smartBox.port.Write((byte)0);
							running = false;
							this.host.Quit();
							break;
						case UpdateEvent.TraceFl:
							this.smartBox.port.Write((byte)0);
							this.host.SetTraceFlag(this.smartBox.port.ReadByte() != 0);
							break;
						case UpdateEvent.Load:
							this.smartBox.port.Write((byte)0);
							this.host.Load(this.smartBox.port.ReadString());
							break;
						case UpdateEvent.Save:
							this.smartBox.port.Write((byte)0);
							this.host.Save(this.smartBox.port.ReadString(), this.smartBox.port.ReadString());
							break;
						case UpdateEvent.Control:
							this.smartBox.port.Write((byte)0);
							this.smartBox.port.Write(this.host.Control(this.smartBox.port.ReadByte()));
							break;
						case UpdateEvent.Rtc:
							this.smartBox.port.Write((byte)0);
							this.smartBox.port.Write((byte)now.Hour);
							this.smartBox.port.Write((byte)now.Minute);
							this.smartBox.port.Write((byte)now.Second);
							this.smartBox.port.Write((byte)(now.Millisecond / 10));
							break;
						case UpdateEvent.Printer:
							this.smartBox.port.Write((byte)0);
							this.host.Print(this.smartBox.port.ReadString(0));
							break;
						case UpdateEvent.Altered:
							this.smartBox.port.Write((byte)0);
							this.host.Altered((AlteredFlags)this.smartBox.port.ReadByte());
							break;
						default:
							Console.WriteLine("Unsupported event {0}", evt);
							break;
					}
				} else if (now >= nextSampleTime) {
					AlbertLinkPortState state;
					if ((DateTime.Now - lastUpdateSensorsTime) > updateSensorsInterval) {
						state = this.GetPortState(true);
						lastUpdateSensorsTime = now;
					} else {
						state = this.GetPortState(false);
					}
					this.host.UpdateState(state);
					nextSampleTime = now + samplePortsInterval;
				}
				this.host.Idle();
			}
		}

		enum UpdateEvent : byte {
			None = 0,
			File = 1,
			Close = 2,
			Store = 3,
			Trace = 4,
			Print = 5,
			Error = 6,
			Ask = 7,
			Inkey = 8,
			Cmd = 9,
			Build = 10,
			Edit = 11,
			Quit = 12,
			TraceFl = 13,
			Load = 14,
			Save = 15,
			Control = 16,
			Rtc = 17,
			Printer = 18,
			Altered = 19,
			Custom = 20,
			Custom2 = 21,
			CustomFn = 22,
			Prompt = 23,
		}

		enum RemoteEvent : byte {
			Setup = 1,
			List = 2,
			NameCode = 3,
			Get = 4,
			Put = 5,
			Escape = 6,
			Quit = 7,
			Cmd = 8,
			GetPorts = 9,
			GetPS = 10,
			SteadyLine = 11,
			TraceFl = 12,
			SetPort = 13,
			Error = 14,
			Version = 15,
			Sleep = 16,
			CheckSensors = 17,
			AskBack = 18,
			ReadLabels = 19,
			WriteLabel = 20,
			FreeMem = 21,
			TraceCont = 22,
			Clock = 23,
			PromptBack = 24,
			FileBack = 25,
		}

		private void SendRemoteEvent(RemoteEvent evt) {
			this.smartBox.port.Write((byte)evt);
			while (this.smartBox.port.ReadByte() != 0) ;
		}

		public void SendCmd(string command) {
			this.SendRemoteEvent(RemoteEvent.Cmd);
			this.smartBox.port.Write(Encoding.ASCII.GetBytes(command));
			this.smartBox.port.Write((byte)13);
		}

		public void AskBack(string response) {
			this.SendRemoteEvent(RemoteEvent.AskBack);
			this.smartBox.port.Write(Encoding.ASCII.GetBytes(response));
			this.smartBox.port.Write((byte)13);
		}

		public enum LabelUse : byte {
			DoNotUseLabels = 0,
			UseLabels = 1,
			UseLblsSetting = 255,
		}

		public string SteadyLine(string line, LabelUse flag) {
			this.SendRemoteEvent(RemoteEvent.SteadyLine);
			this.smartBox.port.Write((byte)flag);
			this.smartBox.port.Write(Encoding.ASCII.GetBytes(line));
			this.smartBox.port.Write(13);
			return this.smartBox.port.ReadString();
		}

		public void Escape() {
			this.SendRemoteEvent(RemoteEvent.Escape);
		}

		public void Quit() {
			this.SendRemoteEvent(RemoteEvent.Quit);
			this.running = false;
		}

		public void Sleep() {
			this.SendRemoteEvent(RemoteEvent.Sleep);
			this.running = false;
		}

		public AlbertLinkPortState GetPortState(bool updateSensorIDs) {
			this.SendRemoteEvent(updateSensorIDs ? RemoteEvent.GetPS : RemoteEvent.GetPorts);
			var portState = this.smartBox.port.ReadBytes(12);
			if (updateSensorIDs) {
				// Check to see if the sensors have changed.
				var newSensorIDs = this.smartBox.port.ReadBytes(4);
				for (int sensor = 0; sensor < this.sensorIDs.Length; ++sensor) {
					if (newSensorIDs[sensor] != this.sensorIDs[sensor]) {
						this.sensorIDs[sensor] = newSensorIDs[sensor];
						var oldSensorName = "SENSOR" + (char)((int)'A' + sensor);
						var newSensorName = SmartBox.GetSensorName(newSensorIDs[sensor]);
						if (string.IsNullOrEmpty(newSensorName)) {
							this.WriteLabel(new AlbertLinkLabel(oldSensorName, null, true));
						} else {
							this.WriteLabel(new AlbertLinkLabel(oldSensorName, newSensorName.ToUpperInvariant(), true));
						}
					}
				}
			}
			return new AlbertLinkPortState {
				RunMode = portState[0] != 0,
				Inputs = portState[1],
				Outputs = portState[2],
				Motors = portState[3],
				SensorA = new AlbertLinkAnalogueSensor { ADC = portState[4], ID = this.sensorIDs[0] },
				SensorB = new AlbertLinkAnalogueSensor { ADC = portState[5], ID = this.sensorIDs[1] },
				SensorC = new AlbertLinkAnalogueSensor { ADC = portState[6], ID = this.sensorIDs[2] },
				SensorD = new AlbertLinkAnalogueSensor { ADC = portState[7], ID = this.sensorIDs[3] },
				ClockHours = portState[8],
				ClockMinutes = portState[9],
				ClockSeconds = portState[10],
				ClockCentiseconds = portState[11],
			};
		}

		public string GetProcedure(string procedureName) {
			this.SendRemoteEvent(RemoteEvent.Get);
			this.smartBox.port.Write((byte)0xFF);
			this.smartBox.port.Write((string)procedureName);
			string code = null;
			byte status = this.smartBox.port.ReadByte();
			if (status != 0) {
				code = (char)status + this.smartBox.port.ReadString(0xFF);
			}
			return code;
        }

		public PutProcedureResult PutProcedure(string procedureName, string code) {
			this.SendRemoteEvent(RemoteEvent.Put);
			this.smartBox.port.Write(procedureName);
			this.smartBox.port.Write(code.Trim() + "\r", 0xFF);
			return (PutProcedureResult)this.smartBox.port.ReadByte();
		}

		public void SetTraceFlag(bool flag) {
			this.SendRemoteEvent(RemoteEvent.TraceFl);
			this.smartBox.port.Write((byte)(flag ? 1 : 0));
		}

		public AlbertLinkLabel[] ReadLabels() {

			var labels = new List<AlbertLinkLabel>(4);

			this.SendRemoteEvent(RemoteEvent.ReadLabels);
			string source;
			while (!string.IsNullOrEmpty(source = this.smartBox.port.ReadString())) {

				var s = new List<byte>(8);
				byte b;
				while (((b = this.smartBox.port.ReadByte()) & 0x7F) != 0) {
					s.Add(b);
				}
				var destination = Encoding.ASCII.GetString(s.ToArray());

				labels.Add(new AlbertLinkLabel(source, destination, b == 128));
			}

			return labels.ToArray();
		}

		public WriteLabelResult WriteLabel(AlbertLinkLabel label) {

			this.SendRemoteEvent(RemoteEvent.WriteLabel);
			this.smartBox.port.Write((string)label.OldName);
			this.smartBox.port.Write((string)label.NewName);
			this.smartBox.port.Write((byte)(label.Soft ? 0x80 : 0x00));

			return (WriteLabelResult)this.smartBox.port.ReadByte();
		}

		public string GetVersion() {
			this.SendRemoteEvent(RemoteEvent.Version);
			return this.smartBox.port.ReadString();
		}

		public ushort GetFreeMem() {
			this.SendRemoteEvent(RemoteEvent.FreeMem);
			return this.smartBox.port.ReadUInt16();
		}

		public void Error(string error) {
			this.SendRemoteEvent(RemoteEvent.Error);
			this.smartBox.port.Write((string)error);
		}

		public string[] List() {
			var procedures = new List<string>();
			this.SendRemoteEvent(RemoteEvent.List);
			string procedure;
			while (!string.IsNullOrEmpty(procedure = this.smartBox.port.ReadString())) {
				procedures.Add(procedure);
			}
			return procedures.ToArray();
		}

		public void FileBack(bool success) {
			this.SendRemoteEvent(RemoteEvent.FileBack);
			this.smartBox.port.Write((byte)(success ? 1 : 0));
		}

		public void SetPort(byte value, byte mask) {
			this.SendRemoteEvent(RemoteEvent.SetPort);
			this.smartBox.port.Write((byte)value);
			this.smartBox.port.Write((byte)mask);
		}

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					this.smartBox?.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
