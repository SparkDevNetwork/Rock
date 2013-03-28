//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.DataFilters
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EnumPropertyFilter<T> : DataFilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return PropertyName.SplitCase(); }
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public abstract string PropertyName { get; }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                string friendlyName = EntityTypeCache.Read( FilteredEntityTypeName ).FriendlyName + " ";
                return friendlyName.TrimStart( ' ' ) + "Properties";
            }
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
        public override string ClientFormatSelection
        {
            get
            {
                return string.Format( "'{0} is ' + '\\'' + $('input:checked', $content).val() + '\\''", Title );
            }
        }
        
        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            return string.Format( "{0} is '{1}'", Title, selection );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Rock.Web.UI.RockPage page )
        {
            RadioButtonList rbl = new RadioButtonList();
            rbl.RepeatDirection = RepeatDirection.Horizontal;

            foreach ( var value in Enum.GetValues( typeof( T ) ) )
            {
                rbl.Items.Add( new ListItem( Enum.GetName( typeof( T ), value ).SplitCase() ) );
            }

            if ( rbl.Items.Count > 0 )
            {
                rbl.Items[0].Selected = true;
            }

            return new Control[1] { rbl };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( HtmlTextWriter writer, Control[] controls )
        {
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Label
            writer.AddAttribute( "class", "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( this.Title + " " );
            writer.RenderEndTag();

            // Controls
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[0].RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            return ( (RadioButtonList)controls[0] ).SelectedValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            var rbl = (RadioButtonList)controls[0];
            foreach ( ListItem item in rbl.Items )
            {
                item.Selected = item.Value == selection;
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
        {
            T theEnum = selection.ConvertToEnum<T>();

            Expression property = Expression.Property( parameterExpression, PropertyName );
            Expression constant = Expression.Constant( theEnum );
            return Expression.Equal( property, constant );
        }
    }
}