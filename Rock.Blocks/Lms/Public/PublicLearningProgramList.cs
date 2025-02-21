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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Lms.PublicLearningProgramList;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of public learning programs.
    /// </summary>
    [DisplayName( "Public Learning Program List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of public learning programs." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [TextField( "Page Title",
        Description = "Provide a clear, welcoming title for the Learning Hub homepage. Example: 'Grow Together in Faith.'",
        IsRequired = true,
        Category = "",
        Order = 1,
        Key = AttributeKey.PageTitle,
        DefaultValue = AttributeDefault.PageTitle )]

    [TextField( "Page Description",
        Description = "Enter a brief description for the homepage to introduce users to the LMS. Example: 'Explore resources to deepen your faith and connect with our community.'",
        IsRequired = true,
        Category = "",
        Order = 2,
        Key = AttributeKey.PageDescription,
        DefaultValue = AttributeDefault.PageDescription )]

    [ImageField(
        "Banner Image",
        Description = "Add a welcoming banner image to visually enhance the homepage. Ideal size: 1200x400 pixels; use high-quality images.",
        Key = AttributeKey.BannerImage,
        IsRequired = false,
        DefaultValue = AttributeDefault.BannerImage )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The lava template to use to render the page. Merge fields include: Programs (a list of ProgramInfos), ShowCompletionStatus, BannerImageGuid, PageTitle, PageDescription, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = AttributeDefault.ProgramListTemplate,
        Order = 4 )]

    [LinkedPage( "Detail Page",
        Description = "Page to link to when the individual would like more details.",
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    [CategoryField( "Program Categories",
        Description = "The categories to filter the programs by.",
        EntityTypeName = "Rock.Model.LearningProgram",
        AllowMultiple = true,
        IsRequired = false,
        Key = AttributeKey.ProgramCategories,
        Order = 3 )]

    [CustomDropdownListField(
        "Show Completion Status",
        Key = AttributeKey.ShowCompletionStatus,
        Description = "Determines if the individual's completion status should be shown.",
        ListSource = "Show,Hide",
        IsRequired = true,
        DefaultValue = "Show",
        Order = 4 )]

    [BooleanField(
        "Public Only",
        Description = "If selected, all non-public programs will be excluded.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.PublicOnly,
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( "59d82730-e4a7-4aaf-bb1e-bec4b7aa8624" )]
    [Rock.SystemGuid.BlockTypeGuid( "DA1460D8-E895-4B23-8A8E-10EBBED3990F" )]
    public class PublicLearningProgramList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string BannerImage = "BannerImage";
            public const string DetailPage = "DetailPage";
            public const string LavaTemplate = "LavaTemplate";
            public const string PageDescription = "PageDescription";
            public const string PageTitle = "PageTitle";
            public const string ProgramCategories = "ProgramCategories";
            public const string PublicOnly = "PublicOnly";
            public const string ShowCompletionStatus = "ShowCompletionStatus";
        }

        private static class AttributeDefault
        {
            public const string BannerImage = "605FD4B7-2DCA-4782-8826-95AAC6C6BAB6";
            public const string PageDescription = "Explore courses and trainings designed to deepen your faith, help you grow in spiritual knowledge, and prepare you for serving and volunteering.";
            public const string PageTitle = "Learning Hub";
            public const string ProgramListTemplate = @"
<style>

    .lms-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 1.5rem;
    }

    .card {
        display: grid;
        grid-row: auto / span 5;
        grid-template-rows: subgrid;
    }
    
    .card-img-h {
        height: 180px;
    }
    

    @media (max-width: 767px) {
        .lms-grid {
            grid-template-columns: 1fr;
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
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ BannerImageGuid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ PageTitle }} </h1>
            <p class=""hero-section-description""> {{ PageDescription }} </p>
        </div>
    </div>
    <div class=""center-block text-center mt-4 mb-4"">
        <div class=""d-flex flex-column gap-2"">
            <h3> Programs Available </h3>
            <p class=""text-muted""> The following types of classes are available. </p>
        </div>
    </div>
    <div> //- MAIN BODY - Container for course grid
        
        <div class=""lms-grid""> //- Grid for Cards

            {% for program in Programs %}
            
            <div class=""card""> 
                
                //- 1 IMAGE
                {% if program.ImageFileGuid %}
                
                    <img src=""/GetImage.ashx?guid={{ program.ImageFileGuid }}"" class=""card-img-top card-img-h object-cover""
                    alt=""program image"" /> 
                    
                    {% else %} 
                        <div class=""d-flex justify-content-center align-items-center card-img-h card-img-top""> 
                        <i class=""fa fa-image fa-2x o-30""></i> </div>
                {% endif %}
                
                //- 2 TITLE
                <div class=""card-body pb-0 pt-0"">
                    <h4 class=""card-title mb-0"">{{ program.PublicName }}</h4>
                </div>
                
                //- 3 BODY TEXT
                <div class=""card-body pt-0 pb-0"">
                    <p class=""line-clamp-3"">{{ program.Summary }}</p>
                </div>
                
                //- 4 CATEGORY
                {% if program.Category %}
                
                    <div class=""card-body pt-0 pb-0"">
                        <div class=""badge badge-default"">
                            {{ program.Category }}
                        </div>
                    </div>

                    {% else %}
                    <div class=""card-body pt-0 pb-0"">
                        <div> <span> </span> </div>
                    </div>
                
                {% endif %}
                
                //- 5 FOOTER
                <div class=""card-footer d-flex justify-content-between"">
                    <a href=""{{ program.CoursesLink }}"" class=""btn btn-default"">Learn More</a>
                    
                    {% if ShowCompletionStatus %}
                    
                        {% if program.CompletionStatus == 'Completed' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-success"">Completed</span></h4>
                            </div>
                            
                        {% elseif program.CompletionStatus == 'Pending' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-warning"">Enrolled</span></h4>
                            </div>
                        
                        {% endif %}
                    {% endif %}
                </div>
            </div>
            {% endfor %}
        </div>
    </div>
</div>
";
        }

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PublicLearningProgramListBlockBox();

            SetBoxInitialEntityState( box );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( PublicLearningProgramListBlockBox box )
        {
            box.ProgramsHtml = GetInitialHtmlContent();
        }

        /// <summary>
        /// Provide HTML to the block for it's initial rendering.
        /// </summary>
        /// <returns>The HTML content to initially render.</returns>
        protected override string GetInitialHtmlContent()
        {
            var programs = GetPrograms();

            var courseDetailUrlTemplate = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LearningProgramId", "((Key))" );

            foreach ( var program in programs )
            {
                program.CoursesLink = courseDetailUrlTemplate.Replace( "((Key))", program.IdKey );
            }

            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Programs", programs );
            mergeFields.Add( "ShowCompletionStatus", ShowCompletionStatus() );
            mergeFields.Add( "BannerImageGuid", GetAttributeValue( AttributeKey.BannerImage ) );
            mergeFields.Add( "PageDescription", GetAttributeValue( AttributeKey.PageDescription ) );
            mergeFields.Add( "PageTitle", GetAttributeValue( AttributeKey.PageTitle ) );

            var template = GetAttributeValue( AttributeKey.LavaTemplate ) ?? string.Empty;
            return template.ResolveMergeFields( mergeFields );
        }

        private bool ShowCompletionStatus()
        {
            return GetAttributeValue( AttributeKey.ShowCompletionStatus ) == "Show";
        }

        private List<Rock.Model.LearningProgramService.PublicLearningProgramBag> GetPrograms()
        {
            var categoryGuids = GetAttributeValue( AttributeKey.ProgramCategories ).SplitDelimitedValues().AsGuidList().ToArray();
            var publicOnly = GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean();
            var pageLevelCategoryFilter = PageParameter( PageParameterKey.CategoryId );
            if ( pageLevelCategoryFilter.Any() )
            {
                var onlyCategoryGuid = new CategoryService( RockContext ).GetSelect( pageLevelCategoryFilter, c => c.Guid );

                if ( categoryGuids.Contains( onlyCategoryGuid ) )
                {
                    return new LearningProgramService( RockContext ).GetPublicPrograms( GetCurrentPerson()?.Id ?? 0, publicOnly, onlyCategoryGuid ).ToList();
                }
            }

            return new LearningProgramService( RockContext ).GetPublicPrograms( GetCurrentPerson()?.Id ?? 0, publicOnly, categoryGuids ).ToList();
        }

        #endregion
    }
}
