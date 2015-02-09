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
    public partial class AddWorkflowNote : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Delete an entity type that might have already been created by Rock
    DELETE [EntityType]
    WHERE [Name] = 'Rock.Workflow.Action.AddWorkflowNote'
    AND [Guid] <> 'F6532683-9797-4F16-8DB7-8B413EDC8AD7'

    -- Rename the entity type
    UPDATE [EntityType] SET
	    [Name] = 'Rock.Workflow.Action.AddWorkflowNote',
	    [AssemblyName] = 'Rock.Workflow.Action.AddWorkflowNote, Rock, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Add Workflow Note',
	    [Guid] = 'F6532683-9797-4F16-8DB7-8B413EDC8AD7'
    WHERE [Name] = 'Rock.Workflow.Action.AddNote'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    -- Delete an entity type that might have already been created by Rock
    DELETE [EntityType]
    WHERE [Name] = 'Rock.Workflow.Action.AddNote'
    AND [Guid] <> 'F6532683-9797-4F16-8DB7-8B413EDC8AD7'

    -- Rename the entity type
    UPDATE [EntityType] SET
	    [Name] = 'Rock.Workflow.Action.AddNote',
	    [AssemblyName] = 'Rock.Workflow.Action.AddNote, Rock, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Add Note',
	    [Guid] = 'F6532683-9797-4F16-8DB7-8B413EDC8AD7'
    WHERE [Name] = 'Rock.Workflow.Action.AddWorkflowNote'
" );
        }
    }
}
