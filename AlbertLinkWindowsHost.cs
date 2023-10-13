﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SmartBox {
	internal class AlbertLinkWindowsHost : IAlbertLinkHost {

		private AlbertLink link;

		private MainWindow mainWindow;
		private readonly Dictionary<string, EditorWindow> editorWindows = new Dictionary<string, EditorWindow>();
		private TraceWindow traceWindow;
		private ConnectionWindow connectionWindow;

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

		public void Idle() {
			Application.DoEvents();
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
			return File.ReadAllBytes("AL.COD");
		}

		public void UpdateConnectionProgress(int value, int maximum) {

			// Create a progress window if need be.
			if (connectionWindow == null || connectionWindow.IsDisposed) {
				if (value >= maximum) return;
				connectionWindow = new ConnectionWindow();
				connectionWindow.Show(this.mainWindow);
			}

			this.connectionWindow.ProgressMaximum = maximum;
			this.connectionWindow.ProgressValue = value;

			if (value >= maximum) {
				this.connectionWindow.Close();
				this.connectionWindow.Dispose();
			}
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

		public void Trace(char value) {
			if (this.traceWindow != null && !this.traceWindow.IsDisposed) {
				traceWindow.Print(value);
			}
		}

		private void TraceWindow_FormClosing(object sender, FormClosingEventArgs e) {
			this.link.SetTraceFlag(false);
		}

		public void ResetLabels() {
			this.mainWindow?.ResetLabels();
		}

		public void UpdateLabel(string sourceLabel, string newLabel, bool softLabel) {
			this.mainWindow?.UpdateLabel(sourceLabel, newLabel, softLabel);
		}

	}
}
