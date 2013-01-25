using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Extension;
using Rock.Field;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class FilterComponent : IComponent
    {
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
            FilterComparisonType comparisonType = FilterComparisonType.None;
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

            return string.Format( "{0} {1} {2}", Title, 
                comparisonType != FilterComparisonType.None ? comparisonType.ConvertToString() : "", 
                value );
        }

        /// <summary>
        /// Gets the selection controls
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="selection">The selection.</param>
        public virtual void AddControls( Control parentControl, bool setSelection, string selection )
        {
            parentControl.Controls.Add( new LiteralControl( this.Title + " " ) );

            DropDownList ddl = ComparisonControl( StringFilterComparisonTypes );
            ddl.ID = parentControl.ID + "_ddl";
            parentControl.Controls.Add( ddl );

            parentControl.Controls.Add( new LiteralControl( " " ) );

            TextBox tb = new TextBox();
            tb.ID = parentControl.ID + "_tb";
            parentControl.Controls.Add( tb );

            if ( setSelection )
            {
                string[] options = selection.Split( '|' );
                if ( options.Length > 0 )
                {
                    ddl.SelectedValue = options[0];
                }
                if ( options.Length > 1 )
                {
                    tb.Text = options[1];
                }
            }

        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public virtual string GetSelection( Control parentControl )
        {
            string comparisonType = string.Empty;
            string value = string.Empty;

            foreach ( Control control in parentControl.Controls )
            {
                if ( control is DropDownList )
                {
                    comparisonType = ( (DropDownList)control ).SelectedValue;
                }
                if ( control is TextBox )
                {
                    value = ( (TextBox)control ).Text;
                }
            }

            return string.Format( "{0}|{1}", comparisonType, value );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
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
                    FilterComparisonType.Contains &
                    FilterComparisonType.DoesNotContain &
                    FilterComparisonType.EqualTo &
                    FilterComparisonType.IsBlank &
                    FilterComparisonType.IsNotBlank &
                    FilterComparisonType.NotEqualTo &
                    FilterComparisonType.StartsWith &
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
                    FilterComparisonType.EqualTo &
                    FilterComparisonType.NotEqualTo &
                    FilterComparisonType.GreaterThan &
                    FilterComparisonType.GreaterThanOrEqualTo &
                    FilterComparisonType.LessThan &
                    FilterComparisonType.LessThanOrEqualTo;
            }
        }

    }


}