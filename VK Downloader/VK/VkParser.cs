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
				_link = @"http://new.vk.com/wall-" + value;
				_altLink = @"http://new.vk.com/wall" + value;
			}
		}

		private string _altLink;
		private string _link;
		private string _id;

		public event EventHandler DownloadCompleted;

		public async Task<List<SongViewModel>> ParseDownloadLinks()
		{
			List<SongViewModel> links = new List<SongViewModel>();
			//string content = await Task.Factory.StartNew(() => DownloadPage(_link));
			string content = await GetParseContent();
			var parser = new HtmlParser();
			var document = parser.Parse(content);

			var trs = document.QuerySelectorAll("div.wall_audio_rows div.audio_row");

			foreach(var element in trs)
			{
				try
				{
					var fileModel = new SongViewModel();
					fileModel.DownloadLink = input.Attributes.GetNamedItem("value").Value;
					fileModel.Artist = title?.InnerHtml;
					fileModel.SongName = songName?.InnerHtml;
					links.Add(fileModel);
				}
				catch(Exception)
				{
					SongViewModel.NumberHolder--;
				}

			}

			ClearDownloadLinks(ref links);
			await Task.Factory.StartNew(() => GetFileSizesAndExtintions(links));
			OnDownloadCompleted();
			return links;
		}

		private void GetFileSizesAndExtintions(List<SongViewModel> songs)
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
					songViewModel.Extintion = MimeTypes.MimeTypeMap.GetExtension(response.ContentType);
				}
			}
		}

		private async Task<string> GetParseContent()
		{
			var content = await Task.Factory.StartNew(() => DownloadPage(_link));
			HtmlParser parser = new HtmlParser();
			var document = parser.Parse(content);
			var error = document.QuerySelector("div.message_page_title");
			if (error == null)
			{
				return content;
			}
			else 
			{
				content = await Task.Factory.StartNew(() => DownloadPage(_altLink));
			}
			return content;
		}

		private string DownloadPage(string link)
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
							link), null, null, outStream);
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

		private void ClearDownloadLinks(ref List<SongViewModel> links)
		{
			//foreach (SongViewModel link in links)
			//{
			//	Uri uri;
			//	bool res = Uri.TryCreate(link.DownloadLink, UriKind.Absolute, out uri);
			//	if (res)
			//	{
			//		if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
			//		{
			//			Trace.WriteLine("then: "+ link.DownloadLink);
			//		}
			//		else
			//		{
			//			Trace.WriteLine("thenNONIF: " + link.DownloadLink);
			//		}
			//	}
			//	else
			//	{
			//		Trace.WriteLine("else: " + link.DownloadLink);
			//	}
			//	//Trace.WriteLine(link.DownloadLink);
			//}

			links.RemoveAll(t =>
			{
				Uri uri;
				bool res = Uri.TryCreate(t.DownloadLink, UriKind.Absolute, out uri);
				if (!res) return true;
				return !(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
			});
			

		}

		protected virtual void OnDownloadCompleted()
		{
			DownloadCompleted?.Invoke(this, EventArgs.Empty);
		}
	}
}