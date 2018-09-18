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

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Extension;


namespace Rock.Storage.AssetStorage
{
    public class AssetStorageContainer : Container<AssetStorageComponent, IComponentData>
    {
        private static readonly Lazy<AssetStorageContainer> instance = new Lazy<AssetStorageContainer>( () => new AssetStorageContainer() );

        public static AssetStorageContainer Instance
        {
            get { return instance.Value; }
        }

        public static AssetStorageComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        // TODO: Do we need or want a default Component?

        [ImportMany( typeof( AssetStorageComponent ) )]
        protected override IEnumerable<Lazy<AssetStorageComponent, IComponentData>> MEFComponents { get; set; }
    }
}
