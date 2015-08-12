using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class SpiritualGiftsDefinedType : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Spiritual Gifts", "A defined type for spiritual giftings", "A9C5FE90-7DAC-4E64-8385-FD29256943A5", @"" );
            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Encouragement", @"The Gift of Encouragement is the God-given ability to present words of comfort, consolation, and encouragement, so to strengthen, or urge to action, those who are discouraged or wavering in their faith.", "8EFC7AC1-EB44-4768-B960-880F5FE4CC73", false );

            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Giving", @"The Gift of Giving is the God-given ability to contribute money and resources to the work of the Lord with cheerfulness and liberality.", "55A19012-B110-41DC-828E-3D63A3EE3C1D", false );

            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Leadership", @"The Gift of Leadership is the God-given ability to cast vision, motivate, and direct people to harmoniously accomplish the purpose of God.", "1E9237CC-7583-4405-868D-9EC5B645710A", false );

            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Mercy", @"The Gift of Mercy is the God-given ability to feel deeply for those in physical, spiritual, or emotional need and then act to meet that need.", "98A0054C-4B92-453C-A78B-5A7564DC442F", false );

            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Ministry", @"The Gift of Ministry is the God-given ability to care and provide one's time and labor to the furthering of God's kingdom.", "74723552-9B45-4E59-9086-6329F8A0BDF4", false );

            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Prophecy", @"The Gift of Prophecy is the God-given ability to reveal God's truth and proclaim it in a timely and relevant manner for understanding, correction, repentance, or edification.", "A520D300-4537-4C04-8323-86B876E5EDD7", false );

            RockMigrationHelper.AddDefinedValue( "A9C5FE90-7DAC-4E64-8385-FD29256943A5", "Teaching", @"The Gift of Teaching is the God-given ability to understand, clearly explain, and apply the word of God, in such a way that it is clearly understood by others.", "0372ECC3-234A-45F5-9A48-8C3F4810E09C", false );

            RockMigrationHelper.UpdatePersonAttributeCategory( "Spiritual Gifts", "fa fa-gift", "Spiritual Gift score Person Attributes", "12D8E61F-ED07-41D9-BE0B-43C73907896D" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Prophecy", "Prophecy", "fa fa-cloud-download", "Prophecy", 1, string.Empty, "DDEC4CE8-F86A-4315-8F74-B9B44D7F0F82" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Ministry", "Ministry", "fa fa-globe", "Ministry", 2, string.Empty, "CC00CCA1-8FF7-49A7-9439-6AF0D6E4AD01" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Teaching", "Teaching", "fa fa-compass", "Teaching", 3, string.Empty, "AE145F94-1D5F-4F5B-A3C8-FED67D21E188" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Encouragement", "Encouragement", "fa fa-fire", "Encouragement", 4, string.Empty, "ADB58969-93FF-4F61-A45C-2728D53036C4" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Giving", "Giving", "fa fa-diamond", "Giving", 5, string.Empty, "8FB9C240-AC0F-4666-8D42-D1244751F8D8" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Leadership", "Leadership", "fa fa-bullhorn", "Leadership", 6, string.Empty, "52E79769-3445-4AB4-9BA1-27C4171A4F41" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Mercy", "Mercy", "fa fa-lifesaver", "Mercy", 7, string.Empty, "3DF89611-E027-4CA1-8F1E-4879BC54002D" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Last Save Date", "LastSaveDate", "fa fa-gift", "The date the person took the spiritual gifts test.", 9, string.Empty, "E0841055-FC6C-47DB-9CF3-36BD8DB66151" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.TEXT, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Gifting", "Gifting", "fa fa-gift", "The strongest spiritual gifting of a person.", 10, string.Empty, "C31F31AD-D1B8-41A8-A06A-FA8F5CA97970" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "C31F31AD-D1B8-41A8-A06A-FA8F5CA97970" );
            RockMigrationHelper.DeleteAttribute( "E0841055-FC6C-47DB-9CF3-36BD8DB66151" );
            RockMigrationHelper.DeleteAttribute( "3DF89611-E027-4CA1-8F1E-4879BC54002D" );
            RockMigrationHelper.DeleteAttribute( "52E79769-3445-4AB4-9BA1-27C4171A4F41" );
            RockMigrationHelper.DeleteAttribute( "8FB9C240-AC0F-4666-8D42-D1244751F8D8" );
            RockMigrationHelper.DeleteAttribute( "ADB58969-93FF-4F61-A45C-2728D53036C4" );
            RockMigrationHelper.DeleteAttribute( "AE145F94-1D5F-4F5B-A3C8-FED67D21E188" );
            RockMigrationHelper.DeleteAttribute( "CC00CCA1-8FF7-49A7-9439-6AF0D6E4AD01" );
            RockMigrationHelper.DeleteAttribute( "DDEC4CE8-F86A-4315-8F74-B9B44D7F0F82" );
            RockMigrationHelper.DeleteCategory( "12D8E61F-ED07-41D9-BE0B-43C73907896D" );

            RockMigrationHelper.DeleteDefinedValue( "0372ECC3-234A-45F5-9A48-8C3F4810E09C" );
            RockMigrationHelper.DeleteDefinedValue( "1E9237CC-7583-4405-868D-9EC5B645710A" );
            RockMigrationHelper.DeleteDefinedValue( "55A19012-B110-41DC-828E-3D63A3EE3C1D" );
            RockMigrationHelper.DeleteDefinedValue( "74723552-9B45-4E59-9086-6329F8A0BDF4" );
            RockMigrationHelper.DeleteDefinedValue( "8EFC7AC1-EB44-4768-B960-880F5FE4CC73" );
            RockMigrationHelper.DeleteDefinedValue( "98A0054C-4B92-453C-A78B-5A7564DC442F" );
            RockMigrationHelper.DeleteDefinedValue( "A520D300-4537-4C04-8323-86B876E5EDD7" );
            RockMigrationHelper.DeleteDefinedType( "A9C5FE90-7DAC-4E64-8385-FD29256943A5" );
        }
    }
}
