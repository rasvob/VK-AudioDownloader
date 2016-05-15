using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ByteSizeLib;
using VK_Downloader.Configuration;
using VK_Downloader.ViewModels;

namespace VK_Downloader.VK
{
	public class VkDownloader
	{
		private readonly ConcurrentQueue<SongViewModel> _queueCollection;
		private SongViewModel _currentItem = new SongViewModel()
		{
			Status = "Complete"
		};

		public event EventHandler<ProgressArgs> CurrentSongDownloadProgressEvent;
		public event EventHandler AllCompleteEvent;

		private readonly string _downloadFolderPath = ConfigurationRepository.LoadDefaultDownloadFolderLocation();

		public VkDownloader()
		{
			_queueCollection = new ConcurrentQueue<SongViewModel>();
		}

		public void AddFilesToQueue(IEnumerable<SongViewModel> files)
		{
			foreach (SongViewModel model in files)
			{
				_queueCollection.Enqueue(model);
			}
		}

		public void DownloadSongs()
		{
			Task.Factory.StartNew(() =>
			{
				while (_queueCollection.Count > 0)
				{
					if (_currentItem.Status.Equals("Incomplete"))
					{
						continue;
					}
					if (_queueCollection.TryDequeue(out _currentItem))
					{
						DownloadSong(_currentItem);
					}
				}
			});
		}

		public void DownloadSong(SongViewModel vm)
		{
			using (WebClient client = new WebClient())
			{
				client.DownloadProgressChanged += vm.ClientOnDownloadProgressChanged;
				client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
				client.DownloadFileCompleted += vm.ClientOnDownloadFileCompleted;
				client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
				client.DownloadFileAsync(new Uri(vm.DownloadLink), GetFullFilePath(vm.FileName));
			}
		}

		private void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
		{
			if (_queueCollection.Count <= 1)
			{
				OnAllCompleteEvent();
			}
		}

		private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
		{
			var args = new ProgressArgs()
			{
				Percentage = downloadProgressChangedEventArgs.ProgressPercentage,
				ViewModel = _currentItem
			};
			OnCurrentSongDownloadProgressEvent(args);
		}

		private string GetFullFilePath(string fileName)
		{
			return Path.Combine(_downloadFolderPath, fileName);
		}

		protected virtual void OnCurrentSongDownloadProgressEvent(ProgressArgs e)
		{
			CurrentSongDownloadProgressEvent?.Invoke(this, e);
		}

		protected virtual void OnAllCompleteEvent()
		{
			AllCompleteEvent?.Invoke(this, EventArgs.Empty);
		}
	}
}