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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Filter entities using another dataview
    /// </summary>
    [Description( "Filter entities using another dataview" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Other Data View Filter" )]
    public class OtherDataViewFilter : DataFilterComponent
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
            return "'Included in ' + " +
                "'\\'' + $('select:first', $content).find(':selected').text() + '\\' ' + " +
                "'Data View'";
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

            int? dataviewId = selection.AsIntegerOrNull();
            if ( dataviewId.HasValue )
            {
                var dataView = new DataViewService( new RockContext() ).Get( dataviewId.Value );
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
            int entityTypeId = EntityTypeCache.Read( entityType ).Id;

            var ddlDataViews = new DataViewPicker();
            ddlDataViews.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( ddlDataViews );
            
            ddlDataViews.EntityTypeId = entityTypeId;
            

            return new Control[1] { ddlDataViews };
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
            controls[0].RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            return ( (DataViewPicker)controls[0] ).SelectedValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            ( (DataViewPicker)controls[0] ).SelectedValue = selection;
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
            int? dataviewId = selection.AsIntegerOrNull();
            if ( dataviewId.HasValue )
            {
                var dataView = new DataViewService( (RockContext)serviceInstance.Context ).Get( dataviewId.Value );
                if ( dataView != null && dataView.DataViewFilter != null )
                {
                    // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
                    if ( !IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
                    {
                        // TODO: Should probably verify security again on the selected dataview and it's filters,
                        // as that could be a moving target.
                        var errorMessages = new List<string>();
                        Expression expression = dataView.GetExpression( serviceInstance, parameterExpression, out errorMessages );
                        if ( errorMessages.Any() )
                        {
                            throw new System.Exception( "Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
                        }

                        return expression;
                    }
                }
            }

            return null;
        }

        #endregion

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
            if ( filter.EntityTypeId == EntityTypeCache.Read( this.GetType() ).Id )
            {
                int? filterDataViewId = filter.Selection.AsIntegerOrNull();
                if ( filterDataViewId.HasValue )
                {
                    if (filterDataViewId == dataViewId)
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