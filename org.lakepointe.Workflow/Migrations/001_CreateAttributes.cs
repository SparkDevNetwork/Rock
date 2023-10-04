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
using Rock.Plugin;

namespace org.lakepointe.Migrations
{

    [MigrationNumber( 1, "1.8.0" )]
    public class CreateAttributes : Migration
    {
        public override void Up()
        {
            Sql( @"
                DECLARE @AttributeId INT
                INSERT INTO [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (0, 46, 16, N'GroupTypeId', N'15', N'AbsoluteBirthdateRange', N'Absolute Birthdate Range', N'The absolute birth date range allowed to check in to these group types.', 0, 0, NULL, 0, 0, N'2FDCC6D6-F7A6-5D98-495F-6A0AD7947DE1')
                SET @AttributeId = SCOPE_IDENTITY()
                INSERT INTO [dbo].[AttributeCategory] ([AttributeId], [CategoryId]) VALUES (@AttributeId, 71)

                INSERT INTO [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (0, 47, 16, N'GroupTypeId', N'15', N'AbsoluteAgeRange', N'Absolute Age Range', N'The absolute age range allowed to check in to these group types.', 0, 0, NULL, 0, 0, N'8EEA182F-174B-3398-4192-A8BC271C8BB5')
                SET @AttributeId = SCOPE_IDENTITY()
                INSERT INTO [dbo].[AttributeCategory] ([AttributeId], [CategoryId]) VALUES (@AttributeId, 71)


                INSERT INTO[dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES(0, 84, 16, N'GroupTypeId', N'17', N'AbsoluteGradeRange', N'Absolute Grade Range', N'Defines the absolute grade level range that is allowed to check in to these group types.', 0, 0, NULL, 0, 0, N'E285A57A-C07A-12AA-4233-504D5863DCB6')
                SET @AttributeId = SCOPE_IDENTITY()
                INSERT INTO [dbo].[AttributeCategory] ([AttributeId], [CategoryId]) VALUES (@AttributeId, 71)

                INSERT [dbo].[AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (1, @AttributeId, N'definedtype', N'24e5a79f-1e62-467a-ad5d-0d10a2328b4d', N'4D5362A5-3DC7-6FB1-4709-8D594A6B9410', NULL, NULL, NULL)
                INSERT [dbo].[AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (1, @AttributeId, N'displaydescription', N'True', N'A5C42B63-4C5C-1EA5-4FF8-CE2FA631D268', NULL, NULL, NULL)
            " );
        }

        public override void Down()
        {
        }
    }
}
