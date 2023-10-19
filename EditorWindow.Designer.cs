namespace SmartMove {
	partial class EditorWindow {
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
			this.EditorInput = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// EditorInput
			// 
			this.EditorInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EditorInput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditorInput.Location = new System.Drawing.Point(0, 0);
			this.EditorInput.Multiline = true;
			this.EditorInput.Name = "EditorInput";
			this.EditorInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.EditorInput.Size = new System.Drawing.Size(473, 314);
			this.EditorInput.TabIndex = 0;
			this.EditorInput.WordWrap = false;
			// 
			// EditorWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(473, 314);
			this.Controls.Add(this.EditorInput);
			this.Name = "EditorWindow";
			this.ShowIcon = false;
			this.Text = "Editor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox EditorInput;
	}
}