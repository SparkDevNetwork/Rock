using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Describes a single element of an <see cref="EntityPath"/>.
    /// </summary>
    public class EntityPathComponent
    {
        /// <summary>
        /// The entity at this specific location in the path.
        /// </summary>
        public IEntity Entity { get; private set; }

        /// <summary>
        /// The name of the property used to reach the next location in the path.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Create a new entity path component.
        /// </summary>
        /// <param name="entity">The entity at this specific location in the path.</param>
        /// <param name="propertyName">The name of the property used to reach the next location in the path.</param>
        public EntityPathComponent( IEntity entity, string propertyName )
        {
            Entity = entity;
            PropertyName = propertyName;
        }
    }
}
