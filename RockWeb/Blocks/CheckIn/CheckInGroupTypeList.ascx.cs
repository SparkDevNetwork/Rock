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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Group Type List" )]
    [Category( "Check-in" )]
    [Description( "Lists groups types that are available for checkin." )]
    [LinkedPage( "Schedule Builder Page" )]
    [LinkedPage( "Configure Groups Page", "Page for configuration of Check-in Groups/Locations" )]
    public partial class CheckinGroupTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroupType.DataKeyNames = new string[] { "Id" };
            gGroupType.Actions.ShowAdd = true;
            gGroupType.Actions.AddClick += gGroupType_Add;
            gGroupType.GridRebind += gGroupType_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gGroupType.Actions.ShowAdd = canAddEditDelete;

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
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                GroupTypeService groupTypeService = new GroupTypeService( rockContext );
                SortProperty sortProperty = gGroupType.SortProperty;

                var qry = groupTypeService.Queryable();

                // limit to show only GroupTypes that have a group type purpose of Checkin Template
                int groupTypePurposeCheckInTemplateId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
                qry = qry.Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId );

                if ( sortProperty != null )
                {
                    gGroupType.DataSource = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    gGroupType.DataSource = qry.OrderBy( p => p.Name ).ToList();
                }

                gGroupType.DataBind();
            }
        }

        #endregion

        /// <summary>
        /// Handles the Add event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupType_Add( object sender, EventArgs e )
        {
            hfGroupTypeId.Value = "0";
            tbGroupTypeName.Text = string.Empty;
            tbGroupTypeDescription.Text = string.Empty;
            mdAddEditCheckinGroupType.Show();
        }

        /// <summary>
        /// Handles the RowSelected event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupType_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupType = new GroupTypeService( rockContext ).Get( e.RowKeyId );
                if ( groupType != null )
                {
                    hfGroupTypeId.Value = groupType.Id.ToString();
                    tbGroupTypeName.Text = groupType.Name;
                    tbGroupTypeDescription.Text = groupType.Description;
                    mdAddEditCheckinGroupType.Show();
                }
            }
        }

        /// <summary>
        /// Handles the RowCommand event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void gGroupType_RowCommand( object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e )
        {
            int? rowIndex = ( e.CommandArgument as string ).AsIntegerOrNull();
            if ( rowIndex != null )
            {
                GridViewRow row = ( sender as Grid ).Rows[rowIndex.Value];
                RowEventArgs rowEventArgs = new RowEventArgs( row );

                if ( e.CommandName.Equals( "schedule" ) )
                {
                    Dictionary<string, string> qryParams = new Dictionary<string, string>();
                    qryParams.Add( "groupTypeId", rowEventArgs.RowKeyId.ToString() );
                    this.NavigateToLinkedPage( "ScheduleBuilderPage", qryParams );
                }
                else if ( e.CommandName.Equals( "configure" ) )
                {
                    Dictionary<string, string> qryParams = new Dictionary<string, string>();
                    qryParams.Add( "groupTypeId", rowEventArgs.RowKeyId.ToString() );
                    this.NavigateToLinkedPage( "ConfigureGroupsPage", qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddCheckinGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddCheckinGroupType_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new GroupTypeService( rockContext );
                GroupType groupType;

                if ( hfGroupTypeId.Value.AsInteger() == 0 )
                {
                    groupType = new GroupType();
                    groupTypeService.Add( groupType );
                    groupType.GroupTypePurposeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
                    groupType.ShowInNavigation = false;
                    groupType.ShowInGroupList = false;
                
                    var defaultRole = new GroupTypeRole();
                    defaultRole.Name = "Member";
                    groupType.Roles.Add( defaultRole );
                }
                else
                {
                    groupType = groupTypeService.Get( hfGroupTypeId.Value.AsInteger() );
                }

                groupType.Name = tbGroupTypeName.Text;
                groupType.Description = tbGroupTypeDescription.Text;

                rockContext.SaveChanges();

                // Reload to check for setting default role
                groupType = groupTypeService.Get( groupType.Id );
                if ( groupType != null && !groupType.DefaultGroupRoleId.HasValue && groupType.Roles.Any() )
                {
                    groupType.DefaultGroupRoleId = groupType.Roles.First().Id;
                    rockContext.SaveChanges();
                }
            
            }

            mdAddEditCheckinGroupType.Hide();

            BindGrid();
        }
    }
}