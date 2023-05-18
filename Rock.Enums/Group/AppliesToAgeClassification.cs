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

namespace Rock.Model
{
    /// <summary>
    /// The Age Classification to which a group requirement applies. All, Adults, or Children.
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum AppliesToAgeClassification
    {
        /// <summary>
        /// All
        /// </summary>
        All = 0,

        /// <summary>
        /// Adults
        /// </summary>
        Adults = 1,

        /// <summary>
        /// Children
        /// </summary>
        Children = 2
    }
}