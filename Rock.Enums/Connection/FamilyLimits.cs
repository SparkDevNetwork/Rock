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

namespace Rock.Utility
{
    /// <summary>
    /// Determines whether a Campaign Connection creates requests for every
    /// person in the data view or limits them to the head of household.
    /// </summary>
    [Enums.EnumDomain( "Connection" )]
    public enum FamilyLimits
    {
        /// <summary>
        /// Only the head of household in each family receives a connection request.
        /// </summary>

        HeadOfHouse = 0,

        /// <summary>
        /// Every person in the data view receives a connection request.
        /// </summary>

        Everyone = 1,
    }
}
