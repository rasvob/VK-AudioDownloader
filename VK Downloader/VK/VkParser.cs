using System;
using System.Collections.Generic;
using AngleSharp;
using VK_Downloader.Logging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using NReco.PhantomJS;
using VK_Downloader.FileDownload;
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
				_link = @"http://new.vk.com/wall-" + _id;
				_altLink = @"http://new.vk.com/wall" + _id;
			}
		}

		private string _altLink;
		private string _link;
		private string _id;
		public string Token { get; set; }

		public event EventHandler DownloadCompleted;

		public async Task<List<SongViewModel>> ParseDownloadLinks()
		{
			string content = await GetParseContent();
			var parser = new HtmlParser();
			var document = parser.Parse(content);

			var trs = document.QuerySelectorAll("div.wall_audio_rows div.audio_row")
				.Cast<IHtmlDivElement>()
				.Where(t => !t.Dataset["audio"].Contains("deleteHash"))
				.Select(s => s.Dataset["full-id"]);

			var linkObtainer = new VkDownloadLinkObtainer(Token);
			List<SongViewModel> links = await linkObtainer.ObtainDownloadLinks(trs);

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
			var content = await Task.Factory.StartNew(() => PhantomDownloader.DownloadPage(_link));
			HtmlParser parser = new HtmlParser();
			var document = parser.Parse(content);
			var error = document.QuerySelector("div.message_page_title");
			if (error == null)
			{
				return content;
			}
			else 
			{
				content = await Task.Factory.StartNew(() => PhantomDownloader.DownloadPage(_altLink));
			}
			return content;
		}

		public static bool ParseId(string text, ref string res)
		{

			Regex regexPureID = new Regex(@"^[0-9_]+$");
			if(regexPureID.IsMatch(text))
			{
				res = text;
				return true;
			}

			Regex start = new Regex(@"^(https:\/\/(new.)?vk.com)");
			if (!start.IsMatch(text))
			{
				return false;
			}

			Regex regex = new Regex(@"(wall-?)([0-9|_]+).*");
			Match match = regex.Match(text);

			if (match.Success)
			{
				res = match.Groups[2].Value;
				return true;
			}

			

			return false;
		}

		protected virtual void OnDownloadCompleted()
		{
			DownloadCompleted?.Invoke(this, EventArgs.Empty);
		}
	}
}