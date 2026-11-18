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

    using Rock.Model;
    using Rock.Plugin.HotFixes;
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20261118 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20):
            // Note: These are ordered by the Rock version they are expected to
            //       run in.
            // ----------------------------------------------------------------
            // v17.8; 345_AddBackgroundCheckAdministratorEditAuth.cs
            CH_AddBackgroundCheckAdministratorEditAuth_Up();

            // v17.8; 346_EscapeLavaChartShortcodeData.cs
            // v19.0; 347_ReapplyEscapeLavaChartShortcodeData.cs
            JMH_EscapeLavaChartShortcodeData_Up();

            // v19.4; 349_SecureEventAndLmsAISkills.cs
            MSE_SecureEventAndLmsAISkills_Up();

            // v20.0; 350_SecureCoreAdministrationAISkill.cs
            MSE_SecureCoreAdministrationAISkill_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region 345_AddBackgroundCheckAdministratorEditAuth

        /// <summary>
        /// CH: Adds a default rule allowing Rock Administrators to edit background checks.
        /// </summary>
        private void CH_AddBackgroundCheckAdministratorEditAuth_Up()
        {
            /*
                11/10/26 - CH

                Saved background checks now take their security from the Background Check
                entity type and then GlobalRestrictedDefault, which only grants Administrate.
                Without this rule, Rock Administrators would lose the Edit access they had
                before. The rule is appended after any existing rules so an administrator's
                own Background Check rules still win, and the helper skips it if it exists.

                Reason: Keep Rock Administrators' existing Edit access on background checks.
            */
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.BackgroundCheck",
                int.MaxValue,
                Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "27CB13F8-290A-4000-8581-2104D61F2F88" );
        }

        #endregion 345_AddBackgroundCheckAdministratorEditAuth

        #region 346_EscapeLavaChartShortcodeData and 347_ReapplyEscapeLavaChartShortcodeData

        /// <summary>
        /// JMH: Encodes the data item and dataset values that the Chart Lava shortcode writes
        /// into its script block.
        /// </summary>
        private void JMH_EscapeLavaChartShortcodeData_Up()
        {
            /*
                11/13/26 - JMH

                The Chart shortcode markup is stored in the database, so each fragment that writes
                a label, tooltip, color, click URL or value into the script block is replaced in place.
                A fragment an administrator has edited no longer matches and is left alone,
                and a fragment that was already replaced is gone, so running this again changes nothing.
                The escape sequences are built with NCHAR(92) and held in captured Lava variables
                because a quoted Lava string can turn a backslash sequence back into the plain character.
                Click URLs are limited to http, https and paths that start with a single slash.
                Values stay unquoted numbers, so they keep only the characters a number can contain.

                Reason: Prevent stored XSS through data bound into the Chart shortcode.
            */

            /*
                11/18/2026 - CLAUDE

                Hotfix 347 ran this same SQL a second time under a new number, because the
                v19.0 EF migration Rollup_20260114 replaces the whole Chart shortcode markup
                after hotfix 346 may have already run. This EF migration runs after
                Rollup_20260114, so a single run here covers both hotfix migrations.

                Reason: One run of the SQL replaces both hotfix migrations 346 and 347.
            */
            Sql( EscapeLavaChartShortcodeData.EscapeChartMarkupSql );
        }

        #endregion 346_EscapeLavaChartShortcodeData and 347_ReapplyEscapeLavaChartShortcodeData

        #region 349_SecureEventAndLmsAISkills

        private const string EventRegistrationSkillEntityTypeGuid = "07712e0b-efa9-4775-a74c-75f6c58d5210";
        private const string EventRegistrationSkillGuid = "127a9726-5922-47e6-ae2d-a3901f60367b";
        private const string LearningManagementSkillEntityTypeGuid = "b808a071-5d86-4d69-936d-7573130dd842";
        private const string LearningManagementSkillGuid = "57f6c7d7-883b-4793-84ea-0fd21b1f09a9";

        /// <summary>
        /// MSE: Limits the Event Registration and Learning Management AI skills to
        /// administrators, staff and staff-like people by default.
        /// </summary>
        private void MSE_SecureEventAndLmsAISkills_Up()
        {
            /*
                11/16/26 - MSE

                These skills were registered with no VIEW rules, so every person who could use an
                agent with one of them attached got its tools. This adds the same staff-only rules
                the Person skill ships with. Core registers skills at startup after migrations run,
                so the entity type and skill rows are added first; otherwise the rules would not
                attach on a fresh install. The helper skips any rule that already exists, so an
                administrator's own rules on these skills are kept and running this again changes nothing.

                Reason: Keep the Event Registration and Learning Management skills away from non-staff agent users by default.
            */
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.EventRegistrationSkill",
                EventRegistrationSkillEntityTypeGuid,
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Event Registration Skill",
                "This skill provides access to working with event registrations",
                EventRegistrationSkillEntityTypeGuid,
                EventRegistrationSkillGuid );

            AddStaffOnlySecurityForAISkill(
                EventRegistrationSkillGuid,
                "131c75ad-9ec1-4f7a-b0de-b7d10bdb0cf2",
                "40f2570c-6858-4f84-b752-72c0cbe6c25c",
                "fd6a20f8-fbaf-46ec-a79c-bbe4f7a062f1",
                "b915c191-3df7-43f7-b410-f5db3a515ab9" );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.LearningManagementSkill",
                LearningManagementSkillEntityTypeGuid,
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Learning Management Skill",
                "This skill provides access to LMS data.",
                LearningManagementSkillEntityTypeGuid,
                LearningManagementSkillGuid );

            AddStaffOnlySecurityForAISkill(
                LearningManagementSkillGuid,
                "2da34f31-8c73-4464-8adc-722f0adcb3c0",
                "359a92b1-5d56-41cf-b82a-b37834735f4d",
                "799af53f-9b92-4938-9352-f28031526812",
                "0b76a44c-1111-4773-aa2d-173876f37ead" );
        }

        /// <summary>
        /// Grants VIEW on a skill to Rock administrators, staff and staff-like
        /// people and denies it to everyone else. Same rules hotfix 309 gives
        /// the Person skill.
        /// </summary>
        /// <param name="skillGuid">The GUID of the AI skill to secure.</param>
        /// <param name="administratorsAuthGuid">The GUID of the administrators allow rule.</param>
        /// <param name="staffAuthGuid">The GUID of the staff allow rule.</param>
        /// <param name="staffLikeAuthGuid">The GUID of the staff-like allow rule.</param>
        /// <param name="allUsersDenyAuthGuid">The GUID of the all users deny rule.</param>
        private void AddStaffOnlySecurityForAISkill( string skillGuid, string administratorsAuthGuid, string staffAuthGuid, string staffLikeAuthGuid, string allUsersDenyAuthGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAISkill(
                skillGuid,
                100,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                administratorsAuthGuid );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                skillGuid,
                101,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) SpecialRole.None,
                staffAuthGuid );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                skillGuid,
                102,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) SpecialRole.None,
                staffLikeAuthGuid );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                skillGuid,
                103,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                allUsersDenyAuthGuid );
        }

        /// <summary>
        /// Adds or updates the AI skill with the specified parameters. If the
        /// skill already exists for the specified <paramref name="skillGuid"/>
        /// then it will be updated with the new values. If it does not exist,
        /// it will be created with the specified values. Copied from hotfix 309.
        /// </summary>
        /// <param name="name">The name of the AI skill.</param>
        /// <param name="description">The user-friendly description of the AI skill.</param>
        /// <param name="codeEntityTypeGuid">The GUID of the code entity type.</param>
        /// <param name="skillGuid">The GUID of the AI skill.</param>
        private void AddOrUpdateCodeAISkill( string name, string description, string codeEntityTypeGuid, string skillGuid )
        {
            if ( codeEntityTypeGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( codeEntityTypeGuid ), "The code entity type guid must be a valid guid." );
            }

            if ( skillGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( skillGuid ), "The skill guid must be a valid guid." );
            }

            Sql( $@"
DECLARE @CodeEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{codeEntityTypeGuid}')

IF NOT EXISTS (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{skillGuid}')
BEGIN
    INSERT INTO [AISkill] (
        [Name]
        , [Description]
        , [CodeEntityTypeId]
        , [Guid]
    )
    VALUES (
        '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , @CodeEntityTypeId
        , '{skillGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkill]
    SET [Name] = '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , [Description] = '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , [CodeEntityTypeId] = @CodeEntityTypeId
    WHERE [Guid] = '{skillGuid}'
END
" );
        }

        #endregion 349_SecureEventAndLmsAISkills

        #region 350_SecureCoreAdministrationAISkill

        private const string CoreAdministrationSkillGuid = "6DBD6867-2E0B-4D2E-9BF9-B34B77E4E94B";

        /// <summary>
        /// MSE: Limits the Core Administration AI skill to Rock administrators by default.
        /// </summary>
        private void MSE_SecureCoreAdministrationAISkill_Up()
        {
            /*
                11/16/2026 - MSE

                The Core Administration skill was registered by the AddCodeComposerPage
                EF migration with no VIEW rules, so every person who could use an agent
                with it attached got its exception and global attribute tools. This adds
                the same two administrator-only rules the Workflow Builder skill gets.
                The skill row exists here because AddCodeComposerPage is an earlier EF
                migration. The rules use orders 100 and 101, like 349, so rules an
                administrator already added sort first. The helper skips any rule that
                already exists, so running this again changes nothing.

                Reason: Keep the Core Administration skill away from non-administrator agent users by default.
            */
            RockMigrationHelper.AddSecurityAuthForAISkill(
                CoreAdministrationSkillGuid,
                100,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "8C1E1E6A-B664-40DE-9816-D22682A05D85" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                CoreAdministrationSkillGuid,
                101,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "C2E20E74-DB12-4B55-AE85-BD444E48C763" );
        }

        #endregion 350_SecureCoreAdministrationAISkill
    }
}
