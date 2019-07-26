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
    public partial class Rollup_0624 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AssessmentBadgeSummaryText();
            AssessmentWorkFlowDoNotAutoPersist();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// ED: Assessment Badge lava
        /// </summary>
        private void AssessmentBadgeSummaryText()
        {
            Sql( @"
                UPDATE [dbo].[AssessmentType]
                SET [BadgeSummaryLava] = '{{ Person | Attribute:''core_DISCDISCProfile'' }}'
                WHERE [Title] = 'DISC'

                UPDATE [dbo].[AssessmentType]
                SET [BadgeSummaryLava] = '{% assign gifts = Person | Attribute:''core_DominantGifts'' %}
                {% if gifts contains '','' %}
                  {% assign gifts = Person | Attribute:''core_DominantGifts'',''Object'' %}
                  {{ gifts | Map:''Value'' | Join:'', '' | ReplaceLast:'','','' and'' }}
                {% else %}
                  {{ gifts }}
                {% endif %}'
                WHERE [Title] = 'Spiritual Gifts'

                UPDATE [dbo].[AssessmentType]
                SET [BadgeSummaryLava] = '{{ Person | Attribute:''core_EQSelfAwareness'' | StripHtml }}'
                WHERE [Title] = 'Emotional Intelligence'

                UPDATE [dbo].[AssessmentType]
                SET [BadgeSummaryLava] = '{% assign gifts = Person | Attribute:''core_MotivatorsTop5Motivators'',''Object'' %}
                {{ gifts | Map:''Value'' | Join:'', '' | ReplaceLast:'','','' and'' }}'
                WHERE [Title] = 'Motivators'

                UPDATE [dbo].[AssessmentType]
                SET [BadgeSummaryLava] = '{{ ConflictTheme }}'
                WHERE [Title] = 'Conflict Profile'" );
        }

        /// <summary>
        /// ED: "Request Assessment" workflow persists immediately, but should not
        /// </summary>
        private void AssessmentWorkFlowDoNotAutoPersist()
        {
            // Request Assessment
            RockMigrationHelper.UpdateWorkflowType(false,true,"Request Assessment","","BBAE05FD-8192-4616-A71E-903A927E0D90","Work","icon-fw fa fa-bar-chart",0,false,0,"31DDC001-C91A-4418-B375-CAB1475F7A62",0);

            // Request Assessment:Launch From Person Profile:User Entry
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","User Entry",4,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"A56DA6B0-60A1-4998-B3F0-6BFA6F342167","",1,"","E9C27804-A31D-4121-A963-9F52BEAE7404");

            // Request Assessment:Launch From Person Profile:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","Persist Workflow",5,"F1A39347-6FE0-43D4-89FB-544195088ECF",true,true,"","",1,"","64D4CCB4-AE71-451F-8EE3-3FD0E13EBD35");

            // Request Assessment:Launch From Person Profile:Persist Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue("64D4CCB4-AE71-451F-8EE3-3FD0E13EBD35","E22BE348-18B1-4420-83A8-6319B35416D2",@"True");
        }
    }
}
