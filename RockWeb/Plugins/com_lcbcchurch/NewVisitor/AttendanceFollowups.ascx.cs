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
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Attendance Follow-ups" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "Attendance Follow-ups using metrics." )]

    #region Block Attributes
    [BooleanField( "Show Total Column",
        Description = "Determines if the total column is shown.",
        IsRequired = false,
        Key = AttributeKeys.ShowTotalColumn,
        Order = 0 )]
    [MetricCategoriesField( "Welcome Email Fulfilled",
        Description = "Select the metric for Welcome Email Fulfilled.",
        IsRequired = false,
        Key = AttributeKeys.WelcomeEmailFulfilled,
        Order = 1 )]
    [MetricCategoriesField( "Welcome Email Qualified",
        Description = "Select the metric for Welcome Email Qualified.",
        IsRequired = false,
        Key = AttributeKeys.WelcomeEmailQualified,
        Order = 2 )]
    [MetricCategoriesField( "Welcome Card Fulfilled",
        Description = "Select the metric for Welcome Card Fulfilled.",
        IsRequired = false,
        Key = AttributeKeys.WelcomeCardFulfilled,
        Order = 3 )]
    [MetricCategoriesField( "Welcome Card Qualified",
        Description = "Select the metric for Welcome Card Qualified.",
        IsRequired = false,
        Key = AttributeKeys.WelcomeCardQualified,
        Order = 4 )]
    [MetricCategoriesField( "No Return 2nd Week Fulfilled",
        Description = "Select the metric for No Return 2nd Week Fulfilled.",
        IsRequired = false,
        Key = AttributeKeys.NoReturn2ndWeekFulfilled,
        Order = 5 )]
    [MetricCategoriesField( "No Return 2nd Week Qualified",
        Description = "Select the metric for No Return 2nd Week Qualified.",
        IsRequired = false,
        Key = AttributeKeys.NoReturn2ndWeekQualified,
        Order = 6 )]
    [MetricCategoriesField( "Cookie Drop Fulfilled",
        Description = "Select the metric for Cookie Drop Fulfilled.",
        IsRequired = false,
        Key = AttributeKeys.CookieDropFulfilled,
        Order = 7 )]
    [MetricCategoriesField( "Cookie Drop Qualified",
        Description = "Select the metric for Cookie Drop Qualified.",
        IsRequired = false,
        Key = AttributeKeys.CookieDropQualified,
        Order = 8 )]
    [MetricCategoriesField( "Serve Card Fulfilled",
        Description = "Select the metric for Serve Card Fulfilled.",
        IsRequired = false,
        Key = AttributeKeys.ServeCardFulfilled,
        Order = 9 )]
    [MetricCategoriesField( "Serve Card Qualified",
        Description = "Select the metric for Serve Card Qualified.",
        IsRequired = false,
        Key = AttributeKeys.ServeCardQualified,
        Order = 10 )]
    [KeyValueListField( "Score Colors",
        description: "The Key is the minimum score and the Value is the color.",
        keyPrompt: "Minimum Score",
        valuePrompt: "label css class",
        IsRequired = true,
        Order = 11,
        Key = AttributeKeys.ScoreColors )]
    # endregion Block Attributes
    public partial class AttendanceFollowups : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the score colors.
        /// </summary>
        /// <value>
        /// The score colors.
        /// </value>
        private Dictionary<int, string> ScoreColors { get; set; }

        #endregion

        #region Fields

        private int _campusEntityId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;

        #endregion

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string ShowTotalColumn = "ShowTotalColumn";
            public const string WelcomeEmailFulfilled = "WelcomeEmailFulfilled";
            public const string WelcomeEmailQualified = "WelcomeEmailQualified ";
            public const string WelcomeCardFulfilled = "WelcomeCardFulfilled";
            public const string WelcomeCardQualified = "WelcomeCardQualified ";
            public const string NoReturn2ndWeekFulfilled = "NoReturn2ndWeekFulfilled";
            public const string NoReturn2ndWeekQualified = "NoReturn2ndWeekQualified";
            public const string CookieDropFulfilled = "CookieDropFulfilled";
            public const string CookieDropQualified = "CookieDropQualified ";
            public const string ServeCardQualified = "ServeCardQualified";
            public const string ServeCardFulfilled = "ServeCardFulfilled";
            public const string ScoreColors = "ScoreColors ";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState["ScoreColors"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ScoreColors = new Dictionary<int, string>();
            }
            else
            {
                ScoreColors = JsonConvert.DeserializeObject<Dictionary<int, string>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAttendances.DataKeyNames = new string[] { "CampusId" };
            gAttendances.GridRebind += gAttendances_GridRebind;
            var totalField = gAttendances.ColumnsOfType<RockTemplateField>().FirstOrDefault( a => a.HeaderText == "Total" );
            if ( totalField != null )
            {
                totalField.Visible = GetAttributeValue( AttributeKeys.ShowTotalColumn ).AsBoolean();
            }

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
                BindGrid();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ScoreColors"] = ScoreColors.ToJson();

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the CheckedChanged event of the tglMetricType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglMetricType_CheckedChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gAttendances_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Gets the metric percentage HTML include bootstrap label
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        /// <returns></returns>
        public string GetMetricColumnHtml( decimal numerator, decimal denominator )
        {
            string css = string.Empty;

            decimal percentage = 100;
            if ( denominator > 0 )
            {
                percentage = Math.Round( numerator / denominator * 100, 1 );
            }

            if ( ScoreColors.Keys.Any() )
            {
                foreach ( var item in ScoreColors.OrderByDescending( a => a.Key ) )
                {
                    if ( item.Key <= percentage )
                    {
                        css = item.Value;
                        break;
                    }
                }
            }


            return string.Format( "<span class='label {0} grid-label'> {1}% <span style='display: none;'> / {2}</span></span>", css, percentage.ToString( "0.#" ), Convert.ToInt32( denominator ) );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            ScoreColors = GetKeyIconValues( AttributeKeys.ScoreColors );

            var campuses = CampusCache.All();

            List<Guid> metricGuids = new List<Guid>();
            var welcomeEmailFulfilledGuid = GetMetricGuidFromAttribute( AttributeKeys.WelcomeEmailFulfilled );
            AddToMetricGuidList( metricGuids, welcomeEmailFulfilledGuid );

            var welcomeEmailQualifiedGuid = GetMetricGuidFromAttribute( AttributeKeys.WelcomeEmailQualified );
            AddToMetricGuidList( metricGuids, welcomeEmailQualifiedGuid );

            var welcomeCardFulfilledGuid = GetMetricGuidFromAttribute( AttributeKeys.WelcomeCardFulfilled );
            AddToMetricGuidList( metricGuids, welcomeCardFulfilledGuid );

            var welcomeCardQualifiedGuid = GetMetricGuidFromAttribute( AttributeKeys.WelcomeCardQualified );
            AddToMetricGuidList( metricGuids, welcomeCardQualifiedGuid );

            var noReturn2ndWeekFulfilledGuid = GetMetricGuidFromAttribute( AttributeKeys.NoReturn2ndWeekFulfilled );
            AddToMetricGuidList( metricGuids, noReturn2ndWeekFulfilledGuid );

            var noReturn2ndWeekQualifiedGuid = GetMetricGuidFromAttribute( AttributeKeys.NoReturn2ndWeekQualified );
            AddToMetricGuidList( metricGuids, noReturn2ndWeekQualifiedGuid );

            var cookieDropFulfilledGuid = GetMetricGuidFromAttribute( AttributeKeys.CookieDropFulfilled );
            AddToMetricGuidList( metricGuids, cookieDropFulfilledGuid );

            var cookieDropQualifiedGuid = GetMetricGuidFromAttribute( AttributeKeys.CookieDropQualified );
            AddToMetricGuidList( metricGuids, cookieDropQualifiedGuid );

            var serveCardFulfilledGuid = GetMetricGuidFromAttribute( AttributeKeys.ServeCardFulfilled );
            AddToMetricGuidList( metricGuids, serveCardFulfilledGuid );

            var serveCardQualifiedGuid = GetMetricGuidFromAttribute( AttributeKeys.ServeCardQualified );
            AddToMetricGuidList( metricGuids, serveCardQualifiedGuid );

            var rockContext = new RockContext();
            List<MetricData> metricDatas = new List<MetricData>();
            foreach ( var campus in campuses )
            {
                Dictionary<Guid, decimal> metricGuidValuePairs;
                if ( !tglMetricType.Checked )
                {
                    metricGuidValuePairs = Get4WeekMetricValues( metricGuids, campus.Id, rockContext );
                }
                else
                {
                    metricGuidValuePairs = GetLastWeekMetricValues( metricGuids, campus.Id, rockContext );
                }

                MetricData metricData = new MetricData()
                {
                    CampusId = campus.Id,
                    CampusName = campus.Name
                };

                metricData.WelcomeEmailFulfilled = GetMetricValue( welcomeEmailFulfilledGuid, metricGuidValuePairs );
                metricData.WelcomeEmailQualified = GetMetricValue( welcomeEmailQualifiedGuid, metricGuidValuePairs );
                metricData.WelcomeCardFulfilled = GetMetricValue( welcomeCardFulfilledGuid, metricGuidValuePairs );
                metricData.WelcomeCardQualified = GetMetricValue( welcomeCardQualifiedGuid, metricGuidValuePairs );
                metricData.NoReturn2ndWeekFulfilled = GetMetricValue( noReturn2ndWeekFulfilledGuid, metricGuidValuePairs );
                metricData.NoReturn2ndWeekQualified = GetMetricValue( noReturn2ndWeekQualifiedGuid, metricGuidValuePairs );
                metricData.CookieDropFulfilled = GetMetricValue( cookieDropFulfilledGuid, metricGuidValuePairs );
                metricData.CookieDropQualified = GetMetricValue( cookieDropQualifiedGuid, metricGuidValuePairs );
                metricData.ServeCardFulfilled = GetMetricValue( serveCardFulfilledGuid, metricGuidValuePairs );
                metricData.ServeCardQualified = GetMetricValue( serveCardQualifiedGuid, metricGuidValuePairs );
                metricDatas.Add( metricData );
            }

            SortProperty sortProperty = gAttendances.SortProperty;
            if ( sortProperty != null )
            {
                gAttendances.DataSource = metricDatas.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gAttendances.DataSource = metricDatas
                    .OrderBy( r => r.CampusName )
                    .ToList();
            }
            gAttendances.DataBind();
        }

        private decimal GetMetricValue( Guid? welcomeEmailFulfilledGuid, Dictionary<Guid, decimal> metricGuidValuePairs )
        {
            decimal value = 0;
            if ( welcomeEmailFulfilledGuid.HasValue && metricGuidValuePairs.ContainsKey( welcomeEmailFulfilledGuid.Value ) )
            {
                value = metricGuidValuePairs[welcomeEmailFulfilledGuid.Value];
            }
            return value;
        }

        private void AddToMetricGuidList( List<Guid> metricGuids, Guid? metricGuid )
        {
            if ( metricGuid.HasValue )
            {
                metricGuids.Add( metricGuid.Value );
            }
        }

        private Guid? GetMetricGuidFromAttribute( string attributeKey )
        {
            var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( attributeKey ) );
            Guid? metricGuid = null;
            if ( metricCategories.Any() )
            {
                metricGuid = metricCategories.Select( a => a.MetricGuid ).First();
            }
            return metricGuid;
        }

        private Dictionary<Guid, decimal> Get4WeekMetricValues( List<Guid> metricGuids, int campusId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable().Include( a => a.MetricValuePartitions );
            qry = qry.Where( a => metricGuids.Contains( a.Metric.Guid ) && a.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == _campusEntityId && x.EntityId == campusId ) );

            return qry.GroupBy( a => a.Metric.Guid ).Select( a => new
            {
                Guid = a.Key,
                Value = a.OrderByDescending( b => b.MetricValueDateTime ).Skip( 1 ).Take( 4 ).Where( b => b.YValue.HasValue ).Select( b => b.YValue.Value ).DefaultIfEmpty( 0 ).Sum()
            } ).ToDictionary( a => a.Guid, b => b.Value );
        }

        private Dictionary<Guid, decimal> GetLastWeekMetricValues( List<Guid> metricGuids, int campusId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable().Include( a => a.MetricValuePartitions );
            qry = qry.Where( a => metricGuids.Contains( a.Metric.Guid ) && a.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == _campusEntityId && x.EntityId == campusId ) );

            return qry.GroupBy( a => a.Metric.Guid ).Select( a => new
            {
                Guid = a.Key,
                Value = a.OrderByDescending( b => b.MetricValueDateTime ).Take( 1 ).Where( b => b.YValue.HasValue ).Select( b => b.YValue.Value ).DefaultIfEmpty( 0 ).Sum()
            } ).ToDictionary( a => a.Guid, b => b.Value );
        }

        private Dictionary<int, string> GetKeyIconValues( string attributeKey )
        {
            var keyIconValues = new Dictionary<int, string>();
            var keyIconValuesString = GetAttributeValue( attributeKey );
            if ( !string.IsNullOrWhiteSpace( keyIconValuesString ) )
            {
                keyIconValuesString = keyIconValuesString.TrimEnd( '|' );
                foreach ( var keyVal in keyIconValuesString.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Where( s => s.Length == 2 ) )
                {
                    keyIconValues.AddOrIgnore( keyVal[0].AsInteger(), keyVal[1] );
                }
            }

            return keyIconValues;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store the metric data
        /// </summary>
        public class MetricData
        {
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int CampusId { get; set; }

            /// <summary>
            /// Gets or sets the campus name.
            /// </summary>
            /// <value>
            /// The campus name.
            /// </value>
            public string CampusName { get; set; }

            /// <summary>
            /// Gets or sets the welcome email fulfilled.
            /// </summary>
            /// <value>
            /// The welcome email fulfilled.
            /// </value>
            public decimal WelcomeEmailFulfilled { get; set; }

            /// <summary>
            /// Gets or sets the welcome email qualified.
            /// </summary>
            /// <value>
            /// The welcome email qualified.
            /// </value>
            public decimal WelcomeEmailQualified { get; set; }

            /// <summary>
            /// Gets or sets the welcome card fulfilled.
            /// </summary>
            /// <value>
            /// The welcome card fulfilled.
            /// </value>
            public decimal WelcomeCardFulfilled { get; set; }

            /// <summary>
            /// Gets or sets the welcome card qualified.
            /// </summary>
            /// <value>
            /// The welcome card qualified.
            /// </value>
            public decimal WelcomeCardQualified { get; set; }

            /// <summary>
            /// Gets or sets the no return 2nd week fulfilled.
            /// </summary>
            /// <value>
            /// The no return 2nd week fulfilled.
            /// </value>
            public decimal NoReturn2ndWeekFulfilled { get; set; }

            /// <summary>
            /// Gets or sets the no return 2nd week qualified.
            /// </summary>
            /// <value>
            /// The no return 2nd week qualified.
            /// </value>
            public decimal NoReturn2ndWeekQualified { get; set; }

            /// <summary>
            /// Gets or sets the cookie drop fulfilled.
            /// </summary>
            /// <value>
            /// The cookie drop fulfilled.
            /// </value>
            public decimal CookieDropFulfilled { get; set; }

            /// <summary>
            /// Gets or sets the cookie drop qualified.
            /// </summary>
            /// <value>
            /// The cookie drop qualified.
            /// </value>
            public decimal CookieDropQualified { get; set; }

            /// <summary>
            /// Gets or sets the serve card fulfilled.
            /// </summary>
            /// <value>
            /// The serve card fulfilled.
            /// </value>
            public decimal ServeCardFulfilled { get; set; }

            /// <summary>
            /// Gets or sets the serve card qualified.
            /// </summary>
            /// <value>
            /// The serve card qualified.
            /// </value>
            public decimal ServeCardQualified { get; set; }

            /// <summary>
            /// Gets or sets the total fulfilled.
            /// </summary>
            /// <value>
            /// The total fulfilled.
            /// </value>
            public decimal TotalFulfilled
            {
                get { return WelcomeEmailFulfilled + WelcomeCardFulfilled + NoReturn2ndWeekFulfilled + CookieDropFulfilled + ServeCardFulfilled; }
            }

            /// <summary>
            /// Gets or sets the total qualified.
            /// </summary>
            /// <value>
            /// The total qualified.
            /// </value>
            public decimal TotalQualified
            {
                get { return WelcomeEmailQualified + WelcomeCardQualified + NoReturn2ndWeekQualified + CookieDropQualified + ServeCardQualified; }
            }
        }

        #endregion

    }
}