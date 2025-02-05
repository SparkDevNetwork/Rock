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
    public partial class UpdateLMSPageNavBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Update certain LMS Page Nav blocks to use a custom PageListAsTabs.lava include.
        /// </summary>
        public override void Up()
        {
            var programCoursesPageMenuBlockGuid = "B3C55400-76E9-42D9-9ECA-842FBFC7C123";
            var coursePageMenuTemplate = @"{% comment %}
   Only show the Page Menu when the program is not an On-Demand ConfigurationMode.
{% endcomment %}

{% assign programId = PageParameter[""LearningProgramId""] | FromIdHash %}

{% if programId > 0 %}
    {% sql %}
        SELECT 1
        FROM [LearningProgram] 
        WHERE [Id] = '{{ programId }}'
            AND [ConfigurationMode] <> 1 -- On-Demand
    {% endsql %}
    
    {% for item in results %}
        {% include '~~/Assets/Lava/LMSPageListAsTabs.lava' %}
    {% endfor %}
{% endif %}
";

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: [see coursePageMenuTemplate variable] */
            RockMigrationHelper.AddBlockAttributeValue( programCoursesPageMenuBlockGuid, "1322186A-862A-4CF1-B349-28ECB67229BA", coursePageMenuTemplate );

            // Next, update attribute values for three (3) additional LMS blocks that also use the PageNav block

            var blockValuePageMenuLavaTemplate = @"{% include '~~/Assets/Lava/LMSPageListAsTabs.lava' %}";

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: [see blockValuePageMenuLavaTemplate variable] */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "1322186A-862A-4CF1-B349-28ECB67229BA", blockValuePageMenuLavaTemplate );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: [see blockValuePageMenuLavaTemplate variable] */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "1322186A-862A-4CF1-B349-28ECB67229BA", blockValuePageMenuLavaTemplate );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: [see blockValuePageMenuLavaTemplate variable] */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "1322186A-862A-4CF1-B349-28ECB67229BA", blockValuePageMenuLavaTemplate );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
