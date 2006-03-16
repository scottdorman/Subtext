using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using MbUnit.Framework;
using Subtext.Common.Syndication;
using Subtext.Framework;
using Subtext.Framework.Configuration;

namespace UnitTests.Subtext.Framework.Syndication
{
	/// <summary>
	/// Tests of the RssHandler http handler class.
	/// </summary>
	[TestFixture]
	public class RssHandlerTests
	{
		/// <summary>
		/// Tests that a simple regular RSS feed works.
		/// </summary>
		[Test]
		[RollBack]
		public void TestSimpleRegularFeedWorks()
		{
			string hostName = System.Guid.NewGuid().ToString().Replace("-", "") + ".com";
			StringBuilder sb = new StringBuilder();
			TextWriter output = new StringWriter(sb);
			UnitTestHelper.SetHttpContextWithBlogRequest(hostName, "", "", "", output);
			Assert.IsTrue(Config.CreateBlog("", "username", "password", hostName, string.Empty));

			Entries.Create(UnitTestHelper.CreateEntryInstanceForSyndication("Haacked", "Body Rocking", "Title Test"));
			Entries.Create(UnitTestHelper.CreateEntryInstanceForSyndication("Haacked", "Body Rocking Pt 2", "Title Test 2"));

			RssHandler handler = new RssHandler();
			handler.ProcessRequest(HttpContext.Current);
			HttpContext.Current.Response.Flush();
	
			string rssOutput = sb.ToString();		

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(rssOutput);
			XmlNodeList itemNodes = doc.SelectNodes("/rss/channel/item");
			Assert.AreEqual(2, itemNodes.Count, "expected two item nodes.");

			Assert.AreEqual("Title Test", itemNodes[0].SelectSingleNode("title").InnerText, "Not what we expected for the first title.");
			Assert.AreEqual("Title Test 2", itemNodes[1].SelectSingleNode("title").InnerText, "Not what we expected for the second title.");

			Assert.AreEqual("Body Rocking", itemNodes[0].SelectSingleNode("description").InnerText.Substring(0, "Body Rocking".Length), "Not what we expected for the first body.");
			Assert.AreEqual("Body Rocking Pt 2", itemNodes[1].SelectSingleNode("description").InnerText.Substring(0, "Body Rocking pt 2".Length), "Not what we expected for the second body.");
		}

		/// <summary>
		/// Tests that sending a Gzip compressed RSS Feed sends the feed 
		/// properly compressed.  USed the RSS Bandit decompress code 
		/// to decompress the feed and test it.
		/// </summary>
		[Test]
		[RollBack]
		public void TestCompressedFeedWorks()
		{
			string hostName = System.Guid.NewGuid().ToString().Replace("-", "") + ".com";
			StringBuilder sb = new StringBuilder();
			TextWriter output = new StringWriter(sb);

			SimulatedHttpRequest workerRequest = UnitTestHelper.SetHttpContextWithBlogRequest(hostName, "", "", "", output);
			workerRequest.Headers.Add("Accept-Encoding", "gzip");
			Assert.IsTrue(Config.CreateBlog("", "username", "password", hostName, string.Empty));
			Config.CurrentBlog.UseSyndicationCompression = true;

			Entries.Create(UnitTestHelper.CreateEntryInstanceForSyndication("Haacked", "Body Rocking", "Title Test"));
			Entries.Create(UnitTestHelper.CreateEntryInstanceForSyndication("Haacked", "Body Rocking Pt 2", "Title Test 2"));

			RssHandler handler = new RssHandler();
			Assert.IsNotNull(HttpContext.Current.Request.Headers, "Headers collection is null! Not Good.");
			handler.ProcessRequest(HttpContext.Current);
			
			//I'm cheating here!
			MethodInfo method = typeof(HttpResponse).GetMethod("FilterOutput", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(HttpContext.Current.Response, new object[] {});
			HttpContext.Current.Response.Flush();
			
			MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(sb.ToString()));
			Stream deflated = UnitTestHelper.GetDeflatedResponse("gzip", stream);
			string rssOutput = string.Empty;
			using(StreamReader reader = new StreamReader(deflated))
			{
				rssOutput = reader.ReadToEnd();
			}
			
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(rssOutput);
			XmlNodeList itemNodes = doc.SelectNodes("/rss/channel/item");
			Assert.AreEqual(2, itemNodes.Count, "expected two item nodes.");

			Assert.AreEqual("Title Test", itemNodes[0].SelectSingleNode("title").InnerText, "Not what we expected for the first title.");
			Assert.AreEqual("Title Test 2", itemNodes[1].SelectSingleNode("title").InnerText, "Not what we expected for the second title.");

			Assert.AreEqual("Body Rocking", itemNodes[0].SelectSingleNode("description").InnerText.Substring(0, "Body Rocking".Length), "Not what we expected for the first body.");
			Assert.AreEqual("Body Rocking Pt 2", itemNodes[1].SelectSingleNode("description").InnerText.Substring(0, "Body Rocking pt 2".Length), "Not what we expected for the second body.");
			
		}
	}
}