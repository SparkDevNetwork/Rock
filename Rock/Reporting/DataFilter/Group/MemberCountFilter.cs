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
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter groups based on member count" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Member Count" )]
    public class MemberCountFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.Group ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Member Count";
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
            return @"
function () {
    var result = 'Number of Members ';
    result += $('select', $content).find(':selected').text() + ( $('input', $content).filter(':visible').length ?  (' \'' +  $('input', $content).filter(':visible').val()  + '\'') : '' );
    return result; 
}

";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var values = selection.Split( '|' );
            if ( values.Length == 1 )
            {
                return string.Format( "Number of Members is {0}", values[3] );
            }
            else if ( values.Length >= 2 )
            {
                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                {
                    return string.Format( "Number of Members {0}", comparisonType.ConvertToString() );
                }
                else
                {
                    return string.Format( "Number of Members {0} '{1}'", comparisonType.ConvertToString(), values[1] );
                }
            }
            else
            {
                return "Member Count";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            var ddlIntegerCompare = ComparisonControl( NumericFilterComparisonTypes );
            ddlIntegerCompare.ID = string.Format( "{0}_ddlIntegerCompare", filterControl.ID );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );
            controls.Add( ddlIntegerCompare );

            var nbMemberCount = new NumberBox();
            nbMemberCount.ID = string.Format( "{0}_nbMemberCount", filterControl.ID, controls.Count() );
            nbMemberCount.AddCssClass( "js-filter-control" );
            filterControl.Controls.Add( nbMemberCount );
            controls.Add( nbMemberCount );

            nbMemberCount.FieldName = "Member Count";

            return controls.ToArray();
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
            // Comparison Row
            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Comparison Type
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[0].RenderControl( writer );
            writer.RenderEndTag();

            // Comparison Value
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[1].RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;

            return string.Format( "{0}|{1}", ddlCompare.SelectedValue, nbValue.Text );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var values = selection.Split( '|' );

            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;

            if ( values.Length == 2 )
            {
                ddlCompare.SelectedValue = values[0];
                nbValue.Text = values[1];
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var values = selection.Split( '|' );

            ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            int? memberCountValue = values[1].AsInteger( false );

            var memberCountQuery = new GroupService( (RockContext)serviceInstance.Context ).Queryable();

            switch ( comparisonType )
            {
                case ComparisonType.EqualTo:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Count() == memberCountValue );
                    break;
                case ComparisonType.GreaterThan:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Count() > memberCountValue );
                    break;
                case ComparisonType.GreaterThanOrEqualTo:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Count() >= memberCountValue );
                    break;
                case ComparisonType.IsBlank:
                    memberCountQuery = memberCountQuery.Where( p => !p.Members.Any() );
                    break;
                case ComparisonType.IsNotBlank:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Any() );
                    break;
                case ComparisonType.LessThan:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Count() < memberCountValue );
                    break;
                case ComparisonType.LessThanOrEqualTo:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Count() <= memberCountValue );
                    break;
                case ComparisonType.NotEqualTo:
                    memberCountQuery = memberCountQuery.Where( p => p.Members.Count() != memberCountValue );
                    break;
            }

            return FilterExpressionExtractor.Extract<Rock.Model.Group>( memberCountQuery, parameterExpression, "p" );
        }

        #endregion
    }
}