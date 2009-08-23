﻿#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using Subtext.Framework;
using Subtext.Web.UI.Controls;

namespace Subtext.Web.UI.Controls
{
    public class AggregateUserControl : BaseControl
    {
        protected int? GetGroupIdFromQueryString()
        {
            return Page.GetGroupIdFromQueryString();
        }

        protected string BlogUrl(Blog blog)
        {
            return Url.BlogUrl(blog);
        }

        protected new AggregatePage Page
        {
            get
            {
                return base.Page as AggregatePage;
            }
        }

        public T Get<T>(object item) where T : class
        {
            return item as T;
        }
    }
}
