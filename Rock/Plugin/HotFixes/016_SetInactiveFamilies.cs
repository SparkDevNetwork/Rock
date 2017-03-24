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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 16, "1.6.0" )]
    public class SetInactiveFamilies : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
	-- Clean up migration for issue #1103.
	-- Find family groups that have only 'inactive' people (record status) and mark the groups inactive.
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)

	DECLARE @cPERSON_RECORD_STATUS_INACTIVE_GUID uniqueidentifier = '1DAD99D5-41A9-4865-8366-F269902B80A4';
	DECLARE @RecordStatusInactiveId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_STATUS_INACTIVE_GUID)

	-- All groups that are currently active, but don't have a single member whose record status is not inactive
	UPDATE [Group]
	SET
		[IsActive] = 0
	WHERE NOT EXISTS (
		-- All family groups whose members are NOT inactive
		SELECT 1 FROM [Group] g 
		INNER JOIN [GroupMember] gm ON gm.[GroupId] = g.[Id]
		INNER JOIN [Person] p on p.[Id] = gm.[PersonId]
		WHERE 
			g.[Id] = [Group].[Id]
			AND g.[GroupTypeId] = @FamilyGroupTypeId 
			AND p.[RecordStatusValueId] <> @RecordStatusInactiveId
	)
	AND [Group].[GroupTypeId] = @FamilyGroupTypeId 
	AND [Group].[IsActive] = 1
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
