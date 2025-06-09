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
    /// <summary>
    ///
    /// </summary>
    public partial class AddRestViewSupport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create all the columns as nullable so existing data works.
            AddColumn( "dbo.Page", "SiteId", c => c.Int( nullable: true ) );
            AddColumn( "dbo.Registration", "RegistrationTemplateId", c => c.Int( nullable: true ) );
            AddColumn( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId", c => c.Int( nullable: true ) );

            //AddColumn("dbo.Attribute", "DefaultValueChecksum", c => c.Int(nullable: false));
            Sql( "ALTER TABLE [dbo].[Attribute] ADD [DefaultValueChecksum] AS (checksum([DefaultValue])) PERSISTED" );

            Sql( @"
CREATE NONCLUSTERED INDEX [IX_DefaultValueChecksum] ON [dbo].[Attribute]
(
	[DefaultValueChecksum] ASC
)" );

            // Set current values of all existing rows.
            Sql( @"
UPDATE [P] SET
    [P].[SiteId] = [L].[SiteId]
FROM [Page] AS [P]
INNER JOIN [Layout] AS [L] ON [L].[Id] = [P].[LayoutId]

UPDATE [R] SET
    [R].[RegistrationTemplateId] = [RI].[RegistrationTemplateId]
FROM [Registration] AS [R]
INNER JOIN [RegistrationInstance] AS [RI] ON [RI].[Id] = [R].[RegistrationInstanceId]

UPDATE [AMI] SET
    [AMI].[AttributeMatrixTemplateId] = [AM].[AttributeMatrixTemplateId]
FROM [AttributeMatrixItem] AS [AMI]
INNER JOIN [AttributeMatrix] AS [AM] ON [AM].[Id] = [AMI].[AttributeMatrixId]" );

            // Switch the columns to non-null now.
            AlterColumn( "dbo.Page", "SiteId", c => c.Int() );
            AlterColumn( "dbo.Registration", "RegistrationTemplateId", c => c.Int() );
            AlterColumn( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId", c => c.Int() );

            // This is used by the Attribute Value SQL Views related to Groups
            // so they can find propert attributes on the nested inheritence.
            Sql( @"
CREATE VIEW [dbo].[GroupTypeInheritance]
AS
WITH cte AS
(
    SELECT
        [GT].[Id]
        , [GT].[InheritedGroupTypeId]
    FROM [GroupType] AS [GT]

    UNION ALL
    
    SELECT
        [cte].[Id]
        , [GT].[InheritedGroupTypeId]
    FROM [cte]
    INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [cte].[InheritedGroupTypeId]
)
SELECT * FROM cte WHERE [InheritedGroupTypeId] IS NOT NULL" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DROP VIEW [dbo].[GroupTypeInheritance]" );
            DropIndex( "dbo.Attribute", "IX_DefaultValueChecksum" );
            DropColumn( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId" );
            DropColumn( "dbo.Registration", "RegistrationTemplateId" );
            DropColumn( "dbo.Attribute", "DefaultValueChecksum" );
            DropColumn( "dbo.Page", "SiteId" );
        }
    }
}
