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
using Rock.Web.UI.Controls;

namespace Rock.DataFilters.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they have a picture or not" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Has Picture Filter" )]
    public class HasPictureFilter : DataFilterComponent
    {

        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        ///   </value>
        public override string GetTitle( Type entityType )
        {
            return "Has Picture";
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
        public override string GetClientFormatSelection( Type entityType )
        {
            return "$('input:first', $content).is(':checked') ? 'Has Picture' : 'Doesn\\'t Have Picture'";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
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
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            CheckBox cb = new CheckBox();
            cb.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( cb );
            cb.Checked = true;

            return new Control[1] { cb };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            writer.Write( this.GetTitle(entityType) + " " );
            controls[0].RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            return ( (CheckBox)controls[0] ).Checked.ToString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            bool hasPicture = true;
            if ( !Boolean.TryParse( selection, out hasPicture ) )
                hasPicture = true;

            ( (CheckBox)controls[0] ).Checked = hasPicture;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, object serviceInstance, Expression parameterExpression, string selection )
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

        #endregion
    }
}