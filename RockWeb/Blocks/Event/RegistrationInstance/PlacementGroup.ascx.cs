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

namespace RockWeb.Blocks.Event.RegistrationInstance
{
    [DisplayName( "Registration Instance Placement Group" )]
    [Category( "Event" )]
    [Description( "Block to manage group placement for Registration Instances." )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class PlacementGroup : RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            //public const string ShowEmailAddress = "ShowEmailAddress";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string PlacementGroupTypeId = "GroupTypeId";
        }

        #endregion PageParameterKeys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            // maybe JSON??
            public const string PlacementConfigurationJSON = "PlacementConfigurationJSON";
        }

        #endregion PageParameterKeys

        #region Fields
        

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js", true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                ShowDetails();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            hfRegistrationTemplateId.Value = this.PageParameter( PageParameterKey.RegistrationTemplateId );
            hfRegistrationInstanceId.Value = this.PageParameter( PageParameterKey.RegistrationInstanceId );
            hfPlacementGroupTypeId.Value = this.PageParameter( PageParameterKey.PlacementGroupTypeId );

            var rockContext = new RockContext();

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            var registrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();

            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var groupTypeId = hfPlacementGroupTypeId.Value.AsIntegerOrNull();


            List<Rock.Model.RegistrationInstance> registrationInstanceList = null;

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                registrationInstanceList = new List<Rock.Model.RegistrationInstance>();
                registrationInstanceList.Add( registrationInstance );
                registrationTemplateId = registrationInstance.RegistrationTemplateId;
            }
            else if ( registrationTemplateId.HasValue )
            {
                registrationInstanceList = registrationInstanceService.Queryable().Where( a => a.RegistrationTemplateId == registrationTemplateId.Value ).OrderBy( a => a.Name ).ToList();
            }

            if ( registrationTemplateId.HasValue )
            {
                var debugPlacementGroupTypes = registrationTemplateService.Get( registrationTemplateId.Value ).Placements.Select( a => a.GroupType ).ToList();
                ddlDebugGroupType.Items.Clear();
                ddlDebugGroupType.Items.Add( new ListItem() );
                foreach ( var debugPlacementGroupType in debugPlacementGroupTypes )
                {
                    ddlDebugGroupType.Items.Add( new ListItem( debugPlacementGroupType.Name, debugPlacementGroupType.Id.ToString() ) );
                }

                if ( groupTypeId.HasValue )
                {
                    ddlDebugGroupType.SetValue( groupTypeId.Value );
                }
            }


            var placementGroupList = new List<Group>();
            if ( registrationInstanceList != null && groupTypeId.HasValue )
            {
                foreach ( var registrationInstance in registrationInstanceList )
                {
                    var placementGroups = registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstance ).Where( a => a.GroupTypeId == groupTypeId.Value ).ToList();
                    foreach ( var placementGroup in placementGroups )
                    {
                        placementGroupList.Add( placementGroup, true );
                    }
                }
            }

            RegistrationTemplatePlacement registrationTemplatePlacement = null;
            GroupTypeCache groupType = null;

            if ( groupTypeId.HasValue )
            {
                groupType = GroupTypeCache.Get( groupTypeId.Value );
                registrationTemplatePlacement = new RegistrationTemplatePlacementService( rockContext ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.RegistrationTemplateId == registrationTemplateId.Value ).FirstOrDefault();
            }

            if ( registrationTemplatePlacement == null || groupType == null )
            {
                nbConfigurationError.Text = "Invalid Group Type Specified";
                nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbConfigurationError.Visible = true;

                return;
            }

            if ( registrationTemplatePlacement.IconCssClass.IsNotNullOrWhiteSpace() )
            {
                lGroupPlacementGroupTypeIconHtml.Text = string.Format( "<i class='{0}'></i>", registrationTemplatePlacement.IconCssClass );
            }
            else
            {
                lGroupPlacementGroupTypeIconHtml.Text = string.Format( "<i class='{0}'></i>", groupType.IconCssClass );
            }

            lGroupPlacementGroupTypeName.Text = groupType.GroupTerm.Pluralize();
            lAddPlacementGroupButtonText.Text = string.Format( "Add {0}", groupType.GroupTerm );

            _groupTypeRoles = groupType.Roles.ToList();

            rptPlacementGroups.DataSource = placementGroupList;
            rptPlacementGroups.DataBind();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void btnAddPlacementGroup_Click( object sender, EventArgs e )
        {

        }

        protected void btnConfiguration_Click( object sender, EventArgs e )
        {
            mdPlacementConfiguration.Show();
        }

        private List<GroupTypeRoleCache> _groupTypeRoles;

        protected void rptPlacementGroups_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var placementGroup = e.Item.DataItem as Group;
            if ( placementGroup == null )
            {
                return;
            }

            var hfPlacementGroupId = e.Item.FindControl( "hfPlacementGroupId" ) as HiddenFieldWithClass;
            hfPlacementGroupId.Value = placementGroup.Id.ToString();

            var hfPlacementGroupCapacity = e.Item.FindControl( "hfPlacementGroupCapacity" ) as HiddenFieldWithClass;
            hfPlacementGroupCapacity.Value = placementGroup.GroupCapacity.ToString();

            var lGroupName = e.Item.FindControl( "lGroupName" ) as Literal;
            lGroupName.Text = placementGroup.Name;

            var rptPlacementGroupRole = e.Item.FindControl( "rptPlacementGroupRole" ) as Repeater;
            rptPlacementGroupRole.DataSource = _groupTypeRoles;
            rptPlacementGroupRole.DataBind();
        }

        protected void rptPlacementGroupRole_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var groupTypeRole = e.Item.DataItem as GroupTypeRoleCache;
            if (groupTypeRole == null)
            {
                return;
            }

            var hfGroupTypeRoleId = e.Item.FindControl( "hfGroupTypeRoleId" ) as HiddenFieldWithClass;
            hfGroupTypeRoleId.Value = groupTypeRole.Id.ToString();

            var lGroupRoleName = e.Item.FindControl( "lGroupRoleName" ) as Literal;
            lGroupRoleName.Text = groupTypeRole.Name.Pluralize();
        }

        protected void mdPlacementConfiguration_SaveClick( object sender, EventArgs e )
        {
            // TODO
            mdPlacementConfiguration.Hide();
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDebugGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDebugGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var additionalQueryParameters = new Dictionary<string, string>();
            additionalQueryParameters.Add( PageParameterKey.PlacementGroupTypeId, ddlDebugGroupType.SelectedValue );
            NavigateToCurrentPageReference( additionalQueryParameters );
        }

        
    }
}