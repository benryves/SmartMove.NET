namespace SmartMove {

	public interface IAlbertLinkHost {

		void Initialize(AlbertLink link);

		byte[] GetAlbertLinkProgram();

		void ShowSignOn();

		void UpdateConnectionProgress(int value, int maximum);

		void Print(char value);

		bool DisplayError(string procedure, string message, string line);

		void EditProcedure(bool build, string procedure);

		void Quit();

		void SetTraceFlag(bool traceFlag);

		void Trace(char value);

		void Idle();

		char GetKey();

		void EnableCommandMode();

		void UpdateState(AlbertLinkPortState state);

		void ResetLabels();

		void UpdateLabel(string sourceLabel, string newLabel, bool softLabel);

		byte Control(byte parameter);

	}
}
