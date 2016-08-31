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
    public partial class CampusFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Campus", "Description", c => c.String());
            AddColumn("dbo.Campus", "IsActive", c => c.Boolean());
            AddColumn("dbo.Campus", "Url", c => c.String());
            AddColumn("dbo.Location", "ImageId", c => c.Int());
            AddColumn("dbo.Category", "HilightColor", c => c.String(maxLength: 50));
            AlterColumn("dbo.Category", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.ServiceLog", "Result", c => c.String(maxLength: 200));
            CreateIndex("dbo.Location", "ImageId");
            AddForeignKey("dbo.Location", "ImageId", "dbo.BinaryFile", "Id");

            Sql( @"
    UPDATE [DefinedValue] SET [IsSystem] = 1 WHERE [Guid] IN ( 'C0D7AE35-7901-4396-870E-3AAF472AAE88', 'D9646A93-1667-4A44-82DA-12E1229B4695', '107C6DA1-266D-4E1C-A443-1CD37064601D' )

    UPDATE [Campus] SET [IsActive] = 1

    UPDATE C SET [LocationId] = NL.[Id]
    FROM [Campus] C
    LEFT OUTER JOIN [Location] CL ON CL.[Id] = C.[LocationId] 
    LEFT OUTER JOIN [Location] NL ON NL.[Name] = C.[Name] AND NL.[ParentLocationId] IS NULL
    WHERE CL.[Id] IS NULL
    AND NL.[Id] IS NOT NULL

    DECLARE @CampusLocationTypeId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'C0D7AE35-7901-4396-870E-3AAF472AAE88' )

    UPDATE L SET [LocationTypeValueId] = @CampusLocationTypeId
    FROM [Location] L
    INNER JOIN [Campus] C ON C.[LocationId] = L.[Id]
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Location", "ImageId", "dbo.BinaryFile");
            DropIndex("dbo.Location", new[] { "ImageId" });
            AlterColumn("dbo.ServiceLog", "Result", c => c.String(maxLength: 50));
            AlterColumn("dbo.Category", "IconCssClass", c => c.String());
            DropColumn("dbo.Category", "HilightColor");
            DropColumn("dbo.Location", "ImageId");
            DropColumn("dbo.Campus", "Url");
            DropColumn("dbo.Campus", "IsActive");
            DropColumn("dbo.Campus", "Description");
        }
    }
}
