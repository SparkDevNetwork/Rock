using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

using Rock.Cms;

namespace Rock.Attribute
{
    public static class Helper
    {
        public static bool CreateAttributes( Type type, string entity, int? currentPersonId )
        {
            return CreateAttributes( type, entity, String.Empty, String.Empty, currentPersonId );
        }
        
        public static bool CreateAttributes( Type type, string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool attributesUpdated = false;

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                foreach ( object customAttribute in type.GetCustomAttributes( typeof( Rock.Attribute.PropertyAttribute ), true ) )
                {
                    var blockInstanceProperty = ( Rock.Attribute.PropertyAttribute )customAttribute;
                    attributesUpdated = blockInstanceProperty.UpdateAttribute( 
                        entity, entityQualifierColumn, entityQualifierValue, currentPersonId) || attributesUpdated;
                }
            }

            return attributesUpdated;
        }

        public static void LoadAttributes( Rock.Attribute.IHasAttributes item )
        {
            List<Rock.Models.Core.Attribute> attributes = new List<Rock.Models.Core.Attribute>();
            Dictionary<string, KeyValuePair<string, string>> attributeValues = new Dictionary<string, KeyValuePair<string, string>>();

            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            Type modelType = item.GetType();
            if (item is Rock.Models.IModel)
                modelType = modelType.BaseType;
            string entityType = modelType.FullName;

            foreach ( PropertyInfo propertyInfo in modelType.GetProperties() )
                properties.Add( propertyInfo.Name.ToLower(), propertyInfo );

            Rock.Services.Core.AttributeService attributeService = new Rock.Services.Core.AttributeService();

            foreach ( Rock.Models.Core.Attribute attribute in attributeService.GetByEntity( entityType ) )
            {
                if ( string.IsNullOrEmpty( attribute.EntityQualifierColumn ) ||
                    ( properties.ContainsKey( attribute.EntityQualifierColumn.ToLower() ) &&
                    ( string.IsNullOrEmpty( attribute.EntityQualifierValue ) ||
                    properties[attribute.EntityQualifierColumn.ToLower()].GetValue( item, null ).ToString() == attribute.EntityQualifierValue ) ) )
                {
                    attributes.Add( attribute );
                    attributeValues.Add( attribute.Key, new KeyValuePair<string, string>( attribute.Name, attribute.GetValue( item.Id ) ) );
                }
            }

            item.Attributes = attributes;
            item.AttributeValues = attributeValues;
        }

        public static void SaveAttributeValue( Rock.Attribute.IHasAttributes model, Rock.Models.Core.Attribute attribute, string value, int? personId )
        {
            Rock.Services.Core.AttributeValueService attributeValueService = new Rock.Services.Core.AttributeValueService();

            Rock.Models.Core.AttributeValue attributeValue = attribute.AttributeValues.Where( v => v.EntityId == model.Id ).FirstOrDefault();

            if ( attributeValue == null )
            {
                attributeValue = new Rock.Models.Core.AttributeValue();
                attributeValueService.Add( attributeValue, personId );
            }

            attributeValue.AttributeId = attribute.Id;
            attributeValue.EntityId = model.Id;
            attributeValue.Value = value;

            attributeValueService.Save( attributeValue, personId );

            model.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, value );
        }

    }
}