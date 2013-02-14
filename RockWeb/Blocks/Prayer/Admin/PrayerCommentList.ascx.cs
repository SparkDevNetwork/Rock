//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DetailPage( Order = 0 )]
    [IntegerField( 1, "Group Category Id", "-1", null, "Filtering", "The id of the 'top level' Category.  Only prayer request comments under this category will be shown." )]
    public partial class PrayerCommentsList : Rock.Web.UI.RockBlock
    {
        #region Private BlockType Attributes
        private static readonly string PrayerCommentKeyParameter = "noteId";
        int _blockInstanceGroupCategoryId = -1;
        protected int? _prayerRequestEntityTypeId = null;
        protected NoteType _noteType;
        bool canAddEditDelete = false;
        #endregion

        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            Int32.TryParse( GetAttributeValue( "GroupCategoryId" ), out _blockInstanceGroupCategoryId );
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();
            _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            // Block Security and special attributes (RockPage takes care of "View")
            canAddEditDelete = IsUserAuthorized( "Edit" );

            // Prayer Comment stuff...
            gPrayerComments.Actions.IsAddEnabled = false;
            gPrayerComments.IsDeleteEnabled = canAddEditDelete;

            gPrayerComments.DataKeyNames = new string[] { "id" };
            gPrayerComments.GridRebind += gPrayerComments_GridRebind;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindCommentsGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Prayer Comment Grid Events

        /// <summary>
        /// Binds the comments grid.
        /// </summary>
        private void BindCommentsGrid()
        {
            var noteTypeService = new NoteTypeService();
            var noteType = noteTypeService.Get( (int)_prayerRequestEntityTypeId, "Prayer Comment" );
            // TODO log exception if noteType is null

            var noteService = new NoteService();
            var prayerComments = noteService.GetByNoteTypeId( noteType.Id );

            SortProperty sortProperty = gPrayerComments.SortProperty;

            // Sort by the given property otherwise sort by the EnteredDate
            if ( sortProperty != null )
            {
                gPrayerComments.DataSource = prayerComments.Sort( sortProperty ).ToList();
            }
            else
            {
                gPrayerComments.DataSource = prayerComments.OrderBy( n => n.Date ).ToList();
            }

            gPrayerComments.DataBind();
        }

        /// <summary>
        /// Handles the Add event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( PrayerCommentKeyParameter, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( PrayerCommentKeyParameter, (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                NoteService noteService = new NoteService();
                Note note = noteService.Get( (int)e.RowKeyValue );

                if ( note != null )
                {
                    string errorMessage;
                    if ( !noteService.CanDelete( note, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    noteService.Delete( note, CurrentPersonId );
                    noteService.Save( note, CurrentPersonId );
                }
            } );

            BindCommentsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPrayerComments_GridRebind( object sender, EventArgs e )
        {
            BindCommentsGrid();
        }

        #endregion
    }
}