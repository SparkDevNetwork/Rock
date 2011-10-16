using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

using Rock.Models;
using Rock.Models.Core;
using Rock.Services.Core;

namespace Rock.Services
{
    public abstract class Service
    {
        #region Attributes

        /// <summary>
        /// Load the attributes associated with a specific instance of a model
        /// </summary>
        /// <param name="model"></param>
        public void LoadAttributes( ModelWithAttributes model )
        {
            List<Rock.Models.Core.Attribute> attributes = new List<Rock.Models.Core.Attribute>();
            Dictionary<string, KeyValuePair<string, string>> attributeValues = new Dictionary<string, KeyValuePair<string, string>>();

            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            Type modelType = model.GetType().BaseType;
            string entityType = modelType.FullName;

            foreach ( PropertyInfo propertyInfo in modelType.GetProperties() )
                properties.Add( propertyInfo.Name.ToLower(), propertyInfo );

            AttributeService attributeService = new AttributeService();

            foreach ( Rock.Models.Core.Attribute attribute in attributeService.GetAttributesByEntity( entityType ) )
            {
                if ( string.IsNullOrEmpty( attribute.EntityQualifier ) ||
                    ( properties.ContainsKey( attribute.EntityQualifier.ToLower() ) &&
                    ( !attribute.EntityQualifierId.HasValue ||
                    ( int )properties[attribute.EntityQualifier.ToLower()].GetValue( model, null ) == attribute.EntityQualifierId.Value ) ) )
                {
                    attributes.Add( attribute );
                    attributeValues.Add( attribute.Key, new KeyValuePair<string, string>( attribute.Name, attribute.GetValue( model.Id ) ) );
                }
            }

            model.Attributes = attributes;
            model.AttributeValues = attributeValues;
        }

        /// <summary>
        /// Save new values for a particular attribute and model instance
        /// </summary>
        /// <param name="model">Any ModelWithAttributes model</param>
        /// <param name="attribute">Attribute to update values for</param>
        /// <param name="values">A Dictionary of updated values.  The attribute's fieldtype object's ReadEdit() method returns this dictionary of values</param>
        public void SaveAttributeValue( ModelWithAttributes model, Rock.Models.Core.Attribute attribute, string value, int? personId )
        {
            AttributeValueService attributeValueService = new AttributeValueService();

            AttributeValue attributeValue = attribute.AttributeValues.Where( v => v.EntityId == model.Id ).FirstOrDefault();

            if ( attributeValue == null )
            {
                attributeValue = new AttributeValue();
                attributeValueService.AddAttributeValue( attributeValue );
            }

            attributeValue.AttributeId = attribute.Id;
            attributeValue.EntityId = model.Id;
            attributeValue.Value = value;

            attributeValueService.Save( attributeValue, personId );

            model.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, value );
        }

        #endregion

    }
}