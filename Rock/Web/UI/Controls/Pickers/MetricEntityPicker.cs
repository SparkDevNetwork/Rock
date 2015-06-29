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
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class MetricEntityPicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Custom implementation)

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _mpMetricCategory.Required;
            }

            set
            {
                EnsureChildControls();
                _mpMetricCategory.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private MetricCategoryPicker _mpMetricCategory;
        private PlaceHolder _phEntityTypeEntityIdValue;
        private Control _entityTypeEditControl;
        private RockRadioButtonList _rblSelectOrContext;
        private RockCheckBox _cbCombine;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        public int? MetricCategoryId
        {
            get
            {
                EnsureChildControls();
                var metricCategoryId = _mpMetricCategory.SelectedValue.AsIntegerOrNull();
                return metricCategoryId;
            }

            set
            {
                EnsureChildControls();
                _mpMetricCategory.SetValue( value );
                mpMetric_SelectItem( null, null );
            }
        }

        /// <summary>
        /// Gets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        public int? MetricId
        {
            get
            {
                var metricCategory = new MetricCategoryService( new RockContext() ).Get( this.MetricCategoryId ?? 0 );
                return metricCategory != null ? metricCategory.MetricId : (int?)null;
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the _mpMetric control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mpMetric_SelectItem( object sender, EventArgs e )
        {
            // figure out which picker to render based on the Metric's Entity
            UpdateEntityTypeControls();
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId
        {
            get
            {
                EnsureChildControls();

                if ( _entityTypeEditControl == null )
                {
                    return null;
                }

                var metric = new MetricService( new RockContext() ).Get( this.MetricId ?? 0 );
                int? result = null;

                var metricEntityType = EntityTypeCache.Read( metric.EntityTypeId ?? 0 );
                if ( metricEntityType != null && metricEntityType.SingleValueFieldType != null && metricEntityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    result = ( metricEntityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId( _entityTypeEditControl, new Dictionary<string, ConfigurationValue>() );
                }

                return result;
            }

            set
            {
                EnsureChildControls();

                if ( _entityTypeEditControl == null )
                {
                    return;
                }

                var metric = new MetricService( new RockContext() ).Get( this.MetricId ?? 0 );

                var metricEntityType = EntityTypeCache.Read( metric.EntityTypeId ?? 0 );
                if ( metricEntityType != null && metricEntityType.SingleValueFieldType != null && metricEntityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    ( metricEntityType.SingleValueFieldType.Field as IEntityFieldType ).SetEditValueFromEntityId( _entityTypeEditControl, new Dictionary<string, ConfigurationValue>(), value );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [get entity from context].
        /// </summary>
        /// <value>
        /// <c>true</c> if [get entity from context]; otherwise, <c>false</c>.
        /// </value>
        public bool GetEntityFromContext
        {
            get
            {
                EnsureChildControls();
                if ( _rblSelectOrContext != null )
                {
                    return _rblSelectOrContext.SelectedValue.AsIntegerOrNull() == 1;
                }
                else
                {
                    return true;
                }
            }

            set
            {
                EnsureChildControls();
                if ( _entityTypeEditControl != null )
                {
                    _entityTypeEditControl.Visible = !value;
                }

                if ( _rblSelectOrContext != null )
                {
                    _rblSelectOrContext.SelectedValue = value ? "1" : "0";
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [combine values].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [combine values]; otherwise, <c>false</c>.
        /// </value>
        public bool CombineValues
        {
            get
            {
                EnsureChildControls();
                if ( _cbCombine != null )
                {
                    return _cbCombine.Checked;
                }
                else
                {
                    return true;
                }
            }

            set
            {
                EnsureChildControls();
                if ( _cbCombine != null )
                {
                    _cbCombine.Checked = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            UpdateEntityTypeControls();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricEntityPicker"/> class.
        /// </summary>
        public MetricEntityPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _mpMetricCategory = new MetricCategoryPicker();
            _mpMetricCategory.AllowMultiSelect = false;
            _mpMetricCategory.ID = this.ID + "_mpMetric";
            _mpMetricCategory.SelectItem += mpMetric_SelectItem;
            Controls.Add( _mpMetricCategory );

            _phEntityTypeEntityIdValue = new PlaceHolder();
            _phEntityTypeEntityIdValue.ID = this.ID + "_phEntityTypeEntityIdValue";
            _phEntityTypeEntityIdValue.EnableViewState = false;
            Controls.Add( _phEntityTypeEntityIdValue );

            // figure out which picker to render based on the Metric's Entity
            UpdateEntityTypeControls();
        }

        /// <summary>
        /// Updates the entity type controls.
        /// </summary>
        private void UpdateEntityTypeControls()
        {
            var metricService = new MetricService( new RockContext() );
            var metric = metricService.Get( this.MetricId ?? 0 );
            _phEntityTypeEntityIdValue.Controls.Clear();

            string fieldTypeName = null;
            Control entityTypeEditControl = null;
            if ( metric != null )
            {
                var entityType = EntityTypeCache.Read( metric.EntityTypeId ?? 0 );
                if ( entityType != null && entityType.SingleValueFieldType != null )
                {
                    fieldTypeName = entityType.SingleValueFieldType.Name;
                    entityTypeEditControl = entityType.SingleValueFieldType.Field.EditControl( new Dictionary<string, Field.ConfigurationValue>(), string.Format( "{0}_{1}_Picker", this.ID, fieldTypeName ) );
                }
            }

            // only set the _entityTypeEditControl is needs to be
            if ( _entityTypeEditControl == null || !_entityTypeEditControl.GetType().Equals( entityTypeEditControl.GetType() ) || _entityTypeEditControl.ID != entityTypeEditControl.ID )
            {
                _entityTypeEditControl = entityTypeEditControl;
            }

            if ( _entityTypeEditControl != null )
            {
                var rockControlWrapper = new RockControlWrapper();
                rockControlWrapper.Label = string.Format( "{0} filter", fieldTypeName );
                rockControlWrapper.Help = string.Format(
                    "Either select a specific {0}, or select 'Get from page context' to determine the {0} based on the page context. Leave {0} blank to show values for all {1}",
                    fieldTypeName,
                    fieldTypeName.Pluralize() );

                rockControlWrapper.ID = string.Format( "{0}_{1}", this.ID, "rockControlWrapper" );
                _phEntityTypeEntityIdValue.Controls.Add( rockControlWrapper );

                if ( _rblSelectOrContext == null )
                {
                    _rblSelectOrContext = new RockRadioButtonList();
                    _rblSelectOrContext.ID = string.Format( "{0}_{1}", this.ID, "rblSelectOrContext" );
                    _rblSelectOrContext.RepeatDirection = RepeatDirection.Horizontal;
                    _rblSelectOrContext.Items.Add( new ListItem( "Select " + fieldTypeName, "0" ) );
                    _rblSelectOrContext.Items.Add( new ListItem( "Get from page context", "1" ) );
                    _rblSelectOrContext.AutoPostBack = true;
                    _rblSelectOrContext.SelectedIndexChanged += rblSelectOrContext_SelectedIndexChanged;
                }
                else
                {
                    _rblSelectOrContext.Items[0].Text = "Select " + fieldTypeName;
                }

                rockControlWrapper.Controls.Add( _rblSelectOrContext );
                if ( string.IsNullOrEmpty( _rblSelectOrContext.SelectedValue ) )
                {
                    // might have to get the SelectedValue since we re-created the control after Load()
                    _rblSelectOrContext.SelectedValue = this.Page.Request.Params[_rblSelectOrContext.UniqueID];
                }

                if ( string.IsNullOrEmpty( _rblSelectOrContext.SelectedValue ) )
                {
                    // if not set from either what it was, or from PageContext, default it to "Get from page context"
                    _rblSelectOrContext.SelectedValue = "1";
                }

                _entityTypeEditControl.Visible = _rblSelectOrContext.SelectedValue.AsIntegerOrNull() == 0;
                rockControlWrapper.Controls.Add( _entityTypeEditControl );
                if ( _cbCombine == null )
                {
                    _cbCombine = new RockCheckBox();
                    _cbCombine.ID = string.Format( "{0}_{1}", this.ID, "cbCombine" );
                }

                _cbCombine.Text = "Combine multiple values to one line when showing values for multiple " + fieldTypeName.Pluralize();

                rockControlWrapper.Controls.Add( _cbCombine );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            _mpMetricCategory.RenderControl( writer );
            _phEntityTypeEntityIdValue.RenderControl( writer );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblSelectOrContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rblSelectOrContext_SelectedIndexChanged( object sender, EventArgs e )
        {
            // intentionally blank, but we need the postback to fire
        }

        /// <summary>
        /// Gets or sets the DelimitedValues (for the stored Attribute Value)
        /// Value is pipe delimited: Metric (as Guid) | EntityId | GetEntityFromContext | CombineValues
        /// </summary>
        /// <value>
        /// The delimited values.
        /// </value>
        public string DelimitedValues
        {
            get
            {
                var metricGuid = Guid.Empty;
                var categoryGuid = Guid.Empty;
                var rockContext = new RockContext();

                // first try to get metric from MetricCategoryId.  If not found, just get the metric regardless of category
                var metricCategory = new MetricCategoryService( rockContext ).Get( this.MetricCategoryId ?? 0 );
                if ( metricCategory == null )
                {
                    metricCategory = new MetricCategoryService( rockContext ).Queryable().Where( a => a.MetricId == ( this.MetricId ?? 0 ) ).FirstOrDefault();
                }

                if ( metricCategory != null )
                {
                    metricGuid = metricCategory.Metric.Guid;
                    categoryGuid = metricCategory.Category.Guid;
                }

                return string.Format( "{0}|{1}|{2}|{3}|{4}", metricGuid.ToString(), this.EntityId, this.GetEntityFromContext, this.CombineValues, categoryGuid );
            }

            set
            {
                var valueParts = value.Split( '|' );
                if ( valueParts.Length > 0 )
                {

                    Guid metricGuid = valueParts[0].AsGuid();
                    Guid categoryGuid = Guid.Empty;

                    if ( valueParts.Length > 4 )
                    {
                        categoryGuid = valueParts[4].AsGuid();
                    }

                    // first try to get metric from Metric.Guid and Category.Guid.  If not found, just get the metric regardless of category
                    var metricCategoryQry = new MetricCategoryService( new RockContext() ).Queryable();
                    MetricCategory metricCategory = metricCategoryQry.Where( a => a.Metric.Guid == metricGuid && a.Category.Guid == categoryGuid ).FirstOrDefault();
                    if ( metricCategory == null )
                    {
                        metricCategory = metricCategoryQry.Where( a => a.Metric.Guid == metricGuid ).FirstOrDefault();
                    }

                    if ( metricCategory != null )
                    {
                        this.MetricCategoryId = metricCategory.Id;
                        if ( valueParts.Length > 3 )
                        {
                            this.EntityId = valueParts[1].AsIntegerOrNull();
                            this.GetEntityFromContext = valueParts[2].AsBoolean( false );
                            this.CombineValues = valueParts[3].AsBoolean( false );
                        }
                    }
                }
            }
        }
    }
}