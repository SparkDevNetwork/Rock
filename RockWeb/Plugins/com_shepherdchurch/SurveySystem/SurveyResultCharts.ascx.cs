using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.shepherdchurch.SurveySystem.Model;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_shepherdchurch.SurveySystem
{
    [DisplayName( "Survey Result Charts" )]
    [Category( "Shepherd Church > Survey System" )]
    [Description( "Shows survey results with charts." )]

    [IntegerField(
        name: "Max Values Per Question",
        Description = "The maximum number of values to display for a single question.",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Key = "MaxValuesPerQuestion",
        Order = 0 )]
    public partial class SurveyResultCharts : RockBlock
    {
        private readonly int[] _customValueFieldTypeIds = new[]
        {
            FieldTypeCache.GetId( Rock.SystemGuid.FieldType.MULTI_SELECT.AsGuid() ).Value,
            FieldTypeCache.GetId( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ).Value
        };

        /// <summary>
        /// Gets or sets the attribute filters.
        /// </summary>
        /// <value>
        /// The attribute filters.
        /// </value>
        public List<AttributeCache> AttributeFilters { get; set; }

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var attributeIds = ViewState["AttributeFilters"] as List<int>;
            AttributeFilters = attributeIds
                .Select( a => AttributeCache.Get( a ) )
                .Where( a => a != null )
                .ToList();

            BuildDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js", false );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                BindAttributes();
                BuildDynamicControls();

                ShowDetails();
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
            ViewState["AttributeFilters"] = AttributeFilters.Select( a => a.Id ).ToList();

            return base.SaveViewState();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Show the details of the survey results.
        /// </summary>
        private void ShowDetails()
        {
            var rockContext = new RockContext();
            var surveyResultService = new SurveyResultService( rockContext );
            int surveyId = PageParameter( "SurveyId" ).AsInteger();

            var survey = new SurveyService( rockContext ).Get( surveyId );

            if ( survey == null || !survey.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbUnauthorizedMessage.Text = EditModeMessage.NotAuthorizedToView( Survey.FriendlyTypeName );
                pnlResults.Visible = false;

                return;
            }

            ltTitle.Text = string.Format( "{0} Results", survey.Name );

            var qry = surveyResultService.Queryable()
                .Where( r => r.SurveyId == surveyId );

            //
            // Date Completed Range Filter.
            //
            if ( drpDateCompleted.LowerValue.HasValue )
            {
                qry = qry.Where( p => p.CreatedDateTime >= drpDateCompleted.LowerValue.Value );
            }
            if ( drpDateCompleted.UpperValue.HasValue )
            {
                DateTime upperDate = drpDateCompleted.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( p => p.CreatedDateTime < upperDate );
            }

            // Filter query by any configured attribute filters
            var preAttributeQry = qry;
            if ( AttributeFilters != null && AttributeFilters.Any() )
            {
                foreach ( var attribute in AttributeFilters )
                {
                    var filterControl = phFilterControls.FindControl( "filter_" + attribute.Id.ToString() );
                    var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( filterValues.Count > 0 && filterValues[filterValues.Count >= 2 ? 1 : 0].IsNotNullOrWhiteSpace() )
                    {
                        qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, surveyResultService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }
            }
            pnlFilter.CssClass = qry != preAttributeQry ? "panel panel-info" : "panel panel-widget";

            //
            // Store the queried objects in the grid for it to use later.
            //
            var results = qry.ToList();
            results.LoadAttributes( rockContext );

            var tempResult = new SurveyResult
            {
                SurveyId = surveyId
            };
            tempResult.LoadAttributes( rockContext );
            var attributes = tempResult.Attributes
                .Values
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Key )
                .ToList();

            var answers = new Dictionary<string, string>();
            if ( survey.PassingGrade.HasValue )
            {
                answers = survey.AnswerData.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            }

            var data = new List<object>();
            foreach ( var attribute in attributes )
            {
                var answer = answers.GetValueOrDefault( attribute.Key, string.Empty );

                var resultAnswers = results.SelectMany( a => GetDisplayValues( a, attribute ) );

                var values = resultAnswers.GroupBy( a => a )
                    .ToDictionary( a => a.Key, a => a.Count() );

                double? passRate = null;

                if ( survey.PassingGrade.HasValue && results.Count > 0 )
                {
                    passRate = resultAnswers.Count( a => a == answer ) / ( double ) results.Count;
                }

                data.Add( new
                {
                    title = attribute.Name,
                    values,
                    passRate
                } );
            }

            hfData.Value = data.ToJson();
            hfMaxValues.Value = GetAttributeValue( "MaxValuesPerQuestion" );
        }

        /// <summary>
        /// Gets the display values.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        private IEnumerable<string> GetDisplayValues( SurveyResult result, AttributeCache attribute )
        {
            var rawValue = result.GetAttributeValue( attribute.Key );

            if ( rawValue.IsNullOrWhiteSpace() )
            {
                return new[] { string.Empty };
            }

            if ( _customValueFieldTypeIds.Contains( attribute.FieldTypeId ) )
            {
                var configuredValues = Rock.Field.Helper.GetConfiguredValues( attribute.QualifierValues );
                var selectedValues = rawValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                return configuredValues
                    .Where( v => selectedValues.Contains( v.Key ) )
                    .Select( v => v.Value )
                    .ToList();
            }
            else if ( attribute.FieldType.Guid == Rock.SystemGuid.FieldType.RATING.AsGuid() )
            {
                return new[] { rawValue };
            }
            else
            {
                return new[] { attribute.FieldType.Field.FormatValue( this, rawValue, attribute.QualifierValues, false ) };
            }
        }

        /// <summary>
        /// Adds the attribute filters.
        /// </summary>
        private void BindAttributes()
        {
            int surveyId = PageParameter( "SurveyId" ).AsInteger();

            var tempResult = new SurveyResult
            {
                SurveyId = surveyId
            };
            tempResult.LoadAttributes();
            AttributeFilters = tempResult.Attributes
                .Values
                .Where( a => a.FieldType.Field.HasFilterControl() )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Key )
                .ToList();
        }

        /// <summary>
        /// Builds the dynamic controls.
        /// </summary>
        private void BuildDynamicControls()
        {
            phFilterControls.Controls.Clear();

            if ( AttributeFilters != null )
            {
                foreach ( var attribute in AttributeFilters )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        AddFilterControl( control, attribute.Name, attribute.Description );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the filter control to the view.
        /// </summary>
        /// <param name="control">The control to be added.</param>
        /// <param name="name">The name of the control.</param>
        /// <param name="description">The help text of the control.</param>
        private void AddFilterControl( Control control, string name, string description )
        {
            if ( control is IRockControl )
            {
                var rockControl = ( IRockControl ) control;
                rockControl.Label = name;
                rockControl.Help = description;
                phFilterControls.Controls.Add( control );
            }
            else
            {
                var wrapper = new RockControlWrapper
                {
                    ID = control.ID + "_wrapper",
                    Label = name
                };
                wrapper.Controls.Add( control );
                phFilterControls.Controls.Add( wrapper );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbApplyFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbApplyFilter_Click( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbClearFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbClearFilter_Click( object sender, EventArgs e )
        {
            drpDateCompleted.DelimitedValues = null;
            BuildDynamicControls();

            ShowDetails();
        }

        #endregion
    }
}