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
		private Dictionary<string, EditorWindow> editorWindows = new Dictionary<string, EditorWindow>();

		private bool focusCommandLine = true;

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
			EditorWindow editor;

			var key = procedure.ToLowerInvariant();

			// Is there an existing window?
			if (!editorWindows.TryGetValue(key, out editor) || editor.IsDisposed) {

				// Was it a disposed window?
				if (editor != null) {
					this.editorWindows.Remove(key);
					editor = null;
				}

				// Create the new instance.
				editor = new EditorWindow();
				this.editorWindows.Add(key, editor);
				editor.FormClosing += Editor_FormClosing;

				// If we're editing an existing procedure, fetch and display the old code.
				if (!build) {
					var code = this.link.GetProcedure(procedure);
					if (code != null) {
						editor.Code = code;
					}
				}

				editor.Show();
			}

			editor.Focus();
			editor.Procedure = procedure;

			// Do not focus the command line immediately after showing an editor window
			this.focusCommandLine = false;
		}

		private void Editor_FormClosing(object sender, FormClosingEventArgs e) {
			var form = sender as EditorWindow;
			if (form != null) {
				Debug.WriteLine(this.link.PutProcedure(form.Procedure, form.Code));
			}
		}

		public void EnableCommandMode() {
			this.mainWindow?.EnableCommandMode(focusCommandLine);
			this.focusCommandLine = true;
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
