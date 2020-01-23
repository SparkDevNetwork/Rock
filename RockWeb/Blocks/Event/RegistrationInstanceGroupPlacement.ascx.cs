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
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Group Placement" )]
    [Category( "Event" )]
    [Description( "Block to manage group placement for Registration Instances." )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class RegistrationInstanceGroupPlacement : RockBlock
    {

        #region Attribute Keys

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
        }

        #endregion PageParameterKeys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string PlacementConfigurationJSON_RegistrantInstanceId = "PlacementConfigurationJSON_RegistrantInstanceId_{0}";
            public const string PlacementConfigurationJSON_RegistrantTemplateId = "PlacementConfigurationJSON_RegistrantTemplateId_{0}";
        }

        private class PlacementConfiguration
        {
            public PlacementConfiguration()
            {
                ShowRegistrationInstanceName = true;
                IncludedRegistrationInstanceIds = new int[0];
                DisplayedRegistrantAttributeIds = new int[0];
                DisplayedGroupAttributeIds = new int[0];
                DisplayedGroupMemberAttributeIds = new int[0];
            }

            public int? DisplayedCampusId { get; set; }

            public bool ShowRegistrationInstanceName { get; set; }

            public int[] IncludedRegistrationInstanceIds { get; set; }

            public bool HighlightGenders { get; set; }

            public bool ShowFees { get; set; }

            public int[] DisplayedRegistrantAttributeIds { get; set; }

            public int? RegistrantDataFilterId { get; set; }

            public int[] DisplayedGroupAttributeIds { get; set; }

            public int[] DisplayedGroupMemberAttributeIds { get; set; }

            public bool HideFullGroups { get; set; }
        }


        #endregion PageParameterKeys

        #region Helper classes

        private enum AddPlacementGroupTab
        {
            AddNewGroup,
            AddExistingGroup
        }

        #endregion

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
            RockPage.AddCSSLink( "~/Themes/Rock/Styles/group-placement.css", true );

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
            hfRegistrationTemplatePlacementId.Value = this.PageParameter( PageParameterKey.RegistrationTemplatePlacementId );

            var rockContext = new RockContext();

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            var registrationInstanceId = this.PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();
            if ( registrationInstanceId.HasValue )
            {
                hfRegistrationInstanceId.Value = registrationInstanceId.ToString();
            }

            int registrationTemplateId;
            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                registrationTemplateId = registrationInstance.RegistrationTemplateId;
            }
            else
            {
                registrationTemplateId = this.PageParameter( PageParameterKey.RegistrationTemplateId ).AsInteger();
            }

            hfRegistrationTemplateId.Value = registrationTemplateId.ToString();

            var placementConfiguration = GetPlacementConfiguration();

            hfOptionsDataFilterId.Value = placementConfiguration.RegistrantDataFilterId.ToString();

            hfOptionsDisplayedRegistrantAttributeIds.Value = placementConfiguration.DisplayedRegistrantAttributeIds.ToJson();
            hfOptionsDisplayedGroupMemberAttributeKeys.Value = placementConfiguration.DisplayedGroupMemberAttributeIds.Select( a => AttributeCache.Get( a ) ).Where( a => a != null ).Select( a => a.Key ).ToList().AsDelimited( "," );

            hfOptionsIncludeFees.Value = placementConfiguration.ShowFees.ToTrueFalse().ToLower();
            hfOptionsHighlightGenders.Value = placementConfiguration.HighlightGenders.ToTrueFalse().ToLower();
            hfRegistrationTemplateInstanceIds.Value = placementConfiguration.IncludedRegistrationInstanceIds.ToJson();
            hfRegistrationTemplateShowInstanceName.Value = placementConfiguration.ShowRegistrationInstanceName.ToTrueFalse().ToLower();

            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsIntegerOrNull();

            RegistrationTemplatePlacement registrationTemplatePlacement = null;
            int? registrationTemplatePlacementGroupTypeId = null;
            if ( registrationTemplatePlacementId.HasValue )
            {
                registrationTemplatePlacement = new RegistrationTemplatePlacementService( rockContext ).Get( registrationTemplatePlacementId.Value );
                registrationTemplatePlacementGroupTypeId = registrationTemplatePlacement.GroupTypeId;

                hfRegistrationTemplatePlacementGroupTypeId.Value = registrationTemplatePlacementGroupTypeId.ToString();
            }

            BindDropDowns( registrationTemplatePlacementId );

            if ( registrationTemplatePlacement == null )
            {
                if ( registrationInstanceId.HasValue )
                {
                    nbConfigurationError.Text = "Invalid Registration Template Placement Specified";
                    nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbConfigurationError.Visible = true;
                }
                else
                {
                    // TODO: Prompt for
                    bgRegistrationTemplatePlacement.Visible = true;
                }

                return;
            }

            var groupType = GroupTypeCache.Get( registrationTemplatePlacementGroupTypeId.Value );

            if ( registrationTemplatePlacement.IconCssClass.IsNotNullOrWhiteSpace() )
            {
                _placementIconCssClass = registrationTemplatePlacement.IconCssClass;
            }
            else
            {
                _placementIconCssClass = groupType.IconCssClass;
            }

            lGroupPlacementGroupTypeIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );
            lAddPlacementGroupButtonIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );

            lGroupPlacementGroupTypeName.Text = groupType.GroupTerm.Pluralize();
            lAddPlacementGroupButtonText.Text = string.Format( " Add {0}", groupType.GroupTerm );

            lRegistrationTemplateName.Text = string.Format( "<p>Registration Template: {0}</p>", registrationTemplatePlacement.RegistrationTemplate.Name );
            if ( registrationInstanceId.HasValue )
            {
                lRegistrationInstanceName.Text = string.Format( "<p>Registration Instance: {0}</p>", new RegistrationInstanceService( rockContext ).GetSelect( registrationInstanceId.Value, s => s.Name ) );
            }

            BindPlacementGroupsRepeater();
        }

        /// <summary>
        /// Gets the placement configuration.
        /// </summary>
        /// <returns></returns>
        private PlacementConfiguration GetPlacementConfiguration()
        {
            if ( _placementConfiguration == null )
            {
                int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
                int registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();

                string placementConfigurationJSON;
                if ( registrationInstanceId.HasValue )
                {
                    placementConfigurationJSON = GetBlockUserPreference( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrantInstanceId, registrationInstanceId.Value ) );
                }
                else
                {
                    placementConfigurationJSON = GetBlockUserPreference( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrantTemplateId, registrationTemplateId ) );
                }

                _placementConfiguration = placementConfigurationJSON.FromJsonOrNull<PlacementConfiguration>() ?? new PlacementConfiguration();
            }

            return _placementConfiguration;
        }
        private PlacementConfiguration _placementConfiguration = null;

        /// <summary>
        /// Saves the placement configuration.
        /// </summary>
        /// <param name="placementConfiguration">The placement configuration.</param>
        private void SavePlacementConfiguration( PlacementConfiguration placementConfiguration )
        {
            int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            int registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();

            string placementConfigurationJSON = placementConfiguration.ToJson();
            if ( registrationInstanceId.HasValue )
            {
                SetBlockUserPreference( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrantInstanceId, registrationInstanceId.Value ), placementConfigurationJSON );
            }
            else
            {
                SetBlockUserPreference( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrantTemplateId, registrationTemplateId ), placementConfigurationJSON );
            }

            ShowDetails();
        }

        /// <summary>
        /// Binds the drop downs.
        /// </summary>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        private void BindDropDowns( int? registrationTemplatePlacementId )
        {
            var registrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();

            var rockContext = new RockContext();
            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationTemplatePlacements = registrationTemplateService.Get( registrationTemplateId.Value ).Placements.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            bgRegistrationTemplatePlacement.Items.Clear();
            foreach ( var registrationTemplatePlacementItem in registrationTemplatePlacements )
            {
                bgRegistrationTemplatePlacement.Items.Add( new ListItem( registrationTemplatePlacementItem.Name, registrationTemplatePlacementItem.Id.ToString() ) );
            }

            if ( registrationTemplatePlacementId.HasValue )
            {
                bgRegistrationTemplatePlacement.SetValue( registrationTemplatePlacementId.Value );
            }
            else
            {
                bgRegistrationTemplatePlacement.SelectedIndex = 0;
                bgRegistrationTemplatePlacement_SelectedIndexChanged( null, null );
            }

            bgAddNewOrExistingPlacementGroup.Items.Clear();
            bgAddNewOrExistingPlacementGroup.BindToEnum<AddPlacementGroupTab>();
        }

        /// <summary>
        /// Binds the placement groups repeater.
        /// </summary>
        private void BindPlacementGroupsRepeater()
        {
            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsInteger();
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();

            var groupType = GroupTypeCache.Get( hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger() );

            _groupTypeRoles = groupType.Roles.ToList();
            var rockContext = new RockContext();
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );

            RegistrationTemplatePlacement registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId );

            List<RegistrationInstance> registrationInstanceList = null;

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                registrationTemplateId = registrationInstance.RegistrationTemplateId;

                registrationInstanceList = new List<Rock.Model.RegistrationInstance>();
                registrationInstanceList.Add( registrationInstance );
            }
            else if ( registrationTemplateId.HasValue )
            {
                registrationInstanceList = registrationInstanceService.Queryable().Where( a => a.RegistrationTemplateId == registrationTemplateId.Value ).OrderBy( a => a.Name ).ToList();
            }

            var registrationInstancePlacementGroupList = new List<Group>();
            if ( registrationInstanceList != null && registrationTemplatePlacement != null )
            {
                foreach ( var registrationInstance in registrationInstanceList )
                {
                    var placementGroups = registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstance ).Where( a => a.GroupTypeId == groupType.Id ).ToList();
                    foreach ( var placementGroup in placementGroups )
                    {
                        registrationInstancePlacementGroupList.Add( placementGroup, true );
                    }
                }
            }

            var registrationTemplatePlacementGroupList = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement );
            var placementGroupList = new List<Group>();
            placementGroupList.AddRange( registrationTemplatePlacementGroupList );
            placementGroupList.AddRange( registrationInstancePlacementGroupList );

            // A placement group could be associated with both the instance and the template. So, we'll make sure the same group isn't listed more than once
            placementGroupList = placementGroupList.DistinctBy( a => a.Id ).ToList();
            placementGroupList = placementGroupList.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

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
            //
        }

        /// <summary>
        /// Handles the Click event of the btnAddPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddPlacementGroup_Click( object sender, EventArgs e )
        {
            bgAddNewOrExistingPlacementGroup.SetValue( AddPlacementGroupTab.AddNewGroup.ConvertToInt().ToString() );
            bgAddNewOrExistingPlacementGroup_SelectedIndexChanged( null, null );

            var newGroup = new Group() { GroupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger() };

            // set up Attribute Values Container
            avcNewPlacementGroupAttributeValues.AddEditControls( newGroup );

            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                // in Registration Instance Mode
                mdAddPlacementGroup.Title = "Add Group";
            }
            else if ( registrationTemplatePlacementId.HasValue )
            {
                // in Registration Template Mode
                mdAddPlacementGroup.Title = "Add Shared Group";
            }

            mdAddPlacementGroup.Show();
        }

        /// <summary>
        /// Handles the Click event of the btnConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfiguration_Click( object sender, EventArgs e )
        {
            var placementConfiguration = GetPlacementConfiguration();
            mdPlacementConfiguration.Show();
            cpConfigurationCampusPicker.SelectedCampusId = placementConfiguration.DisplayedCampusId;
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();

            bool inTemplateMode = registrationInstanceId == null;
            pwRegistrationTemplateConfiguration.Visible = inTemplateMode;

            var rockContext = new RockContext();

            if ( inTemplateMode )
            {
                cbShowRegistrationInstanceName.Checked = placementConfiguration.ShowRegistrationInstanceName;

                cblRegistrationInstances.Items.Clear();
                var registrationTemplateInstances = new RegistrationInstanceService( rockContext ).Queryable()
                    .Where( a => a.RegistrationTemplateId == registrationTemplateId )
                    .OrderBy( a => a.Name )
                    .Select( a => new { a.Id, a.Name } ).ToList();

                foreach ( var registrationTemplateInstance in registrationTemplateInstances )
                {
                    cblRegistrationInstances.Items.Add( new ListItem( registrationTemplateInstance.Name, registrationTemplateInstance.Id.ToString() ) );
                }

                cblRegistrationInstances.SetValues( placementConfiguration.IncludedRegistrationInstanceIds );
            }

            cbHighlightGenders.Checked = placementConfiguration.HighlightGenders;
            cbShowFees.Checked = placementConfiguration.ShowFees;

            cblDisplayedRegistrantAttributes.Items.Clear();

            var registrantAttributes = new AttributeService( rockContext ).GetByEntityTypeId( EntityTypeCache.GetId<RegistrationRegistrant>() )
                .Where( a =>
                    a.EntityTypeQualifierColumn == "RegistrationTemplateId" &&
                    a.EntityTypeQualifierValue == registrationTemplateId.ToString() ).ToAttributeCacheList();

            foreach ( var registrantAttribute in registrantAttributes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                cblDisplayedRegistrantAttributes.Items.Add( new ListItem( registrantAttribute.Name, registrantAttribute.Id.ToString() ) );
            }

            cblDisplayedRegistrantAttributes.SetValues( placementConfiguration.DisplayedRegistrantAttributeIds );

            var fakeGroup = new Rock.Model.Group { GroupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger() };
            fakeGroup.LoadAttributes();

            var groupAttributeList = fakeGroup.Attributes.Select( a => a.Value ).ToList();
            cblDisplayedGroupAttributes.Items.Clear();
            foreach ( var groupAttribute in groupAttributeList.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                cblDisplayedGroupAttributes.Items.Add( new ListItem( groupAttribute.Name, groupAttribute.Id.ToString() ) );
            }

            cblDisplayedGroupAttributes.SetValues( placementConfiguration.DisplayedGroupAttributeIds );

            var fakeGroupMember = new GroupMember() { Group = fakeGroup, GroupId = fakeGroup.Id };
            fakeGroupMember.LoadAttributes();
            var groupMemberAttributeList = fakeGroupMember.Attributes.Select( a => a.Value ).ToList();
            cblDisplayedGroupMemberAttributes.Items.Clear();
            foreach ( var groupMemberAttribute in groupMemberAttributeList.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                cblDisplayedGroupMemberAttributes.Items.Add( new ListItem( groupMemberAttribute.Name, groupMemberAttribute.Id.ToString() ) );
            }

            cblDisplayedGroupMemberAttributes.SetValues( placementConfiguration.DisplayedGroupMemberAttributeIds );

            cbHideFullGroups.Checked = placementConfiguration.HideFullGroups;
        }

        /// <summary>
        /// Handles the SaveClick event of the mdPlacementConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPlacementConfiguration_SaveClick( object sender, EventArgs e )
        {
            var placementConfiguration = new PlacementConfiguration();
            placementConfiguration.DisplayedCampusId = cpConfigurationCampusPicker.SelectedCampusId;
            placementConfiguration.ShowRegistrationInstanceName = cbShowRegistrationInstanceName.Checked;
            placementConfiguration.IncludedRegistrationInstanceIds = cblRegistrationInstances.SelectedValues.AsIntegerList().ToArray();
            placementConfiguration.HighlightGenders = cbHighlightGenders.Checked;
            placementConfiguration.ShowFees = cbShowFees.Checked;
            placementConfiguration.DisplayedRegistrantAttributeIds = cblDisplayedRegistrantAttributes.SelectedValues.AsIntegerList().ToArray();
            //   placementConfiguration.RegistrantDataFilterId = todo
            placementConfiguration.DisplayedGroupAttributeIds = cblDisplayedGroupAttributes.SelectedValues.AsIntegerList().ToArray();
            placementConfiguration.HideFullGroups = cbHideFullGroups.Checked;
            placementConfiguration.DisplayedGroupMemberAttributeIds = cblDisplayedGroupMemberAttributes.SelectedValues.AsIntegerList().ToArray();
            SavePlacementConfiguration( placementConfiguration );

            mdPlacementConfiguration.Hide();
        }

        private List<GroupTypeRoleCache> _groupTypeRoles;
        private string _placementIconCssClass;

        /// <summary>
        /// Handles the ItemDataBound event of the rptPlacementGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
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

            var lGroupIconHtml = e.Item.FindControl( "lGroupIconHtml" ) as Literal;
            lGroupIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );

            var lGroupName = e.Item.FindControl( "lGroupName" ) as Literal;
            lGroupName.Text = placementGroup.Name;

            var hlGroupCampus = e.Item.FindControl( "hlGroupCampus" ) as HighlightLabel;
            if ( placementGroup.CampusId.HasValue )
            {
                hlGroupCampus.Text = CampusCache.Get( placementGroup.CampusId.Value ).Name;
            }

            var avcGroupAttributes = e.Item.FindControl( "avcGroupAttributes" ) as AttributeValuesContainer;
            avcGroupAttributes.ShowCategoryLabel = false;
            avcGroupAttributes.IncludedAttributes = GetPlacementConfiguration().DisplayedGroupAttributeIds.Select( a => AttributeCache.Get( a ) ).Where( a => a != null ).ToArray();
            placementGroup.LoadAttributes();
            avcGroupAttributes.AddDisplayControls( placementGroup );

            var rptPlacementGroupRole = e.Item.FindControl( "rptPlacementGroupRole" ) as Repeater;
            rptPlacementGroupRole.DataSource = _groupTypeRoles;
            rptPlacementGroupRole.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPlacementGroupRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPlacementGroupRole_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var groupTypeRole = e.Item.DataItem as GroupTypeRoleCache;
            if ( groupTypeRole == null )
            {
                return;
            }

            var hfGroupTypeRoleId = e.Item.FindControl( "hfGroupTypeRoleId" ) as HiddenFieldWithClass;
            hfGroupTypeRoleId.Value = groupTypeRole.Id.ToString();

            var hfGroupTypeRoleMaxMembers = e.Item.FindControl( "hfGroupTypeRoleMaxMembers" ) as HiddenFieldWithClass;
            hfGroupTypeRoleMaxMembers.Value = groupTypeRole.MaxCount.ToString();

            var lGroupRoleName = e.Item.FindControl( "lGroupRoleName" ) as Literal;
            lGroupRoleName.Text = groupTypeRole.Name.Pluralize();
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgRegistrationTemplatePlacement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgRegistrationTemplatePlacement_SelectedIndexChanged( object sender, EventArgs e )
        {
            var additionalQueryParameters = new Dictionary<string, string>();
            if ( bgRegistrationTemplatePlacement.SelectedValue.AsInteger() > 0 )
            {
                additionalQueryParameters.Add( PageParameterKey.RegistrationTemplatePlacementId, bgRegistrationTemplatePlacement.SelectedValue );
                NavigateToCurrentPageReference( additionalQueryParameters );
            }
        }

        #region Add Placement Group

        /// <summary>
        /// Handles the SaveClick event of the mdAddPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddPlacementGroup_SaveClick( object sender, EventArgs e )
        {
            Group placementGroup;
            var groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var rockContext = new RockContext();
            if ( bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddExistingGroup.ConvertToInt().ToString() )
            {
                var existingGroupId = gpAddExistingPlacementGroup.SelectedValue.AsIntegerOrNull();
                if ( !existingGroupId.HasValue )
                {
                    return;
                }

                placementGroup = new GroupService( rockContext ).Get( existingGroupId.Value );
                if ( placementGroup == null )
                {
                    return;
                }

                if ( groupTypeId != placementGroup.GroupTypeId )
                {
                    // TODO
                    nbAddExistingPlacementGroupWarning.Text = "Invalid Group Type";
                    return;
                }

                AddPlacementGroup( rockContext, placementGroup );
            }
            else
            {
                placementGroup = new Group();
                placementGroup.GroupTypeId = groupTypeId;
                placementGroup.ParentGroupId = gpNewPlacementGroupParentGroup.GroupId;
                placementGroup.Name = tbNewPlacementGroupName.Text;
                placementGroup.CampusId = cpNewPlacementGroupCampus.SelectedCampusId;
                placementGroup.GroupCapacity = nbGroupCapacity.Text.AsIntegerOrNull();
                placementGroup.Description = tbNewPlacementGroupDescription.Text;
                new GroupService( rockContext ).Add( placementGroup );
                rockContext.SaveChanges();
                avcNewPlacementGroupAttributeValues.GetEditValues( placementGroup );
                placementGroup.SaveAttributeValues();

                AddPlacementGroup( rockContext, placementGroup );
            }

            rockContext.SaveChanges();

            mdAddPlacementGroup.Hide();
            BindPlacementGroupsRepeater();
        }

        /// <summary>
        /// Adds the placement group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="placementGroup">The placement group.</param>
        private void AddPlacementGroup( RockContext rockContext, Group placementGroup )
        {
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                // in RegistrationInstanceMode
                registrationInstanceService.AddRegistrationInstancePlacementGroup( registrationInstance, placementGroup );
            }
            else if ( registrationTemplatePlacementId.HasValue )
            {
                var registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId.Value );
                registrationTemplatePlacementService.AddRegistrationTemplatePlacementPlacementGroup( registrationTemplatePlacement, placementGroup );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgAddNewOrExistingPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void bgAddNewOrExistingPlacementGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            pnlAddExistingPlacementGroup.Visible = bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddExistingGroup.ConvertToInt().ToString();
            pnlAddNewPlacementGroup.Visible = bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddNewGroup.ConvertToInt().ToString();
        }

        /// <summary>
        /// Handles the SelectItem event of the gpNewPlacementGroupParentGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpNewPlacementGroupParentGroup_SelectItem( object sender, EventArgs e )
        {
            int groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var selectedParentGroup = new GroupService( new RockContext() ).Get( gpNewPlacementGroupParentGroup.SelectedValue.AsInteger() );
            if ( !IsValidParentGroup( selectedParentGroup, groupTypeId ) )
            {

                var groupType = GroupTypeCache.Get( groupTypeId );
                nbNewPlacementGroupParentGroupWarning.Text = string.Format( "The selected parent group doesn't allow adding {0} child groups", groupType );
                nbNewPlacementGroupParentGroupWarning.Visible = true;
            }
            else
            {
                nbNewPlacementGroupParentGroupWarning.Visible = false;
            }
        }

        /// <summary>
        /// Determines whether [is valid parent group] [the specified parent group].
        /// </summary>
        /// <param name="parentGroup">The parent group.</param>
        /// <param name="childGroupTypeId">The child group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid parent group] [the specified parent group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidParentGroup( Group parentGroup, int childGroupTypeId )
        {
            bool isValidParentGroup = true;

            if ( parentGroup != null )
            {
                var parentGroupGroupType = GroupTypeCache.Get( parentGroup.GroupTypeId );
                isValidParentGroup = parentGroupGroupType.AllowAnyChildGroupType || parentGroupGroupType.ChildGroupTypes.Any( a => a.Id == childGroupTypeId );
            }

            return isValidParentGroup;
        }

        /// <summary>
        /// Handles the SelectItem event of the gpAddExistingPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpAddExistingPlacementGroup_SelectItem( object sender, EventArgs e )
        {
            int groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var selectedGroup = new GroupService( new RockContext() ).Get( gpAddExistingPlacementGroup.SelectedValue.AsInteger() );
            if ( !IsValidExistingGroup( selectedGroup, groupTypeId ) )
            {

                var groupType = GroupTypeCache.Get( groupTypeId );
                nbAddExistingPlacementGroupWarning.Text = string.Format( "The selected group must be a {0} group", groupType );
                nbAddExistingPlacementGroupWarning.Visible = true;
            }
            else
            {
                nbAddExistingPlacementGroupWarning.Visible = false;
            }
        }

        /// <summary>
        /// Determines whether [is valid existing group] [the specified selected group].
        /// </summary>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid existing group] [the specified selected group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidExistingGroup( Group selectedGroup, int groupTypeId )
        {
            return selectedGroup.GroupTypeId == groupTypeId;
        }

        #endregion Add Placement Group
    }
}