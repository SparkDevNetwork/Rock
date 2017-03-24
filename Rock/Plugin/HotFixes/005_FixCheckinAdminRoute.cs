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
    [MigrationNumber( 5, "1.5.0" )]
    public class FixCheckinAdminRoute : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//  Moved to core migration: 201605232234462_FamilyCheckinType
//            Sql( @"
//    DECLARE @PageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7B7207D0-B905-4836-800E-A24DDC6FE445' )
//    IF @PageId IS NOT NULL 
//    BEGIN
//        UPDATE [PageRoute] SET [PageId] = @PageId
//        WHERE [Route] = 'checkin/{KioskId}/{CheckinConfigId}/{GroupTypeIds}'
//    END
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
