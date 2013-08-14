//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Rock.Extension;

namespace Rock.Financial
{
    /// <summary>
    /// MEF Container class for Binary File Gateway Components
    /// </summary>
    public class GatewayContainer : Container<GatewayComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<GatewayContainer> instance =
            new Lazy<GatewayContainer>( () => new GatewayContainer() );

        /// <summary>
        /// Prevents a default instance of the <see cref="GatewayContainer"/> class from being created.
        /// </summary>
        private GatewayContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static GatewayContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static GatewayComponent GetComponent( string entityTypeName )
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
        [ImportMany( typeof (GatewayComponent) )]
        protected override IEnumerable<Lazy<GatewayComponent, IComponentData>> MEFComponents { get; set; }
    }
}
