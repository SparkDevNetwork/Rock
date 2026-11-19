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

#if NET
namespace Rock.Enums.Group
#else
namespace Rock.Model
#endif
{
    /// <summary>
    /// The type of requirement check that is done. Sql, DataView or Manual.
    /// </summary>
#if !NET
    [Enums.EnumDomain( "Group" )]
#endif
    public enum RequirementCheckType
    {
        /// <summary>
        /// SQL
        /// </summary>
        Sql = 0,

        /// <summary>
        /// A dataview
        /// </summary>
        Dataview = 1,

        /// <summary>
        /// Manual
        /// </summary>
        Manual = 2
    }
}
