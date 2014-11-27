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
    public partial class SystemEmailField : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DELETE [EntityType]
    WHERE [Name] = 'Rock.Workflow.Action.SendSystemEmail'

    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '4487702A-BEAF-4E5A-92AD-71A1AD48DFCE' )

    UPDATE [EntityType] SET 
	      [Name] = 'Rock.Workflow.Action.SendSystemEmail'
	    , [AssemblyName] = 'Rock.Workflow.Action.SendSystemEmail, Rock, Version=1.1.2.0, Culture=neutral, PublicKeyToken=null'
        , [FriendlyName] = 'Send System Email'
    WHERE [Id] = @EntityTypeId

    UPDATE [Attribute] SET
	      [Key] = 'SystemEmail'
	    , [Name] = 'System Email'
	    , [Description] = 'A system email to send.'
    WHERE [EntityTypeId] = @EntityTypeId
    AND [Key] = 'EmailTemplate'
" );

            // update new location of checkscanner installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.2.0/checkscanner.exe' where [Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
