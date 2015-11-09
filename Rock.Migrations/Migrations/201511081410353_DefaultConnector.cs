// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class DefaultConnector : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ConnectionOpportunityCampus", "DefaultConnectorPersonAliasId", c => c.Int());
            CreateIndex("dbo.ConnectionOpportunityCampus", "DefaultConnectorPersonAliasId");
            AddForeignKey("dbo.ConnectionOpportunityCampus", "DefaultConnectorPersonAliasId", "dbo.PersonAlias", "Id");

            Sql( @"
    UPDATE [MergeTemplate] SET 
         [Description] = 'Standard Address Labels (Avery 5150, 5260) with a person''s home address.'
        ,[Name] = 'Mailing Labels - Home Address (Avery 5150, 5260)'
    WHERE [Guid] = '7730FDA8-3A1F-79AF-4F79-1F7AEE5BCB9C'

    UPDATE [MergeTemplate] SET 
         [Description] = '#10 Envelope with a person''s home address.'
        ,[Name] = 'Envelope - Home Address (#10)'
    WHERE [Guid] = '111E35D5-57B9-44A9-4934-B234CF9AFAF1'

    UPDATE [Page] SET 
        [DisplayInNavWhen] = 0
    WHERE [Guid] = 'B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ConnectionOpportunityCampus", "DefaultConnectorPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "DefaultConnectorPersonAliasId" });
            DropColumn("dbo.ConnectionOpportunityCampus", "DefaultConnectorPersonAliasId");
        }
    }
}
