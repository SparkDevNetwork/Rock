using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.blueboxmoon.Rockumentation;
using com.blueboxmoon.Rockumentation.Cache;
using com.blueboxmoon.Rockumentation.Enums;
using com.blueboxmoon.Rockumentation.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.Rockumentation
{
    /// <summary>
    /// Views a documentation book.
    /// </summary>
    [DisplayName( "Book View" )]
    [Category( "Blue Box Moon > Rockumentation" )]
    [Description( "Views a documentation book." )]

    #region Block Attributes

    [BooleanField( "Set Page Title",
        Description = "If enabled, then the browser page title will be updated to match the displayed article.",
        IsRequired = true,
        DefaultValue = "True",
        Order = 0,
        Key = AttributeKey.SetPageTitle )]

    [CodeEditorField( "Article Header Template", "Specifies the lava template to render before each the article.", CodeEditorMode.Lava,
        IsRequired = false,
        DefaultValue = "<h1>{{ Article.Title }}</h1>",
        Order = 1,
        Key = AttributeKey.HeaderTemplate )]

    [CodeEditorField( "Article Footer Template", "Specifies the lava template to render after each article.", CodeEditorMode.Lava,
        IsRequired = false,
        DefaultValue = "",
        Order = 2,
        Key = AttributeKey.FooterTemplate )]

    // *** Advanced Attributes ***

    [BooleanField( "Show Panel",
        Description = "Wraps the entire article content in a panel with the book name in the title.",
        ControlType = Rock.Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 0,
        Key = AttributeKey.ShowPanel,
        Category = "advanced" )]

    [CodeEditorField( "Page Header Template", "Specifies the lava template to render before the page content.", CodeEditorMode.Lava,
        IsRequired = false,
        DefaultValue = "",
        Order = 1,
        Key = AttributeKey.PageHeaderTemplate,
        Category = "advanced" )]

    [CodeEditorField( "Page Footer Template", "Specifies the lava template to render after the page content.", CodeEditorMode.Lava,
        IsRequired = false,
        DefaultValue = "",
        Order = 2,
        Key = AttributeKey.PageFooterTemplate,
        Category = "advanced" )]

    #endregion

    public partial class BookView : RockBlock
    {
        #region Block Attributes

        private static class AttributeKey
        {
            public const string SetPageTitle = "SetPageTitle";

            public const string HeaderTemplate = "HeaderTemplate";

            public const string FooterTemplate = "FooterTemplate";

            public const string ShowPanel = "ShowPanel";

            public const string PageHeaderTemplate = "PageHeaderTemplate";

            public const string PageFooterTemplate = "PageFooterTemplate";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current article identifier.
        /// </summary>
        /// <value>
        /// The current article identifier.
        /// </value>
        protected int? CurrentArticleId
        {
            get
            {
                return ( int? ) ViewState["CurrentArticleId"];
            }
            private set
            {
                ViewState["CurrentArticleId"] = value;
            }
        }

        /// <summary>
        /// Gets the current article.
        /// </summary>
        /// <value>
        /// The current article.
        /// </value>
        protected DocumentationArticleCache CurrentArticle
        {
            get
            {
                if ( CurrentArticleId.HasValue )
                {
                    return DocumentationArticleCache.Get( CurrentArticleId.Value );
                }

                var article = DocumentationArticleCache.GetArticleFromSlug( PageParameter( "Book" ), PageParameter( "Slug" ), PageParameter( "Version" ) );

                if ( article != null )
                {
                    CurrentArticleId = article.Id;
                }

                return article;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/clipboard.js/clipboard.min.js" );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/Rockumentation/Styles/bootstrap-markdown-editor.css" );
            RockPage.AddScriptLink( "~/Scripts/ace/ace.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/Rockumentation/Scripts/bootstrap-markdown-editor.js" );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/Rockumentation/Styles/bootstrap-toc.css" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/Rockumentation/Scripts/bootstrap-toc.js" );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/Rockumentation/Styles/prism.min.css" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/Rockumentation/Scripts/prism.min.js" );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/Rockumentation/Styles/rockumentation.css" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/Rockumentation/Scripts/rockumentation.js" );

            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/Rockumentation/Scripts/editor.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/Rockumentation/Scripts/editor-tools.js" );

            var article = CurrentArticle;

            bool canEdit = false;

            if ( article != null )
            {
                canEdit = article.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            }

            gArticle.DataKeyNames = new[] { "Id" };
            gArticle.Actions.AddClick += gArticle_AddClick;
            gArticle.Actions.ShowAdd = canEdit;
            gArticle.IsDeleteEnabled = true;
            gArticle.RowItemText = "Article";
            gArticle.ColumnsOfType<ReorderField>().ToList().ForEach( a => a.Visible = canEdit );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                ShowDetail();

                var script = @"
;(function() {
    $('article').Rockumentation();

    var skipAsyncInit = true;
    Sys.Application.add_load(function () {
        if (!skipAsyncInit) {
            $('article').Rockumentation();
        }
        skipAsyncInit = false;
    });
})();

Sys.Application.add_load(function () {
    var cb = window.ClipboardJS || window.Clipboard; /* Support Rock < v9.0 */
    new cb('.js-copy-slug');
});
";
                ScriptManager.RegisterStartupScript( this, GetType(), "book-view-startup", script, true );

            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var article = CurrentArticle;

            if ( article == null )
            {
                pnlVersionPicker.Visible = false;
                pnlView.Visible = true;
                pnlEdit.Visible = false;
                pnlCanEdit.Visible = false;

                return;
            }

            //
            // Check if the user has permission to view this article.
            //
            if ( !article.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                lTocContent.Text = article.Version.BuildToc( PageCache, CurrentPerson );
                lArticleContent.Text = Utility.ConvertMarkdownToHtml( "> [!WARNING]\n> You are not authorized to view this article." );

                pnlView.Visible = true;
                pnlEdit.Visible = false;
                pnlCanEdit.Visible = false;

                return;
            }

            if ( article.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                bool canAdmin = article.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

                pnlCanEdit.Visible = true;
                lbChildren.Visible = canAdmin;

                //
                // Setup the security button.
                //
                var securityJs = ResolveUrl( string.Format( "~/Secure/{0}/{1}?t=Article+Security&pb=&sb=Done",
                    EntityTypeCache.Get( typeof( DocumentationArticle ) ).Id, article.Id ) );
                lbSecurity.HRef = string.Format( "javascript: Rock.controls.modal.show($(this), '{0}')", securityJs );
                lbSecurity.Visible = canAdmin;

                lbCopySlug.Attributes.Add( "data-clipboard-text", article.Slug );

                if ( article.Version.IsLocked )
                {
                    lbEdit.Visible = false;
                    lbChildren.Visible = false;
                }
                else
                {
                    lbEdit.Visible = true;
                    lbChildren.Visible = true;
                }
            }
            else
            {
                pnlCanEdit.Visible = false;
            }

            var isPrint = PageParameter( "Print" ).AsBoolean();
            var headerTemplate = GetAttributeValue( AttributeKey.HeaderTemplate );
            var footerTemplate = GetAttributeValue( AttributeKey.FooterTemplate );

            lTocContent.Text = article.Version.BuildToc( PageCache, CurrentPerson );
            lArticleContent.Text = article.GetArticleHtml( CurrentPerson, isPrint, headerTemplate, footerTemplate, PageCache );

            if ( isPrint )
            {
                pnlContent.AddCssClass( "for-print" );
                pnlCanEdit.Visible = false;
            }

            SetPageHeaderAndFooter( article );

            PrepareVersionPicker( article );

            if ( GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
            {
                RockPage.PageTitle = article.Title;
                RockPage.BrowserTitle = string.Format( "{0} | {1}", article.Title, RockPage.Site.Name );
                RockPage.Header.Title = string.Format( "{0} | {1}", article.Title, RockPage.Site.Name );
            }

            //
            // Setup the search field.
            //
            pnlSearch.Visible = article.Book.IsIndexEnabled;
            pnlSearch.Attributes.Add( "data-version-id", article.VersionId.ToString() );
            pnlSearch.Attributes.Add( "data-page-id", RockPage.PageId.ToString() );

            pnlView.Visible = true;
            pnlEdit.Visible = false;
            lTocContent.Visible = true;
            navArticle.Visible = true;
        }

        /// <summary>
        /// Shows the edit panel.
        /// </summary>
        private void ShowEdit()
        {
            var article = CurrentArticle;
            var binaryFileType = new BinaryFileTypeService( new RockContext() ).Get( article.Book.BinaryFileTypeId ?? 0 );
            var isAdmin = article.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            tbTitle.Text = article.Title;

            if ( article.Book.ContentType == ContentType.Structured )
            {
                seContent.Content = article.Content;
                seContent.BinaryFileTypeGuid = binaryFileType != null ? ( Guid? ) binaryFileType.Guid : null;
                seContent.Visible = true;
            }
            else
            {
                mdContent.Text = article.Content;
                mdContent.BinaryFileTypeGuid = binaryFileType != null ? ( Guid? ) binaryFileType.Guid : null;
                mdContent.Visible = true;
            }

            hfEditId.Value = article.Id.ToString();

            //
            // Populate the parent article drop down.
            //
            ddlParentArticle.Items.Clear();
            if ( article.ParentArticle != null )
            {
                PopulateParentArticleDropDown( article.Version.MainArticle, article.Id );
                ddlParentArticle.SetValue( article.ParentArticleId );
            }

            tbTitle.Enabled = isAdmin;
            ddlParentArticle.Enabled = isAdmin && article.ParentArticle != null;
            ddlParentArticle.Required = article.ParentArticle != null;

            using ( var rockContext = new RockContext() )
            {
                var tmpArticle = new DocumentationArticleService( rockContext ).Get( article.Id );
                tmpArticle.LoadAttributes( rockContext );

                avcEditAttributes.AddEditControls( tmpArticle );
            }

            pnlView.Visible = false;
            pnlCanEdit.Visible = false;
            pnlEdit.Visible = true;
            pnlVersionPicker.Visible = false;
            lTocContent.Visible = false;
            navArticle.Visible = false;
        }
        
        /// <summary>
        /// Configure the page headers and footers.
        /// </summary>
        /// <param name="article">The article being displayed.</param>
        private void SetPageHeaderAndFooter( DocumentationArticleCache article )
        {
            bool isPrint = PageParameter( "Print" ).AsBoolean();

            if ( !isPrint )
            {
                ltPageHeader.Text = string.Empty;
                ltPageFooter.Text = string.Empty;
            }

            if ( !isPrint && GetAttributeValue( AttributeKey.ShowPanel ).AsBoolean() )
            {
                ltPanelHeader.Text = string.Format( @"
<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title"">{0}</h1>
    </div>
    <div class=""panel-body"">
", article.Book.Title.EncodeHtml() );
                ltPanelFooter.Text = @"
    </div>
</div>
";
            }
            else
            {
                ltPanelHeader.Text = string.Empty;
                ltPanelFooter.Text = string.Empty;
            }

            var headerTemplate = GetAttributeValue( AttributeKey.PageHeaderTemplate );
            var footerTemplate = GetAttributeValue( AttributeKey.PageFooterTemplate );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

            mergeFields.Add( "Article", article );
            mergeFields.Add( "Book", article.Book );
            mergeFields.Add( "IsPrinting", isPrint );

            ltPageHeader.Text = headerTemplate.ResolveMergeFields( mergeFields );
            ltPageFooter.Text = footerTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Binds the article grid.
        /// </summary>
        private void BindArticleGrid()
        {
            RockContext rockContext = new RockContext();
            var qry = new DocumentationArticleService( rockContext ).Queryable()
                .Where( a => a.ParentArticleId == CurrentArticleId.Value )
                .OrderBy( a => a.Order );

            gArticle.DataSource = qry.ToList();
            gArticle.DataBind();
        }

        /// <summary>
        /// Populates the parent article drop down.
        /// </summary>
        /// <param name="article">The article.</param>
        /// <param name="currentArticleId">The current article identifier.</param>
        /// <param name="depth">The depth of this article.</param>
        private void PopulateParentArticleDropDown( DocumentationArticleCache article, int currentArticleId, int depth = 0 )
        {
            var prefix = string.Empty;

            if ( article.Id == currentArticleId )
            {
                return;
            }

            if ( depth > 0 )
            {
                prefix = new string( '-', depth ) + " ";
            }

            ddlParentArticle.Items.Add( new ListItem( prefix + article.Title, article.Id.ToString() ) );

            foreach ( var c in article.ChildArticles )
            {
                PopulateParentArticleDropDown( c, currentArticleId, depth + 1 );
            }
        }

        /// <summary>
        /// Prepares the version picker.
        /// </summary>
        /// <param name="article">The article.</param>
        private void PrepareVersionPicker( DocumentationArticleCache article )
        {
            var items = new List<object>();

            foreach ( var version in article.Book.Versions.OrderByDescending( v => v.Order ) )
            {
                bool canView = version.IsAuthorized( Authorization.VIEW, CurrentPerson ) && article.Book.AllowVersionBrowsing;

                if ( !canView )
                {
                    canView = version.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }

                if ( canView )
                {
                    var versionArticle = DocumentationArticleCache.GetArticleFromSlug( article.Book.Slug, article.SlugPath, version.Version );

                    //
                    // If this same article doesn't exist in the selected version, then just go
                    // to the index page.
                    //
                    if ( versionArticle == null )
                    {
                        versionArticle = DocumentationArticleCache.GetArticleFromSlug( article.Book.Slug, "/", version.Version );
                    }

                    items.Add( new
                    {
                        Url = Utility.BuildArticleRoute( PageCache, versionArticle, versionArticle.SlugPath ),
                        Title = version.Version
                    } );
                }
            }

            rptrVersionPicker.DataSource = items;
            rptrVersionPicker.DataBind();

            lCurrentVersion.Text = article.Version.Version;
            pnlVersionPicker.Visible = items.Count > 1;
        }

        #endregion

        #region gArticle Events

        /// <summary>
        /// Handles the GridRebind event of the gArticle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gArticle_GridRebind( object sender, EventArgs e )
        {
            BindArticleGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gArticle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gArticle_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new DocumentationArticleService( rockContext );

            var articles = service.Queryable()
                .Where( a => a.ParentArticleId.HasValue && a.ParentArticleId == CurrentArticleId.Value )
                .OrderBy( a => a.Order )
                .ToList();

            service.Reorder( articles, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            //
            // Workaround for cache not being invalided right away on Rock v12.
            //
            if ( Utility.IsRock12OrLater() )
            {
                System.Threading.Thread.Sleep( 250 );
            }

            lTocContent.Text = CurrentArticle.Version.BuildToc( PageCache, CurrentPerson );

            BindArticleGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gArticle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gArticle_AddClick( object sender, EventArgs e )
        {
            tbChildrenAddTitle.Text = string.Empty;

            pnlChildrenAdd.Visible = true;
            pnlChildrenList.Visible = false;
        }

        /// <summary>
        /// Handles the Delete event of the gArticle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gArticle_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new DocumentationArticleService( rockContext );
                var article = service.Get( e.RowKeyId );
                var version = DocumentationVersionCache.Get( article.VersionId );

                service.DeleteRecursive( article );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    throw ex;
                }

                //
                // Workaround for cache not being invalided right away on Rock v12.
                //
                if ( Utility.IsRock12OrLater() )
                {
                    System.Threading.Thread.Sleep( 250 );
                }

                lTocContent.Text = version.BuildToc( PageCache, CurrentPerson );

                BindArticleGrid();
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gArticle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gArticle_RowSelected( object sender, RowEventArgs e )
        {
            var article = new DocumentationArticleService( new RockContext() ).Get( e.RowKeyId );

            var url = Utility.BuildArticleRoute( PageCache, CurrentArticle, article.SlugPath );

            Response.Redirect( url, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gArticle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gArticle_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var deleteIndex = gArticle.GetColumnIndexByFieldType( typeof( DeleteField ) );
                var article = ( DocumentationArticle ) e.Row.DataItem;

                bool canDelete = article.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                e.Row.Cells[deleteIndex.Value].Controls[0].Visible = canDelete;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbChildren control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbChildren_Click( object sender, EventArgs e )
        {
            BindArticleGrid();

            pnlChildrenList.Visible = true;
            pnlChildrenAdd.Visible = false;
            mdlChildren.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var articleService = new DocumentationArticleService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );
            var article = articleService.Get( hfEditId.Value.AsInteger() );
            List<int> uploadedFileIds;
            bool needsRedirect = false;

            article.Title = tbTitle.Text;

            var newSlug = article.Title.GenerateSlug();
            if ( article.Slug != newSlug )
            {
                article.Slug = newSlug;
                needsRedirect = true;
            }

            if ( seContent.Visible )
            {
                article.Content = seContent.Content;
                uploadedFileIds = seContent.UploadedFileIds.Distinct().ToList();
            }
            else
            {
                article.Content = mdContent.Text;
                uploadedFileIds = mdContent.UploadedFileIds.Distinct().ToList();
            }

            if ( article.ParentArticleId.HasValue && article.ParentArticleId != ddlParentArticle.SelectedValueAsId() )
            {
                var p = DocumentationArticleCache.Get( article.ParentArticleId.Value )
                    .ChildArticles
                    .OrderByDescending( a => a.Order )
                    .FirstOrDefault();

                article.ParentArticleId = ddlParentArticle.SelectedValueAsId();
                article.Order = p != null ? p.Order + 1 : 0;
                needsRedirect = true;
            }

            //
            // Check each file they uploaded while writing this note. If it is referenced in the
            // note text then attach it to the project and mark the binary file as permanent.
            //
            foreach ( var fileId in uploadedFileIds )
            {
                if ( article.Book.Attachments.Any( a => a.BinaryFileId == fileId ) )
                {
                    continue;
                }

                if ( article.Content.Contains( string.Format( "/GetFile.ashx?Id={0})", fileId ) )
                    || article.Content.Contains( string.Format( "/GetFile.ashx?Id={0}\"", fileId ) ) )
                {
                    var attachment = new DocumentationBookAttachment { Id = 0 };
                    article.Book.Attachments.Add( attachment );
                    attachment.CreatedByPersonAliasId = CurrentPersonAliasId;
                    attachment.CreatedDateTime = RockDateTime.Now;
                    attachment.BinaryFileId = fileId;
                }
            }

            //
            // Check each EditorJS file uploaded. If it is referenced in the
            // content then attach it to the project and mark the binary file
            // as permanent.
            //
            if ( seContent.Visible )
            {
                var editorData = article.Content.FromJsonOrNull<EditorSaveData>();
                foreach ( var block in editorData.Blocks )
                {
                    if ( block.Type == "image" && block.Data.file.fileId != null )
                    {
                        int fileId = ( int ) ( long ) block.Data.file.fileId;

                        if ( article.Book.Attachments.Any( a => a.BinaryFileId == fileId ) )
                        {
                            continue;
                        }

                        var attachment = new DocumentationBookAttachment { Id = 0 };
                        article.Book.Attachments.Add( attachment );
                        attachment.CreatedByPersonAliasId = CurrentPersonAliasId;
                        attachment.CreatedDateTime = RockDateTime.Now;
                        attachment.BinaryFileId = fileId;
                    }
                }
            }

            avcEditAttributes.GetEditValues( article );

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                article.SaveAttributeValues( rockContext );
            } );

            //
            // Workaround for cache not being invalided right away on Rock v12.
            //
            if ( Utility.IsRock12OrLater() )
            {
                CurrentArticle.SetFromEntity( article );
                System.Threading.Thread.Sleep( 250 );
            }

            if ( !needsRedirect )
            {
                ShowDetail();
            }
            else
            {
                var url = Utility.BuildArticleRoute( PageCache, CurrentArticle, CurrentArticle.SlugPath );
                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Handles the Click event of the lbChildrenAddSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbChildrenAddSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var articleService = new DocumentationArticleService( rockContext );
                var parentArticle = CurrentArticle;

                var article = new DocumentationArticle
                {
                    BookId = parentArticle.BookId,
                    VersionId = parentArticle.VersionId,
                    ParentArticleId = parentArticle.Id,
                    Order = ( parentArticle.ChildArticles.Select( a => ( int? ) a.Order ).Max() ?? -1 ) + 1
                };
                articleService.Add( article );

                article.Title = tbChildrenAddTitle.Text;
                article.Slug = article.Title.GenerateSlug();

                if ( parentArticle.Book.ContentType == ContentType.Structured )
                {
                    var time = RockDateTime.Now.ToJavascriptMilliseconds();
                    var id = Utility.GenerateBlockId();

                    article.Content = string.Format( "{{\"time\":{0},\"blocks\":[{{\"id\":\"{1}\",\"type\":\"header\",\"data\":{{\"text\":\"Overview\",\"level\":2}}}}],\"version\":\"2.21.0\"}}",
                        time, id );
                }
                else
                {
                    article.Content = "## Overview";
                }

                rockContext.SaveChanges();

                //
                // Workaround for cache not being invalided right away on Rock v12.
                //
                if ( Utility.IsRock12OrLater() )
                {
                    System.Threading.Thread.Sleep( 250 );
                }

                lTocContent.Text = parentArticle.Version.BuildToc( PageCache, CurrentPerson );

                BindArticleGrid();

                pnlChildrenAdd.Visible = false;
                pnlChildrenList.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbChildrenAddCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbChildrenAddCancel_Click( object sender, EventArgs e )
        {
            pnlChildrenAdd.Visible = false;
            pnlChildrenList.Visible = true;
        }

        #endregion
    }
}
