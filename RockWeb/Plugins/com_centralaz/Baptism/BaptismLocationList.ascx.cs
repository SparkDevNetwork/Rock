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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Baptism
{
    [DisplayName( "Baptism Location List" )]
    [Category( "com_centralaz > Baptism" )]
    [Description( "Lists all baptism locations." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [BooleanField( "Display Member Count Column", "Should the Member Count column be displayed? Does not affect lists with a person context.", true, "", 7 )]
    public partial class BaptismLocationList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBaptismLocations.DataKeyNames = new string[] { "Id" };
            gBaptismLocations.GridRebind += gBaptismLocations_GridRebind;

            this.BlockUpdated += GroupList_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlGroupList );
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

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the GroupList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void GroupList_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #region Grid Events

        /// <summary>
        /// Handles the RowSelected event of the gBaptismLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBaptismLocations_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gBaptismLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBaptismLocations_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );
            AuthService authService = new AuthService( rockContext );
            Group group = groupService.Get( e.RowKeyId );

            if ( group != null )
            {
                if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdGridWarning.Show( "You are not authorized to delete this group", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !groupService.CanDelete( group, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Role.Flush( group.Id );
                    foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                    {
                        authService.Delete( auth );
                    }
                }

                groupService.Delete( group );

                rockContext.SaveChanges();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Flush();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            UpdateBaptismLocations();
            pnlUpdate.Visible = false;
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBaptismLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gBaptismLocations_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            CheckForUpdates();
            var rockContext = new RockContext();

            SortProperty sortProperty = gBaptismLocations.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "Name", SortDirection.Ascending ) );
            }

            int baptismLocationGroupTypeId = new GroupTypeService( new RockContext() ).Get( "32F8592C-AE11-44A7-A053-DE43789811D9".AsGuid() ).Id;
            var qryGroups = new GroupService( rockContext ).Queryable().Where( g => g.GroupTypeId == baptismLocationGroupTypeId );

            gBaptismLocations.DataSource = qryGroups.Select( g => new
                    {
                        Id = g.Id,
                        Name = g.Name,
                        MemberCount = g.Members.Count()
                    } )
                .Sort( sortProperty )
                .ToList();

            gBaptismLocations.DataBind();
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        private void CheckForUpdates()
        {
            RockContext rockContext = new RockContext();
            CategoryService categoryService = new CategoryService( rockContext );
            GroupService groupService = new GroupService( rockContext );
            bool needsUpdate = false;

            List<Category> categoryList = categoryService.Queryable().ToList();
            List<Group> groupList = groupService.Queryable().ToList();
            foreach ( var campus in CampusCache.All() )
            {
                Category blackout = categoryList.Where( c => c.Name.Equals( String.Format( "{0} Blackout", campus.Name ) ) ).FirstOrDefault();
                if ( blackout == null )
                {
                    needsUpdate = true;
                }

                Category serviceTimes = categoryList.Where( c => c.Name.Equals( String.Format( "{0} Service Times", campus.Name ) ) ).FirstOrDefault();
                if ( serviceTimes == null )
                {
                    needsUpdate = true;

                }

                Group baptismLocationGroup = groupList.Where( g => g.Name.Equals( String.Format( "{0} Baptism Schedule", campus.Name ) ) ).FirstOrDefault();
                if ( baptismLocationGroup == null )
                {
                    needsUpdate = true;
                }
            }

            if ( needsUpdate )
            {
                pnlUpdate.Visible = true;
            }
        }

        /// <summary>
        /// Updates the baptism locations.
        /// </summary>
        private void UpdateBaptismLocations()
        {
            RockContext rockContext = new RockContext();
            CategoryService categoryService = new CategoryService( rockContext );
            GroupService groupService = new GroupService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );

            int baptismLocationGroupTypeId = new GroupTypeService( rockContext ).Get( "32F8592C-AE11-44A7-A053-DE43789811D9".AsGuid() ).Id;
            int blackoutParentCategoryId = categoryService.Get( "FFC06491-1BE9-4F3B-AC76-81A47E17D0AE".AsGuid() ).Id;
            int serviceTimesParentCategoryId = categoryService.Queryable().Where( c => c.Name.Equals( "Service Times" ) && c.ParentCategoryId == null ).FirstOrDefault().Id;
            int scheduleEntityTypeId = new EntityTypeService( rockContext ).Get( "0B2C38A7-D79C-4F85-9757-F1B045D32C8A".AsGuid() ).Id;
            int blackoutDatesAttributeId = attributeService.Get( "D58F0DB5-09AA-4A5A-BC17-CE3E3985D6F8".AsGuid() ).Id;
            int serviceTimesAttributeId = attributeService.Get( "B7371337-57CB-4CB3-994C-72258729950F".AsGuid() ).Id;

            List<Category> categoryList = categoryService.Queryable().ToList();
            List<Group> groupList = groupService.Queryable().ToList();
            foreach ( var campus in CampusCache.All() )
            {
                Category blackout = categoryList.Where( c => c.Name.Equals( String.Format( "{0} Blackout", campus.Name ) ) ).FirstOrDefault();
                if ( blackout == null )
                {
                    blackout = new Category();
                    blackout.IsSystem = false;
                    blackout.ParentCategoryId = blackoutParentCategoryId;
                    blackout.EntityTypeId = scheduleEntityTypeId;
                    blackout.Name = String.Format( "{0} Blackout", campus.Name );
                    blackout.IconCssClass = "fa fa-ban";
                    categoryService.Add( blackout );
                }

                Category serviceTimes = categoryList.Where( c => c.Name.Equals( String.Format( "{0} Service Times", campus.Name ) ) ).FirstOrDefault();
                if ( serviceTimes == null )
                {
                    serviceTimes = new Category();
                    serviceTimes.IsSystem = false;
                    serviceTimes.ParentCategoryId = blackoutParentCategoryId;
                    serviceTimes.EntityTypeId = scheduleEntityTypeId;
                    serviceTimes.Name = String.Format( "{0} Service Times", campus.Name );
                    serviceTimes.IconCssClass = "fa fa-bell";
                    categoryService.Add( serviceTimes );
                }

                Group baptismLocationGroup = groupList.Where( g => g.Name.Equals( String.Format( "{0} Baptism Schedule", campus.Name ) ) ).FirstOrDefault();
                if ( baptismLocationGroup == null )
                {
                    baptismLocationGroup = new Group();
                    baptismLocationGroup.IsSystem = false;
                    baptismLocationGroup.GroupTypeId = baptismLocationGroupTypeId;
                    baptismLocationGroup.CampusId = campus.Id;
                    baptismLocationGroup.Name = String.Format( "{0} Baptism Schedule", campus.Name );
                    baptismLocationGroup.IsSecurityRole = false;
                    baptismLocationGroup.IsActive = true;
                    baptismLocationGroup.Order = 0;
                    groupService.Add( baptismLocationGroup );
                    rockContext.SaveChanges();

                    AttributeValue blackoutAttributeValue = new AttributeValue();
                    blackoutAttributeValue.IsSystem = false;
                    blackoutAttributeValue.AttributeId = blackoutDatesAttributeId;
                    blackoutAttributeValue.Value = blackout.Guid.ToString();
                    blackoutAttributeValue.EntityId = baptismLocationGroup.Id;
                    attributeValueService.Add( blackoutAttributeValue );

                    AttributeValue serviceTimesAttributeValue = new AttributeValue();
                    serviceTimesAttributeValue.IsSystem = false;
                    serviceTimesAttributeValue.AttributeId = serviceTimesAttributeId;
                    serviceTimesAttributeValue.Value = serviceTimes.Guid.ToString();
                    serviceTimesAttributeValue.EntityId = baptismLocationGroup.Id;
                    attributeValueService.Add( serviceTimesAttributeValue );
                }

                rockContext.SaveChanges();
            }
        }

        #endregion
    }
}