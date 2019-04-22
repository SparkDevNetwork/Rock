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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Block used to display person and details about recent check-ins
    /// </summary>
    [DisplayName( "Person Profile" )]
    [Category( "Check-in > Manager" )]
    [Description( "Displays person and details about recent check-ins." )]

    [LinkedPage( "Manager Page", "Page used to manage check-in locations", true, "", "", 0 )]
    [BooleanField( "Show Related People", "Should anyone who is allowed to check-in the current person also be displayed with the family members?", false, "", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "Send SMS From", "The phone number SMS messages should be sent from", false, false, order: 2, key: SMS_FROM_KEY )]
    [AttributeCategoryField( "Child Attribute Category", "The children Attribute Category to display attributes from.", false, "Rock.Model.Person", false, "", "", 3 )]
    [AttributeCategoryField( "Adult Attribute Category", "The adult Attribute Category to display attributes from.", false, "Rock.Model.Person", false, "", "", 4 )]
    public partial class Person : Rock.Web.UI.RockBlock
    {
        private const string SMS_FROM_KEY = "SMSFrom";
        private const string PERSON_GUID_PAGE_QUERY_KEY = "Person";

        #region Fields

        // used for private variables
        private int _deleteFieldIndex = 0;

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

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gHistory.DataKeyNames = new string[] { "Id" };
            gHistory.RowDataBound += gHistory_RowDataBound;
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
                if ( IsUserAuthorized( Authorization.VIEW ) )
                {
                    Guid? personGuid = PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuidOrNull();
                    if ( personGuid.HasValue )
                    {
                        ShowDetail( personGuid.Value );
                    }
                }
            }
            else
            {
                var person = new PersonService( new RockContext() ).Get( PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuid() );
                BindAttribute( person );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuid() );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var attendanceInfo = e.Row.DataItem as AttendanceInfo;
                if ( attendanceInfo == null )
                {
                    var cell = ( e.Row.Cells[_deleteFieldIndex] as DataControlFieldCell ).Controls[0];
                    if ( cell != null )
                    {
                        cell.Visible = false;
                    }

                }
                else
                {
                    Literal lActive = ( Literal ) e.Row.FindControl( "lActive" );
                    if ( attendanceInfo.IsActive && lActive != null )
                    {
                        e.Row.AddCssClass( "success" );
                        lActive.Text = "<span class='label label-success'>Current</span>";
                    }

                    Literal lWhoCheckedIn = ( Literal ) e.Row.FindControl( "lWhoCheckedIn" );
                    if ( lWhoCheckedIn != null && attendanceInfo.CheckInByPersonGuid.HasValue )
                    {
                        string url = String.Format( "{0}{1}{2}{3}?Person={4}", Request.Url.Scheme, Uri.SchemeDelimiter, Request.Url.Authority, Request.Url.AbsolutePath, attendanceInfo.CheckInByPersonGuid );
                        lWhoCheckedIn.Text = string.Format( "<br /><a href=\"{0}\">by: {1}</a>", url, attendanceInfo.CheckInByPersonName );
                    }
                }
            }
        }

        protected void gHistory_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new AttendanceService( rockContext );
                var attendance = service.Get( e.RowKeyId );
                if ( attendance != null )
                {
                    service.Delete( attendance );
                    rockContext.SaveChanges();
                }
            }

            ShowDetail( PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuid() );
        }

        protected void rptrFamily_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                dynamic familyMember = e.Item.DataItem as dynamic;
                Literal lFamilyIcon = ( Literal ) e.Item.FindControl( "lFamilyIcon" );

                if ( familyMember.FamilyRole.ToString() == "Child" )
                {
                    lFamilyIcon.Text = "<i class='fa fa-child'></i>";
                }
                else if ( familyMember.Gender == Gender.Female )
                {
                    lFamilyIcon.Text = "<i class='fa fa-female'></i>";
                }
                else
                {
                    lFamilyIcon.Text = "<i class='fa fa-male'></i>";
                }
            }
        }

        protected void rptrRelationships_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                dynamic relatedMember = e.Item.DataItem as dynamic;
                Literal lRelationshipsIcon = ( Literal ) e.Item.FindControl( "lRelationshipsIcon" );

                if ( relatedMember.Gender == Gender.Female )
                {
                    lRelationshipsIcon.Text = "<i class='fa fa-female'></i>";
                }
                else
                {
                    lRelationshipsIcon.Text = "<i class='fa fa-male'></i>";
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSms_Click( object sender, EventArgs e )
        {
            btnSms.Visible = false;
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
            var definedValueGuid = GetAttributeValue( SMS_FROM_KEY ).AsGuidOrNull();
            var message = tbSmsMessage.Value.Trim();

            if ( message.IsNullOrWhiteSpace() || !definedValueGuid.HasValue )
            {
                ResetSms();
                DisplayResult( NotificationBoxType.Danger, "Error sending message. Please try again or contact an administrator if the error continues." );
                if ( !definedValueGuid.HasValue )
                {
                    LogException( new Exception( string.Format( "While trying to send an SMS from the Check-in Manager, the following error occurred: There is a misconfiguration with the {0} setting.", SMS_FROM_KEY ) ) );
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
            var person = new PersonService( rockContext ).Get( PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuid() );
            var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.IsMessagingEnabled );
            if ( phoneNumber == null )
            {
                ResetSms();
                DisplayResult( NotificationBoxType.Danger, "Could not find a valid number for this person." );
                return;
            }

            var smsMessage = new RockSMSMessage();
            // NumberFormatted and NumberFormattedWithCountryCode does NOT work (this pattern is repeated in Twilio.cs)
            smsMessage.AddRecipient( new RecipientData( "+" + phoneNumber.CountryCode + phoneNumber.Number ) );
            smsMessage.FromNumber = smsFromNumber;
            smsMessage.Message = tbSmsMessage.Value;
            var errorMessages = new List<string>();
            smsMessage.Send( out errorMessages );

            if ( errorMessages.Any() )
            {
                DisplayResult( NotificationBoxType.Danger, "Error sending message. Please try again or contact an administrator if the error continues." );
                LogException( new Exception( string.Format( "While trying to send an SMS from the Check-in Manager, the following error(s) occurred: {0}", string.Join( "; ", errorMessages ) ) ) );
                return;
            }

            DisplayResult( NotificationBoxType.Success, "Message sent." );
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

        private void ShowDetail( Guid personGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var person = personService.Queryable( "PhoneNumbers.NumberTypeValue,RecordTypeValue", true, true )
                    .FirstOrDefault( a => a.Guid == personGuid );

                if ( person != null )
                {
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

                    lGender.Text = person.Gender != Gender.Unknown ? person.Gender.ConvertToString() : "";

                    if ( person.BirthDate.HasValue )
                    {
                        string ageText = ( person.BirthYear.HasValue && person.BirthYear != DateTime.MinValue.Year ) ?
                            string.Format( "{0} yrs old ", person.BirthDate.Value.Age() ) : string.Empty;
                        lAge.Text = string.Format( "{0} <small>({1})</small><br/>", ageText, person.BirthDate.Value.ToShortDateString() );
                    }
                    else
                    {
                        lAge.Text = string.Empty;
                    }

                    lGrade.Text = person.GradeFormatted;

                    lEmail.Visible = !string.IsNullOrWhiteSpace( person.Email );
                    lEmail.Text = person.GetEmailTag( ResolveRockUrl( "/" ), "btn btn-default", "<i class='fa fa-envelope'></i>" );

                    BindAttribute( person );
                    // Text Message
                    var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.IsMessagingEnabled && n.Number.IsNotNullOrWhiteSpace() );
                    if ( GetAttributeValue( SMS_FROM_KEY ).IsNotNullOrWhiteSpace() && phoneNumber != null )
                    {
                        btnSms.Text = string.Format( "<i class='fa fa-mobile'></i> {0} <small>({1})</small>", phoneNumber.NumberFormatted, phoneNumber.NumberTypeValue );
                        btnSms.Visible = true;
                        rcwTextMessage.Label = "Text Message";
                    }
                    else
                    {
                        btnSms.Visible = false;
                        rcwTextMessage.Label = string.Empty;
                    }

                    // Get all family member from all families ( including self )
                    var allFamilyMembers = personService.GetFamilyMembers( person.Id, true ).ToList();

                    // Add flag for this person in each family indicating if they are a child in family.
                    var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                    var isFamilyChild = new Dictionary<int, bool>();
                    foreach ( var thisPerson in allFamilyMembers.Where( m => m.PersonId == person.Id ) )
                    {
                        isFamilyChild.Add( thisPerson.GroupId, thisPerson.GroupRole.Guid.Equals( childGuid ) );
                    }

                    // Get the current url's root (without the person's guid)
                    string urlRoot = Request.Url.ToString().ReplaceCaseInsensitive( personGuid.ToString(), "" );

                    // Get the other family members and the info needed for rendering
                    var familyMembers = allFamilyMembers.Where( m => m.PersonId != person.Id )
                        .OrderBy( m => m.GroupId )
                        .ThenBy( m => m.Person.BirthDate )
                        .Select( m => new
                        {
                            Url = urlRoot + m.Person.Guid.ToString(),
                            FullName = m.Person.FullName,
                            Gender = m.Person.Gender,
                            FamilyRole = m.GroupRole,
                            Note = isFamilyChild[m.GroupId] ?
                                ( m.GroupRole.Guid.Equals( childGuid ) ? " (Sibling)" : "(Parent)" ) :
                                ( m.GroupRole.Guid.Equals( childGuid ) ? " (Child)" : "" )
                        } )
                        .ToList();

                    rcwFamily.Visible = familyMembers.Any();
                    rptrFamily.DataSource = familyMembers;
                    rptrFamily.DataBind();

                    rcwRelationships.Visible = false;
                    if ( GetAttributeValue( "ShowRelatedPeople" ).AsBoolean() )
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
                                .Select( m => new
                                {
                                    Url = urlRoot + m.Person.Guid.ToString(),
                                    FullName = m.Person.FullName,
                                    Gender = m.Person.Gender,
                                    Note = " (" + m.GroupRole.Name + ")"
                                } )
                                .ToList();

                            rcwRelationships.Visible = relatedMembers.Any();
                            rptrRelationships.DataSource = relatedMembers;
                            rptrRelationships.DataBind();
                        }
                    }

                    var phoneNumbers = person.PhoneNumbers.Where( p => !p.IsUnlisted ).ToList();
                    rptrPhones.DataSource = phoneNumbers;
                    rptrPhones.DataBind();
                    rcwPhone.Visible = phoneNumbers.Any();

                    var schedules = new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                        .ToList();

                    var scheduleIds = schedules.Select( s => s.Id ).ToList();

                    int? personAliasId = person.PrimaryAliasId;

                    PersonAliasService personAliasService = new PersonAliasService( rockContext );
                    if ( !personAliasId.HasValue )
                    {
                        personAliasId = personAliasService.GetPrimaryAliasId( person.Id );
                    }

                    var attendances = new AttendanceService( rockContext )
                        .Queryable( "Occurrence.Schedule,Occurrence.Group,Occurrence.Location,AttendanceCode" )
                        .Where( a =>
                            a.PersonAliasId.HasValue &&
                            a.PersonAliasId == personAliasId &&
                            a.Occurrence.ScheduleId.HasValue &&
                            a.Occurrence.GroupId.HasValue &&
                            a.Occurrence.LocationId.HasValue &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            scheduleIds.Contains( a.Occurrence.ScheduleId.Value ) )
                        .OrderByDescending( a => a.StartDateTime )
                        .Take( 20 )
                        .ToList()                                                             // Run query to get recent most 20 checkins
                        .OrderByDescending( a => a.Occurrence.OccurrenceDate )                // Then sort again by startdatetime and schedule start (which is not avail to sql query )
                        .ThenByDescending( a => a.Occurrence.Schedule.StartTimeOfDay )
                        .ToList()
                        .Select( a =>
                        {
                            var checkedInByPerson = a.CheckedInByPersonAliasId.HasValue ? personAliasService.GetPerson( a.CheckedInByPersonAliasId.Value ) : null;

                            return new AttendanceInfo
                            {
                                Id = a.Id,
                                Date = a.StartDateTime,
                                GroupId = a.Occurrence.Group.Id,
                                Group = a.Occurrence.Group.Name,
                                LocationId = a.Occurrence.LocationId.Value,
                                Location = a.Occurrence.Location.Name,
                                Schedule = a.Occurrence.Schedule.Name,
                                IsActive = a.IsCurrentlyCheckedIn,
                                Code = a.AttendanceCode != null ? a.AttendanceCode.Code : "",
                                CheckInByPersonName = checkedInByPerson != null ? checkedInByPerson.FullName : string.Empty,
                                CheckInByPersonGuid = checkedInByPerson != null ? checkedInByPerson.Guid : ( Guid? ) null
                            };
                        }
                        ).ToList();

                    // Set active locations to be a link to the room in manager page
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "Group", "" );
                    qryParam.Add( "Location", "" );
                    foreach ( var attendance in attendances.Where( a => a.IsActive ) )
                    {
                        qryParam["Group"] = attendance.GroupId.ToString();
                        qryParam["Location"] = attendance.LocationId.ToString();
                        attendance.Location = string.Format( "<a href='{0}'>{1}</a>",
                            LinkedPageUrl( "ManagerPage", qryParam ), attendance.Location );
                    }

                    rcwCheckinHistory.Visible = attendances.Any();

                    // Get the index of the delete column
                    var deleteField = gHistory.Columns.OfType<Rock.Web.UI.Controls.DeleteField>().First();
                    _deleteFieldIndex = gHistory.Columns.IndexOf( deleteField );

                    gHistory.DataSource = attendances;
                    gHistory.DataBind();
                }
            }
        }

        /// <summary>
        /// Binds the attribute to attribute value container
        /// </summary>
        private void BindAttribute( Rock.Model.Person person )
        {
            var adultCategoryGuid = GetAttributeValue( "AdultAttributeCategory" ).AsGuidOrNull();
            var childCategoryGuid = GetAttributeValue( "ChildAttributeCategory" ).AsGuidOrNull();
            var isAdult = person.AgeClassification == AgeClassification.Adult || person.AgeClassification == AgeClassification.Unknown;
            var isChild = person.AgeClassification == AgeClassification.Child || person.AgeClassification == AgeClassification.Unknown;

            pnlAdultFields.Visible = isAdult && adultCategoryGuid.HasValue;
            pnlChildFields.Visible = isChild && childCategoryGuid.HasValue;
            if ( isAdult && adultCategoryGuid.HasValue )
            {
                avcAdultAttributes.IncludedCategoryNames = new string[] { CategoryCache.Get( adultCategoryGuid.Value ).Name };
                avcAdultAttributes.AddDisplayControls( person );
            }

            if ( isChild && childCategoryGuid.HasValue )
            {
                avcChildAttributes.IncludedCategoryNames = new string[] { CategoryCache.Get( childCategoryGuid.Value ).Name };
                avcChildAttributes.AddDisplayControls( person );
            }
        }

        /// <summary>
        /// Resets the SMS send feature to its default state
        /// </summary>
        private void ResetSms()
        {
            // If this method got called, we've already checked the person has a valid number
            btnSms.Visible = true;
            tbSmsMessage.Visible = btnSmsCancel.Visible = btnSmsSend.Visible = false;
            tbSmsMessage.Value = String.Empty;
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

        public class AttendanceInfo
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public int GroupId { get; set; }
            public string Group { get; set; }
            public int LocationId { get; set; }
            public string Location { get; set; }
            public string Schedule { get; set; }
            public bool IsActive { get; set; }
            public string Code { get; set; }
            public string CheckInByPersonName { get; set; }
            public Guid? CheckInByPersonGuid { get; set; }
        }

        #endregion
    }
}