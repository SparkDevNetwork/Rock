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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or 1 components filtered by channel
    /// Stored as "Channel.Guid|Component.Guid"
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class InteractionChannelInteractionComponentFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionChannelInteractionComponentFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public InteractionChannelInteractionComponentFieldAttribute( string name )
            : base( name, fieldTypeClass: typeof( Field.Types.InteractionChannelInteractionComponentFieldType ).FullName )
        {
        }
    }
}