// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Reporting;
using com.lcbcchurch.NewVisitor.SystemKey;
using com.lcbcchurch.NewVisitor.Settings;
using Rock.Transactions;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "16 Weeks Onboarding Dashboard" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "16 Weeks Onboarding dashboard using grid." )]

    #region Block Attributes
    [SecurityRoleField( "Executive Team Role",
        Key = AttributeKeys.ExecutiveTeamRole,
        Description = "Member of this group can see all campuses.",
        IsRequired = true,
        Order = 0 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        name: "Campus Attribute",
        description: "The person attribute used to determine which campus a staff person is assigned to.",
        required: true,
        allowMultiple: false,
        order: 1,
        Key = AttributeKeys.CampusAttribute )]
    [GroupTypeField( "Attendance for Group Type",
        Description = "Group type picker used for finding attendance records.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKeys.AttendanceForGroupType )]
    [WorkflowTypeField( "Workflows",
        description: "Allows you to set workflows that will launched by clicking their buttons below the grid. The person record will be provided to the workflow as the Entity.",
        allowMultiple: true,
        IsRequired = false,
        Order = 4,
        Key = AttributeKeys.Workflows )]
    [IntegerField( "Maximum Fresh Week",
        Description = "The maximum number of weeks for the green fresh label.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKeys.MaximumFreshWeek )]
    [IntegerField( "Maximum Recent Week",
        Description = "The maximum number of weeks for the blue recent label.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKeys.MaximumRecentWeek )]
    [IntegerField( "Maximum Trailing Week",
        Description = "The maximum number of weeks for the purple trailing label. Every thing above this value will be assumbed to be in the 'Last Chance' range (red).",
        IsRequired = false,
        Order = 7,
        Key = AttributeKeys.MaximumTrailingWeek )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        "Engagement Begin Date Attribute",
        description: "Points to the attibute that holds the person's current engagement begin date.",
        IsRequired = true,
        Order = 8,
        Key = AttributeKeys.EngagementBeginDateAttribute )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        "Engagement Score Attribute",
        description: "Points to the attibute that holds the person's current engagement score.",
        IsRequired = true,
        Order = 9,
        Key = AttributeKeys.EngagementScoreAttribute )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        "First Steps Class Attribute",
        description: "Points to the attibute to use to indicate whether the person has or has not taken the First Steps class.",
        IsRequired = true,
        Order = 10,
        Key = AttributeKeys.FirstStepsClassAttribute )]
    [KeyValueListField( "Actions Taken",
        description: "The Key is the person attribute key that holds the person’s completed date and the Value is the icon to use for indicating completion.",
        keyPrompt: "Attribute key",
        valuePrompt: "Icon Css Class",
        IsRequired = true,
        Order = 11,
        Key = AttributeKeys.ActionsTaken )]
    [KeyValueListField( "Connections",
        description: "The Key is the NoteTypeId and the Value is the icon to use for indicating completion.",
        keyPrompt: "NoteTypeId",
        valuePrompt: "Icon Css Class",
        IsRequired = true,
        Order = 12,
        Key = AttributeKeys.Connections )]
    # endregion Block Attributes
    public partial class OnboardingDashboard : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the workflow type.
        /// </summary>
        /// <value>
        /// The state of the workflow type.
        /// </value>
        private Dictionary<Guid, string> WorkFlowTypesState { get; set; }

        /// <summary>
        /// Gets or sets the action key value.
        /// </summary>
        /// <value>
        /// The action key value.
        /// </value>
        private Dictionary<string, string> ActionKeyValues { get; set; }

        #endregion

        #region Fields 

        private const int PERIOD_IN_DAYS = 112;
        private const string WELCOME_EMAIL = "bc952c3c-4dff-45a3-9dd0-217cda23e5fb";
        private const string WELCOME_LETTER = "03779898-6834-4da9-84c3-834e7bf291de";
        private const string COOKIE_DROP = "eb185880-38fd-454e-88f9-e72d9791ceab";
        private const string NO_RETURN_CARD = "5e863e0c-16c9-4121-a1ea-c9970573cd51";
        private const string SERVING_CARD = "63d2e2a0-0d9e-4b70-a56c-9b896329dc9f";
        private const string SMS_NOTE_TYPE = "381303b2-5ac9-4d25-a6f5-acd35fff2fcf";
        private const string EMAIL_NOTE_TYPE = "f5c4ba16-6f9b-44b0-a357-3d935abc40ab";
        private const string FACE_TO_FACE_NOTE_TYPE = "ec8e4ed9-246b-4559-89cc-b9cd97c92c53";
        private const string PHONE_CALL_NOTE_TYPE = "73f59c35-4dbc-4b0d-928f-d747668518e3";
        private const string MAILED_PERSONAL_NOTE_NOTE_TYPE = "5cc3fcc6-86da-4b88-8060-bcd31a183c61";
        private const string TOUCHPOINT_CONVERSATION_NOTE_TYPE = "a301da6e-742a-4755-b50d-9611c0e134fd";
        private int? _campusId = null;
        private int _campusEntityId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;


        #endregion

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string ExecutiveTeamRole = "ExecutiveTeamRole";
            public const string CampusAttribute = "CampusAttribute";
            public const string AttendanceForGroupType = "AttendanceForGroupType";
            public const string Workflows = "Workflows";
            public const string MaximumFreshWeek = "MaximumFreshWeek";
            public const string MaximumRecentWeek = "MaximumRecentWeek ";
            public const string MaximumTrailingWeek = "MaximumTrailingWeek";
            public const string EngagementBeginDateAttribute = "EngagementBeginDateAttribute";
            public const string EngagementScoreAttribute = "EngagementScoreAttribute ";
            public const string FirstStepsClassAttribute = "FirstStepsClassAttribute";
            public const string ActionsTaken = "ActionsTaken";
            public const string Connections = "Connections";
        }

        #endregion Attribute Keys

        #region Filter Attribute Keys

        private class FilterAttributeKeys
        {
            public const string Week = "Week";
            public const string Engagement_Score = "Engagement Score";
            public const string First_Steps_Class = "First Steps Class";
            public const string Welcome_Email = "Welcome Email";
            public const string Welcome_Letter = "Welcome Letter";
            public const string Cookie_Drop = "Cookie Drop";
            public const string No_Return_Card = "No Return Card";
            public const string Serving_Card = "Serving Card";
            public const string SMS_Personal_Connection = "SMS Personal Connection";
            public const string Email_Personal_Connection = "Email Personal Connection";
            public const string FaceToFace_Personal_Connection = "Face-To-Face Personal Connection";
            public const string Phone_Call_Personal_Connection = "Phone Call Personal Connection";
            public const string Mailed_Personal_Note_Personal_Connection = "Mailed Personal Note Personal Connection";
            public const string Touchpoint_Conversation_Personal_Connection = "Touchpoint Conversation Personal Connection";
        }

        #endregion

        #region ViewState Keys

        protected static class ViewStateKeys
        {
            public const string WorkFlowTypesState = "WorkFlowTypesState";
            public const string CampusId = "CampusId";
            public const string ActionKeyValues = "ActionKeyValues";
        }

        #endregion ViewState Keys

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState[ViewStateKeys.WorkFlowTypesState] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                WorkFlowTypesState = new Dictionary<Guid, string>();
            }
            else
            {
                WorkFlowTypesState = JsonConvert.DeserializeObject<Dictionary<Guid, string>>( json );
            }

            json = ViewState[ViewStateKeys.ActionKeyValues] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ActionKeyValues = new Dictionary<string, string>();
            }
            else
            {
                ActionKeyValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
            }

            _campusId = ViewState[ViewStateKeys.CampusId] as int?;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            gPersons.DataKeyNames = new string[] { "Id" };
            gPersons.PersonIdField = "Id";
            gPersons.RowDataBound += gPersons_RowDataBound;
            gPersons.GridRebind += gPersons_GridRebind;
            gPersons.EntityTypeId = personEntityTypeId;


            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;
            gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;
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

            if ( !IsPostBack )
            {
                bool isValid = IsBlockSettingValid();
                if ( isValid )
                {
                    SetWorkFlow();
                    BindFilter();
                    BindGrid();
                }
            }

            BindRepeater();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKeys.CampusId] = _campusId;
            ViewState[ViewStateKeys.WorkFlowTypesState] = WorkFlowTypesState.ToJson();
            ViewState[ViewStateKeys.ActionKeyValues] = ActionKeyValues.ToJson();

            return base.SaveViewState();
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
            bool isValid = IsBlockSettingValid();
            if ( isValid )
            {
                SetWorkFlow();
                BindFilter();
                BindGrid();
                BindRepeater();
            }
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gfFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterAttributeKeys.First_Steps_Class:
                case FilterAttributeKeys.Welcome_Email:
                case FilterAttributeKeys.Welcome_Letter:
                case FilterAttributeKeys.Cookie_Drop:
                case FilterAttributeKeys.No_Return_Card:
                case FilterAttributeKeys.Serving_Card:
                case FilterAttributeKeys.SMS_Personal_Connection:
                case FilterAttributeKeys.Email_Personal_Connection:
                case FilterAttributeKeys.FaceToFace_Personal_Connection:
                case FilterAttributeKeys.Phone_Call_Personal_Connection:
                case FilterAttributeKeys.Mailed_Personal_Note_Personal_Connection:
                case FilterAttributeKeys.Touchpoint_Conversation_Personal_Connection:
                    {
                        var values = e.Value.Split( ';' ).ToList();
                        if ( values.Count() == 1 )
                        {
                            e.Value = e.Value.AsBoolean().ToYesNo();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( FilterAttributeKeys.Week, nbWeek.Text );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Engagement_Score, nbEngagementScore.Text );
            gfFilter.SaveUserPreference( FilterAttributeKeys.First_Steps_Class, cblFirstSteps.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Welcome_Email, cblWelcomeEmail.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Welcome_Letter, cblWelcomeLetter.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Cookie_Drop, cblCokieDrop.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.No_Return_Card, cblNoReturnCard.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Serving_Card, cblServingCard.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.SMS_Personal_Connection, cblSMSPersonal.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Email_Personal_Connection, cblEmailPersonal.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.FaceToFace_Personal_Connection, cblFaceToFace.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Phone_Call_Personal_Connection, cblPhoneCall.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Mailed_Personal_Note_Personal_Connection, cblMailedPersonalNote.SelectedValues.AsDelimited( ";" ) );
            gfFilter.SaveUserPreference( FilterAttributeKeys.Touchpoint_Conversation_Personal_Connection, cblTouchpointConversation.SelectedValues.AsDelimited( ";" ) );
            BindGrid();
        }


        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            _campusId = ddlCampus.SelectedValueAsId();
            BindGrid();
        }

        /// <summary>
        /// Gets the attendance percentage HTML include bootstrap label
        /// </summary>
        /// <param name="attended">The attended.</param>
        /// <param name="attendancePercentage">The attendance percentage.</param>
        /// <returns></returns>
        public string GetAttendanceColumnHtml( int attended, decimal attendancePercentage )
        {
            string css;

            if ( attendancePercentage >= 75 )
            {
                css = "label label-success";
            }
            else if ( attendancePercentage <= 20 )
            {
                css = "label label-default";
            }
            else
            {
                css = "label label-warning";
            }

            return string.Format( "<span class='{0}'>{2} / {1}%</span>", css, ( int ) Math.Round( attendancePercentage ), attended );
        }

        /// <summary>
        /// Gets the engagement score HTML include bootstrap label
        /// </summary>
        /// <param name="score">The engagement score.</param>
        /// <param name="week">The week.</param>
        /// <returns></returns>
        public string GetEngagementScoreColumnHtml( int score, int week )
        {
            string css;

            if ( week <= 8 )
            {
                if ( score >= 7 )
                {
                    css = "label label-success";
                }
                else
                {
                    css = "label label-default";
                }
            }
            else
            {
                if ( score >= 9 )
                {
                    css = "label label-success";
                }
                else if ( score < 6 )
                {
                    css = "label label-danger";
                }
                else
                {
                    css = "label label-warning";
                }
            }
            return string.Format( "<span class='{0}'>{1}</span>", css, score );
        }

        /// <summary>
        /// Gets the engagement score HTML include bootstrap label
        /// </summary>
        /// <param name="week">The week.</param>
        /// <returns></returns>
        public string GetWeekColumnHtml( int week )
        {
            string css;

            var maximumFreshWeek = GetAttributeValue( AttributeKeys.MaximumFreshWeek ).AsInteger();
            var maximumRecentWeek = GetAttributeValue( AttributeKeys.MaximumRecentWeek ).AsInteger();
            if ( week <= maximumFreshWeek )
            {
                css = "badge-success";
            }
            else if ( week <= maximumRecentWeek )
            {
                css = "badge-info";
            }
            else if ( week <= maximumRecentWeek )
            {
                css = "badge-critical";
            }
            else
            {
                css = "badge-danger";
            }
            return string.Format( "<span class='badge {0}'>{1}</span>", css, week );
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gPersons_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gPersons_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                PersonDataRow personDataRow = e.Row.DataItem as PersonDataRow;
                Literal lActionsTaken = e.Row.FindControl( "lActionsTaken" ) as Literal;
                if ( lActionsTaken != null )
                {
                    if ( personDataRow.IsExporting )
                    {
                        lActionsTaken.Text = personDataRow.CompletedActionTexts.AsDelimited( "," );
                    }
                    else
                    {
                        var actions = GetKeyIconValues( AttributeKeys.ActionsTaken );

                        string actionHtml = "<div class='status-list'>";
                        foreach ( var key in actions.Keys )
                        {
                            string actionTitle = key;
                            if ( ActionKeyValues.ContainsKey( key ) )
                            {
                                actionTitle = ActionKeyValues[key];
                            }

                            if ( personDataRow.CompletedActions.Contains( key ) )
                            {
                                actionHtml += string.Format( "<i class='margin-r-sm {0}' aria-hidden='true' title='{1}'></i>", actions[key], actionTitle );
                            }
                            else
                            {
                                actionHtml += string.Format( "<i class='margin-r-sm {0}' aria-hidden='true' style='opacity: 0.5;' title='{1}'></i>", actions[key], actionTitle );
                            }
                        }
                        actionHtml += "</div>";
                        lActionsTaken.Text = actionHtml;
                    }
                }

                Literal lConnection = e.Row.FindControl( "lConnection" ) as Literal;
                if ( lConnection != null )
                {
                    if ( personDataRow.IsExporting )
                    {
                        lConnection.Text = personDataRow.ConnectionTexts.AsDelimited( "," );
                    }
                    else
                    {
                        var connections = GetKeyIconValues( AttributeKeys.Connections );

                        string connectionHtml = "<div class='status-list'>";
                        foreach ( var key in connections.Keys )
                        {
                            string connectionTitle = string.Empty;
                            var noteType = NoteTypeCache.Get( key.AsInteger() );
                            if ( noteType != null )
                            {
                                connectionTitle = noteType.Name;
                            }
                            if ( personDataRow.Connections.Contains( key.AsInteger() ) )
                            {
                                connectionHtml += string.Format( "<i class='margin-r-sm {0}' aria-hidden='true' title='{1}'></i>", connections[key], connectionTitle );
                            }
                            else
                            {
                                connectionHtml += string.Format( "<i class='margin-r-sm {0}' aria-hidden='true' style='opacity: 0.5;' title='{1}'></i>", connections[key], connectionTitle );
                            }
                        }
                        connectionHtml += "</div>";
                        lConnection.Text = connectionHtml;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptWorkFlow control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptWorkFlow_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var selectedPersonIds = gPersons.SelectedKeys.OfType<int>().ToList();
            if ( !selectedPersonIds.Any() )
            {
                mdGridWarning.Show( "Please select one or more people to run the workflow against.", ModalAlertType.Warning );
                return;
            }

            string selectedWorkFlow = e.CommandArgument.ToString();
            var selectedWorkFlowId = selectedWorkFlow.AsGuid();
            var workflowType = new WorkflowTypeService( new RockContext() ).Get( selectedWorkFlowId );
            lConfirmMsg.Text = string.Format( "Are your sure you wish to activate a {0} Workflow on {1} individuals?", workflowType.Name, selectedPersonIds.Count );
            hfWorkflowTypeId.Value = selectedWorkFlowId.ToString();
            mdConfirm.Show();
        }

        /// <summary>
        /// Handles the Click event of the mdConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirm_Click( object sender, EventArgs e )
        {
            mdConfirm.Hide();

            var workflowTypeId = hfWorkflowTypeId.Value.AsGuid();
            var selectedPersonIds = gPersons.SelectedKeys.OfType<int>().ToList();
            using ( var rockContext = new RockContext() )
            {
                var persons = new PersonService( rockContext ).GetByIds( selectedPersonIds ).ToList();
                var workflowDetails = persons.Select( p => new LaunchWorkflowDetails( p ) ).ToList();
                var launchWorkflowsTxn = new Rock.Transactions.LaunchWorkflowsTransaction( workflowTypeId, workflowDetails );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( launchWorkflowsTxn );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbFollow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbFollow_Click( object sender, EventArgs e )
        {
            var selectedPersonIds = gPersons.SelectedKeys.OfType<int>().ToList();
            if ( !selectedPersonIds.Any() )
            {
                mdGridWarning.Show( "Please select one or more people to follow.", ModalAlertType.Warning );
                return;
            }

            var rockContext = new RockContext();
            var personAliasService = new PersonAliasService( rockContext );
            var followingService = new FollowingService( rockContext );

            var personAliasEntityTypeId = EntityTypeCache.Get( typeof( PersonAlias ) ).Id;
            var paQry = personAliasService.Queryable();

            var alreadyFollowingIds = followingService.Queryable()
                .Where( f =>
                    f.EntityTypeId == personAliasEntityTypeId &&
                    f.PersonAlias.Id == CurrentPersonAlias.Id )
                .Join( paQry, f => f.EntityId, p => p.Id, ( f, p ) => new { PersonAlias = p } )
                .Select( p => p.PersonAlias.PersonId )
                .Distinct()
                .ToList();

            var persons = new PersonService( rockContext ).GetByIds( selectedPersonIds );
            foreach ( int id in selectedPersonIds.Where( id => !alreadyFollowingIds.Contains( id ) ) )
            {
                var person = persons.FirstOrDefault( p => p.Id == id );
                if ( person != null && person.PrimaryAliasId.HasValue )
                {
                    var following = new Following
                    {
                        EntityTypeId = personAliasEntityTypeId,
                        EntityId = person.PrimaryAliasId.Value,
                        PersonAliasId = CurrentPersonAlias.Id
                    };
                    followingService.Add( following );
                }
            }
            rockContext.SaveChanges();
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Load the dropdown
        /// </summary>
        private void LoadDropdown()
        {
            var campuses = CampusCache.All();
            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
            ddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );
            _campusId = PageParameter( "CampusId" ).AsIntegerOrNull();
            ddlCampus.SetValue( _campusId );
        }

        private bool IsBlockSettingValid()
        {
            bool isValid = false;

            var executiveTeamRole = GetAttributeValue( AttributeKeys.ExecutiveTeamRole ).AsGuid();
            Guid? groupTypeGuid = GetAttributeValue( AttributeKeys.AttendanceForGroupType ).AsGuidOrNull();
            var engagementScoreattributeGuid = GetAttributeValue( AttributeKeys.EngagementScoreAttribute ).AsGuidOrNull();
            var firstStepsAttributeGuid = GetAttributeValue( AttributeKeys.FirstStepsClassAttribute ).AsGuidOrNull();
            var engagementBeginDateAttributeGuid = GetAttributeValue( AttributeKeys.EngagementBeginDateAttribute ).AsGuidOrNull();
            if ( executiveTeamRole == default( Guid )
                || !groupTypeGuid.HasValue
                || !engagementScoreattributeGuid.HasValue
                || !firstStepsAttributeGuid.HasValue
                || !engagementBeginDateAttributeGuid.HasValue )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>block setting are not configured.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }


            var engagementScoreattribute = AttributeCache.Get( engagementScoreattributeGuid.Value );
            if ( !( engagementScoreattribute != null && engagementScoreattribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.INTEGER ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Engagement Score Attribute block setting expect attribute with Integer field type.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            var firstStepsAttribute = AttributeCache.Get( firstStepsAttributeGuid.Value );
            if ( !( firstStepsAttribute != null && firstStepsAttribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.DATE ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>First step Attribute block setting expect attribute with date field type.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            var engagementBeginDateAttribute = AttributeCache.Get( engagementBeginDateAttributeGuid.Value );
            if ( !( engagementBeginDateAttribute != null && engagementBeginDateAttribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.DATE ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Engagement begin date Attribute block setting expect attribute with date field type.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            var isInRole = new GroupMemberService( new RockContext() ).Queryable()
                            .Where( m =>
                                        m.Group.Guid == executiveTeamRole
                                        && m.PersonId == CurrentPerson.Id
                                    )
                            .Any();

            if ( isInRole )
            {
                ddlCampus.Visible = true;
                LoadDropdown();
            }
            else
            {
                ddlCampus.Visible = false;
                var attributeGuid = GetAttributeValue( AttributeKeys.CampusAttribute ).AsGuid();
                var attribute = AttributeCache.Get( attributeGuid );
                if ( !( attribute != null && attribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.CAMPUS ) ).Id ) )
                {
                    nbWarningMessage.Title = "Error";
                    nbWarningMessage.Text = "<p>Campus Attribute block setting is not correctly configured.</p>";
                    nbWarningMessage.Visible = true;
                    return isValid;
                }

                CurrentPerson.LoadAttributes();
                var campusGuid = CurrentPerson.GetAttributeValue( attribute.Key ).AsGuid();
                var campus = CampusCache.Get( campusGuid );
                if ( campus == null )
                {
                    nbWarningMessage.Title = "Error";
                    nbWarningMessage.Text = "<p>No campus found associated with current person.</p>";
                    nbWarningMessage.Visible = true;
                    return isValid;
                }

                _campusId = campus.Id;
            }
            return true;
        }

        /// <summary>
        /// Sets the filter.
        /// </summary>
        private void SetWorkFlow()
        {
            var guidList = GetAttributeValue( AttributeKeys.Workflows ).SplitDelimitedValues().AsGuidList();
            using ( var rockContext = new RockContext() )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                WorkFlowTypesState = new WorkflowTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t => guidList.Contains( t.Guid ) &&
                        t.IsActive == true )
                    .ToDictionary( a => a.Guid, b => b.Name );

            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            nbWeek.Text = gfFilter.GetUserPreference( FilterAttributeKeys.Week );
            nbEngagementScore.Text = gfFilter.GetUserPreference( FilterAttributeKeys.Engagement_Score );

            BindsCheckboxList( FilterAttributeKeys.First_Steps_Class, cblFirstSteps );
            BindsCheckboxList( FilterAttributeKeys.Welcome_Email, cblWelcomeEmail );
            BindsCheckboxList( FilterAttributeKeys.Welcome_Letter, cblWelcomeLetter );
            BindsCheckboxList( FilterAttributeKeys.Cookie_Drop, cblCokieDrop );
            BindsCheckboxList( FilterAttributeKeys.No_Return_Card, cblNoReturnCard );
            BindsCheckboxList( FilterAttributeKeys.Serving_Card, cblServingCard );
            BindsCheckboxList( FilterAttributeKeys.SMS_Personal_Connection, cblSMSPersonal );
            BindsCheckboxList( FilterAttributeKeys.Email_Personal_Connection, cblEmailPersonal );
            BindsCheckboxList( FilterAttributeKeys.FaceToFace_Personal_Connection, cblFaceToFace );
            BindsCheckboxList( FilterAttributeKeys.Phone_Call_Personal_Connection, cblPhoneCall );
            BindsCheckboxList( FilterAttributeKeys.Mailed_Personal_Note_Personal_Connection, cblMailedPersonalNote );
            BindsCheckboxList( FilterAttributeKeys.Touchpoint_Conversation_Personal_Connection, cblTouchpointConversation );
        }

        /// <summary>
        /// Binds the checkbx list.
        /// </summary>
        private void BindsCheckboxList( string attributeKey, RockCheckBoxList control )
        {
            string controlValue = gfFilter.GetUserPreference( attributeKey );
            if ( !string.IsNullOrWhiteSpace( controlValue ) )
            {
                control.SetValues( controlValue.Split( ';' ).ToList() );
            }
            else
            {
                control.SetValues( new List<string>() );
            }

        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {
            var rockContext = new RockContext();
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            AttributeCache beginDateAttribute = AttributeCache.Get( GetAttributeValue( AttributeKeys.EngagementBeginDateAttribute ).AsGuid() );
            AttributeCache engagementScoreattribute = AttributeCache.Get( GetAttributeValue( AttributeKeys.EngagementScoreAttribute ).AsGuid() );
            AttributeCache firstStepsAttribute = AttributeCache.Get( GetAttributeValue( AttributeKeys.FirstStepsClassAttribute ).AsGuid() );
            var noteTypeIds = GetKeyIconValues( AttributeKeys.Connections ).Keys.ToList().AsIntegerList();
            var actionKeys = GetKeyIconValues( AttributeKeys.ActionsTaken ).Keys.ToList();
            Dictionary<int, string> noteTypeKeyValuePair = new Dictionary<int, string>();
            ActionKeyValues = new Dictionary<string, string>();
            if ( isExporting )
            {
                foreach ( var noteTypeId in noteTypeIds )
                {
                    var noteType = NoteTypeCache.Get( noteTypeId );
                    noteTypeKeyValuePair.AddOrReplace( noteTypeId, noteType.Name );
                }
            }

            foreach ( var actionKey in actionKeys )
            {
                var attribute = new AttributeService( rockContext ).GetByEntityTypeId( personEntityTypeId ).FirstOrDefault( a => a.Key == actionKey );
                if ( attribute != null )
                {
                    ActionKeyValues.AddOrReplace( actionKey, attribute.Name );
                }
            }

            GroupTypeCache groupType = GroupTypeCache.Get( GetAttributeValue( AttributeKeys.AttendanceForGroupType ).AsGuid() );
            DateTime currentDate = RockDateTime.Now.Date;
            DateTime tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -PERIOD_IN_DAYS );

            var personIds = new AttributeValueService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.AttributeId == beginDateAttribute.Id &&
                    a.ValueAsDateTime.HasValue &&
                    a.ValueAsDateTime.Value >= startDate &&
                    a.ValueAsDateTime.Value < tomorrowDate &&
                    a.Attribute.EntityTypeId == personEntityTypeId &&
                    a.EntityId.HasValue )
                   .Select( a => a.EntityId.Value )
                   .ToList();

            var persons = new PersonService( rockContext ).GetByIds( personIds );
            if ( _campusId.HasValue )
            {
                persons = persons.Where( a => a.PrimaryFamily.CampusId == _campusId.Value );
            }

            persons = persons.Where( a => a.AgeClassification == AgeClassification.Adult );
            persons.LoadAttributes();
            personIds = persons.Select( a => a.Id ).ToList();

            var filterActions = new Dictionary<string, bool?>();
            filterActions.Add( WELCOME_EMAIL, GetFilterValueAsBoolean( FilterAttributeKeys.Welcome_Email ) );
            filterActions.Add( WELCOME_LETTER, GetFilterValueAsBoolean( FilterAttributeKeys.Welcome_Letter ) );
            filterActions.Add( COOKIE_DROP, GetFilterValueAsBoolean( FilterAttributeKeys.Cookie_Drop ) );
            filterActions.Add( SERVING_CARD, GetFilterValueAsBoolean( FilterAttributeKeys.Serving_Card ) );
            filterActions.Add( NO_RETURN_CARD, GetFilterValueAsBoolean( FilterAttributeKeys.No_Return_Card ) );

            List<PersonDataRow> personDataRows = new List<PersonDataRow>();
            foreach ( var person in persons )
            {
                PersonDataRow personDataRow = new PersonDataRow()
                {
                    Id = person.Id,
                    Person = person,
                    StartDate = person.GetAttributeValue( beginDateAttribute.Key ).AsDateTime().Value,
                    EndDate = person.GetAttributeValue( beginDateAttribute.Key ).AsDateTime().Value.AddDays( PERIOD_IN_DAYS ),
                    EngagementScore = person.GetAttributeValue( engagementScoreattribute.Key ).AsInteger(),
                    FullName = person.FullName,
                    IsFirstSteps = person.GetAttributeValue( firstStepsAttribute.Key ).IsNotNullOrWhiteSpace(),
                    IsExporting = isExporting
                };

                string week = gfFilter.GetUserPreference( FilterAttributeKeys.Week );
                if ( !string.IsNullOrWhiteSpace( week ) )
                {
                    if ( personDataRow.Week != week.AsInteger() )
                    {
                        continue;
                    }
                }

                string engagementScore = gfFilter.GetUserPreference( FilterAttributeKeys.Engagement_Score );
                if ( !string.IsNullOrWhiteSpace( engagementScore ) )
                {
                    if ( personDataRow.EngagementScore != engagementScore.AsInteger() )
                    {
                        continue;
                    }
                }

                bool? isFirstStep = GetFilterValueAsBoolean( FilterAttributeKeys.First_Steps_Class );
                if ( isFirstStep.HasValue )

                {
                    if ( personDataRow.IsFirstSteps != isFirstStep.Value )
                    {
                        continue;
                    }
                }

                bool isActionsFilterValid = true;
                foreach ( var action in filterActions.Where( a => a.Value.HasValue ) )
                {
                    var attribute = AttributeCache.Get( action.Key );
                    var hasActionValid = false;
                    if ( attribute != null && person.GetAttributeValue( attribute.Key ).IsNotNullOrWhiteSpace() )
                    {
                        hasActionValid = true;
                    }

                    if ( action.Value.Value != hasActionValid )
                    {
                        isActionsFilterValid = false;
                        break;
                    }
                }

                if ( !isActionsFilterValid )
                {
                    continue;
                }

                personDataRow.CompletedActions = new List<string>();
                personDataRow.CompletedActionTexts = new List<string>();

                foreach ( var actionKey in actionKeys )
                {
                    if ( person.GetAttributeValue( actionKey ).IsNotNullOrWhiteSpace() )
                    {
                        personDataRow.CompletedActions.Add( actionKey );

                        if ( isExporting )
                        {
                            personDataRow.CompletedActionTexts.Add( ActionKeyValues[actionKey] );
                        }
                    };
                }

                personDataRows.Add( personDataRow );
            }


            var attendanceQry = new AttendanceService( rockContext )
            .Queryable().AsNoTracking()
            .Where( a =>
                a.Occurrence.Group != null &&
                a.Occurrence.Group.GroupTypeId == groupType.Id &&
                a.PersonAlias != null &&
                personIds.Contains( a.PersonAlias.PersonId )
                );

            List<AttendanceRow> attendanceRows = new List<AttendanceRow>();
            if ( groupType.AttendanceCountsAsWeekendService )
            {
                attendanceQry = attendanceQry.Where( a => a.Occurrence.SundayDate >= startDate &&
                     a.Occurrence.SundayDate < tomorrowDate &&
                a.DidAttend.HasValue && a.DidAttend.Value == true );


                attendanceRows = attendanceQry
                    .Select( a => new AttendanceRow
                    {
                        PersonId = a.PersonAlias.PersonId,
                        Date = a.Occurrence.SundayDate,
                        DidAttend = true
                    } )
                    .ToList();
            }
            else
            {
                attendanceQry = attendanceQry.Where( a => a.StartDateTime >= startDate &&
                     a.StartDateTime < tomorrowDate );

                attendanceRows = attendanceQry
                    .Select( a => new AttendanceRow
                    {
                        PersonId = a.PersonAlias.PersonId,
                        Date = a.StartDateTime,
                        DidAttend = a.DidAttend.Value
                    } )
                    .ToList();
            }


            var personConnections = new NoteService( rockContext )
                                .Queryable()
                                .Where( a => noteTypeIds.Contains( a.NoteTypeId )
                                 && a.NoteType.EntityTypeId == personEntityTypeId
                                 && a.EntityId.HasValue
                                 && personIds.Contains( a.EntityId.Value ) )
                                .Select( a => new { a.NoteTypeId, PersonId = a.EntityId.Value } )
                                .Distinct()
                                .ToList();

            var connectionFilters = new Dictionary<string, bool?>();
            connectionFilters.Add( SMS_NOTE_TYPE, GetFilterValueAsBoolean( FilterAttributeKeys.SMS_Personal_Connection ) );
            connectionFilters.Add( EMAIL_NOTE_TYPE, GetFilterValueAsBoolean( FilterAttributeKeys.Email_Personal_Connection ) );
            connectionFilters.Add( FACE_TO_FACE_NOTE_TYPE, GetFilterValueAsBoolean( FilterAttributeKeys.FaceToFace_Personal_Connection ) );
            connectionFilters.Add( PHONE_CALL_NOTE_TYPE, GetFilterValueAsBoolean( FilterAttributeKeys.Phone_Call_Personal_Connection ) );
            connectionFilters.Add( MAILED_PERSONAL_NOTE_NOTE_TYPE, GetFilterValueAsBoolean( FilterAttributeKeys.Mailed_Personal_Note_Personal_Connection ) );
            connectionFilters.Add( TOUCHPOINT_CONVERSATION_NOTE_TYPE, GetFilterValueAsBoolean( FilterAttributeKeys.Touchpoint_Conversation_Personal_Connection ) );

            List<int> invalidPersons = new List<int>();
            foreach ( var personDataRow in personDataRows )
            {
                personDataRow.Connections = personConnections.Where( a => a.PersonId == personDataRow.Id ).Select( a => a.NoteTypeId ).ToList();

                var isConnectionFilterValid = true;
                foreach ( var item in connectionFilters.Where( a => a.Value.HasValue ) )
                {
                    var noteType = NoteTypeCache.Get( item.Key );
                    var hasActionValid = false;
                    if ( noteType != null && personDataRow.Connections.Contains( noteType.Id ) )
                    {
                        hasActionValid = true;
                    }

                    if ( hasActionValid != item.Value.Value )
                    {
                        isConnectionFilterValid = false;
                        break;
                    }
                }

                if ( !isConnectionFilterValid )
                {
                    invalidPersons.Add( personDataRow.Id );
                    continue;
                }

                if ( groupType.AttendanceCountsAsWeekendService )
                {
                    personDataRow.Attendance = attendanceRows.Where( a => a.PersonId == personDataRow.Id ).Select( a => a.Date ).Distinct().Count();
                    if ( personDataRow.Week != default( int ) )
                    {
                        personDataRow.AttendancePercent = personDataRow.Attendance / Convert.ToDecimal( personDataRow.Week ) * 100;
                    }
                }
                else
                {
                    personDataRow.Attendance = attendanceRows.Where( a => a.PersonId == personDataRow.Id && a.DidAttend ).Count();
                    var total = attendanceRows.Where( a => a.PersonId == personDataRow.Id ).Count();
                    personDataRow.AttendancePercent = personDataRow.Attendance / Convert.ToDecimal( total ) * 100;
                }



                if ( isExporting )
                {
                    personDataRow.ConnectionTexts = new List<string>();
                    personDataRow.ConnectionTexts = personDataRow.Connections
                                            .Where( a => noteTypeKeyValuePair.ContainsKey( a ) )
                                            .Select( a => noteTypeKeyValuePair[a] )
                                            .ToList();
                }
            }

            if ( invalidPersons.Any() )
            {
                personDataRows.RemoveAll( a => invalidPersons.Contains( a.Id ) );
            }

            SortProperty sortProperty = gPersons.SortProperty;
            if ( sortProperty != null )
            {
                gPersons.DataSource = personDataRows.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gPersons.DataSource = personDataRows
                    .OrderBy( r => r.Week )
                    .ThenBy( r => r.Person.LastName )
                    .ThenBy( r => r.Person.NickName ).ToList();
            }
            gPersons.DataBind();
        }

        private bool? GetFilterValueAsBoolean( string attributeKey )
        {
            bool? value = null;
            string valueString = gfFilter.GetUserPreference( attributeKey );
            if ( !string.IsNullOrWhiteSpace( valueString ) )
            {
                var values = valueString.Split( ';' ).ToList();
                if ( values.Count == 1 )
                {
                    value = valueString.AsBoolean();
                }
            }

            return value;
        }

        /// <summary>
        /// Binds the workflow repeater.
        /// </summary>
        private void BindRepeater()
        {
            rptWorkFlow.DataSource = WorkFlowTypesState;
            rptWorkFlow.DataBind();
        }

        private Dictionary<string, string> GetKeyIconValues( string attributeKey )
        {
            var keyIconValues = new Dictionary<string, string>();
            var keyIconValuesString = GetAttributeValue( attributeKey );
            if ( !string.IsNullOrWhiteSpace( keyIconValuesString ) )
            {
                keyIconValuesString = keyIconValuesString.TrimEnd( '|' );
                foreach ( var keyVal in keyIconValuesString.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Where( s => s.Length == 2 ) )
                {
                    keyIconValues.AddOrIgnore( keyVal[0], keyVal[1] );
                }
            }

            return keyIconValues;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class PersonDataRow : DotLiquid.Drop
        {
            public int Id { get; set; }

            public Person Person { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public int Week
            {
                get
                {
                    var days = Convert.ToDecimal( RockDateTime.Now.Subtract( StartDate ).Days );
                    var week = Math.Ceiling( days / 7 );
                    if ( week == 0 )
                    {
                        return 0;
                    }
                    else
                    {
                        return Convert.ToInt32( week );
                    }
                }
            }

            public string FullName { get; set; }

            public int Attendance { get; set; }

            public decimal AttendancePercent { get; set; }

            public int EngagementScore { get; set; }

            public bool IsFirstSteps { get; set; }

            public List<string> CompletedActions { get; set; }

            public List<int> Connections { get; set; }

            public List<string> CompletedActionTexts { get; set; }

            public List<string> ConnectionTexts { get; set; }

            public bool IsExporting { get; set; }

        }


        public class AttendanceRow
        {
            public int PersonId { get; set; }
            public DateTime Date { get; set; }
            public bool DidAttend { get; set; }
        }

        #endregion

    }
}