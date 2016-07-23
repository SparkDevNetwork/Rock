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
", Rock.SystemGuid.Group.GROUP_CONNECTION_ADMINISTRATORS ) );

            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/MyConnectionOpportunities.ascx", "~/Blocks/Connection/MyConnectionOpportunities.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ConnectionRequestDetail.ascx", "~/Blocks/Connection/ConnectionRequestDetail.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ConnectionTypeList.ascx", "~/Blocks/Connection/ConnectionTypeList.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ConnectionOpportunityList.ascx", "~/Blocks/Connection/ConnectionOpportunityList.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ConnectionTypeDetail.ascx", "~/Blocks/Connection/ConnectionTypeDetail.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ConnectionOpportunityDetail.ascx", "~/Blocks/Connection/ConnectionOpportunityDetail.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ExternalOpportunitySearch.ascx", "~/Blocks/Connection/ExternalOpportunitySearch.ascx", "Connection" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Involvement/ExternalConnectionOpportunityDetail.ascx", "~/Blocks/Connection/ExternalConnectionOpportunityDetail.ascx", "Connection" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
