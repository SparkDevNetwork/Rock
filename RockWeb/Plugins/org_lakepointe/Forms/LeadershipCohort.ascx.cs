using Newtonsoft.Json.Linq;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI;

namespace RockWeb.Plugins.org_lakepointe.Forms
{
    [DisplayName( "Leadership Cohort Registration" )]
    [Category( "LPC > Forms" )]
    [Description( "Registration form for a Leadership Cohort." )]

    [LavaField(
        name: "Page Title",
        description: "The title that will be displayed in the banner at the top of the page. (HTML)",
        required: true,
        defaultValue: "<h1>Leadership Cohort Registration</h1>",
        category: "",
        order: 0,
        key: AttributeKey.Title )]

    [TextField(
        name: "From Email Address",
        description: "The email address that email should be sent from.",
        required: false,
        defaultValue: "do-not-reply@lakepointe.church",
        category: "",
        order: 1,
        key: AttributeKey.From )]

    [TextField(
        name: "Email From Name",
        description: "The name the email will appear to have come from.",
        required: false,
        defaultValue: "Lakepointe Leadership Cohort",
        category: "",
        order: 2,
        key: AttributeKey.Who )]

    [TextField(
        name: "Email Subject",
        description: "Subject line for the confirmation email.",
        required: false,
        defaultValue: "Your Leadership Cohort Registration",
        category: "",
        order: 3,
        key: AttributeKey.Subject )]

    [CodeEditorField(
        name: "Body",
        key: AttributeKey.Body,
        description: "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        mode: Rock.Web.UI.Controls.CodeEditorMode.Html,
        theme: Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        height: 300,
        required: false,
        order: 4 )]

    [BooleanField(
        name: "Save Communication History",
        key: AttributeKey.SaveCommunicationHistory,
        description: "Should a record of this communication be saved to the recipient's profile?",
        defaultValue: true,
        order: 5 )]

    [CodeEditorField(
        name: "Confirmation Text Message Template",
        description: "The lava template for the message to show on the Confirmation page under normal circumstances.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "Your confirmation text goes here. (In HTML, please. Lava is also acceptable.)",
        order: 6 )]

    public partial class LeadershipCohort : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Title = "Title";
            public const string From = "From";
            public const string Who = "Who";
            public const string Subject = "Subject";
            public const string Body = "Body";
            public const string SaveCommunicationHistory = "SaveCommunicationHistory";
        }

        #endregion
        #region Fields

        Guid LEADERSHIP_COHORT_GUID = "FE379EA5-37FD-491C-BF09-5D329AA029C9".AsGuid();
        Guid DAY_OF_WEEK_ATTRIBUTE_GUID = "04982091-BCE8-405C-8D8F-CFF3660376EA".AsGuid();
        Guid TIME_OF_DAY_ATTRIBUTE_GUID = "E6ADF18A-5FC0-452B-B419-37C587DEC997".AsGuid();
        Guid LEADER_ATTRIBUTE_GUID = "1284B802-03AB-42CB-ABA4-928DFCF0B14D".AsGuid();
        Guid ICS_ATTACHMENT_GUID = "20313662-B39A-4102-95D0-4779B23CB695".AsGuid();
        Guid GROUP_MEMBER_ROLE_MEMBER = "FD6DE2D4-52FF-446B-97A0-96EA97B87ACD".AsGuid();

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbSave.Click += lbSave_click;

            BlockUpdated += EvaluationBlockUpdated;
            AddConfigurationUpdateTrigger( upLeadershipCohort );

            ddlCampus.SelectedIndexChanged += DdlCampus_SelectedIndexChanged;
            ddlDayOfWeek.SelectedIndexChanged += DdlDayOfWeek_SelectedIndexChanged;
            ddlTimeOfDay.SelectedIndexChanged += DdlTimeOfDay_SelectedIndexChanged;
            ddlCohort.SelectedIndexChanged += DdlCohort_SelectedIndexChanged;
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        #endregion

        #region Events

        protected void lbSave_click( object sender, EventArgs e )
        {
            AddToGroup();
        }

        protected void EvaluationBlockUpdated( object sender, EventArgs e )
        {
            InitializeForm();
        }

        #endregion

        #region Methods

        private void InitializeForm()
        {
            nbWarning.Text = string.Empty;
            lPageTitle.Text = GetAttributeValue( AttributeKey.Title );

            PopulateCampusList();

            ddlCampus.SelectedValue = CurrentPerson.PrimaryCampus.Guid.ToString();

            PopulateDayList();

            ddlDayOfWeek.SelectedValue = null;

            PopulateTimeList();

            ddlTimeOfDay.SelectedValue = null;

            PopulateCohortList();

            ddlCohort.SelectedValue = null;
        }

        private void DdlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateDayList();
            ddlDayOfWeek.SelectedValue = null;
            ddlTimeOfDay.SelectedValue = null;
            ddlCohort.SelectedValue = null;
        }

        private void DdlDayOfWeek_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateTimeList();
            ddlTimeOfDay.SelectedValue = null;
            ddlCohort.SelectedValue = null;
        }

        private void DdlTimeOfDay_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateCohortList();
            ddlCohort.SelectedValue = null;
        }

        private void DdlCohort_SelectedIndexChanged( object sender, EventArgs e )
        {
            //throw new NotImplementedException();
        }

        private void AddToGroup()
        {
            //if ( ddlCampus.SelectedIndex == 0 )
            //{
            //    nbWarning.Text = "You must select a campus.";
            //    ScrollToTop();
            //    return;
            //}

            if ( ddlDayOfWeek.SelectedIndex == 0 )
            {
                nbWarning.Text = "You must select a Day of Week.";
                ScrollToTop();
                return;
            }

            if ( ddlTimeOfDay.SelectedIndex == 0 )
            {
                nbWarning.Text = "You must select a Time of Day.";
                ScrollToTop();
                return;
            }

            if ( ddlCohort.SelectedIndex == 0 )
            {
                nbWarning.Text = "You must select a Leadership Cohort.";
                ScrollToTop();
                return;
            }

            Group group = null;

            // add to group
            using ( var context = new RockContext() )
            {
                var leadershipCohort = GroupTypeCache.Get( LEADERSHIP_COHORT_GUID );
                if ( leadershipCohort != null )
                {
                    var memberRoleId = leadershipCohort.Roles.Where( r => r.Guid.Equals( GROUP_MEMBER_ROLE_MEMBER ) ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                    var gm = new GroupMember();
                    group = new GroupService( context ).GetByGuid( ddlCohort.SelectedValue.AsGuid() );

                    var gms = new GroupMemberService( context );
                    if ( gms.Queryable().AsNoTracking().Where( m => m.GroupId == group.Id && m.PersonId == CurrentPersonId.Value ).Any() )
                    {
                        nbWarning.Text = "You are already registered for this group.";
                        ScrollToTop();
                        return;
                    }

                    gm.GroupId = group.Id;
                    gm.PersonId = CurrentPersonId.Value;
                    gm.GroupRoleId = memberRoleId.Value;
                    gm.GroupMemberStatus = GroupMemberStatus.Active;
                    gms.Add( gm );
                    context.SaveChanges();
                }

                // send confirmation email
                string fromEmailAddress = GetAttributeValue( AttributeKey.From );
                string fromName = GetAttributeValue( AttributeKey.Who );
                string subject = GetAttributeValue( AttributeKey.Subject );
                bool createCommunicationRecord = GetAttributeValue( AttributeKey.SaveCommunicationHistory ).AsBoolean();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                var personDict = new Dictionary<string, object>( mergeFields )
                {
                    { "Person", CurrentPerson },
                    { "Group", group },
                };

                var toRecipients = new List<RockEmailMessageRecipient> { new RockEmailMessageRecipient( CurrentPerson, personDict ) };
                string body = GetAttributeValue( AttributeKey.Body );

                group.LoadAttributes();
                var attachments = new BinaryFile[1];
                attachments[0] = null;
                var attachmentKey = AttributeCache.Get( ICS_ATTACHMENT_GUID ).Key;
                if ( group.AttributeValues.ContainsKey( attachmentKey ) )
                {
                    attachments[0] = new BinaryFileService( context ).Get( ( ( string ) group.AttributeValues[attachmentKey].ValueAsType ).AsGuid() );
                }

                var errorMessages = new List<string>();
                Send( toRecipients, fromEmailAddress, fromName, subject, body, null, null, createCommunicationRecord, attachments, out errorMessages );

                // Flip block to the confirmation page
                pnlMain.Visible = false;
                nbConfirmation.Title = "Success";
                nbConfirmation.Text = "Your information has been submitted.";
                pnlConfirmation.Visible = true;
                pnlNavigation.Visible = false;

                ScrollToTop();
            }
        }

        private void PopulateCampusList()
        {
            ddlCampus.DataSource = GetCampusList();
            ddlCampus.DataValueField = "Value";
            ddlCampus.DataTextField = "Text";
            ddlCampus.DataBind();
        }

        private void PopulateDayList()
        {
            ddlDayOfWeek.DataSource = GetDaysList( ddlCampus.SelectedValue );
            ddlDayOfWeek.DataValueField = "Value";
            ddlDayOfWeek.DataTextField = "Text";
            ddlDayOfWeek.DataBind();
        }

        private void PopulateTimeList()
        {
            ddlTimeOfDay.DataSource = GetTimesList( ddlCampus.SelectedValue, ddlDayOfWeek.SelectedValue );
            ddlTimeOfDay.DataValueField = "Value";
            ddlTimeOfDay.DataTextField = "Text";
            ddlTimeOfDay.DataBind();
        }

        private void PopulateCohortList()
        {
            ddlCohort.DataSource = GetCohortList( ddlCampus.SelectedValue, ddlDayOfWeek.SelectedValue, ddlTimeOfDay.SelectedValue );
            ddlCohort.DataValueField = "Value";
            ddlCohort.DataTextField = "Text";
            ddlCohort.DataBind();
        }

        private List<DdlListItem> GetCampusList()
        {
            using ( var context = new RockContext() )
            {
                var leadershipCohort = GroupTypeCache.Get( LEADERSHIP_COHORT_GUID );
                if ( leadershipCohort != null )
                {
                    return new GroupService( context ).Queryable().AsNoTracking()
                        .Where( g => g.GroupTypeId == leadershipCohort.Id && g.IsActive )                           // active leadership cohorts
                        .Where( g => g.Members.Where( m => m.InactiveDateTime == null ).Count() < g.GroupCapacity ) // that have not reached capacity
                        .Select( g => g.Campus )                                                                    // get the campus of this group
                        .Distinct()                                                                                 // remove duplicate campuses from the list
                        .Select( c => new DdlListItem { Value = c.Guid.ToString(), Text = c.Name } )                // build the list that will populate the drop-down
                        .ToList();
                }
            }
            return new List<DdlListItem>();
        }

        private List<DdlListItem> GetDaysList( string campusGuid )
        {
            var result = new List<DdlListItem>();
            result.Add( new DdlListItem { Value = null, Text = "Choose One:" } );
            if ( campusGuid != null )
            {
                using ( var context = new RockContext() )
                {
                    var leadershipCohort = GroupTypeCache.Get( LEADERSHIP_COHORT_GUID );
                    var dayOfWeek = AttributeCache.Get( DAY_OF_WEEK_ATTRIBUTE_GUID );
                    if ( leadershipCohort != null && dayOfWeek != null )
                    {
                        var dayOfWeekAttributes = new AttributeValueService( context ).Queryable().AsNoTracking()
                            .Where( av => av.AttributeId == dayOfWeek.Id );

                        var days = new GroupService( context ).Queryable().AsNoTracking()
                            .Where( g => g.GroupTypeId == leadershipCohort.Id && g.IsActive )                           // active leadership cohorts
                            .Where( g => g.Campus.Guid.ToString() == campusGuid )                                       // on the selected campus
                            .Where( g => g.Members.Where( m => m.InactiveDateTime == null ).Count() < g.GroupCapacity ) // that have not reached capacity
                            .Join( dayOfWeekAttributes, g => g.Id, av => av.EntityId, ( g, av ) => new { Group = g, AttributeValue = av } ) // Join with the attribute value table
                            .Select( g => g.AttributeValue.Value )                                                      // get the day of week attribute of the group
                            .Distinct();                                                                                // remove duplicate days from the list

                        foreach ( var day in days )
                        {
                            result.Add( new DdlListItem { Value = day, Text = ( ( DayOfWeek ) Enum.Parse( typeof( DayOfWeek ), day ) ).ToString() } );
                        }
                    }
                }
            }
            return result;
        }

        private List<DdlListItem> GetTimesList( string campusGuid, string selectedDay )
        {
            var result = new List<DdlListItem>();
            result.Add( new DdlListItem { Value = null, Text = "Choose One:" } );
            if ( campusGuid != null )
            {
                using ( var context = new RockContext() )
                {
                    var leadershipCohort = GroupTypeCache.Get( LEADERSHIP_COHORT_GUID );
                    var dayOfWeek = AttributeCache.Get( DAY_OF_WEEK_ATTRIBUTE_GUID );
                    var timeOfDay = AttributeCache.Get( TIME_OF_DAY_ATTRIBUTE_GUID );
                    if ( leadershipCohort != null && dayOfWeek != null && timeOfDay != null )
                    {
                        var avs = new AttributeValueService( context ).Queryable().AsNoTracking();
                        var dayOfWeekAttributes = avs.Where( av => av.AttributeId == dayOfWeek.Id );
                        var timeOfDayAttributes = avs.Where( av => av.AttributeId == timeOfDay.Id );

                        var times = new GroupService( context ).Queryable().AsNoTracking()
                            .Where( g => g.GroupTypeId == leadershipCohort.Id && g.IsActive )                           // active leadership cohorts
                            .Where( g => g.Campus.Guid.ToString() == campusGuid )                                       // on the selected campus
                            .Where( g => g.Members.Where( m => m.InactiveDateTime == null ).Count() < g.GroupCapacity ) // that have not reached capacity
                            .Join( dayOfWeekAttributes, g => g.Id, av => av.EntityId, ( g, av ) => new { Group = g, DayOfWeek = av } ) // Join with the day of week attribute table
                            .Where( g => g.DayOfWeek.Value == selectedDay )                                             // on the selected day
                            .Join( timeOfDayAttributes, g => g.Group.Id, av => av.EntityId, ( g, av ) => new { Group = g, TimeOfDay = av } ) // Join with the time of day attribute table
                            .Select( g => g.TimeOfDay.Value )                                                           // get the day of week attribute of the group
                            .Distinct();                                                                                // remove duplicate days from the list

                        foreach ( var time in times )
                        {
                            result.Add( new DdlListItem { Value = time, Text = time.Substring(0, time.Length - 3) } );
                        }
                    }
                }
            }
            return result;
        }

        private List<DdlListItem> GetCohortList( string campusGuid, string selectedDay, string selectedTime )
        {
            var result = new List<DdlListItem>();
            result.Add( new DdlListItem { Value = null, Text = "Choose one:" } );
            if ( campusGuid != null )
            {
                using ( var context = new RockContext() )
                {
                    var leadershipCohort = GroupTypeCache.Get( LEADERSHIP_COHORT_GUID );
                    var dayOfWeek = AttributeCache.Get( DAY_OF_WEEK_ATTRIBUTE_GUID );
                    var timeOfDay = AttributeCache.Get( TIME_OF_DAY_ATTRIBUTE_GUID );
                    var leaderAttribute = AttributeCache.Get( LEADER_ATTRIBUTE_GUID );
                    if ( leadershipCohort != null && dayOfWeek != null && timeOfDay != null && leaderAttribute != null )
                    {
                        var avs = new AttributeValueService( context ).Queryable().AsNoTracking();
                        var dayOfWeekAttributes = avs.Where( av => av.AttributeId == dayOfWeek.Id );
                        var timeOfDayAttributes = avs.Where( av => av.AttributeId == timeOfDay.Id );
                        var leaderAttributes = avs.Where( av => av.AttributeId == leaderAttribute.Id );

                        var cohorts = new GroupService( context ).Queryable().AsNoTracking()
                            .Where( g => g.GroupTypeId == leadershipCohort.Id && g.IsActive )                           // active leadership cohorts
                            .Where( g => g.Campus.Guid.ToString() == campusGuid )                                       // on the selected campus
                            .Where( g => g.Members.Where( m => m.InactiveDateTime == null ).Count() < g.GroupCapacity ) // that have not reached capacity
                            .Join( dayOfWeekAttributes, g => g.Id, av => av.EntityId, ( g, av ) => new { Group = g, DayOfWeek = av } ) // Join with the day of week attribute table
                            .Where( g => g.DayOfWeek.Value == selectedDay )                                             // on the selected day
                            .Join( timeOfDayAttributes, g => g.Group.Id, av => av.EntityId, ( g, av ) => new { Group = g, TimeOfDay = av } ) // Join with the time of day attribute table
                            .Where( g => g.TimeOfDay.Value == selectedTime )                                            // at the selected time
                            .Join( leaderAttributes, g => g.Group.Group.Id, av => av.EntityId, ( g, av ) => new { Group = g, Leader = av } ) // Join with leader attribute table
                            .Select( g => new { Group = g.Group.Group.Group, Leader = g.Leader.Value } )                                                           // get the leader of the group
                            .Distinct();                                                                                // remove duplicate days from the list

                        var personAliasService = new PersonAliasService( context );
                        var personService = new PersonService( context );
                        foreach ( var cohort in cohorts )
                        {
                            result.Add( new DdlListItem
                            {
                                Value = cohort.Group.Guid.ToString(),
                                Text = string.Format( "{0} [ {1} / {2} ]", personAliasService.Get( cohort.Leader.AsGuid() ).Person.FullName, cohort.Group.Members.Count(), cohort.Group.GroupCapacity )
                            } );
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="fromEmail">From email.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="ccEmails">The CC emails.</param>
        /// <param name="bccEmails">The BCC emails.</param>
        /// <param name="createCommunicationRecord">if set to <c>true</c> [create communication record].</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="errorMessages">The error messages.</param>
        private void Send( List<RockEmailMessageRecipient> recipients, string fromEmail, string fromName, string subject, string body, List<string> ccEmails, List<string> bccEmails, bool createCommunicationRecord, BinaryFile[] attachments, out List<string> errorMessages )
        {
            var emailMessage = new RockEmailMessage();
            emailMessage.SetRecipients( recipients );
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName.IsNullOrWhiteSpace() ? fromEmail : fromName;
            emailMessage.Subject = subject;
            emailMessage.Message = body;

            emailMessage.CCEmails = ccEmails ?? new List<string>();
            emailMessage.BCCEmails = bccEmails ?? new List<string>();

            foreach ( BinaryFile b in attachments )
            {
                if ( b != null )
                {
                    emailMessage.Attachments.Add( b );
                }
            }

            emailMessage.CreateCommunicationRecord = createCommunicationRecord;
            emailMessage.AppRoot = Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ) ?? string.Empty;

            emailMessage.Send( out errorMessages );
        }

        private void ScrollToTop( int intPosY = 0 )
        {
            string strScript = @"var manager = Sys.WebForms.PageRequestManager.getInstance(); 
            manager.add_beginRequest(beginRequest); 
            function beginRequest() 
            { 
                manager._scrollPosition = null; 
            }
            window.scroll(0, " + intPosY.ToString() + ");";

            ScriptManager.RegisterStartupScript( upLeadershipCohort, upLeadershipCohort.GetType(), "Error_" + RockDateTime.Now.Ticks.ToString(), strScript, true );

            return;
        }

        #endregion

        #region Private Classes

        private class DdlListItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        #endregion
    }
}