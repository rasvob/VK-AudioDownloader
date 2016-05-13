using System;
using System.Collections.Generic;
using AngleSharp;
using VK_Downloader.Logging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using AngleSharp.Dom;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using NReco.PhantomJS;
using VK_Downloader.ViewModels;

namespace VK_Downloader.VK
{
	public class VkParser
	{
		public string Id
		{
			get { return _id; }
			set
			{
				_id = value;
				_link = @"http://vk.com/wall-" + value;
			}
		}

		private string _link;
		private string _id;

		public event EventHandler DownloadCompleted;

		public async Task<List<SongViewModel>> ParseDownloadLinks()
		{
			List<SongViewModel> links = new List<SongViewModel>();
			string content = await Task.Factory.StartNew(DownloadPage);
			var parser = new HtmlParser();
			var document = parser.Parse(content);
			var trs = document.QuerySelectorAll("div.audio div.area table tbody tr");

			foreach(var element in trs)
			{
				var input = element.QuerySelector("td").QuerySelector("input");
				var info = element.QuerySelector("td.info")?.QuerySelector("div.title_wrap");
				var title = info?.QuerySelector("a");
				var songName = info?.QuerySelector("span.title");
				try
				{
					var fileModel = new SongViewModel();
					fileModel.DownloadLink = input.Attributes.GetNamedItem("value").Value;
					fileModel.Artist = title?.InnerHtml;
					fileModel.SongName = songName?.InnerHtml;
					links.Add(fileModel);
				}
				catch(Exception ex)
				{
					SongViewModel.NumberHolder--;
				}

			}
			await Task.Factory.StartNew(() => GetFileSizes(links));
			OnDownloadCompleted();
			return links;
		}

		private void GetFileSizes(List<SongViewModel> songs)
		{
			foreach (SongViewModel songViewModel in songs)
			{
				WebRequest request = WebRequest.Create(songViewModel.DownloadLink);
				request.Method = "HEAD";
				using (WebResponse response = request.GetResponse())
				{
					int contentLenght = 0;
					if (int.TryParse(response.Headers.Get("Content-Length"), out contentLenght))
					{
						var fileSize = ByteSizeLib.ByteSize.FromBytes(contentLenght);
						songViewModel.Size = Math.Truncate(fileSize.MegaBytes * 100) / 100;
						songViewModel.RealSize = contentLenght;
					}
				}
			}
		}

		private string DownloadPage()
		{
			PhantomJS phantomJs = new PhantomJS();
			string content;
			using(var outStream = new MemoryStream())
			{
				try
				{
					phantomJs.RunScript(
						string.Format(
							"var system = require('system'); var page = require('webpage').create(); page.open('{0}', function() {{ system.stdout.writeLine(page.content); phantom.exit(); }});",
							_link), null, null, outStream);
					outStream.Seek(0, SeekOrigin.Begin);
					using (StreamReader reader = new StreamReader(outStream))
					{
						content = reader.ReadToEnd();
					}
				}
			
				finally
				{
					phantomJs.Abort();
				}
			}
			return content;
		}



		protected virtual void OnDownloadCompleted()
		{
			DownloadCompleted?.Invoke(this, EventArgs.Empty);
		}
	}
}