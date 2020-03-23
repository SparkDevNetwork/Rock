using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;
using Rock.Web.Cache;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// MEF Container class for File Format Type components.
    /// </summary>
    public class FileFormatTypeContainer : Container<FileFormatTypeComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static readonly Lazy<FileFormatTypeContainer> instance =
            new Lazy<FileFormatTypeContainer>( () => new FileFormatTypeContainer() );

        static FileFormatTypeContainer()
        {
            Instance.UpdateAttributes();
        }

        /// <summary>
        /// Get the singleton instance of this container.
        /// </summary>
        public static FileFormatTypeContainer Instance
        {
            get
            {
                return instance.Value;
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type name.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <returns>A FileEngineComponent that matches the entity type name.</returns>
        public static FileFormatTypeComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name of a component with the matching Entity Type name.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <returns>The component name.</returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets or sets the list of MEF components found.
        /// </summary>
        [ImportMany( typeof( FileFormatTypeComponent ) )]
        protected override IEnumerable<Lazy<FileFormatTypeComponent, IComponentData>> MEFComponents { get; set; }

        public void UpdateAttributes()
        {
            var entityType = EntityTypeCache.Get<Model.ImageCashLetterFileFormat>( false );
            if ( entityType != null )
            {
                foreach ( var component in this.Components )
                {
                    using ( var rockContext = new Rock.Data.RockContext() )
                    {
                        var fileEngineComponent = component.Value.Value;
                        var type = fileEngineComponent.GetType();

                        Rock.Attribute.Helper.UpdateAttributes( type, entityType.Id, "EntityTypeId", EntityTypeCache.GetId( type.FullName ).ToString(), rockContext );
                    }
                }
            }
        }
    }
}
