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
using System;

namespace Rock.Migrations
{
    /// <summary>
    /// Creates the default check-in labels for the next-gen check-in system.
    /// </summary>
    public partial class AddDefaultNextGenCheckInLabels : Rock.Migrations.RockMigration
    {
        private static readonly string ChildLabelGuid = "9282c162-fd92-4258-89c3-356918714e2c";

        private static readonly string NoteLabelGuid = "2272b545-e254-4377-8b06-3e465a6c8545";

        private static readonly string ParentLabelGuid = "053e5f25-8d88-4c9c-a42d-774b30e6a427";

        private static readonly string NameTagGuid = "b275d754-089d-41c3-83e3-f11f303b901b";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateChildLabelUp();
            CreateNoteLabelUp();
            CreateParentLabelUp();
            CreateNameTagUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateNameTagDown();
            CreateParentLabelDown();
            CreateNoteLabelDown();
            CreateChildLabelDown();
        }

        private void CreateChildLabelUp()
        {
            var previewImageData = BitConverter.ToString( MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_ChildLabelPng ).Replace( "-", "" );
            var content = MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_ChildLabelJson;
            var additionalSettingsJson = "{\"Rock.Model.CheckInLabel.ConditionalCriteria\":{\"Guid\":\"781f4f64-28c2-4351-a15c-90943d74670e\",\"ExpressionType\":1,\"Rules\":[],\"Groups\":null}}";

            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [CheckInLabel] WHERE [Guid] = '{ChildLabelGuid}')
BEGIN
    INSERT INTO [CheckInLabel]
    ([Name], [Description], [IsActive], [IsSystem], [LabelFormat], [LabelType], [Content], [PreviewImage], [AdditionalSettingsJson], [Guid])
    VALUES
    ('Child Label', 'Label for attaching to the child.', 1, 1, 0, 1, '{content.Replace( "'", "''" )}', 0x{previewImageData}, '{additionalSettingsJson.Replace( "'", "''" )}', '{ChildLabelGuid}')
END" );
        }

        private void CreateChildLabelDown()
        {
            Sql( $"DELETE FROM [CheckInLabel] WHERE [Guid] = '{ChildLabelGuid}'" );
        }

        private void CreateNoteLabelUp()
        {
            var previewImageData = BitConverter.ToString( MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_NoteLabelPng ).Replace( "-", "" );
            var content = MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_NoteLabelJson;
            var additionalSettingsJson = "{\"Rock.Model.CheckInLabel.ConditionalCriteria\":{\"Guid\":\"79736bc2-0d92-4808-abc5-c1b24593ada8\",\"ExpressionType\":1,\"Rules\":[],\"Groups\":null}}";

            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [CheckInLabel] WHERE [Guid] = '{NoteLabelGuid}')
BEGIN
    INSERT INTO [CheckInLabel]
    ([Name], [Description], [IsActive], [IsSystem], [LabelFormat], [LabelType], [Content], [PreviewImage], [AdditionalSettingsJson], [Guid])
    VALUES
    ('Note Label', 'Label that contains legal and allergy notes about the child to be used in a log book.', 1, 1, 0, 4, '{content.Replace( "'", "''" )}', 0x{previewImageData}, '{additionalSettingsJson.Replace( "'", "''" )}', '{NoteLabelGuid}')
END" );
        }

        private void CreateNoteLabelDown()
        {
            Sql( $"DELETE FROM [CheckInLabel] WHERE [Guid] = '{NoteLabelGuid}'" );
        }

        private void CreateParentLabelUp()
        {
            var previewImageData = BitConverter.ToString( MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_ParentLabelPng ).Replace( "-", "" );
            var content = MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_ParentLabelJson;
            var additionalSettingsJson = "{\"Rock.Model.CheckInLabel.ConditionalCriteria\":{\"Guid\":\"21cfd80d-83ab-4cf6-a747-917479f82930\",\"ExpressionType\":1,\"Rules\":[],\"Groups\":null}}";

            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [CheckInLabel] WHERE [Guid] = '{ParentLabelGuid}')
BEGIN
    INSERT INTO [CheckInLabel]
    ([Name], [Description], [IsActive], [IsSystem], [LabelFormat], [LabelType], [Content], [PreviewImage], [AdditionalSettingsJson], [Guid])
    VALUES
    ('Parent Label', 'Label for the parent to use to pick-up the child.', 1, 1, 0, 0, '{content.Replace( "'", "''" )}', 0x{previewImageData}, '{additionalSettingsJson.Replace( "'", "''" )}', '{ParentLabelGuid}')
END" );
        }

        private void CreateParentLabelDown()
        {
            Sql( $"DELETE FROM [CheckInLabel] WHERE [Guid] = '{ParentLabelGuid}'" );
        }

        private void CreateNameTagUp()
        {
            var previewImageData = BitConverter.ToString( MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_NameTagPng ).Replace( "-", "" );
            var content = MigrationSQL._202411062044462_AddDefaultNextGenCheckInLabels_NameTagJson;
            var additionalSettingsJson = "{\"Rock.Model.CheckInLabel.ConditionalCriteria\":{\"Guid\":\"74923516-5e78-477f-869d-4c375888c5e1\",\"ExpressionType\":1,\"Rules\":[],\"Groups\":null}}";

            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [CheckInLabel] WHERE [Guid] = '{NameTagGuid}')
BEGIN
    INSERT INTO [CheckInLabel]
    ([Name], [Description], [IsActive], [IsSystem], [LabelFormat], [LabelType], [Content], [PreviewImage], [AdditionalSettingsJson], [Guid])
    VALUES
    ('Name Tag', 'Generic name tag for check-in.', 1, 1, 0, 1, '{content.Replace( "'", "''" )}', 0x{previewImageData}, '{additionalSettingsJson.Replace( "'", "''" )}', '{NameTagGuid}')
END" );
        }

        private void CreateNameTagDown()
        {
            Sql( $"DELETE FROM [CheckInLabel] WHERE [Guid] = '{NameTagGuid}'" );
        }
    }
}
