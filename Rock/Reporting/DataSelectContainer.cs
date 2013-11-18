using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Extension;

namespace Rock.Reporting
{
    /// <summary>
    /// MEF Container class for data selects
    /// </summary>
    public class DataSelectContainer : Container<DataSelectComponent, IComponentData>
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static DataSelectContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DataSelectContainer Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new DataSelectContainer();
                }

                return instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DataSelectContainer"/> class from being created.
        /// </summary>
        private DataSelectContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the available select entity type names.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailableSelectEntityTypeNames()
        {
            var entityTypeNames = new List<string>();

            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( !entityTypeNames.Contains( component.EntityTypeName ) )
                {
                    entityTypeNames.Add( component.EntityTypeName );
                }
            }

            return entityTypeNames;
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static DataSelectComponent GetComponent( string entityTypeName )
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
        /// Gets the components that are for transformed a given entity type name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static List<DataSelectComponent> GetComponentsBySelectedEntityTypeName( string entityTypeName )
        {
            return Instance.Components
                .Where( c => c.Value.Value.EntityTypeName == entityTypeName )
                .Select( c => c.Value.Value )
                .OrderBy( c => c.Order )
                .ToList();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( DataSelectComponent ) )]
        protected override IEnumerable<Lazy<DataSelectComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}