using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using VK_Downloader.ViewModels;

namespace VK_Downloader.VK
{
	public class VkDownloadLinkObtainer
	{
		private readonly string _baseUrl = @"https://vk.com/al_audio.php";
		public string Token { get; set; }

		public VkDownloadLinkObtainer(string token)
		{
			Token = token;
		}

		public async Task<List<SongViewModel>> ObtainDownloadLinks(IEnumerable<string> ids)
		{
			var uri = new Uri(_baseUrl);
			var client = new RestClient(uri);
			client.CookieContainer = new CookieContainer();
			client.CookieContainer.Add(uri, new Cookie("remixsid", Token));
			client.FollowRedirects = false;

			var res = new List<SongViewModel>();

			foreach(string id in ids)
			{
				var req = new RestRequest(Method.POST);
				req.AddParameter("application/x-www-form-urlencoded", $"act=reload_audio&al=1&ids={id}", ParameterType.RequestBody);
				var resForId = await client.ExecuteTaskAsync(req);
				res.Add(ParseContent(resForId.Content));
			}
			return res;
		}

		public SongViewModel ParseContent(string content)
		{
			var res = new SongViewModel();
			var idxStart = content.IndexOf("[[", StringComparison.Ordinal)+2;
			var idxEnd = content.LastIndexOf("]]", StringComparison.Ordinal);

			content = content.Substring(idxStart, idxEnd - idxStart);
			content = content.Replace(@"\", string.Empty);
			var chunks = content.Split(',');
			for (var i = 0; i < chunks.Length; i++)
			{
				chunks[i] = chunks[i].Replace("\"", string.Empty);
			}
			res.Artist = chunks[4];
			res.SongName = chunks[3];
			res.DownloadLink = chunks[2];
			return res;
		}
	}
}