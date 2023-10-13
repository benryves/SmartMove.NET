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
	public partial class TraceWindow : Form {
		public TraceWindow() {
			InitializeComponent();
		}

		public void Print(char c) {
			switch (c) {
				case (char)1: // Token begin
				case (char)2: // Token end
					break;
				case (char)12: // Clear screen
					this.TraceOutput.Clear();
					break;
				case '\r': // New line
					this.TraceOutput.AppendText(Environment.NewLine);
					break;
				default:
					this.TraceOutput.AppendText(c.ToString());
					break;
			}
			this.TraceOutput.ScrollToCaret();
		}

	}
}
