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
    public partial class FlipCheckinClassRelationships : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //Page	Guid
            //Group Type Select	60E3EA1F-FD6B-4F0E-9C72-A9960E13427C
            //Location Select	043BB717-5799-446F-B8DA-30E575110B0C
            //Group Select      6F0CB22B-E05B-42F1-A329-9219E81F6C34
            //Time Select	    C0AFA081-B64E-4006-BFFC-A350A51AE4CC

            // Group Type Select
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0", "6F0CB22B-E05B-42F1-A329-9219E81F6C34" ); // next -> Group Select

            // Group Select
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7B489-39B8-49F9-8C8C-533275FAACDF", "043BB717-5799-446F-B8DA-30E575110B0C" ); // next -> Location Select
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "795530E8-9395-4360-99B6-376A4BF40C5A", "60E3EA1F-FD6B-4F0E-9C72-A9960E13427C" ); // prev -> Group Type Select

            // Location Select
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "39246677-8451-4422-B384-C7AD9DA6C649", "C0AFA081-B64E-4006-BFFC-A350A51AE4CC" ); // next -> Time Select
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "569E033B-A2D5-4C15-8CD5-7F1336C22871", "6F0CB22B-E05B-42F1-A329-9219E81F6C34" ); // prev -> Group Select

            // Time Select
            AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "DE808D50-0861-4E24-A483-F1C74C1FFDE8", "043BB717-5799-446F-B8DA-30E575110B0C" ); // prev -> Location Select

            // Reorder Group Select, then Location Select
            Sql( "UPDATE [Page] SET [Order]='7' WHERE [Guid] = '043BB717-5799-446F-B8DA-30E575110B0C'" );
            Sql( "UPDATE [Page] SET [Order]='6' WHERE [Guid] = '6F0CB22B-E05B-42F1-A329-9219E81F6C34'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Group Type Select
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0", "043BB717-5799-446F-B8DA-30E575110B0C" ); // next -> Location Select

            // Location Select
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "39246677-8451-4422-B384-C7AD9DA6C649", "6F0CB22B-E05B-42F1-A329-9219E81F6C34" ); // next -> Group Select
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "569E033B-A2D5-4C15-8CD5-7F1336C22871", "60E3EA1F-FD6B-4F0E-9C72-A9960E13427C" ); // prev -> Group Type Select

            // Group Select
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7B489-39B8-49F9-8C8C-533275FAACDF", "C0AFA081-B64E-4006-BFFC-A350A51AE4CC" ); // next -> Time Select
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "795530E8-9395-4360-99B6-376A4BF40C5A", "043BB717-5799-446F-B8DA-30E575110B0C" ); // prev -> Location Select

            // Time Select
            AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "DE808D50-0861-4E24-A483-F1C74C1FFDE8", "6F0CB22B-E05B-42F1-A329-9219E81F6C34" ); // prev -> Group Select

            // Reorder Location Select, then Group Select
            Sql( "UPDATE [Page] SET [Order]='6' WHERE [Guid] = '043BB717-5799-446F-B8DA-30E575110B0C'" );
            Sql( "UPDATE [Page] SET [Order]='7' WHERE [Guid] = '6F0CB22B-E05B-42F1-A329-9219E81F6C34'" );
        }
    }
}
