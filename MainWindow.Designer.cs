namespace SmartBox {
	partial class MainWindow {
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
			this.PrintOutput = new System.Windows.Forms.TextBox();
			this.CommandPanel = new System.Windows.Forms.Panel();
			this.CommandInput = new System.Windows.Forms.TextBox();
			this.SendButton = new System.Windows.Forms.Button();
			this.CommandPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrintOutput
			// 
			this.PrintOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrintOutput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PrintOutput.Location = new System.Drawing.Point(0, 0);
			this.PrintOutput.Multiline = true;
			this.PrintOutput.Name = "PrintOutput";
			this.PrintOutput.ReadOnly = true;
			this.PrintOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.PrintOutput.Size = new System.Drawing.Size(771, 427);
			this.PrintOutput.TabIndex = 1;
			this.PrintOutput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommandInput_KeyPress);
			// 
			// CommandPanel
			// 
			this.CommandPanel.Controls.Add(this.CommandInput);
			this.CommandPanel.Controls.Add(this.SendButton);
			this.CommandPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CommandPanel.Enabled = false;
			this.CommandPanel.Location = new System.Drawing.Point(0, 427);
			this.CommandPanel.Name = "CommandPanel";
			this.CommandPanel.Size = new System.Drawing.Size(771, 20);
			this.CommandPanel.TabIndex = 2;
			// 
			// CommandInput
			// 
			this.CommandInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommandInput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CommandInput.Location = new System.Drawing.Point(0, 0);
			this.CommandInput.Name = "CommandInput";
			this.CommandInput.Size = new System.Drawing.Size(696, 20);
			this.CommandInput.TabIndex = 1;
			this.CommandInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommandInput_KeyPress);
			// 
			// SendButton
			// 
			this.SendButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.SendButton.Location = new System.Drawing.Point(696, 0);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = new System.Drawing.Size(75, 20);
			this.SendButton.TabIndex = 0;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(771, 447);
			this.Controls.Add(this.PrintOutput);
			this.Controls.Add(this.CommandPanel);
			this.Name = "MainWindow";
			this.Activated += new System.EventHandler(this.MainWindow_Activated);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
			this.CommandPanel.ResumeLayout(false);
			this.CommandPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox PrintOutput;
		private System.Windows.Forms.Panel CommandPanel;
		private System.Windows.Forms.TextBox CommandInput;
		private System.Windows.Forms.Button SendButton;
	}
}