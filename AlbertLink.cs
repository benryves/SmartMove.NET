using System;
using System.Collections.Generic;
using System.IO.Ports;
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

	public class AlbertLink : IDisposable {

		private readonly SmartBox smartBox;
		private readonly IAlbertLinkHost host;

		private bool running = false;

		private TimeSpan samplePortsInterval = TimeSpan.FromMilliseconds(50);
		private TimeSpan updateSensorsInterval = TimeSpan.FromMilliseconds(1000);
		private DateTime nextSampleTime = DateTime.Now;
		private DateTime lastUpdateSensorsTime = DateTime.MinValue;

		byte[] sensorIDs = new byte[4];
		
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
					throw new OutOfMemoryException();
				}

				// Replace this.DownloadData(lomem, program); with loop to allow for progress reports
				int chunkSize = 128;
				for (int offset = 0; offset < program.Length; offset += chunkSize) {
					this.host.UpdateConnectionProgress(offset, program.Length);
					this.host.Idle();
					int length = Math.Min(chunkSize, program.Length - offset);
					this.smartBox.writer.Write((byte)SmartBox.Command.DownloadData);
					this.smartBox.writer.Write((ushort)(lomem + offset));
					this.smartBox.writer.Write((ushort)length);
					this.smartBox.port.Write(program, offset, length);
				}
				this.host.UpdateConnectionProgress(program.Length, program.Length);
				this.host.Idle();

				ushort entrypoint = (ushort)(lomem + program[0] + program[1] * 256);

				this.smartBox.ExecuteCode(entrypoint, 0, (byte)entrypoint, (byte)(entrypoint / 256));
				this.smartBox.reader.ReadByte();

				albertLinkCall = this.smartBox.GetNameCode("AlbertLink");

			}

			if (albertLinkCall == 0) throw new InvalidOperationException();

			// Invoke AlbertLink
			this.smartBox.writer.Write(albertLinkCall);

			this.smartBox.reader.ReadByte();
			this.smartBox.reader.ReadByte();

			// Setup with appropriate flags
			this.smartBox.writer.Write((byte)1);
			this.smartBox.writer.Write((byte)(SetupFlags.EnableProcedureLabelChangeChecking));

			// Show the sign-on message
			this.host.ShowSignOn();

			// Trigger reading labels
			this.ReadLabels();

			running = true;

			while (running) {
				var now = DateTime.Now;
				UpdateEvent evt;
				if (this.smartBox.port.BytesToRead > 0) {
					switch (evt = (UpdateEvent)this.smartBox.reader.ReadByte()) {
						case UpdateEvent.Print: {
								this.smartBox.writer.Write((byte)0);
								byte b;
								while ((b = this.smartBox.reader.ReadByte()) != 0) {
									this.host.Print((char)b);
								}
							}
							break;
						case UpdateEvent.Trace: {
								this.smartBox.writer.Write((byte)0);
								byte b;
								while ((b = this.smartBox.reader.ReadByte()) != 13) {
									this.host.Trace((char)b);
								}
								this.host.Trace('\r');
							}
							break;
						case UpdateEvent.Error:
							this.smartBox.writer.Write((byte)0);
							var procedure = this.smartBox.ReadString();
							var message = this.smartBox.ReadString();
							var line = this.smartBox.ReadString();
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
							this.smartBox.writer.Write((byte)0);
							this.host.EnableCommandMode();
							break;
						case UpdateEvent.Build:
							this.smartBox.writer.Write((byte)0);
							this.host.EditProcedure(true, this.smartBox.ReadString());
							break;
						case UpdateEvent.Edit:
							this.smartBox.writer.Write((byte)0);
							this.host.EditProcedure(false, this.smartBox.ReadString());
							break;
						case UpdateEvent.Quit:
							this.smartBox.writer.Write((byte)0);
							running = false;
							this.host.Quit();
							break;
						case UpdateEvent.Rtc:
							this.smartBox.writer.Write((byte)0);
							this.smartBox.writer.Write((byte)now.Hour);
							this.smartBox.writer.Write((byte)now.Minute);
							this.smartBox.writer.Write((byte)now.Second);
							this.smartBox.writer.Write((byte)(now.Millisecond / 10));
							break;
						case UpdateEvent.Inkey:
							this.smartBox.writer.Write((byte)0);
							this.smartBox.writer.Write((byte)this.host.GetKey());
							break;
						case UpdateEvent.TraceFl:
							this.smartBox.writer.Write((byte)0);
							this.host.SetTraceFlag(this.smartBox.reader.ReadByte() != 0);
							break;
						case UpdateEvent.Altered:
							this.smartBox.writer.Write((byte)0);
							var altered = this.smartBox.reader.ReadByte();
							//if ((altered & (1 << 0)) != 0) ; // Procedure list changed
							if ((altered & (1 << 1)) != 0) this.ReadLabels();
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
			this.smartBox.writer.Write((byte)evt);
			while (this.smartBox.reader.ReadByte() != 0) ;
		}

		public void SendCmd(string command) {
			this.SendRemoteEvent(RemoteEvent.Cmd);
			this.smartBox.writer.Write(Encoding.ASCII.GetBytes(command));
			this.smartBox.writer.Write((byte)13);
		}

		public enum LabelUse : byte {
			DoNotUseLabels = 0,
			UseLabels = 1,
			UseLblsSetting = 255,
		}

		public string SteadyLine(string line, LabelUse flag) {
			this.SendRemoteEvent(RemoteEvent.SteadyLine);
			this.smartBox.writer.Write((byte)flag);
			this.smartBox.writer.Write(Encoding.ASCII.GetBytes(line));
			this.smartBox.writer.Write(13);
			return this.smartBox.ReadString();
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
			var portState = this.smartBox.reader.ReadBytes(12);
			if (updateSensorIDs) {
				this.sensorIDs = this.smartBox.reader.ReadBytes(4);
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
			this.smartBox.writer.Write((byte)0xFF);
			this.smartBox.WriteString(procedureName);
			string code = null;
			byte status = this.smartBox.reader.ReadByte();
			if (status != 0) {
				code = (char)status + this.smartBox.ReadString(0xFF);
			}
			return code;
        }

		public byte PutProcedure(string procedureName, string code) {
			this.SendRemoteEvent(RemoteEvent.Put);
			this.smartBox.WriteString(procedureName);
			this.smartBox.WriteString(code.Trim() + "\r", 0xFF);
			return this.smartBox.reader.ReadByte();
		}

		public void SetTraceFlag(bool flag) {
			this.SendRemoteEvent(RemoteEvent.TraceFl);
			this.smartBox.writer.Write((byte)(flag ? 1 : 0));
		}

		public void ReadLabels() {
			this.host.ResetLabels();
			this.SendRemoteEvent(RemoteEvent.ReadLabels);
			string source;
			while (!string.IsNullOrEmpty(source = this.smartBox.ReadString())) {

				var s = new List<byte>(8);
				byte b;
				while (((b = this.smartBox.reader.ReadByte()) & 0x7F) != 0) {
					s.Add(b);
				}
				var destination = Encoding.ASCII.GetString(s.ToArray());

				this.host.UpdateLabel(source, destination, b == 128);
			}
		}

		public string GetVersion() {
			this.SendRemoteEvent(RemoteEvent.Version);
			return this.smartBox.ReadString();
		}

		public ushort GetFreeMem() {
			this.SendRemoteEvent(RemoteEvent.FreeMem);
			return this.smartBox.reader.ReadUInt16();
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
