using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VK_Downloader.ViewModels;

namespace VK_Downloader.Configuration
{
	public static class ConfigurationRepository
	{
		private static string _fileListPath = "file_list.xml";
		private static string _defaultFolderPath = "default_folder_location.xml";
		public static void SaveFileList(List<SongViewModel> files)
		{
			using (FileStream fs = new FileStream(_fileListPath, FileMode.Create))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SongViewModel>));
				xmlSerializer.Serialize(fs, files);
			}
		}

		public static List<SongViewModel> LoadFileList()
		{
			if (!File.Exists(_fileListPath))
			{
				return null;
			}
			using(FileStream fs = new FileStream(_fileListPath, FileMode.Open))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SongViewModel>));
				return xmlSerializer.Deserialize(fs) as List<SongViewModel>;
			}
		}

		public static void SaveDefaultDownloadFolderLocation(string location)
		{
			using(FileStream fs = new FileStream(_defaultFolderPath, FileMode.Create))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
				xmlSerializer.Serialize(fs, location);
			}
		}

		public static string LoadDefaultDownloadFolderLocation()
		{

			try
			{
				using(FileStream fs = new FileStream(_defaultFolderPath, FileMode.Open))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
					var result  = xmlSerializer.Deserialize(fs) as string;
					if (result == null)
					{
						throw new Exception("Null object reference");
					}
					return result;
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}