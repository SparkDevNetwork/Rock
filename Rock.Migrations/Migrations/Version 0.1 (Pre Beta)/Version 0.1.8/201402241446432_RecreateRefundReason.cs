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
    public partial class RecreateRefundReason : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedType( "Financial", "Refund Reason", "Determines the reason for the financial transaction refund", "61FE3A58-9F4F-472F-A4E0-5116EB90A323" );
            AddDefinedValue( "61FE3A58-9F4F-472F-A4E0-5116EB90A323", "Duplicate Payment", "The person entered the transaction more than once", Guid.NewGuid().ToString() );
            AddDefinedValue( "61FE3A58-9F4F-472F-A4E0-5116EB90A323", "Incorrect Amount", "The person entered the incorrect amount on the transaction", Guid.NewGuid().ToString() );
            AddDefinedValue( "61FE3A58-9F4F-472F-A4E0-5116EB90A323", "Insufficient Funds", "The account did not have the sufficient funds", Guid.NewGuid().ToString() );
            AddDefinedValue( "61FE3A58-9F4F-472F-A4E0-5116EB90A323", "Refund Requested", "The person requested a refund", Guid.NewGuid().ToString() );
            AddDefinedValue( "61FE3A58-9F4F-472F-A4E0-5116EB90A323", "Returned Check", "The check was returned", Guid.NewGuid().ToString() );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                DELETE [DefinedType] WHERE [Guid] = '61FE3A58-9F4F-472F-A4E0-5116EB90A323'
                -- Cascade delete takes care of DefinedValues
            " );
        }
    }
}
