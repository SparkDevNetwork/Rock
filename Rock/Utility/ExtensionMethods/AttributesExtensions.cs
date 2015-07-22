using Rock.Data;

namespace Rock
{
    /// <summary>
    /// Rock.Attribute Extensions
    /// </summary>
    public static class AttributesExtensions
    {
        #region IHasAttributes extensions

        /// <summary>
        /// Loads the attribute.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static void LoadAttributes( this Rock.Attribute.IHasAttributes entity )
        {
            Rock.Attribute.Helper.LoadAttributes( entity );
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( this Rock.Attribute.IHasAttributes entity, RockContext rockContext )
        {
            Rock.Attribute.Helper.LoadAttributes( entity, rockContext );
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValues( this Rock.Attribute.IHasAttributes entity, RockContext rockContext = null )
        {
            Rock.Attribute.Helper.SaveAttributeValues( entity, rockContext );
        }

        /// <summary>
        /// Copies the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="source">The source.</param>
        public static void CopyAttributesFrom( this Rock.Attribute.IHasAttributes entity, Rock.Attribute.IHasAttributes source )
        {
            Rock.Attribute.Helper.CopyAttributes( source, entity );
        }

        #endregion IHasAttributes extensions
    }
}
