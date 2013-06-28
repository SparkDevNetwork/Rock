//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GlobalAttributeDefaultsForOrg : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Organization Address", "The primary mailing address for the organization.", 1, "555 E Main St, Kansas, USA", "E132C358-F28E-45BD-B357-6A2F8B24743A" );
            UpdateAttribute( "410bf494-0714-4e60-afbd-ad65899a12be", "[DefaultValue]", "'Rock Solid Church'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "E132C358-F28E-45BD-B357-6A2F8B24743A" );
            UpdateAttribute( "410bf494-0714-4e60-afbd-ad65899a12be", "[DefaultValue]", "'Our Organization Name'" );
        }

        /// <summary>
        /// Adds a global attribute.
        /// </summary>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        private void AddGlobalAttribute( string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string description, int order, string defaultValue, string guid )
        {
            Sql( string.Format( @"
                 
                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] IS NULL
                AND [Key] = '{2}'
                AND [EntityTypeQualifierColumn] = '{8}'
                AND [EntityTypeQualifierValue] = '{9}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,NULL,'{8}','{9}',
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    "", // no entity; keeps {#} the same as AddEntityAttribute()
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    entityTypeQualifierColumn,
                    entityTypeQualifierValue )
            );
        }

        /// <summary>
        /// Updates a particular Attribute's column value.
        /// </summary>
        /// <param name="guid">the guid of the attribute to udpate.</param>
        /// <param name="safeColumnName">a column name, such as [Description].</param>
        /// <param name="valueQuotedIfNecessary">the value, quoted if necessary, such as 1 or 'foo'.</param>
        private void UpdateAttribute( string guid, string safeColumnName, string valueQuotedIfNecessary )
        {
            Sql( string.Format( @"UPDATE [Attribute] SET {1} = {2} WHERE [Guid] = '{0}'", guid, safeColumnName, valueQuotedIfNecessary ) );
        }
    }
}
