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

using Newtonsoft.Json;

using Rock.Model;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to add a new defined value to the specified defined
    /// type. This checks permission on the DefinedValue to be added.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "5c56c012-4085-48b3-b2fb-04fa7f622e1f" )]
    public class AddDefinedValueToTypeGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the defined type identifier that must match to grant access.
        /// </summary>
        [JsonProperty( "dt", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int DefinedTypeId { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="AddDefinedValueToTypeGrantRule"/> class from being created.
        /// </summary>
        private AddDefinedValueToTypeGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDefinedValueToTypeGrantRule"/> class
        /// for granting <see cref="Authorization.VIEW"/> access.
        /// </summary>
        /// <param name="definedTypeId">The entity type identifier that must be matched to grant access.</param>
        public AddDefinedValueToTypeGrantRule( int definedTypeId )
            : base( Authorization.EDIT )
        {
            DefinedTypeId = definedTypeId;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( obj is DefinedValue definedValue )
            {
                return definedValue.Id == 0 && definedValue.DefinedTypeId == DefinedTypeId;
            }

            return false;
        }

        #endregion
    }
}
