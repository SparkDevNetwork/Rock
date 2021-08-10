﻿// <copyright>
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
    public partial class AddOfflineAccessClaim : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"DECLARE @OfflineScopeId AS INT
                SELECT @OfflineScopeId = Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.OFFLINE}'

                IF(NOT EXISTS(SELECT 1 FROM AuthClaim WHERE Name = 'offline_access' AND ScopeId = @OfflineScopeId))
                BEGIN
	                INSERT INTO AuthClaim (IsActive, IsSystem, Name, PublicName, ScopeId, CreatedDateTime, ModifiedDateTime, [Guid])
	                VALUES (1, 1, 'offline_access', 'Send Refresh Token', @OfflineScopeId, GETDATE(), GETDATE(), '8B36C3FF-C546-4B8E-9995-C990C29F86AA')
                END
                ELSE
                BEGIN
	                UPDATE AuthClaim SET [Guid] = '8B36C3FF-C546-4B8E-9995-C990C29F86AA', IsActive = 1, IsSystem = 1 WHERE Name = 'offline_access' AND ScopeId = @OfflineScopeId
                END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
