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
using System.Linq;
using System.Web.Compilation;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.WorkflowActivityType"/> entity objects
    /// </summary>
    public partial class WorkflowActivityTypeService 
    {
        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( WorkflowActivityType item )
        {
            var activityService = new WorkflowActivityService( (RockContext)this.Context );
            foreach ( var activity in activityService.Queryable().Where( a => a.ActivityTypeId == item.Id ) )
            {
                activityService.Delete( activity );
            }
            return base.Delete( item );
        }
    }
}