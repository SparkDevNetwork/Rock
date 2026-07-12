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
    /// Field Attribute to select a Step Program.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class StreakTypeFieldAttribute : SelectFieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreakTypeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultGuids">The default guids.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public StreakTypeFieldAttribute( string name = "", string description = "", bool required = true, string defaultGuids = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.STREAK_TYPE.AsGuid(), name )
        {
            Category = category;
            DefaultValue = defaultGuids;
            Description = description;
            IsRequired = required;
            Order = order;

            if ( key.IsNotNullOrWhiteSpace() )
            {
                Key = key;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreakTypeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StreakTypeFieldAttribute( string name )
            : base( SystemGuid.FieldType.STREAK_TYPE, name )
        {
        }
    }
}
