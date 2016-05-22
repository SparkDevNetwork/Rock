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
    public partial class PageRenameSweep : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

                /* delete current person block */
                DELETE FROM [BlockType]
	                WHERE [Guid] = 'F7193487-1234-49D7-9CEC-7F5F452B7E3F'

                /* remove 'financial' block type category */
                UPDATE [BlockType]
	                SET [Category] = 'Finance'
	                WHERE [Category] = 'Financial'

                /* rename blocks */
                UPDATE [BlockType]
	                SET [Name] = 'Transaction Entry', [Path] = '~/Blocks/Finance/TransactionEntry.ascx'
	                WHERE [Guid] = '74EE3481-3E5A-4971-A02E-D463ABB45591'

                UPDATE [BlockType]
	                SET [Name] = 'Scheduled Transaction List', [Path] = '~/Blocks/Finance/ScheduledTransactionList.ascx'
	                WHERE [Guid] = '694FF260-8C6F-4A59-93C9-CF3793FE30E6'

                UPDATE [BlockType]
	                SET [Name] = 'Scheduled Transaction Detail', [Path] = '~/Blocks/Finance/ScheduledTransactionDetail.ascx'
	                WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE'

                UPDATE [BlockType]
	                SET [Name] = 'Pledge Entry', [Path] = '~/Blocks/Finance/PledgeEntry.ascx'
	                WHERE [Guid] = '20B5568E-A010-4E15-9127-E63CF218D6E5'

                UPDATE [BlockType]
	                SET [Name] = 'Communication Entry', [Path] = '~/Blocks/Communication/CommunicationEntry.ascx'
	                WHERE [Guid] = 'D9834641-7F39-4CFA-8CB2-E64068127565'

                UPDATE [BlockType]
	                SET [Name] = 'Email Preference Entry', [Path] = '~/Blocks/Communication/EmailPreferenceEntry.ascx'
	                WHERE [Guid] = 'B3C076C7-1325-4453-9549-456C23702069'

                UPDATE [BlockType]
	                SET [Name] = 'Prayer Request Entry', [Path] = '~/Blocks/Prayer/PrayerRequestEntry.ascx'
	                WHERE [Guid] = '4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6'

                UPDATE [BlockType]
	                SET [Category] = 'Prayer', [Path] = '~/Blocks/Prayer/PrayerRequestList.ascx'
	                WHERE [Guid] = '4D6B686A-79DF-4EFC-A8BA-9841C248BF74'

                UPDATE [BlockType]
	                SET [Category] = 'Prayer', [Path] = '~/Blocks/Prayer/PrayerRequestDetail.ascx'
	                WHERE [Guid] = 'F791046A-333F-4B2A-9815-73B60326162D'

                UPDATE [BlockType]
	                SET [Category] = 'Prayer', [Path] = '~/Blocks/Prayer/PrayerCommentList.ascx'
	                WHERE [Guid] = 'DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22'

                UPDATE [BlockType]
	                SET [Category] = 'Prayer', [Path] = '~/Blocks/Prayer/PrayerCommentDetail.ascx'
	                WHERE [Guid] = '4F3778DF-A25C-4E59-9242-B1D6813311E1'

                UPDATE [BlockType]
	                SET [Name] = 'Group Type Map', [Path] = '~/Blocks/Groups/GroupTypeMap.ascx'
	                WHERE [Guid] = '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF'

                UPDATE [BlockType]
	                SET [Name] = 'Account Edit', [Path] = '~/Blocks/Security/AccountEdit.ascx'
	                WHERE [Guid] = 'F501AB3F-1F41-4C06-9BC2-57C42E702995'

                UPDATE [BlockType]
	                SET [Name] = 'Account Entry', [Path] = '~/Blocks/Security/AccountEntry.ascx', [Description] = 'Block allows users to create a new login account.'
	                WHERE [Guid] = '99362B60-71A5-44C6-BCFE-DDA9B00CC7F3'

                UPDATE [BlockType]
	                SET [Name] = 'Account Detail', [Path] = '~/Blocks/Security/AccountDetail.ascx'
	                WHERE [Guid] = 'B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1'

                UPDATE [BlockType]
	                SET [Name] = 'User Login List', [Path] = '~/Blocks/Security/UserLoginList.ascx'
	                WHERE [Guid] = 'CE06640D-C1BA-4ACE-AF03-8D733FD3247C'

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
