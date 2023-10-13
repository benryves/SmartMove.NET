using System;

namespace SmartMove {

	internal class Program {

		[STAThread]
		static void Main() {
			using (var albertLink = new AlbertLink(new SmartBox("COM2"), new AlbertLinkWindowsHost())) {
				albertLink.Run();
			}
		}

	}
}
