namespace YoutubeStorage
{
	class YoutubeStorage
	{
		public static void Main()
		{
			FFMediaToolkit.FFmpegLoader.FFmpegPath = Path.GetFullPath(@"ffmpeg\");

			string? Type = UserInput.AskQuestion(
				"Do you want to encode a file or decode a video?",
				new string[] { "e", "d" },
				"e"
			);

			if (Type == "e") Encode();
			else if (Type == "d") Decode();
		}

		private static void Encode()
		{
			// We're going to encode a file into a video
			string? SourceFile = UserInput.GetFilePath("Give the path to the file to encode:");
			if (SourceFile == null) return;

			// We have all required information so create the handler.
			// Also check that target file doesn't already exist.
			Encoder handler = new(SourceFile);
			try
			{
				handler.GenerateTargetFile();
				Console.WriteLine($"Target file is: {handler.TargetFile}");
			}
			catch (IOException)
			{
				Console.WriteLine($"Target file already exists: {handler.TargetFile}");
				return;
			}

			handler.Handle();
		}

		private static void Decode()
		{
			// We're going to decode a video into a file
			string? SourceFile = UserInput.GetFilePath("Give the path to the video to decode:");
			if (SourceFile == null) return;

			if (!SourceFile.Contains("(YS-") || !SourceFile.EndsWith(".mp4"))
			{
				Console.WriteLine("File is invalid\n" +
					"Filename should contain the following: " +
					"\"(YS-{Extension excl. dot of original file})\"");
				return;
			}

			// We have all required information so create the handler.
			// Also check that target file doesn't already exist.
			Decoder handler = new(SourceFile);
			try
			{
				handler.GenerateTargetFile();
				Console.WriteLine($"Target file is: {handler.TargetFile}");
			}
			catch (IOException)
			{
				Console.WriteLine($"Target file already exists: {handler.TargetFile}");
				return;
			}

			handler.Handle();
		}
	}

	abstract class Handler
	{
		public const int PixelsPerFrame = 1920 * 1080;

		public abstract void GenerateTargetFile();
		public abstract void Handle();
	}
}
