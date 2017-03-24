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
    [MigrationNumber( 6, "1.5.0" )]
    public class FixCheckinPrevPages : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//  Moved to core migration: 201612121647292_HotFixesFrom6_1
//            Sql( @"
//    DECLARE @AttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B' )
//    UPDATE [AttributeValue] 
//	SET [value] = '67bd09b0-0c6e-44e7-a8eb-0e71551f3e6b'
//	WHERE [AttributeId] = @AttributeId

//    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '39D260A5-A976-4DA9-B3E0-7381E9B8F3D5' )
//    UPDATE [AttributeValue] 
//	SET [value] = 'a1cbdaa4-94dd-4156-8260-5a3781e39fd0'
//	WHERE [AttributeId] = @AttributeId
//" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
