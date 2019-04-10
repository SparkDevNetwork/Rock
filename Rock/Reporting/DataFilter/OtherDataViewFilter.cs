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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Filter entities using another dataview
    /// </summary>
    [Description( "Filter entities using another dataview" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Other Data View Filter" )]
    public class OtherDataViewFilter : DataFilterComponent, IDataFilterWithOverrides
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return int.MaxValue;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public override string GetTitle( Type entityType )
        {
            return "Existing Data View";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            var title = "Included In Data View:";
            return $@"Rock.reporting.formatFilterForOtherDataViewFilter('{title}', $content)";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Another Data View";

            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig.DataViewId.HasValue )
            {
                var dataView = new DataViewService( new RockContext() ).Get( selectionConfig.DataViewId.Value );
                if ( dataView != null )
                {
                    return string.Format( "Included in '{0}' Data View", dataView.Name );
                }
            }

            return s;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            int entityTypeId = EntityTypeCache.Get( entityType ).Id;

            var dvpDataView = new DataViewItemPicker();
            dvpDataView.ID = filterControl.ID + "_dvpDataView";
            dvpDataView.CssClass = "js-dataview";
            dvpDataView.SelectItem += DvpDataView_SelectItem;
            filterControl.Controls.Add( dvpDataView );
            dvpDataView.EntityTypeId = entityTypeId;

            var cbUsePersisted = new RockCheckBox();
            cbUsePersisted.Text = "Use Persisted";
            cbUsePersisted.ID = filterControl.ID + "_cbUsePersisted";
            cbUsePersisted.CssClass = "js-usepersisted";
            cbUsePersisted.Checked = true;
            filterControl.Controls.Add( cbUsePersisted );

            return new Control[2] { dvpDataView, cbUsePersisted };
        }

        /// <summary>
        /// Handles the SelectItem event of the DvpDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DvpDataView_SelectItem( object sender, EventArgs e )
        {
            // if selecting another dataview, default the cbUsePersisted to True
            Control control = sender as Control;
            FilterField filterField = control.FirstParentControlOfType<FilterField>();
            var cbUsePersisted = filterField?.ControlsOfTypeRecursive<RockCheckBox>().Where( a => a.CssClass.Contains( "js-usepersisted" ) ).FirstOrDefault();
            if ( cbUsePersisted != null )
            {
                cbUsePersisted.Checked = true;
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='data-view-filter-label'>Data View</span>" );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            DataViewItemPicker dataViewItemPicker = controls[0] as DataViewItemPicker;
            dataViewItemPicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            RockCheckBox cbUsePersisted = controls[1] as RockCheckBox;
            cbUsePersisted.Visible = false;
            int? dataViewId = dataViewItemPicker.SelectedValueAsId();
            if ( dataViewId.HasValue )
            {
                int? persistedScheduleIntervalMinutes = new DataViewService( new RockContext() ).GetSelect( dataViewId.Value, s => s.PersistedScheduleIntervalMinutes );
                cbUsePersisted.Visible = persistedScheduleIntervalMinutes.HasValue;
            }

            cbUsePersisted.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// 
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Gets or sets the data view identifier.
            /// </summary>
            /// <value>
            /// The data view identifier.
            /// </value>
            public int? DataViewId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [use persisted].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [use persisted]; otherwise, <c>false</c>.
            /// </value>
            public bool UsePersisted { get; set; }

            /// <summary>
            /// Parses the specified selection.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();
                if ( selectionConfig == null )
                {
                    // see if selection is just the plain DataViewId as string (from previous version of config format)
                    selectionConfig = new SelectionConfig();
                    selectionConfig.DataViewId = selection.AsIntegerOrNull();
                    selectionConfig.UsePersisted = true;
                }

                return selectionConfig;
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var selectionConfig = new SelectionConfig
            {
                DataViewId = ( ( DataViewItemPicker ) controls[0] ).SelectedValue.AsIntegerOrNull(),
                UsePersisted = ( ( RockCheckBox ) controls[1] ).Checked
            };

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            ( ( DataViewItemPicker ) controls[0] ).SetValue( selectionConfig.DataViewId );
            ( ( RockCheckBox ) controls[1] ).Checked = selectionConfig.UsePersisted;
        }

        /// <summary>
        /// Gets the selected data view.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public DataView GetSelectedDataView( string selection )
        {
            return GetSelectedDataView( SelectionConfig.Parse( selection ) );
        }

        /// <summary>
        /// Gets the selected data view.
        /// </summary>
        /// <param name="selectionConfig">The selection configuration.</param>
        /// <returns></returns>
        private DataView GetSelectedDataView( SelectionConfig selectionConfig )
        {
            if ( selectionConfig.DataViewId.HasValue )
            {
                var dataView = new DataViewService( new RockContext() ).Get( selectionConfig.DataViewId.Value );
                return dataView;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Filter issue(s):  + errorMessages.AsDelimited( ;  )</exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            return GetExpressionWithOverrides( entityType, serviceInstance, parameterExpression, null, selection );
        }

        #endregion

        #region IDataFilterWithOverrides

        /// <summary>
        /// Gets the expression with overrides.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="selection">A formatted string representing the filter settings: FieldName, <see cref="ComparisonType">Comparison Type</see>, (optional) Comparison Value(s)</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Filter issue(s): " + errorMessages.AsDelimited( "; " )</exception>
        public Expression GetExpressionWithOverrides( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, DataViewFilterOverrides dataViewFilterOverrides, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var dataView = this.GetSelectedDataView( selectionConfig );

            if ( dataView != null && dataView.DataViewFilter != null )
            {
                
                // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
                if ( !IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
                {
                    var errorMessages = new List<string>();
                    if ( selectionConfig.UsePersisted == false )
                    {
                        dataViewFilterOverrides?.IgnoreDataViewPersistedValues.Add( dataView.Id );
                    }

                    Expression expression = dataView.GetExpression( serviceInstance, parameterExpression, dataViewFilterOverrides, out errorMessages );
                    if ( errorMessages.Any() )
                    {
                        throw new System.Exception( "Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
                    }

                    return expression;
                }
            }

            return null;
        }

        #endregion IDataFilterWithOverrides

        #region Private Methods

        /// <summary>
        /// Determines whether [is view in filter] [the specified data view id].
        /// </summary>
        /// <param name="dataViewId">The data view id.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        ///   <c>true</c> if [is view in filter] [the specified data view id]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsViewInFilter( int dataViewId, Rock.Model.DataViewFilter filter )
        {
            if ( filter.EntityTypeId == EntityTypeCache.Get( this.GetType() ).Id )
            {
                int? filterDataViewId = filter.Selection.AsIntegerOrNull();
                if ( filterDataViewId.HasValue )
                {
                    if ( filterDataViewId == dataViewId )
                    {
                        return true;
                    }
                }
            }

            foreach ( var childFilter in filter.ChildFilters )
            {
                if ( IsViewInFilter( dataViewId, childFilter ) )
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}