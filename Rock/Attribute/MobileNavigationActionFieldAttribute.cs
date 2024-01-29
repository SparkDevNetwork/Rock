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
    /// Field Attribute to set color 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class MobileNavigationActionFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// A value that can be used in the default to specify the action of None.
        /// </summary>
        internal const string NoneValue = "{\"Type\": 0}";

        /// <summary>
        /// A value that can be used in the default to specify the action of Pop Page.
        /// </summary>
        internal const string PopSinglePageValue = "{\"Type\": 1, \"PopCount\": 1}";

        /// <summary>
        /// A value that can be used in the default to specify the action of Push Page.
        /// </summary>
        internal const string PushPageValue = "{\"Type\": 4, \"PageGuid\": \"\"}";

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileNavigationActionFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MobileNavigationActionFieldAttribute( string name )
            : base( name, string.Empty, true, string.Empty, string.Empty, 0, null, typeof( Rock.Field.Types.MobileNavigationActionFieldType ).FullName )
        {
        }
    }
}
