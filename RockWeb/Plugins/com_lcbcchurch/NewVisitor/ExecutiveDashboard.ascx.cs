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

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Executive Dashboard" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "Executive and campus dashboard using Lava." )]

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
    [CodeEditorField( "Dashboard Panel Lava Template",
        description: "The lava template used to display the dashboard metrics in the first panel.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        IsRequired = false,
        DefaultValue = @"",
        Key = AttributeKeys.DashboardPanelLavaTemplate,
        Order = 2 )]
    [CodeEditorField( "Activities Panel Lava Template",
        description: "The lava template to be shown below the dashboard panel. If {{ Campus.Id }} is blank then 'All Campuses' has been selected by a member of the executive team.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        IsRequired = false,
        DefaultValue = @"",
        Key = AttributeKeys.ActivitiesPanelLavaTemplate,
        Order = 3 )]
    [MetricCategoriesField( "Welcome Email Fulfilled",
        Description = "Select the metric for Welcome Email Fulfilled.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.WelcomeEmailFulfilled,
        Order = 4 )]
    [MetricCategoriesField( "Welcome Email Qualified",
        Description = "Select the metric for Welcome Email Qualified.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.WelcomeEmailQualified,
        Order = 5 )]
    [MetricCategoriesField( "Welcome Card Fulfilled",
        Description = "Select the metric for Welcome Card Fulfilled.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.WelcomeCardFulfilled,
        Order = 6 )]
    [MetricCategoriesField( "Welcome Card Qualified",
        Description = "Select the metric for Welcome Card Qualified.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.WelcomeCardQualified,
        Order = 7 )]
    [MetricCategoriesField( "No Return 2nd Week Fulfilled",
        Description = "Select the metric for No Return 2nd Week Fulfilled.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.NoReturn2ndWeekFulfilled,
        Order = 8 )]
    [MetricCategoriesField( "No Return 2nd Week Qualified",
        Description = "Select the metric for No Return 2nd Week Qualified.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.NoReturn2ndWeekQualified,
        Order = 9 )]
    [MetricCategoriesField( "Cookie Drop Fulfilled",
        Description = "Select the metric for Cookie Drop Fulfilled.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.CookieDropFulfilled,
        Order = 10 )]
    [MetricCategoriesField( "Cookie Drop Qualified",
        Description = "Select the metric for Cookie Drop Qualified.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.CookieDropQualified,
        Order = 11 )]
    [MetricCategoriesField( "Serve Card Fulfilled",
        Description = "Select the metric for Serve Card Fulfilled.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.ServeCardFulfilled,
        Order = 12 )]
    [MetricCategoriesField( "Serve Card Qualified",
        Description = "Select the metric for Serve Card Qualified.",
        Category = "Attendance Follow-up",
        IsRequired = false,
        Key = AttributeKeys.ServeCardQualified,
        Order = 13 )]
    [MetricCategoriesField( "Staff Connections Fulfilled Metric",
        Description = "Select the metric for staff connection fulfilled.",
        Category = "Staff Connections",
        IsRequired = false,
        Key = AttributeKeys.StaffConnectionsFulfilledMetric,
        Order = 14 )]
    [MetricCategoriesField( "Staff Connections Qualified Metric",
        Description = "Select the metric for staff connection qualified.",
        Category = "Staff Connections",
        IsRequired = false,
        Key = AttributeKeys.StaffConnectionsQualifiedMetric,
        Order = 15 )]
    [MetricCategoriesField( "Face-to-Face Contacts (Progressive)",
        Description = "Select the metric for Face-to-Face Contacts (Progressive).",
        Category = "Face-to-Face Contacts",
        IsRequired = false,
        Key = AttributeKeys.FacetoFaceContactsProgressive,
        Order = 16 )]
    [MetricCategoriesField( "Face-to-Face Contacts (Total)",
        Description = "Select the metric for Face-to-Face Contacts (Total).",
        Category = "Face-to-Face Contacts",
        IsRequired = false,
        Key = AttributeKeys.FacetoFaceContactsTotal,
        Order = 17 )]
    [MetricCategoriesField( "Leaving AOP This Month (Progressive)",
        Description = "Select the metric for Leaving AOP This Month (Progressive).",
        Category = "Face-to-Face Contacts",
        IsRequired = false,
        Key = AttributeKeys.LeavingAOPThisMonthProgressive,
        Order = 18 )]
    [MetricCategoriesField( "Leaving AOP This Month (Total)",
        Description = "Select the metric for Leaving AOP This Month (Total).",
        Category = "Face-to-Face Contacts",
        IsRequired = false,
        Key = AttributeKeys.LeavingAOPThisMonthTotal,
        Order = 19 )]
    [MetricCategoriesField( "First Steps (Progressive)",
        Description = "Select the metric for First Steps (Progressive).",
        Category = "First Steps",
        IsRequired = false,
        Key = AttributeKeys.FirstStepsProgressive,
        Order = 20 )]
    [MetricCategoriesField( "First Steps (Total)",
        Description = "Select the metric for First Steps (Total).",
        Category = "First Steps",
        IsRequired = false,
        Key = AttributeKeys.FirstStepsTotal,
        Order = 21 )]
    [MetricCategoriesField( "Next Steps (Progressive)",
        Description = "Select the metric for Next Steps (Progressive).",
        Category = "Next Steps",
        IsRequired = false,
        Key = AttributeKeys.NextStepsProgressive,
        Order = 22 )]
    [MetricCategoriesField( "Next Steps (Total)",
        Description = "Select the metric for Next Steps (Total).",
        Category = "Next Steps",
        IsRequired = false,
        Key = AttributeKeys.NextStepsTotal,
        Order = 23 )]
    [LinkedPage( "Attendance Follow-up Page",
        Key = AttributeKeys.AttendanceFollowupPage,
        Description = "Link to the Attendance Follow-up page",
        Category = "Dashboard Linked Pages",
        IsRequired = false,
        Order = 24 )]
    [LinkedPage( "Staff Connections Page",
        Key = AttributeKeys.StaffConnectionsPage,
        Description = "Link to the Staff Connections page",
        Category = "Dashboard Linked Pages",
        IsRequired = false,
        Order = 25 )]
    [LinkedPage( "Face-to-Face Contacts Page",
        Key = AttributeKeys.FacetoFaceContactsPage,
        Description = "Link to the Face-to-Face Contacts page",
        Category = "Dashboard Linked Pages",
        IsRequired = false,
        Order = 26 )]
    [LinkedPage( "First Steps Page",
        Key = AttributeKeys.FirstStepsPage,
        Description = "Link to the First Steps page",
        Category = "Dashboard Linked Pages",
        IsRequired = false,
        Order = 27 )]
    [LinkedPage( "Next Steps Page",
        Key = AttributeKeys.NextStepsPage,
        Description = "Link to the Next Steps page",
        Category = "Dashboard Linked Pages",
        IsRequired = false,
        Order = 28 )]
    # endregion Block Attributes
    public partial class ExecutiveDashboard : Rock.Web.UI.RockBlock
    {
        #region Fields 

        private List<int> _campusIds = new List<int>();
        private int _campusEntityId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;

        #endregion

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string ExecutiveTeamRole = "ExecutiveTeamRole";
            public const string CampusAttribute = "CampusAttribute";
            public const string DashboardPanelLavaTemplate = "DashboardPanelLavaTemplate";
            public const string ActivitiesPanelLavaTemplate = "ActivitiesPanelLavaTemplate";
            public const string WelcomeEmailFulfilled = "WelcomeEmailFulfilled";
            public const string WelcomeEmailQualified = "WelcomeEmailQualified ";
            public const string WelcomeCardFulfilled = "WelcomeCardFulfilled";
            public const string WelcomeCardQualified = "WelcomeCardQualified ";
            public const string NoReturn2ndWeekFulfilled = "NoReturn2ndWeekFulfilled";
            public const string NoReturn2ndWeekQualified = "NoReturn2ndWeekQualified";
            public const string CookieDropFulfilled = "CookieDropFulfilled";
            public const string CookieDropQualified = "CookieDropQualified ";
            public const string ServeCardFulfilled = "ServeCardFulfilled";
            public const string ServeCardQualified = "ServeCardQualified ";
            public const string StaffConnectionsFulfilledMetric = "StaffConnectionsFulfilledMetric";
            public const string StaffConnectionsQualifiedMetric = "StaffConnectionsQualifiedMetric";
            public const string FacetoFaceContactsProgressive = "FacetoFaceContactsProgressive";
            public const string FacetoFaceContactsTotal = "FacetoFaceContactsTotal";
            public const string LeavingAOPThisMonthProgressive = "LeavingAOPThisMonthProgressive";
            public const string LeavingAOPThisMonthTotal = "LeavingAOPThisMonthTotal";
            public const string FirstStepsProgressive = "FirstStepsProgressive";
            public const string FirstStepsTotal = "FirstStepsTotal";
            public const string NextStepsProgressive = "NextStepsProgressive";
            public const string NextStepsTotal = "NextStepsTotal";
            public const string AttendanceFollowupPage = "AttendanceFollowupPage";
            public const string StaffConnectionsPage = "StaffConnectionsPage";
            public const string FacetoFaceContactsPage = "FacetoFaceContactsPage";
            public const string FirstStepsPage = "FirstStepsPage";
            public const string NextStepsPage = "NextStepsPage";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                    ShowDashboardContent();
                    ShowActivitiesContent();
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
            bool isValid = IsBlockSettingValid();
            if ( isValid )
            {
                ShowDashboardContent();
                ShowActivitiesContent();
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            if ( !ddlCampus.SelectedValueAsId().HasValue )
            {
                _campusIds = CampusCache.All().Select( a => a.Id ).ToList();
            }
            else
            {
                _campusIds = new List<int>() { ddlCampus.SelectedValue.AsInteger() };
            }
            ShowDashboardContent();
            ShowActivitiesContent();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Load the dropdown
        /// </summary>
        private void LoadDropdown()
        {
            var campuses = CampusCache.All();
            _campusIds = campuses.Select( a => a.Id ).ToList();
            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
            ddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );
        }

        private bool IsBlockSettingValid()
        {
            bool isValid = false;

            var executiveTeamRole = GetAttributeValue( AttributeKeys.ExecutiveTeamRole ).AsGuid();
            if ( executiveTeamRole == default( Guid ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>block setting are not configured.</p>";
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

                _campusIds.Add( campus.Id );
            }
            return true;
        }

        /// <summary>
        /// Display the content
        /// </summary>
        public void ShowActivitiesContent()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            if ( _campusIds.Count == 1 )
            {
                var campus = CampusCache.Get( _campusIds.First() );
                mergeFields.Add( "Campus", campus );
            }
            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( AttributeKeys.AttendanceFollowupPage, LinkedPageRoute( AttributeKeys.AttendanceFollowupPage ) );
            linkedPages.Add( AttributeKeys.StaffConnectionsPage, LinkedPageRoute( AttributeKeys.StaffConnectionsPage ) );
            linkedPages.Add( AttributeKeys.FacetoFaceContactsPage, LinkedPageRoute( AttributeKeys.FacetoFaceContactsPage ) );
            linkedPages.Add( AttributeKeys.FirstStepsPage, LinkedPageRoute( AttributeKeys.FirstStepsPage ) );
            linkedPages.Add( AttributeKeys.NextStepsPage, LinkedPageRoute( AttributeKeys.NextStepsPage ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            lActivitiesContent.Text = GetAttributeValue( AttributeKeys.ActivitiesPanelLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Display the content
        /// </summary>
        public void ShowDashboardContent()
        {
            var attendanceFollowup = new MetricWeeklyCategory();
            attendanceFollowup.LastWeek.Numerator = GetRecentMetricValue( AttributeKeys.WelcomeEmailFulfilled )
                                                + GetRecentMetricValue( AttributeKeys.WelcomeCardFulfilled )
                                                + GetRecentMetricValue( AttributeKeys.NoReturn2ndWeekFulfilled )
                                                + GetRecentMetricValue( AttributeKeys.CookieDropFulfilled )
                                                + GetRecentMetricValue( AttributeKeys.ServeCardFulfilled );
            attendanceFollowup.LastWeek.Denominator = GetRecentMetricValue( AttributeKeys.WelcomeEmailQualified )
                                                + GetRecentMetricValue( AttributeKeys.WelcomeCardQualified )
                                                + GetRecentMetricValue( AttributeKeys.NoReturn2ndWeekQualified )
                                                + GetRecentMetricValue( AttributeKeys.CookieDropQualified )
                                                + GetRecentMetricValue( AttributeKeys.ServeCardQualified );
            attendanceFollowup.FourWeekAverage.Numerator = GetLastFourMetricValue( AttributeKeys.WelcomeEmailFulfilled )
                                                + GetLastFourMetricValue( AttributeKeys.WelcomeCardFulfilled )
                                                + GetLastFourMetricValue( AttributeKeys.NoReturn2ndWeekFulfilled )
                                                + GetLastFourMetricValue( AttributeKeys.CookieDropFulfilled )
                                                + GetLastFourMetricValue( AttributeKeys.ServeCardFulfilled );
            attendanceFollowup.FourWeekAverage.Denominator = GetLastFourMetricValue( AttributeKeys.WelcomeEmailQualified )
                                                + GetLastFourMetricValue( AttributeKeys.WelcomeCardQualified )
                                                + GetLastFourMetricValue( AttributeKeys.NoReturn2ndWeekQualified )
                                                + GetLastFourMetricValue( AttributeKeys.CookieDropQualified )
                                                + GetLastFourMetricValue( AttributeKeys.ServeCardQualified );

            var staffConnection = new MetricWeeklyCategory();
            staffConnection.LastWeek.Numerator = GetRecentMetricValue( AttributeKeys.StaffConnectionsFulfilledMetric );
            staffConnection.LastWeek.Denominator = GetRecentMetricValue( AttributeKeys.StaffConnectionsQualifiedMetric );
            staffConnection.FourWeekAverage.Numerator = GetLastFourMetricValue( AttributeKeys.StaffConnectionsFulfilledMetric );
            staffConnection.FourWeekAverage.Denominator = GetLastFourMetricValue( AttributeKeys.StaffConnectionsQualifiedMetric );

            decimal leavingAOPThisMonthTotal = GetRecentMetricValue( AttributeKeys.LeavingAOPThisMonthTotal );
            decimal leavingAOPThisMonthProgressive = GetRecentMetricValue( AttributeKeys.LeavingAOPThisMonthProgressive );
            var faceToFaceConnections = new MetricWMonthlyCategory();
            faceToFaceConnections.CurrentMonth.Numerator = GetRecentMetricValue( AttributeKeys.FacetoFaceContactsProgressive );
            faceToFaceConnections.CurrentMonth.Denominator = leavingAOPThisMonthProgressive;
            faceToFaceConnections.LastMonth.Numerator = GetRecentMetricValue( AttributeKeys.FacetoFaceContactsTotal );
            faceToFaceConnections.LastMonth.Denominator = leavingAOPThisMonthTotal;

            var firstSteps = new MetricWMonthlyCategory();
            firstSteps.CurrentMonth.Numerator = GetRecentMetricValue( AttributeKeys.FirstStepsProgressive );
            firstSteps.CurrentMonth.Denominator = leavingAOPThisMonthProgressive;
            firstSteps.LastMonth.Numerator = GetRecentMetricValue( AttributeKeys.FirstStepsTotal );
            firstSteps.LastMonth.Denominator = leavingAOPThisMonthTotal;

            var nextSteps = new MetricWMonthlyCategory();
            nextSteps.CurrentMonth.Numerator = GetRecentMetricValue( AttributeKeys.NextStepsProgressive );
            nextSteps.CurrentMonth.Denominator = leavingAOPThisMonthProgressive;
            nextSteps.LastMonth.Numerator = GetRecentMetricValue( AttributeKeys.NextStepsTotal );
            nextSteps.LastMonth.Denominator = leavingAOPThisMonthTotal;


            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "AttendanceFollowup", attendanceFollowup );
            mergeFields.Add( "StaffConnection", staffConnection );
            mergeFields.Add( "FaceToFaceConnections", faceToFaceConnections );
            mergeFields.Add( "FirstSteps", firstSteps );
            mergeFields.Add( "NextSteps", nextSteps );
            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( AttributeKeys.AttendanceFollowupPage, LinkedPageRoute( AttributeKeys.AttendanceFollowupPage ) );
            linkedPages.Add( AttributeKeys.StaffConnectionsPage, LinkedPageRoute( AttributeKeys.StaffConnectionsPage ) );
            linkedPages.Add( AttributeKeys.FacetoFaceContactsPage, LinkedPageRoute( AttributeKeys.FacetoFaceContactsPage ) );
            linkedPages.Add( AttributeKeys.FirstStepsPage, LinkedPageRoute( AttributeKeys.FirstStepsPage ) );
            linkedPages.Add( AttributeKeys.NextStepsPage, LinkedPageRoute( AttributeKeys.NextStepsPage ) );
            mergeFields.Add( "LinkedPages", linkedPages );
            if ( _campusIds.Count == 1 )
            {
                var campus = CampusCache.Get( _campusIds.First() );
                mergeFields.Add( "Campus", campus );
            }
            lDashboardContent.Text = GetAttributeValue( AttributeKeys.DashboardPanelLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        private decimal GetLastFourMetricValue( string attributeKey )
        {
            decimal value = 0;
            var rockContext = new RockContext();
            foreach ( var campusId in _campusIds )
            {
                var qryMetricValues = GetMetricValues( attributeKey, campusId, rockContext );
                var metricValues = qryMetricValues.Skip( 1 ).Take( 4 ).ToList();
                if ( metricValues != null && metricValues.Any() )
                {
                    value += metricValues.Where( a => a.YValue.HasValue ).Select( a => a.YValue.Value ).Sum();
                }
            }
            return value;
        }

        private decimal GetRecentMetricValue( string attributeKey )
        {
            var rockContext = new RockContext();
            decimal value = 0;
            foreach ( var campusId in _campusIds )
            {
                var qryMetricValues = GetMetricValues( attributeKey, campusId, rockContext );
                var metricValue = qryMetricValues.FirstOrDefault();
                if ( metricValue != null && metricValue.YValue.HasValue )
                {
                    value += metricValue.YValue.Value;
                }
            }

            return value;
        }

        private IQueryable<MetricValue> GetMetricValues( string attributeKey, int campusId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( attributeKey ) );
            var metricGuid = Guid.Empty;
            if ( metricCategories.Any() )
            {
                metricGuid = metricCategories.Select( a => a.MetricGuid ).First();
            }
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable().Include( a => a.MetricValuePartitions );
            qry = qry.Where( a => a.Metric.Guid == metricGuid && a.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == _campusEntityId && x.EntityId == campusId ) );
            return qry.OrderByDescending( a => a.MetricValueDateTime );
        }


        #endregion

        #region Helper Classes

        [DotLiquid.LiquidType( "LastWeek", "FourWeekAverage" )]
        public class MetricWeeklyCategory
        {
            public MetricWeeklyCategory()
            {
                LastWeek = new MetricData();
                FourWeekAverage = new MetricData();
            }

            /// <summary>
            /// Gets or sets the last week metric data.
            /// </summary>
            /// <value>
            /// The last week metric data.
            /// </value>
            public MetricData LastWeek { get; set; }

            /// <summary>
            /// Gets or sets the four week average metric data.
            /// </summary>
            /// <value>
            /// The four week average metric data.
            /// </value>
            public MetricData FourWeekAverage { get; set; }
        }

        [DotLiquid.LiquidType( "LastMonth", "CurrentMonth" )]
        public class MetricWMonthlyCategory
        {
            public MetricWMonthlyCategory()
            {
                LastMonth = new MetricData();
                CurrentMonth = new MetricData();
            }

            /// <summary>
            /// Gets or sets the last month metric data.
            /// </summary>
            /// <value>
            /// The last month metric data.
            /// </value>
            public MetricData LastMonth { get; set; }

            /// <summary>
            /// Gets or sets the current month metric data.
            /// </summary>
            /// <value>
            /// The current month metric data.
            /// </value>
            public MetricData CurrentMonth { get; set; }
        }


        /// <summary>
        /// A class to store the metric data
        /// </summary>
        [DotLiquid.LiquidType( "Numerator", "Denominator", "Percentage" )]
        public class MetricData
        {
            /// <summary>
            /// Gets or sets the numerator.
            /// </summary>
            /// <value>
            /// The numerator.
            /// </value>
            public decimal Numerator { get; set; }

            /// <summary>
            /// Gets or sets the denominator.
            /// </summary>
            /// <value>
            /// The denominator.
            /// </value>
            public decimal Denominator { get; set; }

            /// <summary>
            /// Gets or sets the percentage.
            /// </summary>
            /// <value>
            /// The percentage.
            /// </value>
            public decimal Percentage
            {
                get
                {
                    if ( Denominator == 0 )
                    {
                        return 0;
                    }
                    else
                    {
                        return Numerator / Denominator * 100;
                    }
                }
            }
        }

        #endregion

    }
}