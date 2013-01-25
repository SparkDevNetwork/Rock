using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Field;
using Rock.Model;

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
            bool hasPicture = false;
            if (!Boolean.TryParse(selection, out hasPicture))
                hasPicture = false;

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
        /// Gets the selection controls
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="selection">The selection.</param>
        public override void AddControls( Control parentControl, bool setSelection, string selection )
        {
            parentControl.Controls.Add( new LiteralControl( this.Title + " " ) );

            CheckBox cb = new CheckBox();
            cb.ID = parentControl.ID + "_cb";
            parentControl.Controls.Add( cb );

            cb.Text = "Yes";

            if ( setSelection )
            {
                bool hasPicture = false;
                if ( !Boolean.TryParse( selection, out hasPicture ) )
                    hasPicture = false;

                cb.Checked = hasPicture;
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public override string GetSelection( Control parentControl )
        {
            foreach ( Control control in parentControl.Controls )
            {
                if ( control is CheckBox )
                {
                    return ( (CheckBox)control ).Checked.ToString();
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
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