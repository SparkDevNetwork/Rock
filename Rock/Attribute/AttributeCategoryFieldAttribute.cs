//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Field.Types;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeCategoryFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_NAME_KEY = "entityTypeName";
        private const string QUALIFIER_COLUMN_KEY = "qualifierColumn";
        private const string QUALIFIER_VALUE_KEY = "qualifierValue";

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public AttributeCategoryFieldAttribute( string name, string description = "", bool allowMultiple = false,
            string entityTypeName = "", bool required = true, string defaultValue = "", string category = "",
            int order = 0, string key = null ) :
            base( name, description, required, defaultValue, category, order, key,
                ( allowMultiple ? typeof( CategoriesFieldType ).FullName : typeof( CategoryFieldType ).FullName ) )
        {
            FieldConfigurationValues.Add( ENTITY_TYPE_NAME_KEY, new Field.ConfigurationValue( "Rock.Model.Attribute" ) );
            FieldConfigurationValues.Add( QUALIFIER_COLUMN_KEY, new Field.ConfigurationValue( "EntityTypeId" ) );

            var entityType = EntityTypeCache.Read( entityTypeName, false );
            if ( entityType != null )
                FieldConfigurationValues.Add( QUALIFIER_VALUE_KEY, new Field.ConfigurationValue( entityType.Id.ToString() ) );
        }
    }
}
