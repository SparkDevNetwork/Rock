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

namespace Rock.Model
{
    /// <summary>
    /// Represents a circular reference exception. This occurs when a group is set as a parent of a group that is higher in the group hierarchy. 
    /// </summary>
    /// <remarks>
    ///  An example of this is when a child group is set as the parent of its parent group.
    /// </remarks>
    public class GroupParentCircularReferenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParentCircularReferenceException" /> class.
        /// </summary>
        public GroupParentCircularReferenceException()
            : base( "Circular Reference in Group Parents" )
        {
        }
    }
}
