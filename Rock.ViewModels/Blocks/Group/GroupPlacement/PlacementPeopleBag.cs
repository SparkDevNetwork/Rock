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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// A bag containing temporary groups and people available for placement.
    /// </summary>
    public class PlacementPeopleBag
    {
        /// <summary>
        /// The temporary groups.
        /// </summary>
        public List<PlacementGroupBag> TempGroups { get; set; }

        /// <summary>
        /// The people to place.
        /// </summary>
        public List<PersonBag> PeopleToPlace { get; set; }
    }

}
