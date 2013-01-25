using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Field;
using Rock.Model;

namespace Rock.Reporting.PersonFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Filter persons on Gender")]
    [Export(typeof(FilterComponent))]
    [ExportMetadata("ComponentName", "Gender Filter")]
    public class GenderFilter : FilterComponent
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
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            return string.Format( "{0} is {1}", Title, selection );
        }

        /// <summary>
        /// Controls this instance.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="selection">The selection.</param>
        public override void AddControls( Control parentControl, bool setSelection, string selection )
        {
            parentControl.Controls.Add( new LiteralControl( this.Title + " " ) );

            RadioButtonList rbl = new RadioButtonList();
            rbl.ID = parentControl.ID + "_rbl";
            parentControl.Controls.Add( rbl );

            rbl.Items.Add( new ListItem( "Male", "Male" ) );
            rbl.Items.Add( new ListItem( "Female", "Female" ) );

            if ( setSelection )
            {
                if ( selection == "Male" )
                {
                    rbl.Items[0].Selected = true;
                }
                else
                {
                    rbl.Items[1].Selected = true;
                }
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
            Gender gender = selection.ConvertToEnum<Gender>();

            Expression property = Expression.Property( parameterExpression, "Gender" );
            Expression constant = Expression.Constant( gender );
            return Expression.Equal( property, constant );
        }
    }
}