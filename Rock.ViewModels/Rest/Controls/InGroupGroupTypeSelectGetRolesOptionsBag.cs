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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Options bag for the InGroupGroupTypeSelect's GetRoles method.
    /// </summary>
    public class InGroupGroupTypeSelectGetRolesOptionsBag
    {
        /// <summary>
        /// Guid of the group type we wish to get roles for.
        /// </summary>
        public Guid GroupTypeGuid { get; set; }
    }
}
