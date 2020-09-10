﻿// <copyright>
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

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    ///     A Data Filter component that filters groups on whether they exist in a specified branch of the Group hierarchy.
    /// </summary>
    [Description( "Filter groups on whether they exist in a specified group branch" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Branch Filter" )]
    public class GroupBranchFilter : DataFilterComponent
    {
        private enum IncludedGroupsSpecifier
        {
            EntireBranch = 0,
            DescendantsOnly = 1
        }
        

        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.  Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Model.Group ).FullName; }
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
            return "Group Branch";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            string functionText =
                @"
function()
{
  var groupName = $('.group-picker', $content).find('.selected-names').text();
  var branchType = $('.rock-drop-down-list', $content).find(':selected').val(); 
  var result = '';
  switch (branchType)
  {
    case '{AllGroups}':
      result = 'Is in group branch: ';
      break;
    case '{DescendantsOnly}':
      result = 'Is sub-group of: ';
      break;
  }
  if (groupName.length > 0)
  {
    result = result + groupName;
  }
  else
  {
    result = result + '(any)';
  }  
  return result;
}
";
            // Replace enumerated type values in script.
            functionText = functionText.Replace( "{AllGroups}", IncludedGroupsSpecifier.EntireBranch.ToString() );
            functionText = functionText.Replace( "{DescendantsOnly}", IncludedGroupsSpecifier.DescendantsOnly.ToString() );

            return functionText;
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result;

            var settings = new GroupBranchFilterSettings( selection );
            var group = new GroupService( new RockContext() ).Get( settings.ParentGroupId );

            var groupName = ( group != null ) ? group.Name : "(any)";

            switch ( settings.IncludedGroups )
            {
                case IncludedGroupsSpecifier.DescendantsOnly:
                    result = string.Format( "Is sub-group of: {0}", groupName );
                    break;
                default:
                    result = string.Format( "Is in group branch: {0}", groupName );
                    break;
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="filterControl"></param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var gpParentGroup = new GroupPicker();
            gpParentGroup.ID = filterControl.ID + "_0";
            gpParentGroup.Label = "Parent Group";
            filterControl.Controls.Add( gpParentGroup );

            var ddlIncludedGroups = new RockDropDownList();
            ddlIncludedGroups.ID = filterControl.ID + "_1";
            ddlIncludedGroups.Label = "Branch Type";
            ddlIncludedGroups.Items.Add( new ListItem { Text = "Parent and Descendants", Value = IncludedGroupsSpecifier.EntireBranch.ToString(), Selected = true } );
            ddlIncludedGroups.Items.Add( new ListItem { Text = "Descendants Only", Value = IncludedGroupsSpecifier.DescendantsOnly.ToString() } );
            filterControl.Controls.Add( ddlIncludedGroups );

            return new Control[] { gpParentGroup, ddlIncludedGroups };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var settings = new GroupBranchFilterSettings();

            settings.ParentGroupId = ( (GroupPicker)controls[0] ).SelectedValueAsInt() ?? 0;

            IncludedGroupsSpecifier includedGroups;

            Enum.TryParse( ( (RockDropDownList)controls[1] ).SelectedValue, true, out includedGroups );

            settings.IncludedGroups = includedGroups;

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var settings = new GroupBranchFilterSettings( selection );

            // Parent Group
            if ( settings.ParentGroupId > 0 )
                ( (GroupPicker)controls[0] ).SetValue( settings.ParentGroupId );

            // Included Groups
            var ctlIncludedGroups = (RockDropDownList)controls[1];

            ctlIncludedGroups.SelectedValue = settings.IncludedGroups.ToString();
        }

        /// <summary>
        ///     Gets the set of Groups that are included in a Group Branch, either as the parent or a descendant.
        /// </summary>
        /// <param name="groupService"></param>
        /// <param name="parentGroup"></param>
        /// <param name="includedBranchItems"></param>
        /// <returns></returns>
        private HashSet<int> GetGroupBranchKeys( GroupService groupService, Model.Group parentGroup, IncludedGroupsSpecifier includedBranchItems )
        {
            var groupKeys = new HashSet<int>();

            if ( parentGroup == null )
            {
                return groupKeys;
            }

            // Include the Parent Group?
            if ( includedBranchItems == IncludedGroupsSpecifier.EntireBranch )
            {
                groupKeys.Add( parentGroup.Id );
            }

            // Include descendants of the Parent Group.
            foreach ( int childGroupId in groupService.GetAllDescendentGroupIds( parentGroup.Id, false ) )
            {
                groupKeys.Add( childGroupId );
            }

            return groupKeys;
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
            var settings = new GroupBranchFilterSettings( selection );

            if ( !settings.IsValid() )
            {
                return null;
            }

            var groupService = new GroupService( (RockContext)serviceInstance.Context );

            // Get the qualifying Groups.
            var parentGroup = groupService.Get( settings.ParentGroupId );

            var groupIdList = GetGroupBranchKeys( groupService, parentGroup, settings.IncludedGroups );

            // Filter by Groups
            var groupsQry = groupService.Queryable().Where( x => groupIdList.Contains( x.Id ) );

            var qry = groupService.Queryable().Where( g => groupsQry.Any( x => x.Id == g.Id ) );

            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Group>( qry, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion

        #region Settings

        /// <summary>
        ///     Settings for the Data Filter Component "Group Branch Filter".
        /// </summary>
        private class GroupBranchFilterSettings
        {
            public IncludedGroupsSpecifier IncludedGroups = IncludedGroupsSpecifier.EntireBranch;
            public int ParentGroupId;

            public GroupBranchFilterSettings()
            {
                //
            }

            public GroupBranchFilterSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            public bool IsValid()
            {
                return ( ParentGroupId != 0 );
            }

            /// <summary>
            ///     Set values from a string representation of the settings.
            /// </summary>
            /// <param name="selectionString"></param>
            public void FromSelectionString( string selectionString )
            {
                var selectionValues = selectionString.Split( '|' );

                // If selection string is invalid, ignore.
                if ( selectionValues.Length < 2 )
                {
                    return;
                }

                // Group Identifier
                var groupGuid = selectionValues[0].AsGuid();

                var group = new GroupService( new RockContext() ).Get( groupGuid );

                ParentGroupId = ( group != null ) ? group.Id : 0;

                // Included Groups
                var includedGroups = selectionValues[1].AsIntegerOrNull();

                if ( includedGroups != null )
                {
                    Enum.TryParse( includedGroups.ToString(), true, out IncludedGroups );
                }
                else
                {
                    IncludedGroups = IncludedGroupsSpecifier.EntireBranch;
                }
            }

            public string ToSelectionString()
            {
                var groupGuid = string.Empty;

                var group = new GroupService( new RockContext() ).Get( ParentGroupId );

                if ( group != null )
                {
                    groupGuid = group.Guid.ToString();
                }

                return groupGuid + "|" + ( (int)IncludedGroups );
            }
        }

        #endregion
    }
}