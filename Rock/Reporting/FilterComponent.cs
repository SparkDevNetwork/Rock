//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Extension;
using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class FilterComponent : ComponentManaged
    {
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

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public abstract string Title { get; }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        public virtual string FormatSelection( string selection )
        {
            FilterComparisonType comparisonType = FilterComparisonType.StartsWith;
            string value = string.Empty;

            string[] options = selection.Split( '|' );
            if ( options.Length > 0 )
            {
                try { comparisonType= options[0].ConvertToEnum<FilterComparisonType>(); }
                catch {}
            }
            if ( options.Length > 1 )
            {
                value = options[1];
            }

            return string.Format( "{0} {1} {2}", Title, comparisonType.ConvertToString(), value );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls()
        {
            var controls = new Control[2] {
                ComparisonControl( StringFilterComparisonTypes ),
                new TextBox() };
            SetSelection( controls, string.Format( "{0}|", FilterComparisonType.StartsWith.ConvertToInt().ToString() ) );
            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public virtual void RenderControls( HtmlTextWriter writer, Control[] controls )
        {
            writer.Write( this.Title + " " );
            controls[0].RenderControl( writer );
            writer.Write( " " );
            controls[1].RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual string GetSelection( Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            string value = ( (TextBox)controls[1] ).Text;
            return string.Format( "{0}|{1}", comparisonType, value );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public virtual void SetSelection( Control[] controls, string selection )
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
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public abstract Expression GetExpression( Expression parameterExpression, string selection );

        /// <summary>
        /// Gets the comparison expression.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected Expression ComparisonExpression( FilterComparisonType comparisonType, Expression property, Expression value )
        {
            if ( comparisonType == FilterComparisonType.Contains )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value );
            }

            if ( comparisonType == FilterComparisonType.DoesNotContain )
            {
                return Expression.Not( Expression.Call( property, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value ) );
            }

            if ( comparisonType == FilterComparisonType.EndsWith )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ) } ), value );
            }

            if ( comparisonType == FilterComparisonType.EqualTo )
            {
                return Expression.Equal( property, value );
            }

            if ( comparisonType == FilterComparisonType.GreaterThan )
            {
                return Expression.GreaterThan( property, value );
            }

            if ( comparisonType == FilterComparisonType.GreaterThanOrEqualTo )
            {
                return Expression.GreaterThanOrEqual( property, value );
            }

            if ( comparisonType == FilterComparisonType.IsBlank )
            {
                Expression trimmed = Expression.Call( property, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                Expression emtpyString = Expression.Constant( string.Empty );
                return Expression.Equal( trimmed, value );
            }

            if ( comparisonType == FilterComparisonType.IsNotBlank )
            {
                Expression trimmed = Expression.Call( property, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                Expression emtpyString = Expression.Constant( string.Empty );
                return Expression.NotEqual( trimmed, value );
            }

            if ( comparisonType == FilterComparisonType.LessThan )
            {
                return Expression.LessThan( property, value );
            }

            if ( comparisonType == FilterComparisonType.LessThanOrEqualTo )
            {
                return Expression.LessThanOrEqual( property, value );
            }

            if ( comparisonType == FilterComparisonType.NotEqualTo )
            {
                return Expression.NotEqual( property, value );
            }

            if ( comparisonType == FilterComparisonType.StartsWith )
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
        protected DropDownList ComparisonControl( FilterComparisonType supportedComparisonTypes )
        {
            var ddl = new DropDownList();
            foreach ( FilterComparisonType comparisonType in Enum.GetValues( typeof( FilterComparisonType ) ) )
            {
                if ( ( supportedComparisonTypes & comparisonType ) == comparisonType )
                {
                    ddl.Items.Add( new ListItem( comparisonType.ConvertToString(), comparisonType.ConvertToInt().ToString() ) );
                }
            }
            return ddl;
        }

        /// <summary>
        /// Gets the comparison types typically used for string fields
        /// </summary>
        public static FilterComparisonType StringFilterComparisonTypes
        {
            get
            {
                return
                    FilterComparisonType.Contains |
                    FilterComparisonType.DoesNotContain |
                    FilterComparisonType.EqualTo |
                    FilterComparisonType.IsBlank |
                    FilterComparisonType.IsNotBlank |
                    FilterComparisonType.NotEqualTo |
                    FilterComparisonType.StartsWith |
                    FilterComparisonType.EndsWith;
            }
        }

        /// <summary>
        /// Gets the comparison types typically used for numeric fields
        /// </summary>
        public static FilterComparisonType NumericFilterComparisonTypes
        {
            get
            {
                return
                    FilterComparisonType.EqualTo |
                    FilterComparisonType.NotEqualTo |
                    FilterComparisonType.GreaterThan |
                    FilterComparisonType.GreaterThanOrEqualTo |
                    FilterComparisonType.LessThan |
                    FilterComparisonType.LessThanOrEqualTo;
            }
        }

    }


}