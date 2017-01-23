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
    public partial class InteractionData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
-- Channel is just one hardcoded thing called 'Communication' which is for any Email, SMS, etc
DECLARE @InteractionChannel_COMMUNICATION UNIQUEIDENTIFIER = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65'
    , @entityTypeIdCommunication INT = (
        SELECT TOP 1 Id

        FROM EntityType

        WHERE NAME = 'Rock.Model.Communication'
        )
	,@entityTypeIdCommunicationRecipient INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE NAME = 'Rock.Model.CommunicationRecipient'
		);

            IF NOT EXISTS(
                    SELECT *
                    FROM InteractionChannel
            
                    WHERE Guid = @InteractionChannel_COMMUNICATION
                    )
BEGIN
    INSERT INTO InteractionChannel (
        NAME
        , ComponentEntityTypeId
        , InteractionEntityTypeId
        , Guid
        )

    VALUES(
        'Communication'
        , @entityTypeIdCommunication
        , @entityTypeIdCommunicationRecipient
        , @InteractionChannel_COMMUNICATION
        );
            END;
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
