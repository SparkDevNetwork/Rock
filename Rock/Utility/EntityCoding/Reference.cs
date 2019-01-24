using System;
using System.Linq;
using System.Reflection;

using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Most entities in Rock reference other entities by Id number, i.e. CategoryId.
    /// This is not useful when exporting/importing entities between systems. So we
    /// embed a Reference object that contains the Property name that originally
    /// contained the Id number. During an import operation that Property is filled in
    /// with the Id number of the object identified by the EntityType and the Guid.
    /// Other reference types are possible, <see cref="ReferenceType"/> for a list.
    /// </summary>
    public class Reference
    {
        #region Properties

        /// <summary>
        /// The name of the property to be filled in with the Id number of the
        /// referenced entity.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The entity type name that will be loaded by it's Guid.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// The type of reference this is.
        /// </summary>
        public ReferenceType Type { get; set; }

        /// <summary>
        /// The data used to re-create the reference, depends on Type.
        /// </summary>
        public string Data { get; set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Create a new empty reference. This should only be used by Newtonsoft when deserializing.
        /// </summary>
        public Reference()
        {
        }

        /// <summary>
        /// Creates a new entity reference object that is used to reconstruct the
        /// link between two entities in the database.
        /// </summary>
        /// <param name="entity">The entity we are creating a reference to.</param>
        /// <param name="propertyName">The name of the property in the containing entity.</param>
        public Reference( IEntity entity, string propertyName )
        {
            Type entityType = EntityCoder.GetEntityType( entity );

            EntityType = entityType.FullName;
            Property = propertyName;

            if ( entity is EntityType )
            {
                Type = ReferenceType.EntityType;
                Data = ( ( EntityType ) entity ).Name;
            }
            else if ( entity is FieldType )
            {
                Type = ReferenceType.FieldType;
                Data = ( ( FieldType ) entity ).Class;
            }
            else
            {
                Type = ReferenceType.Guid;
                Data = entity.Guid.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reference"/> class as a user reference.
        /// </summary>
        /// <param name="propertyName">The name of the property in the containing entity.</param>
        /// <param name="key">The key that will be used to retrieve the entity value at import time.</param>
        static public Reference UserDefinedReference( string propertyName, string key )
        {
            return new Reference
            {
                EntityType = string.Empty,
                Property = propertyName,
                Type = ReferenceType.UserDefined,
                Data = key
            };
        }

        /// <summary>
        /// Restore this reference into the entity object.
        /// </summary>
        /// <param name="entity">The entity to restore the reference into.</param>
        /// <param name="helper">The helper that provides us data access.</param>
        public void Restore( IEntity entity, EntityDecoder helper )
        {
            PropertyInfo property = entity.GetType().GetProperty( Property );
            object otherEntity = null;

            if ( property == null || Type == ReferenceType.Null )
            {
                return;
            }

            //
            // Find the referenced entity based on the reference type.
            //
            if ( Type == ReferenceType.Guid )
            {
                otherEntity = helper.GetExistingEntity( EntityType, helper.FindMappedGuid( new Guid( ( string ) Data ) ) );
            }
            else if ( Type == ReferenceType.EntityType )
            {
                otherEntity = new EntityTypeService( helper.RockContext ).Queryable().Where( e => e.Name == ( string ) Data ).FirstOrDefault();
            }
            else if ( Type == ReferenceType.FieldType )
            {
                otherEntity = new FieldTypeService( helper.RockContext ).Queryable().Where( f => f.Class == ( string ) Data ).FirstOrDefault();
            }
            else if ( Type == ReferenceType.UserDefined )
            {
                otherEntity = helper.GetUserDefinedValue( ( string ) Data );
            }
            else
            {
                throw new Exception( string.Format( "Don't know how to handle reference type {0}.", Type ) );
            }

            //
            // If we found an entity then get its Id number and store that.
            //
            if ( otherEntity != null )
            {
                property.SetValue( entity, EntityCoder.ChangeType( property.PropertyType, otherEntity.GetPropertyValue( "Id" ) ) );
            }
        }

        #endregion
    }

    /// <summary>
    /// The type of reference that is being performed.
    /// </summary>
    public enum ReferenceType
    {
        /// <summary>
        /// A null value, no target entity exists or should be created.
        /// </summary>
        Null = 0,

        /// <summary>
        /// The reference is being performed by a Guid, which is contained in the Data.
        /// </summary>
        Guid = 1,

        /// <summary>
        /// The reference is to an EntityType, whose name is contained in the Data.
        /// </summary>
        EntityType = 2,

        /// <summary>
        /// The reference is to a FieldType, whose class name is contained in the Data.
        /// </summary>
        FieldType = 3,

        /// <summary>
        /// The user will have defined the target entity.
        /// </summary>
        UserDefined = 4
    }
}
