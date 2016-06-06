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
    public partial class WorkflowFormFieldText : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowActionFormAttribute", "HideLabel", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowActionFormAttribute", "PreHtml", c => c.String());
            AddColumn("dbo.WorkflowActionFormAttribute", "PostHtml", c => c.String());

            Sql( @"
    -- Update icon for bulk update page
    UPDATE [Page] SET [IconCssClass] = 'fa fa-truck'
    WHERE [Guid] = 'B6BFDE54-0EFA-4499-847D-BE1259F83535'

    -- Fix the qualifier for country attribute on state defined type
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3B234A62-B87D-47CD-A33F-32CC6C840A02' )
    DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'D7979EA1-44E9-46E2-BF37-DDAF7F741378' )
    UPDATE [AttributeQualifier] SET [Value] = CAST ( @DefinedTypeId AS varchar )
    WHERE [AttributeId] = @AttributeId
    AND [Key] = 'definedtype'
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.WorkflowActionFormAttribute", "PostHtml");
            DropColumn("dbo.WorkflowActionFormAttribute", "PreHtml");
            DropColumn("dbo.WorkflowActionFormAttribute", "HideLabel");
        }
    }
}
