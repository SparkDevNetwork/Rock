//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;

namespace Rock.Reporting
{
    /// <summary>
    /// MEF Container class for reporting filters
    /// </summary>
    public class FilterContainer : ContainerManaged<FilterComponent, IComponentData>
    {
        private static FilterContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>

        public static FilterContainer Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new FilterContainer();
                }
                return instance;
            }
        }

        private FilterContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static FilterComponent GetComponent( string entityTypeName )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                string componentName = component.GetType().FullName;
                if ( componentName == entityTypeName )
                {
                    return component;
                }
            }

            return null;
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( FilterComponent ) )]
        protected override IEnumerable<Lazy<FilterComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore

    }
}