using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.blueboxmoon.Rockumentation;
using com.blueboxmoon.Rockumentation.Cache;
using com.blueboxmoon.Rockumentation.Enums;
using com.blueboxmoon.Rockumentation.Model;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.Rockumentation
{
    /// <summary>
    /// Displays the details of a documentation book.
    /// </summary>
    [DisplayName( "Book Detail" )]
    [Category( "Blue Box Moon > Rockumentation" )]
    [Description( "Displays the details of a documentation book." )]

    [LinkedPage( "Version Detail Page" )]
    [LinkedPage( "View Book Page" )]
    public partial class BookDetail : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the article attributes.
        /// </summary>
        private List<Rock.Model.Attribute> ArticleAttributesState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["ArticleAttributesState"] as string;
            ArticleAttributesState = json.FromJsonOrNull<List<Rock.Model.Attribute>>() ?? new List<Rock.Model.Attribute>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = false;
            int bookId = PageParameter( "BookId" ).AsInteger();

            var book = new DocumentationBookService( new RockContext() ).Get( bookId );

            if ( book != null )
            {
                canEdit = book.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            }

            gDocumentationBookVersion.DataKeyNames = new string[] { "Id" };
            gDocumentationBookVersion.Actions.ShowAdd = canEdit;
            gDocumentationBookVersion.Actions.AddClick += gDocumentationBookVersion_AddClick;
            gDocumentationBookVersion.IsDeleteEnabled = true;

            gArticleAttributes.DataKeyNames = new string[] { "Guid" };
            gArticleAttributes.Actions.AddClick += gArticleAttributes_Add;
            gArticleAttributes.GridRebind += gArticleAttributes_GridRebind;
            gArticleAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gArticleAttributes.GridReorder += gArticleAttributes_GridReorder;

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
                var bookId = PageParameter( "BookId" ).AsInteger();
                if ( bookId > 0 )
                {
                    ShowDetail( bookId );
                    BindVersionFilter();
                }
                else
                {
                    ShowEdit( bookId );
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ArticleAttributesState"] = JsonConvert.SerializeObject( ArticleAttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }


        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? bookId = PageParameter( pageReference, "BookId" ).AsIntegerOrNull();
            if ( bookId != null )
            {
                var book = new DocumentationBookService( new RockContext() ).Get( bookId.Value );
                if ( book != null )
                {
                    breadCrumbs.Add( new BreadCrumb( book.Title, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Book", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        public void ShowDetail( int bookId )
        {
            var rockContext = new RockContext();

            var book = new DocumentationBookService( rockContext ).Get( bookId );

            if ( book == null )
            {
                nbErrorMessage.Text = "Book not found.";
                pnlDetails.Visible = false;
                pnlVersionList.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            if ( !book.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbUnauthorized.Text = EditModeMessage.NotAuthorizedToView( DocumentationBook.FriendlyTypeName );
                pnlDetails.Visible = false;
                pnlVersionList.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            lTitle.Text = book.Title.EncodeHtml();
            lSlug.Text = book.Slug;
            lDescription.Text = book.Description.EncodeHtml();
            hlIndexed.Visible = book.IsIndexEnabled;
            hlContentType.Text = book.ContentType.ConvertToString();

            book.LoadAttributes( rockContext );
            avcAttributes.AddDisplayControls( book );

            var securityJs = ResolveUrl( string.Format( "~/Secure/{0}/{1}?t=Book+Security&pb=&sb=Done",
                EntityTypeCache.Get( typeof( DocumentationBook ) ).Id, book.Id ) );
            lbSecurity.HRef = string.Format( "javascript: Rock.controls.modal.show($(this), '{0}')", securityJs );
            lbSecurity.Visible = book.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            //
            // Build the URL link for the View Book button.
            //
            var bookCache = DocumentationBookCache.Get( book.Id );
            var versionCache = bookCache.LatestPublishedVersion ?? bookCache.Versions.OrderBy( a => a.Order ).First();
            var viewBookPageCache = PageCache.Get( GetAttributeValue( "ViewBookPage" ).AsGuid() );
            var url = Utility.BuildArticleRoute( viewBookPageCache, versionCache.MainArticle, "/" );
            lbViewBook.HRef = url;

            BindVersionGrid();

            pnlDetails.Visible = true;
            pnlVersionList.Visible = true;
            pnlEdit.Visible = false;
            HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Shows the edit panel.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        public void ShowEdit( int bookId )
        {
            var rockContext = new RockContext();
            DocumentationBook book = null;

            if ( bookId != 0 )
            {
                book = new DocumentationBookService( rockContext ).Get( bookId );
                pdAuditDetails.SetEntity( book, ResolveRockUrl( "~" ) );
            }

            if ( book == null )
            {
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );

                book = new DocumentationBook
                {
                    Id = 0,
                    AllowVersionBrowsing = true,
                    BinaryFileTypeId = binaryFileTypeService.Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id,
                    ContentType = ContentType.Structured
                };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            string title = book.Id > 0 ? ActionTitle.Edit( DocumentationBook.FriendlyTypeName ) : ActionTitle.Add( DocumentationBook.FriendlyTypeName );
            lEditTitle.Text = title.FormatAsHtmlTitle();

            hfId.Value = book.Id.ToString();

            tbEditTitle.Text = book.Title;
            tbEditDescription.Text = book.Description;
            pAttachmentFileType.SetValue( book.BinaryFileTypeId );
            cbEditAllowVersionBrowsing.Checked = book.AllowVersionBrowsing;
            cbEditIsIndexed.Checked = book.IsIndexEnabled;
            ddlContentType.BindToEnum<ContentType>();
            ddlContentType.SetValue( book.ContentType.ConvertToInt() );
            ddlContentType.Enabled = book.Id == 0 || book.ContentType == ContentType.Markdown;

            gArticleAttributes.Actions.ShowAdd = true;
            gArticleAttributes.IsDeleteEnabled = true;

            book.LoadAttributes( rockContext );
            avcEditAttributes.AddEditControls( book );

            //
            // Load attribute data
            //
            ArticleAttributesState = new List<Rock.Model.Attribute>();
            var attributeService = new AttributeService( new RockContext() );
            string qualifierValue = book.Id.ToString();

            attributeService.GetByEntityTypeId( new DocumentationArticle().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "BookId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .ToList()
                .ForEach( a => ArticleAttributesState.Add( a ) );
            BindArticleAttributesGrid();

            pnlDetails.Visible = false;
            pnlVersionList.Visible = false;
            pnlEdit.Visible = true;
            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        protected void BindVersionFilter()
        {
            tbVersionFilter.Text = gfDocumentationBookVersion.GetUserPreference( "Version" );
            ddlIsPublishedFilter.SelectedValue = gfDocumentationBookVersion.GetUserPreference( "Published" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindVersionGrid()
        {
            RockContext rockContext = new RockContext();
            var qry = GetVersionQuery( rockContext );

            gDocumentationBookVersion.DataSource = qry.ToList();
            gDocumentationBookVersion.DataBind();
        }

        /// <summary>
        /// Gets the DocumentationBookVersions.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected IQueryable<DocumentationVersion> GetVersionQuery( RockContext rockContext )
        {
            int bookId = PageParameter( "BookId" ).AsInteger();
            var documentationBookVersionService = new DocumentationVersionService( rockContext );
            var sortProperty = gDocumentationBookVersion.SortProperty;

            var qry = documentationBookVersionService.Queryable().AsNoTracking()
                .Where( a => a.BookId == bookId );

            // Filter by Version
            string version = gfDocumentationBookVersion.GetUserPreference( "Version" );
            if ( !string.IsNullOrWhiteSpace( version ) )
            {
                qry = qry.Where( a => a.Version.Contains( version ) );
            }

            // Filter by Is Published
            string isPublished = gfDocumentationBookVersion.GetUserPreference( "Published" );
            if ( !string.IsNullOrWhiteSpace( isPublished ) )
            {
                qry = qry.Where( a => a.IsPublished == ( isPublished == "Yes" ) );
            }

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.Order ).ThenBy( a => a.Id );
            }

            return qry;
        }

        /// <summary>
        /// Saves the changes to the book.
        /// </summary>
        /// <param name="confirmed">if set to <c>true</c> then the user has already confirmed a conversion.</param>
        private void SaveChangesToBook( bool confirmed )
        {
            var rockContext = new RockContext();
            DocumentationBook book;

            DocumentationBookService bookService = new DocumentationBookService( rockContext );

            int bookId = int.Parse( hfId.Value );

            if ( bookId == 0 )
            {
                book = new DocumentationBook();
                bookService.Add( book );
                book.CreatedByPersonAliasId = CurrentPersonAliasId;
                book.CreatedDateTime = RockDateTime.Now;
            }
            else
            {
                book = bookService.Get( bookId );
                book.ModifiedByPersonAliasId = CurrentPersonAliasId;
                book.ModifiedDateTime = RockDateTime.Now;

                if ( !confirmed && book.ContentType != ddlContentType.SelectedValueAsEnum<ContentType>( ContentType.Structured ) )
                {
                    mdConfirmConversion.Show();
                    return;
                }
            }

            book.Title = tbEditTitle.Text;
            book.Description = tbEditDescription.Text;
            book.BinaryFileTypeId = pAttachmentFileType.SelectedValueAsId();
            book.AllowVersionBrowsing = cbEditAllowVersionBrowsing.Checked;
            book.IsIndexEnabled = cbEditIsIndexed.Checked;
            book.Slug = book.Title.GenerateSlug();

            var contentType = ddlContentType.SelectedValueAsEnum<ContentType>( ContentType.Structured );
            var convertToStructured = contentType == ContentType.Structured && contentType != book.ContentType && book.Id != 0;
            book.ContentType = contentType;

            avcEditAttributes.GetEditValues( book );

            if ( !Page.IsValid || !book.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            bool isNewBook = book.Id == 0;

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                book.SaveAttributeValues( rockContext );

                // Save the Article Attributes
                int entityTypeId = EntityTypeCache.Get( typeof( DocumentationArticle ) ).Id;
                SaveAttributes( book.Id, entityTypeId, ArticleAttributesState, rockContext );

                if ( isNewBook )
                {
                    var bookVersionService = new DocumentationVersionService( rockContext );

                    var bookVersion = new DocumentationVersion
                    {
                        Version = "1.0.0",
                        Description = string.Empty,
                        BookId = book.Id,
                        IsPublished = true,
                        Order = 0
                    };

                    bookVersionService.Add( bookVersion );
                    rockContext.SaveChanges();

                    var articleService = new DocumentationArticleService( rockContext );

                    var article = new DocumentationArticle
                    {
                        Title = book.Title,
                        BookId = book.Id,
                        VersionId = bookVersion.Id,
                        Slug = book.Slug,
                        Order = 0
                    };

                    if ( book.ContentType == ContentType.Structured )
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

                    articleService.Add( article );

                    rockContext.SaveChanges();
                }

                if ( convertToStructured )
                {
                    ConvertAllArticlesToStructured( rockContext, book.Id );
                }
            } );

            if ( isNewBook )
            {
                NavigateToCurrentPage( new Dictionary<string, string>
                {
                    { "BookId", book.Id.ToString() }
                } );
            }
            else
            {
                ShowDetail( book.Id );
            }
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int bookId, int entityTypeId, List<Rock.Model.Attribute> attributes, RockContext rockContext )
        {
            string qualifierColumn = "BookId";
            string qualifierValue = bookId.ToString();

            AttributeService attributeService = new AttributeService( rockContext );

            // Get the existing attributes for this entity type and qualifier value
            var existingAttributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                Rock.Web.Cache.AttributeCache.Remove( attr.Id );
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            // Update the Attributes that were assigned in the UI
            foreach ( var attr in attributes )
            {
                Rock.Attribute.Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }

            AttributeCache.RemoveEntityAttributes();
        }

        /// <summary>
        /// Converts all articles to structured content.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bookId">The book identifier.</param>
        private void ConvertAllArticlesToStructured( RockContext rockContext, int bookId )
        {
            var articleService = new DocumentationArticleService( rockContext );

            var articles = articleService.Queryable()
                .Where( a => a.BookId == bookId )
                .ToList();

            foreach ( var article in articles )
            {
                var markdown = article.Content;
                var time = RockDateTime.Now.ToJavascriptMilliseconds();
                var id = Utility.GenerateBlockId();

                article.Content = string.Format( "{{\"time\":{0},\"blocks\":[{{\"id\":\"{1}\",\"type\":\"bbm.rockumentation.markdown\",\"data\":{{\"text\":{2}}}}}],\"version\":\"2.21.0\"}}",
                    time, id, markdown.ToJson() );
            }

            rockContext.SaveChanges();
        }

        #endregion

        #region gDocumentationBookVersion Events

        /// <summary>
        /// Handles the GridRebind event of the gDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDocumentationBookVersion_GridRebind( object sender, EventArgs e )
        {
            BindVersionGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDocumentationBookVersion_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "VersionDetailPage", "VersionId", 0, "BookId", PageParameter( "BookId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Row Selected event of the gDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDocumentationBookVersion_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "VersionDetailPage", "VersionId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDocumentationBookVersion_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var documentationBookVersionService = new DocumentationVersionService( rockContext );
            var articleService = new DocumentationArticleService( rockContext );

            var documentationBookVersion = documentationBookVersionService.Get( e.RowKeyId );
            if ( documentationBookVersion != null )
            {
                string errorMessage;

                if ( !documentationBookVersionService.CanDelete( documentationBookVersion, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                documentationBookVersion.Articles
                    .ToList()
                    .ForEach( a => articleService.Delete( a ) );

                documentationBookVersionService.Delete( documentationBookVersion );

                rockContext.SaveChanges();
            }

            BindVersionGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDocumentationBookVersion_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var deleteIndex = gDocumentationBookVersion.GetColumnIndexByFieldType( typeof( DeleteField ) );
                var version = ( DocumentationVersion ) e.Row.DataItem;

                e.Row.Cells[deleteIndex.Value].Controls[0].Visible = version.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            }
        }

        #endregion

        #region gfDocumentationBookVersion Events

        /// <summary>
        /// Handles the filter display for the gfDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridFilter.DisplayFilterValueArgs" /> instance containing the event data.</param>
        protected void gfDocumentationBookVersion_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Version":
                    e.Value = e.Value;
                    break;

                case "Published":
                    e.Value = e.Value;
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfDocumentationBookVersion_ApplyFilterClick( object sender, EventArgs e )
        {
            gfDocumentationBookVersion.SaveUserPreference( "Version", tbVersionFilter.Text );
            gfDocumentationBookVersion.SaveUserPreference( "Published", ddlIsPublishedFilter.SelectedValue );

            BindVersionGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfDocumentationBookVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfDocumentationBookVersion_ClearFilterClick( object sender, EventArgs e )
        {
            gfDocumentationBookVersion.DeleteUserPreferences();
            BindVersionFilter();
        }

        #endregion

        #region Article Attributes

        /// <summary>
        /// Handles the Add event of the gArticleAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gArticleAttributes_Add( object sender, EventArgs e )
        {
            gArticleAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gArticleAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gArticleAttributes_Edit( object sender, RowEventArgs e )
        {
            var attributeGuid = ( Guid ) e.RowKeyValue;

            gArticleAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Shows the edit dialog for the attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute guid.</param>
        protected void gArticleAttributes_ShowEdit( Guid attributeGuid )
        {
            Rock.Model.Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Rock.Model.Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtArticleAttributes.ActionTitle = ActionTitle.Add( tbEditTitle.Text + " Article Attribute" );

            }
            else
            {
                attribute = ArticleAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtArticleAttributes.ActionTitle = ActionTitle.Edit( tbEditTitle.Text + " Article Attribute" );
            }

            edtArticleAttributes.ReservedKeyNames = ArticleAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtArticleAttributes.SetAttributeProperties( attribute, typeof( DocumentationArticle ) );

            dlgArticleAttributes.Show();
            //ShowDialog( "ArticleAttributes", true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gArticleAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gArticleAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var movedAttribute = ArticleAttributesState.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedAttribute != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherAttribute in ArticleAttributesState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherAttribute.Order = otherAttribute.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherAttribute in ArticleAttributesState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherAttribute.Order = otherAttribute.Order - 1;
                    }
                }

                movedAttribute.Order = e.NewIndex;
            }

            BindArticleAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gArticleAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gArticleAttributes_Delete( object sender, RowEventArgs e )
        {
            var attributeGuid = ( Guid ) e.RowKeyValue;
            ArticleAttributesState.RemoveEntity( attributeGuid );

            BindArticleAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gArticleAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gArticleAttributes_GridRebind( object sender, EventArgs e )
        {
            BindArticleAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgArticleAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgArticleAttributes_SaveClick( object sender, EventArgs e )
        {
            var attribute = new Rock.Model.Attribute();
            edtArticleAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( ArticleAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = ArticleAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                ArticleAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = ArticleAttributesState.Any() ? ArticleAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            ArticleAttributesState.Add( attribute );

            BindArticleAttributesGrid();

            dlgArticleAttributes.Hide();
            //HideDialog();
        }

        /// <summary>
        /// Binds the article attribute type grid.
        /// </summary>
        private void BindArticleAttributesGrid()
        {
            gArticleAttributes.DataSource = ArticleAttributesState
                .Select( a => new
                {
                    Id = a.Id,
                    Guid = a.Guid,
                    Name = a.Name,
                    Description = a.Description,
                    IsRequired = a.IsRequired,
                    Order = a.Order
                } )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            gArticleAttributes.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( PageParameter( "BookId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SaveChangesToBook( false );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.AsInteger() == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowDetail( hfId.Value.AsInteger() );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int bookId = hfId.ValueAsInt();

            if ( bookId != 0 )
            {
                ShowDetail( bookId );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfirmConversion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmConversion_SaveClick( object sender, EventArgs e )
        {
            mdConfirmConversion.Hide();
            SaveChangesToBook( true );
        }

        #endregion
    }
}
