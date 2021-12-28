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
namespace Rock.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class IgnorePropertiesAttribute : System.Attribute
    {
        /// <summary>
        /// The keys string
        /// </summary>
        public string[] Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnorePropertiesAttribute"/> class.
        /// </summary>
        /// <param name="properties">The keys.</param>
        public IgnorePropertiesAttribute( string[] properties )
            : base()
        {
            this.Properties = properties;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}