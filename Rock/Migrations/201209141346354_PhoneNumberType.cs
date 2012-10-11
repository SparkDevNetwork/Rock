namespace Rock.Migrations
    
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class PhoneNumberType : RockMigration_0
        
        public override void Up()
            
            DropIndex( "dbo.crmPhoneNumber", new string[]      "Number" } );
            AddColumn( "dbo.crmPhoneNumber", "NumberTypeId", c => c.Int() );
            AddColumn( "dbo.crmPhoneNumber", "IsUnlisted", c => c.Boolean( nullable: false ) );
            AlterColumn( "dbo.crmPhoneNumber", "Number", c => c.String( nullable: false, maxLength: 20 ) );
            AddForeignKey( "dbo.crmPhoneNumber", "NumberTypeId", "dbo.coreDefinedValue", "Id" );
            CreateIndex( "dbo.crmPhoneNumber", "NumberTypeId" );
            CreateIndex( "dbo.crmPhoneNumber", "Number" );

            AddDefinedType( "Person", "Phone Type", "Type of phone number", "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD" );
            AddDefinedValue( "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD", "Primary", "Primary Phone Number", "407E7E45-7B2E-4FCD-9605-ECB1339F2453" );
            AddDefinedValue( "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD", "Secondary", "Secondary Phone Number", "AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303" );

            Sql( @"
                UPDATE [cmsBlockInstance] SET [Name] = 'Global Attributes' WHERE [Guid] = '3CBB177B-DBFB-4FB2-A1A7-957DC6C350EB'
" );
        }

        public override void Down()
            
            Sql( @"
                UPDATE [crmPhoneNumber] SET [NumberTypeId] = NULL 
                DELETE [coreDefinedValue] WHERE [DefinedTypeId] IN (
                    SELECT [Id] FROM [coreDefinedType] WHERE [Guid] = '8345DD45-73C6-4F5E-BEBD-B77FC83F18FD')
" );
            DeleteDefinedType( "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD" );

            DropIndex( "dbo.crmPhoneNumber", new string[]      "Number" } );
            DropIndex( "dbo.crmPhoneNumber", new[]      "NumberTypeId" } );
            DropForeignKey( "dbo.crmPhoneNumber", "NumberTypeId", "dbo.coreDefinedValue" );
            AlterColumn( "dbo.crmPhoneNumber", "Number", c => c.String( nullable: false, maxLength: 100 ) );
            DropColumn( "dbo.crmPhoneNumber", "IsUnlisted" );
            DropColumn( "dbo.crmPhoneNumber", "NumberTypeId" );
            CreateIndex( "crmPhoneNumber", "Number" );
        }
    }
}
