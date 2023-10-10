using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartBox {
	internal class AlbertLinkConsoleHost : IAlbertLinkHost {
		
		public bool DisplayError(string procedure, string message, string line) {
			return false;
        }

		public void EditProcedure(bool build, string procedure) {
            Console.WriteLine("{0} {1} not supported", build ? "Building" : "Editing", procedure);
        }

		public byte[] GetAlbertLinkProgram() {
			return File.ReadAllBytes("AL.COD");
		}

		private Queue<ConsoleKeyInfo> keys = new Queue<ConsoleKeyInfo>(8);

		public GetKeyResult CheckKeyAvailable() {

			while (Console.KeyAvailable) {
				keys.Enqueue(Console.ReadKey(true));
			}

			if (keys.Count == 0) {
				return GetKeyResult.None;
			} else if (keys.Any(k => k.Key == ConsoleKey.Escape)) {
				keys.Clear();
				return GetKeyResult.Escape;
			} else if (keys.Any(k => k.Key == ConsoleKey.F10 && (k.Modifiers & ConsoleModifiers.Shift) != 0)) {
				keys.Clear();
				return GetKeyResult.Disconnect;
			} else {
				return GetKeyResult.Success;
			}

		}

		public GetKeyResult GetKey(out char key) {

			var result = CheckKeyAvailable();

			if (result == GetKeyResult.Success) {
				key = keys.Dequeue().KeyChar;
			} else {
				key = (char)0;
				keys.Clear();
			}

			return result;

		}

		public void Print(string value) {
			foreach (var c in value) {
				Console.CursorVisible = false;
				this.Print(c);
				Console.CursorVisible = true;
			}
		}

		public void Print(char value) {
			switch (value) {
				case (char)1:
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case (char)2:
					Console.ResetColor();
					break;
				case '\b':
					Console.Write("\b \b");
					break;
				case '\r':
                    Console.WriteLine();
                    break;
				case (char)12:
					Console.Clear();
					break;
				default:
					Console.Write(value);
					break;
			}

		}

		public void Quit() { }

		public void SetTraceFlag(bool traceFlag) { }

    }
}
