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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace com.centralaz.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) all the groups from each group type for each selected family member except the first match.
    /// </summary>
    [ActionCategory( "com_centralaz: Check-In" )]
    [Description( "Removes all but the 'first match' grroup." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By First Match" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed. Select 'No' if they should just be marked as excluded.", true, "", 0 )]
    public class FilterGroupsByFirstMatch : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                foreach ( var person in family.People )
                {
                    var foundFirstMatch = false;

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        bool useFirstMatch = groupType.GroupType.GetAttributeValue( "UseFirstMatch" ).AsBoolean();

                        // Only perform filtering for group types that are set to UseFirstMatch true.
                        if ( useFirstMatch )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                if ( foundFirstMatch )
                                {
                                    if ( remove )
                                    {
                                        groupType.Groups.Remove( group );
                                    }
                                    else
                                    {
                                        group.ExcludedByFilter = true;
                                    }
                                }
                                foundFirstMatch = true;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}