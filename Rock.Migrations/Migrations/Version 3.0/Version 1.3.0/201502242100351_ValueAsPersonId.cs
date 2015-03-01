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
    public partial class ValueAsPersonId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            RockMigrationHelper.UpdateEntityType( "Rock.Model.PersonAlias", "90F5E87B-F0D5-4617-8AE9-EB57E673F36F", true, false );
            Sql( @"
    DECLARE @PersonEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7' )
    DECLARE @PersonAliasEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '90F5E87B-F0D5-4617-8AE9-EB57E673F36F' )

    UPDATE F SET
	    [EntityTypeId] = @PersonAliasEntityTypeId,
	    [EntityId] = A.[Id]
    FROM [Following] F
    INNER JOIN [PersonAlias] A
	    ON A.[PersonId] = F.[EntityId]
	    AND A.[AliasPersonId] = F.[EntityId]
    WHERE F.[EntityTypeId] = @PersonEntityTypeId
" );

            Sql( @"
    /*
    <doc>
	    <summary>
 		    This function returns the person id for the person alias guid given.
	    </summary>

	    <returns>
		    Int of the person id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid]('58DF3F13-B96E-4682-A14F-D411187CEBBA')
	    </code>
    </doc>
    */

    CREATE FUNCTION [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid](@PersonAliasGuid uniqueidentifier) 

    RETURNS int AS

    BEGIN
	    RETURN (SELECT TOP 1 [PersonId] FROM PersonAlias WHERE [Guid] = @PersonAliasGuid)
    END
" );

            Sql( @"
    ALTER TABLE [AttributeValue] ADD [ValueAsPersonId] AS (
	    case when [Value] like '________-____-____-____-____________' then [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid]([Value]) else null end
    )
" );

            // MP: School Grades Help Text
            Sql( @"
    UPDATE [DefinedType] set [HelpText] = '<p>
        When adjusting the grades, the first thing to keep in mind is that Rock only stores the year that someone graduates from the educational system.
        In the US, that''s their high school graduation. Rock dynamically calculates a person''s grade by:
        
        <ol>
            <li>
                Comparing the current date to their graduation year which provides an offset in years 
                (Rock also uses the Grade Transition Date Global Attribute to help determine the start of the school year).
            </li>
            <li>
                The year offset from step 1 is then compared with the grade defined values below. The first Defined Value 
                (grade) whose value is greater than or equal to the offset is selected. For systems that have one grade for each year, this is a simple 
                setup. The last grade (senior year in the US) would have a value of 0, the next (junior) a value of 1, etc. For systems where a grade 
                spans multiple years, you would ""skip"" years.  For example, to have a Middle School
                grade level instead of separate 7th and 8th grades, you would set the Value
                of Middle School to 5, and the next higher grade level (Freshman) to 3.
            </li>
        </ol>
    </p>' WHERE [Guid] = '24E5A79F-1E62-467A-AD5D-0D10A2328B4D'
" );

            //DT: Give Finance Worker edit access to edit batches/transactions
            RockMigrationHelper.AddSecurityAuthForPage( "EF65EFF2-99AC-4081-8E09-32A04518683A", 0, "Edit", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", 0, "DF002451-BB8F-4A98-839D-ECBF23FE372A" );

            Sql( @"
    UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/ContentChannelView.ascx', [Name] = 'Content Channel View'  WHERE [Guid] = '143A2345-3E26-4ED0-A2FE-42AAF11B4C0F'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DROP FUNCTION [ufnUtility_GetPersonIdFromPersonAliasGuid]
" );
            
            DropColumn("dbo.AttributeValue", "ValueAsPersonId");


        }
    }
}
