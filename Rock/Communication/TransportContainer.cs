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

namespace Rock.Communication
{
    /// <summary>
    /// MEF Container class for Communication Transport Componenets
    /// </summary>
    public class TransportContainer : Container<TransportComponent, IComponentData>
    {
        private static TransportContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static TransportContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new TransportContainer();
                return instance;
            }
        }

        private TransportContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static TransportComponent GetComponent( string entityTypeName )
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

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( TransportComponent ) )]
        protected override IEnumerable<Lazy<TransportComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}