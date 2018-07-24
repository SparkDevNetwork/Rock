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
    using Rock.SystemGuid;

    /// <summary>
    ///
    /// </summary>
    public partial class Homepage : Rock.Migrations.RockMigration
    {
        private static readonly string INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE = "d526f4a5-19b9-410f-a663-400d93c61d3c";
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.AttributeMatrixItem", "Attribute Matrix Item", "Rock.Model.AttributeMatrixItem, Rock, Version=1.8.0.16, Culture=neutral, PublicKeyToken=null", true, true, "3c9d5021-0484-4846-aef6-b6216d26c3c8" );
            Sql( MigrationSQL._201805211735573_Homepage_ContentChannel );
            // Add Salvations and Adult Attendance metrics
            RockMigrationHelper.UpdateCategory( "3D35C859-DF37-433F-A20A-0FFD0FCB9862", "Homepage Metrics", "fa fa-newspaper", "Metrics to display on the homepage.", "073add0c-b1f3-43ab-8360-89a1ce05a95d" );
            Sql( MigrationSQL._201805211735573_Homepage_Metrics );

            RockMigrationHelper.UpdateBlockType( "Internal Communication View", "Block for showing the contents of internal content channels.", "~/Blocks/Utility/InternalCommunicationView.ascx", "Utility", INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE );
            // Add Block to Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Page.INTERNAL_HOMEPAGE, "", INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "Internal Communication View", "Main", @"{% stylesheet id:''home-feature'' %}

.feature-image {
    width: 100%;
    height: 450px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}


.communicationview h1 {
    font-size: 28px;
    margin-top: 12px;
}

.homepage-article .photo {
    width: 100%;
    height: 140px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}

.metric {
    border: 1px solid #ccc;
    padding: 12px;
    margin-bottom: 12px;
}

.metric h5 {
    font-size: 24px;
    margin-top: 0;
    margin-bottom: 0;
    width: 100%;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.metric .value {
    font-size: 48px;
    font-weight: 800;
    line-height: 1em;
}

.metric .value small{
    display: block;
    font-weight: 300;
    font-size: 14px;
    line-height: 1em;
}

.metric .icon {
    float: right;
    opacity: .3;
    font-size: 65px;
    border-radius: 0;
    width: 85px;
    height: 65px;
}

{% endstylesheet %}", @"", 2, "879BC5A7-3CE2-43FC-BEDB-B93B0054F417" );
            /*
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '60469A41-5180-446F-9935-0A09D81CD319'" );  // Page: Internal Homepage,  Zone: Main,  Block: Notification List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'" );  // Page: Internal Homepage,  Zone: Main,  Block: Install Checklist
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '879BC5A7-3CE2-43FC-BEDB-B93B0054F417'" );  // Page: Internal Homepage,  Zone: Main,  Block: Internal Communication View
            */

            // Attrib for BlockType: Internal Communication View:Block Title Icon CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCssClass", "", @"The icon CSS class for use in the block title.", 1, @"fa fa-newspaper", "86044377-02E9-46C7-A90B-4CF4DA8F38B0" );
            // Attrib for BlockType: Internal Communication View:Metric Value Count
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Metric Value Count", "MetricValueCount", "", @"The number of metric values to return per metric. You will always get the lastest value, but if you would like to return additional values (i.e. to create a chart) you can specify that here.", 4, @"0", "89F1FFC4-A1FB-41DA-ACDF-ABDF15DAE062" );
            // Attrib for BlockType: Internal Communication View:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"The time, in seconds, to cache the data for this block. The Lava template will still be run to enable personalization. Only the data for the block will be cached.", 7, @"3600", "8B673CFB-5BFB-4CDF-8FFA-6ECC9766B670" );
            // Attrib for BlockType: Internal Communication View:Block Title Template
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Block Title Template", "BlockTitleTemplate", "", @"Lava template for determining the title of the block.", 0, @"Staff Updates <small>({{ Item.StartDateTime | Date:'sd' }})</small>", "16E7B8AB-E5AC-4560-A060-A4E51DF8E390" );
            // Attrib for BlockType: Internal Communication View:Body Template
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body Template", "BodyTemplate", "", @"The Lava template for rendering the body of the block.", 5, @"d", "E7A96FBE-C05F-4079-896E-E84115A96077" );
            // Attrib for BlockType: Internal Communication View:Metrics
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4", "Metrics", "Metrics", "", @"Select the metrics you would like to display on the page.", 3, @"", "F3B7330F-1E4B-45A8-AF38-587E1409F9D9" );
            // Attrib for BlockType: Internal Communication View:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"The content channel to display with the template. The contant channel must be of type 'Internal Communication Template'.", 2, @"", "0C490E5F-BCEC-4E0A-B4EC-D2A3AE0F0256" );
            // Attrib for BlockType: Internal Communication View:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE, "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be made available to the block.", 6, @"", "44A49D15-3A54-4AF7-A73C-F1AEB384C23A" );
            // Attrib Value for Block:Internal Communication View, Attribute:Enabled Lava Commands Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "44A49D15-3A54-4AF7-A73C-F1AEB384C23A", @"" );
            // Attrib Value for Block:Internal Communication View, Attribute:Block Title Template Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "16E7B8AB-E5AC-4560-A060-A4E51DF8E390", @"Staff Updates <small>({{ Item.StartDateTime | Date:'sd' }})</small>" );
            // Attrib Value for Block:Internal Communication View, Attribute:Content Channel Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "0C490E5F-BCEC-4E0A-B4EC-D2A3AE0F0256", @"78D01959-0EA6-4FE4-B6F8-A3765F0CEDBF" );
            // Attrib Value for Block:Internal Communication View, Attribute:Block Title Icon CSS Class Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "86044377-02E9-46C7-A90B-4CF4DA8F38B0", @"fa fa-newspaper" );
            // Attrib Value for Block:Internal Communication View, Attribute:Body Template Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "E7A96FBE-C05F-4079-896E-E84115A96077", @"{% stylesheet id:''home-feature'' %}

.feature-image {
    width: 100%;
    height: 450px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}


.communicationview h1 {
    font-size: 28px;
    margin-top: 12px;
}

.homepage-article .photo {
    width: 100%;
    height: 140px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}

.metric {
    border: 1px solid #ccc;
    padding: 12px;
    margin-bottom: 12px;
}

.metric h5 {
    font-size: 24px;
    margin-top: 0;
    margin-bottom: 0;
    width: 100%;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.metric .value {
    font-size: 48px;
    font-weight: 800;
    line-height: 1em;
}

.metric .value small{
    display: block;
    font-weight: 300;
    font-size: 14px;
    line-height: 1em;
}

.metric .icon {
    float: right;
    opacity: .3;
    font-size: 65px;
    border-radius: 0;
    width: 85px;
    height: 65px;
}

{% endstylesheet %}

<div class=""communicationview"">
    {% assign featureLink = Item | Attribute:'FeatureLink' -%}

    <div class=""feature"">

        <div class=""feature-image"" style="" background-image: url('/GetImage.ashx?Guid={{ Item | Attribute:'FeatureImage','RawValue' }}&w=2400&h=2400');""></div>
        <h1 class=""feature-title"">{{ Item | Attribute:'FeatureTitle' }}</h1>
        <p>
            {{ Item | Attribute:'FeatureText' }}
        </p>

        {% if featureLink != empty -%}
            <a class=""btn btn-xs btn-link"" href=""{{ featureLink }}"">More Info</a>
        {% endif -%}
    </div>

    <hr class=""margin-v-lg"" />

    <div class=""margin-b-lg"">
        {{ Item | Attribute:'Articles' }}
    </div>

    {% assign metricCount = Metrics | Size -%}

    {% if metricCount > 0 -%}
        <h1>Metrics</h1>

        <div class=""row"">
        {% for metric in Metrics -%}
            <div class=""col-lg-4"">
                <div class=""metric"">
                    <h5>{{ metric.Title }}</h5>
                    <span class=""date"">{{ metric.LastRunDateTime | Date:'sd' }}</span>
                    <i class=""icon {{ metric.IconCssClass  }}""></i>

                    <div class=""value"">
                        {{ metric.LastValue | AsInteger }}
                        <small>{{ metric.UnitsLabel }}</small>
                    </div>
                </div>
            </div>

            {% cycle '', '', '</div><div class=""row"">' %}
        {% endfor -%}
        </div>
    {% endif %}

</div>" );
            // Attrib Value for Block:Internal Communication View, Attribute:Metrics Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "F3B7330F-1E4B-45A8-AF38-587E1409F9D9", @"ecb1b552-9a3d-46fc-952b-d57dbc4a329d|073add0c-b1f3-43ab-8360-89a1ce05a95d,491061b7-1834-44da-8ea1-bb73b2d52ad3|073add0c-b1f3-43ab-8360-89a1ce05a95d,f0a24208-f8ac-4e04-8309-1a276885f6a6|073add0c-b1f3-43ab-8360-89a1ce05a95d" );
            // Attrib Value for Block:Internal Communication View, Attribute:Metric Value Count Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "89F1FFC4-A1FB-41DA-ACDF-ABDF15DAE062", @"0" );
            // Attrib Value for Block:Internal Communication View, Attribute:Cache Duration Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417", "8B673CFB-5BFB-4CDF-8FFA-6ECC9766B670", @"3600" );

            // Hide Person Search from Navigation Display
            Sql( @"UPDATE [Page] SET [DisplayInNavWhen] = 2 WHERE [Guid] = '5E036ADE-C2A4-4988-B393-DAC58230F02E'" );

            // Add Swimlane to Person
            RockMigrationHelper.UpdateBlockType("Person Group History","Displays a timeline of a person's history in groups","~/Blocks/Crm/PersonDetail/PersonGroupHistory.ascx","CRM > Person Detail","F8E351BC-607E-4897-B732-F590B5155451");
            // Add Block to Page: Groups, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D","","F8E351BC-607E-4897-B732-F590B5155451","Person Group History","SectionC1","","",0,"E18B1B2D-BF2A-43AD-BB9E-5DADBEFFB908");
            // Attrib for BlockType: Person Group History:Group Types
            RockMigrationHelper.UpdateBlockTypeAttribute("F8E351BC-607E-4897-B732-F590B5155451","F725B854-A15E-46AE-9D4C-0608D4154F1E","Group Types","GroupTypes","","List of Group Types that this block defaults to, and the user is able to choose from in the options filter. Leave blank to include all group types that have history enabled.",1,@"","82FA5003-05E8-4E22-8E5F-ED841DF4D9CB");
            // Move Group List to be Second on the Page
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '1CBE10C7-5E64-4385-BEE3-81DCA43DC47F'" );

            // Page: Group History Grid
            RockMigrationHelper.AddPage( true, "FCCF2570-DC09-4129-87BE-F1CAE25F1B9D","D65F783D-87A9-4CC9-8110-E83466A0EADB","Group History Grid","", Rock.SystemGuid.Page.GROUP_HISTORY_GRID, ""); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.GROUP_HISTORY_GRID,"group/{GroupId}/history/grid");
            RockMigrationHelper.UpdateBlockType("Group Member History","Displays a timeline of history for a group member","~/Blocks/Groups/GroupMemberHistory.ascx","Groups","EA6EA2E7-6504-41FE-AB55-0B1E7D04B226");
            // Add Block to Page: Group History Grid, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FB9A6BC0-0B51-4A92-A32C-58AC822CD2D0","","EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","Group Member History","Main","","",0,"C41CBC63-0DB6-4F69-8DEA-894580E27312");
            // Attrib for BlockType: Group Member History:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","9C204CD0-1233-41C5-818A-C5DA439445AA","core.CustomGridColumnsConfig","core.CustomGridColumnsConfig","","",0,@"","BFECBDEF-F723-4C97-A8C5-FBDC5707EA22");
            // Attrib for BlockType: Group Member History:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","core.CustomGridEnableStickyHeaders","core.CustomGridEnableStickyHeaders","","",0,@"False","80F5C53B-BA41-41CC-8EB6-2A89CA426622");
            // Attrib for BlockType: Group Member History:Timeline Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Timeline Lava Template","TimelineLavaTemplate","","The Lava Template to use when rendering the timeline view of the history.",1,@"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}","2928385B-09D9-4877-A35C-2A688F22DB22");
            // Attrib for BlockType: Group Member History:Group History Grid Page
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Group History Grid Page","GroupHistoryGridPage","","",2,@"","B72E26E2-5EC5-49BE-829B-18FB9AE12E47");
            // Attrib for BlockType: Group Member History:Group Member History Page
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Group Member History Page","GroupMemberHistoryPage","","",3,@"","A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896");

            // Page: Group Member History
            RockMigrationHelper.AddPage( true, "FCCF2570-DC09-4129-87BE-F1CAE25F1B9D","D65F783D-87A9-4CC9-8110-E83466A0EADB","Group Member History","", Rock.SystemGuid.Page.GROUP_MEMBER_HISTORY, ""); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.GROUP_MEMBER_HISTORY, "group/{GroupId}/history/groupmember/{GroupMemberId}" );
            RockMigrationHelper.UpdateBlockType("Group Member History","Displays a timeline of history for a group member","~/Blocks/Groups/GroupMemberHistory.ascx","Groups","EA6EA2E7-6504-41FE-AB55-0B1E7D04B226");
            // Add Block to Page: Group Member History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "EAAB757E-524F-4DB9-A124-D5EFBCDCA63B","","EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","Group Member History","Main","","",0,"03FB6DBD-3320-46A8-B3E1-662AE2C3FC41");
            // Attrib for BlockType: Group Member History:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","9C204CD0-1233-41C5-818A-C5DA439445AA","core.CustomGridColumnsConfig","core.CustomGridColumnsConfig","","",0,@"","BFECBDEF-F723-4C97-A8C5-FBDC5707EA22");
            // Attrib for BlockType: Group Member History:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","core.CustomGridEnableStickyHeaders","core.CustomGridEnableStickyHeaders","","",0,@"False","80F5C53B-BA41-41CC-8EB6-2A89CA426622");
            // Attrib for BlockType: Group Member History:Timeline Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Timeline Lava Template","TimelineLavaTemplate","","The Lava Template to use when rendering the timeline view of the history.",1,@"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}","2928385B-09D9-4877-A35C-2A688F22DB22");
            // Attrib for BlockType: Group Member History:Group History Grid Page
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Group History Grid Page","GroupHistoryGridPage","","",2,@"","B72E26E2-5EC5-49BE-829B-18FB9AE12E47");
            // Attrib for BlockType: Group Member History:Group Member History Page
            RockMigrationHelper.UpdateBlockTypeAttribute("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Group Member History Page","GroupMemberHistoryPage","","",3,@"","A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896");



            // Add Route to Page: Group History
            RockMigrationHelper.AddPageRoute("FCCF2570-DC09-4129-87BE-F1CAE25F1B9D","group/{GroupId}/history");
            // Attrib Value for Block:Group History, Attribute:Group Member History Page Page: Group History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C27FF3C0-D7BF-4CAD-B33D-C0A6953370FC","8CC92361-B724-4296-B7D0-75B5D33F66C9",@"eaab757e-524f-4db9-a124-d5efbcdca63b");
            // Attrib Value for Block:Group History, Attribute:Group History Grid Page Page: Group History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C27FF3C0-D7BF-4CAD-B33D-C0A6953370FC","6A7F08FE-256E-4FEA-886E-2545BF64887D",@"fb9a6bc0-0b51-4a92-a32c-58ac822cd2d0");


            // Enable Group History on Small Groups
            Sql( @"UPDATE [GroupType] SET [EnableGroupHistory] = 1, [GroupTypeColor] = '#03a9f4' WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" );
            // Enable Group History on Serving Team
            Sql( @"UPDATE [GroupType] SET [EnableGroupHistory] = 1, [GroupTypeColor] = '#4caf50' WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4'" );

            RockMigrationHelper.AddPageRoute("7EA94B4F-013B-4A79-8D01-86994EB04604","group/{GroupId}/attendance");
            RockMigrationHelper.AddPageRoute("60995C8C-862F-40F5-AFBB-13B49CDA77EB","group/{GroupId}/map");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Internal Communication View:Cache Duration
            RockMigrationHelper.DeleteAttribute( "8B673CFB-5BFB-4CDF-8FFA-6ECC9766B670" );
            // Attrib for BlockType: Internal Communication View:Metric Value Count
            RockMigrationHelper.DeleteAttribute( "89F1FFC4-A1FB-41DA-ACDF-ABDF15DAE062" );
            // Attrib for BlockType: Internal Communication View:Metrics
            RockMigrationHelper.DeleteAttribute( "F3B7330F-1E4B-45A8-AF38-587E1409F9D9" );
            // Attrib for BlockType: Internal Communication View:Body Template
            RockMigrationHelper.DeleteAttribute( "E7A96FBE-C05F-4079-896E-E84115A96077" );
            // Attrib for BlockType: Internal Communication View:Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "86044377-02E9-46C7-A90B-4CF4DA8F38B0" );
            // Attrib for BlockType: Internal Communication View:Content Channel
            RockMigrationHelper.DeleteAttribute( "0C490E5F-BCEC-4E0A-B4EC-D2A3AE0F0256" );
            // Attrib for BlockType: Internal Communication View:Block Title Template
            RockMigrationHelper.DeleteAttribute( "16E7B8AB-E5AC-4560-A060-A4E51DF8E390" );
            // Attrib for BlockType: Internal Communication View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "44A49D15-3A54-4AF7-A73C-F1AEB384C23A" );
            // Remove Block: Internal Communication View, from Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "879BC5A7-3CE2-43FC-BEDB-B93B0054F417" );
            RockMigrationHelper.DeleteBlockType( INTERNAL_COMMUNICATION_VIEW_BLOCKTYPE ); // Internal Communication View
            Sql( MigrationSQL._201805211735573_Homepage_Metrics_Down );
            RockMigrationHelper.DeleteCategory( "073add0c-b1f3-43ab-8360-89a1ce05a95d" );
            Sql( MigrationSQL._201805211735573_Homepage_ContentChannel_Down );
            RockMigrationHelper.DeleteEntityType( "3c9d5021-0484-4846-aef6-b6216d26c3c8" );

            // Remove Swimlane from Person
            RockMigrationHelper.DeleteBlock("E18B1B2D-BF2A-43AD-BB9E-5DADBEFFB908");
            // Attrib for BlockType: Group Member History:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.DeleteAttribute("80F5C53B-BA41-41CC-8EB6-2A89CA426622");
            // Attrib for BlockType: Group Member History:core.CustomGridColumnsConfig
            RockMigrationHelper.DeleteAttribute("BFECBDEF-F723-4C97-A8C5-FBDC5707EA22");
            // Attrib for BlockType: Group Member History:Group History Grid Page
            RockMigrationHelper.DeleteAttribute("B72E26E2-5EC5-49BE-829B-18FB9AE12E47");
            // Attrib for BlockType: Group Member History:Timeline Lava Template
            RockMigrationHelper.DeleteAttribute("2928385B-09D9-4877-A35C-2A688F22DB22");
            // Attrib for BlockType: Group Member History:Group Member History Page
            RockMigrationHelper.DeleteAttribute("A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896");
            // Remove Block from Page: Group History Grid, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("C41CBC63-0DB6-4F69-8DEA-894580E27312");
            RockMigrationHelper.DeleteBlockType("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226");
            RockMigrationHelper.DeletePage("FB9A6BC0-0B51-4A92-A32C-58AC822CD2D0"); //  Page: Group History Grid


            // Attrib for BlockType: Group Member History:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.DeleteAttribute("80F5C53B-BA41-41CC-8EB6-2A89CA426622");
            // Attrib for BlockType: Group Member History:core.CustomGridColumnsConfig
            RockMigrationHelper.DeleteAttribute("BFECBDEF-F723-4C97-A8C5-FBDC5707EA22");
            // Attrib for BlockType: Group Member History:Group History Grid Page
            RockMigrationHelper.DeleteAttribute("B72E26E2-5EC5-49BE-829B-18FB9AE12E47");
            // Attrib for BlockType: Group Member History:Timeline Lava Template
            RockMigrationHelper.DeleteAttribute("2928385B-09D9-4877-A35C-2A688F22DB22");
            // Attrib for BlockType: Group Member History:Group Member History Page
            RockMigrationHelper.DeleteAttribute("A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896");
            // Remove Block to Page: Group Member History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("03FB6DBD-3320-46A8-B3E1-662AE2C3FC41");
            RockMigrationHelper.DeleteBlockType("EA6EA2E7-6504-41FE-AB55-0B1E7D04B226");
            RockMigrationHelper.DeletePage("EAAB757E-524F-4DB9-A124-D5EFBCDCA63B"); //  Page: Group Member History

            RockMigrationHelper.DeleteAttribute("6A7F08FE-256E-4FEA-886E-2545BF64887D");
            RockMigrationHelper.DeleteAttribute("8CC92361-B724-4296-B7D0-75B5D33F66C9");

            // Disable Group History on Small Groups
            Sql( @"UPDATE [GroupType] SET [EnableGroupHistory] = 0 WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" );
            // Disable Group History on Serving Team
            Sql( @"UPDATE [GroupType] SET [EnableGroupHistory] = 0 WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4'" );
            // Move Group List to be First on the Page
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '1CBE10C7-5E64-4385-BEE3-81DCA43DC47F'" );
        }
    }
}
