using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;

namespace YoutubeStorage
{
	internal class Encoder : Handler
	{
		private readonly string SourceFile;
		public string? TargetFile;

		public Encoder(string sourceFile)
		{
			SourceFile = sourceFile;
		}

		public override void GenerateTargetFile()
		{
			TargetFile = Path.Combine(
				Path.GetDirectoryName(SourceFile) ?? "",
				$"{Path.GetFileNameWithoutExtension(SourceFile)} (YS-{Path.GetExtension(SourceFile)[1..^0]}).mp4"
			);

			if (File.Exists(TargetFile))
				throw new IOException();
		}

		public override void Handle()
		{
			VideoEncoderSettings Settings = new(1920, 1080, 60, VideoCodec.H264)
			{
				CRF = 16,
				EncoderPreset = EncoderPreset.Medium
			};

			using FileStream File = new(SourceFile, FileMode.Open);
			using MediaOutput Dest = MediaBuilder.CreateContainer(TargetFile).WithVideo(Settings).Create();

			// First frame is length of file encoded in binary
			// Number is at the end of the frame, with 0 before
			long FileSize = File.Length * 8; // bits
			byte[] FileLength = Convert.ToString(File.Length, 2).Select(e => (byte)(e == '1' ? 255 : 0)).ToArray();
			byte[] LengthPixels = new byte[PixelsPerFrame - FileLength.Length].Concat(FileLength).ToArray();
			ImageData LengthFrame = new(LengthPixels, ImagePixelFormat.Gray8, 1920, 1080);
			Dest.Video.AddFrame(LengthFrame);

			// Then second frame and further is file
			int BytesPerFrame = PixelsPerFrame / 8;
			byte[] Bytes = new byte[BytesPerFrame];
			long SizeConverted = 0;

			while (true)
			{
				int BytesRead = File.Read(Bytes, 0, BytesPerFrame);
				if (BytesRead == 0)
					break;

				byte[] Bits = string.Join(
						string.Empty,
						Bytes.Select(
							e => Convert.ToString(e, 2).PadLeft(8, '0')
						)
					).Select(e => (byte)(e == '1' ? 255 : 0))
					.ToArray();
				ImageData Frame = new(Bits, ImagePixelFormat.Gray8, 1920, 1080);
				Dest.Video.AddFrame(Frame);

				Bytes = new byte[BytesPerFrame];
				SizeConverted += BytesRead * 8; // bits
				Console.Write($"\r{(byte)((double)SizeConverted / FileSize * 100)}%");
			}

			// Add empty frame at the end
			Dest.Video.AddFrame(new(new byte[PixelsPerFrame], ImagePixelFormat.Gray8, 1920, 1080));
		}
	}
}
