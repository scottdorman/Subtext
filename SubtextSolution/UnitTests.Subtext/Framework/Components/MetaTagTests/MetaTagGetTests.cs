#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at SourceForge at http://sourceforge.net/projects/subtext
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MbUnit.Framework;
using Microsoft.ApplicationBlocks.Data;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Data;

namespace UnitTests.Subtext.Framework.Components.MetaTagTests
{
    [TestFixture]
    class MetaTagGetTests
    {
        private BlogInfo blog;

        [Test]
        [RollBack2]
        public void GetReturnsZeroWhenNoMetaTagsExistForBlog()
        {
            Assert.AreEqual(0, MetaTags.GetMetaTagsForBlog(blog).Count, "Shouldn't have found any MetaTags for this blog.");
        }

        [Test]
        [RollBack2]
        public void GetReturnsZeroWhenNoMetaTagsExistForEntry()
        {
            Entry e =
                UnitTestHelper.CreateEntryInstanceForSyndication("Steve Harman", "Loves Subtexting!", "Roses are red...");
            Entries.Create(e);

            Assert.AreEqual(0, MetaTags.GetMetaTagsForEntry(e).Count, "Shouldn't have found any MetaTags for this entry.");
        }

        [Test]
        [RollBack2]
        public void CanGetMetaTagsForBlog()
        {
            InsertNewMetaTag("Adding description meta tag", "description", null, DateTime.Now, blog.Id, null);
            InsertNewMetaTag("no-cache", null, "cache-control", DateTime.Now, blog.Id, null);

            ICollection<MetaTag> tags = MetaTags.GetMetaTagsForBlog(blog);

            Assert.AreEqual(2, tags.Count, "Should be two tags for this blog.");
        }

        [Test]
        [RollBack2]
        public void CanGetMetaTagsForEntry()
        {
            Entry e = UnitTestHelper.CreateEntryInstanceForSyndication("Steve-o", "Bar", "Steve is still rockin it... or is he?");
            Entries.Create(e);

            InsertNewMetaTag("Adding description meta tag", "description", null, DateTime.Now, blog.Id, null);
            InsertNewMetaTag("no-cache", null, "cache-control", DateTime.Now, blog.Id, null);

            // insert a few entry specific tags
            InsertNewMetaTag("Yet Another MetaTag", "author", null, DateTime.Now, blog.Id, e.Id);
            InsertNewMetaTag("One more for good measure", "description", null, DateTime.Now, blog.Id, e.Id);
            InsertNewMetaTag("no-cache", null, "cache-control", DateTime.Now, blog.Id, e.Id);
            InsertNewMetaTag("Mon, 22 Jul 2022 11:12:01 GMT", null, "expires", DateTime.Now, blog.Id, e.Id);

            ICollection<MetaTag> tags = MetaTags.GetMetaTagsForEntry(e);

            Assert.AreEqual(4, tags.Count, "Should have found 4 MetaTags for this entry.");
        }

        [SetUp]
        public void Setup()
        {
            this.blog = UnitTestHelper.CreateBlogAndSetupContext();
        }

        #region Some helper code to populate the db w/metatags

        private static readonly string insertSql = @"INSERT INTO subtext_MetaTag ([Content], [Name], HttpEquiv, DateCreated, BlogId, EntryId) VALUES (@Content, @Name, @HttpEquiv, @DateCreated, @BlogId, @EntryId)";


        private void InsertNewMetaTag(string content, string nameValue, string httpEquivValue, DateTime created, int blogId, int? entryId)
        {
            object entryIdValue = entryId.HasValue ? DataHelper.CheckNull(entryId.Value) : DBNull.Value;

            SqlParameter[] p = 
                {
                    DataHelper.MakeInParam("@Content", content),
                    DataHelper.MakeInParam("@Name", DataHelper.CheckNull(nameValue)),
                    DataHelper.MakeInParam("@HttpEquiv", DataHelper.CheckNull(httpEquivValue)),
                    DataHelper.MakeInParam("@DateCreated", created),
                    DataHelper.MakeInParam("@BlogId", blogId),
                    DataHelper.MakeInParam("@EntryId", SqlDbType.Int, 4, entryIdValue)
                };
            SqlHelper.ExecuteNonQuery(Config.ConnectionString, CommandType.Text, insertSql, p);
        }

        #endregion
        
    }
}