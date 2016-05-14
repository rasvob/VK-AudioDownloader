using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gat.Controls;
using MahApps.Metro.Controls;
using VK_Downloader.Configuration;
using VK_Downloader.ViewModels;

namespace VK_Downloader.ConfigurationView
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : MetroWindow
	{
		private readonly SettingsWindowViewModel _viewModel;

		public event EventHandler<string> DownloadFolderLocationChanged; 

		public SettingsWindow()
		{
			_viewModel = new SettingsWindowViewModel();
			DataContext = _viewModel;
			InitializeComponent();
		}

		protected virtual void OnDownloadFolderLocationChanged(string e)
		{
			DownloadFolderLocationChanged?.Invoke(this, e);
		}

		private void ButtonSetFolder_OnClick(object sender, RoutedEventArgs e)
		{
			OpenDialogView openDialog = new OpenDialogView();
			OpenDialogViewModel vm = openDialog.DataContext as OpenDialogViewModel;
			vm.IsDirectoryChooser = true;
			vm.Owner = this;
			vm.Caption = "Choose download folder";
			vm.CancelText = "Cancel";
			vm.OpenText = "Choose";
			bool? result = vm.Show();
			if (result == true)
			{
				_viewModel.FolderPath = vm.SelectedFilePath;
				ConfigurationRepository.SaveDefaultDownloadFolderLocation(vm.SelectedFilePath);
			}
		}
	}
}
