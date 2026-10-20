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
    /// Has the chat sync projection carry each channel's description and the name of its group
    /// type.
    /// </summary>
    public partial class AddChatChannelDescriptionAndGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateProjectionProcedure( RockMigrationSQL._202610201200000_AddChatChannelDescriptionAndGroupType_spChat_SyncProjection );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateProjectionProcedure( RockMigrationSQL._202610061400000_AddGroupTypeRoleChatCapabilities_spChat_SyncProjection );
        }

        /// <summary>
        /// Replaces [spChat_SyncProjection] with the given text.
        /// </summary>
        /// <param name="createProcedureSql">The procedure's CREATE statement.</param>
        private void CreateProjectionProcedure( string createProcedureSql )
        {
            // A procedure keeps the ANSI_NULLS and QUOTED_IDENTIFIER settings it was created with,
            // whatever its caller's are, and this one gathers badge keys with an XML method that
            // fails outright without QUOTED_IDENTIFIER on. So both are pinned for the create and put
            // back afterwards.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Replace [spChat_SyncProjection] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spChat_SyncProjection]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spChat_SyncProjection];" );

            Sql( createProcedureSql );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }
    }
}
