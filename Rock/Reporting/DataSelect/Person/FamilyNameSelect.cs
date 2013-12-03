using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the name of the Family that the Person belongs to" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person Family Name" )]
    public class FamilyNameSelect : DataSelectComponent<Rock.Model.Person>
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get
            {
                return "Family Name";
            }
        }

        /// <summary>
        /// Gets the name of the entity type.
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string EntityTypeName
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Family Name";
            }
        }

        #region Query

        /// <summary>
        /// Returns an IQueryable that subquery of this DataSelectComponent
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override IQueryable<IEntity> SubQuery( string selection )
        {

            Guid groupTypeFamily = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            IQueryable<IEntity> qry = new GroupMemberService().Queryable()
                .Where( a => a.Group.GroupType.Guid == groupTypeFamily );

            return qry;
        }

        /// <summary>
        /// The Linq Expression for the Select portion of the SubQuery
        /// </summary>
        /// <returns></returns>
        public override Expression<System.Func<IEntity, DataSelectData>> SelectExpression
        {
            get
            {
                Expression<Func<IEntity, DataSelectData>> selectExpression = a => new DataSelectData
                {
                    EntityId = ( a as GroupMember ).PersonId,
                    Data = new
                    {
                        // this should be the same as ColumnPropertyName
                        GroupName = ( a as GroupMember ).Group.Name
                    }
                };

                return selectExpression;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "GroupName";
            }
        }

        #endregion

        #region Controls methods

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            return new System.Web.UI.Control[] { };
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            // TODO: 
            return base.FormatSelection( selection );
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the widget is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// The client format script.
        ///   </value>
        public override string GetClientFormatSelection()
        {
            // TODO: 
            return base.GetClientFormatSelection();
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            // TODO: 
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            return null;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            // nothing to do
        }

        #endregion
    }
}
