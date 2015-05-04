// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they are not in a group of a specific group type" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Not In Group of Group Type Filter" )]
    public class NotInGroupGroupTypeFilter : InGroupGroupTypeFilter
    {
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
            return "Not In Group of Group Type";
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
  var result = 'Not in group of group type: ' + groupTypeName;
  if (checkedRoles.length > 0) {
     var roleCommaList = checkedRoles.map(function() { return $(this).text() }).get().join(',');
     result = result + ', with role(s): ' + roleCommaList;
  }

  var groupMemberStatus = $('.js-group-member-status option:selected', $content).text();
  if (groupMemberStatus) {
     result = result + ', with member status:' + groupMemberStatus;
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
                var groupType = Rock.Web.Cache.GroupTypeCache.Read( selectionValues[0].AsGuid() );

                var groupTypeRoleGuidList = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                var groupTypeRoles = new GroupTypeRoleService( new RockContext() ).Queryable().Where( a => groupTypeRoleGuidList.Contains( a.Guid ) ).ToList();

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 3 )
                {
                    groupMemberStatus = selectionValues[2].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                if ( groupType != null )
                {
                    result = string.Format( "Not in group of group type: {0}", groupType.Name );
                    if ( groupTypeRoles.Count() > 0 )
                    {
                        result += string.Format( ", with role(s): {0}", groupTypeRoles.Select( a => a.Name ).ToList().AsDelimited( "," ) );
                    }

                    if ( groupMemberStatus.HasValue )
                    {
                        result += string.Format( ", with member status: {0}", groupMemberStatus.ConvertToString() );
                    }
                }
            }

            return result;
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
            var baseExpression = base.GetExpression( entityType, serviceInstance, parameterExpression, selection );
            if ( baseExpression != null )
            {
                return Expression.Not( baseExpression );
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}