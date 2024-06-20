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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
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

    [LinkedPage( "Detail Page",
        Description = "The page that will show the courses for the program.",
        Key = AttributeKey.DetailPage )]

    [CodeEditorField( "Program List Template",
        Key = AttributeKey.ProgramListTemplate,
        Description = "The lava template showing the program list. Merge fields include: Programs, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.ProgramListTemplate,
        Order = 1 )]
    [Rock.SystemGuid.EntityTypeGuid( "59d82730-e4a7-4aaf-bb1e-bec4b7aa8624" )]
    [Rock.SystemGuid.BlockTypeGuid( "2fc656da-7f5d-41b3-ad18-bfe692cfca57" )]
    public class PublicLearningProgramList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ProgramListTemplate = "ProgramListTemplate";
        }

        private static class AttributeDefault
        {
            public const string ProgramListTemplate = @"
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
				
				{% if program.CompletionStatus == 'Completed' %}
					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
				{% elseif program.CompletionStatus == 'Pending' %}
					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>";
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
            var programs = new LearningProgramService( new RockContext() ).GetPublicPrograms( GetCurrentPerson().Id );

            var courseDetailUrlTemplate = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LearningProgramId", "((Key))" );

            foreach ( var program in programs )
            {
                program.CoursesLink = courseDetailUrlTemplate.Replace( "((Key))", program.Entity.IdKey );
            }

            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Programs", programs );

            var template = GetAttributeValue( AttributeKey.ProgramListTemplate ) ?? string.Empty;
            box.ProgramsHtml = template.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}
