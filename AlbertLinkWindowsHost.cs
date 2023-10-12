using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartBox {
	internal class AlbertLinkWindowsHost : IAlbertLinkHost {

		private AlbertLink link;
		private MainWindow mainWindow;

		public AlbertLinkWindowsHost() {
			Application.EnableVisualStyles();
			this.mainWindow = new MainWindow();
			this.mainWindow.Show();
		}

		public void Initialize(AlbertLink link) {
			this.link = link;
			this.mainWindow.Link = link;
		}

		public void CheckEscapeCondition() {
			Application.DoEvents();
		}

		public bool DisplayError(string procedure, string message, string line) {
			return false;
		}

		public void EditProcedure(bool build, string procedure) {
			throw new NotImplementedException();
		}

		public void EnableCommandMode() {
			this.mainWindow?.EnableCommandMode();
		}

		public byte[] GetAlbertLinkProgram() {
			throw new NotImplementedException();
		}

		public char GetKey() {
			return (char)0;
		}

		public void Print(char value) {
			this.mainWindow?.Print(value);
		}

		public void Quit() {
			var window = this.mainWindow;
			if (window != null) {
				this.mainWindow = null;
				window.Close();
			}
		}

		public void SetTraceFlag(bool traceFlag) {
			throw new NotImplementedException();
		}
	}
}
