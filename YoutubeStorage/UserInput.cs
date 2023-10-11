namespace YoutubeStorage
{
	internal static class UserInput
	{
		private static readonly string CancelWord = "cancel";

		internal static string? AskQuestion(
			string Question,
			string[] Options,
			string DefaultOption
		)
		{
			Question = $"{CancelWord} | {Question} [{string.Join('|', Options.Select(o => o == DefaultOption ? o.ToUpper() : o.ToLower()))}] ";
			while (true)
			{
				Console.Write(Question);
				string? Answer = Console.ReadLine()?.ToLower();
				if (Answer == CancelWord) return null;
				else if (string.IsNullOrWhiteSpace(Answer)) return DefaultOption;
				else if (Options.Contains(Answer)) return Answer;
				else Console.WriteLine("Invalid answer");
			}
		}

		internal static string? GetFilePath(
			string Question
		)
		{
			Question = $"{CancelWord} | {Question} ";
			while (true)
			{
				Console.Write(Question);
				string? Answer = Console.ReadLine();

				if (Answer == CancelWord) return null;
				else if (string.IsNullOrWhiteSpace(Answer)) continue;

				string SourceFile = Path.GetFullPath(Answer);
				if (File.Exists(SourceFile)) return SourceFile;

				Console.WriteLine("File not found");
			}
		}
	}
}
