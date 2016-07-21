                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     using System.Windows.Media.Animation;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AngleSharp.Parser.Html;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using VK_Downloader.Configuration;
using VK_Downloader.ConfigurationView;
using VK_Downloader.ViewModels;
using VK_Downloader.VK;

namespace VK_Downloader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private readonly MainWindowViewModel _viewModel;
		private readonly VkAuthProvider _authProvider;
		private ProgressDialogController _parsingLinksDialogController;

		public MainWindow()
		{
			_authProvider = new VkAuthProvider();
			_viewModel = new MainWindowViewModel();
			_viewModel.FileModels = ConfigurationRepository.LoadFileList();

			_authProvider.LoggedIn += _viewModel.LoggedIn;
			_authProvider.LoggedOut += _viewModel.LoggedOut;

			_authProvider.LoadSavedToken();

			DataContext = _viewModel;
			DesignInit();
			InitializeComponent();
		}

		private void DesignInit()
		{
			SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");
		}

		private async void buttonDownload_Click(object sender, RoutedEventArgs e)
		{
			if (!_authProvider.IsLoggedIn)
			{
				await ShowSucceessDialog("No account provided", "You must login to Vk to be able to download audio files", "Ok");
				return;
			}

			string id = string.Empty;
			if (!VkParser.ParseId(_viewModel.VkPostId,ref id))
			{
				await ShowSucceessDialog("Wrong ID", "Please, provide ID in correct form", "Ok");
				return;
			}

			var parser = new VkParser { Id = id, Token = _authProvider.Token };
			_viewModel.StatusBarText = "Parsing links";
			parser.DownloadCompleted += OnParsingComplete;
			_parsingLinksDialogController =
				await this.ShowProgressAsync("Parsing links", "It only takes a while...", false, new MetroDialogSettings()
				{
					AnimateHide = true,
					AnimateShow = true
				});
			_parsingLinksDialogController.SetIndeterminate();
			var result = await parser.ParseDownloadLinks();
			_viewModel.FileModels.AddRange(result);
			foreach(SongViewModel model in _viewModel.FileModels)
			{
				Trace.WriteLine(model.FileName);
			}
			Dispatcher.Invoke(() =>
			{
				FileGrid.Items.Refresh();
			});
		}

		private async void OnParsingComplete(object sender, EventArgs args)
		{
			_viewModel.StatusBarText = "Ready";
			_viewModel.VkPostId = string.Empty;
			await _parsingLinksDialogController.CloseAsync();
		}

		private void SelecteAllMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			FileGrid.SelectAll();
		}

		private void SelectNoneMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			FileGrid.UnselectAll();
		}

		private async void ButtonDownloadFile_OnClick(object sender, RoutedEventArgs e)
		{
			if (_viewModel.FileModels.Count < 1)
			{
				await this.ShowMessageAsync("No links to download", "You need to parse links first");
				return;
			}
			VkDownloader downloader = new VkDownloader();
			FileGrid.CanUserDeleteRows = false;
			var filesToDownload = FileGrid.Items.Cast<SongViewModel>().ToList().Where(t => t.Status.Equals("Incomplete")).ToList();
			downloader.AddFilesToQueue(filesToDownload);
			downloader.CurrentSongDownloadProgressEvent += StatusProgressUpdate;
			downloader.AllCompleteEvent += DownloaderOnAllCompleteEvent;
			downloader.DownloadSongs();
		}

		private void DownloaderOnAllCompleteEvent(object sender, EventArgs eventArgs)
		{
			_viewModel.StatusBarText = "Ready";
			Dispatcher.Invoke(new Action(() => FileGrid.CanUserDeleteRows = true));
		}

		private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var ids = FileGrid.SelectedItems.Cast<SongViewModel>().Select(s => s.Number);
			_viewModel.FileModels.RemoveAll(t => ids.Any(s => s == t.Number));
			FileGrid.Items.Refresh();
		}

		public void StatusProgressUpdate(object sender, ProgressArgs args)
		{
			_viewModel.StatusBarText = $"{args.ViewModel.Artist} - {args.ViewModel.SongName} ({args.Percentage}%)";
		}

		private async void ButtonDownloadFilesSelected_OnClick(object sender, RoutedEventArgs e)
		{
			if(_viewModel.FileModels.Count < 1)
			{
				await this.ShowMessageAsync("No links to download", "You need to parse links first");
				return;
			}
			VkDownloader downloader = new VkDownloader();
			FileGrid.CanUserDeleteRows = false;
			downloader.AddFilesToQueue(FileGrid.SelectedItems.Cast<SongViewModel>().ToList());
			downloader.CurrentSongDownloadProgressEvent += StatusProgressUpdate;
			downloader.AllCompleteEvent += DownloaderOnAllCompleteEvent;
			downloader.DownloadSongs();
		}

		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var fileList = _viewModel.FileModels.Where(t => t.Status.Equals("Incomplete")).ToList();
			ConfigurationRepository.SaveFileList(fileList);
			ConfigurationRepository.SaveDefaultDownloadFolderLocation(new SettingsWindowViewModel().FolderPath);
		}

		private void FileGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
		{
			e.Row.Header = (e.Row.GetIndex() + 1).ToString();
		}

		private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new SettingsWindow
			{
				Owner = this,
				BorderThickness = new Thickness(1),
				GlowBrush = null
			};
			settingsWindow.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");
			settingsWindow.Closing += (o, args) =>
			{
				settingsWindow.Owner = null;
			};
			settingsWindow.Show();
		}


		private void FileGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var selectedItem = FileGrid.SelectedItem as SongViewModel;
			if (selectedItem == null || selectedItem.Status.Equals("Incomplete"))
			{
				return;
			}
			System.Diagnostics.Process.Start("explorer.exe", ConfigurationRepository.LoadDefaultDownloadFolderLocation());
		}

		private async void MenuItemLogin_OnClick(object sender, RoutedEventArgs e)
		{
			if (_authProvider.IsLoggedIn)
			{
				_authProvider.Logout();
				await ShowSucceessDialog("Logout successful", "You're logged out", "Ok");
				return;
			}

			var settings = new LoginDialogSettings()
			{
				ColorScheme = this.MetroDialogOptions.ColorScheme,
				EnablePasswordPreview = true,
				AnimateShow = true,
				NegativeButtonVisibility = Visibility.Visible,
				NegativeButtonText = "Cancel"
			};

			LoginDialogData res = await this.ShowLoginAsync("Login to you Vk Account", "Enter your credentials", settings);

			if (res != null)
			{
				var settingsProg = new MetroDialogSettings()
				{
					AnimateShow = false,
					AnimateHide = false,
					ColorScheme = this.MetroDialogOptions.ColorScheme
				};
				ProgressDialogController controller = null;
				try
				{
					controller = await this.ShowProgressAsync("Login progress", "Wait a moment...", false, settingsProg);
					controller.SetIndeterminate();
					await _authProvider.Login(res.Username, res.Password);
					await controller.CloseAsync();
					await ShowSucceessDialog("Login successful", $"Current username: {res.Username}", "Continue");
				}
				catch (Exception)
				{
					await controller?.CloseAsync();
					await ShowSucceessDialog("Login failed", "Wrong username or password", "Cancel");
				}
			}
		}

		private async Task ShowSucceessDialog(string title, string message, string okText)
		{
			var settings = new MetroDialogSettings()
			{
				ColorScheme = this.MetroDialogOptions.ColorScheme,
				AnimateShow = true,
				AffirmativeButtonText = okText
			};
			await this.ShowMessageAsync(title, message, settings: settings);
		}
	}
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                