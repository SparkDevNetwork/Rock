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
    public partial class ContentChannelTypeDisableStatusOption : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ContentChannelType", "DisableStatus", c => c.Boolean(nullable: false));

            // rollup: MP - Fixup just in case old version of 201611171650206_InteractionModels.cs was run
            this.Sql( @"
UPDATE dv
SET [Guid] = 'E503E77D-CF35-E09F-41A2-B213184F48E8'
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
    AND dt.[Guid] = '9bf5777a-961f-49a8-a834-45e5c2077967'
    AND dv.Value = 'Website'
	and dv.[Guid] != 'E503E77D-CF35-E09F-41A2-B213184F48E8'

UPDATE dv
SET [Guid] = '55004F5C-A8ED-7CB7-47EE-5988E9F8E0A8'
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
    AND dt.[Guid] = '9bf5777a-961f-49a8-a834-45e5c2077967'
    AND dv.Value = 'Communication'
	and dv.[Guid] != '55004F5C-A8ED-7CB7-47EE-5988E9F8E0A8'

UPDATE dv
SET [Guid] = 'F1A19D09-E010-EEB3-465A-940A6F023CEB'
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
    AND dt.[Guid] = '9bf5777a-961f-49a8-a834-45e5c2077967'
    AND dv.Value = 'Content Channel'
	and dv.[Guid] != 'F1A19D09-E010-EEB3-465A-940A6F023CEB'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.ContentChannelType", "DisableStatus");
        }
    }
}
