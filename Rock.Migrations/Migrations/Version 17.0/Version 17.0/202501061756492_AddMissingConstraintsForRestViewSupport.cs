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
    /// When the AddRestViewSupport migration was added I forgot to add the C#
    /// code to configure the relationships. This adds the foreign keys which
    /// should have been present from the start.
    /// </summary>
    public partial class AddMissingConstraintsForRestViewSupport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // This was already done in a previous migration but EF didn't
            // realize it.
            //AlterColumn("dbo.Registration", "RegistrationTemplateId", c => c.Int(nullable: false));

            // We don't want these indexes for now as they would probably
            // never be used and just waste CPU cycles.
            //CreateIndex("dbo.Page", "SiteId");
            //CreateIndex("dbo.Registration", "RegistrationTemplateId");
            //CreateIndex("dbo.AttributeMatrixItem", "AttributeMatrixTemplateId");

            // Fix any incorrect existing values so that the foreign keys work.
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

            AddForeignKey( "dbo.Page", "SiteId", "dbo.Site", "Id" );
            AddForeignKey( "dbo.Registration", "RegistrationTemplateId", "dbo.RegistrationTemplate", "Id" );
            AddForeignKey( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId", "dbo.AttributeMatrixTemplate", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.AttributeMatrixItem", "AttributeMatrixTemplateId", "dbo.AttributeMatrixTemplate" );
            DropForeignKey( "dbo.Registration", "RegistrationTemplateId", "dbo.RegistrationTemplate" );
            DropForeignKey( "dbo.Page", "SiteId", "dbo.Site" );

            //DropIndex("dbo.AttributeMatrixItem", new[] { "AttributeMatrixTemplateId" });
            //DropIndex("dbo.Registration", new[] { "RegistrationTemplateId" });
            //DropIndex("dbo.Page", new[] { "SiteId" });

            //AlterColumn("dbo.Registration", "RegistrationTemplateId", c => c.Int());
        }
    }
}
