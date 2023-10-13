using System;
using System.Windows.Forms;

namespace SmartMove {
	public partial class ConnectionWindow : Form {
		public ConnectionWindow() {
			InitializeComponent();
		}

		private void UpdateProgressLabel() {
			this.ProgressLabel.Text = ((decimal)this.ProgressBar.Value / (decimal)this.ProgressBar.Maximum).ToString("P");
		}

		public int ProgressValue { get { return this.ProgressBar.Value; } set { this.ProgressBar.Value = value; this.UpdateProgressLabel(); } }
		public int ProgressMaximum { get { return this.ProgressBar.Maximum; } set { this.ProgressBar.Maximum = value; this.UpdateProgressLabel(); } }

		private void ConnectionWindow_Load(object sender, EventArgs e) {
			CenterToParent();
		}

		private void ConnectionWindow_FormClosing(object sender, FormClosingEventArgs e) {
			e.Cancel = true;
		}


	}
}
