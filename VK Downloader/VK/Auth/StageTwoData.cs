namespace VK_Downloader.VK.Auth
{
	public class StageTwoData
	{
		public string QHash { get; set; }
		public string RemixQName { get; set; }
		public string RemixQValue { get; set; }
		public string RemixLkh { get; set; }

		public string ParseQHash(string name)
		{
			int startIndex = name.IndexOf("_") + 1;
			return name.Substring(startIndex);
		}
	}
}