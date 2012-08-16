//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

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
        public static bool UpdateAttributes( Type type, string entity, int? currentPersonId )
        {
            return UpdateAttributes( type, entity, String.Empty, String.Empty, currentPersonId );
        }

        /// <summary>
        /// Uses reflection to find any <see cref="PropertyAttribute"/> attributes for the specified type and will create and/or update
        /// a <see cref="Rock.Core.Attribute"/> record for each attribute defined.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes"/> object.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        public static bool UpdateAttributes( Type type, string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool attributesUpdated = false;

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                List<string> existingKeys = new List<string>();

                foreach ( object customAttribute in type.GetCustomAttributes( typeof( Rock.Attribute.PropertyAttribute ), true ) )
                {
                    var blockInstanceProperty = ( Rock.Attribute.PropertyAttribute )customAttribute;
                    attributesUpdated = UpdateAttribute( blockInstanceProperty, entity, entityQualifierColumn, entityQualifierValue, currentPersonId) || attributesUpdated;

                    existingKeys.Add( blockInstanceProperty.Key );
                }

                Core.AttributeService attributeService = new Core.AttributeService();
                foreach( var a in attributeService.GetAttributesByEntityQualifier(entity, entityQualifierColumn, entityQualifierValue).ToList())
                    if ( !existingKeys.Contains( a.Key ) )
                    {
                        attributeService.Delete( a, currentPersonId );
                        attributeService.Save( a, currentPersonId );
                    }
            }

            return attributesUpdated;
        }

        /// <summary>
        /// Adds or Updates a <see cref="Rock.Core.Attribute"/> item for the attribute.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        private static bool UpdateAttribute( Rock.Attribute.PropertyAttribute property, string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool updated = false;

            Core.AttributeService attributeService = new Core.AttributeService();
            Core.FieldTypeService fieldTypeService = new Core.FieldTypeService();

            // Look for an existing attribute record based on the entity, entityQualifierColumn and entityQualifierValue
            Core.Attribute attribute = attributeService.GetByEntityAndEntityQualifierColumnAndEntityQualifierValueAndKey(
                entity, entityQualifierColumn, entityQualifierValue, property.Key );

            if ( attribute == null )
            {
                // If an existing attribute record doesn't exist, create a new one
                updated = true;
                attribute = new Core.Attribute();
                attribute.Entity = entity;
                attribute.EntityQualifierColumn = entityQualifierColumn;
                attribute.EntityQualifierValue = entityQualifierValue;
                attribute.Key = property.Key;
                attribute.IsGridColumn = false;
            }
            else
            {
                // Check to see if the existing attribute record needs to be updated
                if ( attribute.Name != property.Name ||
                    attribute.Category != property.Category ||
                    attribute.DefaultValue != property.DefaultValue ||
                    attribute.Description != property.Description ||
                    attribute.Order != property.Order ||
                    attribute.FieldType.Assembly != property.FieldTypeAssembly ||
                    attribute.FieldType.Class != property.FieldTypeClass ||
                    attribute.IsRequired != property.IsRequired )
                    updated = true;
            }

            if ( updated )
            {
                // Update the attribute
                attribute.Name = property.Name;
                attribute.Category = property.Category;
                attribute.Description = property.Description;
                attribute.DefaultValue = property.DefaultValue;
                attribute.Order = property.Order;
                attribute.IsRequired = property.IsRequired;

                // Try to set the field type by searching for an existing field type with the same assembly and class name
                if ( attribute.FieldType == null || attribute.FieldType.Assembly != property.FieldTypeAssembly ||
                    attribute.FieldType.Class != property.FieldTypeClass )
                {
                    attribute.FieldType = fieldTypeService.Queryable().FirstOrDefault( f =>
                        f.Assembly == property.FieldTypeAssembly &&
                        f.Class == property.FieldTypeClass );
                }

                // If this is a new attribute, add it, otherwise remove the exiting one from the cache
                if ( attribute.Id == 0 )
                    attributeService.Add( attribute, currentPersonId );
                else
                    Rock.Web.Cache.Attribute.Flush( attribute.Id );

                attributeService.Save( attribute, currentPersonId );

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Loads the <see cref="P:IHasAttributes.Attributes"/> and <see cref="P:IHasAttributes.AttributeValues"/> of any <see cref="IHasAttributes"/> object
        /// </summary>
        /// <param name="item">The item.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes item )
        {
            var attributes = new SortedDictionary<string, List<Web.Cache.Attribute>>();
            var attributeValues = new Dictionary<string, KeyValuePair<string, List<Rock.Web.Cache.AttributeValue>>>();

            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            Type entityType = item.GetType();
            if ( entityType.Namespace == "System.Data.Entity.DynamicProxies" )
                entityType = entityType.BaseType;

            foreach ( PropertyInfo propertyInfo in entityType.GetProperties() )
                properties.Add( propertyInfo.Name.ToLower(), propertyInfo );

            Rock.Core.AttributeService attributeService = new Rock.Core.AttributeService();
            Rock.Core.AttributeValueService attributeValueService = new Rock.Core.AttributeValueService();

            foreach ( Rock.Core.Attribute attribute in attributeService.GetByEntity( entityType.FullName ) )
            {
                if ( string.IsNullOrEmpty( attribute.EntityQualifierColumn ) ||
                    ( properties.ContainsKey( attribute.EntityQualifierColumn.ToLower() ) &&
                    ( string.IsNullOrEmpty( attribute.EntityQualifierValue ) ||
                    properties[attribute.EntityQualifierColumn.ToLower()].GetValue( item, null ).ToString() == attribute.EntityQualifierValue ) ) )
                {
                    Rock.Web.Cache.Attribute cachedAttribute = Rock.Web.Cache.Attribute.Read(attribute);

                    if ( !attributes.ContainsKey( cachedAttribute.Category ) )
                        attributes.Add( cachedAttribute.Category, new List<Web.Cache.Attribute>() );

                    attributes[cachedAttribute.Category].Add( cachedAttribute );
                    attributeValues.Add( attribute.Key, new KeyValuePair<string, List<Rock.Web.Cache.AttributeValue>>( attribute.Name, GetValues( attributeValueService, attribute, item.Id ) ) );
                }
            }

            item.Attributes = attributes;
            item.AttributeValues = attributeValues;
        }

        public static List<Rock.Web.Cache.AttributeValue> GetValues( Rock.Core.Attribute attribute, int entityId )
        {
            Rock.Core.AttributeValueService attributeValueService = new Rock.Core.AttributeValueService();
            return GetValues( attributeValueService, attribute, entityId );
        }

        public static List<Rock.Web.Cache.AttributeValue> GetValues( Rock.Core.AttributeValueService attributeValueService, Rock.Core.Attribute attribute, int entityId )
        {
            var values = new List<Rock.Web.Cache.AttributeValue>();

            foreach ( var value in attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId ) )
                values.Add( new Rock.Web.Cache.AttributeValue( value ) );

            if ( values.Count == 0 )
            {
                var value = new Rock.Web.Cache.AttributeValue();
                value.AttributeId = attribute.Id;
                value.Value = attribute.DefaultValue;
                values.Add( value );
            }

            return values;
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValues( IHasAttributes model, int? personId )
        {
            foreach ( var category in model.Attributes )
                foreach ( var attribute in category.Value )
                    SaveAttributeValues( model, attribute, model.AttributeValues[attribute.Key].Value, personId );
        }

        /// <summary>
        /// Saves an attribute value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValue( IHasAttributes model, Rock.Web.Cache.Attribute attribute, string newValue, int? personId )
        {
            Core.AttributeValueService attributeValueService = new Core.AttributeValueService();

            var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, model.Id ).FirstOrDefault();
            if ( attributeValue == null )
            {
                attributeValue = new Rock.Core.AttributeValue();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.EntityId = model.Id;
                attributeValue.Order = 0;
                attributeValueService.Add( attributeValue, personId );
            }

            attributeValue.Value = newValue;

            attributeValueService.Save( attributeValue, personId );

            model.AttributeValues[attribute.Key] =
                new KeyValuePair<string, List<Rock.Web.Cache.AttributeValue>>(
                    attribute.Name,
                    new List<Rock.Web.Cache.AttributeValue>() { new Rock.Web.Cache.AttributeValue( attributeValue ) } );

        }

        /// <summary>
        /// Saves an attribute value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValues( IHasAttributes model, Rock.Web.Cache.Attribute attribute, List<Rock.Web.Cache.AttributeValue> newValues, int? personId )
        {
            Core.AttributeValueService attributeValueService = new Core.AttributeValueService();

            var attributeValues = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, model.Id ).ToList();
            int i = 0;

            while ( i < attributeValues.Count || i < newValues.Count )
            {
                Rock.Core.AttributeValue attributeValue;

                if ( i < attributeValues.Count )
                {
                    attributeValue = attributeValues[i];
                }
                else 
                {
                    attributeValue = new Rock.Core.AttributeValue();
                    attributeValue.AttributeId = attribute.Id;
                    attributeValue.EntityId = model.Id;
                    attributeValue.Order = i;
                    attributeValueService.Add( attributeValue, personId );
                }

                if ( i >= newValues.Count )
                    attributeValueService.Delete( attributeValue, personId );
                else
                {
                    if ( attributeValue.Value != newValues[i].Value )
                        attributeValue.Value = newValues[i].Value;
                    newValues[i] = new Web.Cache.AttributeValue( attributeValue );
                }

                attributeValueService.Save( attributeValue, personId );


                i++;
            }

            model.AttributeValues[attribute.Key] = 
                new KeyValuePair<string, List<Rock.Web.Cache.AttributeValue>>( attribute.Name, newValues );
        }

        /// <summary>
        /// Helper method to generate a list of <![CDATA[<li>]]> tags that contain the appropriate html edit
        /// control returned by each attribute's <see cref="Rock.Field.IFieldType"/>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="setValue">if set to <c>true</c> set the edit control's value based on the attribute value.</param>
        /// <returns></returns>
        public static List<HtmlGenericControl> GetEditControls( IHasAttributes item, bool setValue )
        {
            List<HtmlGenericControl> controls = new List<HtmlGenericControl>();

            if ( item.Attributes != null )
                foreach(var category in item.Attributes)
                {
                    HtmlGenericControl fieldSet = new HtmlGenericControl("fieldset");
                    controls.Add(fieldSet);

                    HtmlGenericControl legend = new HtmlGenericControl("legend");
                    fieldSet.Controls.Add(legend);
                    legend.InnerText = category.Key.Trim() != string.Empty ? category.Key.Trim() : "Attributes";

                    foreach ( Rock.Web.Cache.Attribute attribute in category.Value )
                    {
                        HtmlGenericControl div = new HtmlGenericControl( "div" );
                        fieldSet.Controls.Add( div );
                        div.ID = string.Format( "attribute_{0}", attribute.Id );
                        div.AddCssClass( "control-group" );
                        if ( attribute.IsRequired )
                            div.AddCssClass( "required" );
                        div.Attributes.Add( "attribute-key", attribute.Key );
                        div.ClientIDMode = ClientIDMode.AutoID;

                        Label lbl = new Label();
                        div.Controls.Add( lbl );
                        lbl.ClientIDMode = ClientIDMode.AutoID;
                        lbl.AddCssClass( "control-label" );
                        lbl.Text = attribute.Name;
                        lbl.AssociatedControlID = string.Format( "attribute_field_{0}", attribute.Id );

                        HtmlGenericControl divControls = new HtmlGenericControl( "div" );
                        div.Controls.Add( divControls );
                        divControls.AddCssClass( "controls" );

                        Control attributeControl = attribute.CreateControl( item.AttributeValues[attribute.Key].Value[0].Value, setValue );
                        divControls.Controls.Add( attributeControl );
                        attributeControl.ID = string.Format( "attribute_field_{0}", attribute.Id );
                        attributeControl.ClientIDMode = ClientIDMode.AutoID;

                        if ( attribute.IsRequired )
                        {
                            RequiredFieldValidator rfv = new RequiredFieldValidator();
                            divControls.Controls.Add( rfv );
                            rfv.CssClass = "help-inline";
                            rfv.ControlToValidate = attributeControl.ID;
                            rfv.ID = string.Format( "attribute_rfv_{0}", attribute.Id );
                            rfv.ErrorMessage = string.Format( "{0} is Required", attribute.Name );
                            rfv.Display = ValidatorDisplay.None;

                            if (!setValue && !rfv.IsValid)
                                div.Attributes.Add( "class", "error" );
                        }

                        if ( !string.IsNullOrEmpty( attribute.Description ) )
                        {
                            HtmlGenericControl helpBlock = new HtmlGenericControl( "div" );
                            divControls.Controls.Add( helpBlock );
                            helpBlock.ClientIDMode = ClientIDMode.AutoID;
                            helpBlock.AddCssClass( "alert alert-info" );
                            helpBlock.InnerHtml = attribute.Description;
                        }
                    }
                }

            return controls;
        }

        /// <summary>
        /// Sets any missing required field error indicators.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        public static void SetErrorIndicators( Control parentControl, IHasAttributes item )
        {
            if ( item.Attributes != null )
                foreach ( var category in item.Attributes )
                    foreach ( var attribute in category.Value )
                    {
                        if ( attribute.IsRequired )
                        {
                            HtmlGenericControl div = parentControl.FindControl( string.Format( "attribute_{0}", attribute.Id ) ) as HtmlGenericControl;
                            RequiredFieldValidator rfv = parentControl.FindControl( string.Format( "attribute_rfv_{0}", attribute.Id ) ) as RequiredFieldValidator;
                            if ( div != null && rfv != null )
                            {
                                if ( rfv.IsValid )
                                    div.RemoveCssClass( "error" );
                                else
                                    div.AddCssClass( "error" );
                            }
                        }
                    }
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
                foreach ( var category in item.Attributes )
                    foreach ( var attribute in category.Value )
                    {
                        Control control = parentControl.FindControl( string.Format( "attribute_field_{0}", attribute.Id.ToString() ) );
                        if ( control != null )
                        {
                            var value = new Rock.Web.Cache.AttributeValue();
                            value.Value = attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                            item.AttributeValues[attribute.Key] = new KeyValuePair<string, List<Rock.Web.Cache.AttributeValue>>( attribute.Name, new List<Rock.Web.Cache.AttributeValue>() { value } );
                        }
                    }
        }
    }
}