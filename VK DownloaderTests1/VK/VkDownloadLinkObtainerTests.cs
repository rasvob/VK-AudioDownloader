using Microsoft.VisualStudio.TestTools.UnitTesting;
using VK_Downloader.VK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_Downloader.ViewModels;

namespace VK_Downloader.VK.Tests
{
	[TestClass()]
	public class VkDownloadLinkObtainerTests
	{
		[TestMethod()]
		public void ParseContentTest(string content = "<!--687144220453<!><!>3<!>3629<!>0<!><!json>[[\"456239532\",\"2000125292\",\"https:\\/\\/cs9-2v4.vk.me\\/p10\\/e59eb5f4b37763.mp3?extra=9V83CF775b8KkE1NAqyS-D7MvilrRxs--aebuM41k5V-f1OzNTIGJeCCq-3NwRT6ROcV4HqCtrKSsKZrUAUv_VvkA0VWDq8l38z1VdouA4g06ZC0ncfKZyxrhQbQ9gVCFJ6rtX29taWj2Nbdaw\",\"Amsterdam\",\"Peter Bjorn And John\",217,2,0,\"\",0,9,\"\",\"[]\",\"45ae54bd01a3800e99\"]]<!><!json>{\"375000324\":1}")
		{
			var res = new SongViewModel();
			var idxStart = content.IndexOf("[[", StringComparison.Ordinal);
			var idxEnd = content.LastIndexOf("]]", StringComparison.Ordinal);

			content = content.Substring(idxStart, idxEnd - idxStart);
		}
	}
}