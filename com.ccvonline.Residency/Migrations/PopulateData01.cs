using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Residency.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 3, "1.0.8" )]
    public class PopulateData01 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                begin

                declare 
                  @groupTypeId int

                INSERT INTO [dbo].[GroupType]
                           ([IsSystem]
                           ,[Name]
                           ,[Description]
                           ,[GroupTerm]
                           ,[GroupMemberTerm]
                           ,[DefaultGroupRoleId]
                           ,[AllowMultipleLocations]
                           ,[ShowInNavigation]
                           ,[ShowInGroupList]
                           ,[IconCssClass]
                           ,[TakesAttendance]
                           ,[AttendanceRule]
                           ,[AttendancePrintTo]
                           ,[Order]
                           ,[LocationSelectionMode]
                           ,[Guid])
                     VALUES
                           (0
                           ,'Residency'
                           ,'Group Types for the Residency program'
                           ,'Residents' 
                           ,'Resident'
                           ,null
                            ,1
                           ,1
                           ,1
                           ,'icon-md'
                            ,0
                            ,0
                            ,0
                            ,0
                            ,0
                           ,'00043CE6-EB1B-43B5-A12A-4552B91A3E28')

                select @groupTypeId = @@IDENTITY

                INSERT INTO [dbo].[GroupTypeRole] 
                    ([IsSystem] ,[GroupTypeId] ,[Name] ,[Description] ,[Order] ,[MaxCount] ,[MinCount] ,[Guid] ,[IsLeader])
                     VALUES
                    (0, @groupTypeId, 'Resident', 'A Resident in the Residency program', 0, null, null, 'AC1CD9C9-782C-42A6-A28B-78B38C3AC833', 0)

                update [GroupType] set [DefaultGroupRoleId] = @@IDENTITY where [Guid] = '00043CE6-EB1B-43B5-A12A-4552B91A3E28'

                end
                " );

            // add all the residency pages
            AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Residency", "", "82B81403-8A93-4F42-A958-5303C3AF1508", "fa fa-user-md" );
            AddPage( "82B81403-8A93-4F42-A958-5303C3AF1508", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Configuration", "Configure various aspects of the Residency application", "FD705EED-CD8D-4F53-8C16-ABBA15CC27D5", "" );
            AddPage( "FD705EED-CD8D-4F53-8C16-ABBA15CC27D5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Periods", "", "4B507217-5C12-4479-B5CD-B696B1445653", "" );
            AddPage( "4B507217-5C12-4479-B5CD-B696B1445653", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Period Detail", "", "F8D8663B-FE4F-4F48-A359-DBE656AE69A2", "" );
            AddPage( "F8D8663B-FE4F-4F48-A359-DBE656AE69A2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Residency Track", "", "038AEF17-65EE-4161-BF9E-64AACC791701", "" );
            AddPage( "038AEF17-65EE-4161-BF9E-64AACC791701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Competency Detail", "", "2BD2E7BB-4199-4C18-B51A-AA3755DECD1B", "" );
            AddPage( "2BD2E7BB-4199-4C18-B51A-AA3755DECD1B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Project Detail", "", "37BA8EAD-16C5-4257-953D-D202684A8E61", "" );
            AddPage( "37BA8EAD-16C5-4257-953D-D202684A8E61", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Point of Assessment Detail", "", "DD65505A-6FE2-4478-8901-9F38F484E3EB", "" );
            AddPage( "FD705EED-CD8D-4F53-8C16-ABBA15CC27D5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Groups", "", "36428AF8-7650-4047-B655-8D39F5EA10C5", "" );
            AddPage( "36428AF8-7650-4047-B655-8D39F5EA10C5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Residents", "", "531C64E2-282B-4644-9619-319BDBAC627E", "" );
            AddPage( "531C64E2-282B-4644-9619-319BDBAC627E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Detail", "", "57F487E8-46E4-4F6E-B03E-96CC2C4BE185", "" );
            AddPage( "57F487E8-46E4-4F6E-B03E-96CC2C4BE185", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Competency Detail", "", "6F095271-8060-4577-8E72-C0EE2389527C", "" );
            AddPage( "6F095271-8060-4577-8E72-C0EE2389527C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Project Detail", "", "39661338-971E-45EA-86C3-7A8A5D2DEA54", "" );
            AddPage( "82B81403-8A93-4F42-A958-5303C3AF1508", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident", "", "F98B0061-8327-4B96-8A5E-B3C58D899B31", "" );
            AddPage( "F98B0061-8327-4B96-8A5E-B3C58D899B31", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Coursework", "", "826C0BFF-C831-4427-98F9-57FF462D82F5", "" );
            AddPage( "826C0BFF-C831-4427-98F9-57FF462D82F5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Projects", "", "ADE663B9-386B-479C-ABD9-3349E1B4B827", "" );
            AddPage( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Project Detail", "", "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "" );
            AddPage( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Grade Request", "", "5D729D30-8E33-4913-A56F-98F803479C6D", "" );
            AddPage( "5D729D30-8E33-4913-A56F-98F803479C6D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Grade Project Detail", "", "A16C4B0F-66C6-4CF0-8B54-B232DDF553B9", "" );
            AddPage( "39661338-971E-45EA-86C3-7A8A5D2DEA54", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Project Assessment", "", "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "" );
            AddPage( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Point of Assessment", "", "4827C8D3-B0FA-4AA4-891F-1F27C7D76606", "" );
            AddPage( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Project Assessment Detail", "", "0DF59029-C17B-474D-8DD1-ED312B734202", "" );
            AddPage( "826C0BFF-C831-4427-98F9-57FF462D82F5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Goals", "", "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "" );
            AddPage( "826C0BFF-C831-4427-98F9-57FF462D82F5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Notes", "", "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "" );
            AddPage( "F98B0061-8327-4B96-8A5E-B3C58D899B31", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Assessments", "", "162927F6-E503-43C4-B075-55F1E592E96E", "" );
            AddPage( "162927F6-E503-43C4-B075-55F1E592E96E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Project Assessment Detail", "", "BDA4C473-01CD-449E-97D4-4B054E3F0959", "" );
            AddPage( "82B81403-8A93-4F42-A958-5303C3AF1508", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reports", "", "5C25CE3F-9AA2-4760-9AC5-0B09350380E9", "" );
            AddPage( "5C25CE3F-9AA2-4760-9AC5-0B09350380E9", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grade Book", "", "64FB3328-D209-4518-94CD-6FEC6BCB1D85", "" );
            AddPage( "64FB3328-D209-4518-94CD-6FEC6BCB1D85", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Residents", "", "F650B68F-DE1E-4952-9A4D-05D8A6B3F51C", "" );
            AddPage( "F650B68F-DE1E-4952-9A4D-05D8A6B3F51C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Grade Book", "", "9A3A80AA-A9B0-4824-B81D-68F070131E92", "" );
            AddPage( "82B81403-8A93-4F42-A958-5303C3AF1508", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Security", "", "FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8", "" );
            // Hide Security Page from Navigation
            Sql( "update [Page] set [DisplayInNavWhen] = 2 where [Guid] = 'FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8'" );
            AddPage( "FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Login", "", "07770489-9C8D-43FA-85B3-E99BB54D3353", "" );



            // hide breadcrumb name for most of the residency admin pages
            Sql( @"
update [Page] set [BreadCrumbDisplayName] = 0 where [Guid] in 
  ('FD705EED-CD8D-4F53-8C16-ABBA15CC27D5', 'F8D8663B-FE4F-4F48-A359-DBE656AE69A2', '038AEF17-65EE-4161-BF9E-64AACC791701','2BD2E7BB-4199-4C18-B51A-AA3755DECD1B','37BA8EAD-16C5-4257-953D-D202684A8E61','DD65505A-6FE2-4478-8901-9F38F484E3EB')"
                );

            UpdateBlockType( "Competency Detail", "Displays the details of a competency.", "~/Plugins/com_ccvonline/Residency/CompetencyDetail.ascx", "CCV > Residency", "D1D1C418-B84B-4307-B4EC-D2FD2970D639" );
            UpdateBlockType( "Competency List", "Lists all the residency competencies.", "~/Plugins/com_ccvonline/Residency/CompetencyList.ascx", "CCV > Residency", "488BB996-8F4B-4DB4-9B0B-FB7B959BCDAF" );
            UpdateBlockType( "Resident Assessments Report", "Reports a summary of competency assessments for a resident.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonAssessmentReport.ascx", "CCV > Residency", "0F4672C8-7475-4061-9F6D-13E48193819E" );
            UpdateBlockType( "Resident Competency Detail", "Simple detail form showing a resident's assignment to a specific competency.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonDetail.ascx", "CCV > Residency", "9E3D6BF6-28EE-4902-B6CD-AC1C31A7B731" );
            UpdateBlockType( "Resident Competency List", "Lists all the resident's competencies.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonList.ascx", "CCV > Residency", "E4A531AD-4FCF-449B-91AB-ACBF87D83881" );
            UpdateBlockType( "Resident Competency Detail", "Simple detail form that for a resident's assignment to a specific competency.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentDetail.ascx", "CCV > Residency", "470EB28D-75A7-46C6-BB74-525A66BD114E" );
            UpdateBlockType( "Resident Project Assessment List ", "Lists all the resident's project assessments.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentList.ascx", "CCV > Residency", "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4" );
            UpdateBlockType( "Resident Project Point of Assessment Detail", "Displays the details of a project's point of assessment for a resident.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentPointOfAssessmentDetail.ascx", "CCV > Residency", "B2F1D26F-0C4F-46F1-91A4-ACBBB50E4202" );
            UpdateBlockType( "Project Assessment - Point of Assessment List", "Lists all the resident's points of assessment for a project assessment.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentPointOfAssessmentList.ascx", "CCV > Residency", "DE2FCBEA-F103-43BC-87E4-4235FA361B87" );
            UpdateBlockType( "Resident Project Detail", "Displays the details of a resident project.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectDetail.ascx", "CCV > Residency", "5847D528-98BB-487C-BE26-A8FF60F74033" );
            UpdateBlockType( "Resident Project List", "Displays a list of a resident projects.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectList.ascx", "CCV > Residency", "C947DC2C-76E1-4B69-8FC6-E9E3134B36C7" );
            UpdateBlockType( "Period Detail", "Displays the details of a residency period.  For example: Fall 2013/Spring 2014", "~/Plugins/com_ccvonline/Residency/PeriodDetail.ascx", "CCV > Residency", "511421DB-E127-447D-81A6-FF8C52D11815" );
            UpdateBlockType( "Period List", "Displays a list of all of the residency periods.  For example: Fall 2013/Spring 2014, etc", "~/Plugins/com_ccvonline/Residency/PeriodList.ascx", "CCV > Residency", "81C5EE50-AE8D-45F9-8014-A7C65F0FDBD6" );
            UpdateBlockType( "Resident Name", "Block that simply shows a resident's name", "~/Plugins/com_ccvonline/Residency/PersonDetail.ascx", "CCV > Residency", "F0A0BE3A-DD15-468F-93A6-97C440DB8253" );
            UpdateBlockType( "Resident List", "Displays a list of all of the residents for a period,  along with a summary of their compentencies and projects", "~/Plugins/com_ccvonline/Residency/PersonList.ascx", "CCV > Residency", "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2" );
            UpdateBlockType( "Project Detail", "Displays the details of a project.", "~/Plugins/com_ccvonline/Residency/ProjectDetail.ascx", "CCV > Residency", "8BA15032-D16A-4FDC-AE7F-A77F50267F39" );
            UpdateBlockType( "Project List", "Displays a list of projects for a competency.", "~/Plugins/com_ccvonline/Residency/ProjectList.ascx", "CCV > Residency", "1A1E32B5-93BC-480E-BFB4-A5D9C06DCBF2" );
            UpdateBlockType( "Point of Assessment Detail", "Displays the details of a project's point of assessment.", "~/Plugins/com_ccvonline/Residency/ProjectPointOfAssessmentDetail.ascx", "CCV > Residency", "A56E3BE8-AB33-4CEA-9C93-F138B7E24498" );
            UpdateBlockType( "Point of Assessment List", "Displays a list of a project's points of assessment.", "~/Plugins/com_ccvonline/Residency/ProjectPointOfAssessmentList.ascx", "CCV > Residency", "8EEE930E-F879-48DC-8AFB-7249B618034D" );
            UpdateBlockType( "Resident Competency Detail", "Displays the details of a resident's competency.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyDetail.ascx", "CCV > Residency", "536A0B29-B427-434D-82B6-C5CE6A8E07FE" );
            UpdateBlockType( "Resident Competency Goals Detail", "Displays the details of a resident's competency's goals.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyGoalsDetail.ascx", "CCV > Residency", "4BD8E3F7-30D3-49C2-B3D6-B897174A9AB8" );
            UpdateBlockType( "Resident Competency List", "Lists all of a resident's competencies.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyList.ascx", "CCV > Residency", "2D404077-7723-4528-A893-800658BAEA4F" );
            UpdateBlockType( "Resident Competency Project List", "Lists all of a resident's projects for a competency.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyProjectList.ascx", "CCV > Residency", "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9" );
            UpdateBlockType( "Project Grading Form", "Form for grading a project.", "~/Plugins/com_ccvonline/Residency/ResidentGradeDetail.ascx", "CCV > Residency", "ABDDD216-80B8-427D-8689-0CF84C9C5646" );
            UpdateBlockType( "Project Grading Request", "Form where a resident can request that a project be graded.", "~/Plugins/com_ccvonline/Residency/ResidentGradeRequest.ascx", "CCV > Residency", "1AD0421D-8B24-4A5A-842F-AF37EACBE35E" );
            UpdateBlockType( "Resident Project Assessment Detail", "Displays the details of a resident's project assessment.", "~/Plugins/com_ccvonline/Residency/ResidentProjectAssessmentDetail.ascx", "CCV > Residency", "D2835421-1D69-4D2E-80BC-836FF606ADDD" );
            UpdateBlockType( "Resident Project Assessment List", "Displays a list of a resident's project assessments.", "~/Plugins/com_ccvonline/Residency/ResidentProjectAssessmentList.ascx", "CCV > Residency", "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20" );
            UpdateBlockType( "Resident Project Detail", "Displays the details of a resident's project. The resident can initiate a grading request from here.", "~/Plugins/com_ccvonline/Residency/ResidentProjectDetail.ascx", "CCV > Residency", "13A42E92-8D1A-407D-B2D1-2472BBC27D13" );
            UpdateBlockType( "Resident Project Point of Assessment List", "Displays a list of a resident's project's points of assessment.", "~/Plugins/com_ccvonline/Residency/ResidentProjectPointOfAssessmentList.ascx", "CCV > Residency", "BC3048EE-6964-4ABB-B710-4616136045BA" );
            UpdateBlockType( "Track Detail", "Displays the details of a residency track.", "~/Plugins/com_ccvonline/Residency/TrackDetail.ascx", "CCV > Residency", "72133176-4E1A-4851-B4D0-BBC447D84440" );
            UpdateBlockType( "Track List", "Displays a list of a residency tracks.", "~/Plugins/com_ccvonline/Residency/TrackList.ascx", "CCV > Residency", "A3E2F4B9-FC87-472A-B873-2BB649C2417B" );

            // all blocks
            AddBlock( "4B507217-5C12-4479-B5CD-B696B1445653", "", "81C5EE50-AE8D-45F9-8014-A7C65F0FDBD6", "Periods", "Main", "", "", 0, "856C9158-08F9-4A89-9AE4-124109DA6A1E" );
            AddBlock( "F8D8663B-FE4F-4F48-A359-DBE656AE69A2", "", "511421DB-E127-447D-81A6-FF8C52D11815", "Period Detail", "Main", "", "", 0, "F868F454-D163-4F35-9768-CCAC14908D83" );
            AddBlock( "F8D8663B-FE4F-4F48-A359-DBE656AE69A2", "", "A3E2F4B9-FC87-472A-B873-2BB649C2417B", "Residency Track List", "Main", "", "", 1, "45B63A50-F7DD-419A-BF8E-97969C193A47" );
            AddBlock( "038AEF17-65EE-4161-BF9E-64AACC791701", "", "72133176-4E1A-4851-B4D0-BBC447D84440", "Residency Track Detail", "Main", "", "", 0, "92F53B4E-1817-4BA6-A673-47DB3DE17722" );
            AddBlock( "038AEF17-65EE-4161-BF9E-64AACC791701", "", "488BB996-8F4B-4DB4-9B0B-FB7B959BCDAF", "Residency Competency List", "Main", "", "", 1, "286A833E-0A5A-4FAB-ACFA-71CCEEEC1AB4" );
            AddBlock( "2BD2E7BB-4199-4C18-B51A-AA3755DECD1B", "", "D1D1C418-B84B-4307-B4EC-D2FD2970D639", "Residency Competency Detail", "Main", "", "", 0, "59C12E5C-7478-4E24-843C-14561C47FBD1" );
            AddBlock( "2BD2E7BB-4199-4C18-B51A-AA3755DECD1B", "", "1A1E32B5-93BC-480E-BFB4-A5D9C06DCBF2", "Residency Project List", "Main", "", "", 1, "F075238F-E4E2-4291-8F2B-7EB0ACD5888D" );
            AddBlock( "37BA8EAD-16C5-4257-953D-D202684A8E61", "", "8BA15032-D16A-4FDC-AE7F-A77F50267F39", "Residency Project Detail", "Main", "", "", 0, "73C4692B-7A54-48CE-9611-4B3E4ABB9EA9" );
            AddBlock( "37BA8EAD-16C5-4257-953D-D202684A8E61", "", "8EEE930E-F879-48DC-8AFB-7249B618034D", "Residency Project Point Of Assessment List", "Main", "", "", 1, "986909E7-D5C4-47BF-AE2E-ED93C2D915A1" );
            AddBlock( "DD65505A-6FE2-4478-8901-9F38F484E3EB", "", "A56E3BE8-AB33-4CEA-9C93-F138B7E24498", "Residency Project Point Of Assessment Detail", "Main", "", "", 0, "4C4223CF-B656-4CF1-9319-C2350D0E9A7D" );
            AddBlock( "531C64E2-282B-4644-9619-319BDBAC627E", "", "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "Residency Person List", "Main", "", "", 1, "A66E242C-D1E0-4F4E-9F6A-ED7484352BFE" );
            AddBlock( "57F487E8-46E4-4F6E-B03E-96CC2C4BE185", "", "F0A0BE3A-DD15-468F-93A6-97C440DB8253", "Residency Person Detail", "Main", "", "", 0, "9CEC61A6-BEA6-420B-AEE1-8424E33998CB" );
            AddBlock( "57F487E8-46E4-4F6E-B03E-96CC2C4BE185", "", "E4A531AD-4FCF-449B-91AB-ACBF87D83881", "Residency Competency Person List", "Main", "", "", 2, "3F70C055-32CF-4F18-BBBB-6A0C88C5BE39" );
            AddBlock( "6F095271-8060-4577-8E72-C0EE2389527C", "", "9E3D6BF6-28EE-4902-B6CD-AC1C31A7B731", "Residency Competency Person Detail", "Main", "", "", 0, "71D6DCB8-6CEC-4703-8088-9B3FC0DEE7A6" );
            AddBlock( "531C64E2-282B-4644-9619-319BDBAC627E", "", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Group Detail", "Main", "", "", 0, "746B438D-75E3-495A-9678-C3C14629511A" );
            AddBlock( "36428AF8-7650-4047-B655-8D39F5EA10C5", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Group List", "Main", "", "", 0, "AD59F37C-97EC-4F07-A604-3AAF8270C737" );
            AddBlock( "6F095271-8060-4577-8E72-C0EE2389527C", "", "C947DC2C-76E1-4B69-8FC6-E9E3134B36C7", "Residency Competency Person Project List", "Main", "", "", 2, "2A39AD5B-3497-4B47-9A66-1CEA68FD848C" );
            AddBlock( "39661338-971E-45EA-86C3-7A8A5D2DEA54", "", "5847D528-98BB-487C-BE26-A8FF60F74033", "Residency Competency Person Project Detail", "Main", "", "", 0, "F377C5DC-0618-4259-AA32-B75959CBEC85" );
            AddBlock( "826C0BFF-C831-4427-98F9-57FF462D82F5", "", "5A880084-7237-449A-9855-3FA02B6BD79F", "Marketing Campaign Ads Xslt", "Main", "", "", 0, "D1AEB343-41A8-4501-9475-40103C7AEA0A" );
            AddBlock( "826C0BFF-C831-4427-98F9-57FF462D82F5", "", "2D404077-7723-4528-A893-800658BAEA4F", "Resident Competency List", "Main", "", "", 1, "EE97ABE8-A124-4437-B962-805C1D0C18D4" );
            AddBlock( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "", "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "Resident Competency Detail", "Main", "", "", 0, "0BC0C139-26FB-403C-A5ED-BAA1CC1231FD" );
            AddBlock( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "", "BC3048EE-6964-4ABB-B710-4616136045BA", "Resident Project Point Of Assessment List", "Main", "", "", 2, "C482389C-6CAC-4EB9-9C56-5FFCC0D17639" );
            AddBlock( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "", "13A42E92-8D1A-407D-B2D1-2472BBC27D13", "Resident Project Detail", "Main", "", "", 0, "402DB31D-7C84-4154-890E-D18AEE5FC0E2" );
            AddBlock( "5D729D30-8E33-4913-A56F-98F803479C6D", "", "1AD0421D-8B24-4A5A-842F-AF37EACBE35E", "Resident Grade Request", "Main", "", "", 0, "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B" );
            AddBlock( "A16C4B0F-66C6-4CF0-8B54-B232DDF553B9", "", "ABDDD216-80B8-427D-8689-0CF84C9C5646", "Resident Grade Detail", "Main", "", "", 0, "52B23A41-93BE-4A65-BF58-C98ED89E54B7" );
            AddBlock( "39661338-971E-45EA-86C3-7A8A5D2DEA54", "", "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4", "Competency Person Project Assessment List", "Main", "", "", 1, "C27D517E-128B-4A1B-B069-C367C7B59AAD" );
            AddBlock( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "", "470EB28D-75A7-46C6-BB74-525A66BD114E", "Competency Person Project Assessment Detail", "Main", "", "", 0, "76714DC6-A171-4E3A-8B61-EE7659968918" );
            AddBlock( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "", "DE2FCBEA-F103-43BC-87E4-4235FA361B87", "Competency Person Project Assessment Point Of Assessment List", "Main", "", "", 1, "5AC72357-5942-46D8-9BE8-27B1E5239DF3" );
            AddBlock( "4827C8D3-B0FA-4AA4-891F-1F27C7D76606", "", "B2F1D26F-0C4F-46F1-91A4-ACBBB50E4202", "Competency Person Project Assessment Point Of Assessment Detail", "Main", "", "", 0, "A4571853-BFF8-4599-8B7C-1A2402C06C05" );
            AddBlock( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "", "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "Resident Project Assessment List", "Main", "", "", 3, "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F" );
            AddBlock( "0DF59029-C17B-474D-8DD1-ED312B734202", "", "D2835421-1D69-4D2E-80BC-836FF606ADDD", "Resident Project Assessment Detail", "Main", "", "", 0, "0A1F651B-621F-4577-B35B-DD9E2FE22302" );

            //AddBlock( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "Main", "", "", 1, "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9" );

            AddBlock( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "", "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9", "Resident Competency Project List", "Main", "", "", 2, "1843F88B-B17F-4F81-AED8-91B87C0A2816" );
            AddBlock( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "", "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "Resident Competency Detail", "Main", "", "", 0, "A3237301-8DAD-4E96-8871-1DE4FF5395A1" );
            //AddBlock( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "Main", "", "", 1, "4132D452-7672-4275-9D4E-B7D6A7DFE745" );
            AddBlock( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "", "4BD8E3F7-30D3-49C2-B3D6-B897174A9AB8", "Resident Competency Goals Detail", "Main", "", "", 2, "B48D7551-B398-431F-87EE-2786465C5A13" );
            AddBlock( "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "", "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "Resident Competency Detail", "Main", "", "", 0, "D4800BCD-B7F1-4D22-ACDF-5F04FB49E148" );
            //AddBlock( "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "Main", "", "", 1, "55629043-F1BB-4B07-8AC7-A32D8B1F632C" );
            AddBlock( "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 2, "FDB0021C-485F-4B75-82D0-514CDBD59B7C" );
            AddBlock( "07770489-9C8D-43FA-85B3-E99BB54D3353", "", "7B83D513-1178-429E-93FF-E76430E038E4", "Login", "Main", "", "", 0, "C88645F1-BDB6-4A08-A581-587DCCB40A3B" );
            AddBlock( "826C0BFF-C831-4427-98F9-57FF462D82F5", "", "F7193487-1234-49D7-9CEC-7F5F452B7E3F", "Current Person", "Main", "", "", 0, "E517DDD7-73DB-4475-87A4-83CBCD7657F1" );
            //AddBlock( "826C0BFF-C831-4427-98F9-57FF462D82F5", "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "Main", "", "", 1, "D07780FC-0ED5-4881-8B76-24F6FAE8A897" );
            AddBlock( "162927F6-E503-43C4-B075-55F1E592E96E", "", "F7193487-1234-49D7-9CEC-7F5F452B7E3F", "Current Person", "Main", "", "", 0, "4EFF1322-6A9A-44A0-B3B8-CB547CB09C0B" );
            //AddBlock( "162927F6-E503-43C4-B075-55F1E592E96E", "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "Main", "", "", 1, "3938E111-C9FF-49E9-B1B8-A2AA89080F51" );

            // Move the Resident Competency List down under the new Page Nav pills 
            Sql( "Update [Block] set [Order] = 2 where [Guid] = 'EE97ABE8-A124-4437-B962-805C1D0C18D4'" );

            AddBlock( "162927F6-E503-43C4-B075-55F1E592E96E", "", "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "Resident Project Assessment List", "Main", "", "", 2, "B459F23A-9C32-4537-BA93-637A81ACB35A" );
            AddBlock( "BDA4C473-01CD-449E-97D4-4B054E3F0959", "", "D2835421-1D69-4D2E-80BC-836FF606ADDD", "Resident Project Assessment Detail", "Main", "", "", 0, "1DB06C26-B318-46CD-9E9F-219FC1EF6338" );

            // Add Block to Page: Grade Book
            AddBlock( "64FB3328-D209-4518-94CD-6FEC6BCB1D85", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Groups", "Main", "", "", 0, "E4E626BD-D100-40E3-A6C8-4D68EC40C2F8" );

            // Add Block to Page: Residents
            AddBlock( "F650B68F-DE1E-4952-9A4D-05D8A6B3F51C", "", "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "Residency Person List", "Main", "", "", 0, "169C1FE4-938B-46FC-85EC-EF0EFCD26BD5" );

            // Add Block to Page: Resident Grade Book
            AddBlock( "9A3A80AA-A9B0-4824-B81D-68F070131E92", "", "0F4672C8-7475-4061-9F6D-13E48193819E", "Competency Person Assessment Report", "Main", "", "", 2, "9E6BA93D-694E-4578-9A85-E0DC32D39470" );

            //*** all blocktype attributes and values***

            // Attrib for BlockType: com .ccvonline - Residency Period List:Detail Page
            AddBlockTypeAttribute( "81C5EE50-AE8D-45F9-8014-A7C65F0FDBD6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "55B21688-D933-4C78-9F78-76B965BD1C3F" );

            // Attrib for BlockType: com .ccvonline - Residency Track List:Detail Page
            AddBlockTypeAttribute( "A3E2F4B9-FC87-472A-B873-2BB649C2417B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "9AF9C65C-F6E1-425C-9AAC-C9BDF988B1F3" );

            // Attrib for BlockType: com .ccvonline - Residency Competency List:Detail Page
            AddBlockTypeAttribute( "488BB996-8F4B-4DB4-9B0B-FB7B959BCDAF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "69538108-46CD-4E29-9701-414E85E7BA0D" );

            // Attrib for BlockType: com .ccvonline - Residency Project List:Detail Page
            AddBlockTypeAttribute( "1A1E32B5-93BC-480E-BFB4-A5D9C06DCBF2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "9CA38214-F938-4800-BCB6-8158C548FDD0" );

            // Attrib for BlockType: com .ccvonline - Residency Project Point Of Assessment List:Detail Page
            AddBlockTypeAttribute( "8EEE930E-F879-48DC-8AFB-7249B618034D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "4089261C-A3B9-451F-AEB1-57B5458B3EEB" );

            // Attrib for BlockType: com .ccvonline - Residency Project Detail:Residency Competency Page
            AddBlockTypeAttribute( "8BA15032-D16A-4FDC-AE7F-A77F50267F39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Residency Competency Page", "ResidencyCompetencyPage", "", "", 0, "", "583DDD83-2D7C-4CA3-886F-98628DAD575D" );

            // Attrib Value for Periods:Detail Page
            AddBlockAttributeValue( "856C9158-08F9-4A89-9AE4-124109DA6A1E", "55B21688-D933-4C78-9F78-76B965BD1C3F", "f8d8663b-fe4f-4f48-a359-dbe656ae69a2" );

            // Attrib for BlockType: com .ccvonline - Residency Track Detail:Residency Period Page
            AddBlockTypeAttribute( "72133176-4E1A-4851-B4D0-BBC447D84440", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Residency Period Page", "ResidencyPeriodPage", "", "", 0, "", "AAD879A5-B1E9-420D-B4CC-E1780D1D7B06" );

            // Attrib Value for Residency Track List:Detail Page
            AddBlockAttributeValue( "45B63A50-F7DD-419A-BF8E-97969C193A47", "9AF9C65C-F6E1-425C-9AAC-C9BDF988B1F3", "038aef17-65ee-4161-bf9e-64aacc791701" );

            // Attrib Value for Residency Competency List:Detail Page
            AddBlockAttributeValue( "286A833E-0A5A-4FAB-ACFA-71CCEEEC1AB4", "69538108-46CD-4E29-9701-414E85E7BA0D", "2bd2e7bb-4199-4c18-b51a-aa3755decd1b" );

            // Attrib Value for Residency Project List:Detail Page
            AddBlockAttributeValue( "F075238F-E4E2-4291-8F2B-7EB0ACD5888D", "9CA38214-F938-4800-BCB6-8158C548FDD0", "37ba8ead-16c5-4257-953d-d202684a8e61" );

            // Attrib Value for Residency Project Point Of Assessment List:Detail Page
            AddBlockAttributeValue( "986909E7-D5C4-47BF-AE2E-ED93C2D915A1", "4089261C-A3B9-451F-AEB1-57B5458B3EEB", "dd65505a-6fe2-4478-8901-9f38f484e3eb" );

            // Attrib Value for Residency Project Detail:Residency Competency Page
            AddBlockAttributeValue( "73C4692B-7A54-48CE-9611-4B3E4ABB9EA9", "583DDD83-2D7C-4CA3-886F-98628DAD575D", "2bd2e7bb-4199-4c18-b51a-aa3755decd1b" );

            // Attrib Value for Residency Track Detail:Residency Period Page
            AddBlockAttributeValue( "92F53B4E-1817-4BA6-A673-47DB3DE17722", "AAD879A5-B1E9-420D-B4CC-E1780D1D7B06", "f8d8663b-fe4f-4f48-a359-dbe656ae69a2" );

            // Attrib for BlockType: com .ccvonline - Residency Competency Detail:Residency Track Page
            AddBlockTypeAttribute( "D1D1C418-B84B-4307-B4EC-D2FD2970D639", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Residency Track Page", "ResidencyTrackPage", "", "", 0, "", "32DB890B-C394-4CA0-ACD2-3D2EA8E9C8F5" );

            // Attrib Value for Page Xslt Transformation:Root Page
            AddBlockAttributeValue( "149C35E9-25AD-4B99-BFB7-5CCF4A6E6ACA", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "fd705eed-cd8d-4f53-8c16-abba15cc27d5" );

            // Attrib Value for Page Xslt Transformation:XSLT File
            AddBlockAttributeValue( "149C35E9-25AD-4B99-BFB7-5CCF4A6E6ACA", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Assets/XSLT/PageListAsBlocks.xslt" );

            // Attrib Value for Page Xslt Transformation:Number of Levels
            AddBlockAttributeValue( "149C35E9-25AD-4B99-BFB7-5CCF4A6E6ACA", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );

            // Attrib Value for Page Xslt Transformation:Include Current Parameters
            AddBlockAttributeValue( "149C35E9-25AD-4B99-BFB7-5CCF4A6E6ACA", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "False" );

            // Attrib Value for Residency Competency Detail:Residency Track Page
            AddBlockAttributeValue( "59C12E5C-7478-4E24-843C-14561C47FBD1", "32DB890B-C394-4CA0-ACD2-3D2EA8E9C8F5", "038aef17-65ee-4161-bf9e-64aacc791701" );

            // Attrib for BlockType: com .ccvonline - Residency Person List:Detail Page
            AddBlockTypeAttribute( "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "7B1CA61D-0E1A-44C9-9210-DF49D29EECF0" );

            // Attrib for BlockType: com .ccvonline - Residency Competency Person List:Detail Page
            AddBlockTypeAttribute( "E4A531AD-4FCF-449B-91AB-ACBF87D83881", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "6E623135-F71C-481A-9A75-CD77EE0D6D81" );

            // Attrib for BlockType: com .ccvonline - Residency Competency Person Project List:Detail Page
            AddBlockTypeAttribute( "C947DC2C-76E1-4B69-8FC6-E9E3134B36C7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "01FD5A4D-3A9C-4F98-8955-E131C3500B23" );

            // Attrib for BlockType: com .ccvonline - Residency Competency Person Project Detail:Residency Competency Person Page
            AddBlockTypeAttribute( "5847D528-98BB-487C-BE26-A8FF60F74033", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Residency Competency Person Page", "ResidencyCompetencyPersonPage", "", "", 0, "", "A718D831-2126-415D-A147-E67B78F8419F" );


            // Attrib Value for Group List:Detail Page
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", "531c64e2-282b-4644-9619-319bdbac627e" );
            
            // Attrib Value for Group List:Limit to Security Role Groups
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "1DAD66E3-8859-487E-8200-483C98DE2E07", "False" );

            // Attrib Value for Group List:Display Description Column
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

            // Attrib Value for Group List:Show Description
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", "False" );

            // Attrib Value for Group List:Display System Column
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", "False" );

            // Attrib Value for Group List:Display Group Type Column
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", "False" );

            // Attrib Value for Group Detail:Show Edit
            AddBlockAttributeValue( "746B438D-75E3-495A-9678-C3C14629511A", "50C7E223-459E-4A1C-AE3C-2892CBD40D22", "True" );

            // Attrib Value for Group Detail:Group Types
            AddBlockAttributeValue( "746B438D-75E3-495A-9678-C3C14629511A", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

            // Attrib Value for Group Detail:Limit to Security Role Groups
            AddBlockAttributeValue( "746B438D-75E3-495A-9678-C3C14629511A", "12295C7E-08F4-4AC5-8A34-C829620FC0B1", "False" );

            // Attrib Value for Residency Person List:Detail Page
            AddBlockAttributeValue( "A66E242C-D1E0-4F4E-9F6A-ED7484352BFE", "7B1CA61D-0E1A-44C9-9210-DF49D29EECF0", "57f487e8-46e4-4f6e-b03e-96cc2c4be185" );

            // Attrib Value for Residency Competency Person List:Detail Page
            AddBlockAttributeValue( "3F70C055-32CF-4F18-BBBB-6A0C88C5BE39", "6E623135-F71C-481A-9A75-CD77EE0D6D81", "6f095271-8060-4577-8e72-c0ee2389527c" );

            // Attrib Value for Residency Competency Person Project List:Detail Page
            AddBlockAttributeValue( "2A39AD5B-3497-4B47-9A66-1CEA68FD848C", "01FD5A4D-3A9C-4F98-8955-E131C3500B23", "39661338-971e-45ea-86c3-7a8a5d2dea54" );

            // Attrib Value for Residency Competency Person Project Detail:Residency Competency Person Page
            AddBlockAttributeValue( "F377C5DC-0618-4259-AA32-B75959CBEC85", "A718D831-2126-415D-A147-E67B78F8419F", "6f095271-8060-4577-8e72-c0ee2389527c" );

            // Attrib Value for Group List:Show GroupType
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "5AF2A432-1A7A-4171-879E-F413D58039C1", "False" );

            // Attrib Value for Group List:Show IsSystem
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "8A6E9BEF-F372-495D-816E-86E84E534DD6", "False" );

            // Residency Groups - Group List: Limit to GroupType Residency
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

            // Residency Groups - Group Detail: Limit to GroupType Residency
            AddBlockAttributeValue( "746B438D-75E3-495A-9678-C3C14629511A", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency List:Detail Page
            AddBlockTypeAttribute( "2D404077-7723-4528-A893-800658BAEA4F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "07B337A8-F3AE-4985-86BC-4B268257DF77" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency Detail:Detail Page
            AddBlockTypeAttribute( "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "E72925FC-288C-4FA9-A509-472870A231ED" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Project Detail:Grade Request Page
            AddBlockTypeAttribute( "13A42E92-8D1A-407D-B2D1-2472BBC27D13", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Grade Request Page", "GradeRequestPage", "", "", 0, "", "0CAB3D65-0492-4CD1-BAEE-139A2BF9A7BA" );

            // Attrib for BlockType: com _ccvonline - Residency - Competency Person Project Assessment List:Detail Page
            AddBlockTypeAttribute( "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "3934ED43-CEAF-4C7E-8022-7F5668A431DE" );

            // Attrib for BlockType: com _ccvonline - Residency - Competency Person Project Assessment Point Of Assessment List:Detail Page
            AddBlockTypeAttribute( "DE2FCBEA-F103-43BC-87E4-4235FA361B87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "89638EF8-98DE-4852-BB12-AA35D3EBA238" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Project Assessment List:Detail Page
            AddBlockTypeAttribute( "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "B6ABAF24-2D2E-4A0E-A195-C66E98110D2D" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency Project List:Detail Page
            AddBlockTypeAttribute( "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "A7168B42-80A7-4077-8949-5012C6793DF1" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Request:Resident Grade Detail Page
            AddBlockTypeAttribute( "1AD0421D-8B24-4A5A-842F-AF37EACBE35E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Resident Grade Detail Page", "ResidentGradeDetailPage", "", "", 0, "", "06B61C6B-84D9-4F3E-A191-F51531A2E905" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Detail:Person Project Detail Page
            AddBlockTypeAttribute( "ABDDD216-80B8-427D-8689-0CF84C9C5646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Project Detail Page", "PersonProjectDetailPage", "", "", 0, "", "70602088-6F35-4C04-9959-6F21A4BC640F" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Request:Residency Grader Security Role
            AddBlockTypeAttribute( "1AD0421D-8B24-4A5A-842F-AF37EACBE35E", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Residency Grader Security Role", "ResidencyGraderSecurityRole", "", "", 0, "", "DEDE968C-1347-4037-9BFC-A62C7BA59186" );

            // Attrib Value for Resident Competency List:Detail Page
            AddBlockAttributeValue( "EE97ABE8-A124-4437-B962-805C1D0C18D4", "07B337A8-F3AE-4985-86BC-4B268257DF77", "ade663b9-386b-479c-abd9-3349e1b4b827" );

            // Attrib Value for Resident Competency Detail:Detail Page
            AddBlockAttributeValue( "0BC0C139-26FB-403C-A5ED-BAA1CC1231FD", "E72925FC-288C-4FA9-A509-472870A231ED", "56f3e462-28ef-4ec5-a58c-c5fde48356e0" );

            // Attrib Value for Resident Project Detail:Grade Request Page
            AddBlockAttributeValue( "402DB31D-7C84-4154-890E-D18AEE5FC0E2", "0CAB3D65-0492-4CD1-BAEE-139A2BF9A7BA", "5d729d30-8e33-4913-a56f-98f803479c6d" );

            // Attrib Value for Resident Grade Request:Residency Grader Security Role
            AddBlockAttributeValue( "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B", "DEDE968C-1347-4037-9BFC-A62C7BA59186", "31" );

            // Attrib Value for Resident Grade Request:Resident Grade Detail Page
            AddBlockAttributeValue( "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B", "06B61C6B-84D9-4F3E-A191-F51531A2E905", "a16c4b0f-66c6-4cf0-8b54-b232ddf553b9" );

            // Attrib Value for Resident Grade Detail:Person Project Detail Page
            AddBlockAttributeValue( "52B23A41-93BE-4A65-BF58-C98ED89E54B7", "70602088-6F35-4C04-9959-6F21A4BC640F", "56f3e462-28ef-4ec5-a58c-c5fde48356e0" );

            // Attrib Value for Competency Person Project Assessment List:Detail Page
            AddBlockAttributeValue( "C27D517E-128B-4A1B-B069-C367C7B59AAD", "3934ED43-CEAF-4C7E-8022-7F5668A431DE", "a4be6749-0190-4655-b3f6-0ceec2ddd5c4" );

            // Attrib Value for Competency Person Project Assessment Point Of Assessment List:Detail Page
            AddBlockAttributeValue( "5AC72357-5942-46D8-9BE8-27B1E5239DF3", "89638EF8-98DE-4852-BB12-AA35D3EBA238", "4827c8d3-b0fa-4aa4-891f-1f27c7d76606" );

            // Attrib Value for Resident Project Assessment List:Detail Page
            AddBlockAttributeValue( "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F", "B6ABAF24-2D2E-4A0E-A195-C66E98110D2D", "0df59029-c17b-474d-8dd1-ed312b734202" );

            // Attrib Value for Resident Competency Project List:Detail Page
            AddBlockAttributeValue( "1843F88B-B17F-4F81-AED8-91B87C0A2816", "A7168B42-80A7-4077-8949-5012C6793DF1", "56f3e462-28ef-4ec5-a58c-c5fde48356e0" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib for BlockType: com_ccvonline - Residency - Resident Project Assessment List:Show Competency Column
            AddBlockTypeAttribute( "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Competency Column", "ShowCompetencyColumn", "", "", 0, "False", "BFE19B2F-D62D-4C87-B928-FFFAE65BD12E" );

            // Attrib for BlockType: com_ccvonline - Residency - Resident Project Assessment List:Show Project Column
            AddBlockTypeAttribute( "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Project Column", "ShowProjectColumn", "", "", 0, "False", "F0390AAB-D114-4367-8349-35EEA8EDACB8" );

            // Attrib for BlockType: com_ccvonline - Residency - Resident Project Assessment List:Show Grid Title
            AddBlockTypeAttribute( "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Title", "ShowGridTitle", "", "", 0, "False", "F5DAD59C-AC05-4BCA-ACC3-171924039872" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Show Competency Column, Page:Resident Project Detail
            AddBlockAttributeValue( "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F", "BFE19B2F-D62D-4C87-B928-FFFAE65BD12E", "False" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Show Project Column, Page:Resident Project Detail
            AddBlockAttributeValue( "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F", "F0390AAB-D114-4367-8349-35EEA8EDACB8", "False" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Show Grid Title, Page:Resident Project Detail
            AddBlockAttributeValue( "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F", "F5DAD59C-AC05-4BCA-ACC3-171924039872", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Coursework
            AddBlockAttributeValue( "D07780FC-0ED5-4881-8B76-24F6FAE8A897", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Coursework
            AddBlockAttributeValue( "D07780FC-0ED5-4881-8B76-24F6FAE8A897", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Coursework
            AddBlockAttributeValue( "D07780FC-0ED5-4881-8B76-24F6FAE8A897", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "f98b0061-8327-4b96-8a5e-b3c58d899b31" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Coursework
            AddBlockAttributeValue( "D07780FC-0ED5-4881-8B76-24F6FAE8A897", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Coursework
            AddBlockAttributeValue( "D07780FC-0ED5-4881-8B76-24F6FAE8A897", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Assessments
            AddBlockAttributeValue( "3938E111-C9FF-49E9-B1B8-A2AA89080F51", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Assessments
            AddBlockAttributeValue( "3938E111-C9FF-49E9-B1B8-A2AA89080F51", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Assessments
            AddBlockAttributeValue( "3938E111-C9FF-49E9-B1B8-A2AA89080F51", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "f98b0061-8327-4b96-8a5e-b3c58d899b31" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Assessments
            AddBlockAttributeValue( "3938E111-C9FF-49E9-B1B8-A2AA89080F51", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Assessments
            AddBlockAttributeValue( "3938E111-C9FF-49E9-B1B8-A2AA89080F51", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Show Grid Title, Page:Assessments
            AddBlockAttributeValue( "B459F23A-9C32-4537-BA93-637A81ACB35A", "F5DAD59C-AC05-4BCA-ACC3-171924039872", "False" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Show Competency Column, Page:Assessments
            AddBlockAttributeValue( "B459F23A-9C32-4537-BA93-637A81ACB35A", "BFE19B2F-D62D-4C87-B928-FFFAE65BD12E", "True" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Show Project Column, Page:Assessments
            AddBlockAttributeValue( "B459F23A-9C32-4537-BA93-637A81ACB35A", "F0390AAB-D114-4367-8349-35EEA8EDACB8", "True" );

            // Attrib Value for Block:Resident Project Assessment List, Attribute:Detail Page, Page:Assessments
            AddBlockAttributeValue( "B459F23A-9C32-4537-BA93-637A81ACB35A", "B6ABAF24-2D2E-4A0E-A195-C66E98110D2D", "bda4c473-01cd-449e-97d4-4b054e3f0959" );

            // Attrib Value for Block:Groups, Attribute:Detail Page, Page:Grade Book
            AddBlockAttributeValue( "E4E626BD-D100-40E3-A6C8-4D68EC40C2F8", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", "f650b68f-de1e-4952-9a4d-05d8a6b3f51c" );

            // Attrib Value for Block:Groups, Attribute:Show Description, Page:Grade Book
            AddBlockAttributeValue( "E4E626BD-D100-40E3-A6C8-4D68EC40C2F8", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", "False" );

            // Attrib Value for Block:Groups, Attribute:Limit to Security Role Groups, Page:Grade Book
            AddBlockAttributeValue( "E4E626BD-D100-40E3-A6C8-4D68EC40C2F8", "1DAD66E3-8859-487E-8200-483C98DE2E07", "False" );

            // Attrib Value for Block:Groups, Attribute:Group Types, Page:Grade Book
            AddBlockAttributeValue( "E4E626BD-D100-40E3-A6C8-4D68EC40C2F8", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

            // Attrib Value for Block:Groups, Attribute:Show GroupType, Page:Grade Book
            AddBlockAttributeValue( "E4E626BD-D100-40E3-A6C8-4D68EC40C2F8", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", "False" );

            // Attrib Value for Block:Groups, Attribute:Show IsSystem, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","766A4BFA-D2D1-4744-B30D-637A7E3B9D8F","False");

            // Attrib for BlockType: com_ccvonline - Residency Person List:Show Add
            AddBlockTypeAttribute( "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Add", "ShowAdd", "", "", 0, "True", "9C78BE0D-3199-4B3C-AA35-B43535E70B10" );

            // Attrib for BlockType: com_ccvonline - Residency Person List:Show Delete
            AddBlockTypeAttribute( "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Delete", "ShowDelete", "", "", 0, "True", "D55E1E71-9AD2-4B9D-A99B-CD50C9238003" );

            // Attrib Value for Block:Residency Person List, Attribute:Show Add, Page:Residents
            AddBlockAttributeValue( "169C1FE4-938B-46FC-85EC-EF0EFCD26BD5", "9C78BE0D-3199-4B3C-AA35-B43535E70B10", "False" );

            // Attrib Value for Block:Residency Person List, Attribute:Show Delete, Page:Residents
            AddBlockAttributeValue( "169C1FE4-938B-46FC-85EC-EF0EFCD26BD5", "D55E1E71-9AD2-4B9D-A99B-CD50C9238003", "False" );

            // Attrib Value for Block:Residency Person List, Attribute:Detail Page, Page:Residents
            AddBlockAttributeValue( "169C1FE4-938B-46FC-85EC-EF0EFCD26BD5", "7B1CA61D-0E1A-44C9-9210-DF49D29EECF0", "9a3a80aa-a9b0-4824-b81d-68f070131e92" );

            // Breadcrumb updates
            Sql( @"
-- Period Detail
update [Page] set [BreadCrumbDisplayName] = 1 where [Guid] = 'F8D8663B-FE4F-4F48-A359-DBE656AE69A2'

-- Project Assessment
update [Page] set [BreadCrumbDisplayName] = 0 where [Guid] = 'A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4'

-- Resident..
update [Page] set [BreadCrumbDisplayName] = 0 where [Guid] in
 ('F98B0061-8327-4B96-8A5E-B3C58D899B31',
'A16C4B0F-66C6-4CF0-8B54-B232DDF553B9',
'5D729D30-8E33-4913-A56F-98F803479C6D',
'56F3E462-28EF-4EC5-A58C-C5FDE48356E0',
'ADE663B9-386B-479C-ABD9-3349E1B4B827',
'130FA92D-9D5F-45D1-84AA-B399F2E868E6',
'83DBB422-38C5-44F3-9FDE-3737AC8CF2A7',
'0DF59029-C17B-474D-8DD1-ED312B734202',
'4827C8D3-B0FA-4AA4-891F-1F27C7D76606',
'57F487E8-46E4-4F6E-B03E-96CC2C4BE185',
'531C64E2-282B-4644-9619-319BDBAC627E')

" );

            Sql( @"
INSERT INTO [dbo].[PageContext]([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[Guid])
     VALUES(1
           ,(select [Id] from [Page] where [Guid] = '130FA92D-9D5F-45D1-84AA-B399F2E868E6')
           ,'com.ccvonline.Residency.Model.CompetencyPerson'
           ,'competencyPersonId'
           ,SYSDATETIME()
           ,'13366F47-7AFD-430D-A0E5-9AF92347DAE1')
" );

            AddDefinedType( "Residency", "Residency Point of Assessment Type", "Used by the ccvonline Residency plugin to be assigned to a Residency Point of Assessment", "338A8802-4022-404F-9FA2-150F1FB3838F" );

            AddDefinedValue( "338A8802-4022-404F-9FA2-150F1FB3838F", "Strategic Agility", "", "E9D1F7A6-4DD4-4D30-B629-72FE3FA58FEC", true );
            AddDefinedValue( "338A8802-4022-404F-9FA2-150F1FB3838F", "Operational Agility", "", "91214D34-8466-44F5-BB00-4736B1C36043", true );
            AddDefinedValue( "338A8802-4022-404F-9FA2-150F1FB3838F", "People Agility", "", "3929AD70-86C7-4F2E-9B91-A7FED4F7085C", true );
            AddDefinedValue( "338A8802-4022-404F-9FA2-150F1FB3838F", "Leadership Agility", "", "0DE0C7A1-E399-4447-8B9F-C5243DC2BEB4", true );
            AddDefinedValue( "338A8802-4022-404F-9FA2-150F1FB3838F", "Personal Composition", "", "C4DE3D73-7168-4AE1-AF7C-B849E7296D81", true );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // intentionally blank
        }
    }
}
