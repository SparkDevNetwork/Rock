﻿// <copyright>
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
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataFilterComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.  Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public abstract string AppliesToEntityType { get; }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public virtual string Section
        {
            get { return "Additional Filters"; }
        }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                return defaults;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public abstract string GetTitle( Type entityType );

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public virtual string GetClientFormatSelection( Type entityType )
        {
            return string.Format( "'{0} ' + $('select', $content).find(':selected').text() + ' \\'' + $('input', $content).val() + '\\''", GetTitle( entityType ) );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public virtual string FormatSelection( Type entityType, string selection )
        {
            ComparisonType comparisonType = ComparisonType.StartsWith;
            string value = string.Empty;

            string[] options = selection.Split( '|' );
            if ( options.Length > 0 )
            {
                comparisonType = options[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
            }

            if ( options.Length > 1 )
            {
                value = options[1];
            }

            return string.Format( "{0} {1} '{2}'", GetTitle( entityType ), comparisonType.ConvertToString(), value );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddl = ComparisonControl( StringFilterComparisonTypes );
            ddl.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( ddl );

            var tb = new TextBox();
            tb.ID = filterControl.ID + "_1";
            filterControl.Controls.Add( tb );

            return new Control[2] { ddl, tb };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public virtual void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            writer.Write( this.GetTitle( entityType ) + " " );
            controls[0].RenderControl( writer );
            writer.Write( " " );
            controls[1].RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual string GetSelection( Type entityType, Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            string value = ( (TextBox)controls[1] ).Text;
            return string.Format( "{0}|{1}", comparisonType, value );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public virtual void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length >= 2 )
            {
                ( (DropDownList)controls[0] ).SelectedValue = options[0];
                ( (TextBox)controls[1] ).Text = options[1];
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
        public abstract Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection );

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the comparison expression.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected Expression ComparisonExpression( ComparisonType comparisonType, Expression property, Expression value )
        {
            if ( comparisonType == ComparisonType.Contains )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value );
            }

            if ( comparisonType == ComparisonType.DoesNotContain )
            {
                return Expression.Not( Expression.Call( property, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value ) );
            }

            if ( comparisonType == ComparisonType.EndsWith )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ) } ), value );
            }

            if ( comparisonType == ComparisonType.EqualTo )
            {
                return Expression.Equal( property, value );
            }

            if ( comparisonType == ComparisonType.GreaterThan )
            {
                return Expression.GreaterThan( property, value );
            }

            if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
            {
                return Expression.GreaterThanOrEqual( property, value );
            }

            if ( comparisonType == ComparisonType.IsBlank )
            {
                Expression trimmed = Expression.Call( property, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                Expression emtpyString = Expression.Constant( string.Empty );
                return Expression.Equal( trimmed, value );
            }

            if ( comparisonType == ComparisonType.IsNotBlank )
            {
                Expression trimmed = Expression.Call( property, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                Expression emtpyString = Expression.Constant( string.Empty );
                return Expression.NotEqual( trimmed, value );
            }

            if ( comparisonType == ComparisonType.LessThan )
            {
                return Expression.LessThan( property, value );
            }

            if ( comparisonType == ComparisonType.LessThanOrEqualTo )
            {
                return Expression.LessThanOrEqual( property, value );
            }

            if ( comparisonType == ComparisonType.NotEqualTo )
            {
                return Expression.NotEqual( property, value );
            }

            if ( comparisonType == ComparisonType.StartsWith )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "StartsWith", new Type[] { typeof( string ) } ), value );
            }

            return null;
        }

        /// <summary>
        /// Gets a dropdownlist of the supported comparison types
        /// </summary>
        /// <param name="supportedComparisonTypes">The supported comparison types.</param>
        /// <returns></returns>
        protected DropDownList ComparisonControl( ComparisonType supportedComparisonTypes )
        {
            var ddl = new RockDropDownList();
            foreach ( ComparisonType comparisonType in Enum.GetValues( typeof( ComparisonType ) ) )
            {
                if ( ( supportedComparisonTypes & comparisonType ) == comparisonType )
                {
                    ddl.Items.Add( new ListItem( comparisonType.ConvertToString(), comparisonType.ConvertToInt().ToString() ) );
                }
            }

            return ddl;
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets the comparison types typically used for string fields
        /// </summary>
        public static ComparisonType StringFilterComparisonTypes
        {
            get
            {
                return
                    ComparisonType.Contains |
                    ComparisonType.DoesNotContain |
                    ComparisonType.EqualTo |
                    ComparisonType.IsBlank |
                    ComparisonType.IsNotBlank |
                    ComparisonType.NotEqualTo |
                    ComparisonType.StartsWith |
                    ComparisonType.EndsWith;
            }
        }

        /// <summary>
        /// Gets the comparison types typically used for numeric fields
        /// </summary>
        public static ComparisonType NumericFilterComparisonTypes
        {
            get
            {
                return
                    ComparisonType.EqualTo |
                    ComparisonType.NotEqualTo |
                    ComparisonType.GreaterThan |
                    ComparisonType.GreaterThanOrEqualTo |
                    ComparisonType.LessThan |
                    ComparisonType.LessThanOrEqualTo;
            }
        }

        /// <summary>
        /// Gets the date filter comparison types.
        /// </summary>
        /// <value>
        /// The date filter comparison types.
        /// </value>
        public static ComparisonType DateFilterComparisonTypes
        {
            get
            {
                return
                    ComparisonType.EqualTo |
                    ComparisonType.GreaterThan |
                    ComparisonType.GreaterThanOrEqualTo |
                    ComparisonType.LessThan |
                    ComparisonType.LessThanOrEqualTo;
            }
        }

        #endregion
    }
}