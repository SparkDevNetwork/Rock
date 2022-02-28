﻿// <copyright>
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
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]

    [LinkedPage( "Communication Page",
        Description = "The communication page to use for when the business email address is clicked. Leave this blank to use the default.",
        Key = AttributeKey.CommunicationPage,
        IsRequired = false,
        Order = 0 )]

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The label badges to display in this block.",
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Display Tags",
        Description = "Should tags be displayed?",
        Key = AttributeKey.DisplayTags,
        DefaultBooleanValue = true,
        Order = 2 )]

    [CategoryField(
        "Tag Category",
        Description = "Optional category to limit the tags to. If specified all new personal tags will be added with this category.",
        Key = AttributeKey.TagCategory,
        AllowMultiple = false,
        EntityType = typeof( Rock.Model.Tag ),
        IsRequired = false,
        Order = 3 )]

    [CustomEnhancedListField(
        "Search Key Types",
        Key = AttributeKey.SearchKeyTypes,
        Description = "Optional list of search key types to limit the display in search keys grid. No selection will show all.",
        ListSource = ListSource.SearchKeyTypes,
        IsRequired = false,
        Order = 4 )]

    [AttributeField(
        "Business Attributes",
        Key = AttributeKey.BusinessAttributes,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Description = "The person attributes that should be displayed / edited for adults.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 5 )]

    public partial class BusinessDetail : ContextEntityBlock
    {
        private static class AttributeKey
        {
            public const string Badges = "Badges";
            public const string BusinessAttributes = "BusinessAttributes";
            public const string CommunicationPage = "CommunicationPage";
            public const string DisplayTags = "DisplayTags";
            public const string SearchKeyTypes = "SearchKeyTypes";
            public const string TagCategory = "TagCategory";
        }

        private static class ListSource
        {
            public const string SearchKeyTypes = @"
                SELECT CAST( V.[Guid] as varchar(40) ) AS [Value], V.[Value] AS [Text]
                FROM [DefinedType] T
                INNER JOIN [DefinedValue] V ON V.[DefinedTypeId] = T.[Id]
                LEFT OUTER JOIN [AttributeValue] AV ON AV.[EntityId] = V.[Id]
	                AND AV.[AttributeId] = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '15C419AA-76A9-4105-AB99-8384AB0E9B44')
	                AND AV.[Value] = 'False'
                WHERE T.[Guid] = '61BDD0E3-173D-45AB-9E8C-1FBB9FA8FDF3'
	                AND AV.[Id] IS NULL
                ORDER BY V.[Order]";
        }

        /// <summary>
        /// Gets or sets the state of the person search keys.
        /// </summary>
        /// <value>
        /// The state of the person search keys.
        /// </value>
        private List<PersonSearchKey> PersonSearchKeysState { get; set; }

        /// <summary>
        /// Gets or sets the state of the person previous names.
        /// </summary>
        /// <value>
        /// The state of the person previous names.
        /// </value>
        private List<PersonPreviousName> PersonPreviousNamesState { get; set; }

        #region Base Control Methods

        /// <inheritdoc />
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += BusinessDetail_BlockUpdated;

            ShowBadges( PageParameter( "BusinessId" ).AsInteger() );

            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ).Id;
            dvpReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ).Id;

            gSearchKeys.Actions.ShowAdd = true;
            gSearchKeys.Actions.AddClick += gSearchKeys_AddClick;
        }

        /// <inheritdoc />
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "BusinessId" ).AsInteger() );
            }
        }

        /// <inheritdoc />
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["PersonPreviousNamesState"] as string;

            if ( string.IsNullOrWhiteSpace( json ) )
            {
                PersonPreviousNamesState = new List<PersonPreviousName>();
            }
            else
            {
                PersonPreviousNamesState = PersonPreviousName.FromJsonAsList( json ) ?? new List<PersonPreviousName>();
            }

            json = ViewState["PersonSearchKeysState"] as string;

            if ( string.IsNullOrWhiteSpace( json ) )
            {
                PersonSearchKeysState = new List<PersonSearchKey>();
            }
            else
            {
                PersonSearchKeysState = PersonSearchKey.FromJsonAsList( json ) ?? new List<PersonSearchKey>();
            }
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["PersonPreviousNamesState"] = JsonConvert.SerializeObject( PersonPreviousNamesState, Formatting.None, jsonSetting );
            ViewState["PersonSearchKeysState"] = JsonConvert.SerializeObject( PersonSearchKeysState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PublicProfileEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BusinessDetail_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "BusinessId" ).AsInteger() );
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var business = new PersonService( rockContext ).Get( int.Parse( hfBusinessId.Value ) );
            ShowEditDetails( business );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var personService = new PersonService( rockContext );
            Person business = null;

            if ( int.Parse( hfBusinessId.Value ) != 0 )
            {
                business = personService.Get( int.Parse( hfBusinessId.Value ) );
            }

            if ( business == null )
            {
                business = new Person();
                personService.Add( business );
                tbBusinessName.Text = tbBusinessName.Text.FixCase();
            }

            // Business Name
            business.LastName = tbBusinessName.Text;

            // Phone Number
            var businessPhoneTypeId = new DefinedValueService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;

            var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == businessPhoneTypeId );

            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
            {
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = businessPhoneTypeId };
                    business.PhoneNumbers.Add( phoneNumber );
                }
                phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                phoneNumber.IsMessagingEnabled = cbSms.Checked;
                phoneNumber.IsUnlisted = cbUnlisted.Checked;
            }
            else
            {
                if ( phoneNumber != null )
                {
                    business.PhoneNumbers.Remove( phoneNumber );
                    new PhoneNumberService( rockContext ).Delete( phoneNumber );
                }
            }

            // Record Type - this is always "business". it will never change.
            business.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            // Record Status
            business.RecordStatusValueId = dvpRecordStatus.SelectedValueAsInt(); ;

            // Record Status Reason
            int? newRecordStatusReasonId = null;
            if ( business.RecordStatusValueId.HasValue && business.RecordStatusValueId.Value == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id )
            {
                newRecordStatusReasonId = dvpReason.SelectedValueAsInt();
            }
            business.RecordStatusReasonValueId = newRecordStatusReasonId;

            // Email
            business.IsEmailActive = true;
            business.Email = tbEmail.Text.Trim();
            business.EmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();

            avcEditAttributes.GetEditValues( business );

            if ( !business.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                // Add/Update Family Group
                var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                int adultRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var adultFamilyMember = UpdateGroupMember( business.Id, familyGroupType, business.LastName + " Business", ddlCampus.SelectedValueAsInt(), adultRoleId, rockContext );
                business.GivingGroup = adultFamilyMember.Group;

                // Add/Update Known Relationship Group Type
                var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                int knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var knownRelationshipOwner = UpdateGroupMember( business.Id, knownRelationshipGroupType, "Known Relationship", null, knownRelationshipOwnerRoleId, rockContext );

                // Add/Update Implied Relationship Group Type
                var impliedRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK.AsGuid() );
                int impliedRelationshipOwnerRoleId = impliedRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var impliedRelationshipOwner = UpdateGroupMember( business.Id, impliedRelationshipGroupType, "Implied Relationship", null, impliedRelationshipOwnerRoleId, rockContext );

                rockContext.SaveChanges();

                // Location
                int workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;

                var groupLocationService = new GroupLocationService( rockContext );
                var workLocation = groupLocationService.Queryable( "Location" )
                    .Where( gl =>
                        gl.GroupId == adultFamilyMember.Group.Id &&
                        gl.GroupLocationTypeValueId == workLocationTypeId )
                    .FirstOrDefault();

                if ( string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                {
                    if ( workLocation != null )
                    {
                        if ( cbSaveFormerAddressAsPreviousAddress.Checked )
                        {
                            GroupLocationHistorical.CreateCurrentRowFromGroupLocation( workLocation, RockDateTime.Now );
                        }

                        groupLocationService.Delete( workLocation );
                    }
                }
                else
                {
                    var newLocation = new LocationService( rockContext ).Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                    if ( workLocation == null )
                    {
                        workLocation = new GroupLocation();
                        groupLocationService.Add( workLocation );
                        workLocation.GroupId = adultFamilyMember.Group.Id;
                        workLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                    else
                    {
                        // Save this to history if the box is checked and the new info is different than the current one.
                        if ( cbSaveFormerAddressAsPreviousAddress.Checked && newLocation.Id != workLocation.Location.Id )
                        {
                            new GroupLocationHistoricalService( rockContext ).Add( GroupLocationHistorical.CreateCurrentRowFromGroupLocation( workLocation, RockDateTime.Now ) );
                        }
                    }

                    workLocation.Location = newLocation;
                    workLocation.IsMailingLocation = true;
                }

                rockContext.SaveChanges();
                
                hfBusinessId.Value = business.Id.ToString();
            } );

            /* Ethan Drotning 2022-01-11
             * Need save the PersonSearchKeys outside of the transaction since the DB might not have READ_COMMITTED_SNAPSHOT enabled.
             */

            // PersonSearchKey
            var personSearchKeyService = new PersonSearchKeyService( rockContext );
            var validSearchTypes = GetValidSearchKeyTypes();
            var databaseSearchKeys = personSearchKeyService.Queryable().Where( a => a.PersonAlias.PersonId == business.Id && validSearchTypes.Contains( a.SearchTypeValue.Guid ) ).ToList();

            foreach ( var deletedSearchKey in databaseSearchKeys.Where( a => !PersonSearchKeysState.Any( p => p.Guid == a.Guid ) ) )
            {
                personSearchKeyService.Delete( deletedSearchKey );
            }

            foreach ( var personSearchKey in PersonSearchKeysState.Where( a => !databaseSearchKeys.Any( d => d.Guid == a.Guid ) ) )
            {
                personSearchKey.PersonAliasId = business.PrimaryAliasId.Value;
                personSearchKeyService.Add( personSearchKey );
            }

            rockContext.SaveChanges();

            business.SaveAttributeValues();

            var queryParams = new Dictionary<string, string>
            {
                { "BusinessId", hfBusinessId.Value }
            };

            NavigateToCurrentPage( queryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int? businessId = hfBusinessId.Value.AsIntegerOrNull();
            if ( businessId.HasValue && businessId > 0 )
            {
                ShowSummary( businessId.Value );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            dvpReason.Visible = dvpRecordStatus.SelectedValueAsInt() == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
        }

        /// <summary>
        /// Handles the AddClick event of the gSearchKeys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gSearchKeys_AddClick( object sender, EventArgs e )
        {
            tbSearchValue.Text = string.Empty;

            var validSearchTypes = GetValidSearchKeyTypes()
                .Where( t => t != Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() )
                .ToList();

            var searchValueTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS ).DefinedValues;
            var searchTypesList = searchValueTypes.Where( a => validSearchTypes.Contains( a.Guid ) ).ToList();

            ddlSearchValueType.DataSource = searchTypesList;
            ddlSearchValueType.DataTextField = "Value";
            ddlSearchValueType.DataValueField = "Id";
            ddlSearchValueType.DataBind();
            ddlSearchValueType.Items.Insert( 0, new ListItem() );
            mdSearchKey.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSearchKey control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSearchKey_SaveClick( object sender, EventArgs e )
        {
            this.PersonSearchKeysState.Add( new PersonSearchKey { SearchValue = tbSearchValue.Text, SearchTypeValueId = ddlSearchValueType.SelectedValue.AsInteger(), Guid = Guid.NewGuid() } );
            BindPersonSearchKeysGrid();
            mdSearchKey.Hide();
        }

        /// <summary>
        /// Handles the Delete event of the gSearchKeys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSearchKeys_Delete( object sender, RowEventArgs e )
        {
            this.PersonSearchKeysState.RemoveEntity( ( Guid ) e.RowKeyValue );
            BindPersonSearchKeysGrid();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        public void ShowDetail( int businessId )
        {
            var rockContext = new RockContext();

            // Load the Campus drop down
            ddlCampus.Items.Clear();
            ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var campus in CampusCache.All() )
            {
                ListItem li = new ListItem( campus.Name, campus.Id.ToString() );
                ddlCampus.Items.Add( li );
            }

            Person business = null;     // A business is a person

            if ( !businessId.Equals( 0 ) )
            {
                business = new PersonService( rockContext ).Get( businessId );
                pdAuditDetails.SetEntity( business, ResolveRockUrl( "~" ) );
            }

            if ( business == null )
            {
                business = new Person { Id = 0, Guid = Guid.NewGuid() };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            bool editAllowed = business.IsAuthorized( Authorization.EDIT, CurrentPerson );

            hfBusinessId.Value = business.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
            }

            if ( GetAttributeValue( AttributeKey.DisplayTags ).AsBoolean( true ) )
            {
                taglPersonTags.Visible = true;
                taglPersonTags.EntityTypeId = business.TypeId;
                taglPersonTags.EntityGuid = business.Guid;
                taglPersonTags.CategoryGuid = GetAttributeValue( AttributeKey.TagCategory ).AsGuidOrNull();
                taglPersonTags.GetTagValues( CurrentPersonId );
            }
            else
            {
                taglPersonTags.Visible = false;
            }

            if ( readOnly )
            {
                ShowSummary( businessId );
            }
            else
            {
                if ( business.Id > 0 )
                {
                    ShowSummary( business.Id );
                }
                else
                {
                    ShowEditDetails( business );
                }
            }
        }

        /// <summary>
        /// Shows the badges.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        private void ShowBadges( int businessId )
        {
            var business = new PersonService( new RockContext() ).Get( businessId );
            Entity = business;

            string badgeList = GetAttributeValue( AttributeKey.Badges );
            blStatus.BadgeTypes.Clear();
            if ( !string.IsNullOrWhiteSpace( badgeList ) )
            {
                foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                {
                    Guid guid = badgeGuid.AsGuid();
                    if ( guid != Guid.Empty )
                    {
                        var badge = BadgeCache.Get( guid );
                        if ( badge != null )
                        {
                            blStatus.BadgeTypes.Add( badge );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the summary.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowSummary( int businessId )
        {
            SetEditMode( false );
            hfBusinessId.SetValue( businessId );
            lTitle.Text = "Business Details".FormatAsHtmlTitle();
            var rockContext = new RockContext();

            var business = new PersonService( rockContext ).Get( businessId );
            if ( business != null )
            {
                SetHeadingStatusInfo( business );
                lViewPanelBusinessName.Text = business.LastName;

                var detailsLeft = new DescriptionList();
                var detailsRight = new DescriptionList();

                if ( business.RecordStatusReasonValue != null )
                {
                    detailsLeft.Add( "Record Status Reason", business.RecordStatusReasonValue );
                }

                // Get addresses
                var workLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
                if ( workLocationType != null )
                {
                    if ( business.GivingGroup != null ) // Giving Group is a shortcut to Family Group for business
                    {
                        var location = business.GivingGroup.GroupLocations
                            .Where( gl => gl.GroupLocationTypeValueId == workLocationType.Id )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        if ( location != null )
                        {
                            detailsLeft.Add( "Address", location.GetFullStreetAddress().ConvertCrLfToHtmlBr() );
                        }

                        // Get Previous Addresses
                        var previousLocations = new GroupLocationHistoricalService( rockContext )
                            .Queryable()
                            .Where( h => h.GroupId == business.GivingGroup.Id && h.GroupLocationTypeValueId == workLocationType.Id )
                            .OrderBy( h => h.EffectiveDateTime )
                            .Select( h => h.Location )
                            .ToList();

                        foreach ( var previouslocation in previousLocations )
                        {
                            detailsLeft.Add( "Previous Address", previouslocation.GetFullStreetAddress().ConvertCrLfToHtmlBr() );
                        }
                    }
                }

                var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                if ( workPhoneType != null )
                {
                    var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                    if ( phoneNumber != null )
                    {
                        detailsRight.Add( "Phone Number", phoneNumber.ToString() );
                    }
                }

                var communicationLinkedPageValue = this.GetAttributeValue( AttributeKey.CommunicationPage );
                Rock.Web.PageReference communicationPageReference;
                if ( communicationLinkedPageValue.IsNotNullOrWhiteSpace() )
                {
                    communicationPageReference = new Rock.Web.PageReference( communicationLinkedPageValue );
                }
                else
                {
                    communicationPageReference = null;
                }

                detailsRight.Add( "Email Address", business.GetEmailTag( ResolveRockUrl( "/" ), communicationPageReference ) );

                lDetailsLeft.Text = detailsLeft.Html;
                lDetailsRight.Text = detailsRight.Html;

                ShowViewAttributes( business );
            }
        }

        /// <summary>
        /// Sets the heading Status information.
        /// </summary>
        /// <param name="business">The business.</param>
        private void SetHeadingStatusInfo( Person business )
        {
            if ( business.RecordStatusValue != null )
            {
                hlStatus.Text = business.RecordStatusValue.Value;
                if ( business.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() )
                {
                    hlStatus.LabelType = LabelType.Warning;
                }
                else if ( business.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )
                {
                    hlStatus.LabelType = LabelType.Danger;
                }
                else
                {
                    hlStatus.LabelType = LabelType.Success;
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowEditDetails( Person business )
        {
            if ( business.Id > 0 )
            {
                var rockContext = new RockContext();

                lTitle.Text = ActionTitle.Edit( business.FullName ).FormatAsHtmlTitle();
                tbBusinessName.Text = business.LastName;

                // address
                Location location = null;
                var workLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
                if ( business.GivingGroup != null )     // Giving group is a shortcut to the family group for business
                {
                    ddlCampus.SelectedValue = business.GivingGroup.CampusId.ToString();

                    location = business.GivingGroup.GroupLocations
                        .Where( gl => gl.GroupLocationTypeValueId == workLocationType.Id )
                        .Select( gl => gl.Location )
                        .FirstOrDefault();
                }

                acAddress.SetValues( location );

                // Phone Number
                var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                PhoneNumber phoneNumber = null;
                if ( workPhoneType != null )
                {
                    phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                }
                if ( phoneNumber != null )
                {
                    pnbPhone.Text = phoneNumber.NumberFormatted;
                    cbSms.Checked = phoneNumber.IsMessagingEnabled;
                    cbUnlisted.Checked = phoneNumber.IsUnlisted;
                }
                else
                {
                    pnbPhone.Text = string.Empty;
                    cbSms.Checked = false;
                    cbUnlisted.Checked = false;
                }

                tbEmail.Text = business.Email;
                rblEmailPreference.SelectedValue = business.EmailPreference.ToString();

                dvpRecordStatus.SelectedValue = business.RecordStatusValueId.HasValue ? business.RecordStatusValueId.Value.ToString() : string.Empty;
                dvpReason.SelectedValue = business.RecordStatusReasonValueId.HasValue ? business.RecordStatusReasonValueId.Value.ToString() : string.Empty;
                dvpReason.Visible = business.RecordStatusReasonValueId.HasValue &&
                    business.RecordStatusValueId.Value == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Business" ).FormatAsHtmlTitle();
            }

            var validSearchTypes = GetValidSearchKeyTypes();
            this.PersonSearchKeysState = business.GetPersonSearchKeys().Where( a => validSearchTypes.Contains( a.SearchTypeValue.Guid ) ).ToList();

            BindPersonSearchKeysGrid();
            SetEditMode( true );
            ShowEditAttributes( business );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Binds the person search keys grid.
        /// </summary>
        private void BindPersonSearchKeysGrid()
        {
            var values = this.PersonSearchKeysState ?? new List<PersonSearchKey>();
            var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
            if ( dv != null )
            {
                values = values.Where( s => s.SearchTypeValueId != dv.Id ).ToList();
            }

            gSearchKeys.DataKeyNames = new string[] { "Guid" };
            gSearchKeys.DataSource = values;
            gSearchKeys.DataBind();
        }

        /// <summary>
        /// Gets the valid search key types.
        /// </summary>
        /// <returns></returns>
        private List<Guid> GetValidSearchKeyTypes()
        {
            var searchKeyTypes = new List<Guid> { Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() };

            var dt = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS );
            if ( dt != null )
            {
                var values = dt.DefinedValues;
                var searchTypesList = this.GetAttributeValue( AttributeKey.SearchKeyTypes ).SplitDelimitedValues().AsGuidList();
                if ( searchTypesList.Any() )
                {
                    values = values.Where( v => searchTypesList.Contains( v.Guid ) ).ToList();
                }

                foreach ( var dv in dt.DefinedValues )
                {
                    if ( dv.GetAttributeValue( "UserSelectable" ).AsBoolean() )
                    {
                        searchKeyTypes.Add( dv.Guid );
                    }
                }
            }

            return searchKeyTypes;

        }

        /// <summary>
        /// Updates the group member.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private GroupMember UpdateGroupMember( int businessId, GroupTypeCache groupType, string groupName, int? campusId, int groupRoleId, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            GroupMember groupMember = groupMemberService.Queryable( "Group" )
                .Where( m =>
                    m.PersonId == businessId &&
                    m.GroupRoleId == groupRoleId )
                .FirstOrDefault();

            if ( groupMember == null )
            {
                groupMember = new GroupMember();
                groupMember.Group = new Group();
            }

            groupMember.PersonId = businessId;
            groupMember.GroupRoleId = groupRoleId;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMember.Group.GroupTypeId = groupType.Id;
            groupMember.Group.Name = groupName;
            groupMember.Group.CampusId = campusId;

            if ( groupMember.Id == 0)
            {
                groupMemberService.Add( groupMember );
            }

            return groupMember;
        }

        /// <summary>
        /// Shows the edit attributes.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowEditAttributes( Person business )
        {
            var attributeGuidList = GetAttributeValue( AttributeKey.BusinessAttributes ).SplitDelimitedValues().AsGuidList();
            if ( !attributeGuidList.Any() )
            {
                pnlEditAttributes.Visible = false;
                return;
            }

            pnlEditAttributes.Visible = true;

            avcEditAttributes.IncludedAttributes = attributeGuidList.Select( a => AttributeCache.Get( a ) ).ToArray();
            avcEditAttributes.NumberOfColumns = 2;
            avcEditAttributes.ShowCategoryLabel = false;
            avcEditAttributes.AddEditControls( business, true );
        }

        /// <summary>
        /// Shows the view attributes.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowViewAttributes( Person business )
        {
            var attributeGuidList = GetAttributeValue( AttributeKey.BusinessAttributes ).SplitDelimitedValues().AsGuidList();
            if ( !attributeGuidList.Any() )
            {
                pnlViewAttributes.Visible = false;
                return;
            }

            pnlViewAttributes.Visible = true;

            avcViewAttributes.IncludedAttributes = attributeGuidList.Select( a => AttributeCache.Get( a ) ).ToArray();
            avcViewAttributes.NumberOfColumns = 3;
            avcViewAttributes.ShowCategoryLabel = false;
            avcViewAttributes.AddDisplayControls( business );
        }

        #endregion Internal Methods
    }
}