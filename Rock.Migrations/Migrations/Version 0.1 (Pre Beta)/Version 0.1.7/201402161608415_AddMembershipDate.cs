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
    public partial class AddMembershipDate : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
            -- Add a membership date attribute for the Person EntityType and Membership Category
            DECLARE @DateFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '6B6AA175-4758-453F-8D83-FCD8044B5F36')
            DECLARE @PersonEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7')
            DECLARE @MembershipCategoryId int = (SELECT [Id] FROM [Category] WHERE [Guid] = 'E919E722-F895-44A4-B86D-38DB8FBA1844')
            INSERT INTO [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid])
            VALUES (1, @DateFieldTypeId, @PersonEntityTypeId, N'', N'', N'MembershipDate', N'Membership Date', N'The date the person became a member.', 0, 0, N'', 0, 0, N'E1F56E34-7A75-474A-8A89-2C1D80DAD658')
            DECLARE @MembershipAttributeId int = ( SELECT SCOPE_IDENTITY() )
            INSERT INTO [dbo].[AttributeCategory] ([AttributeId], [CategoryId]) VALUES ( @MembershipAttributeId, @MembershipCategoryId )
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
            DELETE [Attribute] WHERE [Guid] = 'E1F56E34-7A75-474A-8A89-2C1D80DAD658'
            -- Cascade delete removes from AttributeCategory
            " );
        }
    }
}
