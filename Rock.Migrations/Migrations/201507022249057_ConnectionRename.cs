// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ConnectionRename : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( String.Format( @"
UPDATE [Group]
    SET [Name] = 'RSR - Connection Administration',
    [Description] = 'Group of individuals who can administrate the various parts of the connection functionality.'
    WHERE [Guid] = '{0}'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/MyConnectionOpportunities.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/MyConnectionOpportunities.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ConnectionRequestDetail.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ConnectionRequestDetail.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ConnectionTypeList.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ConnectionTypeList.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ConnectionOpportunityList.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ConnectionOpportunityList.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ConnectionTypeDetail.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ConnectionTypeDetail.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ConnectionOpportunityDetail.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ConnectionOpportunityDetail.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ExternalOpportunitySearch.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ExternalOpportunitySearch.ascx'

UPDATE [BlockType] SET
	[Path] = '~/Blocks/Connection/ExternalConnectionOpportunityDetail.ascx',
	[Category] = 'Connection'
    WHERE [Path] = '~/Blocks/Involvement/ExternalConnectionOpportunityDetail.ascx'
                ", Rock.SystemGuid.Group.GROUP_CONNECTION_ADMINISTRATORS ) );   
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
