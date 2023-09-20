using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;

using com.blueboxmoon.Rockumentation;
using com.blueboxmoon.Rockumentation.Cache;
using com.blueboxmoon.Rockumentation.Model;

using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.Rockumentation
{
    /// <summary>
    /// Displays the details of a documentation book version.
    /// </summary>
    [DisplayName( "Version Detail" )]
    [Category( "Blue Box Moon > Rockumentation" )]
    [Description( "Displays the details of a documentation book version." )]

    [LinkedPage( "View Book Page" )]
    public partial class VersionDetail : RockBlock
    {
        #region Properties

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private readonly IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );

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
                var versionId = PageParameter( "VersionId" ).AsInteger();
                if ( versionId > 0 )
                {
                    ShowDetail( versionId );
                }
                else
                {
                    ShowEdit( versionId );
                }
            }
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? versionId = PageParameter( pageReference, "VersionId" ).AsIntegerOrNull();
            if ( versionId != null )
            {
                var version = new DocumentationVersionService( new RockContext() ).Get( versionId.Value );
                if ( version != null )
                {
                    breadCrumbs.Add( new BreadCrumb( version.Version, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Version", pageReference ) );
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
        /// <param name="versionId">The version identifier.</param>
        private void ShowDetail( int versionId )
        {
            var rockContext = new RockContext();

            var version = new DocumentationVersionService( rockContext ).Get( versionId );

            if ( version == null)
            {
                nbErrorMessage.Text = "Version not found.";
                pnlDetails.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            if ( !version.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbUnauthorized.Text = EditModeMessage.NotAuthorizedToView( DocumentationVersion.FriendlyTypeName );
                pnlDetails.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            lTitle.Text = version.Version.EncodeHtml();
            lDescription.Text = version.Description.EncodeHtml();

            hlNotPublished.Visible = !version.IsPublished;
            hlPublished.Visible = version.IsPublished;
            hlLocked.Visible = version.IsLocked;

            version.LoadAttributes( rockContext );
            avcAttributes.AddDisplayControls( version );

            var securityJs = ResolveUrl( string.Format( "~/Secure/{0}/{1}?t=Book+Version+Security&pb=&sb=Done",
                EntityTypeCache.Get( typeof( DocumentationVersion ) ).Id, version.Id ) );
            lbSecurity.HRef = string.Format( "javascript: Rock.controls.modal.show($(this), '{0}')", securityJs );
            lbSecurity.Visible = version.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            lbEdit.Visible = version.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            lbDuplicate.Visible = version.Book.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            //
            // Build the URL link for the View Book button.
            //
            var versionCache = DocumentationVersionCache.Get( version.Id );
            var viewBookPageCache = PageCache.Get( GetAttributeValue( "ViewBookPage" ).AsGuid() );
            var url = Utility.BuildArticleRoute( viewBookPageCache, versionCache.MainArticle, "/" );
            lbViewBook.HRef = url;

            pnlDetails.Visible = true;
            pnlEdit.Visible = false;

            HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Shows the edit panel.
        /// </summary>
        /// <param name="versionId">The version identifier.</param>
        private void ShowEdit( int versionId )
        {
            var rockContext = new RockContext();
            DocumentationVersion version = null;

            if ( versionId != 0 )
            {
                version = new DocumentationVersionService( rockContext ).Get( versionId );
                pdAuditDetails.SetEntity( version, ResolveRockUrl( "~" ) );
            }

            if ( version == null )
            {
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );

                version = new DocumentationVersion
                {
                    Id = 0,
                    BookId = PageParameter( "BookId" ).AsInteger()
                };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            string title = version.Id > 0 ? ActionTitle.Edit( "Book Version" ) : ActionTitle.Add( "Book Version" );
            lEditTitle.Text = title.FormatAsHtmlTitle();

            hfId.Value = version.Id.ToString();

            tbEditVersion.Text = version.Version;
            tbEditDescription.Text = version.Description;
            cbEditIsPublished.Checked = version.IsPublished;
            cbEditIsLocked.Checked = version.IsLocked;

            if ( PageParameter( "DuplicateVersionId" ).IsNotNullOrWhiteSpace() )
            {
                nbWarningMessage.Text = "You are going to be duplication an existing book version. This may take some time.";
                cbEditIsPublished.Visible = false;
                cbEditIsLocked.Visible = false;
            }
            else
            {
                nbWarningMessage.Text = string.Empty;

                version.LoadAttributes( rockContext );
                avcEditAttributes.AddEditControls( version );
            }

            pnlDetails.Visible = false;
            pnlEdit.Visible = true;

            HideSecondaryBlocks( true );
        }

        #endregion

        private void DuplicateVersion( DocumentationVersion oldVersion, DocumentationVersionService versionService )
        {
            //
            // Define the task that will run to process the data.
            //
            var task = new Task( () =>
            {
                //
                // Wait for the postback to settle.
                //
                Task.Delay( 1000 ).Wait();

                var version = versionService.DuplicateVersion( oldVersion, tbEditVersion.Text, tbEditDescription.Text, CurrentPerson, ( c, t, s ) =>
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).versionDuplicateProgress( c, t, s );
                } );

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).versionDuplicateStatus( version.Id );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    ExceptionLogService.LogException( t.Exception );
                    HubContext.Clients.Client( hfConnectionId.Value ).versionDuplicateStatus( string.Empty, t.Exception.Message );
                }
            } );

            task.Start();
        }

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( PageParameter( "VersionId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var versionService = new DocumentationVersionService( rockContext );
            DocumentationVersion version;

            int versionId = int.Parse( hfId.Value );

            if ( versionId == 0 )
            {
                int bookId;

                version = new DocumentationVersion();

                if ( PageParameter( "DuplicateVersionId" ).IsNullOrWhiteSpace() )
                {
                    bookId = PageParameter( "BookId" ).AsInteger();
                    versionService.Add( version );

                    version.BookId = bookId;
                    version.Order = ( versionService.Queryable().Where( a => a.BookId == bookId ).Select( a => ( int? ) a.Order ).Max() ?? -1 ) + 1;
                    version.CreatedByPersonAliasId = CurrentPersonAliasId;
                    version.CreatedDateTime = RockDateTime.Now;
                }
            }
            else
            {
                version = versionService.Get( versionId );
                version.ModifiedByPersonAliasId = CurrentPersonAliasId;
                version.ModifiedDateTime = RockDateTime.Now;
            }

            if ( version != null )
            {
                bool isNewVersion = version.Id == 0;

                if ( PageParameter( "DuplicateVersionId" ).IsNotNullOrWhiteSpace() )
                {
                    if ( !Page.IsValid )
                    {
                        // Controls will render the error messages                    
                        return;
                    }

                    var oldVersion = versionService.Get( PageParameter( "DuplicateVersionId" ).AsInteger() );

                    DuplicateVersion( oldVersion, versionService );

                    nbWarningMessage.Text = string.Empty;
                    pnlEdit.Visible = false;
                    pnlWorking.Visible = true;
                }
                else
                {
                    version.Version = tbEditVersion.Text;
                    version.Description = tbEditDescription.Text;
                    version.IsPublished = cbEditIsPublished.Checked;
                    version.IsLocked = cbEditIsLocked.Checked;

                    version.LoadAttributes( rockContext );
                    avcEditAttributes.GetEditValues( version );

                    if ( !Page.IsValid || !version.IsValid )
                    {
                        // Controls will render the error messages                    
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        version.SaveAttributeValues( rockContext );

                        if ( isNewVersion )
                        {
                            var book = new DocumentationBookService( rockContext ).Get( version.BookId );
                            var articleService = new DocumentationArticleService( rockContext );

                            var article = new DocumentationArticle
                            {
                                Title = book.Title,
                                Content = string.Format( "# {0}", book.Title ),
                                BookId = version.Id,
                                VersionId = version.Id,
                                Slug = book.Slug,
                                Order = 0
                            };

                            articleService.Add( article );

                            rockContext.SaveChanges();
                        }
                    } );

                    if ( isNewVersion )
                    {
                        NavigateToCurrentPage( new Dictionary<string, string>
                    {
                        { "VersionId", version.Id.ToString() }
                    } );
                    }
                    else
                    {
                        ShowDetail( version.Id );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( PageParameter( "DuplicateVersionId" ).IsNotNullOrWhiteSpace() )
            {
                NavigateToCurrentPage( new Dictionary<string, string>
                {
                    { "VersionId", PageParameter( "DuplicateVersionId" ) }
                } );
            }
            if ( PageParameter( "BookId" ).IsNotNullOrWhiteSpace() )
            {
                NavigateToParentPage( new Dictionary<string, string>
                {
                    { "BookId", PageParameter( "BookId" ) }
                } );
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
            int versionId = hfId.ValueAsInt();

            if ( versionId != 0 )
            {
                ShowDetail( versionId );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDuplicate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDuplicate_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage( new Dictionary<string, string>
            {
                { "DuplicateVersionId", PageParameter( "VersionId" ) }
            } );
        }

        #endregion
    }
}
