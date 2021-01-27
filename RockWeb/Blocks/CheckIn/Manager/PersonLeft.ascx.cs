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
    /// </summary>
    [DisplayName( "Person Profile " )]
    [Category( "Check-in > Manager" )]
    [Description( "Displays person details for a checked-in person" )]

    [BooleanField(
        "Show Related People",
        Key = AttributeKey.ShowRelatedPeople,
        Description = "Should anyone who is allowed to check-in the current person also be displayed with the family members?",
        IsRequired = false,
        Order = 1 )]

    [DefinedValueField(
        "Send SMS From",
        Key = AttributeKey.SMSFrom,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
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
        }

        #endregion

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string SmsPhoneNumberId = "SmsPhoneNumberId";
        }

        #endregion ViewState Keys

        #region Page Parameter Constants

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

        #endregion

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

        #endregion

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
            base.OnLoad( e );

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
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrPhones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrPhones_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            PhoneNumber phoneNumber = e.Item.DataItem as PhoneNumber;

            if ( phoneNumber.Id == SmsPhoneNumberId )
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
            nbResult.Visible = false;
            nbResult.Text = string.Empty;
            tbSmsMessage.Visible = btnSmsCancel.Visible = btnSmsSend.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            var definedValueGuid = GetAttributeValue( AttributeKey.SMSFrom ).AsGuidOrNull();
            var message = tbSmsMessage.Value.Trim();

            if ( message.IsNullOrWhiteSpace() || !definedValueGuid.HasValue )
            {
                ResetSms();
                DisplayResult( NotificationBoxType.Danger, "Error sending message. Please try again or contact an administrator if the error continues." );
                if ( !definedValueGuid.HasValue )
                {
                    LogException( new Exception( string.Format( "While trying to send an SMS from the Check-in Manager, the following error occurred: There is a misconfiguration with the {0} setting.", AttributeKey.SMSFrom ) ) );
                }

                return;
            }

            var smsFromNumber = DefinedValueCache.Get( definedValueGuid.Value );
            if ( smsFromNumber == null )
            {
                ResetSms();
                DisplayResult( NotificationBoxType.Danger, "Could not find a valid phone number to send from." );
                return;
            }

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( GetPersonGuid() );
            var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.IsMessagingEnabled );
            if ( phoneNumber == null )
            {
                ResetSms();
                DisplayResult( NotificationBoxType.Danger, "Could not find a valid number for this person." );
                return;
            }

            // This will queue up the message
            Rock.Communication.Medium.Sms.CreateCommunicationMobile(
                CurrentUser.Person,
                person.PrimaryAliasId,
                message,
                smsFromNumber,
                null,
                rockContext );

            DisplayResult( NotificationBoxType.Success, "Message queued." );
            ResetSms();
        }

        /// <summary>
        /// Handles the Click event of the btnSmsCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSmsCancel_Click( object sender, EventArgs e )
        {
            nbResult.Visible = false;
            ResetSms();
        }

        #endregion

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
                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.IsMessagingEnabled && n.Number.IsNotNullOrWhiteSpace() );
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
                        PhotoTag = Rock.Model.Person.GetPersonPhotoImageTag( m.Person, 64, 64, className: "d-block mb-1" ),
                        Url = GetRelatedPersonUrl( person, m.Person.Guid, m.Person.Id ),
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
                                Url = GetRelatedPersonUrl( person, m.Person.Guid, m.Person.Id ),
                                NickName = m.Person.NickName,
                                RelationshipName = m.GroupRole.Name
                            } )
                            .ToList();

                        pnlRelationships.Visible = relatedMembers.Any();
                        rptrRelationships.DataSource = relatedMembers;
                        rptrRelationships.DataBind();
                    }
                }

                var phoneNumbers = person.PhoneNumbers.Where( p => !p.IsUnlisted ).ToList();
                rptrPhones.DataSource = phoneNumbers;
                rptrPhones.DataBind();
                pnlContact.Visible = phoneNumbers.Any() || lEmail.Visible;
            }
        }

        /// <summary>
        /// Gets the related person URL.
        /// </summary>
        private string GetRelatedPersonUrl( Rock.Model.Person currentPerson, Guid relatedPersonGuid, int relatedPersonId )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( PageParameterKey.PersonGuid, relatedPersonGuid.ToString() );

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
        /// Resets the SMS send feature to its default state
        /// </summary>
        private void ResetSms()
        {
            // If this method got called, we've already checked the person has a valid number.
            tbSmsMessage.Visible = btnSmsCancel.Visible = btnSmsSend.Visible = false;
            tbSmsMessage.Value = string.Empty;
        }

        /// <summary>
        /// Displays the result of an attempt to send a SMS.
        /// </summary>
        /// <param name="type">The NotificationBoxType.</param>
        /// <param name="message">The message to display.</param>
        private void DisplayResult( NotificationBoxType type, string message )
        {
            nbResult.NotificationBoxType = type;
            nbResult.Text = message;
            nbResult.Visible = true;
        }

        #endregion

        #region Helper Classes

        private class PersonInfo
        {
            public string PhotoTag { get; set; }

            public string Url { get; set; }

            public string NickName { get; set; }

            public string RelationshipName { get; set; }
        }

        #endregion
    }
}