using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.CommandCenter.Migrations
{
    [MigrationNumber( 2, "1.0.8" )]
    public class AddSystemData : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Service Recordings", "", "2BAAF392-2FE6-4D83-B949-122E6B97E5BB" );
            RockMigrationHelper.AddPage( "2BAAF392-2FE6-4D83-B949-122E6B97E5BB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Recording Details", "", "6E7ACDFC-0297-473E-8990-9C96CC49394C" );

            RockMigrationHelper.UpdateBlockType( "Recording Detail", "", "~/Plugins/com_ccvonline/CommandCenter/RecordingDetail.ascx", "CCV > Command Center", "1A054FCC-2E0E-4AD1-BA36-21991DB479AB" );
            RockMigrationHelper.UpdateBlockType( "Recording List", "", "~/Plugins/com_ccvonline/CommandCenter/RecordingList.ascx", "CCV > Command Center", "AF4EB7C5-9121-4765-BEF2-558499BD0D6C" );

            RockMigrationHelper.AddBlock( "2BAAF392-2FE6-4D83-B949-122E6B97E5BB", "", "AF4EB7C5-9121-4765-BEF2-558499BD0D6C", "Recording List", "Main", "", "", 0, "7591B01B-8F22-47E3-BEFB-076338A3F24A" );
            RockMigrationHelper.AddBlock( "6E7ACDFC-0297-473E-8990-9C96CC49394C", "", "1A054FCC-2E0E-4AD1-BA36-21991DB479AB", "Recording Detail", "Main", "", "", 0, "FF6657ED-7A19-4869-A887-32EF70F84EDB" );

            // Attrib for BlockType: com .ccvonline - Command Center - Recording List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "AF4EB7C5-9121-4765-BEF2-558499BD0D6C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "D23CF945-F440-4821-8496-C35035E1C3FE" );

            // Attrib Value for Recording List:Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "7591B01B-8F22-47E3-BEFB-076338A3F24A", "D23CF945-F440-4821-8496-C35035E1C3FE", "6e7acdfc-0297-473e-8990-9c96cc49394c" );

            RockMigrationHelper.UpdateFieldType( "Accounts Field Type", "", "Rock", "Rock.Field.Types.AccountsFieldType", "17033CDD-EF97-4413-A483-7B85A787A87F" );
            RockMigrationHelper.UpdateFieldType( "Category Field Type", "", "Rock", "Rock.Field.Types.CategoryFieldType", "309460EF-0CC5-41C6-9161-B3837BA3D374" );

            RockMigrationHelper.AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Wowza Server", "Url of Wowza Server", 0, "", "07784C63-DBF8-4179-85F4-6FCFCF9B752C" );
            
            Sql( @"
    DECLARE @AttributeId INT
    DECLARE @CategoryId INT

    SET @AttributeId = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '07784C63-DBF8-4179-85F4-6FCFCF9B752C' )
    SET @CategoryId = ( SELECT [Id] FROM [Category] WHERE [Guid] = 'A1FAC4BA-DCB8-4DF5-8937-C637AF7217D1' )

    IF @CategoryId IS NULL
    BEGIN

	    DECLARE @AttributeEntityId INT
	    SET @AttributeEntityId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Attribute' )

        INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [Name], [Guid], [Order] )
        VALUES ( 1, @AttributeEntityId, 'EntityTypeId', 'Command Center', 'A1FAC4BA-DCB8-4DF5-8937-C637AF7217D1', 0 )
        SET @CategoryId = SCOPE_IDENTITY()
        
    END

    IF NOT EXISTS (SELECT [AttributeId] FROM [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId)
    BEGIN
        
        INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
        VALUES ( @AttributeId, @CategoryId )

    END
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DELETE [Attribute] WHERE [Guid] = '07784C63-DBF8-4179-85F4-6FCFCF9B752C'
    DELETE [Category] WHERE [Guid] = 'A1FAC4BA-DCB8-4DF5-8937-C637AF7217D1'
");

            RockMigrationHelper.DeleteAttribute( "D23CF945-F440-4821-8496-C35035E1C3FE" ); // Detail Page

            RockMigrationHelper.DeleteBlock( "FF6657ED-7A19-4869-A887-32EF70F84EDB" ); // Recording Detail
            RockMigrationHelper.DeleteBlock( "7591B01B-8F22-47E3-BEFB-076338A3F24A" ); // Recording List

            RockMigrationHelper.DeleteBlockType( "AF4EB7C5-9121-4765-BEF2-558499BD0D6C" ); // com .ccvonline - Command Center - Recording List
            RockMigrationHelper.DeleteBlockType( "1A054FCC-2E0E-4AD1-BA36-21991DB479AB" ); // com .ccvonline - Command Center - Recording Detail

            RockMigrationHelper.DeletePage( "6E7ACDFC-0297-473E-8990-9C96CC49394C" ); // Recording Details
            RockMigrationHelper.DeletePage( "2BAAF392-2FE6-4D83-B949-122E6B97E5BB" ); // Service Recordings
        }
    }
}
