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
    public partial class DiscTweaks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"UPDATE [BlockType] set [Path] = '~/Blocks/Crm/Disc.ascx' where [Guid] = 'A161D12D-FEA7-422F-B00E-A689629680E4'" );

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Custom Message", "CustomMessage", "A custom message you would like to include in the request.  Otherwise the default will be used.", 1, @"We're each a unique creation. We'd love to learn more about you through a simple and quick online personality profile. The results of the assessment will help us tailor our ministry to you and can also be used for building healthier teams and groups.

The assessment takes less than ten minutes and will go a long way toward helping us get to know you better. Thanks in advance!", "840E6A84-9F83-4482-92D1-6F635F062251" ); // DISC Request:Custom Message

            Sql( @"UPDATE [BlockType] set [Path] = '~/Blocks/Crm/Disc.ascx' where [Guid] = 'A161D12D-FEA7-422F-B00E-A689629680E4'" );

            RockMigrationHelper.UpdateWorkflowActionForm( @"Hi &#123;&#123; Person.NickName &#125;&#125;!

{{ Workflow.WarningMessage }}", @"", "Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|", "", true, "", "4AFAB342-D584-4B79-B038-A99C0C469D74" ); // DISC Request:Launch From Person Profile:Custom Message

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailStyles }}
{{ GlobalAttribute.EmailHeader }}
<p>Hi {{ Person.NickName }}!</p>

<p>{{ Workflow.CustomMessage | NewlineToBr }}</p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}DISC/{{ Person.UrlEncodedKey }}"">Take Personality Assessment</a></p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}"">I&#39;m no longer involved with {{ GlobalAttribute.OrganizationName }}. Please remove me from all future communications.</a></p>

<p>- {{ Workflow.Sender }}</p>

{{ GlobalAttribute.EmailFooter }}" ); // DISC Request:Launch From Person Profile:Send Email Action:Body
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
