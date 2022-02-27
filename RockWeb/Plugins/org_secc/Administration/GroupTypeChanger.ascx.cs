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
                DisplayAttributes();
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
                DisplayAttributes();

            }
            else
            {
                pnlAttributes.Visible = false;
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
                    phRoles.Controls.Add( ddlRole );
                }
            }
            else
            {
                pnlRoles.Visible = false;
            }
        }

        private void DisplayAttributes()
        {
            phAttributes.Controls.Clear();

            var newGroupTypeId = ddlGroupTypes.SelectedValue.AsInteger();

            if ( newGroupTypeId == 0 )
            {
                pnlAttributes.Visible = false;
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
                        Name = a.Name + " [" + a.Key + "]"
                    } )
                    .ToList();

                var groupIdString = group.Id.ToString();
                var groupAttributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupId"
                        && a.EntityTypeQualifierValue == groupIdString
                        && a.EntityTypeId == groupMemberEntityId
                        )
                    .ToList()
                    .Select( a => new
                    {
                        Id = a.Id.ToString(),
                        Name = a.Name + " [" + a.Key + "]"
                    } )
                    .ToList();

                selectableAttributes.AddRange( groupAttributes );


                pnlAttributes.Visible = true;
                foreach ( var attribute in attributes )
                {
                    RockDropDownList ddlAttribute = new RockDropDownList()
                    {
                        ID = attribute.Id.ToString() + "_ddlAttribute",
                        Label = attribute.Name,
                        DataValueField = "Id",
                        DataTextField = "Name"
                    };
                    ddlAttribute.DataSource = selectableAttributes;
                    ddlAttribute.DataBind();
                    phAttributes.Controls.Add( ddlAttribute );
                }
            }
            else
            {
                pnlAttributes.Visible = false;
            }
        }

        private void BindRoleDropDown( GroupType newGroupType, RockDropDownList ddlRole )
        {
            ddlRole.DataSource = newGroupType.Roles;
            ddlRole.DataBind();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            //Get the old groupTypeId before we change it
            var stringGroupTypeId = group.GroupTypeId.ToString();

            //Map group roles
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

            //Map attributes
            var attributeService = new AttributeService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );

            var groupMemberEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() ).Id;

            var attributes = attributeService.Queryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn == "GroupTypeId"
                    && a.EntityTypeQualifierValue == stringGroupTypeId
                    && a.EntityTypeId == groupMemberEntityId
                    ).ToList();
            foreach ( var attribute in attributes )
            {
                var ddlAttribute = ( RockDropDownList ) phAttributes.FindControl( attribute.Id.ToString() + "_ddlAttribute" );
                if ( ddlAttribute != null )
                {
                    var newAttributeId = ddlAttribute.SelectedValue.AsInteger();
                    if ( newAttributeId != 0 )
                    {
                        foreach ( var member in groupMembers )
                        {
                            var attributeEntity = attributeValueService.Queryable()
                                .Where( av => av.EntityId == member.Id && av.AttributeId == attribute.Id )
                                .FirstOrDefault();
                            if ( attributeEntity != null )
                            {
                                attributeEntity.AttributeId = newAttributeId;
                            }
                        }
                    }
                }
            }
            rockContext.SaveChanges();
            nbSuccess.Visible = true;
        }
    }
}