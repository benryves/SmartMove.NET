using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace SmartMove {
	public partial class SerialPortWindow : Form {

		public string Description {
			get { return this.DescriptionField.Text; }
			set { this.DescriptionField.Text = value; }
		}

		public SerialPortWindow() {
			InitializeComponent();

			this.SerialPortList.Items.Clear();
			foreach (var port in SerialPort.GetPortNames()) {
				this.SerialPortList.Items.Add(port);
				if (port == Properties.Settings.Default.SerialPort) {
					this.SerialPortList.SelectedIndex = this.SerialPortList.Items.Count - 1;
				}
			}

		}

		private void DialogButton_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void SerialPortWindow_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.UserClosing && this.DialogResult == DialogResult.OK) {
				Properties.Settings.Default.SerialPort = (string)this.SerialPortList.SelectedItem;
            }
		}
	}
}
