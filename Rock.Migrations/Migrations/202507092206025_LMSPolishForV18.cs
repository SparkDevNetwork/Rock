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
    public partial class LMSPolishForV18 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixLMSGradingBreadcrumbsUp();
            UpdatePublicLearningLavaAttributesUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            FixLMSGradingBreadcrumbsDown();
            UpdatePublicLearningLavaAttributesDown();
        }

        #region KH: Fix LMS Grading Breadcrumbs.

        private void FixLMSGradingBreadcrumbsUp()
        {
            Sql( @"
UPDATE [Page]
SET [BreadCrumbDisplayName] = 0
WHERE [Guid] = '6163CCFD-CB15-452E-99F2-229A5E5B22F0'
   OR [Guid] = 'AE85B3FC-C951-497F-8C43-9D0A1E467A50'"
);
        }

        private void FixLMSGradingBreadcrumbsDown()
        {
            Sql( @"
UPDATE [Page]
SET [BreadCrumbDisplayName] = 1
WHERE [Guid] = '6163CCFD-CB15-452E-99F2-229A5E5B22F0'
   OR [Guid] = 'AE85B3FC-C951-497F-8C43-9D0A1E467A50'"
);
        }

        #endregion

        #region KH: Update Lava Template Attributes for Public Learning Course Blocks

        private void UpdatePublicLearningLavaAttributesUp()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
<style>
    
    .lms-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 1.5rem;
    }
    
    .card-img-h {
        height: 180px
    }
    
    .card {
        display: grid;
        grid-row: auto / span 5;
        grid-template-rows: subgrid;
    }
    
    .credits {
        flex-shrink: 0;
        white-space: nowrap;
    }

    @media (max-width: 767px) {
        .lms-grid {
           grid-template-columns: 1fr; 
        }
        h1 {
            font-size: 28px;
        }
    }
    
    @media (min-width: 768px) and (max-width: 1023px) {
        .lms-grid {
           grid-template-columns: 1fr 1fr; 
        }
    }
    
</style>


<div>
	
	<div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ ProgramInfo.ImageFileGuid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ ProgramInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ ProgramInfo.Summary }} </p>
        </div>
    </div>
    <div class=""center-block text-center mt-4 mb-4"">
        <div class=""d-flex flex-column gap-2"">
            <h3> Courses Available </h3>
            <p class=""text-muted""> The following training courses are available for enrollment. </p>
        </div>
    </div>
	
	<div> //- Main Body - Container for course grid

        <div class=""lms-grid""> //- Grid for Cards

            {% for course in Courses %}

            <div class=""card"">
                //- 1 IMAGE
                
                {% if course.ImageFileGuid %}
                    <img src=""/GetImage.ashx?guid={{ course.ImageFileGuid }}"" class=""card-img-top card-img-h object-cover"" alt=""course image"" />
                        
                    {% else %}
                        <div class=""d-flex justify-content-center align-items-center card-img-top card-img-h"">
                            <i class=""fa fa-image fa-2x text-gray-200""></i>
                        </div>
                        
                {% endif %}
                
                //- 2 CARD HEADER

                    <div class=""card-body d-flex justify-content-between align-items-start pt-0 pb-0"">
                        <h4 class=""card-title mb-0"">{{ course.PublicName }}</h4>
                        {% if course.Entity.Credits > 0 %}
        			        <div class=""d-flex w-auto"">
        			            <p class=""credits w-auto text-muted mb-0"">Credits: {{ course.Credits }}</p>
        			        </div>
    			        {% endif %}
                    </div>
                
                //- 3 BODY TEXT
                <div class=""card-body pt-0 pb-0"">
                    <p class=""line-clamp-3"">
                        {{ course.Summary }}    
                    </p>
                </div>
                
                //- 4 CATEGORY
			        <div class=""card-body pt-0 pb-0"">
			            <div class=""badge badge-default"">{{ course.Category }}</div>
            		</div>
                
                //- 5 FOOTER
                <div class=""card-footer bg-transparent d-flex justify-content-between"">
                    <a href=""{{ course.CourseDetailsLink }}"" class=""btn btn-default"">Learn More</a>
                    
                    {% if ShowCompletionStatus and course.IsEnrolled == true %}
                                            
                        {% if course.LearningCompletionStatus == 'Incomplete' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-warning"">Enrolled</span></h4>
                            </div>
                                
                            {% elseif course.LearningCompletionStatus == 'Fail' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-danger"">Failed</span></h4>
                            </div>
                            
                            {% elseif course.LearningCompletionStatus == 'Pass' %}
                                <div class=""d-flex align-items-center"">
                                    <h4 class=""m-0"">
                                        <span class=""label label-success"">
                                            {% if course.IsCompletionOnly == true %}
                                                {% if course.CompletionScaleName != empty %}
                                                    {{ course.CompletionScaleName }}
                                                    {% else %}
                                                    Completed
                                                {% endif %}
                                                {% else %}
                                                Passed
                                            {% endif %}
                                        </span>
                                    </h4>
                                </div>
                        {% endif %}
    
                    {% endif %}
                </div>

            </div>
            
            {% endfor %}
        
        </div>
    </div>
</div>
", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0", @"
<style>
    
    .lms-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 1.5rem;
    }
    
    .card-img-h {
        height: 180px
    }
    
    .card {
        display: grid;
        grid-row: auto / span 5;
        grid-template-rows: subgrid;
    }
    
    .credits {
        flex-shrink: 0;
        white-space: nowrap;
    }

    @media (max-width: 767px) {
        .lms-grid {
           grid-template-columns: 1fr; 
        }
        h1 {
            font-size: 28px;
        }
    }
    
    @media (min-width: 768px) and (max-width: 1023px) {
        .lms-grid {
           grid-template-columns: 1fr 1fr; 
        }
    }
    
</style>


<div>
	
	<div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ ProgramInfo.ImageFileGuid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ ProgramInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ ProgramInfo.Summary }} </p>
        </div>
    </div>
    <div class=""center-block text-center mt-4 mb-4"">
        <div class=""d-flex flex-column gap-2"">
            <h3> Courses Available </h3>
            <p class=""text-muted""> The following training courses are available for enrollment. </p>
        </div>
    </div>
	
	<div> //- Main Body - Container for course grid

        <div class=""lms-grid""> //- Grid for Cards

            {% for course in Courses %}

            <div class=""card"">
                //- 1 IMAGE
                
                {% if course.ImageFileGuid %}
                    <img src=""/GetImage.ashx?guid={{ course.ImageFileGuid }}"" class=""card-img-top card-img-h object-cover"" alt=""course image"" />
                        
                    {% else %}
                        <div class=""d-flex justify-content-center align-items-center card-img-top card-img-h"">
                            <i class=""fa fa-image fa-2x text-gray-200""></i>
                        </div>
                        
                {% endif %}
                
                //- 2 CARD HEADER

                    <div class=""card-body d-flex justify-content-between align-items-start pt-0 pb-0"">
                        <h4 class=""card-title mb-0"">{{ course.PublicName }}</h4>
                        {% if course.Entity.Credits > 0 %}
        			        <div class=""d-flex w-auto"">
        			            <p class=""credits w-auto text-muted mb-0"">Credits: {{ course.Credits }}</p>
        			        </div>
    			        {% endif %}
                    </div>
                
                //- 3 BODY TEXT
                <div class=""card-body pt-0 pb-0"">
                    <p class=""line-clamp-3"">
                        {{ course.Summary }}    
                    </p>
                </div>
                
                //- 4 CATEGORY
			        <div class=""card-body pt-0 pb-0"">
			            <div class=""badge badge-default"">{{ course.Category }}</div>
            		</div>
                
                //- 5 FOOTER
                <div class=""card-footer bg-transparent d-flex justify-content-between"">
                    <a href=""{{ course.CourseDetailsLink }}"" class=""btn btn-default"">Learn More</a>
                    
                    {% if ShowCompletionStatus and course.IsEnrolled == true %}
                                            
                        {% if course.LearningCompletionStatus == 'Incomplete' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-warning"">Enrolled</span></h4>
                            </div>
                                
                            {% elseif course.LearningCompletionStatus == 'Fail' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-danger"">Failed</span></h4>
                            </div>
                            
                            {% elseif course.LearningCompletionStatus == 'Pass' %}
                                <div class=""d-flex align-items-center"">
                                    <h4 class=""m-0"">
                                        <span class=""label label-success"">
                                            {% if course.IsCompletionOnly == true %}
                                                {% if course.CompletionScaleName != empty %}
                                                    {{ course.CompletionScaleName }}
                                                    {% else %}
                                                    Completed
                                                {% endif %}
                                                {% else %}
                                                Passed
                                            {% endif %}
                                        </span>
                                    </h4>
                                </div>
                        {% endif %}
    
                    {% endif %}
                </div>

            </div>
            
            {% endfor %}
        
        </div>
    </div>
</div>
" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "CourseDetailTemplate", "Lava Template", @"The Lava template to use to render the page. Merge fields include: Course, Program, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
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

                            {% if CourseInfo.Credits != empty and CourseInfo.Credits != 0 %}
                            <div class=""d-flex text-gray-600 gap-1 pb-3"">
                                <p class=""text-bold mb-0"">Credits: </p>
                                <p class=""mb-0"">{{CourseInfo.Credits}}</p>
                            </div>
                            {% endif %}

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
                
                {% assign today = 'Now' | Date:'yyyy-MM-dd' | AsDateTime %}
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

                                        {% if CourseInfo.IsCompletionOnly == false %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}

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

                                        {% if classInfo.StudentParticipant.LearningGradingSystemScale.Name != empty %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}
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
                                                    {% if classInfo.Location and classInfo.Location != '' %}
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

            // Delete any existing attribute value for the Lava Template attribute of this block type.
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            Sql( "DELETE [AttributeValue] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'DA6C3170-5264-427D-AC22-8D50D2F6D2F6' )" );

            Sql( @"DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();
DECLARE @completeGradingSystemScaleGuid NVARCHAR(40) = '9D17F412-25A8-4638-9CD7-5343017620D6';

UPDATE [dbo].[LearningGradingSystemScale]
SET 
      [Name] = 'Completed'
    , [ModifiedDateTime] = @now
WHERE 
    [Guid] = @completeGradingSystemScaleGuid;" );
        }

        private void UpdatePublicLearningLavaAttributesDown()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
<style>
    
    .lms-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 1.5rem;
    }
    
    .card-img-h {
        height: 180px
    }
    
    .card {
        display: grid;
        grid-row: auto / span 5;
        grid-template-rows: subgrid;
    }
    
    .credits {
        flex-shrink: 0;
        white-space: nowrap;
    }

    @media (max-width: 767px) {
        .lms-grid {
           grid-template-columns: 1fr; 
        }
        h1 {
            font-size: 28px;
        }
    }
    
    @media (min-width: 768px) and (max-width: 1023px) {
        .lms-grid {
           grid-template-columns: 1fr 1fr; 
        }
    }
    
</style>


<div>
	
	<div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ ProgramInfo.ImageFileGuid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ ProgramInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ ProgramInfo.Summary }} </p>
        </div>
    </div>
    <div class=""center-block text-center mt-4 mb-4"">
        <div class=""d-flex flex-column gap-2"">
            <h3> Courses Available </h3>
            <p class=""text-muted""> The following training courses are available for enrollment. </p>
        </div>
    </div>
	
	<div> //- Main Body - Container for course grid

        <div class=""lms-grid""> //- Grid for Cards

            {% for course in Courses %}

            <div class=""card"">
                //- 1 IMAGE
                
                {% if course.ImageFileGuid %}
                    <img src=""/GetImage.ashx?guid={{ course.ImageFileGuid }}"" class=""card-img-top card-img-h object-cover"" alt=""course image"" />
                        
                    {% else %}
                        <div class=""d-flex justify-content-center align-items-center card-img-top card-img-h"">
                            <i class=""fa fa-image fa-2x text-gray-200""></i>
                        </div>
                        
                {% endif %}
                
                //- 2 CARD HEADER

                    <div class=""card-body d-flex justify-content-between align-items-start pt-0 pb-0"">
                        <h4 class=""card-title mb-0"">{{ course.PublicName }}</h4>
                        {% if course.Entity.Credits > 0 %}
        			        <div class=""d-flex w-auto"">
        			            <p class=""credits w-auto text-muted mb-0"">Credits: {{ course.Credits }}</p>
        			        </div>
    			        {% endif %}
                    </div>
                
                //- 3 BODY TEXT
                <div class=""card-body pt-0 pb-0"">
                    <p class=""line-clamp-3"">
                        {{ course.Summary }}    
                    </p>
                </div>
                
                //- 4 CATEGORY
			        <div class=""card-body pt-0 pb-0"">
			            <div class=""badge badge-default"">{{ course.Category }}</div>
            		</div>
                
                //- 5 FOOTER
                <div class=""card-footer bg-transparent d-flex justify-content-between"">
                    <a href=""{{ course.CourseDetailsLink }}"" class=""btn btn-default"">Learn More</a>
                    
                    {% if ShowCompletionStatus and course.IsEnrolled == true %}
                                            
                        {% if course.LearningCompletionStatus == 'Incomplete' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-warning"">Enrolled</span></h4>
                            </div>
                                
                            {% elseif course.LearningCompletionStatus == 'Fail' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-danger"">Failed</span></h4>
                            </div>
                            
                            {% elseif course.LearningCompletionStatus == 'Pass' %}
                                <div class=""d-flex align-items-center"">
                                    <h4 class=""m-0""><span class=""label label-success"">Passed</span></h4>
                                </div>
                        {% endif %}
    
                    {% endif %}
                </div>

            </div>
            
            {% endfor %}
        
        </div>
    </div>
</div>
", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0", @"
<style>
    
    .lms-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 1.5rem;
    }
    
    .card-img-h {
        height: 180px
    }
    
    .card {
        display: grid;
        grid-row: auto / span 5;
        grid-template-rows: subgrid;
    }
    
    .credits {
        flex-shrink: 0;
        white-space: nowrap;
    }

    @media (max-width: 767px) {
        .lms-grid {
           grid-template-columns: 1fr; 
        }
        h1 {
            font-size: 28px;
        }
    }
    
    @media (min-width: 768px) and (max-width: 1023px) {
        .lms-grid {
           grid-template-columns: 1fr 1fr; 
        }
    }
    
</style>


<div>
	
	<div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ ProgramInfo.ImageFileGuid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ ProgramInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ ProgramInfo.Summary }} </p>
        </div>
    </div>
    <div class=""center-block text-center mt-4 mb-4"">
        <div class=""d-flex flex-column gap-2"">
            <h3> Courses Available </h3>
            <p class=""text-muted""> The following training courses are available for enrollment. </p>
        </div>
    </div>
	
	<div> //- Main Body - Container for course grid

        <div class=""lms-grid""> //- Grid for Cards

            {% for course in Courses %}

            <div class=""card"">
                //- 1 IMAGE
                
                {% if course.ImageFileGuid %}
                    <img src=""/GetImage.ashx?guid={{ course.ImageFileGuid }}"" class=""card-img-top card-img-h object-cover"" alt=""course image"" />
                        
                    {% else %}
                        <div class=""d-flex justify-content-center align-items-center card-img-top card-img-h"">
                            <i class=""fa fa-image fa-2x text-gray-200""></i>
                        </div>
                        
                {% endif %}
                
                //- 2 CARD HEADER

                    <div class=""card-body d-flex justify-content-between align-items-start pt-0 pb-0"">
                        <h4 class=""card-title mb-0"">{{ course.PublicName }}</h4>
                        {% if course.Entity.Credits > 0 %}
        			        <div class=""d-flex w-auto"">
        			            <p class=""credits w-auto text-muted mb-0"">Credits: {{ course.Credits }}</p>
        			        </div>
    			        {% endif %}
                    </div>
                
                //- 3 BODY TEXT
                <div class=""card-body pt-0 pb-0"">
                    <p class=""line-clamp-3"">
                        {{ course.Summary }}    
                    </p>
                </div>
                
                //- 4 CATEGORY
			        <div class=""card-body pt-0 pb-0"">
			            <div class=""badge badge-default"">{{ course.Category }}</div>
            		</div>
                
                //- 5 FOOTER
                <div class=""card-footer bg-transparent d-flex justify-content-between"">
                    <a href=""{{ course.CourseDetailsLink }}"" class=""btn btn-default"">Learn More</a>
                    
                    {% if ShowCompletionStatus and course.IsEnrolled == true %}
                                            
                        {% if course.LearningCompletionStatus == 'Incomplete' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-warning"">Enrolled</span></h4>
                            </div>
                                
                            {% elseif course.LearningCompletionStatus == 'Fail' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-danger"">Failed</span></h4>
                            </div>
                            
                            {% elseif course.LearningCompletionStatus == 'Pass' %}
                                <div class=""d-flex align-items-center"">
                                    <h4 class=""m-0""><span class=""label label-success"">Passed</span></h4>
                                </div>
                        {% endif %}
    
                    {% endif %}
                </div>

            </div>
            
            {% endfor %}
        
        </div>
    </div>
</div>
" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "CourseDetailTemplate", "Lava Template", @"The Lava template to use to render the page. Merge fields include: Course, Program, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
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

                            {% if CourseInfo.Credits != empty and CourseInfo.Credits != 0 %}
                            <div class=""d-flex text-gray-600 gap-1 pb-3"">
                                <p class=""text-bold mb-0"">Credits: </p>
                                <p class=""mb-0"">{{CourseInfo.Credits}}</p>
                            </div>
                            {% endif %}

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
                
                {% assign today = 'Now' | Date:'yyyy-MM-dd' | AsDateTime %}
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

                                        {% if classInfo.StudentParticipant.LearningGradingSystemScale.Name != empty %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}

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

                                        {% if classInfo.StudentParticipant.LearningGradingSystemScale.Name != empty %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}
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
                                                    {% if classInfo.Location and classInfo.Location != '' %}
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
        }

        #endregion
    }
}
