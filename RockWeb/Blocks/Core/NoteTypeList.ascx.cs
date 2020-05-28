// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Note Type List" )]
    [Category( "Core" )]
    [Description( "Allows note types to be managed." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [EntityTypeField( "Entity Type",
        IncludeGlobalAttributeOption = false,
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    public partial class NoteTypeList : RockBlock
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntityType = "EntityType";
        }

        #region fields

        private EntityTypeCache _blockConfigEntityType = null;

        #endregion fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfNoteTypes.ApplyFilterClick += gfNoteTypes_ApplyFilterClick;

            gNoteTypes.DataKeyNames = new string[] { "Id" };
            gNoteTypes.Actions.ShowAdd = true;

            gNoteTypes.Actions.AddClick += gNoteTypes_Add;
            gNoteTypes.GridReorder += gNoteTypes_GridReorder;
            gNoteTypes.GridRebind += gNoteTypes_GridRebind;

            foreach ( var securityField in gNoteTypes.Columns.OfType<SecurityField>() )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( NoteType ) ).Id;
            }

            this.BlockUpdated += NoteTypeList_BlockUpdated;

            ApplyBlockSettings();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the NoteTypeList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NoteTypeList_BlockUpdated( object sender, EventArgs e )
        {
            ApplyBlockSettings();
        }

        /// <summary>
        /// Applies the block settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            Guid? entityTypeGuid = this.GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                _blockConfigEntityType = EntityTypeCache.Get( entityTypeGuid.Value );
            }

            gfNoteTypes.Visible = _blockConfigEntityType == null;
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
                pnlList.Visible = IsUserAuthorized( Authorization.ADMINISTRATE );
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfNoteTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfNoteTypes_ApplyFilterClick( object sender, EventArgs e )
        {
            gfNoteTypes.SaveUserPreference( AttributeKey.EntityType, entityTypeFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Gfs the note types display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfNoteTypes_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case AttributeKey.EntityType:

                    int? entityTypeId = e.Value.AsIntegerOrNull();
                    if ( entityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Get( entityTypeId.Value );
                        if ( entityType != null )
                        {
                            e.Value = entityType.FriendlyName;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Delete event of the gNoteTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gNoteTypes_Delete( object sender, RowEventArgs e )
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
                        int noteTypeId = noteType.Id;

                        service.Delete( noteType );
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
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
        protected void gNoteTypes_Add( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "NoteTypeId", "0" );

            if ( _blockConfigEntityType != null )
            {
                int entityTypeId = _blockConfigEntityType.Id;
                queryParams.Add( "EntityTypeId", entityTypeId.ToString() );
            }

            NavigateToLinkedPage( AttributeKey.DetailPage, queryParams );
        }

        /// <summary>
        /// Handles the Edit event of the gNoteTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gNoteTypes_Edit( object sender, RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "NoteTypeId", e.RowKeyId.ToString() );

            NavigateToLinkedPage( AttributeKey.DetailPage, queryParams );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gNoteTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gNoteTypes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            int? entityTypeId = GetSelectedEntityTypeId();
            var noteTypes = GetNoteTypeQuery( entityTypeId, rockContext ).ToList();
            if ( noteTypes != null )
            {
                new NoteTypeService( rockContext ).Reorder( noteTypes, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var rockContext = new RockContext();

            // Load Entity Type Filter
            var noteTypeEntityTypes = new NoteTypeService( rockContext ).Queryable()
                .AsNoTracking()
                .Select( c => c.EntityType )
                .Where( t => t.IsEntity )
                .Distinct()
                .OrderBy( t => t.FriendlyName )
                .ToList();

            entityTypeFilter.EntityTypes = noteTypeEntityTypes;
            entityTypeFilter.SetValue( gfNoteTypes.GetUserPreference( AttributeKey.EntityType ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int? entityTypeId = GetSelectedEntityTypeId();

            nbOrdering.Visible = !entityTypeId.HasValue;

            var rockContext = new RockContext();
            IQueryable<NoteType> noteTypeQuery = GetNoteTypeQuery( entityTypeId, rockContext );

            gNoteTypes.Columns.OfType<ReorderField>().FirstOrDefault().Visible = entityTypeId.HasValue;

            gNoteTypes.SetLinqDataSource( noteTypeQuery );
            gNoteTypes.DataBind();
        }

        /// <summary>
        /// Gets the selected entity type identifier from either the Block Setting or Grid Filter
        /// </summary>
        /// <returns></returns>
        private int? GetSelectedEntityTypeId()
        {
            int? entityTypeId;
            if ( _blockConfigEntityType != null )
            {
                entityTypeId = _blockConfigEntityType.Id;
            }
            else
            {
                entityTypeId = entityTypeFilter.SelectedEntityTypeId;
            }

            return entityTypeId;
        }

        /// <summary>
        /// Gets the note type query.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<NoteType> GetNoteTypeQuery( int? entityTypeId, RockContext rockContext )
        {
            var noteTypeQuery = new NoteTypeService( rockContext ).Queryable();
            if ( entityTypeId.HasValue )
            {
                noteTypeQuery = noteTypeQuery.Where( t => t.EntityTypeId == entityTypeId.Value );
            }

            var sortProperty = gNoteTypes.SortProperty;
            if ( gNoteTypes.AllowSorting && sortProperty != null )
            {
                noteTypeQuery = noteTypeQuery.Sort( sortProperty );
            }
            else
            {
                noteTypeQuery = noteTypeQuery.OrderBy( a => a.EntityType.Name ).ThenBy( a => a.Order ).ThenBy( a => a.Name );
            }

            return noteTypeQuery;
        }

        #endregion
    }
}