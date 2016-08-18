using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using RestSharp;
using RestSharp.Extensions.MonoHttp;
using VK_Downloader.Configuration;
using VK_Downloader.FileDownload;
using VK_Downloader.VK.Auth;

namespace VK_Downloader.VK
{
	public class VkAuthProvider
	{
		public string Token { get; set; }
		public bool IsLoggedIn { get; set; } = false;

		private readonly string _loginLink = @"https://vk.com/login";
		private readonly string _postReqBaseAddress = @"https://login.vk.com";
		private readonly string _loginPhp = @"https://vk.com/login.php";

		public event EventHandler LoggedIn;
		public event EventHandler LoggedOut;

		public async Task<string> Login(string email, string password)
		{
			var stageOne = await GetStageOneFormData();
			stageOne.Email = HttpUtility.UrlEncode(email);
			stageOne.Password = HttpUtility.UrlEncode(password);
			var stageTwo = await GetStageTwoData(stageOne);
			Token = await GetAuthToken(stageTwo);
			IsLoggedIn = true;
			SaveAuthToken();
			OnLoggedIn();
			return Token;
		}

		private void SaveAuthToken()
		{
			ConfigurationRepository.SaveAuthToken(Token);
		}

		public void LoadSavedToken()
		{
			Token = ConfigurationRepository.LoadAuthToken();
			if (!Token.Equals(string.Empty))
			{
				IsLoggedIn = true;
				OnLoggedIn();
			}
		}

		private async Task<StageOneData> GetStageOneFormData()
		{
			var data = new StageOneData();

			var client = new RestClient(_loginLink);
			client.CookieContainer = new CookieContainer();

			var req = new RestRequest(Method.GET);
			var res = await client.ExecuteTaskAsync(req);
			data.RemixLhk = res.Cookies.LastOrDefault(t => t.Name.ToLower().Equals("remixlhk"))?.Value;

			var parser = new HtmlParser();
			var document = parser.Parse(res.Content);

			var form = document.QuerySelector("form#quick_login_form");
			IHtmlInputElement lgh = form.QuerySelector("input[name='lg_h']") as IHtmlInputElement;
			IHtmlInputElement iph = form.QuerySelector("input[name='ip_h']") as IHtmlInputElement;

			data.LgH = lgh.Value;
			data.IpH = iph.Value;

			return data;
		}


		private async Task<StageTwoData> GetStageTwoData(StageOneData data)
		{
			var uri = new Uri(_postReqBaseAddress);
			var client = new RestClient(uri);
			client.FollowRedirects = false;

			var cookies = new CookieContainer();
			client.CookieContainer = cookies;

			client.CookieContainer.Add(uri, new Cookie("remixlhk", data.RemixLhk));
			client.CookieContainer.Add(uri, new Cookie("remixlang", "3"));

			var request = new RestRequest(Method.POST);
			request.AddHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
			request.AddHeader("accept-encoding", "gzip, deflate, br");
			request.AddHeader("accept-language", "cs,en;q=0.8,sk;q=0.6,pl;q=0.4");
			request.AddHeader("referer", "https://new.vk.com/login");
			request.AddHeader("origin", "https://new.vk.com");
			request.AddHeader("host", "login.vk.com");
			request.AddHeader("dnt", "1");
			request.AddHeader("upgrade-insecure-requests", "1");
			request.AddHeader("content-type", "application/x-www-form-urlencoded");
			request.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
			request.AddQueryParameter("act", "login");
			request.AddParameter("application/x-www-form-urlencoded", $"act=login&role=al_frame&_origin=https%3A%2F%2Fnew.vk.com&ip_h={data.IpH}&lg_h={data.LgH}&email={data.Email}&pass={data.Password}", ParameterType.RequestBody);
			
			var res = await client.ExecuteTaskAsync(request);

			var stageTwoData = new StageTwoData
			{
				RemixQName = res.Cookies.LastOrDefault(t => t.Name.ToLower().StartsWith("remixq"))?.Name,
				RemixQValue = res.Cookies.LastOrDefault(t => t.Name.ToLower().StartsWith("remixq"))?.Value,
				RemixLkh = data.RemixLhk
			};

			stageTwoData.QHash = stageTwoData.ParseQHash(stageTwoData.RemixQName);

			return stageTwoData;
		}

		private async Task<string> GetAuthToken(StageTwoData data)
		{
			var uri = new Uri(_loginPhp);
			var client = new RestClient(uri);
			client.FollowRedirects = false;
			client.CookieContainer = new CookieContainer();

			client.CookieContainer.Add(uri, new Cookie("remixlhk", data.RemixLkh));
			client.CookieContainer.Add(uri, new Cookie(data.RemixQName, data.RemixQValue));

			var req = new RestRequest(Method.GET);
			req.AddQueryParameter("act", "slogin");
			req.AddQueryParameter("to", string.Empty);
			req.AddQueryParameter("s", "1");
			req.AddQueryParameter("__q_hash", data.QHash);

			var res = await client.ExecuteTaskAsync(req);

			return res.Cookies.FirstOrDefault(t => t.Name.Equals("remixsid", StringComparison.CurrentCultureIgnoreCase))?.Value;
		}

		public void Logout()
		{
			IsLoggedIn = false;
			Token = string.Empty;
			ConfigurationRepository.SaveAuthToken(string.Empty);
			OnLoggedOut();
		}

		protected virtual void OnLoggedIn()
		{
			LoggedIn?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnLoggedOut()
		{
			LoggedOut?.Invoke(this, EventArgs.Empty);
		}
	}
}