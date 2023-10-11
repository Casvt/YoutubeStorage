using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;

namespace YoutubeStorage
{
	internal class Decoder : Handler
	{
		private readonly string SourceFile;
		public string? TargetFile;

		public Decoder(string sourceFile)
		{
			SourceFile = sourceFile;
		}

		public override void GenerateTargetFile()
		{
			string Extension = '.' + SourceFile[
				(SourceFile.LastIndexOf($"(YS-") + 4)..SourceFile.LastIndexOf(')')
			];
			TargetFile = SourceFile[..SourceFile.LastIndexOf(" (YS-")] + Extension;

			if (File.Exists(TargetFile))
				throw new IOException();
		}

		public override void Handle()
		{
			MediaFile File = MediaFile.Open(SourceFile);
            ImageData Frame = File.Video.GetNextFrame();

            string FlattenedData = string.Join(
				string.Empty,
				Frame.Data[((PixelsPerFrame - 64) * 3)..(PixelsPerFrame * 3)]
					.ToArray()
					.Where((e, i) => i % 3 == 0)
					.Select(e => e < 127 ? '0' : '1')
			);
			long FileSize = Convert.ToInt64(FlattenedData, 2) * 8; // Bits
			long SizeConverted = 0; // Bits

			using FileStream Dest = new(TargetFile, FileMode.OpenOrCreate);
			while (File.Video.TryGetNextFrame(out Frame))
			{
				// Every three values is one bit (r, g and b of the pixel)
				string Bits = string.Join(
					string.Empty,
					Frame.Data[0..((int)Math.Min(PixelsPerFrame, FileSize - SizeConverted) * 3)]
                        .ToArray()
						.Where((e, i) => i % 3 == 0)
						.Select(e => e < 127 ? '0' : '1')
				);

				byte[] Bytes = new byte[Bits.Length / 8];

				for (int i = 0; i < Bytes.Length; i++)
					Bytes[i] = Convert.ToByte(
						string.Join(
							string.Empty,
							Bits.AsMemory(i * 8, 8)
					), 2);

				Dest.Write(Bytes);

				SizeConverted += Bits.Length;
				Console.Write($"\r{(byte)((double)SizeConverted / FileSize * 100)}%");
			}
		}
	}
}
