//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Rock.Extension;

namespace Rock.Storage
{
    /// <summary>
    /// MEF Container class for Binary File Storage Components
    /// </summary>
    public class ProviderContainer : Container<ProviderComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<ProviderContainer> instance =
            new Lazy<ProviderContainer>( () => new ProviderContainer() );

        /// <summary>
        /// Prevents a default instance of the <see cref="ProviderContainer"/> class from being created.
        /// </summary>
        private ProviderContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ProviderContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static ProviderComponent GetComponent( string entityTypeName )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.TypeName == entityTypeName )
                {
                    return component;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof (ProviderComponent) )]
        protected override IEnumerable<Lazy<ProviderComponent, IComponentData>> MEFComponents { get; set; }
    }
}
