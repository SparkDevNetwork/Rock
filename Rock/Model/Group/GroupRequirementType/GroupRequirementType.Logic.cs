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

namespace Rock.Model
{
    public partial class GroupRequirementType
    {
        #region Methods

        /// <summary>
        /// Gets the merge objects that can be used in the SQL Expression. Set the person parameter if checking a specific person, otherwise pass person as null.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMergeObjects( Group group, Person person )
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Group", group );
            mergeObjects.Add( "GroupRequirementType", this );
            mergeObjects.Add( "Person", person );

            return mergeObjects;
        }

        #endregion
    }
}
