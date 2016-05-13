namespace VK_Downloader.VK
{
	public class FileModel
	{
		public string Artist { get; set; } = "Unknown";
		public string SongName { get; set; } = "Unknown";
		public string FileName => Artist + " - " + SongName.TrimEnd(' ');
		public string DownloadLink { get; set; }
		public int Size { get; set; }

		public override string ToString()
		{
			return $"FileName: {FileName}; Link: {DownloadLink}";
		}
	}
}