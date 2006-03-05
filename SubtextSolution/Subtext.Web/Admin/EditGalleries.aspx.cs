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
using System.Web.UI;
using System.Web.UI.WebControls;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;

namespace Subtext.Web.Admin.Pages
{
	public class EditGalleries : AdminPage
	{
		protected bool _isListHidden;

		protected Subtext.Web.Admin.WebUI.Page PageContainer;
		protected Subtext.Web.Admin.WebUI.AdvancedPanel Results;
		protected System.Web.UI.WebControls.Repeater rprImages;
		protected System.Web.UI.WebControls.DataGrid dgrSelectionList;
		protected System.Web.UI.WebControls.TextBox txbNewTitle;
		protected System.Web.UI.WebControls.CheckBox ckbNewIsActive;
		protected System.Web.UI.WebControls.Button lkbPost;
		protected Subtext.Web.Admin.WebUI.AdvancedPanel Add;
		protected System.Web.UI.WebControls.TextBox txbImageTitle;
		protected System.Web.UI.WebControls.Button lbkAddImage;
		protected Subtext.Web.Admin.WebUI.AdvancedPanel AddImages;
		protected System.Web.UI.WebControls.Panel ImagesDiv;
		protected System.Web.UI.HtmlControls.HtmlInputFile ImageFile;
		protected System.Web.UI.WebControls.CheckBox ckbIsActiveImage;
		protected Subtext.Web.Admin.WebUI.MessagePanel Messages;
		protected System.Web.UI.WebControls.PlaceHolder plhImageHeader;
		protected Subtext.Web.Controls.ScrollPositionSaver scrollsaver;
		protected System.Web.UI.WebControls.TextBox txbNewDescription;

		#region Accessors
		private int CategoryID
		{
			get
			{
				if (null != ViewState["CategoryID"])
					return (int)ViewState["CategoryID"];
				else
					return NullValue.NullInt32;
			}
			set { ViewState["CategoryID"] = value; }
		}
		#endregion

		private void Page_Load(object sender, System.EventArgs e)
		{
			if(!Config.Settings.AllowImages)
			{
				Response.Redirect("index.aspx");
			}

			if (!IsPostBack)
			{
				HideImages();
				ShowResults(false);	
				BindList();
				ckbIsActiveImage.Checked = Preferences.AlwaysCreateIsActive;
				ckbNewIsActive.Checked = Preferences.AlwaysCreateIsActive;

				if (null != Request.QueryString[Keys.QRYSTR_CATEGORYID])
				{
					CategoryID = Convert.ToInt32(Request.QueryString[Keys.QRYSTR_CATEGORYID]);
					BindGallery(CategoryID);
				}					
			}			
		}

		private void BindList()
		{
			// TODO: possibly, later on, add paging support a la other cat editors
			LinkCategoryCollection selectionList = Links.GetCategories(CategoryType.ImageCollection, false);

			if (selectionList.Count > 0)
			{
				dgrSelectionList.DataSource = selectionList;
				dgrSelectionList.DataKeyField = "CategoryID";
				dgrSelectionList.DataBind();
			}
			else
			{
				// TODO: no existing items handling. add label and indicate no existing items. pop open edit.
			}
		}

		private void BindGallery()
		{
			// HACK: reverse the call order with the overloaded version
			BindGallery(CategoryID);
		}

		private void BindGallery(int galleryID)
		{
			CategoryID = galleryID;
			LinkCategory selectedGallery = Links.GetLinkCategory(galleryID,false);
			ImageCollection imageList = Images.GetImagesByCategoryID(galleryID, false);

			plhImageHeader.Controls.Clear();
			string galleryTitle = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} - {1} images", selectedGallery.Title, imageList.Count);
			plhImageHeader.Controls.Add(new LiteralControl(galleryTitle));

			rprImages.DataSource = imageList;
			rprImages.DataBind();

			ShowImages();

			Control container = Page.FindControl("PageContainer");
			if (null != container && container is Subtext.Web.Admin.WebUI.Page)
			{	
				Subtext.Web.Admin.WebUI.Page page = (Subtext.Web.Admin.WebUI.Page)container;
				string title = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Viewing Gallery \"{0}\"", selectedGallery.Title);

				page.BreadCrumbs.AddLastItem(title);
				page.Title = title;
			}

			AddImages.Collapsed = !Preferences.AlwaysExpandAdvanced;
		}

		private void ShowResults(bool collapsible)
		{
			Results.Collapsible = collapsible;
			Results.Collapsed = false;
		}

		private void HideResults()
		{
			Results.Collapsible = true;
			Results.Collapsed = true;
		}

		private void ShowImages()
		{
			HideResults();			
			ImagesDiv.Visible = true;
		}

		private void HideImages()
		{
			ShowResults(false);
			ImagesDiv.Visible = false;
		}

		protected string EvalImageUrl(object imageObject)
		{
			if (imageObject is Subtext.Framework.Components.Image)
			{
				Subtext.Framework.Components.Image image = (Subtext.Framework.Components.Image)imageObject;
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}", Images.HttpGalleryFilePath(Context, image.CategoryID), 
					image.ThumbNailFile);
			}
			else
				return String.Empty;
		}

		protected string EvalImageNavigateUrl(object imageObject)
		{
			if (imageObject is Subtext.Framework.Components.Image)
			{
				Subtext.Framework.Components.Image image = (Subtext.Framework.Components.Image)imageObject;
				return Subtext.Framework.Configuration.Config.CurrentBlog.UrlFormats.ImageUrl(null,image.ImageID);
			}
			else
				return String.Empty;
		}

		protected string EvalImageTitle(object imageObject)
		{
			const int TARGET_HEIGHT = 138;
			const int MAX_IMAGE_HEIGHT = 120;
			const int CHAR_PER_LINE = 19;
			const int LINE_HEIGHT_PIXELS = 16;

			if (imageObject is Subtext.Framework.Components.Image)
			{
				Subtext.Framework.Components.Image image = (Subtext.Framework.Components.Image)imageObject;

				// do a rough calculation of how many chars we can shoehorn into the title space
				// we have to back into an estimated thumbnail height right now with aspect * max
				double aspectRatio = (double)image.Height / image.Width;
				if (aspectRatio > 1 || aspectRatio <= 0)
					aspectRatio = 1;
				int allowedChars = (int)((TARGET_HEIGHT - MAX_IMAGE_HEIGHT * aspectRatio) 
					/ LINE_HEIGHT_PIXELS * CHAR_PER_LINE);

				return Utilities.Truncate(image.Title, allowedChars);
			}
			else
				return String.Empty;
		}

		// REFACTOR: duplicate from category editor; generalize a la EntryEditor
		private void PersistCategory(LinkCategory category)
		{
			try
			{
				if (category.CategoryID > 0)
				{
					Links.UpdateLinkCategory(category);
					Messages.ShowMessage(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Category \"{0}\" was updated.", category.Title));
				}
				else
				{
					category.CategoryID = Links.CreateLinkCategory(category);
					Messages.ShowMessage(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Category \"{0}\" was added.", category.Title));
				}					
			}
			catch(Exception ex)
			{
				Messages.ShowError(String.Format(Constants.RES_EXCEPTION, "TODO...", ex.Message));
			}
		}

		private void PersistImage()
		{
			if (Page.IsValid)
			{
				Subtext.Framework.Components.Image image = new Subtext.Framework.Components.Image();
				image.CategoryID = CategoryID;
				image.Title = txbImageTitle.Text;
				image.IsActive = ckbIsActiveImage.Checked;
				
				try
				{
					image.File = Images.GetFileName(ImageFile.PostedFile.FileName);
					image.LocalFilePath = Images.LocalGalleryFilePath(Context, CategoryID);
					int imageID = Images.InsertImage(image,Images.GetFileStream(ImageFile.PostedFile));				
					if (imageID > 0)
					{
						this.Messages.ShowMessage("The image was successfully added to the gallery.");
						txbImageTitle.Text = String.Empty;
					}
					else
						this.Messages.ShowError(Constants.RES_FAILUREEDIT + " There was a baseline problem posting your entry.");
				}
				catch(Exception ex)
				{
					this.Messages.ShowError(String.Format(Constants.RES_EXCEPTION, "TODO...", ex.Message));
				}
			}
		}

		// REFACTOR: can the flag go in AdminPage along with this meth?
		public string CheckHiddenStyle()
		{
			if (_isListHidden)
				return Constants.CSSSTYLE_HIDDEN;
			else
				return String.Empty;
		}

		private void ConfirmDeleteGallery(int categoryID, string categoryTitle)
		{
			this.Command = new DeleteGalleryCommand(categoryID, categoryTitle);
			this.Command.RedirectUrl = Request.Url.ToString();
			Server.Transfer(Constants.URL_CONFIRM);
		}

		private void ConfirmDeleteImage(int imageID)
		{
			this.Command = new DeleteImageCommand(imageID);
			this.Command.RedirectUrl = Request.Url.ToString();
			Server.Transfer(Constants.URL_CONFIRM);
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
			this.dgrSelectionList.ItemCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.dgrSelectionList_ItemCommand);
			this.dgrSelectionList.CancelCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.dgrSelectionList_CancelCommand);
			this.dgrSelectionList.EditCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.dgrSelectionList_EditCommand);
			this.dgrSelectionList.UpdateCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.dgrSelectionList_UpdateCommand);
			this.dgrSelectionList.DeleteCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.dgrSelectionList_DeleteCommand);
			this.lkbPost.Click += new System.EventHandler(this.lkbPost_Click);
			this.lbkAddImage.Click += new System.EventHandler(this.lbkAddImage_Click);
			this.rprImages.ItemCommand += new System.Web.UI.WebControls.RepeaterCommandEventHandler(this.rprImages_ItemCommand);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void dgrSelectionList_ItemCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			switch (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture)) 
			{
				case "view" :
					int galleryID = Convert.ToInt32(e.CommandArgument);
					BindGallery(galleryID);
					break;
				default:
					break;
			}		
		}

		private void dgrSelectionList_EditCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			HideImages();
			dgrSelectionList.EditItemIndex = e.Item.ItemIndex;
			BindList();
			this.Messages.Clear();
		}

		private void dgrSelectionList_UpdateCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			TextBox title = e.Item.FindControl("txbTitle") as TextBox;
			TextBox desc = e.Item.FindControl("txbDescription") as TextBox;

			CheckBox isActive = e.Item.FindControl("ckbIsActive") as CheckBox;

			if(Page.IsValid && null != title && null != isActive)
			{
				int id = Convert.ToInt32(dgrSelectionList.DataKeys[e.Item.ItemIndex]);
				
				LinkCategory existingCategory = Links.GetLinkCategory(id,false);
				existingCategory.Title = title.Text;
				existingCategory.IsActive = isActive.Checked;
				existingCategory.Description = desc.Text;
		
				if (id != 0) 
					PersistCategory(existingCategory);

				dgrSelectionList.EditItemIndex = -1;
				BindList();
			}		
		}

		private void dgrSelectionList_DeleteCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			int id = Convert.ToInt32(dgrSelectionList.DataKeys[e.Item.ItemIndex]);
			LinkCategory lc = Links.GetLinkCategory(id,false);
			ConfirmDeleteGallery(id, lc.Title);		
		}

		private void dgrSelectionList_CancelCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			dgrSelectionList.EditItemIndex = -1;			
			BindList();
			Messages.Clear();
		}

		private void lkbPost_Click(object sender, System.EventArgs e)
		{
			LinkCategory newCategory = new LinkCategory();
			newCategory.CategoryType = CategoryType.ImageCollection;
			newCategory.Title = txbNewTitle.Text;
			newCategory.IsActive = ckbNewIsActive.Checked;
			newCategory.Description = txbNewDescription.Text;
			PersistCategory(newCategory);

			BindList();	
			txbNewTitle.Text = String.Empty;
			ckbNewIsActive.Checked = Preferences.AlwaysCreateIsActive;
		}

		private void lbkAddImage_Click(object sender, System.EventArgs e)
		{
			PersistImage();
			BindGallery();
		}

		private void rprImages_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
		{
			switch (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture)) 
			{
				case "deleteimage" :
					ConfirmDeleteImage(Convert.ToInt32(e.CommandArgument));
					break;
				default:
					break;
			}			
		}
	}
}
