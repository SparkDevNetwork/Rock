using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Web.Cache;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// General container for exported entity data. This object should be encoded
    /// and decoded as JSON data.
    /// </summary>
    public class ExportedEntitiesContainer
    {
        #region Properties

        /// <summary>
        /// The encoded entities that will be used to identify all the database
        /// entities that are to be recreated.
        /// </summary>
        public List<EncodedEntity> Entities { get; private set; }

        /// <summary>
        /// The Guid values of the root entities that were used when exporting.
        /// </summary>
        public List<Guid> RootEntities { get; private set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Create a new, empty, instance of the data container.
        /// </summary>
        public ExportedEntitiesContainer()
        {
            Entities = new List<EncodedEntity>();
            RootEntities = new List<Guid>();
        }

        /// <summary>
        /// Check for any missing entity types that would be encountered during an import
        /// operation.
        /// </summary>
        /// <returns>A collection of strings that identify the missing entity type class names.</returns>
        public ICollection<string> GetMissingEntityTypes()
        {
            //
            // Ensure we know about all referenced entity types.
            //
            var missingTypes = new List<string>();

            //
            // Get all the explicit EntityTypes for entities that are to be imported.
            //
            var entityTypeStrings = Entities.Select( e => e.EntityType ).ToList();

            //
            // Check for GUID and EntityType references.
            //
            var references = Entities.SelectMany( e => e.References );

            entityTypeStrings.AddRange( references
                .Where( r => r.Type == ReferenceType.Guid )
                .Select( r => r.EntityType ) );

            entityTypeStrings.AddRange( references
                .Where( r => r.Type == ReferenceType.EntityType )
                .Select( r => ( string ) r.Data ) );

            //
            // Just check the unique ones.
            //
            entityTypeStrings = entityTypeStrings.Distinct().ToList();

            foreach ( var entityType in entityTypeStrings )
            {
                if ( EntityTypeCache.Get( entityType, false, null ) == null )
                {
                    missingTypes.Add( entityType );
                }
            }

            return missingTypes;
        }

        #endregion
    }
}
