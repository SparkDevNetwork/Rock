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

using System.Collections.Generic;

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 203, "1.16.4" )]
    public class MigrationRollupsForV17_0_10 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            ReorganizeAdminSettingsPages();
            AddRouteNameForTithingOverviewUp();
            UpdateGivingMetricSqlUp();
            PeerNetworkRelationshipTypeForFollowingUp();
            LMSUp();
            ChopShortenedLinkBlockUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            LMSDown();
        }

        #region PA: Chop Shortened Links Block

        private void ChopShortenedLinkBlockUp()
        {
            RockMigrationHelper.UpdatePageLayout( "A9188D7A-80D9-4865-9C77-9F90E992B65C", "2E169330-D7D7-4ECA-B417-72C64BE150F0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ShortLink
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ShortLink", "Short Link", "Rock.Blocks.Cms.ShortLink, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "026C6A93-5295-43E9-B67D-C3708ACB25B9" );

            // Add/Update Obsidian Block Type              
            //   Name:Shortened Links          
            //   Category:CMS              
            //   EntityType:RockWeb.Blocks.Administration.ShortLink              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Shortened Links", "Displays a dialog for adding a short link to the current page.", "Rock.Blocks.Cms.ShortLink", "Administration", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" );

            // Attribute for BlockType
            //   BlockType: Shortened Links
            //   Category: Administration
            //   Attribute: Minimum Token Length
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Token Length", "MinimumTokenLength", "Minimum Token Length", @"The minimum number of characters for the token.", 0, @"7", "D15CEAF4-8354-42DE-9C3B-50D4F31E32FB" );

            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Shortened Link",
                blockTypeReplacements: new Dictionary<string, string> {
        { "86FB6B0E-E426-4581-96C0-A7654D6A5C7D", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" }, // Shortened Link
                },
                migrationStrategy: "Chop",
                jobGuid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_SHORTENED_LINKS_BLOCK, blockAttributeKeysToIgnore: null );
        }

        #endregion

        #region JC: LMS Plugin Migration 2

        private void LMSUp()
        {
            AddHistoryCategories();
            SetBreadCrumbForDetailPagesWithBreadCrumbBlocks( false );
            SetShowInGroupList( false );
            AddOrUpdateBlockAndPageAttributes();

            // Ensure the public facing learning pages don't show in navigation.
            var learningUniversityRootPageGuid = "B32639B3-604F-441E-A6E4-2613C0A6BE2B";
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) DisplayInNavWhen.Never} WHERE [Guid] = '{learningUniversityRootPageGuid}';" );

            var learningProgramEntityTypeGuid = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";
            var allChurchCategoryGuid = "824B5DD9-47A7-4CE4-A461-F4FDEC8343F3";
            var internalStaffCategoryGuid = "87E1BEC7-171F-4E7E-8EC7-4D0102DCDE70";
            var volunteeringCategoryGuid = "A94C5563-647D-4509-B213-890B6D2A8530";
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "All Church", "", "All Church", allChurchCategoryGuid, 0 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Internal Staff", "", "Internal Staff", internalStaffCategoryGuid, 1 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Volunteering", "", "Volunteering", volunteeringCategoryGuid, 2 );
        }

        private void LMSDown()
        {
            RemoveHistoryCategories();
            SetBreadCrumbForDetailPagesWithBreadCrumbBlocks( true );
            SetShowInGroupList( true );
            DeleteBlockAndPageAttributes();
        }

        /// <summary>
        /// The Up methods from the SQL code-generated CodeGen_PagesBlocksAttributesMigration.sql file.
        /// </summary>
        private void AddOrUpdateBlockAndPageAttributes()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramCompletionDetail", "Learning Program Completion Detail", "Rock.Blocks.Lms.LearningProgramCompletionDetail, Rock.Blocks, Version=1.17.0.22, Culture=neutral, PublicKeyToken=null", false, false, "F9164B10-C913-4CD4-A612-27AD25D62ACA" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program Completion Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program Completion Detail", "Displays the details of a particular learning program completion.", "Rock.Blocks.Lms.LearningProgramCompletionDetail", "LMS", "E0C38A42-2ACE-4258-8D11-BD971C41EADB" );

            // Add Block 
            //  Block Name: Program Completion Detail
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E0C38A42-2ACE-4258-8D11-BD971C41EADB".AsGuid(), "Program Completion Detail", "Main", @"", @"", 0, "F7F137EE-C66D-4BF9-B117-DC8899C6603A" );

            // Update the path for learning completion details.
            Sql( "UPDATE [dbo].[PageRoute] SET [Route] = 'learning/{LearningProgramId}/completions/{LearningProgramCompletionId}' WHERE [Guid] = '218FB50F-A231-4029-887D-0F921918ECB1'" );

            // Should default to True (show KPIs on the Learning Program Detail page).
            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( false, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"True" );

            // This is no longer necessary.
            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semester Detail Page
            RockMigrationHelper.DeleteAttribute( "5A60ECD5-9BCA-4DC5-8077-12E00D475C3B" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completion Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Completion Detail Page", "CompletionDetailPage", "Completion Detail Page", @"The page that will show the program completion detail.", 5, @"", "9FEE276F-9CC9-4FD2-99FF-270B721C6300" );

            // Add Page 
            //  Internal Name: Learning University
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "85F25819-E948-4960-9DDF-00F54D32444E", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Learning University", "", "B32639B3-604F-441E-A6E4-2613C0A6BE2B", "" );

            // Add Page 
            //  Internal Name: Courses
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "B32639B3-604F-441E-A6E4-2613C0A6BE2B", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Courses", "", "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3", "" );

            // Add Page 
            //  Internal Name: Course
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Course", "", "FCCDB330-E1EA-4DC2-971E-3900F1EC2826", "" );

            // Add Page 
            //  Internal Name: Class Workspace
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "FCCDB330-E1EA-4DC2-971E-3900F1EC2826", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Class Workspace", "", "61BE63C7-6611-4235-A6F2-B22456620F35", "" );

            // Add Page Route
            //   Page:Learning University
            //   Route:learn
            RockMigrationHelper.AddOrUpdatePageRoute( "B32639B3-604F-441E-A6E4-2613C0A6BE2B", "learn", "5F29A36F-8B41-492B-89A5-76DF70284F0D" );

            // Add Page Route
            //   Page:Courses
            //   Route:learn/{LearningProgramId}
            RockMigrationHelper.AddOrUpdatePageRoute( "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3", "learn/{LearningProgramId}", "FA31BDF7-875A-4AAA-BD27-734FF10AF61A" );

            // Add Page Route
            //   Page:Course
            //   Route:learn/{LearningProgramId}/courses/{LearningCourseId}
            RockMigrationHelper.AddOrUpdatePageRoute( "FCCDB330-E1EA-4DC2-971E-3900F1EC2826", "learn/{LearningProgramId}/courses/{LearningCourseId}", "6AC3D62C-488B-44C8-98AF-7D23B7B701DD" );

            // Add Page Route
            //   Page:Class Workspace
            //   Route:learn/{LearningProgramId}/courses/{LearningCourseId}/{LearningClassId}
            RockMigrationHelper.AddOrUpdatePageRoute( "61BE63C7-6611-4235-A6F2-B22456620F35", "learn/{LearningProgramId}/courses/{LearningCourseId}/{LearningClassId}", "E2EF9FAC-3E9B-4EC8-A21F-D01178416247" );

            // Add Block 
            //  Block Name: Program List
            //  Page Name: Learning University
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "B32639B3-604F-441E-A6E4-2613C0A6BE2B".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "DA1460D8-E895-4B23-8A8E-10EBBED3990F".AsGuid(), "Program List", "Feature", @"", @"", 0, "B15CC3F1-766B-4469-8F95-E31011A3279F" );

            // Add Block 
            //  Block Name: Course List
            //  Page Name: Courses
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "5D6BA94F-342A-4EC1-B024-FC5046FFE14D".AsGuid(), "Course List", "Feature", @"", @"", 0, "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A" );

            // Add Block 
            //  Block Name: Course Detail
            //  Page Name: Course
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "FCCDB330-E1EA-4DC2-971E-3900F1EC2826".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "B0DCE130-0C91-4AA0-8161-57E8FA523392".AsGuid(), "Course Detail", "Feature", @"", @"", 0, "E921788F-38EA-48F2-B80A-9B7181AB70A5" );

            // Add Block 
            //  Block Name: Class Workspace
            //  Page Name: Class Workspace
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "61BE63C7-6611-4235-A6F2-B22456620F35".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "55F2E89B-DE57-4E24-AC6C-576956FB97C5".AsGuid(), "Class Workspace", "Feature", @"", @"", 0, "D46C2787-60BA-4776-BE6E-7F785A984922" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Completion Status", "ShowCompletionStatus", "Show Completion Status", @"Determines if the individual's completion status should be shown.", 3, @"Show", "50A1C1C9-42F9-43A1-B115-B762D64F5E84" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Next Session Date Range", "NextSessionDateRange", "Next Session Date Range", @"Filter to limit the display of upcming sessions.", 4, @",", "E5232B07-6F05-4A14-B58B-DC1978D4D878" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>

                    {% if ShowCompletionStatus %}
    				    {% if course.LearningCompletionStatus == 'Pass' %}
    					    <span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				    {% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					    <span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				    {% elseif course.UnmetPrerequisites != empty %}
                            <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                        {% else %}
                            <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				    {% endif %}
                    {% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Completion Status", "ShowCompletionStatus", "Show Completion Status", @"Determines if the individual's completion status should be shown.", 3, @"Show", "C276C064-85DF-47AF-886B-672A0942496F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Next Session Date Range", "NextSessionDateRange", "Next Session Date Range", @"Filter to limit the display of upcming sessions.", 4, @",", "224BC26D-D7F6-4674-822A-547424E67B77" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Programs, ShowCompletionStatus, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        height: 280px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid=4812baaf-a173-472c-a9a7-8ceb83c06f53'); 
        background-size: cover;
    }
    
    .programs-list-header-section {
        margin-top: 100px;   
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        bottom: -80%;
        -webkit-transform: translateY(-30%);
        transform: translateY(-30%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	<div class=""programs-list-header-section center-block text-center mb-4"">
		<span class=""program-list-header h5"">
			Programs Available
		</span>

		<div class=""program-list-sub-header text-muted"">
			The following types of classes are available.
		</div>
	</div>
	
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>

                {% if ShowCompletionStatus == True %} 
				    {% if program.CompletionStatus == 'Completed' %}
					    <span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
				    {% elseif program.CompletionStatus == 'Pending' %}
					    <span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
				    {% endif %}
                {% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Program Categories", "ProgramCategories", "Program Categories", @"The categories to filter the programs by.", 3, @"", "7F8B8367-2A29-49E1-B2B4-96560AE68510" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Completion Status", "ShowCompletionStatus", "Show Completion Status", @"Determines if the individual's completion status should be shown.", 4, @"Show", "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Facilitator Portal Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "72DFE773-DA2F-45A8-976A-6C19FD0AFE28", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Lava Header Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69", @"{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .header-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
{% endstylesheet %}
<div class=""header-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Summary }}
			</div>
		</div>
	</div>
</div>
" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: The Number of Notifications to Show
            /*   Attribute Value: 3 */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "3B8378C9-80DD-4A50-9197-CA9E1EC88507", @"3" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Show Grades
            /*   Attribute Value: Show */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "6FE66C3C-E37B-440D-942C-88C008E844F5", @"Show" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Workspace Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "8787C9F3-1CF9-4790-B65E-90303F446536", @"61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Enrollment Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "96644CEF-4FC7-4986-B591-D6675AA38C2C", @"61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC", @"//- Variable Assignments
{% assign requirementTypes = Course.CourseRequirements | Distinct:'RequirementType' %}
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .page-main-content {
        margin-top: 30px;   
    }
    
    .course-detail-container {
        background-color: white; 
        border-radius: 12px;
        padding: 12px;
        display: flex;
        flex-direction: column;
    }
    
    .course-status-sidebar-container {
        padding: 12px; 
        margin-left: 12px;
        background-color: white; 
        border-radius: 12px;
        width: 300px;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.Entity.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Entity.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""page-main-content d-flex"">
		<div class=""course-detail-container text-muted"">
			<div class=""description-header h4"">Course Description</div>
			
			<div class=""course-item-pair-container course-code"">
				<span class=""text-bold"">Course Code: </span>
				<span>{{Course.Entity.CourseCode}}</span>
			</div>
			
			<div class=""course-item-pair-container credits"">
				<span class=""text-bold"">Credits: </span>
				<span>{{Course.Entity.Credits}}</span>
			</div>
			
			<div class=""course-item-pair-container prerequisites"">
				<span class=""text-bold"">Prerequisites: </span>
				
				<span>
					{{ prerequisitesText }}
				</span>
			</div>
			
			<div class=""course-item-pair-container description"">
				<span>{{Course.DescriptionAsHtml}}</span>
			</div>
		</div>
		
		
		<div class=""course-side-panel d-flex flex-column"">
			<div class=""course-status-sidebar-container"">
				
			{% case Course.LearningCompletionStatus %}
			{% when 'Incomplete' %} 
				<div class=""sidebar-header text-bold"">Currently Enrolled</div>
				<div class=""sidebar-value text-muted"">You are currently enrolled in this course.</div>
					
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% when 'Passed' %} 
				<div class=""sidebar-header text-bold"">History</div>
				<div class=""sidebar-value text-muted"">You completed this class on {{ Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
				
				<div class=""side-bar-action mt-3"">
					<a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% else %} 
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
				
				<div class=""sidebar-upcoming-schedule h4"">Upcoming Schedule</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">Next Session Semester: </div>
					<div class=""sidebar-value"">{{ Course.NextSemester.Name }}</div>
				</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">{{ 'Instructor' | PluralizeForQuantity:facilitatorCount }}:</div>
					<div class=""sidebar-value"">{{ facilitators }}</div>
				</div>
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
				</div>
			{% endcase %}
			</div>
		</div>
	</div>
</div>" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Detail Page
            /*   Attribute Value: fccdb330-e1ea-4dc2-971e-3900f1ec2826,6ac3d62c-488b-44c8-98af-7d23b7b701dd */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "5A8A251D-56B3-4E3B-8BF9-1D333C016B74", @"fccdb330-e1ea-4dc2-971e-3900f1ec2826,6ac3d62c-488b-44c8-98af-7d23b7b701dd" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Enrollment Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "FF294CFA-6D54-4CBC-B8EF-D45893677D58", @"61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0", @"//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>

                    {% if ShowCompletionStatus %}
    				    {% if course.LearningCompletionStatus == 'Pass' %}
    					    <span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				    {% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					    <span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				    {% elseif course.UnmetPrerequisites != empty %}
                            <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                        {% else %}
                            <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				    {% endif %}
                    {% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
	</div>
</div>" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Show Completion Status
            /*   Attribute Value: Show */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "C276C064-85DF-47AF-886B-672A0942496F", @"Show" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Next Session Date Range
            /*   Attribute Value: DateRange|||2024-07-18T00:00:00.0000000|2025-07-18T00:00:00.0000000 */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "224BC26D-D7F6-4674-822A-547424E67B77", @"DateRange|||2024-07-18T00:00:00.0000000|2025-07-18T00:00:00.0000000" );

            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C", @"//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        height: 280px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid=4812baaf-a173-472c-a9a7-8ceb83c06f53'); 
        background-size: cover;
    }
    
    .programs-list-header-section {
        margin-top: 100px;   
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        bottom: -80%;
        -webkit-transform: translateY(-30%);
        transform: translateY(-30%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	<div class=""programs-list-header-section center-block text-center mb-4"">
		<span class=""program-list-header h5"">
			Programs Available
		</span>

		<div class=""program-list-sub-header text-muted"">
			The following types of classes are available.
		</div>
	</div>
	
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>
				
				{% if ShowCompletionStatus %}
    				{% if program.CompletionStatus == 'Completed' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif program.CompletionStatus == 'Pending' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% endif %}
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>" );

            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Show Completion Status
            /*   Attribute Value: Show */
            RockMigrationHelper.AddBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7", @"Show" );

            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Detail Page
            /*   Attribute Value: 5b9dea29-8c8f-4edd-9fcf-739061b654d3,fa31bdf7-875a-4aaa-bd27-734ff10af61a */
            RockMigrationHelper.AddBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "0317E0AD-9FE9-409B-9A14-C3D30D303B23", @"5b9dea29-8c8f-4edd-9fcf-739061b654d3,fa31bdf7-875a-4aaa-bd27-734ff10af61a" );
        }

        /// <summary>
        /// The Down methods from the SQL code-generated CodeGen_PagesBlocksAttributesMigration.sql file.
        /// </summary>
        private void DeleteBlockAndPageAttributes()
        {
            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semester Detail Page
            RockMigrationHelper.DeleteAttribute( "5A60ECD5-9BCA-4DC5-8077-12E00D475C3B" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completion Detail Page
            RockMigrationHelper.DeleteAttribute( "9FEE276F-9CC9-4FD2-99FF-270B721C6300" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.DeleteAttribute( "E5232B07-6F05-4A14-B58B-DC1978D4D878" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.DeleteAttribute( "50A1C1C9-42F9-43A1-B115-B762D64F5E84" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.DeleteAttribute( "224BC26D-D7F6-4674-822A-547424E67B77" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.DeleteAttribute( "C276C064-85DF-47AF-886B-672A0942496F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.DeleteAttribute( "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program Categories
            RockMigrationHelper.DeleteAttribute( "7F8B8367-2A29-49E1-B2B4-96560AE68510" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );

            // Remove Block
            //  Name: Program List, from Page: Learning University, Site: External Website
            //  from Page: Learning University, Site: External Website
            RockMigrationHelper.DeleteBlock( "B15CC3F1-766B-4469-8F95-E31011A3279F" );

            // Remove Block
            //  Name: Course List, from Page: Courses, Site: External Website
            //  from Page: Courses, Site: External Website
            RockMigrationHelper.DeleteBlock( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A" );

            // Remove Block
            //  Name: Public Learning Course Detail, from Page: Course, Site: External Website
            //  from Page: Course, Site: External Website
            RockMigrationHelper.DeleteBlock( "E921788F-38EA-48F2-B80A-9B7181AB70A5" );

            // Remove Block
            //  Name: Class Workspace, from Page: Class Workspace, Site: External Website
            //  from Page: Class Workspace, Site: External Website
            RockMigrationHelper.DeleteBlock( "D46C2787-60BA-4776-BE6E-7F785A984922" );

            // Remove Block
            //  Name: Program Completion Detail, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F7F137EE-C66D-4BF9-B117-DC8899C6603A" );

            // Delete Page 
            //  Internal Name: Class Workspace
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "61BE63C7-6611-4235-A6F2-B22456620F35" );

            // Delete Page 
            //  Internal Name: Course
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "FCCDB330-E1EA-4DC2-971E-3900F1EC2826" );

            // Delete Page 
            //  Internal Name: Courses
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3" );

            // Delete Page 
            //  Internal Name: Learning University
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "B32639B3-604F-441E-A6E4-2613C0A6BE2B" );

            // Delete BlockType 
            //   Name: Learning Program Completion Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program Completion Detail
            RockMigrationHelper.DeleteBlockType( "E0C38A42-2ACE-4258-8D11-BD971C41EADB" );
        }

        /// <summary>
        /// Sets the ShowInGroupList value for the LMS Group Type.
        /// </summary>
        /// <param name="show"><c>true</c> to show in Group List, otherwise <c>false</c></param>
        private void SetShowInGroupList( bool show )
        {
            var sqlBitValue = show ? "1" : "0";
            Sql( $@"
DECLARE @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775';
UPDATE [dbo].[GroupType] SET [ShowInGroupList] = {sqlBitValue} WHERE [Guid] = @lmsGroupTypeGuid
" );
        }

        /// <summary>
        /// Sets the BreadCrumbDisplayName bit value for LMS Detail Pages.
        /// </summary>
        /// <param name="showBreadCrumbDisplayName"><c>true</c> to show in Group List, otherwise <c>false</c></param>
        private void SetBreadCrumbForDetailPagesWithBreadCrumbBlocks( bool showBreadCrumbDisplayName )
        {
            var sqlBitValue = showBreadCrumbDisplayName ? "1" : "0";
            Sql( $@"
DECLARE @programDetailGuid NVARCHAR(40) = '7888CAF4-AF5D-44BA-AB9E-80138361F69D',
		@courseDetailGuid NVARCHAR(40) = 'A57D990E-6F34-45CF-ABAA-08C40E8D4844',
		@classDetailGuid NVARCHAR(40) = '23D5076E-C062-4987-9985-B3A4792BF3CE',
		@activityDetailGuid NVARCHAR(40) = 'D72DCBC4-C57F-4028-B503-1954925EDC7D',
		@completionDetailGuid NVARCHAR(40) = 'E0F2E4F1-ED10-49F6-B053-AC6807994204',
		@participantDetailGuid NVARCHAR(40) = '72C75C91-18F8-48D0-B0CF-1FBD82EB50FC',
		@contentPageDetailGuid NVARCHAR(40) = 'E6A89360-B7B4-48A8-B799-39A27EAB6F36',
		@announcementDetailGuid NVARCHAR(40) = '7C8134A4-524A-4C3D-BA4C-875FEE672850',
		@semesterDetailGuid NVARCHAR(40) = '36FFA805-B283-443E-990D-87040339D16F';

UPDATE p SET
	BreadCrumbDisplayName = {sqlBitValue}
FROM [dbo].[Page] p
WHERE p.[Guid] IN (
	@programDetailGuid, 
	@courseDetailGuid, 
	@classDetailGuid, 
	@activityDetailGuid,
	@completionDetailGuid, 
	@participantDetailGuid,
	@contentPageDetailGuid,
	@announcementDetailGuid,
	@semesterDetailGuid
)" );
        }

        /// <summary>
        /// Adds a parent and child history category for <see cref="LearningActivityCompletion"/> changes.
        /// </summary>
        private void AddHistoryCategories()
        {
            Sql( @"
DECLARE @historyEntityTypeId INT = (SELECT TOP 1 Id FROM [dbo].[EntityType] WHERE [Guid] = '546D5F43-1184-47C9-8265-2D7BF4E1BCA5');
DECLARE @parentCategoryGuid NVARCHAR(200) = 'FE5E132D-1A34-4ED5-AE0D-0FDE26D88D25';

-- Add the parent category.
INSERT [Category] ( [IsSystem], [EntityTypeId], [Name], [Description], [Order], [Guid] )
SELECT [IsSystem], [EntityTypeId], [Name], [Description], [Order], [Guid]
FROM (
	SELECT 1 [IsSystem], @historyEntityTypeId [EntityTypeId], 'Learning' [Name], 'Learning History' [Description], 0 [Order], @parentCategoryGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[Category] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)

DECLARE @learningParentCategoryId INT = (SELECT TOP 1 [Id] FROM [dbo].[Category] WHERE [Guid] = @parentCategoryGuid);
DECLARE @childCategoryGuid NVARCHAR(200) = '40A49BFF-5AE5-487B-B4AA-95DE435209FE';
-- Add the child category.
INSERT [Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid] )
SELECT [IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid]
FROM (
	SELECT 1 [IsSystem], @learningParentCategoryId [ParentCategoryId], @historyEntityTypeId [EntityTypeId], 'Activity Completion' [Name], 'fa fa-university' [IconCssClass], 'Learning History' [Description], 0 [Order], @childCategoryGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[Category] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)" );
        }

        /// <summary>
        /// Removes the parent and child history categories for <see cref="LearningActivityCompletion"/> changes.
        /// </summary>
        private void RemoveHistoryCategories()
        {
            Sql( @"
-- Remove the Learning History Categories.
DECLARE @parentCategoryGuid NVARCHAR(200) = 'FE5E132D-1A34-4ED5-AE0D-0FDE26D88D25';
DECLARE @childCategoryGuid NVARCHAR(200) = '40A49BFF-5AE5-487B-B4AA-95DE435209FE';
DELETE Category WHERE [Guid] IN (@parentCategoryGuid, @childCategoryGuid)
" );
        }

        #endregion

        #region JE: Peer Network Relationship Type for Following

        private void PeerNetworkRelationshipTypeForFollowingUp()
        {
            Sql( @"
DECLARE @RelationshipTypeDefindedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'f2e8e639-f16d-489d-aafb-be0133531e41');
INSERT INTO [DefinedValue]
( [DefinedTypeId], [Value], [Description], [Guid], [IsSystem], [Order] )
VALUES
( @RelationshipTypeDefindedTypeId, 'Following Connections','Connects individuals who are following each other.', '84E0360E-0828-E5A5-4BCC-F3113BE338A1', 1, 0 )
" );
        }

        #endregion

        #region KA: Migration to Update Giving Metric SQL

        void UpdateGivingMetricSqlUp()
        {
                        Sql( @"
UPDATE dbo.[Metric]
SET [SourceSql] = REPLACE(
	REPLACE([SourceSql], 'DECLARE @Accounts VARCHAR(100) = ''1''' , 'DECLARE @Accounts VARCHAR(100) = ''1,2,3'''),
'SELECT
    [GivingAmount] AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
	, [AccountName]
FROM CTE
GROUP BY [AccountCampusId], [AccountName], [GivingAmount];',
'SELECT
    SUM([GivingAmount]) AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
FROM CTE
GROUP BY [AccountCampusId];')
WHERE [Guid] = '43338E8A-622A-4195-B153-285E570B229D'
" );
        }

        #endregion

        #region KA: D: v17 Data Migration to add route name for Tithing Overview

        private void AddRouteNameForTithingOverviewUp()
        {
            // Add Page Route.
            RockMigrationHelper.AddOrUpdatePageRoute( "72BA5DD9-8685-4182-833D-22BB1E0F9A36", "finance/tithing-overview", "8DC007D6-1C14-4D8D-B04B-886F36406806" );

            // Update Tithing Metrics SQL to not include PostalCodes with negative FamiliesMedianTithe.
            Sql( @"
UPDATE dbo.[Metric]
SET SourceSql = REPLACE([SourceSql]
,'FamiliesMedianTithe is NOT NULL'
,'[FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0')
WHERE [Guid] IN ('f4951a42-9f71-4cb1-a46e-2a7ed84cd923','2b798177-e8f4-46db-a1d7-308d63ca519a', 'b5bfab51-9b46-4e7e-992e-b0119e4d25ec')
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddRouteNameForTithingOverviewDown()
        {
            RockMigrationHelper.DeletePageRoute( "8DC007D6-1C14-4D8D-B04B-886F36406806" );
        }

        #endregion

        #region KH: v17 ONLY - Reorganize Admin Settings Pages (data migration rollup)

        private const string AdministrationPageGuid = "550A898C-EDEA-48B5-9C58-B20EC13AF13B";
        private const string SQLPageGuid = "03C49950-9C4C-4668-9C65-9A0DF43D1B33";
        private const string DigitalToolsPageGuid = "A6D78C4F-958F-4196-B8FF-527A10F5F047";
        private const string ContentChannelsPageGuid = "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E";
        private const string WebsitesPageGuid = "7596D389-4EAB-4535-8BEE-229737F46F44";
        private const string MobileApplicationsPageGuid = "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6";
        private const string TVAppsPageGuid = "C8B81EBE-E98F-43EF-9E39-0491685145E2";
        private const string MediaAccountsPageGuid = "07CB7BB5-1465-4E75-8DD4-28FA6EA48222";
        private const string ShortLinksPageGuid = "8C0114FF-31CF-443E-9278-3F9E6087140C";
        private const string AdaptiveMessagePageGuid = "73112D38-E051-4452-AEF9-E473EEDD0BCB";

        private void ReorganizeAdminSettingsPages()
        {
            AddAndSetupDigitalToolsPage();
            UpdateAdminToolsPages();
            MoveAdminToolsPages();
            AddAdminToolsPageRoutes();
        }

        private void AddAndSetupDigitalToolsPage()
        {
            // Add Page 
            //  Internal Name: Digital Tools
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84E12152-E456-478E-AF68-BA640E9CE65B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Digital Tools", "", "A6D78C4F-958F-4196-B8FF-527A10F5F047", "" );

            Sql( $@"
                DECLARE @EntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.PAGE}')
                DECLARE @CMSSourcePageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B4A24AB7-9369-4055-883F-4F4892C39AE3')
                DECLARE @DigitalToolsPageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A6D78C4F-958F-4196-B8FF-527A10F5F047')

                INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId])
                SELECT 
	                  a1.[EntityTypeId]
	                , @DigitalToolsPageId
	                , a1.[Order]
	                , a1.[Action]
	                , a1.[AllowOrDeny]
	                , a1.[SpecialRole]
	                , a1.[GroupId]
	                , NEWID()
                    , a1.[PersonAliasId]
                FROM [Auth] a1
                WHERE a1.[EntityTypeId] = @EntityTypeId
	                AND a1.[EntityId] = @CMSSourcePageId
	                AND NOT EXISTS (
		                SELECT 1
		                FROM [Auth] a2
		                WHERE a2.[EntityTypeId] = a1.[EntityTypeId]
			                AND a2.[EntityId] = @DigitalToolsPageId
			                AND a2.[Order] = a1.[Order]
			                AND a2.[Action] = a1.[Action]
			                AND a2.[AllowOrDeny] = a1.[AllowOrDeny]
			                AND a2.[SpecialRole] = a1.[SpecialRole]
                            AND (a2.[GroupId] = a1.[GroupId] OR (a2.GroupId IS NULL AND a1.GroupId IS NULL))
                            AND (a2.[PersonAliasId] = a1.[PersonAliasId] OR (a2.[PersonAliasId] IS NULL AND a1.[PersonAliasId] IS NULL)))" );
        }

        private void UpdateAdminToolsPages()
        {
            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Administration',
[PageTitle] = 'Administration',
[BrowserTitle] = 'Administration'
WHERE [Page].[Guid] = '{AdministrationPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Settings',
[PageTitle] = 'Settings',
[BrowserTitle] = 'Settings'
WHERE [Page].[Guid] = 'A7E36E7A-EFBD-4912-B46E-BB61A74B86FF'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'SQL Editor',
[PageTitle] = 'SQL Editor',
[BrowserTitle] = 'SQL Editor'
WHERE [Page].[Guid] = '{SQLPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 1
WHERE [Guid] = '{ContentChannelsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Websites',
[PageTitle] = 'Websites',
[BrowserTitle] = 'Websites',
[Order] = 2
WHERE [Page].[Guid] = '{WebsitesPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 3
WHERE [Guid] = '{MobileApplicationsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'TV Apps',
[PageTitle] = 'TV Apps',
[BrowserTitle] = 'TV Apps',
[Order] = 4
WHERE [Page].[Guid] = '{TVAppsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 5
WHERE [Guid] = '{MediaAccountsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 6
WHERE [Guid] = '{ShortLinksPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Adaptive Messages',
[PageTitle] = 'Adaptive Messages',
[BrowserTitle] = 'Adaptive Messages',
[Order] = 7
WHERE [Guid] = '{AdaptiveMessagePageGuid}'" );
        }

        private void MoveAdminToolsPages()
        {
            RockMigrationHelper.MovePage( SQLPageGuid, AdministrationPageGuid ); // SQL Editor
            RockMigrationHelper.MovePage( ContentChannelsPageGuid, DigitalToolsPageGuid ); // Content Channels
            RockMigrationHelper.MovePage( WebsitesPageGuid, DigitalToolsPageGuid ); // Websites
            RockMigrationHelper.MovePage( MobileApplicationsPageGuid, DigitalToolsPageGuid ); // Mobile Applications
            RockMigrationHelper.MovePage( TVAppsPageGuid, DigitalToolsPageGuid ); // TV Apps
            RockMigrationHelper.MovePage( MediaAccountsPageGuid, DigitalToolsPageGuid ); // Media Accounts
            RockMigrationHelper.MovePage( ShortLinksPageGuid, DigitalToolsPageGuid ); // Short Links
            RockMigrationHelper.MovePage( AdaptiveMessagePageGuid, DigitalToolsPageGuid ); // Adaptive Message
        }

        private void AddAdminToolsPageRoutes()
        {
            // First we're going to replace/hijack the old route name with the new route name for that particular existing route (926F6D75-1753-0ECD-868D-C7DE06C400DB)
            RockMigrationHelper.AddOrUpdatePageRoute( SQLPageGuid, "admin/sql", "926F6D75-1753-0ECD-868D-C7DE06C400DB" ); // SQL Editor New Route
            // Now we're going to add the old route name back in with a new route record (with new GUID 91A2528B-F4A6-47F0-82CA-A4A64895C8B0)
            RockMigrationHelper.AddOrUpdatePageRoute( SQLPageGuid, "admin/power-tools/sql", "91A2528B-F4A6-47F0-82CA-A4A64895C8B0" ); // SQL Editor Old Route
            // Continue following the pattern described in the comments above for the rest of the page routes.
            RockMigrationHelper.AddOrUpdatePageRoute( ContentChannelsPageGuid, "admin/content-channels", "EFAA0125-1F7F-A293-1EC2-CFA1EF5E1D34" ); // Content Channels New Route
            RockMigrationHelper.AddOrUpdatePageRoute( ContentChannelsPageGuid, "admin/cms/content-channels", "EC7A7EEE-D3C0-4A6E-B778-B4D0CD064795" ); // Content Channels Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( WebsitesPageGuid, "admin/websites", "2EF88F58-0726-4B08-B40F-81D984642F00" ); // Websites New Route
            RockMigrationHelper.AddOrUpdatePageRoute( MobileApplicationsPageGuid, "admin/mobile-applications", "077F1E30-4803-79E6-0658-6A906C1143C5" ); // Mobile Applications New Route
            RockMigrationHelper.AddOrUpdatePageRoute( MobileApplicationsPageGuid, "admin/cms/mobile-applications", "473BF3DB-AEFA-4BE6-B083-FE708EF43349" ); // Mobile Applications Old Route

            Sql( $@"UPDATE [PageRoute]
SET [PageRoute].[Route] = 'admin/tv-apps'
WHERE [PageRoute].[Id] = (
SELECT TOP 1 [PageRoute].[Id]
FROM [PageRoute]
WHERE [PageRoute].[PageId] = (
SELECT [Page].[Id]
FROM [Page]
WHERE [Page].[Guid] = '{TVAppsPageGuid}')
AND [PageRoute].[Route] = 'admin/cms/appletv-applications'
AND [PageRoute].[Guid] != '4FED41DF-C9F8-411C-B281-EB6A8B794399'
ORDER BY [PageRoute].[Id] ASC)" );// TV Apps New Route

            RockMigrationHelper.AddOrUpdatePageRoute( TVAppsPageGuid, "admin/cms/appletv-applications", "4FED41DF-C9F8-411C-B281-EB6A8B794399" ); // TV Apps Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( MediaAccountsPageGuid, "admin/media-accounts", "B12F5786-88A4-6D48-01EA-6E32514C220E" ); // Media Accounts New Route
            RockMigrationHelper.AddOrUpdatePageRoute( MediaAccountsPageGuid, "admin/cms/media-accounts", "9C8B1779-3A9D-4463-8928-BFF97EA355A4" ); // Media Accounts Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( ShortLinksPageGuid, "admin/short-links", "87A329D4-6358-3BB2-0F05-51C814773DC7" ); // Short Links New Route
            RockMigrationHelper.AddOrUpdatePageRoute( ShortLinksPageGuid, "admin/cms/short-links", "671B7309-9725-4D02-937F-D9FE0BE5BD1D" ); // Short Links Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( AdaptiveMessagePageGuid, "admin/adaptive-messages", "3B35F17E-B2DE-4512-8873-06A82F572ABD" ); // Adaptive Message New Route
            RockMigrationHelper.AddOrUpdatePageRoute( AdaptiveMessagePageGuid, "admin/cms/adaptive-messages", "620D5715-AB0F-4CA4-9956-EABF5CDFEFBF" ); // Adaptive Message Old Route
        }

        #endregion
    }
}
