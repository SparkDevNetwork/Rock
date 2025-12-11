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
    public partial class FixRestViewSupportNullableColumns : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // This was supposed to happen in migration 202412172125380_AddRestViewSupport,
            // but the columns were not set to non-nullable correctly.

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
            AlterColumn( "dbo.Page", "SiteId", c => c.Int( nullable: false ) );
            AlterColumn( "dbo.Registration", "RegistrationTemplateId", c => c.Int( nullable: false ) );
            AlterColumn( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId", c => c.Int( nullable: false ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn( "dbo.Page", "SiteId", c => c.Int( nullable: true ) );
            AlterColumn( "dbo.Registration", "RegistrationTemplateId", c => c.Int( nullable: true ) );
            AlterColumn( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId", c => c.Int( nullable: true ) );
        }
    }
}
