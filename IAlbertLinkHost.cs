using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBox {

	internal enum GetKeyResult {
		None,
		Success,
		Escape,
		Disconnect,
		Quit,
	}

	internal interface IAlbertLinkHost {

		byte[] GetAlbertLinkProgram();

		void Print(char value);

		bool DisplayError(string procedure, string message, string line);

		void EditProcedure(bool build, string procedure);

		void Quit();

		void SetTraceFlag(bool traceFlag);

		GetKeyResult CheckKeyAvailable();

		GetKeyResult GetKey(out char key);

	}
}
