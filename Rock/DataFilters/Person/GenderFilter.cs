//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.DataFilters.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Filter persons on Gender")]
    [Export(typeof(DataFilterComponent))]
    [ExportMetadata("ComponentName", "Gender Filter")]
    public class GenderFilter : DataFilterComponent
    {
        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public override string Title
        {
            get { return "Gender"; }
        }

        /// <summary>
        /// Gets the name of the filtered entity type.
        /// </summary>
        /// <value>
        /// The name of the filtered entity type.
        /// </value>
        public override string FilteredEntityTypeName
        {
            get { return "Rock.Model.Person"; }
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
        public override Control[] CreateChildControls()
        {
            RadioButtonList rbl = new RadioButtonList();
            rbl.RepeatLayout = RepeatLayout.Flow;
            rbl.RepeatDirection = RepeatDirection.Horizontal;
            rbl.Items.Add( new ListItem( "Male", "Male" ) );
            rbl.Items.Add( new ListItem( "Female", "Female" ) );
            rbl.SelectedValue = "Male";

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
            rbl.Items[0].Selected = selection != "Female";
            rbl.Items[1].Selected = selection == "Female";
        }
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
        {
            Gender gender = selection.ConvertToEnum<Gender>();

            Expression property = Expression.Property( parameterExpression, "Gender" );
            Expression constant = Expression.Constant( gender );
            return Expression.Equal( property, constant );
        }
    }
}