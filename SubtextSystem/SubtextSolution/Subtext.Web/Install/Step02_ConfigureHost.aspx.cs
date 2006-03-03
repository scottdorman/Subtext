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
using Subtext.Framework;
using Subtext.Framework.Configuration;

namespace Subtext.Web.Install
{
	/// <summary>
	/// Page used to create an initial configuration for the blog.
	/// </summary>
	/// <remarks>
	/// This page will ONLY be displayed if there are no 
	/// blog configurations within the database.
	/// </remarks>
	public class Step02_ConfigureHost : InstallationBase
	{
		protected System.Web.UI.WebControls.TextBox txtUserName;
		protected System.Web.UI.WebControls.TextBox txtPassword;
		protected System.Web.UI.WebControls.TextBox txtConfirmPassword;
		protected System.Web.UI.WebControls.ValidationSummary vldSummary;
		protected System.Web.UI.WebControls.RequiredFieldValidator vldUsernameRequired;
		protected System.Web.UI.WebControls.RequiredFieldValidator vldPasswordRequired;
		protected System.Web.UI.WebControls.RequiredFieldValidator vldConfirmPasswordRequired;
		protected System.Web.UI.WebControls.CompareValidator vldComparePasswords;
		protected System.Web.UI.WebControls.Button btnSave;
		protected System.Web.UI.WebControls.Literal ltlMessage;
		protected Subtext.Web.Controls.ContentRegion MPTitle;
		protected Subtext.Web.Controls.ContentRegion MPSubTitle;
		protected Subtext.Web.Controls.ContentRegion Content;
		protected Subtext.Web.Controls.MasterPage MPContainer;
		protected System.Web.UI.HtmlControls.HtmlTable tblConfigForm;
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			//We need to make sure that the form is ONLY displayed 
			//if there really is no Host record.
			tblConfigForm.Visible = true;
			if(Config.BlogCount == 0)
			{
				ltlMessage.Text = 
					"<p>" 
					+ "At this point, you may <strong>remove the database owner rights</strong> from " 
					+ "the database user account used to connect to the database.</p><p>"
					+ "In this step, you will set up a Host Administrative account. "
					+ "This account has permission to create and manage blogs. "
					+ "</p>"
					+ "<p>" 
					+ "To get you started quickly, just specify a username and password "
					+ "for the special Host Administrator account. " 
					+ "</p>";
			}
			else
			{
				ltlMessage.Text = 
					"<p>" 
					+ "Welcome!  It appears that you have existing blogs, but no Host Admin account set up. "
					+ "We can remedy that situation quickly."
					+ "</p>"
					+ "<p>" 
					+ "Just specify a username and password "
					+ "for the special Host Administrator account. " 
					+ "This account can create blogs in this system. "
					+ "</p>";
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void btnSave_Click(object sender, System.EventArgs e)
		{
			if(Page.IsValid)
			{
				string userName = txtUserName.Text;
				string password = txtPassword.Text;
				
				// Create the HostInfo record.
				if(HostInfo.CreateHost(userName, password))
				{
					if(Config.BlogCount == 0)
					{
						System.Web.Security.FormsAuthentication.SetAuthCookie("HostAdmin", false);
						Response.Redirect(InstallationBase.NextStepUrl);
					}
					else
					{
						InstallationManager.ResetInstallationStatusCache();
						Response.Redirect("InstallationComplete.aspx");
					}
				}
				else
				{
					string errorMessage = "I'm sorry, but we had a problem creating your initial "
						+ "configuration. Please <a href=\"http://sourceforge.net/tracker/?group_id=137896&atid=739979\">report "
						+ "this issue</a> to the Subtext team.";
					
					//TODO: Pick a non-generic exception.
					throw new Exception(errorMessage);
				}
			}
		}
	}
}
