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

namespace Rock.Lava
{
    /// <summary>
    /// Marks a Type as being accessible to a Lava template during rendering.
    /// All public properties of the associated Type will be accessible by default.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class LavaTypeAttribute : Attribute
    {
        /// <summary>
        /// An array of property and method names that are allowed to be called on the object.
        /// </summary>
        public string[] AllowedMembers { get; private set; }

        #region Constructors

        /// <summary>
        /// Apply the LavaTypeAttribute with a collection of allowed members.
        /// </summary>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        public LavaTypeAttribute( params string[] allowedMembers )
        {
            AllowedMembers = allowedMembers;
        }

        #endregion
    }
}