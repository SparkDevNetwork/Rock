// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
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
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_secc.Administration
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [DisplayName( "Group Type Changer" )]
    [Category( "SECC > Administration" )]
    [Description( "Tool to change the group type of a group." )]

    [BooleanField( "Use Field Type Instead of Key", "Should the field type of each attribute be displayed instead of the attribute's key?", false, "", 0, "UseFieldType" )]

    public partial class GroupTypeChanger : RockBlock
    {

        Group group;
        RockContext rockContext;
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbSuccess.Visible = false;
            var groupId = PageParameter( "GroupId" ).AsInteger();
            if ( groupId != 0 )
            {
                rockContext = new RockContext();
                group = new GroupService( rockContext ).Get( groupId );
                if ( group != null )
                {
                    ltName.Text = string.Format( "<span style='font-size:1.5em;'>{0}</span>", group.Name );
                    ltGroupTypeName.Text = string.Format( "<span style='font-size:1.5em;'>{0}</span>", group.GroupType.Name );
                }
            }

            if ( !Page.IsPostBack )
            {
                BindGroupTypeDropDown();
            }
            else
            {
                var groupTypeId = ddlGroupTypes.SelectedValue.AsInteger();
                if ( groupTypeId == 0 || group == null )
                {
                    return;
                }
                var newGroupType = new GroupTypeService( new RockContext() ).Get( groupTypeId );
                BindRoles( newGroupType, group.GroupType.Roles );
                DisplayMemberAttributes();
                DisplayGroupAttributes();
            }

        }

        private void BindGroupTypeDropDown()
        {
            var groupTypes = new GroupTypeService( new RockContext() ).Queryable()
                .Select( gt => new
                {
                    Id = gt.Id,
                    Name = gt.Name
                } ).ToList();

            groupTypes.Insert( 0, new { Id = 0, Name = "" } );

            ddlGroupTypes.DataSource = groupTypes;
            ddlGroupTypes.DataBind();
        }

        protected void ddlGroupTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            var groupTypeId = ddlGroupTypes.SelectedValue.AsInteger();
            if ( groupTypeId != 0 && group != null )
            {
                btnSave.Visible = true;
                var newGroupType = new GroupTypeService( rockContext ).Get( groupTypeId );

                BindRoles( newGroupType, group.GroupType.Roles );
                DisplayMemberAttributes();
                DisplayGroupAttributes();
            }
            else
            {
                pnlMemberAttributes.Visible = false;
                pnlGroupAttributes.Visible = false;
                pnlRoles.Visible = false;
                btnSave.Visible = false;
            }
        }

        private void BindRoles( GroupType newGroupType, ICollection<GroupTypeRole> roles )
        {
            if ( roles.Any() && newGroupType != null )
            {
                pnlRoles.Visible = true;
                phRoles.Controls.Clear();
                foreach ( var role in roles )
                {
                    RockDropDownList ddlRole = new RockDropDownList()
                    {
                        DataTextField = "Name",
                        DataValueField = "Id",
                        Label = role.Name,
                        ID = role.Id.ToString() + "_ddlRole"
                    };
                    BindRoleDropDown( newGroupType, ddlRole );

                    // Automatically match up roles with the same name
                    foreach ( var selectableRole in newGroupType.Roles )
                    {
                        if ( role.Name == selectableRole.Name )
                        {
                            ddlRole.SetValue( selectableRole.Id );
                        }
                    }

                    phRoles.Controls.Add( ddlRole );
                }
            }
            else
            {
                pnlRoles.Visible = false;
            }
        }

        private void DisplayMemberAttributes()
        {
            phMemberAttributes.Controls.Clear();

            var newGroupTypeId = ddlGroupTypes.SelectedValue.AsInteger();

            if ( newGroupTypeId == 0 )
            {
                pnlMemberAttributes.Visible = false;
                return;
            }

            var attributeService = new AttributeService( rockContext );

            var groupMemberEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() ).Id;
            var stringGroupTypeId = group.GroupTypeId.ToString();

            var attributes = attributeService.Queryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn == "GroupTypeId"
                    && a.EntityTypeQualifierValue == stringGroupTypeId
                    && a.EntityTypeId == groupMemberEntityId
                    ).ToList();
            if ( attributes.Any() )
            {
                bool useFieldType = GetAttributeValue( "UseFieldType" ).AsBoolean();
                var newGroupTypeIdString = newGroupTypeId.ToString();
                var selectableAttributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupTypeId"
                        && a.EntityTypeQualifierValue == newGroupTypeIdString
                        && a.EntityTypeId == groupMemberEntityId
                        )
                    .ToList()
                    .Select( a => new
                    {
                        Id = a.Id.ToString(),
                        Name = a.Name + " [" + ( useFieldType ? a.FieldType.Name : a.Key ) + "]"
                    } )
                    .ToList();

                // Inserts a blank option so that transferring an attribute is not required
                selectableAttributes.Insert( 0, new { Id = "0", Name = "" } );

                pnlMemberAttributes.Visible = true;
                foreach ( var attribute in attributes )
                {
                    RockDropDownList ddlAttribute = new RockDropDownList()
                    {
                        ID = attribute.Id.ToString() + "_ddlAttribute",
                        Label = attribute.Name + ( useFieldType ? $" [{attribute.FieldType.Name}]" : "" ),
                        DataValueField = "Id",
                        DataTextField = "Name"
                    };
                    ddlAttribute.DataSource = selectableAttributes;
                    ddlAttribute.DataBind();

                    // Automatically match up attributes with ones with the same name
                    foreach ( var selectableAttribute in selectableAttributes )
                    {
                        if ( attribute.Name == selectableAttribute.Name.Split( '[' )[0].Trim() )
                        {
                            ddlAttribute.SetValue( selectableAttribute.Id );
                        }
                    }

                    phMemberAttributes.Controls.Add( ddlAttribute );
                }
            }
            else
            {
                pnlMemberAttributes.Visible = false;
            }
        }

        private void DisplayGroupAttributes()
        {
            phGroupAttributes.Controls.Clear();

            var newGroupTypeId = ddlGroupTypes.SelectedValue.AsInteger();

            if ( newGroupTypeId == 0 )
            {
                pnlGroupAttributes.Visible = false;
                return;
            }

            var attributeService = new AttributeService( rockContext );

            var groupEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP.AsGuid() ).Id;
            var stringGroupTypeId = group.GroupTypeId.ToString();

            var attributes = attributeService.Queryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn == "GroupTypeId"
                    && a.EntityTypeQualifierValue == stringGroupTypeId
                    && a.EntityTypeId == groupEntityId
                    ).ToList();
            if ( attributes.Any() )
            {
                bool useFieldType = GetAttributeValue( "UseFieldType" ).AsBoolean();
                var newGroupTypeIdString = newGroupTypeId.ToString();
                var selectableAttributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupTypeId"
                        && a.EntityTypeQualifierValue == newGroupTypeIdString
                        && a.EntityTypeId == groupEntityId
                        )
                    .ToList()
                    .Select( a => new
                    {
                        Id = a.Id.ToString(),
                        Name = a.Name + " [" + ( useFieldType ? a.FieldType.Name : a.Key ) + "]"
                    } )
                    .ToList();

                // Inserts a blank option so that transferring an attribute is not required
                selectableAttributes.Insert( 0, new { Id = "0", Name = "" } );

                pnlGroupAttributes.Visible = true;
                foreach ( var attribute in attributes )
                {
                    RockDropDownList ddlAttribute = new RockDropDownList()
                    {
                        ID = attribute.Id.ToString() + "_ddlAttribute",
                        Label = attribute.Name + ( useFieldType ? $" [{attribute.FieldType.Name}]" : "" ),
                        DataValueField = "Id",
                        DataTextField = "Name"
                    };
                    ddlAttribute.DataSource = selectableAttributes;
                    ddlAttribute.DataBind();

                    // Automatically match up attributes with ones with the same name
                    foreach ( var selectableAttribute in selectableAttributes )
                    {
                        if ( attribute.Name == selectableAttribute.Name.Split( '[' )[0].Trim() )
                        {
                            ddlAttribute.SetValue( selectableAttribute.Id );
                        }
                    }

                    phGroupAttributes.Controls.Add( ddlAttribute );
                }
            }
            else
            {
                pnlGroupAttributes.Visible = false;
            }
        }

        private void BindRoleDropDown( GroupType newGroupType, RockDropDownList ddlRole )
        {
            ddlRole.DataSource = newGroupType.Roles;
            ddlRole.DataBind();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var transaction = rockContext.Database.BeginTransaction() )
            {
                // Get the old groupTypeId before we change it
                var stringGroupTypeId = group.GroupTypeId.ToString();

                // Map group roles
                group.GroupTypeId = ddlGroupTypes.SelectedValue.AsInteger();
                var groupMembers = group.Members;
                foreach ( var role in group.GroupType.Roles )
                {
                var ddlRole = ( RockDropDownList ) phRoles.FindControl( role.Id.ToString() + "_ddlRole" );
                    var roleMembers = groupMembers.Where( gm => gm.GroupRoleId == role.Id );
                    foreach ( var member in roleMembers )
                    {
                        member.GroupRoleId = ddlRole.SelectedValue.AsInteger();
                    }
                }

                // Map group member group type id
                int groupTypeId = group.GroupTypeId;
                foreach ( var member in groupMembers )
                {
                    member.GroupTypeId = groupTypeId;
                }
                rockContext.SaveChanges( true );

                // Map attributes
                var attributeService = new AttributeService( rockContext );
                var attributeValueService = new AttributeValueService( rockContext );

                Dictionary<string, int> memberDeletedAttributes = new Dictionary<string, int>();
                Dictionary<string, string> groupDeletedAttributes = new Dictionary<string, string>();

                // Map group member attributes
                var groupMemberEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() ).Id;

                var attributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupTypeId"
                        && a.EntityTypeQualifierValue == stringGroupTypeId
                        && a.EntityTypeId == groupMemberEntityId
                        ).ToList();

                // Delete conflicting attribute values
                foreach ( var attribute in attributes )
                {
                    var ddlAttribute = ( RockDropDownList )phMemberAttributes.FindControl( attribute.Id.ToString() + "_ddlAttribute" );
                    // If the dropdown is found
                    if ( ddlAttribute != null )
                    {
                        var newAttributeId = ddlAttribute.SelectedValue.AsInteger();
                        // For every group member in the group
                        foreach ( var member in groupMembers )
                        {
                            var attributeEntity = attributeValueService.Queryable()
                                .Where( av => av.EntityId == member.Id && av.AttributeId == attribute.Id )
                                .FirstOrDefault();
                            // If the attribute value is found
                            if ( attributeEntity != null )
                            {
                                // Delete any existing attribute values that have the same EntityId and the new AttributeId
                                var existingAttributeValues = attributeValueService.Queryable()
                                    .Where( av => av.EntityId == member.Id && av.AttributeId == newAttributeId ).ToList();

                                foreach ( var existingAttributeValue in existingAttributeValues )
                                {
                                    attributeValueService.Delete( existingAttributeValue );
                                }
                            }

                        }
                    }
                }
                rockContext.SaveChanges( true );

                // Move existing attribute values
                foreach ( var attribute in attributes )
                {
                    var ddlAttribute = ( RockDropDownList )phMemberAttributes.FindControl( attribute.Id.ToString() + "_ddlAttribute" );
                    // If the dropdown is found
                    if ( ddlAttribute != null )
                    {
                        var newAttributeId = ddlAttribute.SelectedValue.AsInteger();
                        // For every group member in the group
                        foreach ( var member in groupMembers )
                        {
                            var attributeEntity = attributeValueService.Queryable()
                                .Where( av => av.EntityId == member.Id && av.AttributeId == attribute.Id )
                                .FirstOrDefault();
                            // If the attribute value is found
                            if ( attributeEntity != null )
                            {
                                // If a new attribute mapping was selected
                                if ( newAttributeId != 0 )
                                {
                                    // Map attribute value to new attribute
                                    attributeEntity.AttributeId = newAttributeId;
                                }
                                else
                                {
                                    // Remove inapplicable attribute value

                                    // Count how many records were affected for each attribute (don't count empty records)
                                    if ( attributeEntity.Value != "" && attributeEntity.Value != null )
                                    {
                                        int currentCount;
                                        memberDeletedAttributes.TryGetValue( attributeEntity.AttributeName, out currentCount );
                                        memberDeletedAttributes[attributeEntity.AttributeName] = currentCount + 1;
                                    }

                                    attributeValueService.Delete( attributeEntity );
                                }
                            }

                        }
                    }
                }
                rockContext.SaveChanges( true );

                // Map group attributes
                var groupEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP.AsGuid() ).Id;

                var groupAttributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupTypeId"
                        && a.EntityTypeQualifierValue == stringGroupTypeId
                        && a.EntityTypeId == groupEntityId
                        ).ToList();

                // Delete conflicting attribute values
                foreach ( var attribute in groupAttributes )
                {
                    var ddlAttribute = ( RockDropDownList )phMemberAttributes.FindControl( attribute.Id.ToString() + "_ddlAttribute" );
                    // If the dropdown is found
                    if ( ddlAttribute != null )
                    {
                        var newAttributeId = ddlAttribute.SelectedValue.AsInteger();

                        var attributeEntity = attributeValueService.Queryable()
                            .Where( av => av.EntityId == group.Id && av.AttributeId == attribute.Id )
                            .FirstOrDefault();
                        // If the attribute value is found
                        if ( attributeEntity != null )
                        {
                            // Delete any existing attribute values that have the same EntityId and the new AttributeId
                            var existingAttributeValues = attributeValueService.Queryable()
                                .Where( av => av.EntityId == group.Id && av.AttributeId == newAttributeId ).ToList();

                            foreach ( var existingAttributeValue in existingAttributeValues )
                            {
                                attributeValueService.Delete( existingAttributeValue );
                            }
                        }
                    }
                }
                rockContext.SaveChanges( true );

                // Move existing attribute values
                foreach ( var attribute in groupAttributes )
                {
                    var ddlAttribute = ( RockDropDownList )phMemberAttributes.FindControl( attribute.Id.ToString() + "_ddlAttribute" );
                    // If the dropdown is found
                    if ( ddlAttribute != null )
                    {
                        var newAttributeId = ddlAttribute.SelectedValue.AsInteger();

                        var attributeEntity = attributeValueService.Queryable()
                            .Where( av => av.EntityId == group.Id && av.AttributeId == attribute.Id )
                            .FirstOrDefault();
                        // If the attribute value is found
                        if ( attributeEntity != null )
                        {
                            // If a new attribute mapping was selected
                            if ( newAttributeId != 0 )
                            {
                                // Map attribute value to new attribute
                                attributeEntity.AttributeId = newAttributeId;
                            }
                            else
                            {
                                // Remove inapplicable attribute value and
                                // count how many records were affected (don't count empty records)
                                if ( attributeEntity.Value != "" && attributeEntity.Value != null )
                                {
                                    groupDeletedAttributes.Add( attributeEntity.AttributeName, attributeEntity.Value );
                                }
                                attributeValueService.Delete( attributeEntity );
                            }
                        }
                    }
                }
                rockContext.SaveChanges( true );

                if ( memberDeletedAttributes.Count > 0 || groupDeletedAttributes.Count > 0 )
                {
                    nbSuccess.Text = "<br>Some attributes were not mapped to new attributes and have been automatically deleted.<br>";
                    if ( memberDeletedAttributes.Count > 0 )
                    {
                        nbSuccess.Text += "<b>Deleted Group Member Attributes</b><br><ul>";
                        foreach ( var deletedAttribute in memberDeletedAttributes )
                        {
                        nbSuccess.Text += $"<li>{ deletedAttribute.Key }<ul><li>Records Affected: { deletedAttribute.Value }</li></ul></li>";
                        }
                        nbSuccess.Text += "</ul>";
                    }
                    if ( groupDeletedAttributes.Count > 0 )
                    {
                        nbSuccess.Text += "<b>Deleted Group Attributes</b><br><ul>";
                        foreach ( var deletedAttribute in groupDeletedAttributes )
                        {
                            nbSuccess.Text += $"<li>{deletedAttribute.Key}<ul><li>Value: {deletedAttribute.Value}</li></ul></li>";
                        }
                        nbSuccess.Text += "</ul>";
                    }
                }

                transaction.Commit();
                nbSuccess.Visible = true;
            }
        }
    }
}