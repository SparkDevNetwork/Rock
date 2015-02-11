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
using System.Data.Entity;
using System.Linq;
using System.Web.Compilation;

using Quartz;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ScheduleService"/> entity objects.
    /// </summary>
    public partial class ScheduleService 
    {
        /// <summary>
        /// Parses the attributes associated with a group type and returns a list of the selected
        /// schedules for any and all attributes associated with the group type that are a 'Schedules' 
        /// field type.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Schedule> GetGroupTypeSchedules( GroupType groupType )
        {
            var scheduleGuids = new List<Guid>();

            if ( groupType != null )
            {
                if ( groupType.Attributes == null )
                {
                    groupType.LoadAttributes( (RockContext)this.Context );
                }

                Guid? scheduleAttributeType = Rock.SystemGuid.FieldType.SCHEDULES.AsGuid();
                foreach ( var attribute in groupType.Attributes.Select( a => a.Value ) )
                {
                    if ( attribute.FieldType.Guid.Equals( scheduleAttributeType ) )
                    {
                        var value = groupType.GetAttributeValue( attribute.Key );
                        foreach ( string splitValue in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            Guid? guid = splitValue.AsGuidOrNull();
                            if ( guid.HasValue && !scheduleGuids.Contains( guid.Value ) )
                            {
                                scheduleGuids.Add( guid.Value );
                            }
                        }
                    }
                }
            }

            return Queryable().AsNoTracking()
                .Where( s => scheduleGuids.Contains( s.Guid ) );
        }
    }
}
