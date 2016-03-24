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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their age.
    /// </summary>
    [Description( "Removes (or excludes) the groups for each selected family member that are not specific to their age." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Age" )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true, "", 0 )]
    [BooleanField( "Age Required", "Select 'Yes' if groups with an age filter should be removed/excluded when person does not have an age.", true, "", 1 )]
    [AttributeField( "9BBFDA11-0D22-40D5-902F-60ADFBC88987", "Group Age Range Attribute", "Select the attribute used to define the age range of the group", true, false, "43511B8F-71D9-423A-85BF-D1CD08C1998E", order: 2 )]
    public class FilterGroupsByAge : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();
                bool ageRequired = GetAttributeValue( action, "AgeRequired" ).AsBoolean( true );

                // get the admin-selected attribute key instead of using a hardcoded key
                var ageRangeAttributeKey = string.Empty;
                var ageRangeAttributeGuid = GetAttributeValue( action, "GroupAgeRangeAttribute" ).AsGuid();
                if ( ageRangeAttributeGuid != Guid.Empty )
                {
                    ageRangeAttributeKey = AttributeCache.Read( ageRangeAttributeGuid, rockContext ).Key;
                }

                // log a warning if the attribute is missing or invalid
                if ( string.IsNullOrWhiteSpace( ageRangeAttributeKey ) )
                {
                    action.AddLogEntry( string.Format( "The Group Age Range attribute is not selected or invalid for '{0}'.", action.ActionType.Name ) );
                }

                foreach ( var person in family.People )
                {
                    var ageAsDouble = person.Person.AgePrecise;
                    decimal? age = null;

                    if ( !ageAsDouble.HasValue && !ageRequired )
                    {
                        continue;
                    }

                    if ( ageAsDouble.HasValue )
                    {
                        age = Convert.ToDecimal( ageAsDouble.Value );
                    }

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            var ageRange = group.Group.GetAttributeValue( ageRangeAttributeKey ).ToStringSafe();

                            var ageRangePair = ageRange.Split( new char[] { ',' }, StringSplitOptions.None );
                            string minAgeValue = null;
                            string maxAgeValue = null;

                            if ( ageRangePair.Length == 2 )
                            {
                                minAgeValue = ageRangePair[0];
                                maxAgeValue = ageRangePair[1];
                            }

                            if ( minAgeValue != null )
                            {
                                decimal minAge = 0;

                                if ( decimal.TryParse( minAgeValue, out minAge ) )
                                {
                                    decimal? personAgePrecise = null;

                                    if ( age.HasValue )
                                    {
                                        int groupMinAgePrecision = minAge.GetDecimalPrecision();
                                        personAgePrecise = age.Floor( groupMinAgePrecision );
                                    }

                                    if ( !age.HasValue || personAgePrecise < minAge )
                                    {
                                        if ( remove )
                                        {
                                            groupType.Groups.Remove( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
                                        }
                                        continue;
                                    }
                                }
                            }

                            if ( maxAgeValue != null )
                            {
                                decimal maxAge = 0;

                                if ( decimal.TryParse( maxAgeValue, out maxAge ) )
                                {
                                    decimal? personAgePrecise = null;

                                    if ( age.HasValue )
                                    {
                                        int groupMaxAgePrecision = maxAge.GetDecimalPrecision();
                                        personAgePrecise = age.Floor( groupMaxAgePrecision );
                                    }

                                    if ( !age.HasValue || personAgePrecise > maxAge )
                                    {
                                        if ( remove )
                                        {
                                            groupType.Groups.Remove( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
                                        }
                                        continue;
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
