using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using VK_Downloader.Annotations;
using VK_Downloader.VK;

namespace VK_Downloader.ViewModels
{
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private string _vkPostId;
		private string _statusBarText = "Ready";
		private List<SongViewModel> _fileModels = new List<SongViewModel>();
		private string _loginText = "Login";
		private bool _isLoggedIn;

		public string VkPostId
		{
			get { return _vkPostId; }
			set
			{
				if (value == _vkPostId) return;
				_vkPostId = value;
				OnPropertyChanged();
			}
		}

		public List<SongViewModel> FileModels
		{
			get { return _fileModels; }
			set
			{
				if (Equals(value, _fileModels)) return;
				_fileModels = value;
				OnPropertyChanged();
			}
		}

		public string StatusBarText
		{
			get { return _statusBarText; }
			set
			{
				if (value == _statusBarText) return;
				_statusBarText = value;
				OnPropertyChanged();
			}
		}

		public string LoginText
		{
			get { return _loginText; }
			set
			{
				if (value == _loginText) return;
				_loginText = value;
				OnPropertyChanged();
			}
		}

		public bool IsLoggedIn
		{
			get { return _isLoggedIn; }
			set
			{
				LoginText = value ? "Logout" : "Login";
				_isLoggedIn = value;
				OnPropertyChanged();
			}
		}

		public void LoggedIn(object sender, EventArgs args)
		{
			IsLoggedIn = true;
		}

		public void LoggedOut(object sender, EventArgs args)
		{
			IsLoggedIn = false;
		}
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}