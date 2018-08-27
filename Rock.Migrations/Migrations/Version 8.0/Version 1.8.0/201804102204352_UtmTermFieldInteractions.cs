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
    public partial class UtmTermFieldInteractions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Interaction", "Term", c => c.String(maxLength: 50));

            // PageView Interaction Session Lava Update
            Sql( @"DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'DF74EDE5-3B7D-4A79-B1F2-499D18FE6F2C')

UPDATE [AttributeValue]
	SET [Value] = REPLACE([Value], '<a href = ''{{ interaction.InteractionData }}''>{{ interaction.InteractionComponent.Name }}', '<a href=''{{ interaction.InteractionData }}''>{{ interaction.InteractionSummary }}')
	WHERE [AttributeId] = @AttributeId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Interaction", "Term");
        }
    }
}
