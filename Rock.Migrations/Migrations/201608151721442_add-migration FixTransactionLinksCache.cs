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
    public partial class addmigrationFixTransactionLinksCache : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"DECLARE @BlockId int;
SELECT TOP 1  @BlockId = [Id]
  FROM [dbo].[Block]
  WHERE [Name] = 'Transaction Links';
DECLARE @BlockTypeId int;
SELECT TOP 1 @BlockTypeId = [Id]
  FROM [dbo].[BlockType]
  WHERE [Path] = '~/Blocks/Cms/HtmlContentDetail.ascx';
DECLARE @AttributeId int;
SELECT TOP 1 @AttributeId = [Id]
FROM [rock-develop].[dbo].[Attribute]
WHERE EntityTypeQualifierColumn = 'BlockTypeId' and EntityTypeQualifierValue = @BlockTypeId and [Key] = 'CacheDuration';
UPDATE[dbo].[AttributeValue]
  SET [Value] = 0
  WHERE AttributeId = @AttributeId and EntityId = @BlockId;" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
