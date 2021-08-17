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
    public partial class RegistrationInstanceTimeout : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            EmptyMigrationCodeForRegistrationInstanceTimeout();

            UpdateFromFieldForSendSMS();

            RockMobileCommunicationPushUpdates();

            // GJ: Fix Person Entity IndexResultTemplate
            Sql( MigrationSQL._202107121730176_RegistrationInstanceTimeout_UpdatePersonEntity );
        }

        /// <summary>
        /// SK: Updated From field value from Defined Value to Workflow Text Or Attribute Value field 
        /// </summary>
        private void UpdateFromFieldForSendSMS()
        {
            Sql( @"
                DECLARE @EntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name]='Rock.Model.WorkflowActionType')
                DECLARE @EntityTypeQualifierValue INT = (SELECT [Id] FROM [EntityType] WHERE [Name]='Rock.Workflow.Action.SendSms')
                DECLARE @NewFieldType INT = (SELECT [Id] FROM [FieldType] WHERE [Guid]='3B1D93D7-9414-48F9-80E5-6A3FC8F94C20')

                UPDATE
	                [Attribute]
                SET [FieldTypeId]=@NewFieldType, [Name]='From|Attribute Value'
                WHERE
	                [EntityTypeId]=@EntityTypeId AND [EntityTypeQualifierColumn]='EntityTypeId' AND [EntityTypeQualifierValue]=@EntityTypeQualifierValue AND [Key] = 'From'" );
        }

        private void EmptyMigrationCodeForRegistrationInstanceTimeout()
        {
            /*  07-12-2021 MDP, these were already added to the database with https://github.com/SparkDevNetwork/Rock/blob/cfa696e257bb42e7f44a30024ca5ffe59f864bb8/Rock.Migrations/Migrations/202106141449072_RegistrationTimeout.cs#L25, 
            *  but the changes to the model weren't added until https://github.com/SparkDevNetwork/Rock/blob/cfa696e257bb42e7f44a30024ca5ffe59f864bb8/Rock/Model/RegistrationInstance.cs#L278.
            *  So, this re-syncs

           AddColumn("dbo.RegistrationInstance", "TimeoutIsEnabled", c => c.Boolean(nullable: false));
           AddColumn("dbo.RegistrationInstance", "TimeoutLengthMinutes", c => c.Int());
           AddColumn("dbo.RegistrationInstance", "TimeoutThreshold", c => c.Int());
           */
        }

        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// DH: Update Mobile Communication View default template
        /// </summary>
        private void RockMobileCommunicationPushUpdates()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "39B8B16D-D213-46FD-9B8F-710453806193",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW,
                "Default",
                @"<StackLayout>
    <Label Text=""{{ Communication.PushTitle | Escape }}"" StyleClass=""h1"" />
    <Rock:Html>
        <![CDATA[{{ Content }}]]>
    </Rock:Html>
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }


        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
