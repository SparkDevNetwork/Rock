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
    public partial class LengthenActivityDetail : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.CommunicationRecipientActivity", "ActivityDetail", c => c.String(maxLength: 2200));

            Sql( @"  UPDATE [CommunicationTemplate]
  SET [ChannelDataJson] = '{ ""HtmlMessage"": ""{{ GlobalAttribute.EmailHeader }}\n<p>{{ Person.NickName }},</p>\n\n<p>--&gt; Insert Your Communication Text Here &lt;--</p>\n\n<p>{{ Communication.ChannelData.FromName }}<br />\nEmail: <a href=\""mailto:{{ Communication.ChannelData.FromAddress }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ Communication.ChannelData.FromAddress }}</a></p>\n{{ GlobalAttribute.EmailFooter }}"" }'
  WHERE [Guid] = 'AFE2ADD1-5278-441E-8E84-1DC743D99824'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.CommunicationRecipientActivity", "ActivityDetail", c => c.String(maxLength: 200));
        }
    }
}
