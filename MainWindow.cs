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

		public void EnableCommandMode() {
			this.CommandPanel.Enabled = true;
			this.CommandInput.Clear();
			this.CommandInput.Focus();
		}

		private void SendCommand() {
			if (this.CommandPanel.Enabled) {
				this.PrintOutput.AppendText(": " + this.CommandInput.Text + Environment.NewLine);
				this.PrintOutput.ScrollToCaret();
				this.CommandPanel.Enabled = false;
				this.Link?.SendCmd(this.CommandInput.Text);
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
	}
}
