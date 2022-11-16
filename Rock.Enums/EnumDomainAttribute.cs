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

namespace Rock.Enums
{
    /// <summary>
    /// Special use attribute to help the code generation tool know what domain
    /// the legacy enums from the Rock.Model namespace actually belong to.
    /// </summary>
    internal class EnumDomainAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the domain the Enum type belongs to.
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Initializes a new instance of the EnumDomainAttribute class.
        /// </summary>
        /// <param name="domain">The domain.</param>
        public EnumDomainAttribute( string domain )
        {
            Domain = domain;
        }
    }
}
