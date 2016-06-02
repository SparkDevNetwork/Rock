// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Accountability.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    public class AddCustomPrayerEntryBlock : Migration
    {
        public override void Up()
        {
            // New Custom "Prayer Request Entry" Block Type
            RockMigrationHelper.UpdateBlockType("Prayer Request Entry","Allows prayer requests to be added via visitors on the website.","~/Plugins/com_centralaz/Core/Prayer/PrayerRequestEntry.ascx","com_centralaz > Prayer","1347CB3B-ECC7-472D-B639-D5CAAFF2442F");

            // New Block Type's Attributes
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","309460EF-0CC5-41C6-9161-B3837BA3D374","Category Selection","GroupCategoryId","","A top level category. This controls which categories the person can choose from when entering their prayer request.",1,@"","7D174E07-5E68-4BFF-8E95-98E874E60C07");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","309460EF-0CC5-41C6-9161-B3837BA3D374","Default Category","DefaultCategory","","If categories are not being shown, choose a default category to use for all new prayer requests.",2,@"4B2D88F5-6E45-4B4B-8776-11118C8E8269","E3A116A4-87AC-4840-8E0C-E4E31589AF0B");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Auto Approve","EnableAutoApprove","","If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.",3,@"True","84510A37-83D5-4E0F-BB68-E340F6C1D9AA");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Expires After (Days)","ExpireDays","","Number of days until the request will expire (only applies when auto-approved is enabled).",4,@"14","D6A9B513-F71D-4A9D-BCE9-7724648A91A4");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Default Allow Comments Setting","DefaultAllowCommentsSetting","","This is the default setting for the 'Allow Comments' on prayer requests. If you enable the 'Comments Flag' below, the requestor can override this default setting.",5,@"True","133EACF6-A30C-4DBC-BC1C-9DBE41EE902E");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Urgent Flag","EnableUrgentFlag","","If enabled, requestors will be able to flag prayer requests as urgent.",6,@"False","6F2394D3-27CB-4143-B794-EAB2B090C645");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Comments Flag","EnableCommentsFlag","","If enabled, requestors will be able set whether or not they want to allow comments on their requests.",7,@"False","FFECC8E5-CAB1-4929-89F2-4F8A650FD4B9");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Public Display Flag","EnablePublicDisplayFlag","","If enabled, requestors will be able set whether or not they want their request displayed on the public website.",8,@"False","6841AA56-76C4-464C-86BA-BA27975D162A");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Character Limit","CharacterLimit","","If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.",9,@"250","B03712C3-BB30-4332-9EEE-4852FABD175A");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Navigate To Parent On Save","NavigateToParentOnSave","","If enabled, on successful save control will redirect back to the parent page.",10,@"False","BB1BD515-1041-415D-B9BB-8581CA078E46");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Save Success Text","SaveSuccessText","","Text to display upon successful save. (Only applies if not navigating to parent page on save.) <span class='tip tip-lava'></span><span class='tip tip-html'></span>",11,@"<p>Thank you for allowing us to pray for you.</p>","E36A3C00-F871-4797-84F8-2C66BFE8174F");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow","Workflow","","An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' when processing is started.",12,@"","DE88DED9-9869-411E-B103-27CAAA5D06A7");
            RockMigrationHelper.AddBlockTypeAttribute("1347CB3B-ECC7-472D-B639-D5CAAFF2442F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Outputs the object graph to help create your liquid syntax.",13,@"False","797B1F3A-BA98-4230-B5D9-203E34EA8A38");
        }

        public override void Down()
        {
            // Delete the"Prayer Request Entry" Block Type
            RockMigrationHelper.DeleteAttribute( "D6A9B513-F71D-4A9D-BCE9-7724648A91A4" );
            RockMigrationHelper.DeleteAttribute( "84510A37-83D5-4E0F-BB68-E340F6C1D9AA" );
            RockMigrationHelper.DeleteAttribute( "E3A116A4-87AC-4840-8E0C-E4E31589AF0B" );
            RockMigrationHelper.DeleteAttribute( "7D174E07-5E68-4BFF-8E95-98E874E60C07" );
            RockMigrationHelper.DeleteAttribute( "797B1F3A-BA98-4230-B5D9-203E34EA8A38" );
            RockMigrationHelper.DeleteAttribute( "DE88DED9-9869-411E-B103-27CAAA5D06A7" );
            RockMigrationHelper.DeleteAttribute( "E36A3C00-F871-4797-84F8-2C66BFE8174F" );
            RockMigrationHelper.DeleteAttribute( "BB1BD515-1041-415D-B9BB-8581CA078E46" );
            RockMigrationHelper.DeleteAttribute( "B03712C3-BB30-4332-9EEE-4852FABD175A" );
            RockMigrationHelper.DeleteAttribute( "FFECC8E5-CAB1-4929-89F2-4F8A650FD4B9" );
            RockMigrationHelper.DeleteAttribute( "6F2394D3-27CB-4143-B794-EAB2B090C645" );
            RockMigrationHelper.DeleteAttribute( "133EACF6-A30C-4DBC-BC1C-9DBE41EE902E" );
            RockMigrationHelper.DeleteAttribute( "6841AA56-76C4-464C-86BA-BA27975D162A" );
            RockMigrationHelper.DeleteBlockType( "1347CB3B-ECC7-472D-B639-D5CAAFF2442F" );
        }
    }
}