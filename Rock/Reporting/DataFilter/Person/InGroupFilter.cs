//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataTransform.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they are in the specified group" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group Filter" )]
    public class InGroupFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.Person ).FullName; }
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
            return "In Group";
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
            return "#TODO#";// "$('input:first', $content).is(':checked') ? 'Has Picture' : 'Doesn\\'t Have Picture'";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            return "#TODO##";
        }

        private GroupPicker gp = null;
        private GroupRolePicker grp = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            gp = new GroupPicker();
            gp.ID = "groupFilterGroupPicker";
            gp.SelectItem += gp_SelectItem;
            grp = new GroupRolePicker();
            grp.ID = "groupFilterGroupRolePicker";

            return new Control[2] { gp, grp };
        }

        /// <summary>
        /// Handles the SelectItem event of the gp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gp_SelectItem( object sender, EventArgs e )
        {
            grp.GroupTypeId = gp.SelectedValueAsId();
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            //controls[0].RenderControl( writer );
            controls[1].RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var value1 = ( controls[0] as GroupPicker ).SelectedValueAsId().ToString();
            var value2 = ( controls[1] as GroupRolePicker ).GroupRoleId.ToString();
            return value1 + "," + value2;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            ( controls[0] as GroupPicker ).SetValue( selection.SplitDelimitedValues()[0].AsInteger() );
            ( controls[1] as GroupRolePicker ).GroupRoleId = selection.SplitDelimitedValues()[1].AsInteger();
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, object serviceInstance, Expression parameterExpression, string selection )
        {
            //MemberExpression property = Expression.Property( parameterExpression, "PhotoId" );
            //Expression hasValue = Expression.Property( property, "HasValue" );
            //Expression value = Expression.Constant( selection == "1" );
            //Expression result = Expression.Equal( hasValue, value );

            selection = "1,1";

            GroupMemberService groupMemberService = new GroupMemberService();
            int groupId = selection.SplitDelimitedValues()[0].AsInteger() ?? 0;
            var groupMemberServiceQry = groupMemberService.Queryable().Where( xx => xx.GroupId == groupId );

            MethodCallExpression methodCallExpression = new Rock.Data.Service<Rock.Model.Person>().Queryable()
                .Where( p => groupMemberServiceQry.Any(xx => xx.PersonId == p.Id))
                .Expression as MethodCallExpression;

            Expression<Func<LambdaExpression>> executionLambda = Expression.Lambda<Func<LambdaExpression>>( methodCallExpression.Arguments[1] );
            Expression extractedExpression = ( executionLambda.Compile().Invoke() as Expression<Func<Rock.Model.Person, bool>> ).Body;
            extractedExpression = new ParameterRebinder( parameterExpression as ParameterExpression ).Visit( extractedExpression );

            return extractedExpression;
        }

        public class ParameterRebinder : ExpressionVisitor
        {
            private ParameterExpression _parameterExpression;

            public ParameterRebinder( ParameterExpression parameterExpression )
            {
                this._parameterExpression = parameterExpression;
            }

            protected override Expression VisitParameter( ParameterExpression p )
            {
                if ( p.Name != "xx" )
                {
                    p = _parameterExpression;
                }
                return base.VisitParameter( p );
            }
        }

        #endregion

    }
}