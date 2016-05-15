﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using VK_Downloader.Annotations;
using VK_Downloader.Configuration;

namespace VK_Downloader.ViewModels
{
	public class SettingsWindowViewModel: INotifyPropertyChanged
	{
		private string _folderPath;

		public string FolderPath
		{
			get { return _folderPath; }
			set
			{
				if (value == _folderPath) return;
				_folderPath = value;
				OnPropertyChanged();
			}
		}

		public SettingsWindowViewModel()
		{
			FolderPath = ConfigurationRepository.LoadDefaultDownloadFolderLocation();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}