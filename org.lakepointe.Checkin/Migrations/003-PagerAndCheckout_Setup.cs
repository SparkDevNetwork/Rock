using Rock.Plugin;

namespace org.lakepointe.Checkin.Migrations
{
    [MigrationNumber( 4, "1.8.1.0" )]
    public class PagerAndCheckout_Setup : Migration
    {
        public override void Down()
        {
            /* Delete Pager Person Attribute*/
            RockMigrationHelper.DeleteAttribute( "CFBC57A8-16E7-4EA1-B126-E29A143F5E17" );
            /* Delete Check-out Restriction Person Attribute*/
            RockMigrationHelper.DeleteAttribute( "C9F520EC-88BA-4757-8611-120706543B0F" );

            /*Delete Check-out Restriction Defined Values*/
            RockMigrationHelper.DeleteDefinedValue( "C9F520EC-88BA-4757-8611-120706543B0F" );
            RockMigrationHelper.DeleteDefinedValue( "7B2AF732-7BEC-4A47-89FB-8C5B85D4FFDA" );
            RockMigrationHelper.DeleteDefinedValue( "7B54E2C5-026B-48F3-B53B-FAC75AA30963" );

            /* Delete Check-out Restriction Defined Type*/
            RockMigrationHelper.DeleteDefinedValue( "2DA85C58-5565-4346-9194-3CF92B2DC32E" );
        }

        public override void Up()
        {
            /* Add Check-out Restriction DefinedType */
            RockMigrationHelper.AddDefinedType( "Check-in", "Check-out Restrictions", "Restrictions on a person's ability to be checked out.", "2DA85C58-5565-4346-9194-3CF92B2DC32E" );
            RockMigrationHelper.AddDefinedTypeAttribute( "2DA85C58-5565-4346-9194-3CF92B2DC32E", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Message to Display", "MessageToDisplay", "", 1013, "", "4D35CDDB-E509-4ADB-89C7-799BB3CE0D30" );
            Sql( @"UPDATE dbo.[Attribute] SET IsGridColumn = 1, IsMultiValue = 0, IsRequired = 0 WHERE [Guid] = '4D35CDDB-E509-4ADB-89C7-799BB3CE0D30' " );

            /*Add CheckOut Restriction DefinedValues */
            RockMigrationHelper.UpdateDefinedValue( "2DA85C58-5565-4346-9194-3CF92B2DC32E", "None", "No Checkout Restrictions.", "C9F520EC-88BA-4757-8611-120706543B0F" );
            Sql( @"Update dbo.[DefinedValue] SET [Order] = 0 WHERE [Guid] = 'C9F520EC-88BA-4757-8611-120706543B0F'" );

            RockMigrationHelper.UpdateDefinedValue( "2DA85C58-5565-4346-9194-3CF92B2DC32E", "Check-in Person Only", "Can only be checked out by the same person who checked them in.", "7B2AF732-7BEC-4A47-89FB-8C5B85D4FFDA" );
            Sql( @"Update dbo.[DefinedValue] SET [Order] = 1 WHERE [Guid] = '7B2AF732-7BEC-4A47-89FB-8C5B85D4FFDA'" );
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "7B2AF732-7BEC-4A47-89FB-8C5B85D4FFDA", "4D35CDDB-E509-4ADB-89C7-799BB3CE0D30", "Please proceed to a staffed check-in kiosk for assistance." );

            RockMigrationHelper.UpdateDefinedValue( "2DA85C58-5565-4346-9194-3CF92B2DC32E", "Assisted Check-out Only", "Can only be checked out at an assisted check-in station.", "7B54E2C5-026B-48F3-B53B-FAC75AA30963" );
            Sql( @"Update dbo.[DefinedValue] SET [Order] = 2 WHERE [Guid] = '7B54E2C5-026B-48F3-B53B-FAC75AA30963'" );
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "7B54E2C5-026B-48F3-B53B-FAC75AA30963", "4D35CDDB-E509-4ADB-89C7-799BB3CE0D30", "Please proceed to a staffed check-in kiosk for assistance." );

            /*Add Check-out Restriction Person Attribute*/
            RockMigrationHelper.UpdatePersonAttribute( "59D5A94C-94A0-4630-B80A-BB25697D74C7", "752DC692-836E-4A3E-B670-4325CD7724BF", "Check-out Restrictions", "CheckOutRestrictions", "", "", 1014, "", "C9F520EC-88BA-4757-8611-120706543B0F" );
            RockMigrationHelper.UpdateAttributeQualifier( "C9F520EC-88BA-4757-8611-120706543B0F", "allowmultiple", "False", "58449F9A-9DC3-4642-875A-4C1A52BA1F77" );
            RockMigrationHelper.UpdateAttributeQualifier( "C9F520EC-88BA-4757-8611-120706543B0F", "definedtype", "269", "71D3AFCF-9895-49BA-8F71-A6C6F3A9BE74" );
            RockMigrationHelper.UpdateAttributeQualifier( "C9F520EC-88BA-4757-8611-120706543B0F", "displaydescription", "False", "87213805-81BE-4720-9F0E-53653495DB76" );
            RockMigrationHelper.UpdateAttributeQualifier( "C9F520EC-88BA-4757-8611-120706543B0F", "enhancedselection", "False", "F1B29C85-514C-4C5D-A8B2-72A9C73C80BB" );


            /*Add PagerId Person Attribute */
            RockMigrationHelper.UpdatePersonAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "752DC692-836E-4A3E-B670-4325CD7724BF", "Pager Id", "PagerId", "", "Children's Ministry Pager Id", 1015, "", "CFBC57A8-16E7-4EA1-B126-E29A143F5E17" );
            RockMigrationHelper.UpdateAttributeQualifier( "CFBC57A8-16E7-4EA1-B126-E29A143F5E17", "ispassword", "False", "E96DBFB5-7B2F-427F-99D6-6797D3231E3A" );
            RockMigrationHelper.UpdateAttributeQualifier( "CFBC57A8-16E7-4EA1-B126-E29A143F5E17", "maxcharacters", "", "3E6CD913-AE5A-4C81-8BF7-6B26D7F7A85D" );
            RockMigrationHelper.UpdateAttributeQualifier( "CFBC57A8-16E7-4EA1-B126-E29A143F5E17", "showcountdown", "False", "8F5A72DA-0AE9-4405-8695-8E70597B72DE" );
        }
    }
}
