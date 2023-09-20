using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace org.lakepointe.Checkin.Migrations
{
    [MigrationNumber( 1, "1.8.1.0" )]
    public class AddKnownRelationshipTypes : Migration
    {

        public override void Up()
        {
            RockMigrationHelper.AddGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS, "Can Temporarily Check-in", "A person that can be checked in by the owner of this known relationship group. This relationship will be automatically deleted at the end of the day.", 99, null, null, "EFAEE6AE-6889-43D8-84F2-25154AACEF69", false, false, false );
            RockMigrationHelper.AddGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS, "Allow Temporary Check-in by", "A person that can check in the owner of this known relationship group. This relationship will be deleted at the end of the day.", 99, null, null, "A7942CD0-E2BF-40AF-9127-0B3C21FBC7DF", false, false, false );

            Sql( @"
DECLARE @CanTemporaryCheckinId INT = 
(
	SELECT 
		[Id]
	FROM 
		[GroupTypeRole]
	WHERE 
		[Guid] = 'EFAEE6AE-6889-43D8-84F2-25154AACEF69'
)

DECLARE @AllowTempCheckinById INT = 
(
	SELECT
		[Id]
	FROM
		[GroupTypeRole]
	WHERE 
		[Guid] = 'A7942CD0-E2BF-40AF-9127-0B3C21FBC7DF'
)

DECLARE @CanCheckinAttributeId INT =
(
	SELECT
		[Id]
	FROM
		[Attribute]
	WHERE 
		[Guid] = '610A5BE8-8FDE-46AA-8F9D-1AF7F1F23441'
)

DECLARE @InverseRelationshipAttributeId INT = 
(
	SELECT
		[Id]
	FROM
		[Attribute]
	WHERE
		[Guid] = 'C91148D9-D663-493A-86E8-5000BD281852'
)

IF NOT EXISTS(SELECT * FROM [AttributeValue] WHERE AttributeId = @CanCheckinAttributeId AND EntityId = @CanTemporaryCheckinId)
BEGIN
	INSERT INTO AttributeValue 
	(
		[IsSystem],
		[AttributeId],
		[EntityId], 
		[Value],
		[Guid],
		[CreatedDateTime],
		[ModifiedDateTime],
		[CreatedByPersonAliasId],
		[ModifiedByPersonAliasId]
	)
	VALUES
	(
		0,
		@CanCheckinAttributeId,
		@CanTemporaryCheckinId,
		'True',
		'A30446E2-5619-4772-BA14-0D70FFD5759D',
		GETDATE(),
		GETDATE(),
		null,
		null
	)
END

IF NOT EXISTS(SELECT * FROM [AttributeValue] WHERE AttributeId = @CanCheckinAttributeId AND EntityId = @AllowTempCheckinById)
BEGIN
	INSERT INTO AttributeValue 
	(
		[IsSystem],
		[AttributeId],
		[EntityId], 
		[Value],
		[Guid],
		[CreatedDateTime],
		[ModifiedDateTime],
		[CreatedByPersonAliasId],
		[ModifiedByPersonAliasId]
	)
	VALUES
	(
		0,
		@CanCheckinAttributeId,
		@AllowTempCheckinById,
		'True',
		'471B7E50-DD42-43B1-B56A-DF602F135B62',
		GETDATE(),
		GETDATE(),
		null,
		null
	)
END

IF NOT EXISTS(SELECT * FROM [AttributeValue] WHERE AttributeId = @InverseRelationshipAttributeId AND EntityId = @CanCheckinAttributeId)
BEGIN
	INSERT INTO AttributeValue 
	(
		[IsSystem],
		[AttributeId],
		[EntityId], 
		[Value],
		[Guid],
		[CreatedDateTime],
		[ModifiedDateTime],
		[CreatedByPersonAliasId],
		[ModifiedByPersonAliasId]
	)
	VALUES
	(
		0,
		@InverseRelationshipAttributeId,
		@CanTemporaryCheckinId,
		'a7942cd0-e2bf-40af-9127-0b3c21fbc7df',
		'18B6E1A9-848C-4017-8493-0046CDDB0205',
		GETDATE(),
		GETDATE(),
		null,
		null
	)
END

IF NOT EXISTS(SELECT * FROM [AttributeValue] WHERE AttributeId = @InverseRelationshipAttributeId AND EntityId = @AllowTempCheckinById)
BEGIN
	INSERT INTO AttributeValue 
	(
		[IsSystem],
		[AttributeId],
		[EntityId], 
		[Value],
		[Guid],
		[CreatedDateTime],
		[ModifiedDateTime],
		[CreatedByPersonAliasId],
		[ModifiedByPersonAliasId]
	)
	VALUES
	(
		0,
		@InverseRelationshipAttributeId,
		@AllowTempCheckinById,
		'efaee6ae-6889-43d8-84f2-25154aacef69',
		'6CC7E4E6-AFF6-47CA-AAB4-19A501865B08',
		GETDATE(),
		GETDATE(),
		null,
		null
	)
END" );

        }

        public override void Down()
        {
            // Delete Attribute Values associated with GroupTypeRoles
            Sql(
            @"  DELETE FROM [AttributeValue] WHERE [Guid] = 'A30446E2-5619-4772-BA14-0D70FFD5759D'
                DELETE FROM [AttributeValue] WHERE [Guid] = '471B7E50-DD42-43B1-B56A-DF602F135B62'
                DELETE FROM [AttributeValue] WHERE [Guid] = '18B6E1A9-848C-4017-8493-0046CDDB0205'
                DELETE FROM [AttributeValue] WHERE [Guid] = '6CC7E4E6-AFF6-47CA-AAB4-19A501865B08'"
                );

            // Delete Relationship GroupMembers that use these GroupRoleIds
            Sql(
            @"  DECLARE @CanTemporaryCheckinId INT = 
                (
	                SELECT 
		                [Id]
	                FROM 
		                [GroupTypeRole]
	                WHERE 
		                [Guid] = 'EFAEE6AE-6889-43D8-84F2-25154AACEF69'
                )

                DECLARE @AllowTempCheckinById INT = 
                (
	                SELECT
		                [Id]
	                FROM
		                [GroupTypeRole]
	                WHERE 
		                [Guid] = 'A7942CD0-E2BF-40AF-9127-0B3C21FBC7DF'
                )

                DELETE FROM [GroupMember] WHERE [GroupRoleId] = @CanTemporaryCheckinId
                DELETE FROM [GroupMember] WHERE [GroupRoleId] = @AllowTempCheckinById "
            );

            //Delete GroupTypeRoles
            RockMigrationHelper.DeleteGroupTypeRole( "EFAEE6AE-6889-43D8-84F2-25154AACEF69" );
            RockMigrationHelper.DeleteGroupTypeRole( "A7942CD0-E2BF-40AF-9127-0B3C21FBC7DF" );
        }

    }
}
