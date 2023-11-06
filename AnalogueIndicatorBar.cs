using System.Drawing;
using System.Windows.Forms;

namespace SmartMove {
	internal class AnalogueIndicatorBar : ProgressBar {

		public AnalogueIndicatorBar() {
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
		}

		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.Clear(this.BackColor);
			
			using (var pen = new Pen(Color.FromKnownColor(KnownColor.ControlDark))) {
				e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
			}

			int w = this.ClientSize.Width - 2;
			if (w > 0 && this.Maximum > 0) {
				w = (w * this.Value) / this.Maximum;
				using (var brush = new SolidBrush(Color.FromKnownColor(KnownColor.Highlight))) {
					e.Graphics.FillRectangle(brush, 1, 1, w, this.ClientSize.Height - 2);
				}
			}
		}

	}
}
