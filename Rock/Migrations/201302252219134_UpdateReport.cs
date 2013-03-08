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
    public partial class UpdateReport : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Report", "EntityTypeId", c => c.Int());
            AddForeignKey("dbo.Report", "EntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.Report", "EntityTypeId");

            AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "Reports", "", "TwoColumnLeft", "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D" );
            AddBlockType( "Reporting - Report Detail", "", "~/Blocks/Reporting/ReportDetail.ascx", "E431DBDF-5C65-45DC-ADC5-157A02045CCD" );
            AddBlock( "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D", "ADE003C7-649B-466A-872B-B8AC952E7841", "Report Category Tree", "", "LeftContent", 0, "0F1F8343-A187-4653-9A4A-47D67CE86D71" );
            AddBlock( "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "", "RightContent", 1, "76545653-16E1-4227-82D2-63295755D8D3" );
            AddBlock( "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D", "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "Report Detail", "", "RightContent", 0, "1B7D7C5C-A201-4FFD-BCEC-762D126DFC2F" );
            // Attrib Value for Category Detail:Entity Type
            AddBlockAttributeValue( "76545653-16E1-4227-82D2-63295755D8D3", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "79" );
            // Attrib Value for Report Category Tree:Page Parameter Key
            AddBlockAttributeValue( "0F1F8343-A187-4653-9A4A-47D67CE86D71", "AA057D3E-00CC-42BD-9998-600873356EDB", "ReportId" );
            // Attrib Value for Report Category Tree:Entity Type
            AddBlockAttributeValue( "0F1F8343-A187-4653-9A4A-47D67CE86D71", "06D414F0-AA20-4D3C-B297-1530CCD64395", "79" );
            // Attrib Value for Report Category Tree:Detail Page Guid
            AddBlockAttributeValue( "0F1F8343-A187-4653-9A4A-47D67CE86D71", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", "0fdf1f63-cfb3-4f8e-ac5d-a5312b522d6d" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "0F1F8343-A187-4653-9A4A-47D67CE86D71" ); // Report Category Tree
            DeleteBlock( "76545653-16E1-4227-82D2-63295755D8D3" ); // Category Detail
            DeleteBlock( "1B7D7C5C-A201-4FFD-BCEC-762D126DFC2F" ); // Report Detail
            DeleteBlockType( "E431DBDF-5C65-45DC-ADC5-157A02045CCD" ); // Reporting - Report Detail
            DeletePage( "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D" ); // Reports

            DropIndex( "dbo.Report", new[] { "EntityTypeId" } );
            DropForeignKey("dbo.Report", "EntityTypeId", "dbo.EntityType");
            DropColumn("dbo.Report", "EntityTypeId");
        }
    }
}
