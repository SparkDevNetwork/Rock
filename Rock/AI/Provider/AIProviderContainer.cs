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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.CSharp;
using Rock.Address;
using Rock.Data;
using Rock.Extension;
using Rock.UniversalSearch;
using Rock.Web.Cache;

namespace Rock.AI.Provider
{
    /// <summary>
    /// The container for AI components.
    /// </summary>
    public class AIProviderContainer : Container<AIProviderComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<AIProviderContainer> instance =
            new Lazy<AIProviderContainer>( () => new AIProviderContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static AIProviderContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static AIProviderComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Returns the active component
        /// </summary>
        /// <returns></returns>
        public static AIProviderComponent GetActiveComponent()
        {
            return Instance.Components.Select( c => c.Value.Value ).Where( c => c.IsActive ).FirstOrDefault();
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( AIProviderComponent ) )]
        protected override IEnumerable<Lazy<AIProviderComponent, IComponentData>> MEFComponents { get; set; }

        /// <summary>
        /// Forces a reloading of all the components
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // Create any attributes that need to be created
            var providerEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.AIProvider ) );

            using ( var rockContext = new RockContext() )
            {
                foreach ( var component in this.Components )
                {
                    var componentType = component.Value.Value.GetType();
                    var componentEntityTypeId = EntityTypeCache.GetId( componentType );

                    Rock.Attribute.Helper.UpdateAttributes( componentType,
                        providerEntityTypeId,
                        "EntityTypeId",
                        componentEntityTypeId.ToString(),
                        rockContext );
                }
            }
        }
    }
}
