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

namespace Rock.Reporting.PersonFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter persons on whether they have a picture or not" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "Has Picture Filter" )]
    public class HasPictureFilter : FilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Has Picture"; }
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            bool hasPicture = true;
            if (!Boolean.TryParse(selection, out hasPicture))
                hasPicture = true;

            if (hasPicture)
            {
                return "Has Picture";
            }
            else
            {
                return "Doesn't Have Picture";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls()
        {
            CheckBox cb = new CheckBox();
            cb.Checked = true;
            return new Control[1] { cb };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( HtmlTextWriter writer, Control[] controls )
        {
            writer.Write( this.Title + " " );
            controls[0].RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            return ( (CheckBox)controls[0] ).Checked.ToString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            bool hasPicture = true;
            if ( !Boolean.TryParse( selection, out hasPicture ) )
                hasPicture = true;

            ( (CheckBox)controls[0] ).Checked = hasPicture;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
        {
            bool hasPicture = true;
            if ( Boolean.TryParse( selection, out hasPicture ) )
            {
                MemberExpression property = Expression.Property( parameterExpression, "PhotoId" );
                Expression hasValue = Expression.Property( property, "HasValue");
                Expression value = Expression.Constant( hasPicture );
                return Expression.Equal( hasValue, value);
            }
            return null;
        }
    }
}