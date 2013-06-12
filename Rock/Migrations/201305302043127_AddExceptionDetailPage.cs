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
    public partial class AddExceptionDetailPage : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "21DA6141-0A03-4F00-B0A8-3B110FBE2438", "Exception Detail", "", "Default", "F1F58172-E03E-4299-910A-ED34F857DAFB" );
            AddBlockType( "Administration - Exception Detail", "", "~/Blocks/Administration/ExceptionDetail.ascx", "B9E704E8-2097-491D-A216-8011012AA84E" );
            AddBlock( "F1F58172-E03E-4299-910A-ED34F857DAFB", "B9E704E8-2097-491D-A216-8011012AA84E", "Exception Detail", "", "Content", 0, "7103AC26-413A-42D1-92C5-E2A26FC9E9E5" );
            AddBlockTypeAttribute( "B9E704E8-2097-491D-A216-8011012AA84E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "4B65CB0D-F878-408C-85C1-FA74D5B09169" );
            AddBlockTypeAttribute( "B9E704E8-2097-491D-A216-8011012AA84E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Cookies", "ShowCookies", "", "Show cookie information when block loads.", 0, "False", "DBD823AE-9F3B-4623-9158-285C78AECAF0" );
            AddBlockTypeAttribute( "B9E704E8-2097-491D-A216-8011012AA84E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Server Variables", "ShowServerVariables", "", "Show server variables when block loads.", 0, "False", "C169BF27-0905-43BC-B332-D94161FAC609" );
            
            // Attrib Value for Exception Detail:Detail Page              
            AddBlockAttributeValue("7103AC26-413A-42D1-92C5-E2A26FC9E9E5","4B65CB0D-F878-408C-85C1-FA74D5B09169","00000000-0000-0000-0000-000000000000");  
            
            // Attrib Value for Exception Detail:Show Cookies
            AddBlockAttributeValue("7103AC26-413A-42D1-92C5-E2A26FC9E9E5","DBD823AE-9F3B-4623-9158-285C78AECAF0","False");  
            
            // Attrib Value for Exception Detail:Show Server Variables              
            AddBlockAttributeValue("7103AC26-413A-42D1-92C5-E2A26FC9E9E5","C169BF27-0905-43BC-B332-D94161FAC609","False");  

            //Attrib Value for Exception List; Detail Page
            AddBlockAttributeValue( "557E75A4-1841-4CBE-B976-F36DF209AA17", "A742376A-0148-4777-B704-E47841879337", "F1F58172-E03E-4299-910A-ED34F857DAFB" );

            //Update Exception Detail Page Properties
            Sql( @" UPDATE [Page] 
                SET [IconCssClass] = 'icon-warning-sign',
                    [Description] = 'View Exceptions'
                WHERE [Guid] = '21DA6141-0A03-4F00-B0A8-3B110FBE2438'" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //Reset Exception Detail Page Properties
            Sql( @" UPDATE [Page] 
                SET [IconCssClass] = '',
                    [Description] = ''
                WHERE [Guid] = '21DA6141-0A03-4F00-B0A8-3B110FBE2438'" );

            DeleteBlockAttributeValue( "557E75A4-1841-4CBE-B976-F36DF209AA17", "A742376A-0148-4777-B704-E47841879337" ); //Remove Exception List Detail Page attribute value
            DeleteAttribute( "4B65CB0D-F878-408C-85C1-FA74D5B09169" ); // Detail Page
            DeleteAttribute( "DBD823AE-9F3B-4623-9158-285C78AECAF0" ); // Show Cookies
            DeleteAttribute( "C169BF27-0905-43BC-B332-D94161FAC609" ); // Show Server Variables
            DeleteBlock( "7103AC26-413A-42D1-92C5-E2A26FC9E9E5" ); // Exception Detail
            DeleteBlockType( "B9E704E8-2097-491D-A216-8011012AA84E" ); // Administration - Exception Detail
            DeletePage( "F1F58172-E03E-4299-910A-ED34F857DAFB" ); // Exception Detail
        }
    }
}
