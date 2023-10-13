using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace SmartBox {

	public class SmartBox : IDisposable {

		public enum Command : byte {
			Blank = 0,
			Version = 1,
			Reset = 2,
			NameCode = 3,
			CodeName = 4,
			MultipleSetup = 5,
			MultipleRead = 6,
			MultipleServer = 7,
			Credits = 9,
			WriteMotors = 10,
			ReadMotors = 11,
			MotorForward = 12,
			MotorReverse = 13,
			MotorHalt = 14,
			MotorPower = 15,
			WriteOutputs = 20,
			OutputPower = 21,
			GetSensors = 22,
			CheckSensors = 23,
			SetBitHigh = 28,
			SetBitLow = 29,
			ReadADCReg = 30,
			WriteADCReg = 31,
			ReadACIAReg = 32,
			WriteACIAReg = 33,
			ReadVIAReg = 34,
			WriteVIAReg = 35,
			SetVIAHigh = 36,
			SetVIALow = 37,
			ReadADC = 40,
			ReadADCs = 41,
			ForcedADCRead = 42,
			HighResADC = 44,
			LowResADC = 45,
			ReadResolution = 47,
			DownloadData = 50,
			UploadData = 52,
			ExecuteCode = 54,
			StoreByte = 55,
			ReadByte = 56,
			ReadRAMSize = 57,
			ExtendCall = 59,
			SetClock = 60,
			ReadClock = 61,
			ReadTopmem = 62,
			WriteTopmem = 63,
			ReadLomem = 64,
			WriteLomem = 65,
			ReadHimem = 66,
			WriteHimem = 67,
			ReadInputs = 90,
			ReadBit = 91,
		};

		public enum MotorDirection : byte {
			Stopped = 0,
			Forward = 1,
			Backward = 2,
		}

		private bool disposedValue;

		protected SerialPort port;
		protected BinaryReader reader;
		protected BinaryWriter writer;

		public SmartBox(SerialPort port) {
			this.port = port;
			if (!this.port.IsOpen) this.port.Open();
			this.reader = new BinaryReader(this.port.BaseStream);
			this.writer = new BinaryWriter(this.port.BaseStream);
		}

		public SmartBox(string portName) : this(new SerialPort(portName, 9600, Parity.None, 8, StopBits.One) {
			Handshake = Handshake.RequestToSend
		}) { }

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (this.port != null) {
						this.port.Close();
						this.port.Dispose();
						this.port = null;
					}
				}
				disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected string ReadString(byte terminator = 13) {
			var s = new List<byte>(8);
			byte b;
			while ((b = this.reader.ReadByte()) != terminator) {
				s.Add(b);
			}
			return Encoding.ASCII.GetString(s.ToArray());
		}

		protected void WriteString(string s, byte terminator = 13) {
			this.writer.Write(Encoding.ASCII.GetBytes(s));
			this.writer.Write((byte)terminator);
		}

		/// <summary>
		/// Read the operating system version number
		/// </summary>
		/// <returns>The OS version number as a decimal.</returns>
		public decimal GetVersion() {
			this.writer.Write((byte)Command.Version);
			return this.reader.ReadUInt16() / 1000m;
		}

		/// <summary>
		/// Reset SmartBox
		/// </summary>
		/// <param name="hard">Set to <c>true</c> to clear battery-backed RAM.</param>
		/// <remarks>Before sending anymore codes create a small delay while SmartBox resets various parts of hardware</remarks>
		public void Reset(bool hard) {
			this.writer.Write((byte)Command.Reset);
			this.writer.Write((byte)(hard ? 255 : 254));
		}

		/// <summary>
		/// To obtain the operating system call number where the name is known
		/// </summary>
		/// <param name="osCallName">OS call name</param>
		/// <returns>Operating system call number</returns>
		public byte GetNameCode(string osCallName) {
			this.writer.Write((byte)Command.NameCode);
			this.writer.Write((byte[])Encoding.ASCII.GetBytes(osCallName));
			this.writer.Write((byte)13);
			return this.reader.ReadByte();
		}

		/// <summary>
		/// Obtain the name associated with an operating system call
		/// </summary>
		/// <param name="osCallNumber">OS call number</param>
		/// <returns>OS call name</returns>
		public string GetCodeName(byte osCallNumber) {
			this.writer.Write((byte)Command.CodeName);
			this.writer.Write((byte)osCallNumber);
			return this.ReadString();
		}


		/// <summary>
		/// Set the values that will be returned by MultipleRead
		/// </summary>
		/// <param name="analogueChannel1">Return status of analogue channel 1</param>
		/// <param name="analogueChannel2">Return status of analogue channel 2</param>
		/// <param name="analogueChannel3">Return status of analogue channel 3</param>
		/// <param name="analogueChannel4">Return status of analogue channel 4</param>
		/// <param name="digitalInputs">Return status of digital inputs</param>
		/// <param name="digitalOutputs">Return status of digital outputs</param>
		/// <param name="motorOutputs">Return status of motor outputs</param>
		public void MultipleSetup(bool analogueChannel1, bool analogueChannel2, bool analogueChannel3, bool analogueChannel4, bool digitalInputs, bool digitalOutputs, bool motorOutputs) {
			this.writer.Write((byte)Command.MultipleSetup);
			this.writer.Write((byte)(analogueChannel1 ? 1 : 0));
			this.writer.Write((byte)(analogueChannel2 ? 1 : 0));
			this.writer.Write((byte)(analogueChannel3 ? 1 : 0));
			this.writer.Write((byte)(analogueChannel4 ? 1 : 0));
			this.writer.Write((byte)(digitalInputs ? 1 : 0));
			this.writer.Write((byte)(digitalOutputs ? 1 : 0));
			this.writer.Write((byte)(motorOutputs ? 1 : 0));
		}

		/// <summary>
		/// Returns multiple readings as defined using MultipleSetup.
		/// </summary>
		/// <param name="length">The number of readings being returned</param>
		/// <returns>Bytes as defined by MultipleSetup</returns>
		public byte[] MultipleRead(int length) {
			this.writer.Write((byte)Command.MultipleRead);
			return this.reader.ReadBytes(length);
		}

		/// <summary>
		/// Returns the copyright string
		/// </summary>
		/// <returns>The copyright message and OS details</returns>
		public string GetCredits() {
			this.writer.Write((byte)Command.Credits);
			return this.ReadString(0);
		}

		/// <summary>
		/// Writes a byte to the motor drivers
		/// </summary>
		/// <param name="value">Value to write</param>
		/// <remarks>
		/// Each of the four motor outputs has two bits encoded into this value, bits 0 and 1 are for motor a, bits 2 and 3 are for motor b, bits 4 and 5 are for motor c and bits 6 and 7 are for motor d, setting both bits to 0 will stop the motor, setting the low bit high and high bit low will make the motor go forward and setting the high bit high and the low bit low will make it go backwards.
		/// Using this call automatically stops all pulsing of motors
		/// </remarks>
		public void WriteMotors(byte value) {
			this.writer.Write((byte)Command.WriteMotors);
			this.writer.Write((byte)value);
		}

		/// <summary>
		/// Writes the direction of each motor to the motor drivers
		/// </summary>
		/// <param name="a">The direction of motor A</param>
		/// <param name="b">The direction of motor B</param>
		/// <param name="c">The direction of motor C</param>
		/// <param name="d">The direction of motor D</param>
		public void WriteMotors(MotorDirection a, MotorDirection b, MotorDirection c, MotorDirection d) {
			this.WriteMotors((byte)(((byte)a & 3) << 0 | ((byte)b & 3) << 2 | ((byte)c & 3) << 4 | ((byte)d & 3) << 6));
		}

		/// <summary>
		/// Read the state of the motor drivers
		/// </summary>
		/// <returns>The value returned is the same value as what would be sent to WriteMotors</returns>
		public byte ReadMotors() {
			this.writer.Write((byte)Command.ReadMotors);
			return this.reader.ReadByte();
		}

		/// <summary>
		/// Reads the direction of each motor from the motor drivers
		/// </summary>
		/// <param name="a">The direction of motor A</param>
		/// <param name="b">The direction of motor B</param>
		/// <param name="c">The direction of motor C</param>
		/// <param name="d">The direction of motor D</param>
		public void ReadMotors(out MotorDirection a, out MotorDirection b, out MotorDirection c, out MotorDirection d) {
			var value = this.ReadMotors();
			a = (MotorDirection)((value >> 0) & 3);
			b = (MotorDirection)((value >> 2) & 3);
			c = (MotorDirection)((value >> 4) & 3);
			d = (MotorDirection)((value >> 6) & 3);
		}

		public ushort ReadLomem() {
			this.writer.Write((byte)Command.ReadLomem);
			return this.reader.ReadUInt16();
		}

		public ushort ReadHimem() {
			this.writer.Write((byte)Command.ReadHimem);
			return this.reader.ReadUInt16();
		}

		public void DownloadData(ushort address, byte[] data) {
			this.writer.Write((byte)Command.DownloadData);
			this.writer.Write((ushort)address);
			this.writer.Write((ushort)data.Length);
			this.writer.Write((byte[])data);
		}

		public void ExecuteCode(ushort address, byte a, byte x, byte y) {
			this.writer.Write((byte)Command.ExecuteCode);
			this.writer.Write((ushort)address);
			this.writer.Write((byte)a);
			this.writer.Write((byte)x);
			this.writer.Write((byte)y);
		}

		/// <summary>
		/// Reads a byte from the digital inputs port
		/// </summary>
		/// <returns>Byte read from the digital inputs port</returns>
		public byte ReadInputs() {
			this.writer.Write((byte)Command.ReadInputs);
			return this.reader.ReadByte();
		}

	}
}
