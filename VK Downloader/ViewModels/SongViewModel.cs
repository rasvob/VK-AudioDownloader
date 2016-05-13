using System.ComponentModel;
using System.Runtime.CompilerServices;
using VK_Downloader.Annotations;

namespace VK_Downloader.ViewModels
{
	public class SongViewModel: INotifyPropertyChanged
	{
		private double _size;
		private long _completedBytes = 0;
		private string _artist;
		private string _songName;
		private string _status = "Incomplete";
		private int _number;
		private long _realSize;
		public static int NumberHolder = 1;

		public string Status
		{
			get { return _status; }
			set
			{
				if (value == _status) return;
				_status = value;
				OnPropertyChanged();
			}
		}

		public int Number => _number;
		

		public string Artist
		{
			get { return _artist; }
			set
			{
				if (value == _artist) return;
				_artist = value;
				OnPropertyChanged();
			}
		}

		public string SongName
		{
			get { return _songName; }
			set
			{
				if (value == _songName) return;
				_songName = value;
				OnPropertyChanged();
			}
		}

		public double Size
		{
			get { return _size; }
			set
			{
				if (value == _size) return;
				_size = value;
				OnPropertyChanged();
			}
		}

		public string DownloadLink { get; set; }

		public long CompletedBytes
		{
			get { return _completedBytes; }
			set
			{
				if (value == _completedBytes) return;
				_completedBytes = value;
				OnPropertyChanged();
			}
		}

		public SongViewModel()
		{
			_number = NumberHolder++;
		}


		public static void ResetSeed()
		{
			NumberHolder = 1;
		}

		public long RealSize
		{
			get { return _realSize; }
			set
			{
				if (value == _realSize) return;
				_realSize = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}