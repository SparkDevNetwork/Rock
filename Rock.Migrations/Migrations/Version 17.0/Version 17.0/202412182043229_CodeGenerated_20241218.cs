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
    public partial class CodeGenerated_20241218 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update Public Learning Program List BlockType Guid
            Sql( @"UPDATE [BlockType] SET [Guid] = 'DA1460D8-E895-4B23-8A8E-10EBBED3990F' WHERE [Guid] = '2FC656DA-7F5D-41B3-AD18-BFE692CFCA57' AND NOT EXISTS(SELECT * FROM [BlockType] WHERE [Guid] = 'DA1460D8-E895-4B23-8A8E-10EBBED3990F')" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentList", "Signature Document List", "Rock.Blocks.Core.SignatureDocumentList, Rock.Blocks, Version=1.17.0.33, Culture=neutral, PublicKeyToken=null", false, false, "B4526EB4-3CA4-47BE-B686-4B9FBEE2BF4D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentDetail", "Signature Document Detail", "Rock.Blocks.Core.SignatureDocumentDetail, Rock.Blocks, Version=1.17.0.33, Culture=neutral, PublicKeyToken=null", false, false, "BCA3D113-8A98-4757-8471-A737011226A9" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document Detail", "Displays the details of a given signature document.", "Rock.Blocks.Core.SignatureDocumentDetail", "Core", "B80E8563-41F2-4528-81E5-C62CF1ECE9DE" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document List", "Block for viewing values for a signature document type.", "Rock.Blocks.Core.SignatureDocumentList", "Core", "6076609B-D4D2-4825-8BB2-8681E99C59F2" );

            // Update Order for Page: Prayer,  Zone: Main,  Block: Add Prayer Request
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'A2595CD0-A674-47D0-986E-DF74866C1777'" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Auto Focus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Focus", "AutoFocus", "Auto Focus", @"Decide whether the textbox should automatically receive focus when the page loads.", 0, @"True", "7CEF3A4B-2B60-4577-A49E-8F3B707C6D64" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Register
            //   Category: Engagement > Sign-Up
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Prospect').", 6, @"368DD475-242C-49C4-A42C-7278BE690CC2", "5035FE34-274C-46B8-BA10-32D0B4CADA2F" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Register
            //   Category: Engagement > Sign-Up
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending').", 7, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "8AB81246-8429-4660-B736-0DCF52A53543" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Confirmed Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmed Button Text", "ConfirmedButtonText", "Confirmed Button Text", @"The text to display for the Confirmed Button Text.", 4, @"Confirmed", "AE39BCCF-5F0C-4E2B-A0E0-B6875B9AC34D" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Decline Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Decline Button Text", "DeclineButtonText", "Decline Button Text", @"The text to display for the Decline Button Text.", 5, @"Cancel Confirmation", "410A9CBF-AEDC-42F4-AA60-4C092486437E" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "CourseDetailTemplate", "Lava Template", @"The Lava template to use to render the page. Merge fields include: CourseInfo, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles

<style>

    @media (max-width: 991px) {
        .course-side-panel {
            padding-left: 0;
        }
        .card {
            margin-bottom: 24px;
        }
    }
    
    @media (max-width: 767px) {
        h1 {
            font-size: 28px;
        }
        .card {
            margin-bottom: 24px;
        }
    }

</style>

<div class=""d-flex flex-column gap-4"">
    
    <div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ CourseInfo.ImageFileGuid}}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ CourseInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ CourseInfo.Summary }} </p>
        </div>
    </div>

    <div>

        <div class=""row"">

            <div class=""col-xs-12 col-sm-12 col-md-8""> //- LEFT CONTAINER

                <div class=""card rounded-lg""> //- COURSE DESCRIPTION

                    <div class=""card-body"">
                        <div class=""card-title"">
                            <h4 class=""m-0"">Course Description</h4>
                        </div>
                        <div class=""card-text"">
                            {% if CourseInfo.CourseCode != empty %}
                            <div class=""text-gray-600 d-flex gap-1"">
                                <p class=""text-bold mb-0"">Course Code: </p>
                                <p class=""mb-0"">{{CourseInfo.CourseCode}}</p>
                            </div>
                            {% endif %}

                            <div class=""d-flex text-gray-600 gap-1 pb-3"">
                                <p class=""text-bold mb-0"">Credits: </p>
                                <p class=""mb-0"">{{CourseInfo.Credits}}</p>
                            </div>
                            <div class=""pt-3 border-top border-gray-200"">
                                {% if CourseInfo.DescriptionAsHtml == empty %}
                                    <span>No course description provided.</span>
                                {% else %}
                                    <span>{{CourseInfo.DescriptionAsHtml}}</span>
                                {% endif %}
                            </div>
                        </div>
                    </div>

                </div>

            </div>

            <div class=""col-xs-12 col-sm-12 col-md-4""> //- RIGHT CONTAINER
                <div class=""card rounded-lg mb-4"">
                    <div class=""card-body"">
                        <div class=""card-title d-flex align-items-center"">
							<h4 class=""m-0""><span><i class=""fa fa-clipboard-list mr-2""></i></span>Requirements
							    {% if CourseInfo.UnmetPrerequisites != empty %}
                                    <i class=""fa fa-exclamation-circle text-danger""></i>
                                    </h4>
                                {% else %}
                                    <i class=""fa fa-check-circle text-success""></i>
                                    </h4>
                                {% endif %}
							</h4>
						</div>
						<div class=""card-text text-muted"">
							{% if CourseInfo.CourseRequirements != empty %}
                                {% assign requirementsText = CourseInfo.CourseRequirements |
                                Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                                {% if CourseInfo.UnmetPrerequisites != empty %}
                                    </p>
                                    <p class=""mb-0"">{{requirementsText}}</p>
                                    <p class=""text-danger mb-0 mt-3"">You do not meet the course requirements.</p>
                                {% else %}
                                    </p>
                                    <p class=""mb-0"">{{requirementsText}}</p>
                                {% endif %}
                            {% else %}
                            <p class=""mb-0"">None</p>
                            {% endif %}
						</div>
                    </div>
                </div>
                
                {% assign today = 'Now' | Date:'yyyy-MM-dd' %}
                {% assign hasClasses = false %}
                {% for semesterInfo in CourseInfo.Semesters %}
                    
                    {% for classInfo in semesterInfo.AvailableClasses %}
                        {% if semesterInfo.EnrollmentCloseDate == null or semesterInfo.EnrollmentCloseDate >= today %}
                            {% assign hasClasses = true %}
						{% endif %}
						//- CURRENTLY ENROLLED
						{% assign isActiveSemester = semesterInfo.StartDate == null 
						    or today >= semesterInfo.StartDate %}
						{% if classInfo.IsEnrolled 
						and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" 
						and isActiveSemester %} 
							
							<div class=""card rounded-lg mb-4"">
								<div class=""card-body"">
									<div class=""card-title d-flex align-items-center"">
										<i class=""fa fa-user-check mr-2""></i>
										<h4 class=""m-0"">Currently Enrolled</h4>
									</div>
									<div class=""card-text text-muted mb-3"">
										<p>You are currently enrolled in this course.</p>
										<p class=""text-gray-800""><i class=""fa fa-arrow-right mr-2""></i>{{classInfo.Name}}</p>
									</div>
									<div>
										<a class=""btn btn-info"" href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
									</div>
								</div>
							</div>
                        {% elseif classInfo.StudentParticipant and classInfo.StudentParticipant.LearningCompletionStatus != ""Incomplete"" %}
							
                            //- HISTORICAL ACCESS
                            {% if CourseInfo.AllowHistoricalAccess == true %}
                            
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}.</div>
                                        <div class=""mt-2"">
                                            <a href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
                                        </div>
                                    </div>
                                    
                                </div>
							 
							//- NO HISTORICAL ACCESS
							{% else %}
                                
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}</div>
                            
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                    </div>
                                    
                                </div>
                                
                            {% endif %}
                        {% endif %}
                    {% endfor %}
                {% endfor %}
                                
                {% if hasClasses == false %} //- NO UPCOMING SEMESTERS OR CLASSES
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""mt-0"">No Available Upcoming Classes</h4>
                            <p class=""text-gray-600"">Please check back again later.</p>
                        </div>
                    </div>
                {% else %}
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""card-title mt-0""><i class=""fa fa-chalkboard-teacher mr-2""></i>Classes</h4>
                            
                            //- SCOPING TO CLASS DETAILS
                            
                            {% for semesterInfo in CourseInfo.Semesters %}

                                {% assign semesterStartDate = semesterInfo.StartDate | Date: 'sd' %}
                                {% assign semesterEndDate = semesterInfo.EndDate | Date: 'sd' %}
                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                    <div class=""py-1"">
                                        <h5 class=""mt-0 mb-0"">{{semesterInfo.Name}}</h5>
                                        {% if semesterStartDate and semesterEndDate %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}-{{semesterEndDate}}</p>
                                        {% elseif semesterEndDate != empty %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}</p>
                                        {% else %}
                                            <p class=""text-gray-600"">Date Pending</p>
                                        {% endif %}
                                    </div>
                                {% endif %}
                                        
                                {% for classInfo in semesterInfo.AvailableClasses %}
                                    {% assign facilitatorsText = classInfo.Facilitators | Select:'Name' | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
                                   
                                    <div class=""card rounded-lg bg-gray-100 mb-4"">
                                        <div class=""card-body pb-0"">
                                            <div class=""d-grid grid-flow-row gap-0 mb-3""> //- BEGIN CLASS DETAILS
                                                
                                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                    <p class=""text-bold"">{{classInfo.Name}}
                                                    {% if classInfo.IsEnrolled and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" %}
                                                        <span class=""text-normal align-top badge bg-info"">Enrolled</span>
                                                    {% elseif classInfo.IsEnrolled %}
														<span class=""text-normal align-top badge bg-success"">Recently Completed</span>
                                                    {% endif %}
                                                    </p>
                                                {% else %}
                                                <h4 class=""mt-0"">Class Details</h4>
                                                {% endif %}
                                                
                                                <div class=""d-flex flex-column"">
                                                    {% if facilitatorsText %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Facilitators: 
                                                            <p>{{facilitatorsText}}</p>
                                                        </div>
                                                    {% endif %}
                                                    
                                                    {% if classInfo.Campus %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Campus: 
                                                            <p>{{classInfo.Campus}}</p>
                                                        </div>
                                                    {% endif %}
                                                    {% if classInfo.Location %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Location: 
                                                            <p>{{classInfo.Location}}</p>
                                                        </div>
                                                    {% endif %}
                                                    {% if classInfo.Schedule %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Schedule: 
                                                            <p>{{classInfo.Schedule}}</p>
                                                        </div>
                                                    {% endif %}
                                                    
                                                    {% if classInfo.IsEnrolled  == false %} //- NEVER ENROLLED?
                                                        {% if semesterInfo.IsEnrolled == true and classInfo.IsEnrolled == false %}
                                                            {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                                <p class=""text-danger"">You've already enrolled in this course this semester.</p>
                                                            {% else %}
                                                                <p class=""text-danger"">You've already enrolled in a version of this course.</p>
                                                            {% endif %}
                                                        {% elseif classInfo.CanEnroll == false %}
                                                            {% if classInfo.EnrollmentErrorKey == 'class_full' %}
                                                                <p class=""text-danger"">Class is full.</p>
                                                            {% endif %}
                                                        
                                                        {% else %}
                                                            {% if CourseInfo.ProgramInfo.ConfigurationMode != ""AcademicCalendar"" or semesterInfo.IsEnrolled  == false %}
                                                                <div>
                                                                    <a class=""btn btn-default"" href=""{{ classInfo.EnrollmentLink }}"">Enroll</a>
                                                                </div>
                                                            {% endif %}
                                                        {% endif %}
                                                    {% endif %}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                {% endfor %}
       
                            {% endfor %}
                        
                        </div>
                    </div>        
                    
                {% endif %}

            </div>
        </div>
    </div>
</div>
", "DA6C3170-5264-427D-AC22-8D50D2F6D2F6" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Title", "PageTitle", "Page Title", @"Provide a clear, welcoming title for the Learning Hub homepage. Example: 'Grow Together in Faith.'", 1, @"Learning Hub", "95945DE7-48C1-439D-AFE1-F5AF23BAA1DF" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Description", "PageDescription", "Page Description", @"Enter a brief description for the homepage to introduce users to the LMS. Example: 'Explore resources to deepen your faith and connect with our community.'", 2, @"Explore courses and trainings designed to deepen your faith, help you grow in spiritual knowledge, and prepare you for serving and volunteering.", "1AEA5EC9-C5E1-4B61-8C53-745629444D6A" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Image
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Image", "BannerImage", "Image", @"Add a welcoming banner image to visually enhance the homepage. Ideal size: 1200x400 pixels; use high-quality images.", 0, @"605FD4B7-2DCA-4782-8826-95AAC6C6BAB6", "8A88307E-CB82-4A69-9A5D-D53856F2E484" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public programs will be excluded.", 5, @"True", "74723C91-D52C-4468-AB1C-16C7AE98B21C" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6076609B-D4D2-4825-8BB2-8681E99C59F2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the signature document details.", 0, @"", "2D9F3DAB-723C-4C22-A583-603F81F5EFB4" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6076609B-D4D2-4825-8BB2-8681E99C59F2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C231C26D-772D-4C93-8D73-BECE7BB61B6E" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6076609B-D4D2-4825-8BB2-8681E99C59F2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5228AB7E-4698-45C5-9F2E-CCAEC93E0EA4" );

            // Add Block Attribute Value
            //   Block: Group Member Schedule Template List
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Block Location: Page=Group Member Schedule Templates, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "A0D34249-661A-4D94-9D0D-70AB604A60FA", "2989AD4D-07F4-437C-8C3E-F99E54A8881D", @"" );

            // Add Block Attribute Value
            //   Block: Group Member Schedule Template List
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Block Location: Page=Group Member Schedule Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "A0D34249-661A-4D94-9D0D-70AB604A60FA", "DA9054C8-7D99-489D-A3A9-00E7A61B8750", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Sign-Up Register
            //   Category: Engagement > Sign-Up
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute( "8AB81246-8429-4660-B736-0DCF52A53543" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Register
            //   Category: Engagement > Sign-Up
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute( "5035FE34-274C-46B8-BA10-32D0B4CADA2F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute( "74723C91-D52C-4468-AB1C-16C7AE98B21C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Image
            RockMigrationHelper.DeleteAttribute( "8A88307E-CB82-4A69-9A5D-D53856F2E484" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Description
            RockMigrationHelper.DeleteAttribute( "1AEA5EC9-C5E1-4B61-8C53-745629444D6A" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Title
            RockMigrationHelper.DeleteAttribute( "95945DE7-48C1-439D-AFE1-F5AF23BAA1DF" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Auto Focus
            RockMigrationHelper.DeleteAttribute( "7CEF3A4B-2B60-4577-A49E-8F3B707C6D64" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "1B7A6515-A82A-4E0E-A092-9FB26BD56D7E" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5228AB7E-4698-45C5-9F2E-CCAEC93E0EA4" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C231C26D-772D-4C93-8D73-BECE7BB61B6E" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "2D9F3DAB-723C-4C22-A583-603F81F5EFB4" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Decline Button Text
            RockMigrationHelper.DeleteAttribute( "410A9CBF-AEDC-42F4-AA60-4C092486437E" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Confirmed Button Text
            RockMigrationHelper.DeleteAttribute( "AE39BCCF-5F0C-4E2B-A0E0-B6875B9AC34D" );

            // Delete BlockType 
            //   Name: Signature Document List
            //   Category: Core
            //   Path: -
            //   EntityType: Signature Document List
            RockMigrationHelper.DeleteBlockType( "6076609B-D4D2-4825-8BB2-8681E99C59F2" );

            // Delete BlockType 
            //   Name: Signature Document Detail
            //   Category: Core
            //   Path: -
            //   EntityType: Signature Document Detail
            RockMigrationHelper.DeleteBlockType( "B80E8563-41F2-4528-81E5-C62CF1ECE9DE" );
        }
    }
}
