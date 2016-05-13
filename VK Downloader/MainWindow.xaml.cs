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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AngleSharp.Parser.Html;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
			this.DataContext = _viewModel;
			InitializeComponent();
		}

		private async void buttonDownload_Click(object sender, RoutedEventArgs e)
		{
			var parser = new VkParser { Id = "9125493_217057" };
			_viewModel.StatusBarText = "Parsing links";
			parser.DownloadCompleted += OnParsingComplete;
			_parsingLinksDialogController =
				await this.ShowProgressAsync("Parsing links", "It only takes a while...", false, new MetroDialogSettings()
				{
					AnimateHide = true,
					AnimateShow = true
				});
			_parsingLinksDialogController.SetIndeterminate();
			_viewModel.FileModels.AddRange(await parser.ParseDownloadLinks());
			FileGrid.Items.Refresh();

			//var file1 = res[0];
			//WebRequest request = WebRequest.Create(file1.DownloadLink);
			//var response = request.GetResponse();
			//using (WebClient client = new WebClient())
			//{
			//	client.DownloadFile(file1.DownloadLink, $"{file1.FileName}{MimeTypes.MimeTypeMap.GetExtension(response.ContentType)}");
			//}

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

		private void ButtonDownloadFile_OnClick(object sender, RoutedEventArgs e)
		{
			_viewModel.FileModels.ForEach(t =>
			{
				Trace.WriteLine(t.SongName);
			});
		}

		private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var ids = FileGrid.SelectedItems.Cast<SongViewModel>().Select(s => s.Number);
			_viewModel.FileModels.RemoveAll(t => ids.Any(s => s == t.Number));
			FileGrid.Items.Refresh();
		}
	}
}
