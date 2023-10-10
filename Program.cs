using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBox {

	internal class Program {

		static void Main(string[] args) {
			using (var albertLink = new AlbertLink("COM2", new AlbertLinkConsoleHost())) {
				albertLink.Run();
			}
		}

	}
}
