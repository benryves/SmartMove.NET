using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmartBox {

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

	public class AlbertLink : SmartBox {

		private IAlbertLinkHost host;

		private bool running = false;

		private TimeSpan samplePortsInterval = TimeSpan.FromMilliseconds(50);
		private TimeSpan updateSensorsInterval = TimeSpan.FromMilliseconds(1000);
		private DateTime nextSampleTime = DateTime.Now;
		private DateTime lastUpdateSensorsTime = DateTime.MinValue;

		byte[] sensorIDs = new byte[4];

		public AlbertLink(SerialPort port, IAlbertLinkHost host) : base(port) {
			this.host = host;
		}

		public AlbertLink(string portName, IAlbertLinkHost host) : base(portName) {
			this.host = host;
		}

		public void Run() {

			// Initialise the host
			this.host.Initialize(this);

			// Is AlbertLink installed?
			var albertLinkCall = this.GetNameCode("AlbertLink");

			if (albertLinkCall == 0) {

				// Retrieve the program from the host.
				var program = this.host.GetAlbertLinkProgram();

				// No, so we need to load it.
				ushort lomem = this.ReadLomem(), himem = this.ReadHimem();

				if (program.Length > (himem - lomem) + 1) {
					throw new OutOfMemoryException();
				}

				this.DownloadData(lomem, program);

				ushort entrypoint = (ushort)(lomem + program[0] + program[1] * 256);

				this.ExecuteCode(entrypoint, 0, (byte)entrypoint, (byte)(entrypoint / 256));

				byte response = this.reader.ReadByte();

				albertLinkCall = this.GetNameCode("AlbertLink");

			}

			if (albertLinkCall == 0) throw new InvalidOperationException();

			// Invoke AlbertLink
			this.writer.Write(albertLinkCall);

			var engineNumber = this.reader.ReadByte();
			var setupFlags = this.reader.ReadByte();

			// Setup with no flags set
			this.writer.Write((byte)1);
			this.writer.Write((byte)0);

			// Get the version
			this.writer.Write((byte)15);
			while (this.reader.ReadByte() != 0) ;

			var version = this.ReadString();

			// Get the amount of free memory
			this.writer.Write((byte)21);
			while (this.reader.ReadByte() != 0) ;

			var freeMemory = this.reader.ReadUInt16();

			running = true;

			while (running) {
				var now = DateTime.Now;
				UpdateEvent evt = UpdateEvent.None;
				if (this.port.BytesToRead > 0) {
					switch (evt = (UpdateEvent)this.reader.ReadByte()) {
						case UpdateEvent.Print: {
								this.writer.Write((byte)0);
								byte b;
								while ((b = this.reader.ReadByte()) != 0) {
									this.host.Print((char)b);
								}
							}
							break;
						case UpdateEvent.Trace: {
								this.writer.Write((byte)0);
								byte b;
								while ((b = this.reader.ReadByte()) != 13) {
									this.host.Print((char)b);
								}
								this.host.Print('\r');
							}
							break;
						case UpdateEvent.Error:
							this.writer.Write((byte)0);
							var procedure = this.ReadString();
							var message = this.ReadString();
							var line = this.ReadString();
							if (!this.host.DisplayError(procedure, message, line)) {
								if (!string.IsNullOrEmpty(procedure)) {
									foreach (char e in (procedure + " : ")) this.host.Print(e);
								}
								foreach (char e in (message)) this.host.Print(e);
								if (!string.IsNullOrEmpty(line)) {
									foreach (char e in (" (" + line + ")")) this.host.Print(e);
								}
								this.host.Print('\r');
							}
							break;
						case UpdateEvent.Cmd:
							this.writer.Write((byte)0);
							this.host.EnableCommandMode();
							break;
						case UpdateEvent.Build:
							this.writer.Write((byte)0);
							this.host.EditProcedure(true, this.ReadString());
							break;
						case UpdateEvent.Edit:
							this.writer.Write((byte)0);
							this.host.EditProcedure(false, this.ReadString());
							break;
						case UpdateEvent.Quit:
							this.writer.Write((byte)0);
							running = false;
							this.host.Quit();
							break;
						case UpdateEvent.Rtc:
							this.writer.Write((byte)0);
							this.writer.Write((byte)now.Hour);
							this.writer.Write((byte)now.Minute);
							this.writer.Write((byte)now.Second);
							this.writer.Write((byte)(now.Millisecond / 10));
							break;
						case UpdateEvent.Inkey:
							this.writer.Write((byte)0);
							this.writer.Write((byte)this.host.GetKey());
							break;
						case UpdateEvent.TraceFl:
							this.writer.Write((byte)0);
							this.host.SetTraceFlag(this.reader.ReadByte() != 0);
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
					nextSampleTime = now + samplePortsInterval;
				} else {
					this.host.CheckEscapeCondition();
				}
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
			this.writer.Write((byte)evt);
			while (this.reader.ReadByte() != 0) ;
		}

		public void SendCmd(string command) {
			this.SendRemoteEvent(RemoteEvent.Cmd);
			this.writer.Write(Encoding.ASCII.GetBytes(command));
			this.writer.Write((byte)13);
		}

		public enum LabelUse : byte {
			DoNotUseLabels = 0,
			UseLabels = 1,
			UseLblsSetting = 255,
		}

		public string SteadyLine(string line, LabelUse flag) {
			this.SendRemoteEvent(RemoteEvent.SteadyLine);
			this.writer.Write((byte)flag);
			this.writer.Write(Encoding.ASCII.GetBytes(line));
			this.writer.Write(13);
			return this.ReadString();
		}

		public void Escape() {
			this.SendRemoteEvent(RemoteEvent.Escape);
		}

		public void Sleep() {
			this.SendRemoteEvent(RemoteEvent.Sleep);
			this.running = false;
		}

		public AlbertLinkPortState GetPortState(bool updateSensorIDs) {
			this.SendRemoteEvent(updateSensorIDs ? RemoteEvent.GetPS : RemoteEvent.GetPorts);
			var portState = this.reader.ReadBytes(12);
			if (updateSensorIDs) {
				this.sensorIDs = this.reader.ReadBytes(4);
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
			this.writer.Write((byte)0xFF);
			this.WriteString(procedureName);
			string code = null;
			if (this.reader.ReadByte() != 0) {
				code = this.ReadString(0xFF);
			}
			return code;
        }

		public byte PutProcedure(string procedureName, string code) {
			this.SendRemoteEvent(RemoteEvent.Put);
			this.WriteString(procedureName);
			this.WriteString(code.Trim() + "\r", 0xFF);
			return this.reader.ReadByte();
		}
	}
}
