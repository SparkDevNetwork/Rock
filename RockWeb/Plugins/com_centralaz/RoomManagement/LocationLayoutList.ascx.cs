using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    /// <summary>
    /// User control for managing note types
    /// </summary>
    [DisplayName( "Location Layout List" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "A list of layouts tied to a location" )]
    [IntegerField( "Layout Image Height", "", true, 150 )]
    public partial class LocationLayoutList : RockBlock, ISecondaryBlock
    {
        #region Properties

        public int LocationId
        {
            get
            {
                return PageParameter( "LocationId" ).AsInteger();
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = true;

            rGrid.Actions.AddClick += rGrid_Add;
            rGrid.GridRebind += rGrid_GridRebind;

            modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    modalDetails.Show();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var locationLayoutService = new LocationLayoutService( rockContext );
                var locationLayout = locationLayoutService.Get( e.RowKeyId );
                if ( locationLayout != null )
                {
                    locationLayoutService.Delete( locationLayout );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( null );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var locationLayoutService = new LocationLayoutService( rockContext );

            // Save the layout
            int layoutId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out layoutId ) )
            {
                layoutId = 0;
            }

            LocationLayout layout = null;

            if ( layoutId != 0 )
            {
                layout = locationLayoutService.Get( layoutId );
            }

            if ( layout == null )
            {
                layout = new LocationLayout();
                if ( LocationId != 0 )
                {
                    layout.LocationId = LocationId;
                }

                locationLayoutService.Add( layout );
            }

            // Add layout Info
            layout.Name = tbName.Text;
            layout.Description = tbDescription.Text;
            layout.IsActive = cbIsActive.Checked;
            layout.IsDefault = cbIsDefault.Checked;

            int? orphanedImageId = null;
            if ( layout.LayoutPhotoId != iuPhoto.BinaryFileId )
            {
                orphanedImageId = layout.LayoutPhotoId;
                layout.LayoutPhotoId = iuPhoto.BinaryFileId;
            }

            if ( layout.IsValid )
            {
                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    if ( layout.IsDefault == true )
                    {
                        var otherDefaultLocationLayouts = locationLayoutService.Queryable().Where( ll => ll.LocationId == layout.LocationId && ll.IsDefault == true ).ToList();
                        foreach ( var otherDefaultLocationLayout in otherDefaultLocationLayouts )
                        {
                            otherDefaultLocationLayout.IsDefault = false;
                        }
                    }

                    rockContext.SaveChanges();

                    var binaryFileService = new BinaryFileService( rockContext );
                    if ( orphanedImageId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( orphanedImageId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                        }
                    }

                    if ( layout.LayoutPhotoId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( layout.LayoutPhotoId.Value );
                        if ( binaryFile != null )
                        {
                            binaryFile.IsTemporary = false;
                        }
                    }

                    rockContext.SaveChanges();

                } );

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();
                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            List<LocationLayout> layoutList = GetLayouts();
            string photoFormat = "<img src=\"/GetImage.ashx?id={0}\" height=\"{1}\"></img>";

            rGrid.DataSource = layoutList.Select( l => new
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                LayoutPhoto = l.LayoutPhotoId != null ? string.Format( photoFormat, l.LayoutPhotoId, GetAttributeValue( "LayoutImageHeight" ) ) : "",
                IsActive = l.IsActive,
                IsDefault = l.IsDefault
            } )
            .ToList();

            rGrid.DataBind();
        }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <returns></returns>
        private List<LocationLayout> GetLayouts()
        {
            var layoutList = new List<LocationLayout>();
            var locationLayoutService = new LocationLayoutService( new RockContext() );

            if ( LocationId != 0 )
            {
                layoutList = locationLayoutService.Queryable().Where( l => l.LocationId == LocationId ).ToList();
            }

            return layoutList;
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        protected void ShowEdit( int? layoutId )
        {
            LocationLayout layout = null;
            if ( layoutId.HasValue )
            {
                layout = new LocationLayoutService( new RockContext() ).Get( layoutId.Value );
            }
            else
            {
                layout = new LocationLayout();
                layout.IsActive = true;
            }

            tbName.Text = layout.Name;
            tbDescription.Text = layout.Description;
            cbIsActive.Checked = layout.IsActive;
            cbIsDefault.Checked = layout.IsDefault;
            iuPhoto.BinaryFileId = layout.LayoutPhotoId;

            hfIdValue.Value = layoutId.ToString();
            modalDetails.Show();
        }
        #endregion
    }
}