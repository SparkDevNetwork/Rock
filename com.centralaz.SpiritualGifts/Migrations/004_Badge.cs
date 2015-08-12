using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class Badge : Migration
    {
        public static readonly string SPIRITUAL_GIFT_BADGE = "88F61BA1-35AA-4080-AC56-CF4B1145C51F";

        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Badge items
            // Ensure the badge is a registered EntityType
            RockMigrationHelper.UpdateEntityType( "com.centralaz.SpiritualGifts.PersonProfile.Badge.SpiritualGift", "SpiritualGift", "com.centralaz.SpiritualGifts.PersonProfile.Badge.SpiritualGift, Rock, Version=1.1.2.0, Culture=neutral, PublicKeyToken=null", false, true, "A8B149BC-8E16-459C-8B93-BEC15089FBC4" );

            // Ensure the PersonBadge for Rock.PersonProfile.Badge.SpiritualGift is added
            RockMigrationHelper.UpdatePersonBadge( "Spiritual Gift Personality Assessment Result", "Shows a small chart of a person's Spiritual Gift personality assessment results and links to the details of their assessment.",
                "com.centralaz.SpiritualGifts.PersonProfile.Badge.SpiritualGift", 0, SPIRITUAL_GIFT_BADGE );

            // Add/Update the Active Attribute
            RockMigrationHelper.AddPersonBadgeAttribute( SPIRITUAL_GIFT_BADGE, Rock.SystemGuid.FieldType.BOOLEAN,
                "Active", "Active", "Should Service be used?", 0, string.Empty, "C38ACE08-6DC7-4153-A497-BFFFB30FAF19" );

            // Add/Update the Order Attribute
            RockMigrationHelper.AddPersonBadgeAttribute( SPIRITUAL_GIFT_BADGE, Rock.SystemGuid.FieldType.INTEGER,
                "Order", "Order", "The order that this service should be used (priority)", 1, string.Empty, "DFC70633-D06D-44D8-87D5-DD6C5832F0A8" );

            // Add/Update the SpiritualGift Result Detail Attribute
            RockMigrationHelper.AddPersonBadgeAttribute( SPIRITUAL_GIFT_BADGE, Rock.SystemGuid.FieldType.PAGE_REFERENCE,
                "Spiritual Gift Result Detail", "SpiritualGiftResultDetail", "Page to show the details of the Spiritual Gift assessment results. If blank no link is created.", 2, string.Empty, "56D4946D-DD70-4C06-A382-3CF34B56A8BE" );

            // add the badge to PersonBadge SpiritualGift Result Detail page guid
            RockMigrationHelper.AddPersonBadgeAttributeValue( SPIRITUAL_GIFT_BADGE, "56D4946D-DD70-4C06-A382-3CF34B56A8BE", "039F770B-5734-4735-ABF1-B39B77C84AD0" );

            // add the new badge to the person profile's Badge 3 block
            RockMigrationHelper.AddBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", SPIRITUAL_GIFT_BADGE, appendToExisting: true );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // delete badge items
            // delete the Badge 3 attribute value
            RemoveBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", SPIRITUAL_GIFT_BADGE );

            // delete PersonBadgeAttribute
            RockMigrationHelper.DeleteAttribute( "F1F4AA7A-E657-4BA4-B3E2-6EEEB5A839CE" );
            RockMigrationHelper.DeleteAttribute( "EC96AA8D-31D8-4281-AE5B-6DCB4FEC6234" );
            RockMigrationHelper.DeleteAttribute( "0EB9498A-5F92-41E2-94CD-F5F86B4E7D6F" );

            Sql( string.Format( @"
            DELETE FROM [PersonBadge] where [Guid] = '{0}'", SPIRITUAL_GIFT_BADGE ) );

            RockMigrationHelper.DeleteEntityType( "6D29DB44-EDC5-42AA-B42C-482BC0920AD0" );
        }

        /// <summary>
        /// Removes only the given value string from the comma delimited list of values in an AttributeValue.
        /// It assumes the value is specific enough and only appears once in the string.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="value">The value to remove.</param>
        public void RemoveBlockAttributeValue( string blockGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                -- replace a string in an AttributeValue
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                DECLARE @TheValue NVARCHAR(MAX) = '{2}'

                -- Get the current value
                IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
                BEGIN
                    -- replace if there is a trailing comma
                    -- and replace if there is a leading comma
                    UPDATE [AttributeValue]
                    SET [Value] = 
                        REPLACE(
                            REPLACE([Value], '{2},', ''),
                            ',{2}', ''
                        )
                    WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
                END
",
                    blockGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }
    }
}
