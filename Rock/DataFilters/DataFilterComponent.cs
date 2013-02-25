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

namespace Rock.DataFilters
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataFilterComponent : Component
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
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public virtual string Section 
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the name of the filtered entity type.
        /// </summary>
        /// <value>
        /// The name of the filtered entity type.
        /// </value>
        public abstract string FilteredEntityTypeName { get; }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public virtual string ClientFormatSelection
        {
            get
            {
                return string.Format( "'{0} ' + $('select', $content).find(':selected').text() + ' \\'' + $('input', $content).val() + '\\''", Title );
            }
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        public virtual string FormatSelection( string selection )
        {
            ComparisonType comparisonType = ComparisonType.StartsWith;
            string value = string.Empty;

            string[] options = selection.Split( '|' );
            if ( options.Length > 0 )
            {
                try { comparisonType= options[0].ConvertToEnum<ComparisonType>(); }
                catch {}
            }
            if ( options.Length > 1 )
            {
                value = options[1];
            }

            return string.Format( "{0} {1} '{2}'", Title, comparisonType.ConvertToString(), value );
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
            //SetSelection( controls, string.Format( "{0}|", ComparisonType.StartsWith.ConvertToInt().ToString() ) );
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
            var ddl = new DropDownList();
            foreach ( ComparisonType comparisonType in Enum.GetValues( typeof( ComparisonType ) ) )
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

    }

}