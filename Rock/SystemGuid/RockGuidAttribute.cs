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
using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Base class for attributes that decorate items in Rock with well-known GUID attributes.
    /// When applying these, the Guids must be unique or an exception will occur.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public abstract class RockGuidAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public readonly Guid Guid;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rock.RockGuidAttribute" /> class from the specified  <see cref="string"/>.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public RockGuidAttribute( string guid )
            : this( new Guid( guid ) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rock.RockGuidAttribute" /> class from the specified <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private RockGuidAttribute( Guid guid )
        {
            this.Guid = guid;
        }
    }
}