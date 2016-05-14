﻿using System;
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
		private ProgressDialogController _parsingLinksDialogController;

		public MainWindow()
		{
			_viewModel = new MainWindowViewModel();
			_viewModel.FileModels = ConfigurationRepository.LoadFileList();
			DataContext = _viewModel;
			InitializeComponent();
		}

		private async void buttonDownload_Click(object sender, RoutedEventArgs e)
		{
			var parser = new VkParser { Id = "43722144_4912" };
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
			foreach (SongViewModel model in _viewModel.FileModels)
			{
				Trace.WriteLine(model.FileName);
			}
			FileGrid.Items.Refresh();
		}

		private async void OnParsingComplete(object sender, EventArgs args)
		{
			_viewModel.StatusBarText = "Ready";
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
				await this.ShowMessageAsync("No links to download", "Yout need to parse links first");
				return;
			}
			VkDownloader downloader = new VkDownloader();
			var filesToDownload = FileGrid.Items.Cast<SongViewModel>().ToList().Where(t => t.Status.Equals("Incomplete")).ToList();
			downloader.AddFilesToQueue(filesToDownload);
			downloader.CurrentSongDownloadProgressEvent += StatusProgressUpdate;
			downloader.AllCompleteEvent += DownloaderOnAllCompleteEvent;
			downloader.DownloadSongs();
		}

		private void DownloaderOnAllCompleteEvent(object sender, EventArgs eventArgs)
		{
			_viewModel.StatusBarText = "Ready";
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
				await this.ShowMessageAsync("No links to download", "Yout need to parse links first");
				return;
			}
			VkDownloader downloader = new VkDownloader();
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
			SettingsWindow settingsWindow = new SettingsWindow {Owner = this};
			settingsWindow.BorderThickness = new Thickness(1);
			settingsWindow.GlowBrush = null;
			settingsWindow.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");
			settingsWindow.Show();
		}
	}
}
