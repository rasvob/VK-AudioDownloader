using System.IO;
using NReco.PhantomJS;

namespace VK_Downloader.FileDownload
{
	public static class PhantomDownloader
	{
		public static string DownloadPage(string link)
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
					using(StreamReader reader = new StreamReader(outStream))
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
	}
}