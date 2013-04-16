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

namespace Rock.DataFilters
{
    /// <summary>
    /// MEF Container class for data filters
    /// </summary>
    public class DataFilterContainer : Container<DataFilterComponent, IComponentData>
    {
        private static DataFilterContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>

        public static DataFilterContainer Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new DataFilterContainer();
                }
                return instance;
            }
        }

        private DataFilterContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets a list of entity type names that have Data Filter components
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailableFilteredEntityTypeNames()
        {
            var entityTypeNames = new List<string>();

            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( !entityTypeNames.Contains( component.AppliesToEntityType ) )
                {
                    entityTypeNames.Add( component.AppliesToEntityType );
                }
            }

            return entityTypeNames;
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static DataFilterComponent GetComponent( string entityTypeName )
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
        /// Gets the components that are for filtering a given entity type name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static List<DataFilterComponent> GetComponentsByFilteredEntityName( string entityTypeName )
        {
            return Instance.Components
                .Where( c => 
                    c.Value.Value.AppliesToEntityType == entityTypeName ||
                    c.Value.Value.AppliesToEntityType == string.Empty )
                .Select( c => c.Value.Value )
                .OrderBy( c => c.Order)
                .ToList();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( DataFilterComponent ) )]
        protected override IEnumerable<Lazy<DataFilterComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore

    }
}