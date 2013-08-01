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
    public partial class GroupSearchComponent : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {           
            // Add the Group Search results page
            AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "Group Search Results", "displays the results of a group search", "Default", "9C9CAD94-095E-4CC9-BC29-24BDE30492B2" );
            Sql( " UPDATE [Page] SET [ParentPageId] = NULL WHERE [Guid] = '9C9CAD94-095E-4CC9-BC29-24BDE30492B2' " ); // null out the parent page id

            AddBlockType( "Crm - Group Search", "", "~/Blocks/CRM/GroupSearch.ascx", "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C" );

            // add that block type onto the Group Search Results page
            AddBlock( "9C9CAD94-095E-4CC9-BC29-24BDE30492B2", "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C", "Group Search Results", "", "Content", 0, "3A03B5F2-C1F7-43E9-A7C9-5E4D054064E0" );

            // Add the new Group Name Search component as an entity
            UpdateEntityType( "Rock.Search.Group.Name", "Group Name", "Rock.Search.Group.Name, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "94825231-dc38-4dc0-a1d3-64b4ad6a87f0" );
            
            // add the four attributes:
            //   * Active with a value of True
            //   * Result URL with value "Group/Search/name/{0}"
            //   * Search Label with value "Group Name"
            //   * Order with a value of 4
            AddEntityAttribute( "Rock.Search.Group.Name", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should service be used?", 0, "False", "95C2DE5A-47AF-44B6-AF57-5E4A0CD5D3A4" );
            AddEntityAttribute( "Rock.Search.Group.Name", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Result URL", "", "The url to redirect the user to after they have entered search text.  (use '{0}' for the search text)", 0, "", "FAF631BA-6EFE-43E9-B92E-D4A557C0F3A4" );
            AddEntityAttribute( "Rock.Search.Group.Name", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Search Label", "", "The text to display in the search type dropdown", 0, "Group Name", "A4AB0991-B946-463B-AD30-A9F662F2ACD9" );
            AddEntityAttribute( "Rock.Search.Group.Name", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "4", "B7A0DA50-FF17-4D5E-B6FC-DD2A916E7C56" );

            // now the values for those three attributes...
            AddAttributeValue( "95C2DE5A-47AF-44B6-AF57-5E4A0CD5D3A4", 0, "True", "A31D414E-7775-4608-98C4-46357F461AB8" );
            AddAttributeValue( "FAF631BA-6EFE-43E9-B92E-D4A557C0F3A4", 0, "Group/Search/name/{0}", "E114EA45-75FD-47EF-BEC2-E87CEB9E5119" );
            AddAttributeValue( "A4AB0991-B946-463B-AD30-A9F662F2ACD9", 0, "Group Name", "3C5A5234-5140-4A81-A2AD-E73049D22144" );
            AddAttributeValue( "B7A0DA50-FF17-4D5E-B6FC-DD2A916E7C56", 0, "4", "C8DC4E2E-BEAF-4A4E-9827-9D300A88B840" );
 
            // Add a route for the group search results page
            AddPageRoute( "9C9CAD94-095E-4CC9-BC29-24BDE30492B2", "Group/Search/{SearchType}/{SearchTerm}" );

            // Add a route to view the group for the Group Viewer page
            AddPageRoute( "4E237286-B715-4109-A578-C1445EC02707", "Group/{groupId}" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete the "Result URL", "Search Label", and "Active" attributes. 
            DeleteAttribute( "95C2DE5A-47AF-44B6-AF57-5E4A0CD5D3A4" );
            DeleteAttribute( "FAF631BA-6EFE-43E9-B92E-D4A557C0F3A4" );
            DeleteAttribute( "A4AB0991-B946-463B-AD30-A9F662F2ACD9" );
            DeleteAttribute( "B7A0DA50-FF17-4D5E-B6FC-DD2A916E7C56" );

            // Delete the new search component entity
            DeleteEntityType( "94825231-dc38-4dc0-a1d3-64b4ad6a87f0" );

            // Delete the block instance then the block type
            DeleteBlock( "3A03B5F2-C1F7-43E9-A7C9-5E4D054064E0" ); // Group Search Results
            DeleteBlockType( "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C" ); // Group Search Results BlockType

            // Delete the route to the Group Viewer page 
            Sql( "DELETE FROM [PageRoute] WHERE [Route] = 'Group/{groupId}'" );

            // Delete a route for the group search results page
            Sql( "DELETE FROM [PageRoute] WHERE [Route] = 'Group/Search/{SearchType}/{SearchTerm}'" );

            // Delete the Group Search Results page
            DeletePage( "9C9CAD94-095E-4CC9-BC29-24BDE30492B2" ); // Group Search Results
        }
    }
}
