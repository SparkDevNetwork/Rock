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
using Rock.Checkr.Constants;
using Rock.Plugin;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 9, "1.8.0" )]
    public class Checkr_UpdateExistingPmmPackages : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// This add "PMM - " to any Protect My Ministry packages that the previous migration missed. The previous migration only updated the build in packages
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [dbo].[DefinedValue]
   SET 
      [Value] = 'PMM - ' + Value
      ,[ForeignId] = 1
WHERE( DefinedTypeId = (SELECT TOP (1) Id FROM DefinedType WHERE (Guid = '"+ DefinedType.BACKGROUND_CHECK_TYPES + @"'))
 AND ( ForeignId is NULL OR ForeignId = 0 ) 
 AND( NOT( Value LIKE N'PMM - %' ) ))" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}