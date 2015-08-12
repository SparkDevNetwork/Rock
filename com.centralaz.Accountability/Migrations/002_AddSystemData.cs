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
    [MigrationNumber( 2, "1.0.14" )]
    public class AddSystemData : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE, "Accountability Group", "A group type for accountability groups", "ECF09F24-41DD-4E57-9642-A4CAE0B29E14", false );
            RockMigrationHelper.AddGroupType( "Accountability Group", "", "Group", "Member", false, true, true, "fa fa-eye", 0, null, 0, null, "DC99BF69-8A1A-411F-A267-1AE75FDC2341", false );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "DC99BF69-8A1A-411F-A267-1AE75FDC2341", Rock.SystemGuid.FieldType.DATE, "Report Start Date", "The first report submit date for the group", 0, "10/1/2014", "37D911E7-5E0A-45CE-A35A-91F75480DB6B" );
           
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Accountability Groups", "", "1D5A2E3C-8D8A-4086-9FF5-CE173874568D", "fa fa-eye" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "1D5A2E3C-8D8A-4086-9FF5-CE173874568D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Accountability Group Detail", "", "6B3C673B-E693-48EF-8262-C9EA46B70C3F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "6B3C673B-E693-48EF-8262-C9EA46B70C3F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Detail Page", "", "AAD50DFC-5412-4DD1-B41A-E1840F848FC8", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Accountability Group Types", "", "D3CDF45D-F938-45AB-8CED-BF6E75465AA9", "fa fa-eye" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "D3CDF45D-F938-45AB-8CED-BF6E75465AA9", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Accountability Group Type Detail", "", "ADB9288D-4012-4E98-9A30-8C6235FFB2F0", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "7625A63E-6650-4886-B605-53C2234FA5E1", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Accountability Groups", "", "3B84E3FA-64E4-4783-BE0A-20E7C237D5AE", "" ); // Site:External Website
            RockMigrationHelper.AddPage( "3B84E3FA-64E4-4783-BE0A-20E7C237D5AE", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Application Group Detail", "", "249F1E4C-C290-4BD3-B971-079F9595FA9C", "" ); // Site:External Website
            RockMigrationHelper.AddPage( "249F1E4C-C290-4BD3-B971-079F9595FA9C", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Submit Report Page", "", "64B5B1F6-472A-4C64-85B5-1F6864FE1992", "" ); // Site:External Website
            RockMigrationHelper.AddPage( "249F1E4C-C290-4BD3-B971-079F9595FA9C", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Group Member Detail Page", "", "08079646-1016-4680-BCC6-809EB7C58095", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Groups of Group Type List", "Lists all groups for the configured group types.", "~/Plugins/com_centralaz/Accountability/GroupsOfGroupTypeList.ascx", "com_centralaz > Accountability", "71D20566-4DCA-4673-8AD4-B6EB237F903F" );
            RockMigrationHelper.UpdateBlockType( "Accountability Group Type Detail", "Displays the details of an Accountability Group Type.", "~/Plugins/com_centralaz/Accountability/AccountabilityGroupTypeDetail.ascx", "com_centralaz > Accountability", "8F3A56F3-398B-4008-8E7D-1CB649E80DA5" );
            RockMigrationHelper.UpdateBlockType( "Accountability Question List", "Lists all the questions for the Accountability Group Type.", "~/Plugins/com_centralaz/Accountability/AccountabilityQuestionList.ascx", "com_centralaz > Accountability", "E409A44F-55A1-4AE5-A6B3-C7CCCCF46190" );
            RockMigrationHelper.UpdateBlockType( "Accountability Group Detail", "Displays the details of the given accountability group.", "~/Plugins/com_centralaz/Accountability/AccountabilityGroupDetail.ascx", "com_centralaz > Accountability", "0F962947-0DE7-408D-BCBE-24B44E0CD14C" );
            RockMigrationHelper.UpdateBlockType( "Accountability Group Member List", "Lists all members in the accountability group", "~/Plugins/com_centralaz/Accountability/AccountabilityGroupMemberList.ascx", "com_centralaz > Accountability", "89E7E8CD-A692-4EB6-B0E5-D1C63D2C7428" );
            RockMigrationHelper.UpdateBlockType( "Submit Report", "The Submit Report Block", "~/Plugins/com_centralaz/Accountability/SubmitReportBlock.ascx", "com_centralaz > Accountability", "D159193A-BB75-410F-8531-BE4E7683B71C" );
            RockMigrationHelper.UpdateBlockType( "Accountability Group Member Detail", "Shows the detail for a group Member", "~/Plugins/com_centralaz/Accountability/AccountabilityGroupMemberDetail.ascx", "com_centralaz > Accountability", "0AAFB967-7254-49F6-9D2B-7698E50DFA38" );
            RockMigrationHelper.UpdateBlockType( "Accountability Question Score", "Shows the score for each question", "~/Plugins/com_centralaz/Accountability/QuestionScore.ascx", "com_centralaz > Accountability", "ADEC3020-B1D8-4EC1-8D11-3D6AEBD29DF7" );
            RockMigrationHelper.UpdateBlockType( "Accountability Report List", "A list of reports for a certain group member", "~/Plugins/com_centralaz/Accountability/AccountabilityReportList.ascx", "com_centralaz > Accountability", "5EA51F75-E9AE-4724-927B-962C95449491" );
            RockMigrationHelper.UpdateBlockType( "Accountability Report Form", "Block for accountability group members to fill out and submit reports", "~/Plugins/com_centralaz/Accountability/AccountabilityReportForm.ascx", "com_centralaz > Accountability", "7DEAD984-AB96-44BB-8688-1B42D457348C" );
            RockMigrationHelper.UpdateBlockType( "Accountability Question Summary", "Block for accountability group members to view question response stats", "~/Plugins/com_centralaz/Accountability/AccountabilityQuestionSummary.ascx", "com_centralaz > Accountability", "B21E3F69-350E-495E-9339-4AFDEB6BBBC5" );
            RockMigrationHelper.UpdateBlockType( "Accountability Group List", "Lists all accountability groups for the configured group types.", "~/Plugins/com_centralaz/Accountability/AccountabilityGroupList.ascx", "com_centralaz > Accountability", "7D1000EC-77F1-440B-8B30-3BE96395255B" );
           
            // Add Block to Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1D5A2E3C-8D8A-4086-9FF5-CE173874568D", "", "7D1000EC-77F1-440B-8B30-3BE96395255B", "My Groups", "Main", "", "", 0, "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E" );

            // Add Block to Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.AddBlock( "AAD50DFC-5412-4DD1-B41A-E1840F848FC8", "", "0AAFB967-7254-49F6-9D2B-7698E50DFA38", "Detail", "Main", "", "", 0, "9A9FC9F1-A8D3-4B60-BE77-FDF13813FB8D" );

            // Add Block to Page: Accountability Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6B3C673B-E693-48EF-8262-C9EA46B70C3F", "", "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "Group Detail", "Main", "", "", 0, "12909FB9-0316-4CC8-9DD5-FF16E3061748" );

            // Add Block to Page: Accountability Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6B3C673B-E693-48EF-8262-C9EA46B70C3F", "", "89E7E8CD-A692-4EB6-B0E5-D1C63D2C7428", "Group Members", "Main", "", "", 2, "EF11659E-1DC9-4552-A691-69FDB555F4B6" );

            // Add Block to Page: Accountability Group Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D3CDF45D-F938-45AB-8CED-BF6E75465AA9", "", "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E", "Accountability Group Types", "Main", "", "", 0, "49E50E18-886D-4DFA-8E4C-4B7542AFC78B" );

            // Add Block to Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "ADB9288D-4012-4E98-9A30-8C6235FFB2F0", "", "8F3A56F3-398B-4008-8E7D-1CB649E80DA5", "Accountability Group Type Detail", "Main", "", "", 0, "77365C1D-E675-4082-A490-B008D54DDAA2" );

            // Add Block to Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "ADB9288D-4012-4E98-9A30-8C6235FFB2F0", "", "E409A44F-55A1-4AE5-A6B3-C7CCCCF46190", "Questions", "Main", "", "", 1, "9F669732-BE5B-47DE-9F87-889DF333AA51" );

            // Add Block to Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "ADB9288D-4012-4E98-9A30-8C6235FFB2F0", "", "71D20566-4DCA-4673-8AD4-B6EB237F903F", "Accountability Groups", "Main", "", "", 2, "23887B0A-B1CF-4948-B134-DAFFE33486B1" );

            // Add Block to Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlock( "3B84E3FA-64E4-4783-BE0A-20E7C237D5AE", "", "7D1000EC-77F1-440B-8B30-3BE96395255B", "Your Application Groups", "Main", "", "", 0, "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1" );

            // Add Block to Page: Application Group Detail, Site: External Website
            RockMigrationHelper.AddBlock( "249F1E4C-C290-4BD3-B971-079F9595FA9C", "", "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "Group Detail", "Main", "", "", 0, "6874752A-8659-4195-9D70-5D95E2C6FDA0" );

            // Add Block to Page: Application Group Detail, Site: External Website
            RockMigrationHelper.AddBlock( "249F1E4C-C290-4BD3-B971-079F9595FA9C", "", "D159193A-BB75-410F-8531-BE4E7683B71C", "Submit Report", "Main", "", "", 1, "22774F85-F508-4C5A-9630-76B7074DB587" );

            // Add Block to Page: Application Group Detail, Site: External Website
            RockMigrationHelper.AddBlock( "249F1E4C-C290-4BD3-B971-079F9595FA9C", "", "89E7E8CD-A692-4EB6-B0E5-D1C63D2C7428", "Group Members", "Main", "", "", 2, "E3392C37-FBF8-487B-960B-DF3EF6B0ED45" );

            // Add Block to Page: Group Member Detail Page, Site: External Website
            RockMigrationHelper.AddBlock( "08079646-1016-4680-BCC6-809EB7C58095", "", "0AAFB967-7254-49F6-9D2B-7698E50DFA38", "Member Detail", "Main", "", "", 0, "DF3FF4DA-5702-4E14-8533-DBE2512ED54F" );

            // Add Block to Page: Group Member Detail Page, Site: External Website
            RockMigrationHelper.AddBlock( "08079646-1016-4680-BCC6-809EB7C58095", "", "ADEC3020-B1D8-4EC1-8D11-3D6AEBD29DF7", "Score", "Main", "", "", 1, "71EB35E0-5123-4818-B692-B687A9A298B1" );

            // Add Block to Page: Group Member Detail Page, Site: External Website
            RockMigrationHelper.AddBlock( "08079646-1016-4680-BCC6-809EB7C58095", "", "5EA51F75-E9AE-4724-927B-962C95449491", "Report List", "Main", "", "", 2, "AFB5D47D-5F99-4EC6-B1E6-07913E5B5B1A" );

            // Add Block to Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.AddBlock( "AAD50DFC-5412-4DD1-B41A-E1840F848FC8", "", "ADEC3020-B1D8-4EC1-8D11-3D6AEBD29DF7", "Accountability Question Score", "Main", "", "", 1, "05DB5B27-57CE-4E40-8198-E3A0FDDD849A" );

            // Add Block to Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.AddBlock( "AAD50DFC-5412-4DD1-B41A-E1840F848FC8", "", "5EA51F75-E9AE-4724-927B-962C95449491", "Accountability Report List", "Main", "", "", 2, "8E1E225A-70D7-4ADE-A061-632E5828733D" );

            // Add Block to Page: Submit Report Page, Site: External Website
            RockMigrationHelper.AddBlock( "64B5B1F6-472A-4C64-85B5-1F6864FE1992", "", "B21E3F69-350E-495E-9339-4AFDEB6BBBC5", "Accountability Question Summary", "Main", "", "", 0, "FDE47F60-EBCE-499D-9573-9C2E5EF9D415" );

            // Add Block to Page: Submit Report Page, Site: External Website
            RockMigrationHelper.AddBlock( "64B5B1F6-472A-4C64-85B5-1F6864FE1992", "", "7DEAD984-AB96-44BB-8688-1B42D457348C", "Accountability Report Form", "Main", "", "", 1, "8A9E4226-E00F-4D04-A062-6B6A5B7F9011" );

            // Attrib for BlockType: Group Type Map:Info Window Contents
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 9, @"
<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.PostalCode}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            {% for PhoneType in GroupMember.PhoneTypes %}
                <br>{{PhoneType.Name}}: {{PhoneType.Number}}
            {% endfor %}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

", "6C693AFE-4A4B-4FD4-95E4-F971B3092BF5" );

            // Attrib for BlockType: Group Type Map:Map Height
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", "Height of the map in pixels (default value is 600px)", 2, @"600", "AAFBD952-5883-42A8-BB7F-F18E0F997DC5" );

            // Attrib for BlockType: Group Type Map:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "Page to use as a link to the group details (optional).", 3, @"", "11F50134-9D30-43E4-9B31-3D841C4FE371" );

            // Attrib for BlockType: Group Type Map:Person Profile Page
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page to use as a link to the person profile page (optional).", 4, @"", "28059437-E68D-46DA-9338-EDC73FEB519C" );

            // Attrib for BlockType: Group Type Map:Show Map Info Window
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map Info Window", "ShowMapInfoWindow", "", "Control whether a info window should be displayed when clicking on a map point.", 5, @"True", "6E270E29-7F37-43C9-A166-FC0856E632E9" );

            // Attrib for BlockType: Group Type Map:Include Inactive Groups
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Groups", "IncludeInactiveGroups", "", "Determines if inactive groups should be included on the map.", 6, @"False", "51ECA642-8750-4298-A20B-03507097B058" );

            // Attrib for BlockType: Group Type Map:Attributes
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attributes", "Attributes", "", "Comma delimited list of attribute keys to include values for in the map info window (e.g. 'StudyTopic,MeetingTime').", 7, @"", "C5AB0DDE-19B7-4C93-B846-291C338694C2" );

            // Attrib for BlockType: Group Type Map:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the map.", 8, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "BDE99914-D06A-4890-9E48-B7F7E0BC972F" );

            // Attrib for BlockType: Group Type Map:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Enabling debug will display the fields of the first 5 groups to help show you wants available for your liquid.", 10, @"False", "7BA095EE-84A9-4897-B602-0F70B4FC6FBF" );

            // Attrib for BlockType: Group Type Map:Group Type
            RockMigrationHelper.AddBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "The type of group to map.", 0, @"", "F43F5642-AD2B-46A1-92F4-7089B1C9C008" );

            // Attrib for BlockType: Group List:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "3D04DEA4-5B7C-476C-B53C-C460E7916E8E" );

            // Attrib for BlockType: Group List:Display Filter
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Filter", "DisplayFilter", "", "Should filter be displayed to allow filtering by group type?", 7, @"False", "0BD18C8D-8950-4B6D-A157-A5F0642310A5" );

            // Attrib for BlockType: Group List:Display Active Status Column
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Active Status Column", "DisplayActiveStatusColumn", "", "Should the Active Status column be displayed?", 6, @"False", "71554EE9-49E2-4DAD-BA00-C0B9A858D30A" );

            // Attrib for BlockType: Group List:Display Description Column
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Description Column", "DisplayDescriptionColumn", "", "Should the Description column be displayed?", 5, @"True", "D3C75DEE-5C9D-4664-83D2-F0D5C82FC6BF" );

            // Attrib for BlockType: Group List:Display System Column
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display System Column", "DisplaySystemColumn", "", "Should the System column be displayed?", 6, @"True", "239D4D60-030A-437B-8CB0-97E9BE1161EA" );

            // Attrib for BlockType: Group List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "42DE3456-BF32-475B-8ECD-347EA4846886" );

            // Attrib for BlockType: Group List:Limit to Security Role Groups
            RockMigrationHelper.AddBlockTypeAttribute( "71D20566-4DCA-4673-8AD4-B6EB237F903F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?", 2, @"False", "EBAD4F97-C320-4A36-89C8-24BD7FFAFF11" );

            // Attrib for BlockType: Submit Report:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "D159193A-BB75-410F-8531-BE4E7683B71C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "014A4F42-2D91-43FF-A066-81C92AD52BAC" );

            // Attrib for BlockType: Accountability Group Member List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "89E7E8CD-A692-4EB6-B0E5-D1C63D2C7428", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "440119C0-8DBF-4768-AC5F-E2B18960B799" );

            // Attrib for BlockType: Accountability Group Member List:Add Member Page
            RockMigrationHelper.AddBlockTypeAttribute( "89E7E8CD-A692-4EB6-B0E5-D1C63D2C7428", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Member Page", "AddMemberPage", "", "", 0, @"", "7EFF5A6D-07F2-4A66-8D68-85CD5A2D1668" );

            // Attrib for BlockType: Accountability Group Detail:Group Map Page
            RockMigrationHelper.AddBlockTypeAttribute( "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Map Page", "GroupMapPage", "", "The page to display detailed group map.", 0, @"", "F1DA2972-6BBC-42A4-945D-144381725B55" );

            // Attrib for BlockType: Accountability Group Detail:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The style of maps to use", 4, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "FE342E27-6950-43D1-AEF0-ACE01FBEE67A" );

            // Attrib for BlockType: Accountability Group Detail:Group Types
            RockMigrationHelper.AddBlockTypeAttribute( "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "", "Select group types to show in this block.  Leave all unchecked to show all group types.", 0, @"", "21B030FD-108E-4AB3-A21E-4858F09BF8C5" );

            // Attrib for BlockType: Accountability Group Detail:Show Edit
            RockMigrationHelper.AddBlockTypeAttribute( "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Edit", "ShowEdit", "", "", 1, @"True", "50A3724A-219C-4611-AC52-B64FB1D52A78" );

            // Attrib for BlockType: Accountability Group Detail:Limit to Group Types that are shown in navigation
            RockMigrationHelper.AddBlockTypeAttribute( "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Group Types that are shown in navigation", "LimitToShowInNavigationGroupTypes", "", "", 3, @"False", "4E1C2BA2-52E3-4777-88ED-255DF39D3DFE" );

            // Attrib for BlockType: Accountability Group Detail:Limit to Security Role Groups
            RockMigrationHelper.AddBlockTypeAttribute( "0F962947-0DE7-408D-BCBE-24B44E0CD14C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "", 2, @"False", "1DF95A83-3767-4257-9033-2B6D5D96D479" );

            // Attrib for BlockType: Accountability Group Member Detail:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "0AAFB967-7254-49F6-9D2B-7698E50DFA38", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "24FBAA42-E8D0-4CA1-83C6-186314A96EB4" );

            // Attrib for BlockType: Accountability Group List:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "36A15DB2-0FE4-4F46-A8DD-21CE4D480A21" );

            // Attrib for BlockType: Accountability Group List:Display Group Type Column
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Group Type Column", "DisplayGroupTypeColumn", "", "Should the Group Type column be displayed?", 4, @"True", "D48538ED-E3F6-4C78-AD5E-FEB97DBD9F51" );

            // Attrib for BlockType: Accountability Group List:Display Active Status Column
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Active Status Column", "DisplayActiveStatusColumn", "", "Should the Active Status column be displayed?", 6, @"False", "5ADDA3CE-3F11-4BA0-8454-27C22440BF64" );

            // Attrib for BlockType: Accountability Group List:Display System Column
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display System Column", "DisplaySystemColumn", "", "Should the System column be displayed?", 6, @"True", "40FBFB68-AA43-4875-AAF0-419A2094CA71" );

            // Attrib for BlockType: Accountability Group List:Display Description Column
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Description Column", "DisplayDescriptionColumn", "", "Should the Description column be displayed?", 5, @"True", "E21D3EC0-0114-45E7-83A2-13B5B8A60FE4" );

            // Attrib for BlockType: Accountability Group List:Limit to Security Role Groups
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?", 2, @"False", "44851A1A-F6C1-4021-8E0E-B9B379544D65" );

            // Attrib for BlockType: Accountability Group List:Include Group Types
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Include Group Types", "IncludeGroupTypes", "", "The group types to display in the list.  If none are selected, all group types will be included.", 1, @"", "278D773B-5C83-4F21-B589-EB2609F144EC" );

            // Attrib for BlockType: Accountability Group List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "855F6AD7-A7A9-4BD0-A4FE-66C71F5C0457" );

            // Attrib for BlockType: Accountability Group List:Exclude Group Types
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Exclude Group Types", "ExcludeGroupTypes", "", "The group types to exclude from the list (only valid if including all groups).", 3, @"", "A7DC20BB-9B16-46E2-899A-D124F79C372E" );

            // Attrib for BlockType: Accountability Group List:Display Filter
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Filter", "DisplayFilter", "", "Should filter be displayed to allow filtering by group type?", 7, @"False", "ED9D3696-A77C-4310-82B1-35E1B562EB1D" );

            // Attrib for BlockType: Accountability Group List:Is Editor
            RockMigrationHelper.AddBlockTypeAttribute( "7D1000EC-77F1-440B-8B30-3BE96395255B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Editor", "IsEditor", "", "", 0, @"False", "01B835A6-7040-4111-B7C7-5D396A08BCA7" );

            // Attrib for BlockType: Accountability Group Member Detail:Able to choose leader
            RockMigrationHelper.AddBlockTypeAttribute( "0AAFB967-7254-49F6-9D2B-7698E50DFA38", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Able to choose leader", "Abletochooseleader", "", "", 0, @"False", "04B74B24-274E-43D4-9148-5E432390014A" );

            // Attrib Value for Block:My Groups, Attribute:Entity Type Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "A32C7EFE-2E5F-4E99-9867-DD562407B72E", @"d155c373-9e47-4c6a-badd-792f31af5fba" );

            // Attrib Value for Block:My Groups, Attribute:Limit to Security Role Groups Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Detail Page Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"6b3c673b-e693-48ef-8262-c9ea46b70c3f" );

            // Attrib Value for Block:My Groups, Attribute:Display Filter Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Include Group Types Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"" );

            // Attrib Value for Block:My Groups, Attribute:Exclude Group Types Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"3981cf6d-7d15-4b57-aace-c0e25d28bd49,13a6139d-eeec-412d-8572-773eca1939cc,0572a5fe-20a4-4bf1-95cd-c71db5281392,4f9565a7-dd5a-41c3-b4e8-13f0b872b10b,caaf4f9b-58b9-45b4-aabc-9188347948b7,e3c8f7d6-5ceb-43bb-802f-66c3e734049e,790e3215-3b10-442b-af69-616c0dcb998e,8400497b-c52f-40ae-a529-3fccb9587101,9a88743b-f336-4404-b877-2a623689195d,8c0e5852-f08f-4327-9aa5-87800a6ab53e,7a17235b-69ad-439b-bab0-1a0a472db96f,e0c5a0e2-b7b3-4ef4-820d-bbf7f9a374ef,cadb2d12-7836-44bc-8eea-3c6ab22fd5e8,aece949f-704c-483e-a4fb-93d5e4720c4c,2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4,50fcfb30-f51a-49df-86f4-2b176ea1820b,fab75ec6-0402-456a-be34-252097de4f20,fedd389a-616f-4a53-906c-63d8255631c5" );

            // Attrib Value for Block:My Groups, Attribute:Display Group Type Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"True" );

            // Attrib Value for Block:My Groups, Attribute:Display Description Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Display System Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Display Active Status Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Entity Type Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "36A15DB2-0FE4-4F46-A8DD-21CE4D480A21", @"" );

            // Attrib Value for Block:My Groups, Attribute:Display Group Type Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "D48538ED-E3F6-4C78-AD5E-FEB97DBD9F51", @"True" );

            // Attrib Value for Block:My Groups, Attribute:Display Active Status Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "5ADDA3CE-3F11-4BA0-8454-27C22440BF64", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Display System Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "40FBFB68-AA43-4875-AAF0-419A2094CA71", @"True" );

            // Attrib Value for Block:My Groups, Attribute:Display Description Column Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "E21D3EC0-0114-45E7-83A2-13B5B8A60FE4", @"True" );

            // Attrib Value for Block:My Groups, Attribute:Limit to Security Role Groups Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "44851A1A-F6C1-4021-8E0E-B9B379544D65", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Include Group Types Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "278D773B-5C83-4F21-B589-EB2609F144EC", @"" );

            // Attrib Value for Block:My Groups, Attribute:Detail Page Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "855F6AD7-A7A9-4BD0-A4FE-66C71F5C0457", @"6b3c673b-e693-48ef-8262-c9ea46b70c3f" );

            // Attrib Value for Block:My Groups, Attribute:Exclude Group Types Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "A7DC20BB-9B16-46E2-899A-D124F79C372E", @"" );

            // Attrib Value for Block:My Groups, Attribute:Display Filter Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "ED9D3696-A77C-4310-82B1-35E1B562EB1D", @"False" );

            // Attrib Value for Block:My Groups, Attribute:Is Editor Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E", "01B835A6-7040-4111-B7C7-5D396A08BCA7", @"True" );

            // Attrib Value for Block:Detail, Attribute:Detail Page Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9A9FC9F1-A8D3-4B60-BE77-FDF13813FB8D", "24FBAA42-E8D0-4CA1-83C6-186314A96EB4", @"21d83fea-5da7-4343-93b5-806b0da4e1d5" );

            // Attrib Value for Block:Detail, Attribute:Able to choose leader Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9A9FC9F1-A8D3-4B60-BE77-FDF13813FB8D", "04B74B24-274E-43D4-9148-5E432390014A", @"True" );

            // Attrib Value for Block:Group Members, Attribute:Detail Page Page: Accountability Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EF11659E-1DC9-4552-A691-69FDB555F4B6", "440119C0-8DBF-4768-AC5F-E2B18960B799", @"aad50dfc-5412-4dd1-b41a-e1840f848fc8" );

            // Attrib Value for Block:Group Members, Attribute:Add Member Page Page: Accountability Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EF11659E-1DC9-4552-A691-69FDB555F4B6", "7EFF5A6D-07F2-4A66-8D68-85CD5A2D1668", @"aad50dfc-5412-4dd1-b41a-e1840f848fc8" );

            // Attrib Value for Block:Accountability Group Types, Attribute:Detail Page Page: Accountability Group Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "49E50E18-886D-4DFA-8E4C-4B7542AFC78B", "54731F4A-041F-47BD-B908-91ECC592D679", @"adb9288d-4012-4e98-9a30-8c6235ffb2f0" );

            // Attrib Value for Block:Accountability Groups, Attribute:Entity Type Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "3D04DEA4-5B7C-476C-B53C-C460E7916E8E", @"" );

            // Attrib Value for Block:Accountability Groups, Attribute:Display Filter Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "0BD18C8D-8950-4B6D-A157-A5F0642310A5", @"False" );

            // Attrib Value for Block:Accountability Groups, Attribute:Display Active Status Column Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "71554EE9-49E2-4DAD-BA00-C0B9A858D30A", @"False" );

            // Attrib Value for Block:Accountability Groups, Attribute:Display Description Column Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "D3C75DEE-5C9D-4664-83D2-F0D5C82FC6BF", @"True" );

            // Attrib Value for Block:Accountability Groups, Attribute:Display System Column Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "239D4D60-030A-437B-8CB0-97E9BE1161EA", @"True" );

            // Attrib Value for Block:Accountability Groups, Attribute:Detail Page Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "42DE3456-BF32-475B-8ECD-347EA4846886", @"6b3c673b-e693-48ef-8262-c9ea46b70c3f" );

            // Attrib Value for Block:Accountability Groups, Attribute:Limit to Security Role Groups Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "23887B0A-B1CF-4948-B134-DAFFE33486B1", "EBAD4F97-C320-4A36-89C8-24BD7FFAFF11", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Active Status Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display System Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"True" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Description Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"True" );

            // Attrib Value for Block:Your Application Groups, Attribute:Limit to Security Role Groups Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Detail Page Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"249f1e4c-c290-4bd3-b971-079f9595fa9c" );

            // Attrib Value for Block:Your Application Groups, Attribute:Exclude Group Types Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"fb2eeb40-68b7-49d5-9315-1dd3fc3e0699,3981cf6d-7d15-4b57-aace-c0e25d28bd49,13a6139d-eeec-412d-8572-773eca1939cc,0572a5fe-20a4-4bf1-95cd-c71db5281392,4f9565a7-dd5a-41c3-b4e8-13f0b872b10b,caaf4f9b-58b9-45b4-aabc-9188347948b7,e3c8f7d6-5ceb-43bb-802f-66c3e734049e,790e3215-3b10-442b-af69-616c0dcb998e,8400497b-c52f-40ae-a529-3fccb9587101,9a88743b-f336-4404-b877-2a623689195d,8c0e5852-f08f-4327-9aa5-87800a6ab53e,7a17235b-69ad-439b-bab0-1a0a472db96f,e0c5a0e2-b7b3-4ef4-820d-bbf7f9a374ef,cadb2d12-7836-44bc-8eea-3c6ab22fd5e8,aece949f-704c-483e-a4fb-93d5e4720c4c,2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4,50fcfb30-f51a-49df-86f4-2b176ea1820b,fab75ec6-0402-456a-be34-252097de4f20,fedd389a-616f-4a53-906c-63d8255631c5" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Group Type Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"True" );

            // Attrib Value for Block:Your Application Groups, Attribute:Include Group Types Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"" );

            // Attrib Value for Block:Your Application Groups, Attribute:Entity Type Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "A32C7EFE-2E5F-4E99-9867-DD562407B72E", @"49668b95-fedc-43dd-8085-d2b0d6343c48" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Filter Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Is Editor Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "01B835A6-7040-4111-B7C7-5D396A08BCA7", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Filter Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "ED9D3696-A77C-4310-82B1-35E1B562EB1D", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Exclude Group Types Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "A7DC20BB-9B16-46E2-899A-D124F79C372E", @"" );

            // Attrib Value for Block:Your Application Groups, Attribute:Detail Page Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "855F6AD7-A7A9-4BD0-A4FE-66C71F5C0457", @"249f1e4c-c290-4bd3-b971-079f9595fa9c" );

            // Attrib Value for Block:Your Application Groups, Attribute:Limit to Security Role Groups Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "44851A1A-F6C1-4021-8E0E-B9B379544D65", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Include Group Types Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "278D773B-5C83-4F21-B589-EB2609F144EC", @"" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Description Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "E21D3EC0-0114-45E7-83A2-13B5B8A60FE4", @"True" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display System Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "40FBFB68-AA43-4875-AAF0-419A2094CA71", @"True" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Active Status Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "5ADDA3CE-3F11-4BA0-8454-27C22440BF64", @"False" );

            // Attrib Value for Block:Your Application Groups, Attribute:Display Group Type Column Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "D48538ED-E3F6-4C78-AD5E-FEB97DBD9F51", @"True" );

            // Attrib Value for Block:Your Application Groups, Attribute:Entity Type Page: Accountability Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1", "36A15DB2-0FE4-4F46-A8DD-21CE4D480A21", @"" );

            // Attrib Value for Block:Submit Report, Attribute:Detail Page Page: Application Group Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "22774F85-F508-4C5A-9630-76B7074DB587", "014A4F42-2D91-43FF-A066-81C92AD52BAC", @"64b5b1f6-472a-4c64-85b5-1f6864fe1992" );

            // Attrib Value for Block:Group Members, Attribute:Detail Page Page: Application Group Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E3392C37-FBF8-487B-960B-DF3EF6B0ED45", "440119C0-8DBF-4768-AC5F-E2B18960B799", @"08079646-1016-4680-bcc6-809eb7c58095" );

            // Attrib Value for Block:Group Members, Attribute:Add Member Page Page: Application Group Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E3392C37-FBF8-487B-960B-DF3EF6B0ED45", "7EFF5A6D-07F2-4A66-8D68-85CD5A2D1668", @"08079646-1016-4680-bcc6-809eb7c58095" );

            RockMigrationHelper.UpdateFieldType( "Schedules", "", "Rock", "Rock.Field.Types.SchedulesFieldType", "CD8C60E6-63E1-400D-893B-A46592C692E5" );

        }

        public override void Down()
        {
            // Attrib for BlockType: Accountability Group Member Detail:Able to choose leader
            RockMigrationHelper.DeleteAttribute( "04B74B24-274E-43D4-9148-5E432390014A" );
            // Attrib for BlockType: Accountability Group List:Is Editor
            RockMigrationHelper.DeleteAttribute( "01B835A6-7040-4111-B7C7-5D396A08BCA7" );
            // Attrib for BlockType: Accountability Group List:Display Filter
            RockMigrationHelper.DeleteAttribute( "ED9D3696-A77C-4310-82B1-35E1B562EB1D" );
            // Attrib for BlockType: Accountability Group List:Exclude Group Types
            RockMigrationHelper.DeleteAttribute( "A7DC20BB-9B16-46E2-899A-D124F79C372E" );
            // Attrib for BlockType: Accountability Group List:Detail Page
            RockMigrationHelper.DeleteAttribute( "855F6AD7-A7A9-4BD0-A4FE-66C71F5C0457" );
            // Attrib for BlockType: Accountability Group List:Include Group Types
            RockMigrationHelper.DeleteAttribute( "278D773B-5C83-4F21-B589-EB2609F144EC" );
            // Attrib for BlockType: Accountability Group List:Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute( "44851A1A-F6C1-4021-8E0E-B9B379544D65" );
            // Attrib for BlockType: Accountability Group List:Display Description Column
            RockMigrationHelper.DeleteAttribute( "E21D3EC0-0114-45E7-83A2-13B5B8A60FE4" );
            // Attrib for BlockType: Accountability Group List:Display System Column
            RockMigrationHelper.DeleteAttribute( "40FBFB68-AA43-4875-AAF0-419A2094CA71" );
            // Attrib for BlockType: Accountability Group List:Display Active Status Column
            RockMigrationHelper.DeleteAttribute( "5ADDA3CE-3F11-4BA0-8454-27C22440BF64" );
            // Attrib for BlockType: Accountability Group List:Display Group Type Column
            RockMigrationHelper.DeleteAttribute( "D48538ED-E3F6-4C78-AD5E-FEB97DBD9F51" );
            // Attrib for BlockType: Accountability Group List:Entity Type
            RockMigrationHelper.DeleteAttribute( "36A15DB2-0FE4-4F46-A8DD-21CE4D480A21" );
            // Attrib for BlockType: Accountability Group Member Detail:Detail Page
            RockMigrationHelper.DeleteAttribute( "24FBAA42-E8D0-4CA1-83C6-186314A96EB4" );
            // Attrib for BlockType: Accountability Group Detail:Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute( "1DF95A83-3767-4257-9033-2B6D5D96D479" );
            // Attrib for BlockType: Accountability Group Detail:Limit to Group Types that are shown in navigation
            RockMigrationHelper.DeleteAttribute( "4E1C2BA2-52E3-4777-88ED-255DF39D3DFE" );
            // Attrib for BlockType: Accountability Group Detail:Show Edit
            RockMigrationHelper.DeleteAttribute( "50A3724A-219C-4611-AC52-B64FB1D52A78" );
            // Attrib for BlockType: Accountability Group Detail:Group Types
            RockMigrationHelper.DeleteAttribute( "21B030FD-108E-4AB3-A21E-4858F09BF8C5" );
            // Attrib for BlockType: Accountability Group Detail:Map Style
            RockMigrationHelper.DeleteAttribute( "FE342E27-6950-43D1-AEF0-ACE01FBEE67A" );
            // Attrib for BlockType: Accountability Group Detail:Group Map Page
            RockMigrationHelper.DeleteAttribute( "F1DA2972-6BBC-42A4-945D-144381725B55" );
            // Attrib for BlockType: Accountability Group Member List:Add Member Page
            RockMigrationHelper.DeleteAttribute( "7EFF5A6D-07F2-4A66-8D68-85CD5A2D1668" );
            // Attrib for BlockType: Accountability Group Member List:Detail Page
            RockMigrationHelper.DeleteAttribute( "440119C0-8DBF-4768-AC5F-E2B18960B799" );
            // Attrib for BlockType: Submit Report:Detail Page
            RockMigrationHelper.DeleteAttribute( "014A4F42-2D91-43FF-A066-81C92AD52BAC" );
            // Attrib for BlockType: Group List:Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute( "EBAD4F97-C320-4A36-89C8-24BD7FFAFF11" );
            // Attrib for BlockType: Group List:Detail Page
            RockMigrationHelper.DeleteAttribute( "42DE3456-BF32-475B-8ECD-347EA4846886" );
            // Attrib for BlockType: Group List:Display System Column
            RockMigrationHelper.DeleteAttribute( "239D4D60-030A-437B-8CB0-97E9BE1161EA" );
            // Attrib for BlockType: Group List:Display Description Column
            RockMigrationHelper.DeleteAttribute( "D3C75DEE-5C9D-4664-83D2-F0D5C82FC6BF" );
            // Attrib for BlockType: Group List:Display Active Status Column
            RockMigrationHelper.DeleteAttribute( "71554EE9-49E2-4DAD-BA00-C0B9A858D30A" );
            // Attrib for BlockType: Group List:Display Filter
            RockMigrationHelper.DeleteAttribute( "0BD18C8D-8950-4B6D-A157-A5F0642310A5" );
            // Attrib for BlockType: Group List:Entity Type
            RockMigrationHelper.DeleteAttribute( "3D04DEA4-5B7C-476C-B53C-C460E7916E8E" );
            // Remove Block: Accountability Report Form, from Page: Submit Report Page, Site: External Website
            RockMigrationHelper.DeleteBlock( "8A9E4226-E00F-4D04-A062-6B6A5B7F9011" );
            // Remove Block: Accountability Question Summary, from Page: Submit Report Page, Site: External Website
            RockMigrationHelper.DeleteBlock( "FDE47F60-EBCE-499D-9573-9C2E5EF9D415" );
            // Remove Block: Accountability Report List, from Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8E1E225A-70D7-4ADE-A061-632E5828733D" );
            // Remove Block: Accountability Question Score, from Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "05DB5B27-57CE-4E40-8198-E3A0FDDD849A" );
            // Remove Block: Report List, from Page: Group Member Detail Page, Site: External Website
            RockMigrationHelper.DeleteBlock( "AFB5D47D-5F99-4EC6-B1E6-07913E5B5B1A" );
            // Remove Block: Score, from Page: Group Member Detail Page, Site: External Website
            RockMigrationHelper.DeleteBlock( "71EB35E0-5123-4818-B692-B687A9A298B1" );
            // Remove Block: Member Detail, from Page: Group Member Detail Page, Site: External Website
            RockMigrationHelper.DeleteBlock( "DF3FF4DA-5702-4E14-8533-DBE2512ED54F" );
            // Remove Block: Group Members, from Page: Application Group Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "E3392C37-FBF8-487B-960B-DF3EF6B0ED45" );
            // Remove Block: Submit Report, from Page: Application Group Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "22774F85-F508-4C5A-9630-76B7074DB587" );
            // Remove Block: Group Detail, from Page: Application Group Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "6874752A-8659-4195-9D70-5D95E2C6FDA0" );
            // Remove Block: Your Application Groups, from Page: Accountability Groups, Site: External Website
            RockMigrationHelper.DeleteBlock( "28BFA0DD-9F9C-45B0-9F4B-ECE8663CE5A1" );
            // Remove Block: Accountability Groups, from Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "23887B0A-B1CF-4948-B134-DAFFE33486B1" );
            // Remove Block: Questions, from Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9F669732-BE5B-47DE-9F87-889DF333AA51" );
            // Remove Block: Accountability Group Type Detail, from Page: Accountability Group Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "77365C1D-E675-4082-A490-B008D54DDAA2" );
            // Remove Block: Accountability Group Types, from Page: Accountability Group Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "49E50E18-886D-4DFA-8E4C-4B7542AFC78B" );
            // Remove Block: Group Members, from Page: Accountability Group Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EF11659E-1DC9-4552-A691-69FDB555F4B6" );
            // Remove Block: Group Detail, from Page: Accountability Group Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "12909FB9-0316-4CC8-9DD5-FF16E3061748" );
            // Remove Block: Detail, from Page: Group Member Detail Page, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9A9FC9F1-A8D3-4B60-BE77-FDF13813FB8D" );
            // Remove Block: My Groups, from Page: Accountability Groups, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BE5FF7EE-0F96-44D5-BAEA-C28D602E329E" );

            RockMigrationHelper.DeleteBlockType( "7D1000EC-77F1-440B-8B30-3BE96395255B" ); // Accountability Group List
            RockMigrationHelper.DeleteBlockType( "B21E3F69-350E-495E-9339-4AFDEB6BBBC5" ); // Accountability Question Summary
            RockMigrationHelper.DeleteBlockType( "7DEAD984-AB96-44BB-8688-1B42D457348C" ); // Accountability Report Form
            RockMigrationHelper.DeleteBlockType( "5EA51F75-E9AE-4724-927B-962C95449491" ); // Accountability Report List
            RockMigrationHelper.DeleteBlockType( "ADEC3020-B1D8-4EC1-8D11-3D6AEBD29DF7" ); // Accountability Question Score
            RockMigrationHelper.DeleteBlockType( "0AAFB967-7254-49F6-9D2B-7698E50DFA38" ); // Accountability Group Member Detail
            RockMigrationHelper.DeleteBlockType( "D159193A-BB75-410F-8531-BE4E7683B71C" ); // Submit Report
            RockMigrationHelper.DeleteBlockType( "89E7E8CD-A692-4EB6-B0E5-D1C63D2C7428" ); // Accountability Group Member List
            RockMigrationHelper.DeleteBlockType( "0F962947-0DE7-408D-BCBE-24B44E0CD14C" ); // Accountability Group Detail
            RockMigrationHelper.DeleteBlockType( "E409A44F-55A1-4AE5-A6B3-C7CCCCF46190" ); // Accountability Question List
            RockMigrationHelper.DeleteBlockType( "8F3A56F3-398B-4008-8E7D-1CB649E80DA5" ); // Accountability Group Type Detail
            RockMigrationHelper.DeleteBlockType( "71D20566-4DCA-4673-8AD4-B6EB237F903F" ); // Groups of Group Type List
            RockMigrationHelper.DeletePage( "08079646-1016-4680-BCC6-809EB7C58095" ); //  Page: Group Member Detail Page, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "64B5B1F6-472A-4C64-85B5-1F6864FE1992" ); //  Page: Submit Report Page, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "249F1E4C-C290-4BD3-B971-079F9595FA9C" ); //  Page: Application Group Detail, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "3B84E3FA-64E4-4783-BE0A-20E7C237D5AE" ); //  Page: Accountability Groups, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "ADB9288D-4012-4E98-9A30-8C6235FFB2F0" ); //  Page: Accountability Group Type Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "D3CDF45D-F938-45AB-8CED-BF6E75465AA9" ); //  Page: Accountability Group Types, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "AAD50DFC-5412-4DD1-B41A-E1840F848FC8" ); //  Page: Group Member Detail Page, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6B3C673B-E693-48EF-8262-C9EA46B70C3F" ); //  Page: Accountability Group Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1D5A2E3C-8D8A-4086-9FF5-CE173874568D" ); //  Page: Accountability Groups, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeleteAttribute( "37D911E7-5E0A-45CE-A35A-91F75480DB6B" );
            RockMigrationHelper.DeleteGroupType( "DC99BF69-8A1A-411F-A267-1AE75FDC2341" );
            RockMigrationHelper.DeleteDefinedValue( "ECF09F24-41DD-4E57-9642-A4CAE0B29E14" );


        }
    }
}