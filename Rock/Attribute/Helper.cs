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
using Rock.Constants;

using Rock.Model;
using Rock.Web.UI;

namespace Rock.Attribute
{
    /// <summary>
    /// Static Helper class for creating, saving, and reading attributes and attribute values of any <see cref="IHasAttributes"/> class
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Updates the attributes.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes" /> object.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        public static bool UpdateAttributes( Type type, int? entityTypeId, int? currentPersonId )
        {
            return UpdateAttributes( type, entityTypeId, String.Empty, String.Empty, currentPersonId );
        }

        /// <summary>
        /// Uses reflection to find any <see cref="FieldAttribute" /> attributes for the specified type and will create and/or update
        /// a <see cref="Rock.Model.Attribute" /> record for each attribute defined.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes" /> object.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        public static bool UpdateAttributes( Type type, int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool attributesUpdated = false;

            List<string> existingKeys = new List<string>();

            var blockProperties = new List<FieldAttribute>();

            // If a ContextAwareAttribute exists without an EntityType defined, add a property attribute to specify the type
            int properties = 0;
            foreach ( var customAttribute in type.GetCustomAttributes( typeof( ContextAwareAttribute ), true ) )
            {
                var contextAttribute = (ContextAwareAttribute)customAttribute;
                if ( String.IsNullOrWhiteSpace( contextAttribute.EntityType ) )
                {
                    string propertyKeyName = string.Format( "ContextEntityType{0}", properties > 0 ? properties.ToString() : "" );
                    properties++;

                    blockProperties.Add( new TextFieldAttribute( "Context Entity Type", "Context Entity Type", false, contextAttribute.DefaultParameterName, "Filter", 0, propertyKeyName ) );
                }
            }

            // Add any property attributes that were defined for the block
            foreach ( var customAttribute in type.GetCustomAttributes( typeof( FieldAttribute ), true ) )
            {
                blockProperties.Add( (FieldAttribute)customAttribute );
            }

            // Create any attributes that need to be created
            var attributeService = new Model.AttributeService();

            if ( blockProperties.Count > 0 )
            {
                var attributeQualifierService = new Model.AttributeQualifierService();
                var fieldTypeService = new Model.FieldTypeService();
                var categoryService = new Model.CategoryService();

                foreach ( var blockProperty in blockProperties )
                {
                    attributesUpdated = UpdateAttribute( attributeService, attributeQualifierService, fieldTypeService, categoryService,
                        blockProperty, entityTypeId, entityQualifierColumn, entityQualifierValue, currentPersonId ) || attributesUpdated;
                    existingKeys.Add( blockProperty.Key );
                }
            }

            // Remove any old attributes
            foreach ( var a in attributeService.Get( entityTypeId, entityQualifierColumn, entityQualifierValue ).ToList() )
            {
                if ( !existingKeys.Contains( a.Key ) )
                {
                    attributeService.Delete( a, currentPersonId );
                    attributeService.Save( a, currentPersonId );
                }
            }

            return attributesUpdated;
        }

        /// <summary>
        /// Adds or Updates a <see cref="Rock.Model.Attribute" /> item for the attribute.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        private static bool UpdateAttribute( Model.AttributeService attributeService, Model.AttributeQualifierService attributeQualifierService, Model.FieldTypeService fieldTypeService, Model.CategoryService categoryService,
            FieldAttribute property, int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool updated = false;

            var propertyCategories = property.Category.SplitDelimitedValues( false ).ToList();

            // Look for an existing attribute record based on the entity, entityQualifierColumn and entityQualifierValue
            Model.Attribute attribute = attributeService.Get(
                entityTypeId, entityQualifierColumn, entityQualifierValue, property.Key );

            if ( attribute == null )
            {
                // If an existing attribute record doesn't exist, create a new one
                updated = true;
                attribute = new Model.Attribute();
                attribute.EntityTypeId = entityTypeId;
                attribute.EntityTypeQualifierColumn = entityQualifierColumn;
                attribute.EntityTypeQualifierValue = entityQualifierValue;
                attribute.Key = property.Key;
                attribute.IsGridColumn = false;
            }
            else
            {
                // Check to see if the existing attribute record needs to be updated
                if ( attribute.Name != property.Name ||
                    attribute.DefaultValue != property.DefaultValue ||
                    attribute.Description != property.Description ||
                    attribute.Order != property.Order ||
                    attribute.FieldType.Assembly != property.FieldTypeAssembly ||
                    attribute.FieldType.Class != property.FieldTypeClass ||
                    attribute.IsRequired != property.IsRequired )
                {
                    updated = true;
                }

                // Check category
                else if ( attribute.Categories.Select( c => c.Name ).Except( propertyCategories ).Any() ||
                    propertyCategories.Except( attribute.Categories.Select( c => c.Name ) ).Any() )
                {
                    updated = true;
                }

                // Check the qualifier values
                else if ( attribute.AttributeQualifiers.Select( q => q.Key ).Except( property.FieldConfigurationValues.Select( c => c.Key ) ).Any() ||
                    property.FieldConfigurationValues.Select( c => c.Key ).Except( attribute.AttributeQualifiers.Select( q => q.Key ) ).Any() )
                {
                    updated = true;
                }
                else
                {
                    foreach ( var attributeQualifier in attribute.AttributeQualifiers )
                    {
                        if ( !property.FieldConfigurationValues.ContainsKey( attributeQualifier.Key ) ||
                            property.FieldConfigurationValues[attributeQualifier.Key].Value != attributeQualifier.Value )
                        {
                            updated = true;
                            break;
                        }
                    }
                }

            }

            if ( updated )
            {
                // Update the attribute
                attribute.Name = property.Name;
                attribute.Description = property.Description;
                attribute.DefaultValue = property.DefaultValue;
                attribute.Order = property.Order;
                attribute.IsRequired = property.IsRequired;

                attribute.Categories.Clear();
                if ( propertyCategories.Any() )
                {
                    foreach ( string propertyCategory in propertyCategories )
                    {
                        int attributeEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
                        var category = categoryService.Get( propertyCategory, attributeEntityTypeId, "EntityTypeId", entityTypeId.ToString() ).FirstOrDefault();
                        if ( category == null )
                        {
                            category = new Category();
                            category.Name = propertyCategory;
                            category.EntityTypeId = attributeEntityTypeId;
                            category.EntityTypeQualifierColumn = "EntityTypeId";
                            category.EntityTypeQualifierValue = entityTypeId.ToString();
                        }
                        attribute.Categories.Add( category );
                    }
                }

                foreach ( var qualifier in attribute.AttributeQualifiers.ToList() )
                {
                    attributeQualifierService.Delete( qualifier, currentPersonId );
                }
                attribute.AttributeQualifiers.Clear();

                foreach ( var configValue in property.FieldConfigurationValues )
                {
                    var qualifier = new Model.AttributeQualifier();
                    qualifier.Key = configValue.Key;
                    qualifier.Value = configValue.Value.Value;
                    attribute.AttributeQualifiers.Add( qualifier );
                }

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
                    Rock.Web.Cache.AttributeCache.Flush( attribute.Id );

                attributeService.Save( attribute, currentPersonId );

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the <see cref="P:IHasAttributes.Attributes"/> and <see cref="P:IHasAttributes.AttributeValues"/> of any <see cref="IHasAttributes"/> object
        /// </summary>
        /// <param name="entity">The item.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes entity )
        {
            var attributes = new List<Rock.Web.Cache.AttributeCache>();
            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            Type entityType = entity.GetType();
            if ( entityType.Namespace == "System.Data.Entity.DynamicProxies" )
                entityType = entityType.BaseType;

            foreach ( PropertyInfo propertyInfo in entityType.GetProperties() )
                properties.Add( propertyInfo.Name.ToLower(), propertyInfo );

            Rock.Model.AttributeService attributeService = new Rock.Model.AttributeService();
            Rock.Model.AttributeValueService attributeValueService = new Rock.Model.AttributeValueService();

            // Get all the attributes that apply to this entity type and this entity's properties match any attribute qualifiers
            int? entityTypeId = Rock.Web.Cache.EntityTypeCache.Read( entityType ).Id;
            foreach ( Rock.Model.Attribute attribute in attributeService.GetByEntityTypeId( entityTypeId ) )
            {
                if ( string.IsNullOrEmpty( attribute.EntityTypeQualifierColumn ) ||
                    ( properties.ContainsKey( attribute.EntityTypeQualifierColumn.ToLower() ) &&
                    ( string.IsNullOrEmpty( attribute.EntityTypeQualifierValue ) ||
                    properties[attribute.EntityTypeQualifierColumn.ToLower()].GetValue( entity, null ).ToString() == attribute.EntityTypeQualifierValue ) ) )
                {
                    attributes.Add( Rock.Web.Cache.AttributeCache.Read( attribute ) );
                }
            }

            var attributeValues = new Dictionary<string, List<Rock.Model.AttributeValue>>();

            if ( attributes.Any() )
            {
                foreach ( var attribute in attributes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                {
                    // Add a placeholder for this item's value for each attribute
                    attributeValues.Add( attribute.Key, new List<Rock.Model.AttributeValue>() );
                }

                // Read this item's value(s) for each attribute 
                List<int> attributeIds = attributes.Select( a => a.Id ).ToList();
                foreach ( var attributeValue in attributeValueService.Queryable( "Attribute" )
                    .Where( v => v.EntityId == entity.Id && attributeIds.Contains( v.AttributeId ) ) )
                {
                    attributeValues[attributeValue.Attribute.Key].Add( attributeValue.Clone( false ) as Rock.Model.AttributeValue );
                }

                // Look for any attributes that don't have a value and create a default value entry
                foreach ( var attribute in attributes )
                {
                    if ( attributeValues[attribute.Key].Count == 0 )
                    {
                        var attributeValue = new Rock.Model.AttributeValue();
                        attributeValue.AttributeId = attribute.Id;
                        if ( entity.AttributeValueDefaults != null && entity.AttributeValueDefaults.ContainsKey( attribute.Name ) )
                        {
                            attributeValue.Value = entity.AttributeValueDefaults[attribute.Name];
                        }
                        else
                        {
                            attributeValue.Value = attribute.DefaultValue;
                        }
                        attributeValues[attribute.Key].Add( attributeValue );
                    }
                    else
                    {
                        if ( !String.IsNullOrWhiteSpace( attribute.DefaultValue ) )
                        {
                            foreach ( var value in attributeValues[attribute.Key] )
                            {
                                if ( String.IsNullOrWhiteSpace( value.Value ) )
                                {
                                    value.Value = attribute.DefaultValue;
                                }

                            }
                        }
                    }
                }
            }

            entity.Attributes = new Dictionary<string, Web.Cache.AttributeCache>();
            attributes.ForEach( a => entity.Attributes.Add( a.Key, a ) );

            entity.AttributeValues = attributeValues;
        }

        /// <summary>
        /// Gets the attribute categories.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="allowMultiple">if set to <c>true</c> returns the attribute in each of it's categories, if false, only returns attribut in first category.</param>
        /// <returns></returns>
        public static List<AttributeCategory> GetAttributeCategories( IHasAttributes entity, bool allowMultiple = false )
        {
            var attributes = entity.Attributes.Select( a => a.Value ).ToList();
            return GetAttributeCategories( attributes, allowMultiple );
        }

        /// <summary>
        /// Gets attributes grouped by category
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="allowMultiple">if set to <c>true</c> returns the attribute in each of it's categories, if false, only returns attribut in first category.</param>
        /// <returns></returns>
        public static List<AttributeCategory> GetAttributeCategories( List<Rock.Web.Cache.AttributeCache> attributes, bool allowMultiple = false )
        {
            var attributeCategories = new List<AttributeCategory>();
            foreach ( var attribute in attributes )
            {
                if ( attribute.Categories.Any() )
                {
                    foreach ( var category in attribute.Categories.OrderBy( c => c.Name ) )
                    {
                        AddAttributeCategory( attributeCategories, category, attribute );
                        if (!allowMultiple)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    AddAttributeCategory( attributeCategories, null, attribute );
                }
            }

            return attributeCategories.OrderBy( c => c.CategoryName ).ToList();
        }

        private static void AddAttributeCategory( List<AttributeCategory> attributeCategories, Rock.Web.Cache.CategoryCache category, Rock.Web.Cache.AttributeCache attribute )
        {
            AttributeCategory attributeCategory = null;
            if (category != null)
            {
                attributeCategory = attributeCategories.Where( g => g.Category != null && g.Category.Id == category.Id ).FirstOrDefault();
            }
            else
            {
                attributeCategory = attributeCategories.Where( g => g.Category == null ).FirstOrDefault();
            }

            if ( attributeCategory == null )
            {
                attributeCategory = new AttributeCategory();
                attributeCategory.Category = category;
                attributeCategory.Attributes = new List<Web.Cache.AttributeCache>();
                attributeCategories.Add( attributeCategory );
            }

            attributeCategory.Attributes.Add( attribute );
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValues( IHasAttributes model, int? personId )
        {
            foreach ( var attribute in model.Attributes )
                SaveAttributeValues( model, attribute.Value, model.AttributeValues[attribute.Key], personId );
        }

        /// <summary>
        /// Saves an attribute value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValue( IHasAttributes model, Rock.Web.Cache.AttributeCache attribute, string newValue, int? personId )
        {
            Model.AttributeValueService attributeValueService = new Model.AttributeValueService();

            var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, model.Id ).FirstOrDefault();
            if ( attributeValue == null )
            {
                attributeValue = new Rock.Model.AttributeValue();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.EntityId = model.Id;
                attributeValue.Order = 0;
                attributeValueService.Add( attributeValue, personId );
            }

            attributeValue.Value = newValue;

            attributeValueService.Save( attributeValue, personId );

            model.AttributeValues[attribute.Key] = new List<Rock.Model.AttributeValue>() { attributeValue.Clone() as Rock.Model.AttributeValue };

        }

        /// <summary>
        /// Saves an attribute value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newValues">The new values.</param>
        /// <param name="personId">The person id.</param>
        public static void SaveAttributeValues( IHasAttributes model, Rock.Web.Cache.AttributeCache attribute, List<Rock.Model.AttributeValue> newValues, int? personId )
        {
            Model.AttributeValueService attributeValueService = new Model.AttributeValueService();

            var attributeValues = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, model.Id ).ToList();
            int i = 0;

            while ( i < attributeValues.Count || i < newValues.Count )
            {
                Rock.Model.AttributeValue attributeValue;

                if ( i < attributeValues.Count )
                {
                    attributeValue = attributeValues[i];
                }
                else
                {
                    attributeValue = new Rock.Model.AttributeValue();
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
                    newValues[i] = attributeValue.Clone() as Rock.Model.AttributeValue;
                }

                attributeValueService.Save( attributeValue, personId );

                i++;
            }

            model.AttributeValues[attribute.Key] = newValues;
        }

        /// <summary>
        /// Copies the attributes from one entity to another
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static void CopyAttributes( IHasAttributes source, IHasAttributes target )
        {
            // Copy Attributes
            if ( source.Attributes != null )
            {
                target.Attributes = new Dictionary<string, Web.Cache.AttributeCache>();
                foreach ( var item in source.Attributes )
                {
                    target.Attributes.Add( item.Key, item.Value );
                }
            }
            else
            {
                target.Attributes = null;
            }

            // Copy Attribute Values
            if ( source.AttributeValues != null )
            {
                target.AttributeValues = new Dictionary<string, List<Model.AttributeValue>>();
                foreach ( var item in source.AttributeValues )
                {
                    var list = new List<Model.AttributeValue>();
                    foreach ( var value in item.Value )
                    {
                        var attributeValue = new Model.AttributeValue();
                        attributeValue.IsSystem = value.IsSystem;
                        attributeValue.AttributeId = value.AttributeId;
                        attributeValue.EntityId = value.EntityId;
                        attributeValue.Order = value.Order;
                        attributeValue.Value = value.Value;
                        attributeValue.Id = value.Id;
                        attributeValue.Guid = value.Guid;
                        list.Add( attributeValue );
                    }
                    target.AttributeValues.Add( item.Key, list );
                }
            }
            else
            {
                target.AttributeValues = null;
            }

        }

        /// <summary>
        /// Adds edit controls for each of the item's attributes
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parentControl"></param>
        /// <param name="setValue"></param>
        public static void AddEditControls( IHasAttributes item, Control parentControl, bool setValue )
        {
            AddEditControls( item, parentControl, setValue, new List<string>() );
        }

        /// <summary>
        /// Adds edit controls for each of the item's attributes
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parentControl"></param>
        /// <param name="setValue"></param>
        /// <param name="exclude">List of attribute names not to render</param>
        public static void AddEditControls( IHasAttributes item, Control parentControl, bool setValue, List<string> exclude )
        {
            if ( item.Attributes != null )
            {
                foreach(var attributeCategory in GetAttributeCategories(item ))
                {
                    AddEditControls(
                        attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty,
                        attributeCategory.Attributes.Select( a => a.Key ).ToList(),
                        item, parentControl, setValue, exclude);
                }
            }
        }

        private static void AddEditControls( string category, List<string> attributeKeys, IHasAttributes item, Control parentControl, bool setValue, List<string> exclude )
        {
            HtmlGenericControl fieldSet = new HtmlGenericControl( "fieldset" );
            parentControl.Controls.Add( fieldSet );
            fieldSet.Controls.Clear();

            if ( !string.IsNullOrEmpty( category ) )
            {
                HtmlGenericControl legend = new HtmlGenericControl( "legend" );
                fieldSet.Controls.Add( legend );
                legend.Controls.Clear();
                legend.InnerText = category.Trim();
            }

            foreach ( string key in attributeKeys )
            {
                var attribute = item.Attributes[key];

                if ( !exclude.Contains( attribute.Name ) )
                {
                    HtmlGenericControl div = new HtmlGenericControl( "div" );
                    fieldSet.Controls.Add( div );
                    div.Controls.Clear();

                    div.ID = string.Format( "attribute_{0}", attribute.Id );
                    div.AddCssClass( "control-group" );
                    if ( attribute.IsRequired )
                        div.AddCssClass( "required" );
                    div.Attributes.Add( "attribute-key", attribute.Key );
                    div.ClientIDMode = ClientIDMode.AutoID;

                    Control attributeControl = attribute.CreateControl( item.AttributeValues[attribute.Key][0].Value, setValue, true );
                    if ( !( attributeControl is CheckBox ) )
                    {
                        HtmlGenericControl labelDiv = new HtmlGenericControl( "div" );
                        div.Controls.Add( labelDiv );
                        labelDiv.AddCssClass( "control-label" );
                        labelDiv.ClientIDMode = ClientIDMode.AutoID;

                        Literal lbl = new Literal();
                        labelDiv.Controls.Add( lbl );
                        lbl.Text = attribute.Name;

                        if ( !string.IsNullOrEmpty( attribute.Description ) )
                        {
                            var HelpBlock = new Rock.Web.UI.Controls.HelpBlock();
                            labelDiv.Controls.Add( HelpBlock );
                            HelpBlock.Text = attribute.Description;
                        }
                    }

                    HtmlGenericControl divControls = new HtmlGenericControl( "div" );
                    div.Controls.Add( divControls );
                    divControls.AddCssClass( "controls" );
                    divControls.Controls.Clear();

                    divControls.Controls.Add( attributeControl );

                    if ( attributeControl is CheckBox )
                    {
                        ( attributeControl as CheckBox ).Text = attribute.Name;
                        if ( !string.IsNullOrEmpty( attribute.Description ) )
                        {
                            var HelpBlock = new Rock.Web.UI.Controls.HelpBlock();
                            divControls.Controls.Add( HelpBlock );
                            HelpBlock.Text = attribute.Description;
                        }
                    }

                    if ( attribute.IsRequired && ( attributeControl is TextBox ) )
                    {
                        RequiredFieldValidator rfv = new RequiredFieldValidator();
                        divControls.Controls.Add( rfv );
                        rfv.CssClass = "validation-error help-inline";
                        rfv.ControlToValidate = attributeControl.ID;
                        rfv.ID = string.Format( "attribute_rfv_{0}", attribute.Id );
                        rfv.ErrorMessage = string.Format( "{0} is Required", attribute.Name );
                        rfv.Display = ValidatorDisplay.None;

                        if ( !setValue && !rfv.IsValid )
                            div.Attributes.Add( "class", "error" );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the display HTML.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="exclude">The exclude.</param>
        /// <returns></returns>
        public static void AddDisplayControls( IHasAttributes item, Control parentControl, List<string> exclude = null )
        {
            exclude = exclude ?? new List<string>();
            string result = string.Empty;

            if ( item.Attributes != null )
            {
                foreach ( var attributeCategory in GetAttributeCategories( item ) )
                {
                    HtmlGenericControl header = new HtmlGenericControl( "h4" );

                    string categoryName = attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty;

                    header.InnerText = string.IsNullOrWhiteSpace( categoryName ) ? item.GetType().GetFriendlyTypeName() + " Attributes" : categoryName.Trim();
                    parentControl.Controls.Add( header );

                    HtmlGenericControl dl = new HtmlGenericControl( "dl" );
                    parentControl.Controls.Add( dl );

                    foreach ( var attribute in attributeCategory.Attributes )
                    {
                        if ( !exclude.Contains( attribute.Name ) )
                        {
                            HtmlGenericControl dt = new HtmlGenericControl( "dt" );
                            dt.InnerText = attribute.Name;
                            dl.Controls.Add( dt );

                            HtmlGenericControl dd = new HtmlGenericControl( "dd" );
                            string value = item.AttributeValues[attribute.Key][0].Value;
                            string controlHtml = attribute.FieldType.Field.FormatValue( parentControl, value, attribute.QualifierValues, false );
                            if ( string.IsNullOrWhiteSpace( controlHtml ) )
                            {
                                controlHtml = None.TextHtml;
                            }

                            dd.InnerHtml = controlHtml;
                            dl.Controls.Add( dd );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets any missing required field error indicators.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        public static void SetErrorIndicators( Control parentControl, IHasAttributes item )
        {
            if ( item.Attributes != null )
                foreach ( var attribute in item.Attributes )
                {
                    if ( attribute.Value.IsRequired )
                    {
                        HtmlGenericControl div = parentControl.FindControl( string.Format( "attribute_{0}", attribute.Value.Id ) ) as HtmlGenericControl;
                        RequiredFieldValidator rfv = parentControl.FindControl( string.Format( "attribute_rfv_{0}", attribute.Value.Id ) ) as RequiredFieldValidator;
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
        /// Gets the edit values.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        public static void GetEditValues( Control parentControl, IHasAttributes item )
        {
            if ( item.Attributes != null )
                foreach ( var attribute in item.Attributes )
                {
                    Control control = parentControl.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id.ToString() ) );
                    if ( control != null )
                    {
                        var value = new Rock.Model.AttributeValue();
                        value.Value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        item.AttributeValues[attribute.Key] = new List<Rock.Model.AttributeValue>() { value };
                    }
                }
        }


    }

    /// <summary>
    /// Attributes group by category
    /// </summary>
    public class AttributeCategory
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public Rock.Web.Cache.CategoryCache Category { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <value>
        /// The name of the category.
        /// </value>
        public string CategoryName
        {
            get { return Category != null ? Category.Name : string.Empty; }
        }
    }

}