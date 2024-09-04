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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

using CheckInLabel = Rock.CheckIn.CheckInLabel;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Success" )]
    [Category( "Check-in" )]
    [Description( "Displays the details of a successful checkin." )]

    #region Block Attributes

    [LinkedPage( "Person Select Page",
        Key = AttributeKey.PersonSelectPage,
        IsRequired = false,
        Order = 5 )]

    [TextField( "Title",
        Key = AttributeKey.Title,
        IsRequired = false,
        DefaultValue = "Checked-in",
        Category = "Text",
        Order = 6 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "18911F1B-294E-48D6-9E6B-0F72BF6C9491" )]
    public partial class Success : CheckInBlock
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string PersonSelectPage = "PersonSelectPage";
            public const string Title = "Title";
        }

        private static class MergeFieldKey
        {
            public const string CheckinResultList = "CheckinResultList";
            public const string Kiosk = "Kiosk";
            public const string RegistrationModeEnabled = "RegistrationModeEnabled";
            public const string Messages = "Messages";
            public const string CheckinAreas = "CheckinAreas";
            public const string ZebraPrintMessageList = "ZebraPrintMessageList";

            public const string Person = "Person";

            public const string AchievementAttempts = "AchievementAttempts";

            public const string InProgressCount = "InProgressCount";
            public const string AchievementType = "AchievementType";
            public const string AchievementTypeEvent = "AchievementTypeEvent";
            public const string NumberToAchieve = "NumberToAchieve";
            public const string NumberToAccumulate = "NumberToAccumulate";
            public const string JustCompleted = "JustCompleted";

            public const string CurrentAchievementAttempt = "CurrentAchievementAttempt";

            // Number of times person has successful achieved this (For example, successfully did  '3 times a month" 10 times )
            public const string SuccessfulAchievementCount = "SuccessfulAchievementCount";

            public const string ProgressCount = "ProgressCount";
            public const string ProgressPercent = "ProgressPercent";
            public const string StreakType = "StreakType";
            public const string FrequencyText = "FrequencyText";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    try
                    {
                        ShowDetails();
                    }
                    catch ( Exception ex )
                    {
                        LogException( ex );
                        if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                        {
                            base.OnLoad( e );
                            throw;
                        }
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            lTitle.Text = GetAttributeValue( AttributeKey.Title );

            var printFromClient = new List<CheckInLabel>();
            var printFromServer = new List<CheckInLabel>();

            List<CheckinResult> checkinResultList = new List<CheckinResult>();

            var successfullyCompletedAchievementIdsPriorToCheckin = CurrentCheckInState.CheckIn.SuccessfullyCompletedAchievementsPriorToCheckin?.Select( a => a.AchievementAttempt.Id ).ToArray() ?? new int[0];
            var achievementsStateAfterCheckin = CurrentCheckInState.CheckIn.AchievementsStateAfterCheckin;
            var successLavaTemplateDisplayMode = CurrentCheckInState.CheckInType.SuccessLavaTemplateDisplayMode;

            // Populate Checkin Results and label data
            foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
            {
                lbAnother.Visible =
                    CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Individual &&
                    family.People.Count > 1;

                foreach ( var person in family.GetPeople( true ) )
                {
                    foreach ( var groupType in person.GetGroupTypes( true ) )
                    {
                        foreach ( var group in groupType.GetGroups( true ) )
                        {
                            foreach ( var location in group.GetLocations( true ) )
                            {
                                foreach ( var schedule in location.GetSchedules( true ) )
                                {
                                    CheckinResult checkinResult = new CheckinResult();
                                    checkinResult.Person = person;
                                    checkinResult.Group = group;
                                    checkinResult.Location = location.Location;
                                    checkinResult.Schedule = schedule;
                                    checkinResult.UpdateAchievementFields( successfullyCompletedAchievementIdsPriorToCheckin, achievementsStateAfterCheckin );
                                    checkinResultList.Add( checkinResult );
                                }
                            }
                        }

                        if ( groupType.Labels != null && groupType.Labels.Any() )
                        {
                            printFromClient.AddRange( groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                            printFromServer.AddRange( groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                        }
                    }
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( MergeFieldKey.CheckinResultList, checkinResultList );
            mergeFields.Add( MergeFieldKey.Kiosk, CurrentCheckInState.Kiosk );
            mergeFields.Add( MergeFieldKey.RegistrationModeEnabled, CurrentCheckInState.Kiosk.RegistrationModeEnabled );
            mergeFields.Add( MergeFieldKey.Messages, CurrentCheckInState.Messages );
            if ( LocalDeviceConfig.CurrentGroupTypeIds != null )
            {
                var checkInAreas = LocalDeviceConfig.CurrentGroupTypeIds.Select( a => Rock.Web.Cache.GroupTypeCache.Get( a ) );
                mergeFields.Add( MergeFieldKey.CheckinAreas, checkInAreas );
            }

            if ( printFromClient.Any() )
            {
                var proxySafeUrl = Request.UrlProxySafe();
                var urlRoot = $"{proxySafeUrl.Scheme}://{proxySafeUrl.Authority}";

                /*
                // This is extremely useful when debugging with ngrok and an iPad on the local network.
                // X-Original-Host will contain the name of your ngrok hostname, therefore the labels will
                // get a LabelFile url that will actually work with that iPad.
                if ( Request.Headers["X-Original-Host" ] != null )
                {
                    var scheme = Request.Headers["X-Forwarded-Proto"] ?? "http";
                    urlRoot = string.Format( "{0}://{1}", scheme, Request.Headers.GetValues( "X-Original-Host" ).First() );
                }
                */

                printFromClient
                    .OrderBy( l => l.PersonId )
                    .ThenBy( l => l.Order )
                    .ToList()
                    .ForEach( l => l.LabelFile = urlRoot + l.LabelFile );

                AddLabelScript( printFromClient.ToJson() );
            }

            if ( printFromServer.Any() )
            {
                var messages = ZebraPrint.PrintLabels( printFromServer );
                mergeFields.Add( MergeFieldKey.ZebraPrintMessageList, messages );
                if ( messages.Any() )
                {
                    lCheckinLabelErrorMessages.Visible = true;
                    var messageHtml = new StringBuilder();
                    foreach ( var message in messages )
                    {
                        messageHtml.AppendLine( $"<li>{message}</li>" );
                    }

                    lCheckinLabelErrorMessages.Text = messageHtml.ToString();
                }
            }

            if ( CurrentCheckInState?.Messages?.Any() == true )
            {
                lMessages.Visible = true;
                StringBuilder sbMessages = new StringBuilder();
                foreach ( var message in CurrentCheckInState.Messages )
                {
                    var messageHtml = $@"<li><div class='alert alert-{ message.MessageType.ConvertToString( false ).ToLower() }'> { message.MessageText }  </div></li>";
                    sbMessages.AppendLine( messageHtml );
                }

                lMessages.Text = sbMessages.ToString();
            }

            if ( lbAnother.Visible )
            {
                var bodyTag = this.Page.Master.FindControl( "body" ) as HtmlGenericControl;
                if ( bodyTag != null )
                {
                    bodyTag.AddCssClass( "checkin-anotherperson" );
                }
            }

            GenerateQRCodes();

            RenderCheckinResults( checkinResultList, mergeFields, successLavaTemplateDisplayMode );
        }

        /// <summary>
        /// Generates the QR codes.
        /// </summary>
        private void GenerateQRCodes()
        {
            if ( !LocalDeviceConfig.GenerateQRCodeForAttendanceSessions )
            {
                return;
            }

            HttpCookie attendanceSessionGuidsCookie = Request.Cookies[CheckInCookieKey.AttendanceSessionGuids];
            if ( attendanceSessionGuidsCookie == null )
            {
                attendanceSessionGuidsCookie = new HttpCookie( CheckInCookieKey.AttendanceSessionGuids );
                attendanceSessionGuidsCookie.Value = string.Empty;
            }

            // set (or reset) the expiration to be 8 hours from the current time)
            attendanceSessionGuidsCookie.Expires = RockDateTime.Now.AddHours( 8 );

            var attendanceSessionGuids = attendanceSessionGuidsCookie.Value.Split( ',' ).AsGuidList();
            attendanceSessionGuids = ValidAttendanceSessionGuids( attendanceSessionGuids );

            // Add the guid to the list of checkin session cookie guids if it's not already there.
            if ( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.HasValue &&
                !attendanceSessionGuids.Contains( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.Value ) )
            {
                attendanceSessionGuids.Add( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.Value );
            }

            attendanceSessionGuidsCookie.Value = attendanceSessionGuids.AsDelimited( "," );

            Rock.Web.UI.RockPage.AddOrUpdateCookie( attendanceSessionGuidsCookie );

            lCheckinQRCodeHtml.Text = string.Format( "<div class='qr-code-container text-center'><img class='img-responsive qr-code' src='{0}' alt='Check-in QR Code' width='500' height='500'></div>", GetAttendanceSessionsQrCodeImageUrl( attendanceSessionGuidsCookie ) );
        }

        /// <summary>
        /// Renders the checkin results.
        /// </summary>
        /// <param name="checkinResultList">The checkin result list.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="successLavaTemplateDisplayMode">The success lava template display mode.</param>
        private void RenderCheckinResults( List<CheckinResult> checkinResultList, Dictionary<string, object> mergeFields, SuccessLavaTemplateDisplayMode successLavaTemplateDisplayMode )
        {
            if ( successLavaTemplateDisplayMode != SuccessLavaTemplateDisplayMode.Replace )
            {
                pnlDefaultCheckinSuccessResults.Visible = true;
                RenderDefaultCheckinResults( checkinResultList );
            }
            else
            {
                pnlDefaultCheckinSuccessResults.Visible = false;
            }

            if ( successLavaTemplateDisplayMode != SuccessLavaTemplateDisplayMode.Never )
            {
                lCustomSuccessLavaTemplateHtml.Visible = true;
                var successLavaTemplate = CurrentCheckInState.CheckInType.SuccessLavaTemplate;
                lCustomSuccessLavaTemplateHtml.Text = successLavaTemplate.ResolveMergeFields( mergeFields );
            }
            else
            {
                lCustomSuccessLavaTemplateHtml.Visible = false;
            }
        }

        /// <summary>
        /// Renders the checkin results and any achievements and celebrations
        /// </summary>
        /// <param name="checkinResultList">The checkin result list.</param>
        private void RenderDefaultCheckinResults( List<CheckinResult> checkinResultList )
        {
            if ( checkinResultList.Any( a => a.JustCompletedAchievementAttempts?.Any() == true ) )
            {
                List<PersonAchievementType> justCompletedPersonAchievementTypes = new List<PersonAchievementType>();
                foreach ( var checkinResult in checkinResultList.DistinctBy( a => a.Person.Person.Id ) )
                {
                    justCompletedPersonAchievementTypes.AddRange( checkinResult.GetPersonAchievementTypes( true ).Where( a => a.JustCompleted ) );
                }

                pnlCheckinCelebrations.Visible = true;
                rptAchievementsSuccess.DataSource = justCompletedPersonAchievementTypes;
                rptAchievementsSuccess.DataBind();
            }
            else
            {
                pnlCheckinCelebrations.Visible = false;
            }

            pnlCheckinConfirmations.Visible = true;
            rptCheckinResults.DataSource = checkinResultList;
            rptCheckinResults.DataBind();
        }

        /// <summary>
        /// Gets the achievement merge fields.
        /// </summary>
        /// <param name="achievementAttempt">The achievement attempt.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private Dictionary<string, object> GetAchievementMergeFields( PersonAchievementType personAchievementType )
        {
            var person = personAchievementType.Person;

            AchievementTypeCache achievementTypeCache = personAchievementType.AchievementType;
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( MergeFieldKey.Person, person );

            AchievementAttempt currentAchievementAttempt;

            if ( personAchievementType.JustCompletedAchievementAttempt != null )
            {
                currentAchievementAttempt = personAchievementType.JustCompletedAchievementAttempt;
            }
            else
            {
                currentAchievementAttempt = personAchievementType.AchievementAttempts.Where( a => !a.IsSuccessful && !a.IsClosed ).FirstOrDefault();
            }

            // there should be either 1 or 0 achievementAttempts in progress
            int inProgressCount;
            if ( personAchievementType.AchievementAttempts.Where( a => !a.IsSuccessful && !a.IsClosed ).Any() )
            {
                inProgressCount = 1;
            }
            else
            {
                inProgressCount = 0;
            }

            int successfulAchievementCount = personAchievementType.AchievementAttempts.Where( a => a.IsSuccessful ).Count();

            mergeFields.Add( MergeFieldKey.CurrentAchievementAttempt, currentAchievementAttempt );
            mergeFields.Add( MergeFieldKey.AchievementAttempts, personAchievementType.AchievementAttempts );
            mergeFields.Add( MergeFieldKey.InProgressCount, inProgressCount );
            mergeFields.Add( MergeFieldKey.SuccessfulAchievementCount, successfulAchievementCount );
            mergeFields.Add( MergeFieldKey.JustCompleted, personAchievementType.JustCompleted );
            mergeFields.Add( MergeFieldKey.AchievementType, achievementTypeCache );

            var achievementTypeEventEntityType = EntityTypeCache.Get( achievementTypeCache.ComponentEntityTypeId );
            mergeFields.Add( MergeFieldKey.AchievementTypeEvent, new RockDynamic( achievementTypeEventEntityType ) );
            mergeFields.Add( MergeFieldKey.NumberToAchieve, achievementTypeCache.NumberToAchieve );
            mergeFields.Add( MergeFieldKey.NumberToAccumulate, achievementTypeCache.NumberToAccumulate );
            mergeFields.Add( MergeFieldKey.ProgressCount, achievementTypeCache.GetProgressCount( currentAchievementAttempt ) );
            mergeFields.Add( MergeFieldKey.ProgressPercent, currentAchievementAttempt?.Progress * 100 );
            mergeFields.Add( MergeFieldKey.StreakType, achievementTypeCache.StreakType );
            mergeFields.Add( MergeFieldKey.FrequencyText, achievementTypeCache.StreakType?.OccurrenceFrequency.ConvertToString() );
            return mergeFields;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAchievementsSuccess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAchievementsSuccess_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var personJustCompletedAchievement = e.Item.DataItem as PersonAchievementType;
            if ( personJustCompletedAchievement == null )
            {
                return;
            }

            AchievementTypeCache achievementTypeCache = personJustCompletedAchievement.AchievementType;
            var customSummaryLavaTemplate = achievementTypeCache.CustomSummaryLavaTemplate;

            if ( customSummaryLavaTemplate.IsNullOrWhiteSpace() )
            {
                customSummaryLavaTemplate = achievementTypeCache.DefaultSummaryLavaTemplate;
            }

            var lAchievementSuccessHtml = e.Item.FindControl( "lAchievementSuccessHtml" ) as Literal;

            var mergeFields = GetAchievementMergeFields( personJustCompletedAchievement );
            lAchievementSuccessHtml.Text = customSummaryLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCheckinResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCheckinResults_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var checkinResult = e.Item.DataItem as CheckinResult;
            if ( checkinResult == null )
            {
                return;
            }

            var lCheckinResultsPersonName = e.Item.FindControl( "lCheckinResultsPersonName" ) as Literal;
            var lCheckinResultsCheckinMessage = e.Item.FindControl( "lCheckinResultsCheckinMessage" ) as Literal;
            var pnlCheckinResultsAchievementsScoreboard = e.Item.FindControl( "pnlCheckinResultsAchievementsScoreboard" ) as Panel;

            lCheckinResultsPersonName.Text = checkinResult.Person.ToString();
            lCheckinResultsCheckinMessage.Text = $"{checkinResult.Group} in {checkinResult.Location.Name} at {checkinResult.Schedule}";

            PersonAchievementType[] personAchievementTypes = checkinResult.GetPersonAchievementTypes( false );

            if ( personAchievementTypes.Any() == true )
            {
                pnlCheckinResultsAchievementsScoreboard.Visible = true;
                var rptCheckinResultsAchievementsScoreboard = e.Item.FindControl( "rptCheckinResultsAchievementsScoreboard" ) as Repeater;
                rptCheckinResultsAchievementsScoreboard.DataSource = personAchievementTypes;
                rptCheckinResultsAchievementsScoreboard.DataBind();
            }
            else
            {
                pnlCheckinResultsAchievementsScoreboard.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCheckinResultsAchievementsScoreboard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCheckinResultsAchievementsScoreboard_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lCheckinResultsAchievementScoreboardHtml = e.Item.FindControl( "lCheckinResultsAchievementScoreboardHtml" ) as Literal;
            PersonAchievementType personAchievementType = e.Item.DataItem as PersonAchievementType;
            if ( personAchievementType == null )
            {
                return;
            }

            var achievementType = personAchievementType.AchievementType;
            if ( achievementType == null )
            {
                return;
            }

            var customSummaryLavaTemplate = achievementType.CustomSummaryLavaTemplate;
            var mergeFields = GetAchievementMergeFields( personAchievementType );

            if ( customSummaryLavaTemplate.IsNullOrWhiteSpace() )
            {
                customSummaryLavaTemplate = achievementType.DefaultSummaryLavaTemplate;
            }

            lCheckinResultsAchievementScoreboardHtml.Text = customSummaryLavaTemplate?.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            NavigateToHomePage();
        }

        /// <summary>
        /// Handles the Click event of the lbAnother control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAnother_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        person.Selected = false;

                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            groupType.Selected = false;
                        }
                    }
                }

                SaveState();
                NavigateToLinkedPage( AttributeKey.PersonSelectPage );
            }
            else
            {
                NavigateToHomePage();
            }
        }

        /// <summary>
        /// Checks the given list of the attendance check-in session guids are still valid
        /// and returns the valid ones back.
        /// NOTE: Because someone could check-in a person multiple times, only the
        /// latest attendance record will have the correct attendance check-in session guid.
        /// That means attendance check-in session guids could be old/invalid, so
        /// this method will filter out the old/ones so a QR code does not
        /// become unnecessarily dense.
        /// </summary>
        /// <param name="sessionGuids">The attendance session guids.</param>
        /// <returns></returns>
        private List<Guid> ValidAttendanceSessionGuids( List<Guid> sessionGuids )
        {
            if ( sessionGuids == null )
            {
                return new List<Guid>();
            }

            if ( !sessionGuids.Any() )
            {
                return sessionGuids;
            }

            return new AttendanceService( new RockContext() ).Queryable().AsNoTracking()
                .Where( a => sessionGuids.Contains( a.AttendanceCheckInSession.Guid ) )
                .Select( a => a.AttendanceCheckInSession.Guid ).Distinct().ToList();
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/) && typeof window.RockCheckinNative === 'undefined') {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{
                printLabels();
            }}
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}

		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData),
            	function(result) {{
			        console.log('Tag printed');
			    }},
			    function(error) {{
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    alert('An error occurred while printing the labels. ' + error[0]);
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

    }
}