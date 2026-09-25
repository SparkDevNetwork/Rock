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

    using Rock.Migrations.Migrations;

    /// <summary>
    /// Adds the stored procedure that reads a church's chat picture for the chat platform.
    /// </summary>
    public partial class AddChatSyncProjectionProcedure : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // A procedure keeps the ANSI_NULLS and QUOTED_IDENTIFIER settings it was created with,
            // whatever its caller's are, and this one gathers badge keys with an XML method that
            // fails outright without QUOTED_IDENTIFIER on. So both are pinned for the create and put
            // back afterwards.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spChat_SyncProjection] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spChat_SyncProjection]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spChat_SyncProjection];" );

            Sql( RockMigrationSQL._202609251612047_AddChatSyncProjectionProcedure_spChat_SyncProjection );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            // The sync job was seeded to notify on every run, which is an email an hour; errors only
            // is the usual choice. A setting an administrator has changed is left alone.
            Sql( $@"
UPDATE [ServiceJob]
SET [NotificationStatus] = 3
WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC_JOB}'
    AND [NotificationStatus] = 1;" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spChat_SyncProjection]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spChat_SyncProjection];" );
        }
    }
}
