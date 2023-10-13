using System;

namespace SmartBox {

	internal class Program {

		[STAThread]
		static void Main() {
			using (var albertLink = new AlbertLink("COM2", new AlbertLinkWindowsHost())) {
				albertLink.Run();
			}
		}

	}
}
