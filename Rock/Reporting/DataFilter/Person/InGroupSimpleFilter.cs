// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they are in the specified group or groups" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group(s) Filter" )]
    public class InGroupSimpleFilter : DataFilterComponent
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
            return "In Group(s)";
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
            return GetGroupFilterClientSelection( false );
        }

        /// <summary>
        /// Gets the group filter client selection based on if we are in "Not" mode
        /// </summary>
        /// <param name="not">if set to <c>true</c> [not].</param>
        /// <returns></returns>
        public virtual string GetGroupFilterClientSelection( bool not )
        {
            return string.Format( @"Rock.reporting.formatFilterForGroupSimpleFilterField('{0}', $content)", not ? "Not in groups:" : "In groups:" );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            return GroupFilterFormatSelection( selection, false );
        }

        /// <summary>
        /// Formats the selection for the InGroupFilter/NotInGroupFilter based on if we are in "Not" mode
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="not">if set to <c>true</c> [not].</param>
        /// <returns></returns>
        public virtual string GroupFilterFormatSelection( string selection, bool not )
        {
            string result = "Group Member";

            var rockContext = new RockContext();
            var groupGuids = selection.Split( ',' ).AsGuidList();
            var groups = new GroupService( rockContext ).GetByGuids( groupGuids );

            if ( groups != null )
            {
                result = string.Format( not ? "Not in groups: {0}" : "In groups: {0}", groups.Select( a => a.Name ).ToList().AsDelimited( ", ", " or " ) );
            }

            return result;
        }

        /// <summary>
        /// The GroupPicker
        /// </summary>
        private GroupPicker gp = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            gp = new GroupPicker();
            gp.ID = filterControl.ID + "_gp";
            gp.Label = "Group(s)";
            gp.CssClass = "js-group-picker";
            gp.AllowMultiSelect = true;
            filterControl.Controls.Add( gp );

            return new Control[1] { gp };
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
            if ( controls.Count() < 1 )
            {
                return;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            groupPicker.RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            if ( controls.Count() < 1 )
            {
                return null;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            List<int> groupIdList = groupPicker.SelectedValues.AsIntegerList();
            var groupGuids = new GroupService( new RockContext() ).GetByIds( groupIdList ).Select( a => a.Guid ).Distinct().ToList();

            return groupGuids.AsDelimited( "," );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() < 1 )
            {
                return;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;

            List<Guid> groupGuids = selection.Split( ',' ).AsGuidList();
            var groups = new GroupService( new RockContext() ).GetByGuids( groupGuids );
            if ( groups != null )
            {
                groupPicker.SetValues( groups );
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
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var rockContext = (RockContext)serviceInstance.Context;

            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            List<Guid> groupGuids = selection.Split( ',' ).AsGuidList();
            var groupService = new GroupService( rockContext );
            var groupIds = groupService.GetByGuids( groupGuids ).Select( a => a.Id ).Distinct().ToList();

            var groupMemberServiceQry = groupMemberService.Queryable().Where( xx => groupIds.Contains( xx.GroupId ) );

            var qry = new PersonService( (RockContext)serviceInstance.Context ).Queryable()
                .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}