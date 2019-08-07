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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes or excludes check-in groups that require the person to be in a particular data view.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes or excludes check-in groups that require the person to be in a data view." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Data View" )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "DataView Group Attribute", "Select the attribute used to filter by DataView.", true, false, defaultValue: "E8F8498F-5C51-4216-AC81-875349D6C2D0", order: 0 )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be removed.  Select 'No' if they should just be marked as excluded.", true, order: 1 )]
    public class FilterGroupsByDataView : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var dataViewAttributeKey = string.Empty;
            var dataViewAttributeGuid = GetAttributeValue( action, "DataViewGroupAttribute" ).AsGuid();
            if ( dataViewAttributeGuid != Guid.Empty )
            {
                dataViewAttributeKey = AttributeCache.Get( dataViewAttributeGuid, rockContext ).Key;
            }

            var dataViewService = new DataViewService( rockContext );

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            if ( group.ExcludedByFilter == true )
                            {
                                continue;
                            }

                            var dataviewGuids = group.Group.GetAttributeValue( dataViewAttributeKey );
                            if ( string.IsNullOrWhiteSpace( dataviewGuids ) )
                            {
                                continue;
                            }

                            foreach ( var dataviewGuid in dataviewGuids.SplitDelimitedValues() )
                            {
                                DataView dataview = dataViewService.Get( dataviewGuid.AsGuid() );
                                if ( dataview == null )
                                {
                                    continue;
                                }

                                if ( dataview.PersistedScheduleIntervalMinutes.HasValue && dataview.PersistedLastRefreshDateTime.HasValue )
                                {
                                    //Get record from persisted.
                                    var persistedValuesQuery = rockContext.DataViewPersistedValues.Where( a => a.DataViewId == dataview.Id );
                                    if ( !persistedValuesQuery.Any( v => v.EntityId == person.Person.Id ) )
                                    {
                                        if ( remove )
                                        {
                                            groupType.Groups.Remove( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    //Qry dataview
                                    var approvedPeopleQry = dataview.GetQuery( null, 30, out errorMessages );
                                    if ( approvedPeopleQry != null )
                                    {
                                        var approvedPeopleList = approvedPeopleQry.Select( e => e.Id ).ToList();

                                        if ( approvedPeopleList != null && !approvedPeopleList.Contains( person.Person.Id ) )
                                        {
                                            if ( remove )
                                            {
                                                groupType.Groups.Remove( group );
                                            }
                                            else
                                            {
                                                group.ExcludedByFilter = true;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
