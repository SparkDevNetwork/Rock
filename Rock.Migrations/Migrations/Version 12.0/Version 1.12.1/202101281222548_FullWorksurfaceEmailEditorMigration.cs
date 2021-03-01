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

    /// <summary>
    ///
    /// </summary>
    public partial class FullWorksurfaceEmailEditorMigration : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Site: Rock 
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "FullWorksurface", "Full Worksurface", "", "C2467799-BB45-4251-8EE6-F0BF27201535" );
            // Update New Communication Page to Use Full Worksurface
            RockMigrationHelper.UpdatePageLayout( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "C2467799-BB45-4251-8EE6-F0BF27201535" );
            // Add Block Page Menu to Layout: Full Worksurface, Site: Rock RMS 
            RockMigrationHelper.AddBlock( true, null, "C2467799-BB45-4251-8EE6-F0BF27201535".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Navigation", @"", @"", 0, "4709764E-5378-4B7D-AC85-A5D06BE86ECA" );
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Include Current Parameters // Attribute Value: False 
            RockMigrationHelper.AddBlockAttributeValue( "4709764E-5378-4B7D-AC85-A5D06BE86ECA", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Template // Attribute Value: {% include '~~/Assets/Lava/PageNav.lava' %} 
            RockMigrationHelper.AddBlockAttributeValue( "4709764E-5378-4B7D-AC85-A5D06BE86ECA", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageNav.lava' %}" );
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Root Page // Attribute Value: 20f97a93-7949-4c2a-8a5e-c756fe8585ca 
            RockMigrationHelper.AddBlockAttributeValue( "4709764E-5378-4B7D-AC85-A5D06BE86ECA", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"20f97a93-7949-4c2a-8a5e-c756fe8585ca" );
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Number of Levels // Attribute Value: 3 
            RockMigrationHelper.AddBlockAttributeValue( "4709764E-5378-4B7D-AC85-A5D06BE86ECA", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Include Current QueryString // Attribute Value: False 
            RockMigrationHelper.AddBlockAttributeValue( "4709764E-5378-4B7D-AC85-A5D06BE86ECA", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Add Block Attribute Value // Block: Page Menu // BlockType: Page Menu // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Is Secondary Block // Attribute Value: False 
            RockMigrationHelper.AddBlockAttributeValue( "4709764E-5378-4B7D-AC85-A5D06BE86ECA", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
            // Add Block Login Status to Layout: Full Worksurface, Site: Rock RMS 
            RockMigrationHelper.AddBlock( true, null, "C2467799-BB45-4251-8EE6-F0BF27201535".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "04712F3D-9667-4901-A49D-4507573EF7AD".AsGuid(), "Login Status", "Login", @"", @"", 0, "0E3484CA-06EA-46CD-A55E-A1A480965DC9" );
            // Add Block Attribute Value // Block: Login Status // BlockType: Login Status // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: My Profile Page // Attribute Value: 08dbd8a5-2c35-4146-b4a8-0f7652348b25 
            RockMigrationHelper.AddBlockAttributeValue( "0E3484CA-06EA-46CD-A55E-A1A480965DC9", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            // Add Block Attribute Value // Block: Login Status // BlockType: Login Status // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: My Settings Page // Attribute Value: cf54e680-2e02-4f16-b54b-a2f2d29cd932 
            RockMigrationHelper.AddBlockAttributeValue( "0E3484CA-06EA-46CD-A55E-A1A480965DC9", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", @"cf54e680-2e02-4f16-b54b-a2f2d29cd932" );
            // Add Block Attribute Value // Block: Login Status // BlockType: Login Status // Block Location: Layout=Full Worksurface, Site=Rock RMS // Attribute: Logged In Page List // Attribute Value: My Dashboard^~/MyDashboard 
            RockMigrationHelper.AddBlockAttributeValue( "0E3484CA-06EA-46CD-A55E-A1A480965DC9", "1B0E8904-196B-433E-B1CC-937AD3CA5BF2", @"My Dashboard^~/MyDashboard" );
            // Add Block Smart Search to Layout: Full Worksurface, Site: Rock RMS 
            RockMigrationHelper.AddBlock( true, null, "C2467799-BB45-4251-8EE6-F0BF27201535".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74".AsGuid(), "Smart Search", "Header", @"", @"", 0, "AD1A8AD8-3E94-45F3-A4B4-6BD2AE72A133" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
