using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBox {

	public interface IAlbertLinkHost {

		void Initialize(AlbertLink link);

		byte[] GetAlbertLinkProgram();

		void Print(char value);

		bool DisplayError(string procedure, string message, string line);

		void EditProcedure(bool build, string procedure);

		void Quit();

		void SetTraceFlag(bool traceFlag);

		void Trace(char value);

		void CheckEscapeCondition();

		char GetKey();

		void EnableCommandMode();

		void Update(AlbertLinkPortState state);

	}
}
