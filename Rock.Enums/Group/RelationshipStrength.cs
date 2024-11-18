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

namespace Rock.Enums.Group
{
    /// <summary>
    /// Represents the strength of individual relationships among members of a group.
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum RelationshipStrength
    {
        /// <summary>
        /// No established relationship or interaction.
        /// </summary>
        None = 0,

        /// <summary>
        /// Basic interactions with a familiar but limited bond.
        /// </summary>
        Casual = 5,

        /// <summary>
        /// Frequent interactions characterized by a strong and supportive relationship.
        /// </summary>
        Close = 10,

        /// <summary>
        /// Intense and trusted relationship with a high level of personal engagement and understanding.
        /// </summary>
        Deep = 20
    }
}
