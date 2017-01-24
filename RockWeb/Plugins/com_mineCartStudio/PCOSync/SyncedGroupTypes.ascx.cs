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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.minecartstudio.PCOSync.Model;
using System.Collections.Generic;
using Rock.Model;
using System.Data.Entity;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_mineCartStudio.PCOSync
{
    /// <summary>
    /// Lists all the Group Types that can have groups configured to sync with PCO.
    /// </summary>
    [DisplayName( "Synced Group Type List" )]
    [Category( "Mine Cart Studio > PCO Sync" )]
    [Description( "Lists all the Group Types that can have groups configured to sync with PCO." )]

    public partial class SyncedGroupTypes : RockBlock, ISecondaryBlock
    {
        #region Fields

        FieldTypeCache _pcoAccountFieldType = null;
        EntityTypeCache _groupEntityType = null;

        #endregion

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
            gGroupType.IsDeleteEnabled = canAddEditDelete;
            gGroupType.ShowConfirmDeleteDialog = false;

            mdlgGroupType.SaveClick += MdlgGroupType_SaveClick;
            using ( var rockContext = new RockContext() )
            {
                _pcoAccountFieldType = FieldTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.FieldType.PCO_ACCOUNT.AsGuid(), rockContext );
                _groupEntityType = EntityTypeCache.Read<Rock.Model.Group>( false, rockContext );
            }

            string deleteScript = @"
    $('table.js-grid-grouptypes a.grid-delete-button').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to remove this group type from being able to sync groups to PCO? This could result in all of the people previously synced to be disabled in PCO!', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gGroupType, gGroupType.GetType(), "deleteGroupTypeScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                RebindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Add( object sender, EventArgs e )
        {
            ddlGroupType.Items.Clear();
            ddlGroupType.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                var groupTypeIds = GetGroupTypeIds( rockContext );
                foreach ( var groupType in new GroupTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t => !groupTypeIds.Contains( t.Id ) )
                    .OrderBy( t => t.Name ) )
                {
                    ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
                }
            }

            mdlgGroupType.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the MdlgGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MdlgGroupType_SaveClick( object sender, EventArgs e )
        {
            string qualifierValue = ddlGroupType.SelectedValue;
            if ( !string.IsNullOrWhiteSpace( qualifierValue ) && _pcoAccountFieldType != null && _groupEntityType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var attributeService = new AttributeService( rockContext );
                    var groupTypeAttributes = attributeService.Queryable()
                        .Where( a =>
                            a.EntityTypeId == _groupEntityType.Id &&
                            a.EntityTypeQualifierColumn == "GroupTypeId" &&
                            a.EntityTypeQualifierValue == qualifierValue )
                        .ToList();

                    int order = groupTypeAttributes.Any() ? groupTypeAttributes.Max( a => a.Order ) + 1 : 0;

                    // Add the PCO Account attribute
                    var accountAttribute = groupTypeAttributes
                        .Where( a => a.FieldTypeId == _pcoAccountFieldType.Id )
                        .FirstOrDefault();
                    if ( accountAttribute == null )
                    {
                        accountAttribute = new Rock.Model.Attribute();
                        accountAttribute.EntityTypeId = _groupEntityType.Id;
                        accountAttribute.FieldTypeId = _pcoAccountFieldType.Id;
                        accountAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                        accountAttribute.EntityTypeQualifierValue = qualifierValue;
                        accountAttribute.Key = "PCOAccount";
                        accountAttribute.Name = "PCO Account";
                        accountAttribute.Description = "If members of this group should be synced to Planning Center Online (PCO), select the PCO Account that they should be synced to.";
                        accountAttribute.Order = order++;
                        attributeService.Add( accountAttribute );
                    }

                    // Add the permission level attribute
                    var permissionsType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid(), rockContext );
                    var definedValueFieldType = FieldTypeCache.Read( Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid(), rockContext );
                    if ( permissionsType != null && definedValueFieldType != null )
                    {
                        string permissionsTypeValue = permissionsType.Id.ToString();
                        var permissionAttribute = groupTypeAttributes
                            .Where( a =>
                                a.FieldTypeId == definedValueFieldType.Id &&
                                a.AttributeQualifiers.Any( q => q.Key == "definedtype" && q.Value == permissionsTypeValue ) )
                            .FirstOrDefault();

                        if ( permissionAttribute == null )
                        {
                            permissionAttribute = new Rock.Model.Attribute();
                            permissionAttribute.EntityTypeId = _groupEntityType.Id;
                            permissionAttribute.FieldTypeId = definedValueFieldType.Id;
                            permissionAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                            permissionAttribute.EntityTypeQualifierValue = qualifierValue;
                            permissionAttribute.Key = "PCOPermissionLevel";
                            permissionAttribute.Name = "PCO Permission Level";
                            permissionAttribute.Description = "The Planning Center Online (PCO) permission level to set when syncing members of this group to a PCO Account.";
                            permissionAttribute.Order = order++;

                            permissionAttribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "allowmultiple", Value = "False" } );
                            permissionAttribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "definedtype", Value = permissionsTypeValue } );
                            permissionAttribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "displaydescription", Value = "False" } );

                            attributeService.Add( permissionAttribute );
                        }
                    }

                    AttributeCache.FlushEntityAttributes();

                    rockContext.SaveChanges();

                }
            }

            RebindGrid();

            mdlgGroupType.Hide();
        }

        /// <summary>
        /// Handles the Delete event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Delete( object sender, RowEventArgs e )
        {
            if ( _pcoAccountFieldType != null && _groupEntityType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var attributeService = new AttributeService( rockContext );
                    string qualifierValue = e.RowKeyId.ToString();

                    // Delete the PCO Account attribute
                    foreach ( var attribute in attributeService.Queryable()
                        .Where( a =>
                            a.EntityTypeId == _groupEntityType.Id &&
                            a.FieldTypeId == _pcoAccountFieldType.Id &&
                            a.EntityTypeQualifierColumn == "GroupTypeId" &&
                            a.EntityTypeQualifierValue == qualifierValue )
                        .ToList() )
                    {
                        int attributeId = attribute.Id;
                        attributeService.Delete( attribute );
                        rockContext.SaveChanges();
                        AttributeCache.Flush( attributeId );
                    }

                    // Delete the permission level attribute
                    var permissionsType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid(), rockContext );
                    var definedValueFieldType = FieldTypeCache.Read( Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid(), rockContext );
                    if ( permissionsType != null && definedValueFieldType != null )
                    {
                        string permissionsTypeValue = permissionsType.Id.ToString();
                        foreach ( var attribute in attributeService.Queryable()
                            .Where( a =>
                                a.EntityTypeId == _groupEntityType.Id &&
                                a.FieldTypeId == definedValueFieldType.Id &&
                                a.AttributeQualifiers.Any( q => q.Key == "definedtype" && q.Value == permissionsTypeValue ) &&
                                a.EntityTypeQualifierColumn == "GroupTypeId" &&
                                a.EntityTypeQualifierValue == qualifierValue )
                            .ToList() )
                        {
                            int attributeId = attribute.Id;
                            attributeService.Delete( attribute );
                            rockContext.SaveChanges();
                            AttributeCache.Flush( attributeId );
                        }
                    }

                    AttributeCache.FlushEntityAttributes();

                }
            }

            RebindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupType_GridRebind( object sender, EventArgs e )
        {
            RebindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void RebindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( _pcoAccountFieldType != null && _groupEntityType != null )
                {
                    var groupTypeIds = GetGroupTypeIds( rockContext );
                    var queryable = new GroupTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( t => groupTypeIds.Contains( t.Id ) )
                        .OrderBy( t => t.Name );

                    SortProperty sortProperty = gGroupType.SortProperty;
                    if ( sortProperty != null )
                    {
                        queryable = queryable.Sort( sortProperty );
                    }
                    else
                    {
                        queryable = queryable.OrderBy( a => a.Name );
                    }

                    var groupTypes = new List<GroupTypeHelper>();
                    foreach ( var groupType in queryable )
                    {
                        groupTypes.Add( new GroupTypeHelper( groupType, _groupEntityType, _pcoAccountFieldType, rockContext ) );
                    }

                    gGroupType.DataSource = groupTypes;
                    gGroupType.DataBind();
                }
            }
        }

        /// <summary>
        /// Gets the group type ids.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetGroupTypeIds( RockContext rockContext)
        {
            return new AttributeService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.EntityTypeId == _groupEntityType.Id &&
                    a.FieldTypeId == _pcoAccountFieldType.Id &&
                    a.EntityTypeQualifierColumn == "GroupTypeId" )
                .Select( a => a.EntityTypeQualifierValue )
                .Distinct()
                .ToList()
                .AsIntegerList();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        #region HelperClass

        public class GroupTypeHelper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int GroupCount { get; set; }
            public int MemberCount { get; set; }

            public GroupTypeHelper( GroupType groupType, EntityTypeCache groupEntityType, FieldTypeCache pcoAccountFieldType, RockContext rockContext )
            {
                if ( groupType != null )
                {
                    Id = groupType.Id;
                    Name = groupType.Name;

                    var qualifierValue = groupType.Id.ToString();
                    var groupIds = new AttributeValueService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            v.Attribute.EntityTypeId == groupEntityType.Id &&
                            v.Attribute.FieldTypeId == pcoAccountFieldType.Id &&
                            v.Attribute.EntityTypeQualifierColumn == "GroupTypeId" &&
                            v.Attribute.EntityTypeQualifierValue == qualifierValue &&
                            v.EntityId.HasValue )
                        .Select( v => v.EntityId.Value )
                        .ToList();

                    GroupCount = new GroupService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            groupIds.Contains( m.Id ) &&
                            m.IsActive )
                        .Count();

                    MemberCount = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            groupIds.Contains( m.Group.Id ) &&
                            m.Group.IsActive &&
                            m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Count();
                }
            }

        }

        #endregion
    }
}