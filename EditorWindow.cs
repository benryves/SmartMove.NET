﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartBox {
	public partial class EditorWindow : Form {
		public EditorWindow() {
			InitializeComponent();
		}

		private string procedure;
		public string Procedure {
			get { return this.procedure; }
			set {
				this.procedure = value;
				this.Text = "Procedure: " + value;
			}
		}

		public string Code {
			get { return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(this.EditorInput.Text.Replace(Environment.NewLine, "\r"))); }
			set { this.EditorInput.Text = value.Replace("\r", Environment.NewLine).Replace("\x01", "").Replace("\x02", ""); this.EditorInput.Select(0, 0); }
		}
	}
}
