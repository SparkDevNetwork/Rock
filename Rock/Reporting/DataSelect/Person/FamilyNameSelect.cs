//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock;

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

        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
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
                return "FamilyName";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( string ); }
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

        #endregion

        #region Methods

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
            return "Family Name";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, Expression entityIdProperty, string selection )
        {
            // groupmembers
            var groupMembers = context.Set<GroupMember>();

            // m
            ParameterExpression groupMemberParameter = Expression.Parameter( typeof( GroupMember ), "m" );

            // m.PersonId
            MemberExpression memberPersonIdProperty = Expression.Property( groupMemberParameter, "PersonId" );

            // m.Group
            MemberExpression groupProperty = Expression.Property( groupMemberParameter, "Group" );

            // m.Group.GroupType
            MemberExpression groupTypeProperty = Expression.Property( groupProperty, "GroupType" );

            // m.Group.GroupType.Guid
            MemberExpression groupTypeGuidProperty = Expression.Property( groupTypeProperty, "Guid" );

            // family group type guid
            Expression groupTypeConstant = Expression.Constant(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid());

            // m.PersonId == p.Id
            Expression personCompare = Expression.Equal( memberPersonIdProperty, entityIdProperty );

            // m.Group.GroupType.Guid == GROUPTYPE_FAMILY guid
            Expression groupTypeCompare = Expression.Equal( groupTypeGuidProperty, groupTypeConstant );

            // m.PersonID == p.Id && m.Group.GroupType.Guid == GROUPTYPE_FAMILY guid
            Expression andExpression = Expression.And( personCompare, groupTypeCompare );

            // m => m.PersonID == p.Id && m.Group.GroupType.Guid == GROUPTYPE_FAMILY guid
            var compare = new Expression[] {
                Expression.Constant(groupMembers),
                Expression.Lambda<Func<GroupMember, bool>>(andExpression, new ParameterExpression[] { groupMemberParameter } )
            };

            // groupmembers.Where(m => m.PersonID == p.Id && m.Group.GroupType.Guid == GROUPTYPE_FAMILY guid)
            Expression whereExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(GroupMember)}, compare);

            // m.Group.Name
            MemberExpression groupName = Expression.Property(groupProperty, "Name");

            // m => m.Group.Name
            Expression groupNameLambda = Expression.Lambda(groupName, new ParameterExpression[] { groupMemberParameter });

            // groupmembers.Where(m => m.PersonID == p.Id && m.Group.GroupType.Guid == GROUPTYPE_FAMILY guid).Select( m => m.Group.Name);
            Expression selectName = Expression.Call( typeof( Queryable ), "Select", new Type[] { typeof( GroupMember ), typeof( string ) }, whereExpression, groupNameLambda );

            // groupmembers.Where(m => m.PersonID == p.Id && m.Group.GroupType.Guid == GROUPTYPE_FAMILY guid).Select( m => m.Group.Name).FirstOrDefault();
            Expression firstOrDefault = Expression.Call(typeof(Queryable), "FirstOrDefault", new Type[] { typeof(string)}, selectName);

            return firstOrDefault;
        }

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
