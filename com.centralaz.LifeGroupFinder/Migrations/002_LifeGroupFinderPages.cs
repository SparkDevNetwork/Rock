using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class LifeGroupFinderPages : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Small Groups
            // Delete current Group Finder
            RockMigrationHelper.DeleteAttribute( "4D326BF5-EF92-455E-9B15-C4D5094D76FB" );
            RockMigrationHelper.DeleteAttribute( "7437E1B6-CCF4-4A00-8004-B6074F79C107" );
            RockMigrationHelper.DeleteAttribute( "9EFF53E3-256D-4251-8A7A-629AAAEB856D" );
            RockMigrationHelper.DeleteAttribute( "6A113765-5FEA-4714-93A1-6675AD5DF8FE" );
            RockMigrationHelper.DeleteAttribute( "71140493-AB2A-4040-8724-EA69F95FE264" );
            RockMigrationHelper.DeleteAttribute( "AFB2F4FD-772C-4238-9805-13897A600DCC" );
            RockMigrationHelper.DeleteAttribute( "1846B075-E3B2-4220-A458-8F74B67272B7" );
            RockMigrationHelper.DeleteAttribute( "9DA54BB6-986C-4723-8FE1-E3EF53119C6A" );
            RockMigrationHelper.DeleteAttribute( "16CC2B4B-3BA5-4B76-8601-8D5B91637B06" );
            RockMigrationHelper.DeleteAttribute( "2F71D27F-8EC1-4A13-A349-59229173F88B" );
            RockMigrationHelper.DeleteAttribute( "3CE83B17-1E68-4FC2-BDC3-33920C0BC99C" );
            RockMigrationHelper.DeleteAttribute( "FA863C93-132D-4888-BB51-E46603585199" );
            RockMigrationHelper.DeleteAttribute( "CCDE5B4C-D195-4E9C-9212-36068E8406C8" );
            RockMigrationHelper.DeleteAttribute( "6748E6B3-45B0-4FA1-8405-3FBEDF514BB1" );
            RockMigrationHelper.DeleteAttribute( "B6F965A8-0EF8-48DC-B599-D9131EC640CA" );
            RockMigrationHelper.DeleteAttribute( "1097A3DD-5A58-414B-A17C-DF679FBF12D6" );
            RockMigrationHelper.DeleteAttribute( "DACB3F11-AA38-4470-9D81-4BB0F7D43AA8" );
            RockMigrationHelper.DeleteAttribute( "ED4809CD-1F8A-491D-91F3-ED4C04E36F96" );
            RockMigrationHelper.DeleteAttribute( "CCF6E7CA-F3EA-490F-B910-DD1049B75B5A" );
            RockMigrationHelper.DeleteAttribute( "380D0978-AF4F-4EEC-B872-56F0FB9F91E4" );
            RockMigrationHelper.DeleteBlock( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571" );

            //Add custom Group Finder
            RockMigrationHelper.UpdateBlockType( "Life Group Search", "Central custom group search block.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupSearch.ascx", "Groups", "205531A1-C1BC-494C-911E-EE88D29969FB" );
            RockMigrationHelper.AddBlock( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "", "205531A1-C1BC-494C-911E-EE88D29969FB", "Life Group Finder", "Main", "", "", 1, "6E830C1F-E5BB-4BCB-AA59-807204134E3E" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Information Security Page", "InformationSecurityPage", "", "The page to navigate to for group details.", 0, @"", "97717892-A27D-4709-81FF-DF834DD3B730" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Life Group List Page", "LifeGroupListPage", "", "The page to navigate to for group details.", 0, @"", "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Life Group Map Page", "LifeGroupMapPage", "", "The page to navigate to for group details.", 0, @"", "B4463438-776A-4F4B-9AC5-6D993151004D" );
            RockMigrationHelper.AddBlockAttributeValue( "6E830C1F-E5BB-4BCB-AA59-807204134E3E", "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B", @"218263dc-0877-4956-9610-25e3b70a10f0" ); // Life Group List Page
            RockMigrationHelper.AddBlockAttributeValue( "6E830C1F-E5BB-4BCB-AA59-807204134E3E", "B4463438-776A-4F4B-9AC5-6D993151004D", @"012a3649-bca2-4106-918a-f1916c462f9d" ); // Life Group Map Page

            // Page: Life Group Campus Map
            RockMigrationHelper.AddPage( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Life Group Campus Map", "", "012A3649-BCA2-4106-918A-F1916C462F9D", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Life Group Map", "Block for people to find a group that matches their search parameters.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupMap.ascx", "com_central > Groups", "8798F5F3-301B-49CF-8FE3-60FE97E37F02" );
            RockMigrationHelper.AddBlock( "012A3649-BCA2-4106-918A-F1916C462F9D", "", "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "Life Group Map", "Main", "", "", 0, "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Attribute Columns", "AttributeColumns", "", "", 0, @"", "D21868E1-B2C8-4F2B-81EE-644E1AF2E37E" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Geofenced Group Type", "GeofencedGroupType", "", "", 0, @"", "6155B57C-D3A4-4D41-997E-0665BDAF7118" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Attribute Filters", "AttributeFilters", "", "", 0, @"", "ADAE235E-425B-439A-B2D0-5B6766F018DC" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map", "ShowMap", "", "", 0, @"False", "F0B5510E-79F7-41F3-BE18-5D2C2CA8CD12" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "", 0, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "F5FE7799-F4AE-47AF-AAB2-A13835D902FF" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", "", 0, @"600", "9C17E070-EED5-4BD5-BE97-BFF70B5B5214" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Fence", "ShowFence", "", "", 0, @"False", "036BC875-1BE2-4C63-8AC9-764B46F6D9BF" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "7BDAE237-6E49-47AC-9961-A45AFB69E240", "Polygon Colors", "PolygonColors", "", "", 0, @"#f37833|#446f7a|#afd074|#649dac|#f8eba2|#92d0df|#eaf7fc", "FCA23695-8DEC-4645-9BA2-8AC5CBA8AF1A" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map Info", "MapInfo", "", "", 0, @"
<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-b-sm'>
{% for attribute in Group.AttributeValues %}
    <strong>{{ attribute.AttributeName }}:</strong> {{ attribute.ValueFormatted }} <br />
{% endfor %}
</div>

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}

{% if LinkedPages.RegisterPage != '' %}
    <a class='btn btn-xs btn-action' href='{{ LinkedPages.RegisterPage }}?GroupId={{ Group.Id }}'>Register</a>
{% endif %}
", "851F9FD6-8985-497A-A89D-96A29AA9C117" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "", 0, @"", "BC7F8411-B0A7-46E0-9E78-F3AB4BD96B3A" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Lava Output", "ShowLavaOutput", "", "", 0, @"False", "E23C1C3A-3071-41BA-B12E-5BE04AF7B31E" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Output", "LavaOutput", "", "", 0, @"
", "C07A279B-DF84-4DD6-A54E-980E788E385E" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Lava Output Debug", "LavaOutputDebug", "", "", 0, @"False", "EF610CDC-23A0-4FEE-ADDA-1D6B7145D35B" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid", "ShowGrid", "", "", 0, @"False", "D5D9B5BF-5820-497E-BBD1-0CF8F414BDB3" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Schedule", "ShowSchedule", "", "", 0, @"False", "9F6FCCF4-06E2-4D80-B762-35C3FE9EB5DC" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Proximity", "ShowProximity", "", "", 0, @"False", "A61E2AF1-9BE4-4A20-8B4B-84C0968492AD" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Count", "ShowCount", "", "", 0, @"False", "FC042B7C-6E77-4964-BF3D-F78B42A7827E" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Age", "ShowAge", "", "", 0, @"False", "7CCB17B4-8502-4D85-82E5-56C725CFFFD7" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Map Info Debug", "MapInfoDebug", "", "", 0, @"False", "675C0B64-494B-4455-AB64-C90F95A23E6F" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "9C204CD0-1233-41C5-818A-C5DA439445AA", "ScheduleFilters", "ScheduleFilters", "", "", 0, @"", "AA66CCD8-E3AE-42B8-AA1B-EBC33067F689" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page to navigate to for group details.", 0, @"", "46BFE298-5F6D-4679-90ED-AAAE703F9BC1" );

            RockMigrationHelper.AddBlockTypeAttribute( "8798F5F3-301B-49CF-8FE3-60FE97E37F02", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Register Page", "RegisterPage", "", "The page to navigate to when registering for a group.", 1, @"", "DB09976B-46D1-4307-93C8-65885FE2A57D" );

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "D21868E1-B2C8-4F2B-81EE-644E1AF2E37E", @"" ); // Attribute Columns

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "6155B57C-D3A4-4D41-997E-0665BDAF7118", @"" ); // Geofenced Group Type

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "ADAE235E-425B-439A-B2D0-5B6766F018DC", @"" ); // Attribute Filters

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "F0B5510E-79F7-41F3-BE18-5D2C2CA8CD12", @"True" ); // Show Map

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "F5FE7799-F4AE-47AF-AAB2-A13835D902FF", @"223" ); // Map Style

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "9C17E070-EED5-4BD5-BE97-BFF70B5B5214", @"600" ); // Map Height

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "036BC875-1BE2-4C63-8AC9-764B46F6D9BF", @"False" ); // Show Fence

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "FCA23695-8DEC-4645-9BA2-8AC5CBA8AF1A", @"#f37833|#446f7a|#afd074|#649dac|#f8eba2|#92d0df|#eaf7fc" ); // Polygon Colors

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "851F9FD6-8985-497A-A89D-96A29AA9C117", @"<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-b-sm'>
{% for attribute in Group.AttributeValues %}
    <strong>{{ attribute.AttributeName }}:</strong> {{ attribute.ValueFormatted }} <br />
{% endfor %}
</div>

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}

{% if LinkedPages.RegisterPage != '' %}
    <a class='btn btn-xs btn-action' href='{{ LinkedPages.RegisterPage }}?GroupId={{ Group.Id }}'>Register</a>
{% endif %}
" ); // Map Info

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "BC7F8411-B0A7-46E0-9E78-F3AB4BD96B3A", @"50fcfb30-f51a-49df-86f4-2b176ea1820b" ); // Group Type

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "E23C1C3A-3071-41BA-B12E-5BE04AF7B31E", @"False" ); // Show Lava Output

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "C07A279B-DF84-4DD6-A54E-980E788E385E", @"" ); // Lava Output

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "EF610CDC-23A0-4FEE-ADDA-1D6B7145D35B", @"False" ); // Lava Output Debug

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "D5D9B5BF-5820-497E-BBD1-0CF8F414BDB3", @"False" ); // Show Grid

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "9F6FCCF4-06E2-4D80-B762-35C3FE9EB5DC", @"False" ); // Show Schedule

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "A61E2AF1-9BE4-4A20-8B4B-84C0968492AD", @"False" ); // Show Proximity

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "FC042B7C-6E77-4964-BF3D-F78B42A7827E", @"False" ); // Show Count

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "7CCB17B4-8502-4D85-82E5-56C725CFFFD7", @"False" ); // Show Age

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "675C0B64-494B-4455-AB64-C90F95A23E6F", @"False" ); // Map Info Debug

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "AA66CCD8-E3AE-42B8-AA1B-EBC33067F689", @"" ); // ScheduleFilters

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "46BFE298-5F6D-4679-90ED-AAAE703F9BC1", @"803a7786-6a83-43df-a247-bb4dff50aae8" ); // Group Detail Page

            RockMigrationHelper.AddBlockAttributeValue( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193", "DB09976B-46D1-4307-93C8-65885FE2A57D", @"" ); // Register Page

            // Page: Group Search List
            RockMigrationHelper.AddPage( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Group Search List", "", "218263DC-0877-4956-9610-25E3B70A10F0", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Life Group List", "Lists all groups for the configured group types.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupList.ascx", "com_centralaz > Groups", "57D90EE5-8425-448A-82F6-292D35CEAEAE" );
            RockMigrationHelper.AddBlock( "218263DC-0877-4956-9610-25E3B70A10F0", "", "57D90EE5-8425-448A-82F6-292D35CEAEAE", "Life Group List", "Main", "", "", 0, "48DBE5C1-14FF-4E69-AEC5-247EE43E6068" );

            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "39E42561-BEDB-450E-A026-98E617B6E70D" );

            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C54408D6-AF4E-4E01-A23C-D6E307C23206" );

            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Limit to Active Status", "LimittoActiveStatus", "", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", 10, @"all", "18146156-9AAE-4255-92B1-2CFE183CAF79" );

            // Page: Life Group Detail
            RockMigrationHelper.DeleteAttribute( "BCDB7126-5F5E-486D-84CB-C6D7E8F265FC" );
            RockMigrationHelper.DeleteAttribute( "E3544F7A-0E7A-421B-9142-AE858E9CCFBB" );
            RockMigrationHelper.DeleteAttribute( "B61174DE-6E7D-4171-9067-6A7981F888E8" );
            RockMigrationHelper.DeleteAttribute( "EDEC82F3-C9E1-4B26-862D-E896F6C26376" );
            RockMigrationHelper.DeleteAttribute( "C58B22F0-1CAC-436D-AFFE-5FC616F36DB1" );
            RockMigrationHelper.DeleteAttribute( "BAD40ACB-CC0B-4CE4-A3B8-E7C5134AE0E2" );
            RockMigrationHelper.DeleteAttribute( "8E37CB4A-AF69-4671-9EC6-2ED72380B749" );
            RockMigrationHelper.DeleteAttribute( "C4287E3F-D2D8-413E-A3AE-F9A3EE7A5021" );
            RockMigrationHelper.DeleteAttribute( "FBF13ACE-F9DC-4A28-87B3-2FA3D36FF55A" );
            RockMigrationHelper.DeleteBlock( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1" );
            RockMigrationHelper.DeleteBlockType( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7" );
            RockMigrationHelper.DeletePage( "7D24FE9A-710C-4B25-B1C7-76161ED78DB8" ); //  Page: Group Registration

            RockMigrationHelper.AddPage( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Life Group Detail", "", "803A7786-6A83-43DF-A247-BB4DFF50AAE8", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "803A7786-6A83-43DF-A247-BB4DFF50AAE8", "LifeGroup/{groupId}" );
            RockMigrationHelper.UpdateBlockType( "Life Group Detail", "Allows a person to register for a group.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupDetail.ascx", "com_centralaz > Groups", "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2" );
            RockMigrationHelper.AddBlock( "803A7786-6A83-43DF-A247-BB4DFF50AAE8", "", "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "Life Group Detail", "Main", "", "", 0, "18DC3025-3791-43C8-9805-113AF17D5942" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "", "The group member status to use when adding person to group (default: 'Pending'.)", 1, @"2", "A6C8D708-AB6F-4CED-AA0A-1923DDB5CF87" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 2, @"368DD475-242C-49C4-A42C-7278BE690CC2", "0E9F8D12-C389-43F2-90C6-AC449996F925" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 3, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "0E40AB39-FF5C-41FC-BE93-95594FF9AFB2" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 4, @"", "42D95717-2494-4E76-B837-C78AC6E8B139" );
            
            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Email Workflow", "EmailWorkflow", "", "An optional workflow to start when an email request is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 4, @"", "C3537C7E-7105-4E3C-8FF2-6FD956D5EC40" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 5, @"False", "C466E238-A22B-49E9-9EA6-D9B173AC9334" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the group details.", 6, @"
", "4E190755-5DD3-4D4D-987E-163889F8861A" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Result Page", "ResultPage", "", "An optional page to redirect user to after they have been registered for the group.", 7, @"", "00C4D211-78F0-424E-BD3F-4F73998C8CD4" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Result Lava Template", "ResultLavaTemplate", "", "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).", 8, @"
", "B6C4D550-D4AF-4B52-A889-0BCB488E5192" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "C3537C7E-7105-4E3C-8FF2-6FD956D5EC40" );
            RockMigrationHelper.DeleteAttribute( "A6C8D708-AB6F-4CED-AA0A-1923DDB5CF87" );
            RockMigrationHelper.DeleteAttribute( "4E190755-5DD3-4D4D-987E-163889F8861A" );
            RockMigrationHelper.DeleteAttribute( "42D95717-2494-4E76-B837-C78AC6E8B139" );
            RockMigrationHelper.DeleteAttribute( "00C4D211-78F0-424E-BD3F-4F73998C8CD4" );
            RockMigrationHelper.DeleteAttribute( "0E9F8D12-C389-43F2-90C6-AC449996F925" );
            RockMigrationHelper.DeleteAttribute( "C466E238-A22B-49E9-9EA6-D9B173AC9334" );
            RockMigrationHelper.DeleteAttribute( "0E40AB39-FF5C-41FC-BE93-95594FF9AFB2" );
            RockMigrationHelper.DeleteAttribute( "B6C4D550-D4AF-4B52-A889-0BCB488E5192" );
            RockMigrationHelper.DeleteBlock( "18DC3025-3791-43C8-9805-113AF17D5942" );
            RockMigrationHelper.DeleteBlockType( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2" );
            RockMigrationHelper.DeletePage( "803A7786-6A83-43DF-A247-BB4DFF50AAE8" ); //  Page: Life Group Detail

            RockMigrationHelper.DeleteAttribute( "18146156-9AAE-4255-92B1-2CFE183CAF79" );
            RockMigrationHelper.DeleteAttribute( "C54408D6-AF4E-4E01-A23C-D6E307C23206" );
            RockMigrationHelper.DeleteAttribute( "39E42561-BEDB-450E-A026-98E617B6E70D" );
            RockMigrationHelper.DeleteBlock( "48DBE5C1-14FF-4E69-AEC5-247EE43E6068" );
            RockMigrationHelper.DeleteBlockType( "57D90EE5-8425-448A-82F6-292D35CEAEAE" );
            RockMigrationHelper.DeletePage( "218263DC-0877-4956-9610-25E3B70A10F0" ); //  Page: Group Search List

            RockMigrationHelper.DeleteAttribute( "DB09976B-46D1-4307-93C8-65885FE2A57D" );
            RockMigrationHelper.DeleteAttribute( "46BFE298-5F6D-4679-90ED-AAAE703F9BC1" );
            RockMigrationHelper.DeleteAttribute( "AA66CCD8-E3AE-42B8-AA1B-EBC33067F689" );
            RockMigrationHelper.DeleteAttribute( "675C0B64-494B-4455-AB64-C90F95A23E6F" );
            RockMigrationHelper.DeleteAttribute( "7CCB17B4-8502-4D85-82E5-56C725CFFFD7" );
            RockMigrationHelper.DeleteAttribute( "FC042B7C-6E77-4964-BF3D-F78B42A7827E" );
            RockMigrationHelper.DeleteAttribute( "A61E2AF1-9BE4-4A20-8B4B-84C0968492AD" );
            RockMigrationHelper.DeleteAttribute( "9F6FCCF4-06E2-4D80-B762-35C3FE9EB5DC" );
            RockMigrationHelper.DeleteAttribute( "D5D9B5BF-5820-497E-BBD1-0CF8F414BDB3" );
            RockMigrationHelper.DeleteAttribute( "EF610CDC-23A0-4FEE-ADDA-1D6B7145D35B" );
            RockMigrationHelper.DeleteAttribute( "C07A279B-DF84-4DD6-A54E-980E788E385E" );
            RockMigrationHelper.DeleteAttribute( "E23C1C3A-3071-41BA-B12E-5BE04AF7B31E" );
            RockMigrationHelper.DeleteAttribute( "BC7F8411-B0A7-46E0-9E78-F3AB4BD96B3A" );
            RockMigrationHelper.DeleteAttribute( "851F9FD6-8985-497A-A89D-96A29AA9C117" );
            RockMigrationHelper.DeleteAttribute( "FCA23695-8DEC-4645-9BA2-8AC5CBA8AF1A" );
            RockMigrationHelper.DeleteAttribute( "036BC875-1BE2-4C63-8AC9-764B46F6D9BF" );
            RockMigrationHelper.DeleteAttribute( "9C17E070-EED5-4BD5-BE97-BFF70B5B5214" );
            RockMigrationHelper.DeleteAttribute( "F5FE7799-F4AE-47AF-AAB2-A13835D902FF" );
            RockMigrationHelper.DeleteAttribute( "F0B5510E-79F7-41F3-BE18-5D2C2CA8CD12" );
            RockMigrationHelper.DeleteAttribute( "ADAE235E-425B-439A-B2D0-5B6766F018DC" );
            RockMigrationHelper.DeleteAttribute( "6155B57C-D3A4-4D41-997E-0665BDAF7118" );
            RockMigrationHelper.DeleteAttribute( "D21868E1-B2C8-4F2B-81EE-644E1AF2E37E" );
            RockMigrationHelper.DeleteBlock( "27D8D74E-14CB-4E1B-8BEE-5E5B8EAF0193" );
            RockMigrationHelper.DeleteBlockType( "8798F5F3-301B-49CF-8FE3-60FE97E37F02" );
            RockMigrationHelper.DeletePage( "012A3649-BCA2-4106-918A-F1916C462F9D" ); //  Page: Life Group Campus Map

            RockMigrationHelper.DeleteAttribute( "B4463438-776A-4F4B-9AC5-6D993151004D" );
            RockMigrationHelper.DeleteAttribute( "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B" );
            RockMigrationHelper.DeleteAttribute( "97717892-A27D-4709-81FF-DF834DD3B730" );
            RockMigrationHelper.DeleteBlock( "6E830C1F-E5BB-4BCB-AA59-807204134E3E" );
            RockMigrationHelper.DeleteBlockType( "205531A1-C1BC-494C-911E-EE88D29969FB" ); //Small groups
        }
    }
}
