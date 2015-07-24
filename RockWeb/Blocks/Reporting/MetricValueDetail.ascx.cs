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
                FieldTypeCache fieldType = FieldTypeCache.Read( hfSingleValueFieldTypeId.Value.AsInteger() );
                if ( fieldType != null )
                {
                    var entityTypeEditControl = fieldType.Field.EditControl( new Dictionary<string, Rock.Field.ConfigurationValue>(), "entityTypeEditControl" );
                    phEntityTypeEntityIdValue.Controls.Add( entityTypeEditControl );
                    if ( entityTypeEditControl is IRockControl )
                    {
                        ( entityTypeEditControl as IRockControl ).Label = fieldType.Name;
                    }
                }
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
                metricValue.Metric = metricValue.Metric ?? new MetricService(rockContext).Get(metricValue.MetricId);
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
            var metricEntityType = EntityTypeCache.Read( metricValue.Metric.EntityTypeId ?? 0 );
            Control entityTypeEditControl = phEntityTypeEntityIdValue.FindControl("entityTypeEditControl");
            if ( metricEntityType != null && metricEntityType.SingleValueFieldType != null && metricEntityType.SingleValueFieldType.Field is IEntityFieldType )
            {
                metricValue.EntityId = ( metricEntityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>() );
            }
            else
            {
                metricValue.EntityId = null;
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
            }

            if ( metricValue == null && metricId.HasValue )
            {
                metricValue = new MetricValue { Id = 0, MetricId = metricId.Value };
                metricValue.Metric = metricValue.Metric ?? new MetricService( new RockContext() ).Get( metricValue.MetricId );
                lActionTitle.Text = ActionTitle.Add( MetricValue.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            hfMetricValueId.Value = metricValue.Id.ToString();

            LoadDropDowns();

            ddlMetricValueType.SelectedValue = metricValue.MetricValueType.ConvertToInt().ToString();
            tbXValue.Text = metricValue.XValue;
            tbYValue.Text = metricValue.YValue.ToString();
            hfMetricId.Value = metricValue.MetricId.ToString();
            tbNote.Text = metricValue.Note;
            dpMetricValueDateTime.SelectedDate = metricValue.MetricValueDateTime;

            var metricEntityType = EntityTypeCache.Read( metricValue.Metric.EntityTypeId ?? 0 );

            // Setup EntityType UI controls
            Control entityTypeEditControl = null;
            if ( metricEntityType != null && metricEntityType.SingleValueFieldType != null )
            {
                hfSingleValueFieldTypeId.Value = metricEntityType.SingleValueFieldType.Id.ToString();
                FieldTypeCache fieldType = FieldTypeCache.Read( hfSingleValueFieldTypeId.Value.AsInteger() );
                entityTypeEditControl = fieldType.Field.EditControl( new Dictionary<string, Rock.Field.ConfigurationValue>(), "entityTypeEditControl" );

                if ( entityTypeEditControl is IRockControl )
                {
                    ( entityTypeEditControl as IRockControl ).Label = fieldType.Name;
                }

                phEntityTypeEntityIdValue.Controls.Add( entityTypeEditControl );
                IEntityFieldType entityFieldType = metricEntityType.SingleValueFieldType.Field as IEntityFieldType;
                if ( entityFieldType != null )
                {
                    entityFieldType.SetEditValueFromEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>(), metricValue.EntityId );
                }
            }

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MetricValue.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( MetricValue.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            ddlMetricValueType.Enabled = !readOnly;
            tbXValue.ReadOnly = readOnly;
            tbYValue.ReadOnly = readOnly;
            tbNote.ReadOnly = readOnly;
            dpMetricValueDateTime.Enabled = !readOnly;
            if ( entityTypeEditControl is WebControl )
            {
                ( entityTypeEditControl as WebControl ).Enabled = !readOnly;
            }

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