using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows.Forms;

namespace SmartMove {
	internal class AlbertLinkWindowsHost : IAlbertLinkHost, IDisposable {

		private AlbertLink link;

		private MainWindow mainWindow;
		private readonly Dictionary<string, EditorWindow> editorWindows = new Dictionary<string, EditorWindow>();
		private TraceWindow traceWindow;
		private ConnectionWindow connectionWindow;

		private bool focusCommandLine = true;

		private bool controlError = false;
		private bool customFnError = false;

		private bool disposedValue;

		public AlbertLinkWindowsHost() {
			this.mainWindow = new MainWindow();
			this.mainWindow.Show();
		}

		public void Initialize(AlbertLink link) {
			this.link = link;
			this.mainWindow.Link = link;
		}

		public void ShowSignOn() {
			var versionString = string.Format("{0} {1} ({2})\r{3} bytes free\r\r", Application.ProductName, Application.ProductVersion, link.GetVersion(), link.GetFreeMem());
			this.mainWindow.Print(versionString);
		}

		public void Idle() {
			if (controlError) {
				controlError = false;
				this.link.Error("CONTROL is not supported in this version of SmartMove");
			} else if (customFnError) {
				customFnError = false;
				this.link.Error("Bad expression");
			} else {
				Application.DoEvents();
			}
		}

		public bool DisplayError(string procedure, string message, string line) {
			return false;
		}

		public void EditProcedure(bool build, string procedure) {

			var key = procedure.ToLowerInvariant();

			// Is there an existing window?
			if (!editorWindows.TryGetValue(key, out EditorWindow editor) || editor.IsDisposed) {

				// Was it a disposed window?
				if (editor != null) {
					this.editorWindows.Remove(key);
					editor = null;
				}

				// Create the new instance.
				editor = new EditorWindow();
				this.editorWindows.Add(key, editor);
				editor.FormClosing += Editor_FormClosing;

				// Fetch the code.
				var code = this.link.GetProcedure(procedure);
				if (code != null) {
					editor.Code = code;
				}

				editor.Show();
			}

			editor.Focus();
			editor.Procedure = procedure;

			// Do not focus the command line immediately after showing an editor window
			this.focusCommandLine = false;
		}

		private void Editor_FormClosing(object sender, FormClosingEventArgs e) {
			if (sender is EditorWindow form) {
				//TODO: Handle close errors (out of memory etc)
				Debug.WriteLine(this.link.PutProcedure(form.Procedure, form.Code));
			}
		}

		public void EnableCommandMode() {
			this.mainWindow?.EnableCommandMode(focusCommandLine);
			this.focusCommandLine = true;
		}

		public byte[] GetAlbertLinkProgram() {
			return System.IO.File.ReadAllBytes("AL.COD");
		}

		public void UpdateConnectionProgress(int value, int maximum) {

			// Create a progress window if need be.
			if (connectionWindow == null || connectionWindow.IsDisposed) {
				if (value >= maximum) return;
				this.connectionWindow = new ConnectionWindow();
				this.connectionWindow.Show(this.mainWindow);
				if (this.mainWindow != null) this.mainWindow.Enabled = false;
			}

			this.connectionWindow.ProgressMaximum = maximum;
			this.connectionWindow.ProgressValue = value;

			if (value >= maximum) {
				this.connectionWindow.Close();
				this.connectionWindow.Dispose();
				if (this.mainWindow != null) this.mainWindow.Enabled = true;
			}
		}

		public char GetKey() {
			if (this.mainWindow != null) {
				return this.mainWindow.GetKey();
			} else { 
				return (char)0;
			}
		}

		public void Print(string value) {
			this.mainWindow?.Print(value);
		}

		public void Printer(string value) {
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
			if (traceFlag) {
				this.traceWindow = new TraceWindow();
				this.traceWindow.FormClosing += TraceWindow_FormClosing;
				this.traceWindow.Show();
				this.traceWindow.Focus();
				this.focusCommandLine = false;
			} else {
				if (this.traceWindow != null) {
					this.traceWindow.Close();
					this.traceWindow.Dispose();
					this.traceWindow = null;
				}
			}
		}

		public void UpdateState(AlbertLinkPortState state) {
			this.mainWindow?.UpdateState(state);
		}

		public void Trace(string value) {
			if (this.traceWindow != null && !this.traceWindow.IsDisposed) {
				traceWindow.Trace(value);
			}
		}

		private void TraceWindow_FormClosing(object sender, FormClosingEventArgs e) {
			this.link.SetTraceFlag(false);
		}

		public void Altered(AlteredFlags flags) {
			this.mainWindow?.Altered(flags);
		}

		public byte Control(byte parameter) {
			controlError = true;
			return 0;
		}

		public void Ask(AskType type, string prompt) {
			this.mainWindow?.Ask(type, prompt);
		}

		public void Load(string filename) {
			this.mainWindow?.LoadFile(filename);
		}

		public void Save(string filename, string procedure) {
			this.mainWindow?.SaveFile(filename, procedure);
		}

		private readonly Dictionary<byte, StreamWriter> openFiles = new Dictionary<byte, StreamWriter>(10);

		public bool File(byte channel, string filename) {
			if (string.IsNullOrEmpty(Path.GetExtension(filename))) filename += ".dat";
			try {
				var fh = System.IO.File.CreateText(filename);
				openFiles.Add(channel, fh);
				return true;
			} catch {
				return false;
			}
		}

		public void Close(byte channel) {
			if (openFiles.TryGetValue(channel, out StreamWriter fh)) {
				fh.Close();
				fh.Dispose();
				openFiles.Remove(channel);
			}
		}

		public void Store(byte channel, string data) {
			if (openFiles.TryGetValue(channel, out StreamWriter fh)) {
				fh.Write(data.Replace("\r", Environment.NewLine));
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					this.Close(0);
				}
				disposedValue = true;
			}
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

	}
}
