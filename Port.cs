using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace SmartMove {

	/// <summary>
	/// Provides low-level access to the Smart Box via a serial port.
	/// </summary>
	public class Port : IDisposable {

		private readonly SerialPort port;
		private readonly BinaryReader reader;
		private readonly BinaryWriter writer;
		
		private bool disposedValue;

		/// <summary>
		/// Creates an instance of the Smart Box port from a SerialPort instance.
		/// </summary>
		/// <param name="port">The serial port to use to access the Smart Box.</param>
		public Port(SerialPort port) {
			this.port = port;
			if (!this.port.IsOpen) this.port.Open();
			this.reader = new BinaryReader(this.port.BaseStream);
			this.writer = new BinaryWriter(this.port.BaseStream);
		}

		/// <summary>
		/// Creates an instance of the Smart Box port from a serial port name.
		/// </summary>
		/// <param name="portName">The name of the serial port to use to access the Smart Box.</param>
		public Port(string portName) : this(new SerialPort(portName, 9600, Parity.None, 8, StopBits.One) {
			Handshake = Handshake.RequestToSend
		}) { }

		/// <summary>
		/// Determines how many bytes are available to read in the port's input buffer.
		/// </summary>
		public int BytesToRead {
			get {
				return this.port.BytesToRead;
			}
		}

		/// <summary>
		/// Writes a single byte to the port.
		/// </summary>
		/// <param name="value">The byte value to write.</param>
		public void Write(byte value) {
			this.writer.Write(value);
		}

		/// <summary>
		/// Writes an array of bytes to the port.
		/// </summary>
		/// <param name="buffer">The bytes of data to write.</param>
		public void Write(byte[] buffer) {
			this.writer.Write(buffer);
		}

		/// <summary>
		/// Writes an subset of an array of bytes to the port.
		/// </summary>
		/// <param name="buffer">The bytes of data to write.</param>
		/// <param name="offset">The starting index in the buffer to write.</param>
		/// <param name="count">The number of bytes from the buffer to write.</param>
		public void Write(byte[] buffer, int offset, int count) {
			this.port.Write(buffer, offset, count);
		}

		/// <summary>
		/// Writes a 16-bit value to the port.
		/// </summary>
		/// <param name="value">The 16-bit value to write.</param>
		public void Write(ushort value) {
			this.writer.Write(value);
		}

		/// <summary>
		/// Writes a string to the port.
		/// </summary>
		/// <param name="value">The value to write.</param>
		/// <param name="terminator">The optional terminator value (defaults to CR).</param>
		public void Write(string value, byte terminator = 13) {
			this.writer.Write(Encoding.ASCII.GetBytes(value));
			this.writer.Write(terminator);
		}

		/// <summary>
		/// Reads a single byte from the port.
		/// </summary>
		/// <returns>The byte value read from the port.</returns>
		public byte ReadByte() {
			return this.reader.ReadByte();
		}

		/// <summary>
		/// Reads an arrayof byte values from the port.
		/// </summary>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The read data from the port.</returns>
		public byte[] ReadBytes(int count) {
			return this.reader.ReadBytes(count);
		}

		/// <summary>
		/// Reads a 16-bit value from the port.
		/// </summary>
		/// <returns>The 16-byte value read from the port.</returns>
		public ushort ReadUInt16() {
			return this.reader.ReadUInt16();
		}

		/// <summary>
		/// Reads a string from the port.
		/// </summary>
		/// <param name="terminator">The optional terminator value (defaults to CR).</param>
		/// <returns>The string value read from the port.</returns>
		public string ReadString(byte terminator = 13) {
			var s = new List<byte>(8);
			byte b;
			while ((b = this.reader.ReadByte()) != terminator) {
				s.Add(b);
			}
			return Encoding.ASCII.GetString(s.ToArray());
		}

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					this.port?.Dispose();
					this.reader?.Dispose();
					this.writer?.Dispose();
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
