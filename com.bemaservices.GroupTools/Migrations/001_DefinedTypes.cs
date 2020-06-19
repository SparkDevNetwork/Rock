using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 1, "1.9.4" )]
    public class DefinedTypes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Group", "Small Group Preferred Contact Method", "Used to manage the preferred contact method options for small group members.", "982CC7BD-451B-420F-9F54-FDBAE32672E0", @"" );
            RockMigrationHelper.UpdateDefinedValue( "982CC7BD-451B-420F-9F54-FDBAE32672E0", "Email", "", "5281BD8C-6A7B-4ACC-9491-9C1A9F753EB3", false );
            RockMigrationHelper.UpdateDefinedValue( "982CC7BD-451B-420F-9F54-FDBAE32672E0", "Phone", "", "9AEA0AF0-15CD-483D-B75A-0472B549FA4B", false );
            RockMigrationHelper.UpdateDefinedValue( "982CC7BD-451B-420F-9F54-FDBAE32672E0", "Text", "", "78163760-84E2-4377-AC5F-126D1E5D9E2E", false );

            RockMigrationHelper.AddDefinedType( "Group", "Small Group Category", "Used to manage the category options for small groups.", "8BF1BECD-9B0C-4689-8204-8405327DCBCF", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Color", "Color", "", 1041, true, "", false, true, "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA" );
            RockMigrationHelper.AddAttributeQualifier( "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", "selectiontype", "Color Picker", "87734DA6-276C-46A7-B5CD-A45A14C2C2F2" );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "Featured", "", "99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9", false );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "New", "", "3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852", false );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "Married", "", "E470E214-ABCB-4571-978C-6018441EF8DB", false );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "Men's", "", "7542E760-7630-4965-AA38-0ABCEA4490E6", false );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "Single", "", "A926D0EA-60EC-40AD-99DC-F422343B3C8C", false );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "Widowed", "", "7ED1AA89-6A0C-4593-8D12-BD4B4567A19A", false );
            RockMigrationHelper.UpdateDefinedValue( "8BF1BECD-9B0C-4689-8204-8405327DCBCF", "Women's", "", "42642E5C-6235-48AC-8242-D80BC3EAD053", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"rgb(255,235,59)" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "42642E5C-6235-48AC-8242-D80BC3EAD053", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"rgb(156,39,176)" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7542E760-7630-4965-AA38-0ABCEA4490E6", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"rgb(238,118,37)" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7ED1AA89-6A0C-4593-8D12-BD4B4567A19A", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"#3f51b5" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"#2196f3" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A926D0EA-60EC-40AD-99DC-F422343B3C8C", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"rgb(76,175,80)" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E470E214-ABCB-4571-978C-6018441EF8DB", "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA", @"rgb(76,175,80)" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "7F46AA05-150C-4AE7-B54C-D0E34A4CEBAA" ); // Color
            RockMigrationHelper.DeleteDefinedValue( "3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852" ); // New
            RockMigrationHelper.DeleteDefinedValue( "42642E5C-6235-48AC-8242-D80BC3EAD053" ); // Women's
            RockMigrationHelper.DeleteDefinedValue( "7542E760-7630-4965-AA38-0ABCEA4490E6" ); // Men's
            RockMigrationHelper.DeleteDefinedValue( "99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9" ); // Featured
            RockMigrationHelper.DeleteDefinedValue( "E470E214-ABCB-4571-978C-6018441EF8DB" ); // College
            RockMigrationHelper.DeleteDefinedType( "8BF1BECD-9B0C-4689-8204-8405327DCBCF" ); // Small Group Category

            RockMigrationHelper.DeleteDefinedValue( "5281BD8C-6A7B-4ACC-9491-9C1A9F753EB3" ); // Email
            RockMigrationHelper.DeleteDefinedValue( "78163760-84E2-4377-AC5F-126D1E5D9E2E" ); // Text
            RockMigrationHelper.DeleteDefinedValue( "9AEA0AF0-15CD-483D-B75A-0472B549FA4B" ); // Phone
            RockMigrationHelper.DeleteDefinedType( "982CC7BD-451B-420F-9F54-FDBAE32672E0" ); // Small Group Preferred Contact Method

            RockMigrationHelper.DeleteDefinedValue( "23141BD1-34CE-49F7-AAB6-05D58FAB7C27" ); // 30s
            RockMigrationHelper.DeleteDefinedValue( "6BCC0FE9-2A9A-402F-8B80-6CAB5AA19AD8" ); // 18-28
            RockMigrationHelper.DeleteDefinedValue( "A96649CE-1D07-46C1-987C-92D5AF69D720" ); // 40s
            RockMigrationHelper.DeleteDefinedValue( "F7522BF7-511E-476F-880A-388D13C89F02" ); // 50s
            RockMigrationHelper.DeleteDefinedType( "64C9BFA7-EE35-4829-802E-004CD2F78971" ); // Small Group Age Range
        }
    }
}

