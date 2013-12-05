//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;

namespace Rock.PersonProfile
{
    /// <summary>
    /// MEF Container class for Person Badge Componenets
    /// </summary>
    public class BadgeContainer : Container<BadgeComponent, IComponentData>
    {
        private static BadgeContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static BadgeContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new BadgeContainer();
                return instance;
            }
        }

        private BadgeContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static BadgeComponent GetComponent( string entityTypeName )
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
        [ImportMany( typeof( BadgeComponent ) )]
        protected override IEnumerable<Lazy<BadgeComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}