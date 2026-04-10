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
    /// <summary>
    ///
    /// </summary>
    public partial class IncreaseBenevolenceRequestPhoneNumberSizes : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update BenevolenceRequest phone number columns from nvarchar(20) to nvarchar(50)
            Sql( @"
                ALTER TABLE [dbo].[BenevolenceRequest] ALTER COLUMN [HomePhoneNumber] NVARCHAR(50) NULL;
                ALTER TABLE [dbo].[BenevolenceRequest] ALTER COLUMN [CellPhoneNumber] NVARCHAR(50) NULL;
                ALTER TABLE [dbo].[BenevolenceRequest] ALTER COLUMN [WorkPhoneNumber] NVARCHAR(50) NULL;
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}