// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.DpsMatch.Migrations
{
    [MigrationNumber( 5, "1.4.0" )]
    public class NoteTypeAndGroup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @NoteTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.NoteType' )
    IF @NoteTypeEntityTypeId IS NOT NULL 
    BEGIN

        DECLARE @PersonEntityTypeId INT
	    SET @PersonEntityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

        INSERT INTO [NoteType]
		    ([IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [CssClass], [IconCssClass], [Order])
	    VALUES
		    (0, @PersonEntityTypeId, 'Offender Alert', '961F352B-40A0-4446-8D56-4C570522F7EB', 0, 'note-danger', 'fa fa-exclamation-triangle', '4')
	    
		INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id], 3, 'View', 'D', 1, NULL, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id], 3, 'Edit', 'D', 1, NULL, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id],3, 'Administrate', 'D', 1, NULL, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

         DECLARE @SecurityRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB' )
		INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id], 1, 'View', 'A', 0, @SecurityRoleId, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id], 1, 'Edit', 'A', 0, @SecurityRoleId, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id],1, 'Administrate', 'A', 0, @SecurityRoleId, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        DECLARE @AdminRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
		INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id], 0, 'View', 'A', 0, @AdminRoleId, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id], 0, 'Edit', 'A', 0, @AdminRoleId, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

        INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		SELECT @NoteTypeEntityTypeId, [Id],0, 'Administrate', 'A', 0, @AdminRoleId, NEWID()
		FROM [NoteType]
        Where [Guid] = '961F352B-40A0-4446-8D56-4C570522F7EB'

    END
" );
            RockMigrationHelper.AddBlockTypeAttribute( "DE2ACACA-7839-47C9-AB79-C02E2CF5ECB5", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Offender Group", "OffenderGroup", "", "The Group for Offenders", 0, @"", "62624CF6-DDEA-4999-93EE-BA69C3A46C7C" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "62624CF6-DDEA-4999-93EE-BA69C3A46C7C" );
        }
    }
}
