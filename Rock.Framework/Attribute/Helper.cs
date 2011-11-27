using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Cms;

namespace Rock.Attribute
{
    /// <summary>
    /// Static Helper class for creating, saving, and reading attributes and attribute values of any <see cref="IHasAttributes"/> class
    /// </summary>
    public static class Helper
    {
        /// <param name="type">The type (should be a <see cref="IHasAttributes"/> object.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        public static bool CreateAttributes( Type type, string entity, int? currentPersonId )
        {
            return CreateAttributes( type, entity, String.Empty, String.Empty, currentPersonId );
        }

        /// <summary>
        /// Uses reflection to find any <see cref="PropertyAttribute"/> attributes for the specified type and will create and/or update
        /// a <see cref="Rock.Models.Core.Attribute"/> record for each attribute defined.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes"/> object.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Loads the <see cref="P:IHasAttributes.Attributes"/> and <see cref="P:IHasAttributes.AttributeValues"/> of any <see cref="IHasAttributes"/> object
        /// </summary>
        /// <param name="item">The item.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes item )
        {
            List<Rock.Cms.Cached.Attribute> attributes = new List<Rock.Cms.Cached.Attribute>();
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
                    attributes.Add( Rock.Cms.Cached.Attribute.Read( attribute ));
                    attributeValues.Add( attribute.Key, new KeyValuePair<string, string>( attribute.Name, attribute.GetValue( item.Id ) ) );
                }
            }

            item.Attributes = attributes;
            item.AttributeValues = attributeValues;
        }

        /// <summary>
        /// Saves an attribute value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValue( IHasAttributes model, Rock.Cms.Cached.Attribute attribute, string value, int? personId )
        {
            Rock.Services.Core.AttributeValueService attributeValueService = new Rock.Services.Core.AttributeValueService();
            Rock.Models.Core.AttributeValue attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, model.Id );

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

        /// <summary>
        /// Helper method to generate a list of <![CDATA[<li>]]> tags that contain the appropriate html edit
        /// control returned by each attribute's <see cref="Rock.FieldTypes.IFieldType"/>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="setValue">if set to <c>true</c> set the edit control's value based on the attribute value.</param>
        /// <returns></returns>
        public static List<HtmlGenericControl> GetEditControls( IHasAttributes item, bool setValue )
        {
            List<HtmlGenericControl> controls = new List<HtmlGenericControl>();

            if ( item.Attributes != null )
                foreach ( Rock.Cms.Cached.Attribute attribute in item.Attributes )
                {
                    HtmlGenericControl li = new HtmlGenericControl( "li" );
                    li.ID = string.Format( "attribute-{0}", attribute.Id );
                    li.Attributes.Add( "attribute-key", attribute.Key );
                    li.ClientIDMode = ClientIDMode.AutoID;
                    controls.Add( li );

                    Label lbl = new Label();
                    lbl.ClientIDMode = ClientIDMode.AutoID;
                    lbl.Text = attribute.Name;
                    lbl.AssociatedControlID = string.Format( "attribute-field-{0}", attribute.Id );
                    li.Controls.Add( lbl );

                    Control attributeControl = attribute.CreateControl( item.AttributeValues[attribute.Key].Value, setValue );
                    attributeControl.ID = string.Format( "attribute-field-{0}", attribute.Id );
                    attributeControl.ClientIDMode = ClientIDMode.AutoID;
                    li.Controls.Add( attributeControl );

                    if ( !string.IsNullOrEmpty( attribute.Description ) )
                    {
                        HtmlAnchor a = new HtmlAnchor();
                        a.ClientIDMode = ClientIDMode.AutoID;
                        a.Attributes.Add( "class", "attribute-description tooltip" );
                        a.InnerHtml = "<span>" + attribute.Description + "</span>";

                        li.Controls.Add( a );
                    }
                }

            return controls;
        }

        /// <summary>
        /// Gets the values entered into the edit controls generated by the <see cref="GetEditControls"/> method and sets the <see cref="P:IHasAttributes.AttributeValues"/> of 
        /// the <see cref="IHasAttributes"/> object
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        public static void GetEditValues( Control parentControl, IHasAttributes item )
        {
            if ( item.Attributes != null )
                foreach ( Rock.Cms.Cached.Attribute attribute in item.Attributes )
                {
                    Control control = parentControl.FindControl( string.Format( "attribute-field-{0}", attribute.Id.ToString() ) );
                    if ( control != null )
                        item.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, attribute.FieldType.Field.ReadValue( control ) );
                }
        }
    }
}