using System;
using System.IO;

namespace VK_Downloader.Logging
{
	public static class FileLogger
	{
		public static readonly string LogName = "vk_downloader_log_file.txt";

		public static void LogAction(string desc)
		{
			using (FileStream fs = new FileStream(LogName, FileMode.Append))
			{
				using (StreamWriter writer = new StreamWriter(fs))
				{
					writer.WriteLine($"{DateTime.UtcNow}: {desc}");
				}
			}
		}
	}
}