using System;
using System.Text;
using System.Windows.Forms;

namespace SmartMove {
	public partial class TraceWindow : Form {
		public TraceWindow() {
			InitializeComponent();
		}

		public void Trace(string value) {
			if (!string.IsNullOrEmpty(value)) {
				StringBuilder parts = new StringBuilder(value.Length);

				foreach (char c in value) {
					switch (c) {
						case (char)1: // Token begin
						case (char)2: // Token end
							break;
						case (char)12: // Clear screen
							if (parts.Length > 0) {
								this.TraceOutput.AppendText(parts.ToString());
								parts.Clear();
							}
							this.TraceOutput.Clear();
							break;
						case '\r': // New line
							parts.Append(Environment.NewLine);
							break;
						default:
							parts.Append(c);
							break;
					}
				}

				if (parts.Length > 0) {
					this.TraceOutput.AppendText(parts.ToString());
					parts.Clear();
				}
				this.TraceOutput.AppendText(Environment.NewLine);
			}

			this.TraceOutput.ScrollToCaret();
		}

	}
}
