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
            foreach ( var control in controls )
            {
                control.RenderControl( writer );
                writer.WriteLine();
            }
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
        protected Expression ComparisonExpression( ComparisonType comparisonType, MemberExpression property, Expression value )
        {
            MemberExpression valueExpression;
            Expression comparisonExpression = null;
            bool isNullableType = property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof( Nullable<> );
            if ( isNullableType )
            {
                // if Nullable Type compare on the .Value of the property (if it HasValue)
                valueExpression = Expression.Property( property, "Value" );
            }
            else
            {
                valueExpression = property;
            }

            if ( comparisonType == ComparisonType.Contains )
            {
                comparisonExpression = Expression.Call( valueExpression, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value );
            }

            else if ( comparisonType == ComparisonType.DoesNotContain )
            {
                comparisonExpression = Expression.Not( Expression.Call( valueExpression, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value ) );
            }

            else if ( comparisonType == ComparisonType.EndsWith )
            {
                comparisonExpression = Expression.Call( valueExpression, typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ) } ), value );
            }

            else if ( comparisonType == ComparisonType.EqualTo )
            {
                comparisonExpression = Expression.Equal( valueExpression, value );
            }

            else if ( comparisonType == ComparisonType.GreaterThan )
            {
                comparisonExpression = Expression.GreaterThan( valueExpression, value );
            }

            else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
            {
                comparisonExpression = Expression.GreaterThanOrEqual( valueExpression, value );
            }

            else if ( comparisonType == ComparisonType.IsBlank )
            {
                if ( valueExpression.Type == typeof( string ) )
                {
                    Expression trimmed = Expression.Call( valueExpression, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                    comparisonExpression = Expression.Or( Expression.Equal( trimmed, value ), Expression.Equal( valueExpression, Expression.Constant( null, valueExpression.Type ) ) );
                }
                else
                {
                    if ( isNullableType )
                    {
                        comparisonExpression = Expression.Equal(Expression.Property( property, "HasValue" ), Expression.Constant(false));
                    }
                    else
                    {
                        // if not a Nullable type, return false since there aren't any null values
                        comparisonExpression = Expression.Constant( false );
                    }
                }
            }

            else if ( comparisonType == ComparisonType.IsNotBlank )
            {
                if ( valueExpression.Type == typeof( string ) )
                {
                    Expression trimmed = Expression.Call( valueExpression, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                    Expression emtpyString = Expression.Constant( string.Empty );
                    comparisonExpression = Expression.NotEqual( trimmed, value );
                }
                else
                {
                    if ( isNullableType )
                    {
                        comparisonExpression = Expression.Property( property, "HasValue" );
                    }
                    else
                    {
                        // if not a Nullable type, return true since there aren't any non-null values
                        comparisonExpression = Expression.Constant( true );
                    }
                }
            }

            else if ( comparisonType == ComparisonType.LessThan )
            {
                comparisonExpression = Expression.LessThan( valueExpression, value );
            }

            else if ( comparisonType == ComparisonType.LessThanOrEqualTo )
            {
                comparisonExpression = Expression.LessThanOrEqual( valueExpression, value );
            }

            else if ( comparisonType == ComparisonType.NotEqualTo )
            {
                comparisonExpression = Expression.NotEqual( valueExpression, value );
            }

            else if ( comparisonType == ComparisonType.StartsWith )
            {
                comparisonExpression = Expression.Call( valueExpression, typeof( string ).GetMethod( "StartsWith", new Type[] { typeof( string ) } ), value );
            }

            // unless we are simply checking for Null/NotNull, make sure to check on HasValue for Nullable types
            if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
            {
                if ( comparisonExpression != null && isNullableType )
                {
                    // if Nullable Type we are comparing on the .Value of the property, so also make sure it HasValue
                    MemberExpression hasValue = Expression.Property( property, "HasValue" );
                    return Expression.AndAlso( hasValue, comparisonExpression );
                }
            }

            return comparisonExpression;
        }

        /// <summary>
        /// Gets a dropdownlist of the supported comparison types
        /// </summary>
        /// <param name="supportedComparisonTypes">The supported comparison types.</param>
        /// <returns></returns>
        protected RockDropDownList ComparisonControl( ComparisonType supportedComparisonTypes )
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
        /// Gets the comparison types typically used for Guid fields
        /// </summary>
        public static ComparisonType GuidFilterComparisonTypes
        {
            get
            {
                return
                    ComparisonType.EqualTo |
                    ComparisonType.NotEqualTo;
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
                    ComparisonType.IsBlank |
                    ComparisonType.IsNotBlank |
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
                    ComparisonType.IsBlank |
                    ComparisonType.IsNotBlank |
                    ComparisonType.GreaterThan |
                    ComparisonType.GreaterThanOrEqualTo |
                    ComparisonType.LessThan |
                    ComparisonType.LessThanOrEqualTo;
            }
        }

        /// <summary>
        /// Registers Javascript to hide/show .js-filter-control child elements of a .js-filter-compare dropdown
        /// </summary>
        /// <value>
        /// </value>
        public void RegisterFilterCompareChangeScript( FilterField filterControl )
        {
            string filterComparescript = @"
            $('.js-filter-compare').change( function () {
        var $fieldCriteriaRow = $(this).closest('.field-criteria');
        var compareValue = $(this).val();
        var isNullCompare = (compareValue == 32 || compareValue == 64);
        if (isNullCompare) {
            $fieldCriteriaRow.find('.js-filter-control').hide();
        }
        else {
            $fieldCriteriaRow.find('.js-filter-control').show();
        }
    });
";
            // only need this script once per page
            ScriptManager.RegisterStartupScript( filterControl.Page, filterControl.Page.GetType(), "js-filter-compare-change-script", filterComparescript, true );
        }

        #endregion
    }
}