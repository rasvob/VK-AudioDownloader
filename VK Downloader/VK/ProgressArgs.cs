using System;
using VK_Downloader.ViewModels;

namespace VK_Downloader.VK
{
	public class ProgressArgs: EventArgs
	{
		public SongViewModel ViewModel { get; set; }
		public int Percentage { get; set; }
	}
}