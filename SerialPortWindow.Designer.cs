namespace SmartMove {
	partial class SerialPortWindow {
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
			this.DescriptionField = new System.Windows.Forms.Label();
			this.FieldTable = new System.Windows.Forms.TableLayoutPanel();
			this.DialogButtonTable = new System.Windows.Forms.TableLayoutPanel();
			this.OKDialogButton = new System.Windows.Forms.Button();
			this.CancelDialogButton = new System.Windows.Forms.Button();
			this.SerialPortList = new System.Windows.Forms.ComboBox();
			this.FieldTable.SuspendLayout();
			this.DialogButtonTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// DescriptionField
			// 
			this.DescriptionField.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionField.Location = new System.Drawing.Point(3, 0);
			this.DescriptionField.Name = "DescriptionField";
			this.DescriptionField.Size = new System.Drawing.Size(333, 32);
			this.DescriptionField.TabIndex = 0;
			this.DescriptionField.Text = "Please select the Smart Box serial port:";
			// 
			// FieldTable
			// 
			this.FieldTable.ColumnCount = 1;
			this.FieldTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.FieldTable.Controls.Add(this.DialogButtonTable, 0, 2);
			this.FieldTable.Controls.Add(this.DescriptionField, 0, 0);
			this.FieldTable.Controls.Add(this.SerialPortList, 0, 1);
			this.FieldTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FieldTable.Location = new System.Drawing.Point(8, 8);
			this.FieldTable.Name = "FieldTable";
			this.FieldTable.RowCount = 3;
			this.FieldTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.FieldTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.FieldTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.FieldTable.Size = new System.Drawing.Size(339, 97);
			this.FieldTable.TabIndex = 1;
			// 
			// DialogButtonTable
			// 
			this.DialogButtonTable.ColumnCount = 2;
			this.DialogButtonTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DialogButtonTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DialogButtonTable.Controls.Add(this.OKDialogButton, 0, 0);
			this.DialogButtonTable.Controls.Add(this.CancelDialogButton, 1, 0);
			this.DialogButtonTable.Dock = System.Windows.Forms.DockStyle.Right;
			this.DialogButtonTable.Location = new System.Drawing.Point(156, 64);
			this.DialogButtonTable.Margin = new System.Windows.Forms.Padding(0);
			this.DialogButtonTable.Name = "DialogButtonTable";
			this.DialogButtonTable.RowCount = 1;
			this.DialogButtonTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DialogButtonTable.Size = new System.Drawing.Size(183, 33);
			this.DialogButtonTable.TabIndex = 2;
			// 
			// OKDialogButton
			// 
			this.OKDialogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKDialogButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKDialogButton.Location = new System.Drawing.Point(13, 7);
			this.OKDialogButton.Name = "OKDialogButton";
			this.OKDialogButton.Size = new System.Drawing.Size(75, 23);
			this.OKDialogButton.TabIndex = 0;
			this.OKDialogButton.Text = "&OK";
			this.OKDialogButton.UseVisualStyleBackColor = true;
			this.OKDialogButton.Click += new System.EventHandler(this.DialogButton_Click);
			// 
			// CancelDialogButton
			// 
			this.CancelDialogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelDialogButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelDialogButton.Location = new System.Drawing.Point(105, 7);
			this.CancelDialogButton.Name = "CancelDialogButton";
			this.CancelDialogButton.Size = new System.Drawing.Size(75, 23);
			this.CancelDialogButton.TabIndex = 1;
			this.CancelDialogButton.Text = "&Cancel";
			this.CancelDialogButton.UseVisualStyleBackColor = true;
			this.CancelDialogButton.Click += new System.EventHandler(this.DialogButton_Click);
			// 
			// SerialPortList
			// 
			this.SerialPortList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SerialPortList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SerialPortList.FormattingEnabled = true;
			this.SerialPortList.Location = new System.Drawing.Point(3, 35);
			this.SerialPortList.Name = "SerialPortList";
			this.SerialPortList.Size = new System.Drawing.Size(333, 21);
			this.SerialPortList.TabIndex = 3;
			// 
			// SerialPortWindow
			// 
			this.AcceptButton = this.OKDialogButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelDialogButton;
			this.ClientSize = new System.Drawing.Size(355, 113);
			this.Controls.Add(this.FieldTable);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SerialPortWindow";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Smart Box Serial Port";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SerialPortWindow_FormClosing);
			this.FieldTable.ResumeLayout(false);
			this.DialogButtonTable.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label DescriptionField;
		private System.Windows.Forms.TableLayoutPanel FieldTable;
		private System.Windows.Forms.TableLayoutPanel DialogButtonTable;
		private System.Windows.Forms.Button OKDialogButton;
		private System.Windows.Forms.Button CancelDialogButton;
		private System.Windows.Forms.ComboBox SerialPortList;
	}
}