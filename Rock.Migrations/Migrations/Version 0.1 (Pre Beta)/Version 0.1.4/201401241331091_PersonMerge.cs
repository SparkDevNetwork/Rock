// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PersonMerge : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("B0F4B33D-DD11-4CCC-B79D-9342831B8701","D65F783D-87A9-4CC9-8110-E83466A0EADB","Merge People","","F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3",""); // Site:Rock Internal
            AddPageRoute( "F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3", "PersonMerge/{People}" );

            AddBlockType("Person Merge","Merges two or more person records into one.","~/Blocks/Crm/PersonMerge.ascx","9B274A75-1D9B-4533-9849-7892F10A7672");
            // Add Block to Page: Merge People, Site: Rock Internal
            AddBlock("F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3","","9B274A75-1D9B-4533-9849-7892F10A7672","Person Merge","Feature","","",0,"9D41F7DE-537C-4B7C-8BD8-66B972A606F7"); 

       }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "9D41F7DE-537C-4B7C-8BD8-66B972A606F7" );
            DeleteBlockType( "9B274A75-1D9B-4533-9849-7892F10A7672" ); // Person Merge
            DeletePage( "F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3" ); // Page: Merge PeopleLayout: Full Width, Site: Rock Internal
        }
    }
}
