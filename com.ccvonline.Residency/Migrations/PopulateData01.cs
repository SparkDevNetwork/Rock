using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Residency.Migrations
{
    [MigrationNumber( 2, "1.0.8" )]
    public class PopulateData01 : Migration
    {
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

            AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Residency", "", "82B81403-8A93-4F42-A958-5303C3AF1508", "icon-user-md" );
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
            AddPage( "39661338-971E-45EA-86C3-7A8A5D2DEA54", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Project Assignment Detail", "", "165E7CB7-E15A-4AC2-8383-567A593279F0", "" );
            AddPage( "165E7CB7-E15A-4AC2-8383-567A593279F0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resident Project Assignment Assessment Detail", "", "32E7BCDE-37BC-48B5-B0FE-AA784AF0425A", "" );
            AddPage( "32E7BCDE-37BC-48B5-B0FE-AA784AF0425A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Project Assignment Point of Assessment", "", "69A714EB-1870-4516-AB4F-63ADF2100FEA", "" );

            UpdateBlockType( "Competency Detail", "Displays the details of a competency.", "~/Plugins/com_ccvonline/Residency/CompetencyDetail.ascx", "CCV > Residency", "201B966E-9CAB-4169-8B9A-2B1834EAA10B" );
            UpdateBlockType( "Competency List", "Lists all the residency competencies.", "~/Plugins/com_ccvonline/Residency/CompetencyList.ascx", "CCV > Residency", "67CAEFD7-B9CB-454D-AB5C-8F2A256D5A74" );
            UpdateBlockType( "Resident Assessments Report", "Reports a summary of competency assessments for a resident.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonAssessmentReport.ascx", "CCV > Residency", "4F8AF8A5-19A6-4331-A366-6F0D6C8F8CAC" );
            UpdateBlockType( "Resident Competency Detail", "Simple detail form showing a resident's assignment to a specific competency.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonDetail.ascx", "CCV > Residency", "C6CDCB34-2E28-4660-8B74-275339E09D7E" );
            UpdateBlockType( "Resident Competency List", "Lists all the resident's competencies.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonList.ascx", "CCV > Residency", "6A91DD5F-DD5A-4524-8263-66CB3121D7D0" );
            UpdateBlockType( "Resident Competency Detail", "Simple detail form that for a resident's assignment to a specific competency.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentDetail.ascx", "CCV > Residency", "0B1ABD40-C146-4098-A6D5-7864D79F60DB" );
            UpdateBlockType( "Resident Project Assessment List ", "Lists all the resident's project assessments.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentList.ascx", "CCV > Residency", "8178142C-03B5-4E50-9276-4041F43F3FC5" );
            UpdateBlockType( "Resident Project Point of Assessment Detail", "Displays the details of a project's point of assessment for a resident.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentPointOfAssessmentDetail.ascx", "CCV > Residency", "A9887719-6CC2-46D4-8988-66035EB32853" );
            UpdateBlockType( "Project Assessment - Point of Assessment List", "Lists all the resident's points of assessment for a project assessment.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentPointOfAssessmentList.ascx", "CCV > Residency", "2094CEC9-A33D-4B74-AA12-A8E8ACA3977B" );
            UpdateBlockType( "Resident Project Detail", "Displays the details of a resident project.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectDetail.ascx", "CCV > Residency", "EBA68FE6-1C4A-4BFF-81C2-B51078DE5FA9" );
            UpdateBlockType( "Resident Project List", "Displays a list of a resident projects.", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectList.ascx", "CCV > Residency", "BD8ED244-A990-46BB-8B85-CA6F63D04FA8" );
            UpdateBlockType( "Period Detail", "Displays the details of a residency period.  For example: Fall 2013/Spring 2014", "~/Plugins/com_ccvonline/Residency/PeriodDetail.ascx", "CCV > Residency", "988C13E9-906C-430E-AD57-DD15423EFC31" );
            UpdateBlockType( "Period List", "Displays a list of all of the residency periods.  For example: Fall 2013/Spring 2014, etc", "~/Plugins/com_ccvonline/Residency/PeriodList.ascx", "CCV > Residency", "31BA002B-7189-4649-BC24-85511C4264D2" );
            UpdateBlockType( "Resident Name", "Block that simply shows a resident's name", "~/Plugins/com_ccvonline/Residency/PersonDetail.ascx", "CCV > Residency", "D16B9889-4D35-4FCB-8E8A-694E3E64C092" );
            UpdateBlockType( "Resident List", "Displays a list of all of the residents for a period, along with a summary of their compentencies and projects", "~/Plugins/com_ccvonline/Residency/PersonList.ascx", "CCV > Residency", "AD5EE8C6-ADC8-40C2-83E9-182070AE5A5F" );
            UpdateBlockType( "Project Detail", "Displays the details of a project.", "~/Plugins/com_ccvonline/Residency/ProjectDetail.ascx", "CCV > Residency", "B6DF0E1A-F260-4E15-BC97-57E3E9ECEFA5" );
            UpdateBlockType( "Project List", "Displays a list of projects for a competency.", "~/Plugins/com_ccvonline/Residency/ProjectList.ascx", "CCV > Residency", "5ACAAE0E-2591-4E54-AD51-1EE486428D70" );
            UpdateBlockType( "Point of Assessment Detail", "Displays the details of a project's point of assessment.", "~/Plugins/com_ccvonline/Residency/ProjectPointOfAssessmentDetail.ascx", "CCV > Residency", "4A8E6F8E-3DFF-4A4B-9341-89BEDB3DB472" );
            UpdateBlockType( "Point of Assessment List", "Displays a list of a project's points of assessment.", "~/Plugins/com_ccvonline/Residency/ProjectPointOfAssessmentList.ascx", "CCV > Residency", "4044C2E2-C5F7-461A-A8B9-515E39EDB5C1" );
            UpdateBlockType( "Resident Competency Detail", "Displays the details of a resident's competency.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyDetail.ascx", "CCV > Residency", "167AC36E-B95A-4EC0-B1B0-873AF4D85813" );
            UpdateBlockType( "Resident Competency Goals Detail", "Displays the details of a resident's competency's goals.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyGoalsDetail.ascx", "CCV > Residency", "2F0FA2D8-CBB0-4230-90B2-6C992D35F6A2" );
            UpdateBlockType( "Resident Competency List", "Lists all of a resident's competencies.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyList.ascx", "CCV > Residency", "202C3FF6-6C3A-4463-8AE0-90E1DF0D16F0" );
            UpdateBlockType( "Resident Competency Project List", "Lists all of a resident's projects for a competency.", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyProjectList.ascx", "CCV > Residency", "B97B85C4-A9C5-49BA-9FC0-8B2542A5177D" );
            UpdateBlockType( "Project Grading Form", "Form for grading a project.", "~/Plugins/com_ccvonline/Residency/ResidentGradeDetail.ascx", "CCV > Residency", "8A765635-40BB-4569-AF22-C74299C97545" );
            UpdateBlockType( "Project Grading Request", "Form where a resident can request that a project be graded.", "~/Plugins/com_ccvonline/Residency/ResidentGradeRequest.ascx", "CCV > Residency", "3AC2C129-34CE-4B01-B40C-B0A827A418B5" );
            UpdateBlockType( "Resident Project Assessment Detail", "Displays the details of a resident's project assessment.", "~/Plugins/com_ccvonline/Residency/ResidentProjectAssessmentDetail.ascx", "CCV > Residency", "0934E08F-DCE3-4A18-B266-B85A4E058BCD" );
            UpdateBlockType( "Resident Project Assessment List", "Displays a list of a resident's project assessments.", "~/Plugins/com_ccvonline/Residency/ResidentProjectAssessmentList.ascx", "CCV > Residency", "AE1F4870-FF89-4D81-BA62-5B8566C045FE" );
            UpdateBlockType( "Resident Project Detail", "Displays the details of a resident's project. The resident can initiate a grading request from here.", "~/Plugins/com_ccvonline/Residency/ResidentProjectDetail.ascx", "CCV > Residency", "C92070FE-A4ED-41F6-BF79-FFDDD9C0DB84" );
            UpdateBlockType( "Resident Project Point of Assessment List", "Displays a list of a resident's project's points of assessment.", "~/Plugins/com_ccvonline/Residency/ResidentProjectPointOfAssessmentList.ascx", "CCV > Residency", "D3F86578-D391-4711-AD2F-33C21AA11C93" );
            UpdateBlockType( "Track Detail", "Displays the details of a residency track.", "~/Plugins/com_ccvonline/Residency/TrackDetail.ascx", "CCV > Residency", "1172872E-6480-4C42-A806-312DDD8CFEF1" );
            UpdateBlockType( "Track List", "Displays a list of a residency tracks.", "~/Plugins/com_ccvonline/Residency/TrackList.ascx", "CCV > Residency", "E0A2EF6E-0342-4759-B6C2-2E0154FED9F2" );
        }

        public override void Down()
        {
            DeleteBlockType( "E0A2EF6E-0342-4759-B6C2-2E0154FED9F2" ); // Track List
            DeleteBlockType( "1172872E-6480-4C42-A806-312DDD8CFEF1" ); // Track Detail
            DeleteBlockType( "D3F86578-D391-4711-AD2F-33C21AA11C93" ); // Resident Project Point of Assessment List
            DeleteBlockType( "C92070FE-A4ED-41F6-BF79-FFDDD9C0DB84" ); // Resident Project Detail
            DeleteBlockType( "AE1F4870-FF89-4D81-BA62-5B8566C045FE" ); // Resident Project Assessment List
            DeleteBlockType( "0934E08F-DCE3-4A18-B266-B85A4E058BCD" ); // Resident Project Assessment Detail
            DeleteBlockType( "3AC2C129-34CE-4B01-B40C-B0A827A418B5" ); // Project Grading Request
            DeleteBlockType( "8A765635-40BB-4569-AF22-C74299C97545" ); // Project Grading Form
            DeleteBlockType( "B97B85C4-A9C5-49BA-9FC0-8B2542A5177D" ); // Resident Competency Project List
            DeleteBlockType( "202C3FF6-6C3A-4463-8AE0-90E1DF0D16F0" ); // Resident Competency List
            DeleteBlockType( "2F0FA2D8-CBB0-4230-90B2-6C992D35F6A2" ); // Resident Competency Goals Detail
            DeleteBlockType( "167AC36E-B95A-4EC0-B1B0-873AF4D85813" ); // Resident Competency Detail
            DeleteBlockType( "4044C2E2-C5F7-461A-A8B9-515E39EDB5C1" ); // Point of Assessment List
            DeleteBlockType( "4A8E6F8E-3DFF-4A4B-9341-89BEDB3DB472" ); // Point of Assessment Detail
            DeleteBlockType( "5ACAAE0E-2591-4E54-AD51-1EE486428D70" ); // Project List
            DeleteBlockType( "B6DF0E1A-F260-4E15-BC97-57E3E9ECEFA5" ); // Project Detail
            DeleteBlockType( "AD5EE8C6-ADC8-40C2-83E9-182070AE5A5F" ); // Resident List
            DeleteBlockType( "D16B9889-4D35-4FCB-8E8A-694E3E64C092" ); // Resident Name
            DeleteBlockType( "31BA002B-7189-4649-BC24-85511C4264D2" ); // Period List
            DeleteBlockType( "988C13E9-906C-430E-AD57-DD15423EFC31" ); // Period Detail
            DeleteBlockType( "BD8ED244-A990-46BB-8B85-CA6F63D04FA8" ); // Resident Project List
            DeleteBlockType( "EBA68FE6-1C4A-4BFF-81C2-B51078DE5FA9" ); // Resident Project Detail
            DeleteBlockType( "2094CEC9-A33D-4B74-AA12-A8E8ACA3977B" ); // Project Assessment - Point of Assessment List
            DeleteBlockType( "A9887719-6CC2-46D4-8988-66035EB32853" ); // Resident Project Point of Assessment Detail
            DeleteBlockType( "8178142C-03B5-4E50-9276-4041F43F3FC5" ); // Resident Project Assessment List 
            DeleteBlockType( "0B1ABD40-C146-4098-A6D5-7864D79F60DB" ); // Resident Competency Detail
            DeleteBlockType( "6A91DD5F-DD5A-4524-8263-66CB3121D7D0" ); // Resident Competency List
            DeleteBlockType( "C6CDCB34-2E28-4660-8B74-275339E09D7E" ); // Resident Competency Detail
            DeleteBlockType( "4F8AF8A5-19A6-4331-A366-6F0D6C8F8CAC" ); // Resident Assessments Report
            DeleteBlockType( "67CAEFD7-B9CB-454D-AB5C-8F2A256D5A74" ); // Competency List
            DeleteBlockType( "201B966E-9CAB-4169-8B9A-2B1834EAA10B" ); // Competency Detail
                        
            DeletePage( "69A714EB-1870-4516-AB4F-63ADF2100FEA" ); // Project Assignment Point of Assessment
            DeletePage( "32E7BCDE-37BC-48B5-B0FE-AA784AF0425A" ); // Resident Project Assignment Assessment Detail
            DeletePage( "165E7CB7-E15A-4AC2-8383-567A593279F0" ); // Project Assignment Detail
            DeletePage( "39661338-971E-45EA-86C3-7A8A5D2DEA54" ); // Resident Project Detail
            DeletePage( "6F095271-8060-4577-8E72-C0EE2389527C" ); // Resident Competency Detail
            DeletePage( "57F487E8-46E4-4F6E-B03E-96CC2C4BE185" ); // Resident Detail
            DeletePage( "531C64E2-282B-4644-9619-319BDBAC627E" ); // Residents
            DeletePage( "36428AF8-7650-4047-B655-8D39F5EA10C5" ); // Resident Groups
            DeletePage( "DD65505A-6FE2-4478-8901-9F38F484E3EB" ); // Point of Assessment Detail
            DeletePage( "37BA8EAD-16C5-4257-953D-D202684A8E61" ); // Project Detail
            DeletePage( "2BD2E7BB-4199-4C18-B51A-AA3755DECD1B" ); // Competency Detail
            DeletePage( "038AEF17-65EE-4161-BF9E-64AACC791701" ); // Residency Track
            DeletePage( "F8D8663B-FE4F-4F48-A359-DBE656AE69A2" ); // Period Detail
            DeletePage( "4B507217-5C12-4479-B5CD-B696B1445653" ); // Periods
            DeletePage( "FD705EED-CD8D-4F53-8C16-ABBA15CC27D5" ); // Configuration
            DeletePage( "82B81403-8A93-4F42-A958-5303C3AF1508" ); // Residency
        }
    }


}
