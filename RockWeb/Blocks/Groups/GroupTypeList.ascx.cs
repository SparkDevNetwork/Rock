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
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Type List" )]
    [Category( "Groups" )]
    [Description( "Lists all group types with filtering by purpose and system group types." )]

    [LinkedPage( "Detail Page" )]
    public partial class GroupTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            gGroupType.DataKeyNames = new string[] { "Id" };
            gGroupType.Actions.ShowAdd = true;
            gGroupType.Actions.AddClick += gGroupType_Add;
            gGroupType.GridReorder += gGroupType_GridReorder;
            gGroupType.GridRebind += gGroupType_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            gGroupType.Actions.ShowAdd = canEditBlock;
            gGroupType.IsDeleteEnabled = canEditBlock;

            // Only display reordering column if user can edit the block
            gGroupType.ColumnsOfType<ReorderField>().First().Visible = canEditBlock;

            SecurityField securityField = gGroupType.ColumnsOfType<SecurityField>().First();
            securityField.EntityTypeId = EntityTypeCache.GetId<Rock.Model.GroupType>().Value;

            BindFilter();
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

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Purpose", ddlPurpose.SelectedValue );
            rFilter.SaveUserPreference( "System Group Types", ddlIsSystem.SelectedValue );
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
                case "Purpose":

                    int? id = e.Value.AsIntegerOrNull();
                    if ( id.HasValue )
                    {
                        var purpose = DefinedValueCache.Read( id.Value );
                        if ( purpose != null )
                        {
                            e.Value = purpose.Value;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Add event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            GroupType groupType = groupTypeService.Get( e.RowKeyId );

            if ( groupType != null )
            {
                int groupTypeId = groupType.Id;

                if ( !groupType.IsAuthorized( "Administrate", CurrentPerson ) )
                {
                    mdGridWarning.Show( "Sorry, you're not authorized to delete this group type.", ModalAlertType.Alert );
                    return;
                }

                string errorMessage;
                if ( !groupTypeService.CanDelete( groupType, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                groupType.ChildGroupTypes.Clear();

                groupTypeService.Delete( groupType );
                rockContext.SaveChanges();

                GroupTypeCache.Flush( groupTypeId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gGroupType_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();

            var groupTypes = GetGroupTypes( rockContext );
            if ( groupTypes != null )
            {
                new GroupTypeService( rockContext ).Reorder( groupTypes.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroupType_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlPurpose.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE ) ), true );
            ddlPurpose.SelectedValue = rFilter.GetUserPreference( "Purpose" );
            ddlIsSystem.SelectedValue = rFilter.GetUserPreference( "System Group Types" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var selectQry = GetGroupTypes( new RockContext() )
                .Select( a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    Purpose = a.GroupTypePurposeValue.Value,
                    GroupsCount = a.Groups.Count(),
                    a.ShowInNavigation,
                    a.IsSystem
                } );

            gGroupType.EntityTypeId = EntityTypeCache.GetId<GroupType>();
            gGroupType.DataSource = selectQry.ToList();
            gGroupType.DataBind();
        }

        /// <summary>
        /// Gets the group types.
        /// </summary>
        /// <returns></returns>
        private IQueryable<GroupType> GetGroupTypes( RockContext rockContext )
        {
            var qry = new GroupTypeService( rockContext ).Queryable();

            int? purposeId = rFilter.GetUserPreference( "Purpose" ).AsIntegerOrNull();
            if ( purposeId.HasValue )
            {
                qry = qry.Where( t => t.GroupTypePurposeValueId == purposeId.Value );
            }

            var isSystem = rFilter.GetUserPreference( "System Group Types" );
            if ( isSystem == "Yes" )
            {
                qry = qry.Where( t => t.IsSystem );
            }
            else if ( isSystem == "No" )
            {
                qry = qry.Where( t => !t.IsSystem );
            }

            return qry.OrderBy( g => g.Order );
        }

        #endregion
    }
}