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
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// 
    /// </summary>
    public class AssetStorageContainer : Container<AssetStorageComponent, IComponentData>
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static readonly Lazy<AssetStorageContainer> instance = new Lazy<AssetStorageContainer>( () => new AssetStorageContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static AssetStorageContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static AssetStorageComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        // TODO: Do we need or want a default Component?

        /// <summary>
        /// Gets or sets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        [ImportMany( typeof( AssetStorageComponent ) )]
        protected override IEnumerable<Lazy<AssetStorageComponent, IComponentData>> MEFComponents { get; set; }

        /// <inheritdoc/>
        public override void Refresh()
        {
            base.Refresh();

            // Load all the Attributes to the Asset Storage Provider so that they may not be loaded every time by the Detail Block in the remote device.
            var assetStorageProviderEntityType = EntityTypeCache.Get( "Rock.Model.AssetStorageProvider" );
            foreach ( var component in MEFComponents )
            {
                var providerComponentEntityType = component.Value.EntityType;
                Rock.Attribute.Helper.UpdateAttributes(
                    providerComponentEntityType.GetEntityType(),
                    assetStorageProviderEntityType.Id,
                    "EntityTypeId",
                    providerComponentEntityType.Id.ToString() );
            }
        }
    }
}
