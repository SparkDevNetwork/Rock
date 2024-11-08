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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// ######### OBSOLETE: Use PersonLeft/PersonRight instead #######
    /// </summary>
    [DisplayName( "Person Profile (Obsolete)" )]
    [RockObsolete( "1.12" )]
    [Category( "Check-in > Manager" )]
    [Description( "Obsolete: Use PersonLeft/PersonRight instead" )]

    [LinkedPage(
        "Manager Page",
        Key = AttributeKey.ManagerPage,
        Description = "Page used to manage check-in locations",
        IsRequired = true,
        Order = 0 )]

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
        "Allow Label Reprinting",
        Key = AttributeKey.AllowLabelReprinting,
        Description = "Determines if reprinting labels should be allowed.",
        DefaultBooleanValue = false,
        Category = "Manager Settings",
        Order = 5 )]

    [BadgesField(
        "Badges - Left",
        Key = AttributeKey.BadgesLeft,
        Description = "The badges to display on the left side of the badge bar.",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Badge.FAMILY_ATTENDANCE,
        Order = 6 )]

    [BadgesField(
        "Badges - Right",
        Key = AttributeKey.BadgesRight,
        Description = "The badges to display on the right side of the badge bar.",
        IsRequired = false,
        DefaultValue =
            Rock.SystemGuid.Badge.LAST_VISIT_ON_EXTERNAL_SITE + ","
            + Rock.SystemGuid.Badge.FAMILY_16_WEEK_ATTENDANCE + ","
            + Rock.SystemGuid.Badge.BAPTISM + ","
            + Rock.SystemGuid.Badge.IN_SERVING_TEAM,
        Order = 7 )]
    [Rock.SystemGuid.BlockTypeGuid( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1" )]
    public partial class Person : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ManagerPage = "ManagerPage";
            public const string ShowRelatedPeople = "ShowRelatedPeople";
            public const string SMSFrom = "SMSFrom";
            public const string ChildAttributeCategory = "ChildAttributeCategory";
            public const string AdultAttributeCategory = "AdultAttributeCategory";
            public const string AllowLabelReprinting = "AllowLabelReprinting";
            public const string BadgesLeft = "BadgesLeft";
            public const string BadgesRight = "BadgesRight";
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
            /// The location identifier
            /// </summary>
            public const string LocationId = "LocationId";
        }

        #endregion

        #region Fields

        // used for private variables
        private int _deleteFieldIndex = 0;

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


            var leftBadgeGuids = GetAttributeValues( AttributeKey.BadgesLeft ).AsGuidList();
            var rightBadgeGuids = GetAttributeValues( AttributeKey.BadgesRight ).AsGuidList();

            var leftBadges = leftBadgeGuids.Select( a => BadgeCache.Get( a ) ).Where( a => a != null ).OrderBy( a => a.Order ).ToList();
            var rightBadges = rightBadgeGuids.Select( a => BadgeCache.Get( a ) ).Where( a => a != null ).OrderBy( a => a.Order ).ToList();

            // set BadgeEntity using a new RockContext that won't get manually disposed
            var badgesEntity = new PersonService( new RockContext() ).Get( GetPersonGuid() );
            blBadgesLeft.Entity = badgesEntity;
            blBadgesRight.Entity = badgesEntity;

            foreach ( var badge in leftBadges )
            {
                blBadgesLeft.BadgeTypes.Add( badge );
            }

            foreach ( var badge in rightBadges )
            {
                blBadgesRight.BadgeTypes.Add( badge );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
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
            ShowDetail( GetPersonGuid() );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var attendanceInfo = e.Row.DataItem as AttendanceInfo;
            if ( attendanceInfo == null )
            {
                var cell = ( e.Row.Cells[_deleteFieldIndex] as DataControlFieldCell ).Controls[0];
                if ( cell != null )
                {
                    cell.Visible = false;
                }

                return;
            }

            var lCheckinDate = ( Literal ) e.Row.FindControl( "lCheckinDate" );
            var lCheckinScheduleName = ( Literal ) e.Row.FindControl( "lCheckinScheduleName" );
            var lWhoCheckedIn = ( Literal ) e.Row.FindControl( "lWhoCheckedIn" );
            lCheckinDate.Text = attendanceInfo.Date.ToShortDateString();
            lCheckinScheduleName.Text = attendanceInfo.ScheduleName;
            if ( lWhoCheckedIn != null && attendanceInfo.CheckInByPersonGuid.HasValue )
            {
                var proxySafeUrl = Request.UrlProxySafe();
                var oldWayUrl = $"{proxySafeUrl.Scheme}{Uri.SchemeDelimiter}{proxySafeUrl.Authority}{proxySafeUrl.AbsolutePath}?Person={attendanceInfo.CheckInByPersonGuid}";
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "Person", attendanceInfo.CheckInByPersonGuid.ToString() );
                var urlWithPersonParameter = GetCurrentPageUrl( queryParams );
                lWhoCheckedIn.Text = string.Format( "<br /><a href=\"{0}\">by: {1}</a>", urlWithPersonParameter, attendanceInfo.CheckInByPersonName );
            }


            var lLocationName = ( Literal ) e.Row.FindControl( "lLocationName" );
            var lGroupName = ( Literal ) e.Row.FindControl( "lGroupName" );
            var lCode = ( Literal ) e.Row.FindControl( "lCode" );
            var lActiveLabel = ( Literal ) e.Row.FindControl( "lActiveLabel" );
            lLocationName.Text = attendanceInfo.LocationNameHtml;
            lGroupName.Text = attendanceInfo.GroupName;
            lCode.Text = attendanceInfo.Code;

            if ( attendanceInfo.IsActive && lActiveLabel != null )
            {
                e.Row.AddCssClass( "success" );
                lActiveLabel.Text = "<span class='label label-success'>Current</span>";
                var attendanceIds = hfCurrentAttendanceIds.Value.SplitDelimitedValues().ToList();
                attendanceIds.Add( attendanceInfo.Id.ToStringSafe() );
                hfCurrentAttendanceIds.Value = attendanceIds.AsDelimited( "," );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
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

            ShowDetail( GetPersonGuid() );
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

                Literal lRelationshipPhoto = ( Literal ) e.Item.FindControl( "lRelationshipPhoto" );
                lRelationshipPhoto.Text = relatedMember.PhotoTag;
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
            var systemPhoneNumberGuid = GetAttributeValue( AttributeKey.SMSFrom ).AsGuidOrNull();
            var message = tbSmsMessage.Value.Trim();

            if ( message.IsNullOrWhiteSpace() || !systemPhoneNumberGuid.HasValue )
            {
                ResetSms();
                DisplayResult( NotificationBoxType.Danger, "Error sending message. Please try again or contact an administrator if the error continues." );
                if ( !systemPhoneNumberGuid.HasValue )
                {
                    LogException( new Exception( string.Format( "While trying to send an SMS from the Check-in Manager, the following error occurred: There is a misconfiguration with the {0} setting.", AttributeKey.SMSFrom ) ) );
                }

                return;
            }

            var smsFromNumber = SystemPhoneNumberCache.Get( systemPhoneNumberGuid.Value );
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

        #region Reprint Labels
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnReprintLabels_Click( object sender, EventArgs e )
        {
            nbReprintMessage.Visible = false;

            Guid personGuid = GetPersonGuid();

            if ( personGuid == Guid.Empty )
            {
                maNoLabelsFound.Show( "No person was found.", ModalAlertType.Alert );
                return;
            }

            if ( string.IsNullOrEmpty( hfCurrentAttendanceIds.Value ) )
            {
                maNoLabelsFound.Show( "No labels were found for re-printing.", ModalAlertType.Alert );
                return;
            }

            var rockContext = new RockContext();

            var attendanceIds = hfCurrentAttendanceIds.Value.SplitDelimitedValues().AsIntegerList();

            // Get the person Id from the PersonId page parameter, or look it up based on the Person Guid page parameter.
            int? personIdParam = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            int personId = personIdParam.HasValue
                ? personIdParam.Value
                : new PersonService( rockContext ).GetId( personGuid ).GetValueOrDefault();

            hfPersonId.Value = personId.ToString();

            var possibleLabels = ZebraPrint.GetLabelTypesForPerson( personId, attendanceIds );

            if ( possibleLabels != null && possibleLabels.Count != 0 )
            {
                cblLabels.DataSource = possibleLabels;
                cblLabels.DataBind();
            }
            else
            {
                maNoLabelsFound.Show( "No labels were found for re-printing.", ModalAlertType.Alert );
                return;
            }

            cblLabels.DataSource = ZebraPrint.GetLabelTypesForPerson( personId, attendanceIds ).OrderBy( l => l.Name );
            cblLabels.DataBind();

            // Bind the printers list
            var printers = new DeviceService( rockContext )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .OrderBy( d => d.Name )
                .ToList();

            if ( printers == null || printers.Count == 0 )
            {
                maNoLabelsFound.Show( "Due to browser limitations, only server based printers are supported and none are defined on this server.", ModalAlertType.Information );
                return;
            }

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = printers;
            ddlPrinter.DataBind();
            ddlPrinter.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

            nbReprintLabelMessages.Text = string.Empty;
            mdReprintLabels.Show();
        }

        /// <summary>
        /// Handles sending the selected labels off to the selected printer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void mdReprintLabels_PrintClick( object sender, EventArgs e )
        {
            var personId = hfPersonId.ValueAsInt();
            if ( personId == 0 )
            {
                return;
            }

            if ( string.IsNullOrWhiteSpace( cblLabels.SelectedValue ) )
            {
                nbReprintLabelMessages.Visible = true;
                nbReprintLabelMessages.Text = "Please select at least one label.";
                return;
            }

            if ( ddlPrinter.SelectedValue == null || ddlPrinter.SelectedValue == None.IdValue )
            {
                nbReprintLabelMessages.Visible = true;
                nbReprintLabelMessages.Text = "Please select a printer.";
                return;
            }

            // Get the person Id from the Guid
            var selectedAttendanceIds = hfCurrentAttendanceIds.Value.SplitDelimitedValues().AsIntegerList();

            var fileGuids = cblLabels.SelectedValues.AsGuidList();

            var reprintLabelOptions = new ReprintLabelOptions
            {
                PrintFrom = PrintFrom.Server,
                ServerPrinterIPAddress = ddlPrinter.SelectedValue
            };

            // Now, finally, re-print the labels.
            List<string> messages = ZebraPrint.ReprintZebraLabels( fileGuids, personId, selectedAttendanceIds, nbReprintMessage, this.Request, reprintLabelOptions );
            nbReprintMessage.Visible = true;
            nbReprintMessage.Text = messages.JoinStrings( "<br>" );

            mdReprintLabels.Hide();
        }

        #endregion

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
            btnReprintLabels.Visible = GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean();

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var person = personService.Queryable( true, true ).Include( a => a.PhoneNumbers ).Include( a => a.RecordStatusValue )
                    .FirstOrDefault( a => a.Guid == personGuid );

                if ( person == null )
                {
                    return;
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

                lGender.Text = person.Gender != Gender.Unknown ?
                    string.Format( @"<div class=""text-semibold text-uppercase"">{0}</div>", person.Gender.ConvertToString().Substring( 0, 1 ) ) : string.Empty;

                if ( person.BirthDate.HasValue )
                {
                    string ageText = ( person.BirthYear.HasValue && person.BirthYear != DateTime.MinValue.Year ) ?
                        string.Format( @"<div class=""text-semibold"">{0}yrs</div>", person.BirthDate.Value.Age() ) : string.Empty;
                    lAge.Text = string.Format( @"{0}<div class=""text-sm text-muted"">{1}</div>", ageText, person.BirthDate.Value.ToShortDateString() );
                }
                else
                {
                    lAge.Text = string.Empty;
                }

                string grade = person.GradeFormatted;
                string[] gradeParts = grade.Split( ' ' );
                if ( gradeParts.Length >= 2 )
                {
                    // Note that Grade names might be different in other countries. See  https://separatedbyacommonlanguage.blogspot.com/2006/12/types-of-schools-school-years.html for examples
                    var firstWord = gradeParts[0];
                    var remainderWords = gradeParts.Skip( 1 ).ToList().AsDelimited( " " );
                    if ( firstWord.Equals( "Year", StringComparison.OrdinalIgnoreCase ) )
                    {
                        // MDP 2020-10-21 (at request of GJ)
                        // Special case if formatted grade is 'Year 1', 'Year 2', etc (see https://separatedbyacommonlanguage.blogspot.com/2006/12/types-of-schools-school-years.html)
                        // Make the word Year on the top
                        grade = string.Format( @"<div class=""text-semibold"">{0}</div><div class=""text-sm text-muted"">{1}</div>", remainderWords, firstWord );
                    }
                    else
                    {
                        grade = string.Format( @"<div class=""text-semibold"">{0}</div><div class=""text-sm text-muted"">{1}</div>", firstWord, remainderWords );
                    }
                }

                lGrade.Text = grade;

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

                // Get all family member from all families ( including self )
                var allFamilyMembers = personService.GetFamilyMembers( person.Id, true ).ToList();

                // Add flag for this person in each family indicating if they are a child in family.
                var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                var isFamilyChild = new Dictionary<int, bool>();
                foreach ( var thisPerson in allFamilyMembers.Where( m => m.PersonId == person.Id ) )
                {
                    isFamilyChild.Add( thisPerson.GroupId, thisPerson.GroupRole.Guid.Equals( childGuid ) );
                }

                // Get the other family members and the info needed for rendering
                var familyMembers = allFamilyMembers.Where( m => m.PersonId != person.Id )
                    .OrderBy( m => m.GroupId )
                    .ThenBy( m => m.Person.BirthDate )
                    .Select( m => new PersonInfo
                    {
                        PhotoTag = Rock.Model.Person.GetPersonPhotoImageTag( m.Person, 64, 64, className: "d-block mb-1" ),
                        Url = GetRelatedPersonUrl( person, m.Person.Guid, m.Person.Id ),
                        NickName = m.Person.NickName,
                        //FullName = m.Person.FullName,
                        //Gender = m.Person.Gender,
                        //FamilyRole = m.GroupRole,
                        //Note = isFamilyChild[m.GroupId] ?
                        //    ( m.GroupRole.Guid.Equals( childGuid ) ? " (Sibling)" : "(Parent)" ) :
                        //    ( m.GroupRole.Guid.Equals( childGuid ) ? " (Child)" : "" )
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
                                //FullName = m.Person.FullName,
                                //Gender = m.Person.Gender,
                                //Note = " (" + m.GroupRole.Name + ")"
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

                var schedules = new ScheduleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                    .ToList();

                var scheduleIds = schedules.Select( s => s.Id ).ToList();

                var personAliasIds = person.Aliases.Select( a => a.Id ).ToList();

                PersonAliasService personAliasService = new PersonAliasService( rockContext );

                var attendances = new AttendanceService( rockContext )
                    .Queryable( "Occurrence.Schedule,Occurrence.Group,Occurrence.Location,AttendanceCode" )
                    .Where( a =>
                        a.PersonAliasId.HasValue &&
                        personAliasIds.Contains( a.PersonAliasId.Value ) &&
                        a.Occurrence.ScheduleId.HasValue &&
                        a.Occurrence.GroupId.HasValue &&
                        a.Occurrence.LocationId.HasValue &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        scheduleIds.Contains( a.Occurrence.ScheduleId.Value ) )
                    .OrderByDescending( a => a.StartDateTime )
                    .Take( 20 )
                    .ToList()                                                             // Run query to get recent most 20 checkins
                    .OrderByDescending( a => a.Occurrence.OccurrenceDate )                // Then sort again by start datetime and schedule start (which is not avail to sql query )
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
                            GroupName = a.Occurrence.Group.Name,
                            LocationId = a.Occurrence.LocationId.Value,
                            LocationName = a.Occurrence.Location.Name,
                            ScheduleName = a.Occurrence.Schedule.Name,
                            IsActive = a.IsCurrentlyCheckedIn,
                            Code = a.AttendanceCode != null ? a.AttendanceCode.Code : "",
                            CheckInByPersonName = checkedInByPerson != null ? checkedInByPerson.FullName : string.Empty,
                            CheckInByPersonGuid = checkedInByPerson != null ? checkedInByPerson.Guid : ( Guid? ) null
                        };
                    }
                    ).ToList();

                // Set active locations to be a link to the room in manager page
                var qryParams = new Dictionary<string, string>
                {
                    { PageParameterKey.LocationId, string.Empty }
                };

                // If an Area Guid was passed to the Page, pass it back.
                string areaGuid = PageParameter( PageParameterKey.AreaGuid );
                if ( areaGuid.IsNotNullOrWhiteSpace() )
                {
                    qryParams.Add( PageParameterKey.AreaGuid, areaGuid );
                }

                foreach ( var attendance in attendances )
                {
                    if ( attendance.IsActive )
                    {
                        qryParams[PageParameterKey.LocationId] = attendance.LocationId.ToString();
                        attendance.LocationNameHtml = string.Format(
                            "<a href='{0}'>{1}</a>",
                            LinkedPageUrl( AttributeKey.ManagerPage, qryParams ),
                            attendance.LocationName );
                    }
                    else
                    {
                        attendance.LocationNameHtml = attendance.LocationName;
                    }
                }

                pnlCheckinHistory.Visible = attendances.Any();

                // Get the index of the delete column
                var deleteField = gHistory.Columns.OfType<Rock.Web.UI.Controls.DeleteField>().First();
                _deleteFieldIndex = gHistory.Columns.IndexOf( deleteField );

                gHistory.DataSource = attendances;
                gHistory.DataBind();
            }
        }

        /// <summary>
        /// Gets the related person URL.
        /// </summary>
        private string GetRelatedPersonUrl( Rock.Model.Person currentPerson, Guid relatedPersonGuid, int relatedPersonId )
        {
            var template = "{0}={1}";
            var relatedPersonUrl = Request.UrlProxySafe().ToString()
                .ReplaceCaseInsensitive( string.Format( template, PageParameterKey.PersonGuid, currentPerson.Guid ), string.Format( template, PageParameterKey.PersonGuid, relatedPersonGuid ) )
                .ReplaceCaseInsensitive( string.Format( template, PageParameterKey.PersonId, currentPerson.Id ), string.Format( template, PageParameterKey.PersonId, relatedPersonId ) );

            return relatedPersonUrl;
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
            // If this method got called, we've already checked the person has a valid number
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
            public string GroupName { get; set; }
            public int LocationId { get; set; }
            public string LocationName { get; set; }
            public string LocationNameHtml { get; set; }
            public string ScheduleName { get; set; }
            public bool IsActive { get; set; }
            public string Code { get; set; }
            public string CheckInByPersonName { get; set; }
            public Guid? CheckInByPersonGuid { get; set; }
        }

        private class PersonInfo
        {
            public string PhotoTag { get; set; }
            public string Url { get; set; }
            public string NickName { get; set; }
        }

        #endregion
    }
}