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
    public partial class CreditCardTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [DefinedValue] SET
        [IsSystem] = 1,
        [Order] = 0,
        [Name] = 'Visa',
        [Description] = 'Visa Card'
    WHERE [Guid] = 'FC66B5F8-634F-4800-A60D-436964D27B64'
        
    UPDATE [DefinedValue] SET
        [IsSystem] = 1,
        [Order] = 1,
        [Name] = 'MasterCard',
        [Description] = 'Master Card'
    WHERE [Guid] = '6373A4B6-4DCA-4EB6-9ADE-B30E8A7F8621'
        
" );
            AddDefinedValue( "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9", "American Express", "American Express Card", "696A54E3-352C-49FB-88A1-BCDBD81AA9EC" );
            AddDefinedValue( "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9", "Discover", "Discover Card", "4B746601-E9EB-4660-BA13-C0B66B24E248" );
            AddDefinedValue( "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9", "Diner''s Club", "Diner's Club Card", "1A9A4DB9-AFF3-4773-875C-C10346BD1CA7" );
            AddDefinedValue( "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9", "JCB", "Japan Credit Bureau Card", "4DD7F0C2-F6B7-4510-90E6-287ADC25FD05" );

            Sql( @"
    DECLARE @TextFieldTypeId int
    SET @TextFieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

    DECLARE @DefinedTypeEntityTypeId int
    SET @DefinedTypeEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue')

    DECLARE @DefinedTypeId int
    SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9')

    INSERT INTO [Attribute] (
        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
        [Key],[Name],[Description],
        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
        [Guid])
    VALUES(
        1,@TextFieldTypeId,@DefinedTypeEntityTypeId,'DefinedTypeId',CAST(@DefinedTypeId AS varchar),
        'RegExPattern','Regular Expression Pattern','The regular expression pattern to use when evaluating a credit card number to see if it is of a particular type',
        0,1,'',0,1,
        '77B9E2F2-28E7-4F67-B1D6-8514DA8E35CD')

    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '77B9E2F2-28E7-4F67-B1D6-8514DA8E35CD')

    DECLARE @EntityId int

    -- Visa
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'FC66B5F8-634F-4800-A60D-436964D27B64')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
        VALUES(1,@AttributeId,@EntityId,0,'^4[0-9]{12}(?:[0-9]{3})?$','A96D8E14-A6EE-4C0F-AC09-7A84D7234C44')

    -- MasterCard
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6373A4B6-4DCA-4EB6-9ADE-B30E8A7F8621')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
        VALUES(1,@AttributeId,@EntityId,0,'^5[1-5][0-9]{14}$','4FD161F0-1908-4043-A5AE-05457D8B0CC9')

    -- American Express
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '696A54E3-352C-49FB-88A1-BCDBD81AA9EC')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
        VALUES(1,@AttributeId,@EntityId,0,'^3[47][0-9]{13}$','B5F39FBE-DA28-411C-A966-48662139F1FD')

    -- Discover
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '4B746601-E9EB-4660-BA13-C0B66B24E248')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
        VALUES(1,@AttributeId,@EntityId,0,'^6(?:011|5[0-9]{2})[0-9]{12}$','1491860F-19BF-4C48-8C7A-5CE72F6302E4')

    -- Diner's Club
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '1A9A4DB9-AFF3-4773-875C-C10346BD1CA7')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
        VALUES(1,@AttributeId,@EntityId,0,'^3(?:0[0-5]|[68][0-9])[0-9]{11}$','01B7230C-F0F4-4581-BF44-C24455F10063')

    -- JCB
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '4DD7F0C2-F6B7-4510-90E6-287ADC25FD05')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
        VALUES(1,@AttributeId,@EntityId,0,'^(?:2131|1800|35\d{3})\d{11}$','E88B7F29-11EA-4EC7-BA21-86409824D72B')

" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "77B9E2F2-28E7-4F67-B1D6-8514DA8E35CD" );

            DeleteDefinedValue( "696A54E3-352C-49FB-88A1-BCDBD81AA9EC" );
            DeleteDefinedValue( "4B746601-E9EB-4660-BA13-C0B66B24E248" );
            DeleteDefinedValue( "1A9A4DB9-AFF3-4773-875C-C10346BD1CA7" );
            DeleteDefinedValue( "4DD7F0C2-F6B7-4510-90E6-287ADC25FD05" );
        }
    }
}
