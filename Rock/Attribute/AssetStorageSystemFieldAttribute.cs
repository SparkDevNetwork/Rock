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
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    class AssetStorageProviderFieldAttributeAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetStorageProviderFieldAttributeAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AssetStorageProviderFieldAttributeAttribute( string name )
            : base( SystemGuid.FieldType.ASSET_STORAGE_PROVIDER.AsGuid(), name )
        {
        }

        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public AssetStorageProviderFieldAttributeAttribute( string name = "Asset Storage System", string description = "", bool required = true, string defaultFinancialGatewayGuid = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.ASSET_STORAGE_PROVIDER.AsGuid(), name, description, required, defaultFinancialGatewayGuid, category, order, key )
        {
        }
    }
}
