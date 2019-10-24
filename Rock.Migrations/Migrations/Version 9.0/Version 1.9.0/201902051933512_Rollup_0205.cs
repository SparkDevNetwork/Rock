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
    public partial class Rollup_0205 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateDisc();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// CodeGenMigration Up
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: DISC:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute( "A161D12D-FEA7-422F-B00E-A689629680E4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "", @"The number of questions to show per page while taking the test", 6, @"1", "CC012E4A-EA89-4733-A1DC-76E2342A30FE" );
            // Attrib for BlockType: Stark Detail:Show Email Address
            RockMigrationHelper.UpdateBlockTypeAttribute( "D6B14847-B652-49E2-9D4B-658D502F0AEC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Address", "ShowEmailAddress", "", @"Should the email address be shown?", 1, @"True", "BBB51D1D-FB57-4A1F-8C02-E8521ACA8A88" );
            // Attrib for BlockType: Transaction Matching:Expand Person Search Options
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Expand Person Search Options", "ExpandPersonSearchOptions", "", @"When selecting a person, expand the additional search options by default.", 5, @"True", "26F1FDAD-9F84-4A52-952E-A80B383CFDC0" );

        }

        /// <summary>
        /// CodeGenMigration Down
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Stark Detail:Show Email Address
            RockMigrationHelper.DeleteAttribute( "BBB51D1D-FB57-4A1F-8C02-E8521ACA8A88" );
            // Attrib for BlockType: Transaction Matching:Expand Person Search Options
            RockMigrationHelper.DeleteAttribute( "26F1FDAD-9F84-4A52-952E-A80B383CFDC0" );
            // Attrib for BlockType: DISC:Number of Questions
            RockMigrationHelper.DeleteAttribute( "CC012E4A-EA89-4733-A1DC-76E2342A30FE" );
        }

        /// <summary>Updates the workflow action.
        /// SK: Disc Updates
        /// </summary>
        private void UpdateDisc()
        {
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "840E6A84-9F83-4482-92D1-6F635F062251", 1, true, false, false, true, @"", @"<br/>", "94B20E87-9C79-4C37-926C-0426496AC722" );
            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }} <p>Hi {{ Person.NickName }}!</p>  <p>{{ Workflow | Attribute:'CustomMessage' | NewlineToBr }}</p>  <blockquote>     <p><a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}DISC/{{ Person.UrlEncodedKey }}"">Take Personality Assessment</a></p>     <p><i>- or -</i></p>     <p><a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ p.UrlEncodedKey }}"">I&#39;m no longer involved with      {{ 'Global' | Attribute :'OrganizationName' }}. Please remove me from all future communications.</a></p> </blockquote>   <p>- {{ Workflow | Attribute:'Sender' }}</p>  {{ 'Global' | Attribute:'EmailFooter' }}" );
        }

       
    }
}
