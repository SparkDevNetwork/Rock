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
    /// as per https://trello.com/c/e6UuzpJT/355-add-migration-for-abilitylevel-definedtype-definedvalues-and-person-attribute
    /// </summary>
    public partial class AddAbilityLevels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Ability Level DefinedType
            AddDefinedType( "Check-in", "Ability Level", "The ability levels of children (used with children''s check-in).", "7BEEF4D4-0860-4913-9A3D-857634D1BF7C" );
            AddDefinedValue( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C", "Infant", "The child is unable to crawl yet.", "C4550426-ED87-4CB0-957E-C6E0BC96080F" );
            AddDefinedValue( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C", "Crawler", "The child is able to crawl.", "F78D64D3-6BA1-4ECA-A9EC-058FBDF8E586" );
            AddDefinedValue( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C", "Walking Confidently", "The child is able to walk easily and confidently.", "06B60F5F-D566-4BE2-8A33-DAE896562136" );
            AddDefinedValue( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C", "Walking Confidently and Potty Trained", "The child is also now potty trained.", "E6905502-4C23-4879-A60F-8C4CEB3EE2E9" );

            // Ability Level Person attribute which is of DefinedType 'AbilityLevel'
            AddEntityAttributeForDefinedType( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C", "Rock.Model.Person", "Ability Level", "Check-in", "The ability level of the child (used with children''s check-in).", 0, "4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A" );
        }

        /// <summary>
        /// Add an Entity Attribute that is of type DefinedType AND for a particular DefinedType (where the values are obtained).
        /// For example, the AbilityLevel attribute on a person has a FieldType of DefinedType and it pulls it's values from 
        /// a particular DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The guid of the DefinedType from where the eventual values are later chosen.</param>
        /// <param name="entityTypeName">Class name of the entity (eg, Rock.Model.Person).</param>
        /// <param name="name">The friendly name of the attribute (the Key will be derived from the name by removal of spaces)</param>
        /// <param name="category">The guid (preferably, or name if you can't know the guid) of the category to use</param>
        /// <param name="description">Description of the attribute</param>
        /// <param name="order">the ordinal value</param>
        /// <param name="guid"></param>
        private void AddEntityAttributeForDefinedType( string definedTypeGuid, string entityTypeName, string name, string category, string description, int order, string guid )
        {
            //EnsureEntityTypeExists( entityTypeName );

            Sql( string.Format( @"
                
                DECLARE @AttributeEntityTypeId int
                SET @AttributeEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Attribute')

                --// TODO add if not exists?
                DECLARE @CategoryId int
                -- quick-dirty check to see if it's a guid
                IF ( LEN( REPLACE( '{0}', '-', '' ) ) = 32 )
                    SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{0}')
                ELSE
                    SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Name] = '{0}' AND EntityTypeId = @AttributeEntityTypeId AND EntityTypeQualifierValue IS NULL)

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{1}')

                DECLARE @DefinedTypeFieldTypeId int
                SET @DefinedTypeFieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = 'BC48720C-3610-4BCF-AE66-D255A17F1CDF')

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{2}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [Key] = '{3}'
                    AND [FieldTypeId] = @DefinedTypeFieldTypeId

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1, @DefinedTypeFieldTypeId, @EntityTypeid, '', '',
                    '{3}', '{4}', '{5}',
                    {6}, 0, @DefinedTypeId, 0, 0,
                    '{7}')
",
                    category,
                    entityTypeName,
                    definedTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    guid )
            );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Ability Level person attribute
            DeleteAttribute( "4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A" );

            // Ability Level
            DeleteDefinedType( "7BEEF4D4-0860-4913-9A3D-857634D1BF7C" );
        }
    }
}
