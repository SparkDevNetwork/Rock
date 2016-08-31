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
    public partial class Add404PagesForSites : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("936C90C4-29CF-4665-A489-7C687217F7B8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Page Not Found", "", "FAD8EC2B-0070-4C18-8DC3-F50DD0D1E086", ""); // Site:Rock Internal
            AddPage("EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Page Not Found", "", "B5CA96C9-9DBF-46F3-B2C0-425D394ABECB", ""); // Site:External Website

            // Add Block to Page: Page Not Found, Site: Rock Internal
            AddBlock("FAD8EC2B-0070-4C18-8DC3-F50DD0D1E086", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Content", "Main", "", "", 0, "D4FD3F5A-63DA-422B-900D-CD95CDC786D8");

            // Add Block to Page: Page Not Found, Site: External Website
            AddBlock("B5CA96C9-9DBF-46F3-B2C0-425D394ABECB", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Content", "Main", "", "", 0, "C21F0478-0211-4C43-8EFC-190F4335EA9D"); 


            // Attrib Value for Block:Content, Attribute:Use Code Editor Page: Page Not Found, Site: Rock Internal
            AddBlockAttributeValue("D4FD3F5A-63DA-422B-900D-CD95CDC786D8", "0673E015-F8DD-4A52-B380-C758011331B2", @"True");

            // Attrib Value for Block:Content, Attribute:Cache Duration Page: Page Not Found, Site: Rock Internal
            AddBlockAttributeValue("D4FD3F5A-63DA-422B-900D-CD95CDC786D8", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0");

            // Attrib Value for Block:Content, Attribute:Context Parameter Page: Page Not Found, Site: Rock Internal
            AddBlockAttributeValue("D4FD3F5A-63DA-422B-900D-CD95CDC786D8", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"");

            // Attrib Value for Block:Content, Attribute:Context Name Page: Page Not Found, Site: Rock Internal
            AddBlockAttributeValue("D4FD3F5A-63DA-422B-900D-CD95CDC786D8", "466993F7-D838-447A-97E7-8BBDA6A57289", @"");

            // Attrib Value for Block:Content, Attribute:Require Approval Page: Page Not Found, Site: Rock Internal
            AddBlockAttributeValue("D4FD3F5A-63DA-422B-900D-CD95CDC786D8", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False");

            // Attrib Value for Block:Content, Attribute:Support Versions Page: Page Not Found, Site: Rock Internal
            AddBlockAttributeValue("D4FD3F5A-63DA-422B-900D-CD95CDC786D8", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False");

            // Attrib Value for Block:Content, Attribute:Use Code Editor Page: Page Not Found, Site: External Website
            AddBlockAttributeValue("C21F0478-0211-4C43-8EFC-190F4335EA9D", "0673E015-F8DD-4A52-B380-C758011331B2", @"True");

            // Attrib Value for Block:Content, Attribute:Cache Duration Page: Page Not Found, Site: External Website
            AddBlockAttributeValue("C21F0478-0211-4C43-8EFC-190F4335EA9D", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0");

            // Attrib Value for Block:Content, Attribute:Context Parameter Page: Page Not Found, Site: External Website
            AddBlockAttributeValue("C21F0478-0211-4C43-8EFC-190F4335EA9D", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"");

            // Attrib Value for Block:Content, Attribute:Context Name Page: Page Not Found, Site: External Website
            AddBlockAttributeValue("C21F0478-0211-4C43-8EFC-190F4335EA9D", "466993F7-D838-447A-97E7-8BBDA6A57289", @"");

            // Attrib Value for Block:Content, Attribute:Require Approval Page: Page Not Found, Site: External Website
            AddBlockAttributeValue("C21F0478-0211-4C43-8EFC-190F4335EA9D", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False");

            // Attrib Value for Block:Content, Attribute:Support Versions Page: Page Not Found, Site: External Website
            AddBlockAttributeValue("C21F0478-0211-4C43-8EFC-190F4335EA9D", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False");

            Sql( @"
                DECLARE @BlockId INT
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = 'D4FD3F5A-63DA-422B-900D-CD95CDC786D8')
                INSERT INTO [HtmlContent]
                   ([BlockId]
                   ,[EntityValue]
                   ,[Version]
                   ,[Content]
                   ,[IsApproved]
                   ,[Guid]
                   ,[LastModifiedDateTime])
             VALUES (
                    @BlockId
                   ,''
                   ,1
                   ,'<div class=''alert alert-warning''>
<h4>The Page You Requested Was Not Found</h4>
<p>Sorry, but the page you are looking for can not be found. Check the address of the page
and see your adminstrator if you still need assistance.<p>
</div>'
                   ,1
                   ,newid()
                   ,getdate())
            
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = 'C21F0478-0211-4C43-8EFC-190F4335EA9D')
                INSERT INTO [HtmlContent]
                   ([BlockId]
                   ,[EntityValue]
                   ,[Version]
                   ,[Content]
                   ,[IsApproved]
                   ,[Guid]
                   ,[LastModifiedDateTime])
             VALUES (
                    @BlockId
                   ,''
                   ,1
                   ,'<h2 class=''text-center''>Whoops... Page Not Found !!!</h2>

<div class=''not-found''>
    <h2 class=''text-center''>404 <i class=''fa fa-question-circle''></i></h2>
</div>

<p class=''text-center''>Your requested page could not be found or it is currently unavailable.
Please <a href=''/''>click here</a> to go back to our home page</a>'
                   ,1
                   ,newid()
                   ,getdate())
            " );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Content, from Page: Page Not Found, Site: External Website
            DeleteBlock("C21F0478-0211-4C43-8EFC-190F4335EA9D");
            // Remove Block: Content, from Page: Page Not Found, Site: Rock Internal
            DeleteBlock("D4FD3F5A-63DA-422B-900D-CD95CDC786D8");

            DeletePage("B5CA96C9-9DBF-46F3-B2C0-425D394ABECB"); // Page: Page Not FoundLayout: FullWidth, Site: External Website
            DeletePage("FAD8EC2B-0070-4C18-8DC3-F50DD0D1E086"); // Page: Page Not FoundLayout: Full Width, Site: Rock Internal
        }
    }
}
