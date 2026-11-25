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
namespace Rock.Data.Migrations;

/// <summary>
///
/// </summary>
[SqlMigration( 20260608223733, "20.0" )]
internal class WidenPersonViewedIpAddressTo45Chars : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        Sql( @"
                DECLARE @var0 nvarchar(128)
                SELECT @var0 = name
                FROM sys.default_constraints
                WHERE parent_object_id = object_id(N'dbo.PersonViewed')
                AND col_name(parent_object_id, parent_column_id) = 'IpAddress';
                IF @var0 IS NOT NULL
                    EXECUTE('ALTER TABLE [dbo].[PersonViewed] DROP CONSTRAINT [' + @var0 + ']')
                ALTER TABLE [dbo].[PersonViewed] ALTER COLUMN [IpAddress] [nvarchar](45) NULL" );
    }
}
