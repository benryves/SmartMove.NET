using System;
using System.Windows.Forms;

namespace SmartMove {
	public partial class TextInputWindow : Form {

		public TextBox TextInputField {
			get { return this.TextField; }
		}

		public string Description {
			get { return this.DescriptionField.Text; }
			set { this.DescriptionField.Text = value; }
		}

		public TextInputWindow() {
			InitializeComponent();
		}

		private void DialogButton_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}
