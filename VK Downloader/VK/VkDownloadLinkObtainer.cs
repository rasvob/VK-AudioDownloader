using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VK_Downloader.VK
{
	public class VkDownloadLinkObtainer
	{
		public string Ids { get; set; }
		private readonly string _al = "1";
		private readonly string _act = "reload_audio";
		private readonly string _baseUrl = @"https://new.vk.com/al_audio.php";
		public string StId { get; set; }

		public VkDownloadLinkObtainer(string ids)
		{
			Ids = ids;
		}

		public async Task<string> ObtainDownloadLink()
		{
			var res = await MakeRequestForLink();

			return string.Empty;
		}

		private async Task<string> MakeRequestForLink()
		{
			//using(WebClient client = new WebClient())
			//{
			//	var reqParams = new NameValueCollection();
			//	reqParams.Add("act", _act);
			//	reqParams.Add("al", _al);
			//	reqParams.Add("ids", Ids);
			//	byte[] response = await client.UploadValuesTaskAsync(new Uri(_baseUrl), "POST", reqParams);
			//	string resBody = Encoding.ASCII.GetString(response);
			//	return resBody;
			//}
			
			using(var handler = new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer() })
			{
				using(var client = new HttpClient(handler) { BaseAddress = new Uri(@"https://new.vk.com/") })
				{
					string stid = string.Empty;
					var res = await client.GetAsync(new Uri(@"https://new.vk.com/"));
					var resString = res.Content.ReadAsStringAsync();
					Cookie o = handler.CookieContainer.GetCookies(new Uri(@"https://new.vk.com/"))["remixsid"];
					foreach (Cookie cookie in handler.CookieContainer.GetCookies(new Uri(@"https://new.vk.com/")))
					{
						Trace.WriteLine($"{cookie.Name} - {cookie.Value}");
					}
					if (o != null)
						stid = o.Value;
					Trace.WriteLine(stid);
					StId = stid;
				}
			}


			var formData = new Dictionary<string, string>();
			formData.Add("act", _act);
			formData.Add("al", _al);
			formData.Add("ids", Ids);

			var content = new FormUrlEncodedContent(formData);

			using(var handler = new HttpClientHandler() { UseCookies = false })
			{
				using(var client = new HttpClient(handler) { BaseAddress = new Uri(@"https://new.vk.com/") })
				{
					var message = new HttpRequestMessage(HttpMethod.Post, "/al_audio.php");
					StId = "2fc40b95ca14daa4ee29a38e368269cefb14b66a9dfbc9409363a";
					message.Headers.Add("Cookie", $"remixsid={StId}");
					message.Content = content;
					var res = await client.SendAsync(message);
					var resString = res.Content.ReadAsStringAsync();
				}
			}

			//using(var handler = new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer()})
			//{
			//	using(var client = new HttpClient(handler) { BaseAddress = new Uri(@"https://new.vk.com/") })
			//	{
			//		var message = new HttpRequestMessage(HttpMethod.Post, "/al_audio.php");
			//		message.Content = content;
			//		var res = await client.SendAsync(message);
			//		var resString = res.Content.ReadAsStringAsync();
			//	}
			//}

			return string.Empty;
		}
	}
}