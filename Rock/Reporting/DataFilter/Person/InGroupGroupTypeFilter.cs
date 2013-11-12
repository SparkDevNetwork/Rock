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
    [Description( "Filter people on whether they are in a group of a specific group type" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group of Group Type Filter" )]
    public class InGroupGroupTypeFilter : DataFilterComponent
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
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "In Group of Group Type";
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
            return @"
function() {
  var groupTypeName = $('.group-type-picker', $content).find(':selected').text()
  var checkedRoles = $('.rock-check-box-list', $content).find(':checked').closest('label');
  var result = 'In group of group type: ' + groupTypeName;
  if (checkedRoles.length > 0) {
     var roleCommaList = checkedRoles.map(function() { return $(this).text() }).get().join(',');
     result = result + ', with role(s): ' + roleCommaList;
  }

  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Group Member";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var groupType = new GroupTypeService().Get( selectionValues[0].AsInteger() ?? 0 );

                var groupTypeRoleIdList = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsInteger() ).ToList();

                var groupTypeRoles = new GroupTypeRoleService().Queryable().Where( a => groupTypeRoleIdList.Contains( a.Id ) ).ToList();

                if ( groupType != null )
                {
                    result = string.Format( "In group of group type: {0}", groupType.Name );
                    if ( groupTypeRoles.Count() > 0 )
                    {
                        result += string.Format( ", with role(s): {0}", groupTypeRoles.Select( a => a.Name ).ToList().AsDelimited( "," ) );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The GroupTypePicker
        /// </summary>
        private GroupTypePicker groupTypePicker = null;

        /// <summary>
        /// The GroupTypeRole CheckBoxList
        /// </summary>
        private RockCheckBoxList cblRole = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.ID + "_0";
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService().Queryable().OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            groupTypePicker.SelectedIndexChanged += groupTypePicker_SelectedIndexChanged;
            groupTypePicker.AutoPostBack = true;
            filterControl.Controls.Add( groupTypePicker );

            cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Role(s)";
            cblRole.ID = filterControl.ID + "_1";
            filterControl.Controls.Add( cblRole );

            return new Control[2] { groupTypePicker, cblRole };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the groupTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupTypeId = groupTypePicker.SelectedValueAsId() ?? 0;
            var groupTypeService = new GroupTypeService();
            var groupType = groupTypeService.Get( groupTypeId );
            if ( groupType != null )
            {
                var groupTypeRoleService = new GroupTypeRoleService();
                var list = groupTypeRoleService.Queryable().Where( a => a.GroupTypeId == groupType.Id ).OrderBy( a => a.Order ).ToList();
                cblRole.Items.Clear();
                foreach ( var item in list )
                {
                    cblRole.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
                }

                cblRole.Visible = list.Count > 0;
            }
            else
            {
                cblRole.Visible = false;
            }
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
            controls[0].RenderControl( writer );
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
            var value1 = ( controls[0] as GroupTypePicker ).SelectedValueAsId().ToString();
            var value2 = ( controls[1] as RockCheckBoxList ).SelectedValuesAsInt.AsDelimited( "," );
            return value1 + "|" + value2;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                ( controls[0] as GroupTypePicker ).SetValue( selectionValues[0].AsInteger() );

                groupTypePicker_SelectedIndexChanged( this, new EventArgs() );

                string[] selectedRoleIds = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                RockCheckBoxList cblRole = ( controls[1] as RockCheckBoxList );

                foreach ( var item in cblRole.Items.OfType<ListItem>() )
                {
                    item.Selected = selectedRoleIds.Contains( item.Value );
                }
            }
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
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                GroupMemberService groupMemberService = new GroupMemberService();
                int groupTypeId = selectionValues[0].AsInteger() ?? 0;

                var groupMemberServiceQry = groupMemberService.Queryable().Where( xx => xx.Group.GroupTypeId == groupTypeId );

                var groupRoleIds = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => int.Parse( n ) ).ToList();
                if ( groupRoleIds.Count() > 0 )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupRoleIds.Contains( xx.GroupRoleId ) );
                }

                var qry = new Rock.Data.Service<Rock.Model.Person>().Queryable()
                    .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}