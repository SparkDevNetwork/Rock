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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Displays person details for a checked-in person
    /// </summary>
    [DisplayName( "Person Profile " )]
    [Category( "Check-in > Manager" )]
    [Description( "Displays person details for a checked-in person" )]

    #region Block Attributes

    [BooleanField(
        "Show Related People",
        Key = AttributeKey.ShowRelatedPeople,
        Description = "Should anyone who is allowed to check-in the current person also be displayed with the family members?",
        IsRequired = false,
        Order = 1 )]

    [SystemPhoneNumberField(
        "Send SMS From",
        Key = AttributeKey.SMSFrom,
        Description = "The phone number SMS messages should be sent from",
        IsRequired = false,
        AllowMultiple = false,
        Order = 2 )]

    [AttributeCategoryField(
        "Child Attribute Category",
        Key = AttributeKey.ChildAttributeCategory,
        Description = "The children Attribute Category to display attributes from.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 3 )]

    [AttributeCategoryField(
        "Adult Attribute Category",
        Key = AttributeKey.AdultAttributeCategory,
        Description = "The adult Attribute Category to display attributes from.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 4 )]

    [BooleanField(
        "Show Share Person Button",
        Key = AttributeKey.ShowSharePersonButton,
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 5 )]

    [LinkedPage(
        "Share Person Page",
        Key = AttributeKey.SharePersonPage,
        DefaultValue = Rock.SystemGuid.Page.EDIT_PERSON + "," + Rock.SystemGuid.PageRoute.EDIT_PERSON_ROUTE,
        IsRequired = false,
        Order = 6
        )]

    [LinkedPage(
        "Profile Page",
        Description = "The Page to go to when a family member of the attendee is clicked.",
        Key = AttributeKey.PersonProfilePage,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER,
        IsRequired = false,
        Order = 6
        )]

    [CategoryField(
        "Snippet Category",
        Description = "The category to show SMS Snippets for (leave blank for all categories).",
        Key = AttributeKey.SnippetCategory,
        EntityType = typeof( Snippet ),
        IsRequired = false,
        Order = 7 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "D54909DB-8A5D-4665-97ED-E2C8577E3C64" )]
    public partial class PersonLeft : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowRelatedPeople = "ShowRelatedPeople";
            public const string SMSFrom = "SMSFrom";
            public const string ChildAttributeCategory = "ChildAttributeCategory";
            public const string AdultAttributeCategory = "AdultAttributeCategory";
            public const string SharePersonPage = "SharePersonPage";
            public const string ShowSharePersonButton = "ShowSharePersonButton";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string SnippetCategory = "SnippetCategory";
        }

        #endregion Attribute Keys

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string SmsPhoneNumberId = "SmsPhoneNumberId";
        }

        #endregion ViewState Keys

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            /// <summary>
            /// The person Guid
            /// </summary>
            public const string PersonGuid = "Person";

            /// <summary>
            /// The person identifier
            /// </summary>
            public const string PersonId = "PersonId";

            /// <summary>
            /// The area Guid
            /// </summary>
            public const string AreaGuid = "Area";

            /// <summary>
            /// The attendance identifier parameter (if Person isn't specified in URL, get the Person from the Attendance instead
            /// </summary>
            public const string AttendanceId = "AttendanceId";
        }

        #endregion Page Parameter Keys

        #region Properties

        // used for public / protected properties

        /// <summary>
        /// This is a potentially-temporary property, until we decide whether to re-work this Block to allow sending SMS messages to ALL SMS-enabled phone numbers.
        /// As of now, we are only allowing the sending of the first SMS-enabled phone number for a given person.
        /// </summary>
        public int SmsPhoneNumberId
        {
            get
            {
                return ( ViewState[ViewStateKey.SmsPhoneNumberId] as string ).AsInteger();
            }

            set
            {
                ViewState[ViewStateKey.SmsPhoneNumberId] = value.ToString();
            }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            var personId = this.PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            if ( !personId.HasValue )
            {
                // If a PersonId wasn't specified, but an AttendanceId parameter was, reload page with the PersonId
                // in the URL this will help any other blocks on this page that need to know the PersonId.
                var attendanceId = this.PageParameter( PageParameterKey.AttendanceId ).AsIntegerOrNull();
                if ( attendanceId.HasValue )
                {
                    personId = new AttendanceService( new RockContext() ).GetSelect( attendanceId.Value, s => ( int? ) s.PersonAlias.PersonId );
                    if ( personId.HasValue )
                    {
                        var extraParams = new Dictionary<string, string>();
                        extraParams.Add( PageParameterKey.PersonId, personId.ToString() );
                        NavigateToCurrentPageReference( extraParams );
                    }
                }
            }

            Guid personGuid = GetPersonGuid();

            if ( !Page.IsPostBack )
            {
                if ( IsUserAuthorized( Authorization.VIEW ) )
                {
                    if ( personGuid != Guid.Empty )
                    {
                        ShowDetail( personGuid );
                    }
                }
            }
            else
            {
                var person = new PersonService( new RockContext() ).Get( personGuid );
                if ( person != null )
                {
                    BindAttribute( person );
                }
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrPhones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrPhones_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var phoneNumber = e.Item.DataItem as PhoneNumberDisplay;

            if ( phoneNumber != null && phoneNumber.Id == SmsPhoneNumberId )
            {
                LinkButton btnSms = ( LinkButton ) e.Item.FindControl( "btnSms" );
                if ( btnSms != null )
                {
                    btnSms.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrFamily_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var familyMember = e.Item.DataItem as PersonInfo;

                Literal lFamilyPhoto = ( Literal ) e.Item.FindControl( "lFamilyPhoto" );
                lFamilyPhoto.Text = familyMember.PhotoTag;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrRelationships control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrRelationships_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                PersonInfo relatedMember = e.Item.DataItem as PersonInfo;

                Literal lRelationshipPhoto = e.Item.FindControl( "lRelationshipPhoto" ) as Literal;
                lRelationshipPhoto.Text = relatedMember.PhotoTag;

                Literal lRelationshipName = e.Item.FindControl( "lRelationshipName" ) as Literal;
                lRelationshipName.Text = relatedMember.RelationshipName;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSms_Click( object sender, EventArgs e )
        {
            nbSmsSendResult.Visible = false;
            nbSmsSendResult.Text = string.Empty;
            nbSmsError.Visible = false;
            nbSmsError.Text = string.Empty;
            mdSms.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSms_SaveClick( object sender, EventArgs e )
        {
            var systemPhoneNumberGuid = GetAttributeValue( AttributeKey.SMSFrom ).AsGuidOrNull();
            var message = tbSmsMessage.Text.Trim();

            if ( message.IsNullOrWhiteSpace() )
            {
                ResetSms();
                DisplaySmsError( "Please enter a valid message to send." );
                return;
            }
            else if ( !systemPhoneNumberGuid.HasValue )
            {
                ResetSms();
                DisplaySmsError( "Error sending message. Please try again or contact an administrator if the error continues." );
                LogException( new Exception( string.Format( "While trying to send an SMS from the Check-in Manager, the following error occurred: There is a misconfiguration with the {0} setting.", AttributeKey.SMSFrom ) ) );
                return;
            }

            var smsFromNumber = SystemPhoneNumberCache.Get( systemPhoneNumberGuid.Value );
            if ( smsFromNumber == null )
            {
                ResetSms();
                DisplaySmsError( "Could not find a valid phone number to send from." );
                LogException( new Exception( $"While trying to send an SMS from the Check-in Manager, the following error occurred: The configured System Phone Number ({systemPhoneNumberGuid}) does not exist." ) );
                return;
            }

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( GetPersonGuid() );
            var phoneNumber = GetSmsCapableMobilePhoneNumber( person );
            if ( phoneNumber == null )
            {
                ResetSms();
                DisplaySmsError( "Could not find a valid number for this person." );
                return;
            }

            /*
                6/16/26 - NA

                Check-in Manager needs volunteers to be able to reach a parent about a child's need with as
                little friction as possible, and those volunteers typically don't have access to the person
                profile to fix these fields themselves. Two v18-era safeguards silently prevent the send in
                cases we do want to override in this specific context:

                  - Rock/Communication/TransportComponent.cs (commit 369bebfb) marks the recipient Failed
                    when Person.RecordStatusValueId is Inactive.
                  - Rock/Utility/ExtensionMethods/ICollectionExtensions.cs (commit 126c8097) causes the
                    SMS transport to skip phones with IsMessagingOptedOut = true.

                Neither safeguard surfaces an error to the volunteer -- they just see "Message queued" while
                the SMS is quietly dropped. To make the send reliable, we auto-adjust the recipient's
                properties (enable SMS on the phone number, clear opt-out, reactivate the person record)
                before queueing the message. A custom person-history entry is written so admins can see the
                change was made by Check-in Manager and why.

                Reason: Check-in Manager SMS was silently dropped for inactive people or opted-out mobile numbers.
            */
            var prepResult = EnsureRecipientCanReceiveSms( rockContext, person, phoneNumber );

            // This will queue up the message.
            Rock.Communication.Medium.Sms.CreateCommunicationMobile(
                CurrentUser.Person,
                person.PrimaryAliasId,
                message,
                smsFromNumber,
                null,
                GetAttachments(),
                rockContext );

            WriteHistoryLogForSmsAutoAdjustments( rockContext, person, phoneNumber, prepResult );

            DisplaySmsSuccess( "Message queued." );
            ResetSms();
        }

        /// <summary>
        /// Handles the Command event of the btnSmsSnippet control.  This event handler is used for buttons within the rptrSmsSnippets repeater control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void btnSmsSnippet_Command( object sender, CommandEventArgs e )
        {
            if ( e.CommandName == "InputSnippet" )
            {
                SetSmsSnippet( e.CommandArgument.ToStringSafe() );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglUsePersonal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglUsePersonal_CheckedChanged( object sender, EventArgs e )
        {
            var snippets = GetSnippets();

            var showSnippets = snippets.Any();
            if ( showSnippets )
            {
                pnlSmsNoSnippets.Visible = false;
                rptrSmsSnippets.Visible = true;
                rptrSmsSnippets.DataSource = snippets;
                rptrSmsSnippets.DataBind();
            }
            else
            {
                pnlSmsNoSnippets.Visible = true;
                rptrSmsSnippets.Visible = false;
            }

            var usePersonal = tglUsePersonal.Checked;
            if ( usePersonal )
            {
                lblSmsSnippetType.Text = "personal";
            }
            else
            {
                lblSmsSnippetType.Text = "shared";
            }

        }

        #endregion Events

        #region Methods

        private Guid? _personGuid;

        /// <summary>
        /// Gets the person unique identifier.
        /// </summary>
        private Guid GetPersonGuid()
        {
            /*
                7/23/2020 - JH
                This Block was originally written specifically around Person Guid, so its usage is interwoven throughout the Block.
                We are now introducing Person ID as an alternate query string parameter, so we might get one or the other.. or both.
                Rather than re-factor all existing usages throughout the Block to be aware of either identifier, this method will
                serve as a central point to merge either identifier into a Guid result.

                Reason: Enhancing Check-in functionality.
            */

            if ( _personGuid.HasValue )
            {
                return _personGuid.Value;
            }

            Guid? personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                _personGuid = personGuid;
                return _personGuid.Value;
            }

            int? personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    _personGuid = new PersonService( rockContext ).GetGuid( personId.Value );
                }
            }

            return _personGuid ?? Guid.Empty;
        }

        /// <summary>
        /// Show the details for the given person.
        /// </summary>
        /// <param name="personGuid"></param>
        private void ShowDetail( Guid personGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var person = personService.Queryable( true, true ).Include( a => a.PhoneNumbers ).Include( a => a.RecordStatusValue )
                    .FirstOrDefault( a => a.Guid == personGuid );

                if ( person == null )
                {
                    return;
                }

                if ( GetAttributeValue( AttributeKey.ShowSharePersonButton ).AsBoolean() )
                {
                    btnShare.Visible = true;
                    var urlParams = new Dictionary<string, string> { { "PersonId", personGuid.ToString() } };
                    var url = this.LinkedPageUrl( AttributeKey.SharePersonPage, urlParams );
                    hfShareEditPersonUrl.Value = this.ResolveRockUrlIncludeRoot( url );
                }
                else
                {
                    btnShare.Visible = false;
                }

                lName.Text = person.FullName;
                lSmsRecipient.Text = person.FullName;

                string photoTag = Rock.Model.Person.GetPersonPhotoImageTag( person, 200, 200 );
                if ( person.PhotoId.HasValue )
                {
                    lPhoto.Text = string.Format( "<div class='photo'><a href='{0}'>{1}</a></div>", person.PhotoUrl, photoTag );
                }
                else
                {
                    lPhoto.Text = photoTag;
                }

                var campus = person.GetCampus();
                if ( campus != null )
                {
                    hlCampus.Visible = true;
                    hlCampus.Text = campus.Name;
                }
                else
                {
                    hlCampus.Visible = false;
                }

                lEmail.Visible = !string.IsNullOrWhiteSpace( person.Email );
                lEmail.Text = string.Format( @"<div class=""text-truncate"">{0}</div>", person.GetEmailTag( ResolveRockUrl( "/" ), "text-color" ) );

                BindAttribute( person );

                // Text Message
                var phoneNumber = GetSmsCapableMobilePhoneNumber( person );
                if ( GetAttributeValue( AttributeKey.SMSFrom ).IsNotNullOrWhiteSpace() && phoneNumber != null )
                {
                    SmsPhoneNumberId = phoneNumber.Id;
                }
                else
                {
                    SmsPhoneNumberId = 0;
                }

                // Get all family member from all families ( including self ).
                var allFamilyMembers = personService.GetFamilyMembers( person.Id, true ).ToList();

                // Add flag for this person in each family indicating if they are a child in family.
                var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                var isFamilyChild = new Dictionary<int, bool>();
                foreach ( var thisPerson in allFamilyMembers.Where( m => m.PersonId == person.Id ) )
                {
                    isFamilyChild.Add( thisPerson.GroupId, thisPerson.GroupRole.Guid.Equals( childGuid ) );
                }

                // Get the other family members and the info needed for rendering.
                var familyMembers = allFamilyMembers.Where( m => m.PersonId != person.Id )
                    .OrderBy( m => m.GroupId )
                    .ThenBy( m => m.Person.BirthDate )
                    .Select( m => new PersonInfo
                    {
                        PhotoTag = Rock.Model.Person.GetPersonPhotoImageTag( m.Person, 64, 64, className: "d-block mb-spacing-tiny" ),
                        Url = GetRelatedPersonUrl( m.Person.Guid ),
                        NickName = m.Person.NickName
                    } )
                    .ToList();

                pnlFamily.Visible = familyMembers.Any();
                rptrFamily.DataSource = familyMembers;
                rptrFamily.DataBind();

                pnlRelationships.Visible = false;
                if ( GetAttributeValue( AttributeKey.ShowRelatedPeople ).AsBoolean() )
                {
                    var roles = new List<int>();
                    var krRoles = new GroupTypeRoleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) )
                        .ToList();

                    foreach ( var role in krRoles )
                    {
                        role.LoadAttributes( rockContext );
                        if ( role.GetAttributeValue( "CanCheckin" ).AsBoolean() &&
                            role.Attributes.ContainsKey( "InverseRelationship" ) )
                        {
                            var inverseRoleGuid = role.GetAttributeValue( "InverseRelationship" ).AsGuidOrNull();
                            if ( inverseRoleGuid.HasValue )
                            {
                                var inverseRole = krRoles.FirstOrDefault( r => r.Guid == inverseRoleGuid.Value );
                                if ( inverseRole != null )
                                {
                                    roles.Add( inverseRole.Id );
                                }
                            }
                        }
                    }

                    if ( roles.Any() )
                    {
                        var relatedMembers = personService.GetRelatedPeople( new List<int> { person.Id }, roles )
                            .OrderBy( m => m.Person.LastName )
                            .ThenBy( m => m.Person.NickName )
                            .Select( m => new PersonInfo
                            {
                                PhotoTag = Rock.Model.Person.GetPersonPhotoImageTag( m.Person, 50, 50, className: "rounded" ),
                                Url = GetRelatedPersonUrl( m.Person.Guid ),
                                NickName = m.Person.NickName,
                                RelationshipName = m.GroupRole.Name
                            } )
                            .ToList();

                        pnlRelationships.Visible = relatedMembers.Any();
                        rptrRelationships.DataSource = relatedMembers;
                        rptrRelationships.DataBind();
                    }
                }

                // Exclude phone numbers whose type has been deactivated so they don't
                // appear alongside active phone numbers on the check-in Person Profile.
                var activePhoneNumberTypeIds = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) )
                    .DefinedValues
                    .Where( dv => dv.IsActive )
                    .Select( dv => dv.Id )
                    .ToList();

                var phoneNumbers = person.PhoneNumbers
                    .Where( p => p.NumberTypeValueId.HasValue
                                 && activePhoneNumberTypeIds.Contains( p.NumberTypeValueId.Value ) )
                    .Select( p => new PhoneNumberDisplay
                    {
                        Id = p.Id,
                        Number = p.IsUnlisted ? MaskDigits( p.Number ) : p.Number,
                        NumberFormatted = p.IsUnlisted ? MaskDigits( p.NumberFormatted ) : p.NumberFormatted,
                        NumberType = p.NumberTypeValue?.Value,
                        IsUnlisted = p.IsUnlisted
                    } )
                    .ToList();
                rptrPhones.DataSource = phoneNumbers;
                rptrPhones.DataBind();
                pnlContact.Visible = phoneNumbers.Any() || lEmail.Visible;

                var snippets = GetSnippets();
                if ( snippets.Any() )
                {
                    pnlSmsNoSnippets.Visible = false;
                    rptrSmsSnippets.Visible = true;
                    rptrSmsSnippets.DataSource = snippets;
                    rptrSmsSnippets.DataBind();
                }
                else
                {
                    pnlSmsNoSnippets.Visible = true;
                    rptrSmsSnippets.Visible = false;
                }
            }
        }

        /// <summary>
        /// Gets the related person URL.
        /// </summary>
        private string GetRelatedPersonUrl( Guid relatedPersonGuid )
        {
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.PersonGuid, relatedPersonGuid.ToString() }
            };

            return LinkedPageUrl( AttributeKey.PersonProfilePage, queryParams );
        }

        /// <summary>
        /// Binds the attribute to attribute value container
        /// </summary>
        private void BindAttribute( Rock.Model.Person person )
        {
            var adultCategoryGuid = GetAttributeValue( AttributeKey.AdultAttributeCategory ).AsGuidOrNull();
            var childCategoryGuid = GetAttributeValue( AttributeKey.ChildAttributeCategory ).AsGuidOrNull();
            var isAdult = person.AgeClassification == AgeClassification.Adult || person.AgeClassification == AgeClassification.Unknown;
            var isChild = person.AgeClassification == AgeClassification.Child || person.AgeClassification == AgeClassification.Unknown;

            pnlAdultFields.Visible = false;
            pnlChildFields.Visible = false;
            if ( isAdult && adultCategoryGuid.HasValue )
            {
                avcAdultAttributes.IncludedCategoryNames = new string[] { CategoryCache.Get( adultCategoryGuid.Value ).Name };
                avcAdultAttributes.AddDisplayControls( person );

                pnlAdultFields.Visible = avcAdultAttributes.GetDisplayedAttributes().Any();
            }

            if ( isChild && childCategoryGuid.HasValue )
            {
                avcChildAttributes.IncludedCategoryNames = new string[] { CategoryCache.Get( childCategoryGuid.Value ).Name };
                avcChildAttributes.AddDisplayControls( person );

                pnlChildFields.Visible = avcChildAttributes.GetDisplayedAttributes().Any();
            }
        }

        /// <summary>
        /// Returns the phone number Check-in Manager should use when sending an SMS. Prefers a phone that
        /// is already SMS-enabled and falls back to any Mobile-type number with a non-empty value -- even
        /// if it is currently opted out of messaging. The fallback allows the SMS icon to surface for
        /// people whose mobile hasn't been flagged as SMS-enabled and lets <see cref="EnsureRecipientCanReceiveSms"/>
        /// flip the necessary flags at send time.
        /// </summary>
        /// <param name="person">The person whose phone numbers should be evaluated.</param>
        /// <returns>A <see cref="PhoneNumber"/> that Check-in Manager can send an SMS to, or <c>null</c> if none exists.</returns>
        private static PhoneNumber GetSmsCapableMobilePhoneNumber( Rock.Model.Person person )
        {
            if ( person == null )
            {
                return null;
            }

            var smsEnabledPhone = person.PhoneNumbers.FirstOrDefault( n => n.IsMessagingEnabled && n.Number.IsNotNullOrWhiteSpace() );
            if ( smsEnabledPhone != null )
            {
                return smsEnabledPhone;
            }

            var mobilePhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( !mobilePhoneTypeId.HasValue )
            {
                return null;
            }

            return person.PhoneNumbers.FirstOrDefault( n =>
                n.NumberTypeValueId == mobilePhoneTypeId.Value
                && n.Number.IsNotNullOrWhiteSpace() );
        }

        /// <summary>
        /// Adjusts the supplied phone number and person so a Check-in Manager SMS will actually be sent:
        /// enables SMS messaging on the phone if disabled, clears an SMS opt-out if present, and activates
        /// the person if their record status is currently something other than Active. Saves any changes
        /// and returns a summary describing which adjustments were made so the caller can log an
        /// explanatory history entry.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> that owns the entities being modified.</param>
        /// <param name="person">The person the SMS is being sent to.</param>
        /// <param name="phoneNumber">The phone number the SMS will be sent to.</param>
        /// <returns>A <see cref="SmsRecipientPrepResult"/> describing what changed.</returns>
        private static SmsRecipientPrepResult EnsureRecipientCanReceiveSms( RockContext rockContext, Rock.Model.Person person, PhoneNumber phoneNumber )
        {
            var result = new SmsRecipientPrepResult();

            if ( phoneNumber != null && !phoneNumber.IsMessagingEnabled )
            {
                phoneNumber.IsMessagingEnabled = true;
                result.EnabledMessaging = true;
            }

            if ( phoneNumber != null && phoneNumber.IsMessagingOptedOut )
            {
                phoneNumber.IsMessagingOptedOut = false;
                phoneNumber.MessagingOptedOutDateTime = null;
                result.ClearedOptOut = true;
            }

            var activeRecordStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var deceasedRecordStatusReasonValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );
            var isDeceased = deceasedRecordStatusReasonValueId.HasValue && person != null && person.RecordStatusReasonValueId == deceasedRecordStatusReasonValueId.Value;
            if ( activeRecordStatusValueId.HasValue && person != null && person.RecordStatusValueId != activeRecordStatusValueId.Value && !isDeceased )
            {
                person.RecordStatusValueId = activeRecordStatusValueId.Value;
                person.RecordStatusReasonValueId = null;
                result.ReactivatedPerson = true;
            }

            if ( result.HasAnyChange )
            {
                rockContext.SaveChanges();
            }

            return result;
        }

        /// <summary>
        /// Writes a custom person-history entry describing the fields Check-in Manager auto-adjusted so
        /// the SMS could be sent. Rock's SaveHooks already log the raw property changes for RecordStatus
        /// and IsMessagingEnabled, so this entry supplies the "why" and covers IsMessagingOptedOut (which
        /// the SaveHook does not track). Does nothing when no adjustments were made.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> used to save the history entry.</param>
        /// <param name="person">The person whose history should be updated.</param>
        /// <param name="phoneNumber">The phone number the adjustments were made on.</param>
        /// <param name="prepResult">The set of adjustments that were made.</param>
        private static void WriteHistoryLogForSmsAutoAdjustments( RockContext rockContext, Rock.Model.Person person, PhoneNumber phoneNumber, SmsRecipientPrepResult prepResult )
        {
            if ( person == null || prepResult == null || !prepResult.HasAnyChange )
            {
                return;
            }

            var numberLabel = phoneNumber != null
                ? $"{phoneNumber.NumberTypeValue?.Value} phone ({phoneNumber.NumberFormatted})".Trim()
                : "phone number";

            var adjustments = new List<string>();
            if ( prepResult.EnabledMessaging )
            {
                adjustments.Add( $"Enabled SMS on {numberLabel}" );
            }

            if ( prepResult.ClearedOptOut )
            {
                adjustments.Add( $"Cleared SMS opt-out on {numberLabel}" );
            }

            if ( prepResult.ReactivatedPerson )
            {
                adjustments.Add( "Set Record Status to Active" );
            }

            var caption = adjustments.AsDelimited( "; " );

            var historyChanges = new History.HistoryChangeList();
            var historyChange = historyChanges.AddCustom( History.HistoryVerb.Sent.ConvertToString().ToUpper(), History.HistoryChangeType.Record.ToString(), "SMS from Check-in Manager" );
            historyChange.Caption = caption;

            HistoryService.SaveChanges(
                rockContext,
                typeof( Rock.Model.Person ),
                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                person.Id,
                historyChanges );
        }

        /// <summary>
        /// Summary of the recipient adjustments made by <see cref="EnsureRecipientCanReceiveSms"/> so the
        /// caller can compose a matching person-history entry.
        /// </summary>
        private sealed class SmsRecipientPrepResult
        {
            /// <summary>
            /// <c>true</c> if <see cref="PhoneNumber.IsMessagingEnabled"/> was flipped from <c>false</c> to <c>true</c>.
            /// </summary>
            public bool EnabledMessaging { get; set; }

            /// <summary>
            /// <c>true</c> if <see cref="PhoneNumber.IsMessagingOptedOut"/> was flipped from <c>true</c> to <c>false</c>
            /// (and <see cref="PhoneNumber.MessagingOptedOutDateTime"/> was cleared).
            /// </summary>
            public bool ClearedOptOut { get; set; }

            /// <summary>
            /// <c>true</c> if the person's <see cref="Rock.Model.Person.RecordStatusValueId"/> was changed to Active.
            /// </summary>
            public bool ReactivatedPerson { get; set; }

            /// <summary>
            /// <c>true</c> when at least one adjustment was made.
            /// </summary>
            public bool HasAnyChange => EnabledMessaging || ClearedOptOut || ReactivatedPerson;
        }

        /// <summary>
        /// Resets the SMS send feature to its default state
        /// </summary>
        private void ResetSms()
        {
            tbSmsMessage.Text = string.Empty;
            imgSmsImage.BinaryFileId = null;
        }

        /// <summary>
        /// Displays the result of an attempt to send a SMS.
        /// </summary>
        /// <param name="type">The NotificationBoxType.</param>
        /// <param name="message">The message to display.</param>
        private void DisplaySmsError( string message )
        {
            nbSmsError.Text = message;
            nbSmsError.Visible = true;
        }

        /// <summary>
        /// Displays the result of an attempt to send a SMS.
        /// </summary>
        /// <param name="type">The NotificationBoxType.</param>
        /// <param name="message">The message to display.</param>
        private void DisplaySmsSuccess( string message )
        {
            nbSmsSendResult.Text = message;
            nbSmsSendResult.Visible = true;
            mdSms.Hide();
        }

        /// <summary>
        /// Gets the attachements.
        /// </summary>
        /// <returns>A list of <see cref="BinaryFile"/> attachments or null.</returns>
        private List<BinaryFile> GetAttachments()
        {
            if ( imgSmsImage.BinaryFileId.IsNotNullOrZero() )
            {
                var binaryFile = new BinaryFileService( new RockContext() ).Get( imgSmsImage.BinaryFileId.Value );
                return new List<BinaryFile> { binaryFile };
            }

            return null;
        }

        /// <summary>
        /// Gets the SMS Snippets.
        /// </summary>
        /// <returns></returns>
        private List<Snippet> GetSnippets()
        {
            var snippetTypeGuidSms = Rock.SystemGuid.SnippetType.SMS.AsGuid();

            var rockContext = new RockContext();
            var snippetService = new SnippetService( rockContext );
            var snippetsQry = snippetService.Queryable().AsNoTracking()
                .Where( s => s.SnippetType.Guid == snippetTypeGuidSms );

            var snippetCategoryGuid = GetAttributeValue( AttributeKey.SnippetCategory ).AsGuidOrNull();
            if ( snippetCategoryGuid.HasValue )
            {
                var snippetCategoryId = CategoryCache.GetId( snippetCategoryGuid.Value );
                if ( snippetCategoryId.HasValue )
                {
                    snippetsQry = snippetsQry.Where( s => s.CategoryId == snippetCategoryId );
                }
            }

            var snippets = new List<Snippet>();
            var usePersonal = tglUsePersonal.Checked;
            if ( usePersonal )
            {
                snippetsQry = snippetsQry.Where( s => s.OwnerPersonAlias.PersonId == CurrentPersonId );
                snippetsQry = snippetsQry.OrderBy( s => s.Order ).ThenBy( s => s.Name );
                snippets = snippetsQry.ToList();
            }
            else
            {
                snippetsQry = snippetsQry.Where( s => s.OwnerPersonAliasId == null );
                snippetsQry = snippetsQry.OrderBy( s => s.Order ).ThenBy( s => s.Name );
                snippets = snippetsQry.ToList();
                // check authorization if we're not using personal snippets.
                snippets = snippets.Where( s => s.IsAuthorized( Authorization.VIEW, CurrentPerson ) ).ToList();
            }

            return snippets;
        }

        /// <summary>
        /// Sets the SMS Snippet Text.
        /// </summary>
        /// <param name="selectedSnippetId">The snippet selected by the user</param>
        private void SetSmsSnippet( string selectedSnippetId )
        {
            var snippetId = selectedSnippetId.AsIntegerOrNull();
            if ( snippetId == null )
            {
                ResetSms();
                DisplaySmsError( "The snippet could not be loaded." );
                return;
            }

            var rockContext = new RockContext();
            var snippetService = new SnippetService( rockContext );
            var personService = new PersonService( rockContext );

            Guid personGuid = GetPersonGuid();
            var person = personService.Queryable( true, true ).Include( a => a.PhoneNumbers ).Include( a => a.RecordStatusValue )
                .FirstOrDefault( a => a.Guid == personGuid );

            var snippet = snippetService.Get( snippetId.Value );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            mergeFields.Add( "Person", person );

            tbSmsMessage.Text = snippet.Content.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Replaces every digit in <paramref name="value"/> with an asterisk, preserving formatting
        /// characters such as parentheses, spaces, and dashes. Used to render unlisted phone numbers
        /// so staff can see the number type and layout without seeing the actual digits.
        /// </summary>
        private static string MaskDigits( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return value;
            }

            return Regex.Replace( value, @"\d", "*" );
        }

        #endregion Methods

        #region Helper Class

        /// <summary>
        /// The Person Information
        /// </summary>
        private class PersonInfo
        {
            /// <summary>
            /// The Photo Tag
            /// </summary>
            public string PhotoTag { get; set; }

            /// <summary>
            /// The Url
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// The Nick Name
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// The Relationship Name
            /// </summary>
            public string RelationshipName { get; set; }
        }

        /// <summary>
        /// A lightweight projection of <see cref="PhoneNumber"/> used to bind the phones repeater.
        /// Unlisted numbers are masked at projection time so the actual digits never reach the
        /// markup or the DOM.
        /// </summary>
        private class PhoneNumberDisplay
        {
            /// <summary>
            /// The <see cref="PhoneNumber.Id"/>, used to match the SMS-capable number.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The raw number, masked with asterisks when <see cref="IsUnlisted"/> is <c>true</c>.
            /// Used for the <c>tel:</c> link href.
            /// </summary>
            public string Number { get; set; }

            /// <summary>
            /// The formatted number, masked with asterisks when <see cref="IsUnlisted"/> is <c>true</c>.
            /// Used for display.
            /// </summary>
            public string NumberFormatted { get; set; }

            /// <summary>
            /// The phone number type (e.g. "Mobile", "Home").
            /// </summary>
            public string NumberType { get; set; }

            /// <summary>
            /// Whether the underlying phone number is unlisted. Bound in the markup to disable the
            /// <c>tel:</c> button so staff can't dial a number they aren't allowed to see.
            /// </summary>
            public bool IsUnlisted { get; set; }
        }

        #endregion Helper Class
    }
}