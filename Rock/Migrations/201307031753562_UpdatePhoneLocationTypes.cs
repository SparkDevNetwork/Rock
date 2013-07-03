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
    public partial class UpdatePhoneLocationTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

    -- Phone Numbers
    UPDATE [DefinedValue] SET 
        [Name] = 'Mobile'
       ,[Description] = 'Mobile/Cell phone number'
    WHERE [Guid] = '407E7E45-7B2E-4FCD-9605-ECB1339F2453'

    UPDATE [DefinedValue] SET 
        [Name] = 'Home'
       ,[Description] = 'Home phone number'
    WHERE [Guid] = 'AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303'
    
    IF EXISTS (SELECT [ID] FROM [DefinedValue] WHERE [Guid] = '2CC66D5A-F61C-4B74-9AF9-590A9847C13C')
    BEGIN
	    UPDATE [DefinedValue] SET
		     [Name] = 'Business'
		    ,[Description] = 'Business phone number'
	    WHERE [Guid] = '2CC66D5A-F61C-4B74-9AF9-590A9847C13C'
    END
    ELSE
    BEGIN
	    INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
	    VALUES (1, (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '8345DD45-73C6-4F5E-BEBD-B77FC83F18FD'), 
		    2, 'Business', 'Business phone number', '2CC66D5A-F61C-4B74-9AF9-590A9847C13C')
    END

    -- Update any group locations using 'Business' to use 'Office'
    UPDATE GL
	    SET [GroupLocationTypeValueId] = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'E071472A-F805-4FC4-917A-D5E3C095C35C' )
    FROM [GroupLocation] GL
    INNER JOIN [DefinedValue] OV
	    ON OV.[Id] = GL.[GroupLocationTypeValueId]
	    AND OV.[Guid] = 'C89D123C-8645-4B96-8C71-6C87B5A96525'

    -- Delete 'Business' and 'Sports Field'
    DELETE [DefinedValue]
    WHERE [Guid] IN ('C89D123C-8645-4B96-8C71-6C87B5A96525', 'F560DC25-E964-46C4-8CEF-0E67BB922163')

    -- Update 'Office' to 'Work'
    UPDATE [DefinedValue] SET 
        [Name] = 'Work'
       ,[Description] = 'Work address'
    WHERE [Guid] = 'E071472A-F805-4FC4-917A-D5E3C095C35C'
    
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"

        -- Add 'Business' location type
	    INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
	    VALUES (1, (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '2E68D37C-FB7B-4AA5-9E09-3785D52156CB'), 
		    2, 'Business', 'Business', 'C89D123C-8645-4B96-8C71-6C87B5A96525')

        -- Add 'Sports Field' Location type
	    INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
	    VALUES (1, (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '2E68D37C-FB7B-4AA5-9E09-3785D52156CB'), 
		    3, 'Sports Field', 'Sports Field', 'F560DC25-E964-46C4-8CEF-0E67BB922163')
" );
        }
    }
}
