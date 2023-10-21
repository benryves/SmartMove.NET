using System.Windows.Forms;

namespace SmartMove {

	public interface IAlbertLinkHost {

		void Initialize(AlbertLink link);

		byte[] GetAlbertLinkProgram();

		void ShowSignOn();

		void UpdateConnectionProgress(int value, int maximum);

		void Print(string value);

		bool DisplayError(string procedure, string message, string line);

		void EditProcedure(bool build, string procedure);

		void Quit();

		void SetTraceFlag(bool traceFlag);

		void Trace(string value);

		void Idle();

		char GetKey();

		void EnableCommandMode();

		void UpdateState(AlbertLinkPortState state);

		void Altered(AlteredFlags flags);

		byte Control(byte parameter);

		void Ask(AskType type, string prompt);

		void Load(string filename);

		void Save(string filename, string procedure);

		bool File(byte channel, string filename);

		void Close(byte channel);

		void Store(byte channel, string data);

		void Printer(string value);

	}
}
