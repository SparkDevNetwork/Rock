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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 1, "1.4.5" )]
    public class FixPhoneCountryCode : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Moved to 201606281347449_PersonDirectory
//            Sql( @"
//    DECLARE @CountryCode nvarchar(3) = ( SELECT TOP 1 [Value] FROM [DefinedValue] WHERE [DefinedTypeId] IN ( SELECT [Id] FROM [DefinedType] WHERE [Guid] = '45E9EF7C-91C7-45AB-92C1-1D6219293847' ) ORDER BY [Order] )
//    IF @CountryCode IS NOT NULL
//    BEGIN
//	    UPDATE [PhoneNumber]
//	    SET [CountryCode] = @CountryCode
//	    WHERE [CountryCode] IS NULL
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
