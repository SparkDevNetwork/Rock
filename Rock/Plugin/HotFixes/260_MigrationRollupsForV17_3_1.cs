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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 260, "17.3" )]
    public class MigrationRollupsForV17_3_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteObsoleteStarkDynamicAttributesBlock();
            UpdateRestActionDetailBlockToLegacy();
            JPH_RevertHiddenApplicationGroupTypeAddition_20250715_Up();
            UpdateAttendanceOccurrenceIndexUp();
            AddUpdateNamelessSchedulesJob();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
        }

        #region JH: Delete Obsolete StarkDynamicAttributes Block

        private void DeleteObsoleteStarkDynamicAttributesBlock()
        {
            Sql( @"
DECLARE @StarkDynamicAttributesBlockTypeGuid UNIQUEIDENTIFIER = '7c34a0fa-ed0d-4b8b-b458-6ec970711726';
DECLARE @BlockTypeId INT;

-- Look up the BlockType.Id if it exists
SELECT @BlockTypeId = [Id]
FROM [BlockType]
WHERE [Guid] = @StarkDynamicAttributesBlockTypeGuid ;

-- If it exists, delete dependent Blocks and then the BlockType
IF @BlockTypeId IS NOT NULL
BEGIN
    DELETE FROM [Block]
    WHERE [BlockTypeId] = @BlockTypeId;

    DELETE FROM [BlockType]
    WHERE [Id] = @BlockTypeId;
END;
" );
        }

        #endregion

        #region NA: Update REST Action Detail block to Legacy

        private void UpdateRestActionDetailBlockToLegacy()
        {
            Sql( @"
  UPDATE BlockType SET [Name] = 'REST Action Detail (Legacy)'
  WHERE [Guid] = '5BB83A28-CED2-4B40-9FDA-9C3D21FD6A83'
" );
        }

        #endregion

        #region JPH: Revert addition of "Hidden Application Group" Group Type and associated Role

        /// <summary>
        /// JPH: Revert the addition of the "Hidden Application Group" group type and associated role.
        /// </summary>
        /// <remarks>
        /// The "Chat Ban List" group was the only group seeded with these references, and they'll be shifted to the
        /// preexisting "Application Group" group type and associated role below.
        /// </remarks>
        private void JPH_RevertHiddenApplicationGroupTypeAddition_20250715_Up()
        {
            /*
                7/15/2025 - JPH

                When adding Rock's Chat feature, we originally added a "Hidden Application Group" group type and
                associated "Hidden Application Group Member" group type role. Unfortunately, a loose migration updated
                all existing "Application Group Member" group member records to be assigned to the new "Hidden .." role,
                which caused the photo verification process to break.

                While addressing this issue, it was decided that the "Hidden .." group type and role are not needed, and
                the "Chat Ban List" group can instead simply use the preexisting "Application Group" group type and
                associated role. This migration serves to update targeted group member records back to the "Application
                Group Member" role and also delete the no-longer-needed "Hidden .." group type and role.

                Reason: Remove "Hidden Application Group Type" and associated role that are not needed.
                https://github.com/SparkDevNetwork/Rock/issues/6369
                https://app.asana.com/1/20866866924293/project/1208321217019996/task/1210746747838998
            */

            Sql( @"
-- Revert the previous change that set all 'Application Group Member' records to 'Hidden Application Group Member'.
DECLARE @ApplicationGroupMemberRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '09B14358-FA17-4D65-A8E9-03FA7312CD62');
DECLARE @HiddenApplicationGroupMemberRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2008B263-CD41-45F0-8033-26D949FC0DA7');

UPDATE [GroupMember]
SET [GroupRoleId] = @ApplicationGroupMemberRoleId
WHERE [GroupRoleId] = @HiddenApplicationGroupMemberRoleId;

-- Update any groups and group members currently referencing the 'Hidden Application Group' group type to instead
-- reference the 'Application Group' group type. There aren't likely to be any.
DECLARE @ApplicationGroupGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '3981CF6D-7D15-4B57-AACE-C0E25D28BD49');
DECLARE @HiddenApplicationGroupGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '2C6F2847-B404-4595-AB35-CE42F2303868');

UPDATE [Group]
SET [GroupTypeId] = @ApplicationGroupGroupTypeId
WHERE [GroupTypeId] = @HiddenApplicationGroupGroupTypeId;

UPDATE [GroupMember]
SET [GroupTypeId] = @ApplicationGroupGroupTypeId
WHERE [GroupTypeId] = @HiddenApplicationGroupGroupTypeId;" );

            try
            {
                // Delete the "Hidden Application Group" group type and associated role.
                RockMigrationHelper.DeleteGroupTypeRole( "2008B263-CD41-45F0-8033-26D949FC0DA7" );
                RockMigrationHelper.DeleteGroupType( "2C6F2847-B404-4595-AB35-CE42F2303868" );
            }
            catch
            {
                // Fail silently.
            }
        }

        #endregion

        #region KH: Update Attendance Occurrence Index

        private void UpdateAttendanceOccurrenceIndexUp()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.3 - Update AttendanceOccurrence Index",
                description: "This job will remove a redundant index from the AttendanceOccurrence table.",
                jobType: "Rock.Jobs.PostV173UpdateAttendanceOccurrenceIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_173_UPDATE_ATTENDANCEOCCURRENCE_INDEX );
        }

        #endregion

        #region KH: Add Update Nameless Schedules Job

        private void AddUpdateNamelessSchedulesJob()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.3 - Update Nameless Schedules Job",
                description: "This job will update Nameless Schedules to store a Friendly Name in their Description Column.",
                jobType: "Rock.Jobs.PostV173UpdateNamelessSchedules",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_173_UPDATE_NAMELESS_SCHEDULES );
        }

        #endregion
    }
}