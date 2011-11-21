using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PropertyAttribute : System.Attribute
    {
        public string FieldTypeAssembly { get; set; }
        public string FieldTypeClass { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }

        public PropertyAttribute( string name )
            : this( name, name.Replace(" ", ""), string.Empty, string.Empty, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        public PropertyAttribute( string name, string description )
            : this( name, name.Replace( " ", "" ), description, string.Empty, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        public PropertyAttribute( string name, string description, string defaultValue )
            : this( name, name.Replace( " ", "" ), description, defaultValue, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        public PropertyAttribute( string name, string key, string description, string defaultValue )
            : this( key, name, description, defaultValue, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        public PropertyAttribute( string name, string key, string description, string defaultValue, 
            string fieldTypeAssembly, string fieldTypeClass)
        {
            FieldTypeAssembly = fieldTypeAssembly;
            FieldTypeClass = fieldTypeClass;
            Key = key;
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
        }

        internal bool UpdateAttribute( string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool updated = false;

            Services.Core.AttributeService attributeService = new Services.Core.AttributeService();
            Services.Core.FieldTypeService fieldTypeService = new Services.Core.FieldTypeService();

            Models.Core.Attribute attribute = attributeService.GetAttributesByEntityQualifierAndKey(
                entity, entityQualifierColumn, entityQualifierValue, this.Key );

            if ( attribute == null )
            {
                updated = true;
                attribute = new Models.Core.Attribute();
                attribute.Entity = entity;
                attribute.EntityQualifierColumn = entityQualifierColumn;
                attribute.EntityQualifierValue = entityQualifierValue;
                attribute.Key = this.Key;
                attribute.Order = int.MaxValue;
                attribute.GridColumn = false;
            }
            else
            {
                if ( attribute.Name != this.Name ||
                    attribute.DefaultValue != this.DefaultValue ||
                    attribute.Description != this.Description ||
                    attribute.FieldType.Assembly != this.FieldTypeAssembly ||
                    attribute.FieldType.Class != this.FieldTypeClass )
                    updated = true;
            }

            if ( updated )
            {
                attribute.Name = this.Name;
                attribute.Description = this.Description;
                attribute.DefaultValue = this.DefaultValue;

				// Q for David: Upon first use, immediately after adding a new
				// BlockInstanceProperty in a block, the attribute.FieldType was null
				// so I changed this below to fall into the 'if' block on that case too.
				// Is this correct or should the attribute have had a FieldType? (Nick)
                if ( attribute.FieldType == null || attribute.FieldType.Assembly != this.FieldTypeAssembly ||
                    attribute.FieldType.Class != this.FieldTypeClass )
                {
                    attribute.FieldType = fieldTypeService.Queryable().FirstOrDefault( f =>
                        f.Assembly == this.FieldTypeAssembly &&
                        f.Class == this.FieldTypeClass );
                }

                if ( attribute.Id == 0 )
                    attributeService.Add( attribute, currentPersonId );
                else
                    Rock.Cms.Cached.Attribute.Flush( attribute.Id );

                attributeService.Save( attribute, currentPersonId );

                return true;
            }
            else
                return false;
        }
    }
}