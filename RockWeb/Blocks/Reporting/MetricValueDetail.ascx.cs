﻿// <copyright>
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Metric Value Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of a particular metric value." )]
    public partial class MetricValueDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                // create dynamic controls
                var rockContext = new RockContext();
                var metricValue = new MetricValueService( rockContext ).Get( hfMetricValueId.Value.AsInteger() );
                if ( metricValue == null )
                {
                    metricValue = new MetricValue { MetricId = hfMetricId.Value.AsInteger() };
                    metricValue.Metric = new MetricService( rockContext ).Get( metricValue.MetricId );
                }

                CreateDynamicControls( metricValue, false, false );
            }

            if ( !Page.IsPostBack )
            {
                int? metricValueId = PageParameter( "MetricValueId" ).AsIntegerOrNull();

                // in case called with MetricId as the parent id parameter
                int? metricId = PageParameter( "MetricId" ).AsIntegerOrNull();

                // in case called with MetricCategoryId as the parent id parameter
                int? metricCategoryId = PageParameter( "MetricCategoryId" ).AsIntegerOrNull();
                MetricCategory metricCategory = null;
                if ( metricCategoryId.HasValue )
                {
                    if ( metricCategoryId.Value > 0 )
                    {
                        // editing a metric, but get the metricId from the metricCategory
                        metricCategory = new MetricCategoryService( new RockContext() ).Get( metricCategoryId.Value );
                        if ( metricCategory != null )
                        {
                            metricId = metricCategory.MetricId;
                        }
                    }
                    else
                    {
                        // adding a new metric. Block will (hopefully) not be shown
                        metricId = 0;
                    }
                }

                hfMetricCategoryId.Value = metricCategoryId.ToString();

                if ( metricValueId.HasValue )
                {
                    ShowDetail( metricValueId.Value, metricId );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        /// <param name="metricValue">The metric value.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        private void CreateDynamicControls( MetricValue metricValue, bool setValues, bool readOnly )
        {
            if ( metricValue != null )
            {
                foreach ( var metricPartition in metricValue.Metric.MetricPartitions )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( metricPartition.EntityTypeId.Value );
                        if ( entityTypeCache != null && entityTypeCache.SingleValueFieldType != null )
                        {
                            var fieldType = entityTypeCache.SingleValueFieldType;

                            Dictionary<string, Rock.Field.ConfigurationValue> configurationValues;
                            if ( fieldType.Field is IEntityQualifierFieldType )
                            {
                                configurationValues = ( fieldType.Field as IEntityQualifierFieldType ).GetConfigurationValuesFromEntityQualifier( metricPartition.EntityTypeQualifierColumn, metricPartition.EntityTypeQualifierValue );
                            }
                            else
                            {
                                configurationValues = new Dictionary<string, ConfigurationValue>();
                            }

                            var entityTypeEditControl = fieldType.Field.EditControl( configurationValues, string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id ) );
                            var panelCol4 = new Panel { CssClass = "col-md-4" };
                            if ( entityTypeEditControl != null && entityTypeEditControl is IRockControl)
                            {
                                panelCol4.Controls.Add( entityTypeEditControl );
                                phMetricValuePartitions.Controls.Add( panelCol4 );
                                
                                var entityTypeRockControl = ( entityTypeEditControl as IRockControl );
                                entityTypeRockControl.Label = metricPartition.Label;
                                entityTypeRockControl.Required = metricPartition.IsRequired;
                                if ( entityTypeEditControl is WebControl )
                                {
                                    ( entityTypeEditControl as WebControl ).Enabled = !readOnly;
                                }

                                if ( setValues && metricValue.MetricValuePartitions != null )
                                {
                                    var metricValuePartition = metricValue.MetricValuePartitions.FirstOrDefault( a => a.MetricPartitionId == metricPartition.Id );
                                    if ( metricValuePartition != null )
                                    {
                                        if ( fieldType.Field is IEntityFieldType )
                                        {
                                            ( fieldType.Field as IEntityFieldType ).SetEditValueFromEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>(), metricValuePartition.EntityId );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var errorControl = new LiteralControl();
                                errorControl.Text = string.Format( "<span class='label label-danger'>Unable to create Partition control for {0}. Verify that the metric partition settings are set correctly</span>", metricPartition.Label );
                                phMetricValuePartitions.Controls.Add( errorControl );
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "MetricId", hfMetricId.Value );
            qryParams.Add( "MetricCategoryId", hfMetricCategoryId.Value );
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            MetricValue metricValue;
            var rockContext = new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );

            int metricValueId = int.Parse( hfMetricValueId.Value );

            if ( metricValueId == 0 )
            {
                metricValue = new MetricValue();
                metricValueService.Add( metricValue );
                metricValue.MetricId = hfMetricId.ValueAsInt();
                metricValue.Metric = metricValue.Metric ?? new MetricService( rockContext ).Get( metricValue.MetricId );
                metricValue.MetricValuePartitions = new List<MetricValuePartition>();
            }
            else
            {
                metricValue = metricValueService.Get( metricValueId );
            }

            metricValue.MetricValueType = ddlMetricValueType.SelectedValueAsEnum<MetricValueType>();
            metricValue.XValue = tbXValue.Text;
            metricValue.YValue = tbYValue.Text.AsDecimalOrNull();
            metricValue.Note = tbNote.Text;
            metricValue.MetricValueDateTime = dpMetricValueDateTime.SelectedDate;

            // Get EntityId from EntityType UI controls
            foreach ( var metricPartition in metricValue.Metric.MetricPartitions )
            {
                var metricPartitionEntityType = EntityTypeCache.Get( metricPartition.EntityTypeId ?? 0 );
                var controlId = string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id );
                Control entityTypeEditControl = phMetricValuePartitions.FindControl( controlId );
                var metricValuePartition = metricValue.MetricValuePartitions.FirstOrDefault( a => a.MetricPartitionId == metricPartition.Id );
                if ( metricValuePartition == null )
                {
                    metricValuePartition = new MetricValuePartition();
                    metricValuePartition.MetricPartitionId = metricPartition.Id;
                    metricValue.MetricValuePartitions.Add( metricValuePartition );
                }

                if ( metricPartitionEntityType != null && metricPartitionEntityType.SingleValueFieldType != null && metricPartitionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    metricValuePartition.EntityId = ( metricPartitionEntityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>() );
                }
                else
                {
                    metricValuePartition.EntityId = null;
                }

                if ( metricPartition.IsRequired && metricPartitionEntityType != null && !metricValuePartition.EntityId.HasValue )
                {
                    nbValueRequired.Text = string.Format( "A value for {0} is required", metricPartition.Label ?? metricPartitionEntityType.FriendlyName );
                    nbValueRequired.Dismissable = true;
                    nbValueRequired.Visible = true;
                    return;
                }
            }


            if ( !metricValue.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "MetricId", hfMetricId.Value );
            qryParams.Add( "MetricCategoryId", hfMetricCategoryId.Value );
            qryParams.Add( "ExpandedIds", PageParameter( "ExpandedIds" ) );
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="metricValueId">The metric value identifier.</param>
        public void ShowDetail( int metricValueId )
        {
            ShowDetail( metricValueId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="metricValueId">The metric value identifier.</param>
        /// <param name="metricId">The metric identifier.</param>
        public void ShowDetail( int metricValueId, int? metricId )
        {
            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            MetricValue metricValue = null;
            if ( !metricValueId.Equals( 0 ) )
            {
                metricValue = new MetricValueService( new RockContext() ).Get( metricValueId );
                lActionTitle.Text = ActionTitle.Edit( MetricValue.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( metricValue, ResolveRockUrl( "~" ) );
            }

            if ( metricValue == null && metricId.HasValue )
            {
                metricValue = new MetricValue { Id = 0, MetricId = metricId.Value };
                metricValue.Metric = metricValue.Metric ?? new MetricService( new RockContext() ).Get( metricValue.MetricId );
                lActionTitle.Text = ActionTitle.Add( MetricValue.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfMetricValueId.Value = metricValue.Id.ToString();

            LoadDropDowns();

            ddlMetricValueType.SelectedValue = metricValue.MetricValueType.ConvertToInt().ToString();
            tbXValue.Text = metricValue.XValue;
            tbYValue.Text = metricValue.YValue.ToString();
            hfMetricId.Value = metricValue.MetricId.ToString();
            tbNote.Text = metricValue.Note;
            dpMetricValueDateTime.SelectedDate = metricValue.MetricValueDateTime;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            bool canEdit = UserCanEdit;
            if ( !canEdit && metricId.HasValue && metricId.Value > 0 )
            {
                var metric = new MetricService( new RockContext() ).Get( metricId.Value );
                if ( metric != null && metric.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) 
                {
                    canEdit = true;
                }
            }

            if ( !canEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MetricValue.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( MetricValue.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            CreateDynamicControls( metricValue, true, readOnly );

            ddlMetricValueType.Enabled = !readOnly;
            tbXValue.ReadOnly = readOnly;
            tbYValue.ReadOnly = readOnly;
            tbNote.ReadOnly = readOnly;
            dpMetricValueDateTime.Enabled = !readOnly;

            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlMetricValueType.Items.Clear();
            foreach ( var item in Enum.GetValues( typeof( MetricValueType ) ).OfType<MetricValueType>().OrderBy( a => a.ConvertToString() ) )
            {
                ddlMetricValueType.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
            }
        }

        #endregion
    }
}