using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.blueboxmoon.Rockumentation.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.Rockumentation
{
    /// <summary>
    /// Displays a list of Book items.
    /// </summary>
    [DisplayName( "Book List" )]
    [Category( "Blue Box Moon > Rockumentation" )]
    [Description( "Displays a list of Documentation Books." )]

    [LinkedPage( "Detail Page" )]
    public partial class BookList : RockBlock, ICustomGridColumns
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var emptyBook = new DocumentationBook();
            bool canEdit = emptyBook.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            gDocBook.DataKeyNames = new string[] { "Id" };
            gDocBook.Actions.ShowAdd = canEdit;
            gDocBook.Actions.AddClick += gDocBook_AddClick;
            gDocBook.IsDeleteEnabled = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region gDocBook Events

        /// <summary>
        /// Handles the GridRebind event of the gDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDocBook_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDocBook_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BookId", 0 );
        }

        /// <summary>
        /// Handles the Row Selected event of the gDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDocBook_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BookId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDocBook_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var bookService = new DocumentationBookService( rockContext );
            var versionService = new DocumentationVersionService( rockContext );
            var articleService = new DocumentationArticleService( rockContext );
            var attachmentService = new DocumentationBookAttachmentService( rockContext );

            var book = bookService.Get( e.RowKeyId );
            if ( book != null )
            {
                string errorMessage;

                if ( !bookService.CanDelete( book, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                foreach ( var version in book.Versions.ToList() )
                {
                    foreach ( var article in version.Articles.ToList() )
                    {
                        articleService.Delete( article );
                    }

                    versionService.Delete( version );
                }

                foreach ( var attachment in book.Attachments.ToList() )
                {
                    attachmentService.Delete( attachment );
                }

                bookService.Delete( book );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDocBook_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var deleteIndex = gDocBook.GetColumnIndexByFieldType( typeof( DeleteField ) );
                var book = ( DocumentationBook ) e.Row.DataItem;

                e.Row.Cells[deleteIndex.Value].Controls[0].Visible = book.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            }
        }

        #endregion

        #region gfDocBook Events

        /// <summary>
        /// Handles the filter display for the gfDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridFilter.DisplayFilterValueArgs" /> instance containing the event data.</param>
        protected void gfDocBook_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Title":
                    e.Value = e.Value;
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfDocBook_ApplyFilterClick( object sender, EventArgs e )
        {
            gfDocBook.SaveUserPreference( "Title", tbTitleFilter.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfDocBook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfDocBook_ClearFilterClick( object sender, EventArgs e )
        {
            gfDocBook.DeleteUserPreferences();
            BindFilter();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        protected void BindFilter()
        {
            tbTitleFilter.Text = gfDocBook.GetUserPreference( "Title" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            RockContext rockContext = new RockContext();
            var qry = GetQuery( rockContext );

            gDocBook.DataSource = qry.ToList();
            gDocBook.DataBind();
        }

        /// <summary>
        /// Gets the Books.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected IQueryable<DocumentationBook> GetQuery( RockContext rockContext )
        {
            var bookService = new DocumentationBookService( rockContext );
            var sortProperty = gDocBook.SortProperty;

            var qry = bookService.Queryable().AsNoTracking();

            // Filter by Title
            string title = gfDocBook.GetUserPreference( "Title" );
            if ( !string.IsNullOrWhiteSpace( title ) )
            {
                qry = qry.Where( a => a.Title.Contains( title ) );
            }

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.Title );
            }

            return qry;
        }

        #endregion
    }
}