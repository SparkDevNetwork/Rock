//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Rock.Extension;

namespace Rock.BinaryFile
{
    /// <summary>
    /// MEF Container class for Binary File Storage Components
    /// </summary>
    public class StorageContainer : Container<StorageComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<StorageContainer> instance =
            new Lazy<StorageContainer>( () => new StorageContainer() );

        /// <summary>
        /// Prevents a default instance of the <see cref="StorageContainer"/> class from being created.
        /// </summary>
        private StorageContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static StorageContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static StorageComponent GetComponent( string entityTypeName )
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
        [ImportMany( typeof (StorageComponent) )]
        protected override IEnumerable<Lazy<StorageComponent, IComponentData>> MEFComponents { get; set; }
    }
}
