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
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Person Recent Attendances" )]
    [Category( "Check-in > Manager" )]
    [Description( "Shows most recent attendances for a person." )]

    [SecurityAction( SecurityActionKey.ReprintLabels, "The roles and/or users that can reprint labels for the selected person." )]

    [LinkedPage(
        "Manager Page",
        Key = AttributeKey.ManagerPage,
        Description = "Page used to manage check-in locations",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage(
        "Attendance Detail Page",
        Key = AttributeKey.AttendanceDetailPage,
        Description = "Page to show details of an attendance.",
        DefaultValue = Rock.SystemGuid.Page.CHECK_IN_MANAGER_ATTENDANCE_DETAIL,
        IsRequired = true,
        Order = 1 )]

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

    [LinkedPage(
        "Attendance History Page",
        Key = AttributeKey.PersonAttendanceHistoryPage,
        Description = "Page to shows a history of changes to person's attendances.",
        DefaultValue = Rock.SystemGuid.Page.CHECK_IN_MANAGER_PERSON_ATTENDANCE_CHANGE_HISTORY,
        IsRequired = true,
        Order = 8 )]
    public partial class PersonRight : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ManagerPage = "ManagerPage";
            public const string AllowLabelReprinting = "AllowLabelReprinting";
            public const string BadgesLeft = "BadgesLeft";
            public const string BadgesRight = "BadgesRight";
            public const string AttendanceDetailPage = "AttendanceDetailPage";
            public const string PersonAttendanceHistoryPage = "PersonAttendanceHistoryPage";
        }

        #endregion

        #region Security Actions

        private static class SecurityActionKey
        {
            public const string ReprintLabels = "ReprintLabels";
        }

        #endregion

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

            /// <summary>
            /// The attendance identifier parameter (if Person isn't specified in URL, get the Person from the Attendance instead
            /// </summary>
            public const string AttendanceId = "AttendanceId";
        }

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gAttendanceHistory.DataKeyNames = new string[] { "Id" };

            var leftBadgeGuids = GetAttributeValues( AttributeKey.BadgesLeft ).AsGuidList();
            var rightBadgeGuids = GetAttributeValues( AttributeKey.BadgesRight ).AsGuidList();

            var leftBadges = leftBadgeGuids.Select( a => BadgeCache.Get( a ) ).Where( a => a != null ).OrderBy( a => a.Order ).ToList();
            var rightBadges = rightBadgeGuids.Select( a => BadgeCache.Get( a ) ).Where( a => a != null ).OrderBy( a => a.Order ).ToList();

            // Set BadgeEntity using a new RockContext that won't get manually disposed.
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
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the RowSelected event of the gAttendanceHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAttendanceHistory_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.AttendanceDetailPage, "AttendanceId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendanceHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendanceHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var attendanceInfo = e.Row.DataItem as AttendanceInfo;
            if ( attendanceInfo == null )
            {
                return;
            }

            var lCheckinDate = ( Literal ) e.Row.FindControl( "lCheckinDate" );
            var lCheckinScheduleName = ( Literal ) e.Row.FindControl( "lCheckinScheduleName" );
            var lWhoCheckedIn = ( Literal ) e.Row.FindControl( "lWhoCheckedIn" );
            lCheckinDate.Text = attendanceInfo.Date.ToShortDateString();
            lCheckinScheduleName.Text = attendanceInfo.ScheduleName;
            if ( lWhoCheckedIn != null && attendanceInfo.CheckInByPersonGuid.HasValue )
            {
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

            // Get the printers list.
            var printerList = new DeviceService( rockContext )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .OrderBy( d => d.Name )
                .Select( a => new { a.Name, a.Guid } )
                .ToList();

            ddlPrinter.Items.Clear();
            ddlPrinter.Items.Add( new ListItem() );

            if ( hfHasClientPrinter.Value.AsBoolean() )
            {
                ddlPrinter.Items.Add( new ListItem( "local printer", Guid.Empty.ToString() ) );
            }

            foreach ( var printer in printerList )
            {
                ddlPrinter.Items.Add( new ListItem( printer.Name, printer.Guid.ToString() ) );
            }

            var labelPrinterGuid = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().LabelPrinterGuid;
            ddlPrinter.SetValue( labelPrinterGuid );

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

            var selectedPrinterGuid = ddlPrinter.SelectedValue.AsGuidOrNull();

            if ( !selectedPrinterGuid.HasValue )
            {
                nbReprintLabelMessages.Visible = true;
                nbReprintLabelMessages.Text = "Please select a printer.";
                return;
            }

            var selectedAttendanceIds = hfCurrentAttendanceIds.Value.SplitDelimitedValues().AsIntegerList();

            var fileGuids = cblLabels.SelectedValues.AsGuidList();

            ReprintLabelOptions reprintLabelOptions;

            string selectedPrinterIPAddress;
            if ( selectedPrinterGuid == Guid.Empty )
            {
                reprintLabelOptions = new ReprintLabelOptions
                {
                    PrintFrom = PrintFrom.Client
                };
            }
            else
            {
                selectedPrinterIPAddress = new DeviceService( new RockContext() ).GetSelect( selectedPrinterGuid.Value, s => s.IPAddress );

                reprintLabelOptions = new ReprintLabelOptions
                {
                    PrintFrom = PrintFrom.Server,
                    ServerPrinterIPAddress = selectedPrinterIPAddress
                };
            }

            CheckinManagerHelper.SaveSelectedLabelPrinterToCookie( selectedPrinterGuid );

            // Now, finally, re-print the labels.
            List<string> messages = ZebraPrint.ReprintZebraLabels( fileGuids, personId, selectedAttendanceIds, nbReprintMessage, this.Request, reprintLabelOptions );
            nbReprintMessage.Visible = true;
            nbReprintMessage.Text = messages.JoinStrings( "<br>" );

            mdReprintLabels.Hide();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnPersonAttendanceHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPersonAttendanceHistory_Click( object sender, EventArgs e )
        {
            // Get the person Id from the PersonId page parameter, or look it up based on the Person Guid page parameter.
            int? personIdParam = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            int personId = personIdParam.HasValue
                ? personIdParam.Value
                : new PersonService( new RockContext() ).GetId( GetPersonGuid() ).GetValueOrDefault();

            var queryParams = new Dictionary<string, string>() {
                { "PersonId", personId.ToString() }
            };

            this.NavigateToLinkedPage( AttributeKey.PersonAttendanceHistoryPage, queryParams );
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
            btnReprintLabels.Visible = GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean() && this.IsUserAuthorized( SecurityActionKey.ReprintLabels );

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var person = personService.Queryable( true, true ).Include( a => a.PhoneNumbers ).Include( a => a.RecordStatusValue )
                    .FirstOrDefault( a => a.Guid == personGuid );

                if ( person == null )
                {
                    return;
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
                    // Note that Grade names might be different in other countries.
                    // See  https://separatedbyacommonlanguage.blogspot.com/2006/12/types-of-schools-school-years.html for examples.
                    var firstWord = gradeParts[0];
                    var remainderWords = gradeParts.Skip( 1 ).ToList().AsDelimited( " " );
                    if ( firstWord.Equals( "Year", StringComparison.OrdinalIgnoreCase ) )
                    {
                        // MDP 2020-10-21 (at request of GJ)
                        // Special case if formatted grade is 'Year 1', 'Year 2', etc (see https://separatedbyacommonlanguage.blogspot.com/2006/12/types-of-schools-school-years.html)
                        // Make the word Year on the top.
                        grade = string.Format( @"<div class=""text-semibold"">{0}</div><div class=""text-sm text-muted"">{1}</div>", remainderWords, firstWord );
                    }
                    else
                    {
                        grade = string.Format( @"<div class=""text-semibold"">{0}</div><div class=""text-sm text-muted"">{1}</div>", firstWord, remainderWords );
                    }
                }

                lGrade.Text = grade;

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
                            Code = a.AttendanceCode != null ? a.AttendanceCode.Code : string.Empty,
                            CheckInByPersonName = checkedInByPerson != null ? checkedInByPerson.FullName : string.Empty,
                            CheckInByPersonGuid = checkedInByPerson != null ? checkedInByPerson.Guid : ( Guid? ) null
                        };
                    } ).ToList();

                // Set active locations to be a link to the room in manager page.
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

                gAttendanceHistory.DataSource = attendances;
                gAttendanceHistory.DataBind();
            }
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

        #endregion
    }
}