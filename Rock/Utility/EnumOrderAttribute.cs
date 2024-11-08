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

namespace Rock.Utility
{
    /// <summary>
    /// Allow the display order of an enum to be specified (vs just sorting by the numeric value of the enum)
    /// from https://stackoverflow.com/a/30654676/1755417
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Field )]
    [RockObsolete( "1.16.6" )]
    [Obsolete( "Use the new Rock.Enums.EnumOrderAttribute instead." )]
    public class EnumOrderAttribute : System.Attribute
    {
        /// <summary>
        /// The order
        /// </summary>
        public readonly int Order;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumOrderAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        public EnumOrderAttribute( int order )
        {
            this.Order = order;
        }
    }
}
