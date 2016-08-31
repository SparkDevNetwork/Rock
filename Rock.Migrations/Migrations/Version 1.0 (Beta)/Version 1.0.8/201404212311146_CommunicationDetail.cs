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
    public partial class CommunicationDetail : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Rename 'Communication' block to 'NewCommunication'
            Sql( @"
    UPDATE [BlockType] SET 
        [Path] = '~/Blocks/Communication/NewCommunication.ascx',
        [Name] = 'New Communication'
    WHERE [Guid] = 'D9834641-7F39-4CFA-8CB2-E64068127565'
" );
            // Add 'CommunicationDetail' block
            UpdateBlockType( "Communication Detail", "Used for displaying details of an existing communication that has already been created.", "~/Blocks/Communication/CommunicationDetail.ascx", "Communication", "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF" );

            // Add Block to Page: Communication Detail, Site: Rock RMS
            AddBlock( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "", "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "Communication Detail", "Main", "", "", 1, "A02F7695-4C6E-44E9-84CB-42E6F51F285F" );

            // Add approve security for communication detail block
            AddSecurityAuthForBlock( "A02F7695-4C6E-44E9-84CB-42E6F51F285F", 0, "Approve", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", Model.SpecialRole.None, "61C581F1-1F43-49A7-A49A-B743990D2465" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Communication Detail, from Page: New Communication, Site: Rock RMS
            DeleteSecurityAuth( "61C581F1-1F43-49A7-A49A-B743990D2465" );
            DeleteBlock( "A02F7695-4C6E-44E9-84CB-42E6F51F285F" );
            DeleteBlockType( "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF" ); // Communication Detail

            Sql( @"
    UPDATE [BlockType] SET 
        [Path] = '~/Blocks/Communication/Communication.ascx',
        [Name] = 'Communication'
    WHERE [Guid] = 'D9834641-7F39-4CFA-8CB2-E64068127565'
" );
        }
    }
}
