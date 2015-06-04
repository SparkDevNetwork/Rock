// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
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

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for managing note types
    /// </summary>
    [DisplayName( "Note Types" )]
    [Category( "Core" )]
    [Description( "Allows note types to be managed." )]
    public partial class NoteTypes : RockBlock
    {
        #region Fields

        bool _canConfigure = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "Id" };
                rGrid.Actions.ShowAdd = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridReorder += rGrid_GridReorder;
                rGrid.GridRebind += rGrid_GridRebind;

                foreach ( var securityField in rGrid.Columns.OfType<SecurityField>() )
                {
                    securityField.EntityTypeId = EntityTypeCache.Read( typeof( NoteType ) ).Id;
                }

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            }
            else
            {
                nbMessage.Text = "You are not authorized to configure this page";
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _canConfigure )
                {
                    BindFilter();
                    BindGrid();
                }
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
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "EntityType", entityTypeFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "EntityType":

                    int? entityTypeId = e.Value.AsIntegerOrNull();
                    if ( entityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeId.Value );
                        if ( entityType != null )
                        {
                            e.Value = entityType.FriendlyName;
                        }
                    }
                    break;
            }

        }

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
                var service = new NoteTypeService( rockContext );
                var noteType = service.Get( e.RowKeyId );
                if ( noteType != null )
                {
                    string errorMessage = string.Empty;
                    if ( service.CanDelete( noteType, out errorMessage ) )
                    {
                        service.Delete( noteType );
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        nbMessage.Text = errorMessage;
                        nbMessage.Visible = true;
                    }
                }
            }

            BindFilter();
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
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var noteTypes = GetNoteTypes( rockContext );
            if ( noteTypes != null )
            {
                new NoteTypeService( rockContext ).Reorder( noteTypes.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            int noteTypeId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out noteTypeId ) )
            {
                noteTypeId = 0;
            }

            var rockContext = new RockContext();
            var service = new NoteTypeService( rockContext );
            NoteType noteType = null;

            if ( noteTypeId != 0 )
            {
                noteType = service.Get( noteTypeId );
            }

            if ( noteType == null )
            {
                var orders = service.Queryable()
                    .Where( t => t.EntityTypeId == ( entityTypePicker.SelectedEntityTypeId ?? 0 ) )
                    .Select( t => t.Order )
                    .ToList();

                noteType = new NoteType();
                noteType.Order = orders.Any() ? orders.Max( t => t ) + 1 : 0;
                service.Add( noteType );
            }

            noteType.Name = tbName.Text;
            noteType.EntityTypeId = entityTypePicker.SelectedEntityTypeId ?? 0;
            noteType.EntityTypeQualifierColumn = "";
            noteType.EntityTypeQualifierValue = "";
            noteType.UserSelectable = cbUserSelectable.Checked;
            noteType.CssClass = tbCssClass.Text;
            noteType.IconCssClass = tbIconCssClass.Text;

            if ( noteType.IsValid )
            {
                rockContext.SaveChanges();

                NoteTypeCache.Flush( noteType.Id );
                NoteTypeCache.FlushEntityNoteTypes();

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();

                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var rockContext = new RockContext();
            var entityTypes = new EntityTypeService( rockContext ).GetEntities()
                .OrderBy( t => t.FriendlyName )
                .ToList();
            entityTypePicker.EntityTypes = entityTypes;

            // Load Entity Type Filter
            var noteTypeEntities = new NoteTypeService( rockContext ).Queryable()
                .Select( c => c.EntityTypeId )
                .Distinct()
                .ToList();

            entityTypeFilter.EntityTypes = entityTypes.Where( e => noteTypeEntities.Contains( e.Id ) ).ToList();
            entityTypeFilter.SetValue( rFilter.GetUserPreference( "EntityType" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int? entityTypeId = entityTypeFilter.SelectedValueAsInt( false );

            nbOrdering.Visible = entityTypeId.HasValue;

            rGrid.Columns.OfType<ReorderField>().FirstOrDefault().Visible = entityTypeId.HasValue;
            rGrid.DataSource = GetNoteTypes().ToList();
            rGrid.DataBind();
        }

        private IQueryable<NoteType> GetNoteTypes( RockContext rockContext = null )
        {
            int? entityTypeId = entityTypeFilter.SelectedValueAsInt( false );

            rockContext = rockContext ?? new RockContext();
            var unorderedNoteTypes = GetUnorderedNoteTypes( entityTypeId, rockContext );
            return unorderedNoteTypes.OrderBy( a => a.EntityType.Name ).ThenBy( a => a.Order ).ThenBy( a => a.Name );
        }

        private IQueryable<NoteType> GetUnorderedNoteTypes( int? entityTypeId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var queryable = new NoteTypeService( rockContext ).Queryable();
            if ( entityTypeId.HasValue )
            {
                queryable = queryable.Where( t => t.EntityTypeId == entityTypeId.Value );
            }

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int? noteTypeId )
        {
            NoteType noteType = null;
            if ( noteTypeId.HasValue )
            {
                noteType = new NoteTypeService( new RockContext() ).Get( noteTypeId.Value );
            }

            if ( noteType != null )
            {
                tbName.Text = noteType.Name;
                cbUserSelectable.Checked = noteType.UserSelectable;
                tbCssClass.Text = noteType.CssClass;
                tbIconCssClass.Text = noteType.IconCssClass;
                entityTypePicker.SelectedEntityTypeId = noteType.EntityTypeId;
            }
            else
            {
                tbName.Text = string.Empty;
                cbUserSelectable.Checked = true;
                tbCssClass.Text = string.Empty;
                tbIconCssClass.Text = string.Empty;
                entityTypePicker.SelectedEntityTypeId = entityTypeFilter.SelectedValueAsInt( false );
            }

            hfIdValue.Value = noteTypeId.ToString();
            modalDetails.Show();
        }

        #endregion
    }
}