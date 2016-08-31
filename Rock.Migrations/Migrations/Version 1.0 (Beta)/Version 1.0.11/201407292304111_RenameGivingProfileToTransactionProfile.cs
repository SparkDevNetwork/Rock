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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class RenameGivingProfileToTransactionProfile : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            @Sql( @"

                  UPDATE [BlockType] 
                  SET	[Path] = '~/Blocks/Finance/TransactionProfileList.ascx' 
		                , [Name] = 'Transaction Profile List'
                  WHERE [Guid] = '694FF260-8C6F-4A59-93C9-CF3793FE30E6'

                    UPDATE [BlockType] 
                  SET	[Path] = '~/Blocks/Finance/TransactionProfileDetail.ascx'
		                , [Name] = 'Transaction Profile Detail' 
                  WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE'

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
