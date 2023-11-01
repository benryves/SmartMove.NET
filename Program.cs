using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace SmartMove {

	internal class Program {

		[STAThread]
		static void Main() {

			Application.EnableVisualStyles();

			// We can only run if we have at least one serial port.
			var running = SerialPort.GetPortNames().Length > 0;

			while (running) {

				// Fetch the saved serial port name.
				var serialPortName = Properties.Settings.Default.SerialPort;

				// Can we open it?
				var canOpenSerialPort = false;
				if (!string.IsNullOrEmpty(serialPortName) && Array.Exists(SerialPort.GetPortNames(), pn => pn == serialPortName)) {
					try {
						using (var serialPort = new SerialPort(serialPortName)) {
							serialPort.Open();
							canOpenSerialPort = true;
						}
					} catch (Exception ex) {
						if (MessageBox.Show("Could not open " + serialPortName + ": " + ex.Message, Application.ProductName, MessageBoxButtons.OKCancel) != DialogResult.OK) {
							running = false;
						}
					}
				}

				// Bail out if we're no longer running.
				if (!running) break;

				// Try to get the version number from the Smart Box.
				if (canOpenSerialPort) {
					using (var smartBox = new SmartBox(serialPortName)) {
						if (smartBox.GetVersion() == 0) {
							canOpenSerialPort = false;
							running = false;
						}
					}
				}

				// Bail out if we're no longer running.
				if (!running) break;

				if (canOpenSerialPort) {
					// If we can open it, try to run the application.
					using (var albertLink = new AlbertLink(new SmartBox(serialPortName), new AlbertLinkWindowsHost())) {
						albertLink.Run();
						running = false;
						Properties.Settings.Default.Save();
					}
				} else {
					// We can't open the port, so display the port picker
					var serialPortWindow = new SerialPortWindow();
					running = serialPortWindow.ShowDialog() == DialogResult.OK;
				}
			}
		}

	}
}
