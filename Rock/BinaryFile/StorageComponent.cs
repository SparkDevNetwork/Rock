//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Attribute;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.BinaryFile
{
    [ComponentField( "Rock.BinaryFile.StorageContainer, Rock" )]
    public abstract class StorageComponent : Component
    {
        public abstract StorageControl Control { get; }

        public EntityTypeCache EntityType
        {
            get { return EntityTypeCache.Read( this.GetType() ); }
        }

        public StorageComponent Storage
        {
            get
            {
                Guid entityTypeGuid = Guid.Empty;

                if ( Guid.TryParse( GetAttributeValue( "StorageContainer" ), out entityTypeGuid ) )
                {
                    foreach ( var serviceEntry in StorageContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        var entityType = EntityTypeCache.Read( component.GetType() );

                        if ( entityType != null && entityType.Guid.Equals( entityTypeGuid ) )
                        {
                            return component;
                        }
                    }
                }

                return null;
            }
        }

        public StorageComponent()
        {
            this.LoadAttributes();
        }
    }
}
