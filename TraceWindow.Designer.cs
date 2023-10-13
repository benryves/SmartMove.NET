namespace SmartBox {
	partial class TraceWindow {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.TraceOutput = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// TraceOutput
			// 
			this.TraceOutput.BackColor = System.Drawing.SystemColors.Control;
			this.TraceOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TraceOutput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TraceOutput.Location = new System.Drawing.Point(0, 0);
			this.TraceOutput.Multiline = true;
			this.TraceOutput.Name = "TraceOutput";
			this.TraceOutput.ReadOnly = true;
			this.TraceOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TraceOutput.Size = new System.Drawing.Size(512, 297);
			this.TraceOutput.TabIndex = 2;
			// 
			// TraceWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(512, 297);
			this.Controls.Add(this.TraceOutput);
			this.Name = "TraceWindow";
			this.Text = "Trace";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox TraceOutput;
	}
}