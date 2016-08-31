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
    public partial class AddWorkflowTypeIcon : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowType", "IconCssClass", c => c.String());
            Sql( @"UPDATE [WorkflowType] SET [IconCssClass] = 'fa fa-list-ol'" );

            // Grant Staff and Staff-Like roles edit access to the Known Relationships group type
            RockMigrationHelper.DeleteSecurityAuthForGroupType("E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF");
            RockMigrationHelper.AddSecurityAuthForGroupType( "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "C463D46C-DE81-4B78-828F-D219198DCB94" );
            RockMigrationHelper.AddSecurityAuthForGroupType( "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "2F714BE7-FC9C-4DE6-9F3B-C5616F77ED71" );

            Sql( @"
    IF OBJECT_ID('[dbo].[ufnUtility_GetPersonIdFromPersonAlias]', 'FN') IS NOT NULL
      DROP FUNCTION [dbo].[ufnUtility_GetPersonIdFromPersonAlias]
" );
            Sql( @"
    /*
    <doc>
	    <summary>
 		    This function returns the person id for the person alias given.
	    </summary>

	    <returns>
		    Int of the person id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPersonIdFromPersonAlias](1)
	    </code>
    </doc>
    */

    CREATE FUNCTION [dbo].[ufnUtility_GetPersonIdFromPersonAlias](@PersonAlias int) 

    RETURNS int AS

    BEGIN

	    RETURN (SELECT [PersonId] FROM PersonAlias WHERE [Id] = @PersonAlias)
    END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.WorkflowType", "IconCssClass");
        }
    }
}
