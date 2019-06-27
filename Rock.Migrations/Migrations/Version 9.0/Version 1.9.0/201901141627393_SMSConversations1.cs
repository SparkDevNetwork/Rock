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
    public partial class SMSConversations1 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Updating the name of the DefinedType attribute "Enable Mobile Conversations" to "Enable Response Recipient Forwarding"
            RockMigrationHelper.UpdateDefinedTypeAttribute( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
                Rock.SystemGuid.FieldType.BOOLEAN, 
                "Enable Response Recipient Forwarding", 
                "EnableResponseRecipientForwarding", 
                "When enabled, SMS conversations would be processed by sending messages to the Response Recipient's mobile phone. Otherwise, the conversations would be handled using the SMS Conversations page.", 
                1019, 
                false, 
                "True", 
                false, 
                false, 
                "60E05E00-E1A3-46A2-A56D-FE208D91FE4F" );

            // Changed the page title to match the new DefinedType name.
            Sql( @"
                UPDATE [Page]
                SET 
	                  InternalName = 'SMS Phone Numbers'
	                , PageTitle = 'SMS Phone Numbers'
	                , BrowserTitle = 'SMS Phone Numbers'
                WHERE [Guid] = '3F1EA6E5-6C61-444A-A80E-5B66F96F521B'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
