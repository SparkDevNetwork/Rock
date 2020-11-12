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
    public partial class WorkflowActionFormChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Drop the default constraint on WorkflowActionForm.PersonEntryHideIfCurrentPersonKnown if an older version of the 202011052358368_WorkflowActionFormAllowPersonEntry migration was run
            Sql( @"
DECLARE @Sql NVARCHAR(max)
    ,@constraintName NVARCHAR(max) = (
        SELECT TOP 1 obj_Constraint.NAME AS 'constraint'
        FROM sys.objects obj_table
        JOIN sys.objects obj_Constraint ON obj_table.object_id = obj_Constraint.parent_object_id
        JOIN sys.sysconstraints constraints ON constraints.constid = obj_Constraint.object_id
        JOIN sys.columns columns ON columns.object_id = obj_table.object_id AND columns.column_id = constraints.colid
        WHERE obj_table.NAME = 'WorkflowActionForm' AND columns.name = 'PersonEntryHideIfCurrentPersonKnown' AND obj_Constraint.type = 'D'
        )

SELECT @constraintName

IF @constraintName IS NOT NULL
BEGIN
    SET @Sql = CONCAT (
            'ALTER TABLE [dbo].[WorkflowActionForm] DROP CONSTRAINT ['
            ,@constraintName
            ,']'
            )

    EXECUTE (@Sql)

    UPDATE WorkflowActionForm
    SET PersonEntryHideIfCurrentPersonKnown = 0
    WHERE AllowPersonEntry = 0 AND PersonEntryHideIfCurrentPersonKnown = 1
END
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
