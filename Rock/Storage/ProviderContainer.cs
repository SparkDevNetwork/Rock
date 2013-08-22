//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Extension;

namespace Rock.Storage
{
    /// <summary>
    /// MEF Container class for Binary File Storage Components
    /// </summary>
    public class ProviderContainer : Container<ProviderComponent, IComponentData>
    {
        /// <summary>
        /// The fully qualified class name of the default provider.
        /// </summary>
        private const string DEFAULT_COMPONENT_NAME = "Rock.Storage.Provider.Database, Rock";

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<ProviderContainer> instance = new Lazy<ProviderContainer>( () => new ProviderContainer() );

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
        /// Gets the default component.
        /// </summary>
        /// <value>
        /// The default component.
        /// </value>
        public static ProviderComponent DefaultComponent
        {
            get
            {
                return Instance.Components
                    .Select( serviceEntry => serviceEntry.Value.Value )
                    .Single( component => component.TypeName == DEFAULT_COMPONENT_NAME );
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static ProviderComponent GetComponent( string entityTypeName )
        {
            return Instance.Components
                .Select( serviceEntry => serviceEntry.Value.Value )
                .FirstOrDefault( component => component.TypeName == entityTypeName );
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
