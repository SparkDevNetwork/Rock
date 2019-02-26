namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 68, "1.8.6" )]
    public class MigrationRollupsForV8_7_5 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            CreatePersonGivingLeaderIdPersistedIndexed();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// Re-creates the person GivingLeaderID column as a computed-persisted column and adds an index on it
        /// </summary>
        private void CreatePersonGivingLeaderIdPersistedIndexed()
        {
            Sql( @"IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_GivingLeaderId'
			AND object_id = OBJECT_ID('Person')
		)
BEGIN
	DROP INDEX IX_GivingLeaderId ON [Person]
END

ALTER TABLE Person
DROP COLUMN GivingLeaderId

ALTER TABLE Person ADD GivingLeaderId INT NULL

CREATE INDEX IX_GivingLeaderId ON [Person] ([GivingLeaderId])

UPDATE x
SET x.GivingLeaderId = x.CalculatedGivingLeaderId
FROM (
	SELECT p.Id
		,p.NickName
		,p.LastName
		,p.GivingLeaderId
		,isnull(pf.CalculatedGivingLeaderId, p.Id) CalculatedGivingLeaderId
	FROM Person p
	OUTER APPLY (
		SELECT TOP 1 p2.[Id] CalculatedGivingLeaderId
		FROM [GroupMember] gm
		INNER JOIN [GroupTypeRole] r ON r.[Id] = gm.[GroupRoleId]
		INNER JOIN [Person] p2 ON p2.[Id] = gm.[PersonId]
		WHERE gm.[GroupId] = p.GivingGroupId
			AND p2.[IsDeceased] = 0
			AND p2.[GivingGroupId] = p.GivingGroupId
		ORDER BY r.[Order]
			,p2.[Gender]
			,p2.[BirthYear]
			,p2.[BirthMonth]
			,p2.[BirthDay]
		) pf
	WHERE (
			p.GivingLeaderId IS NULL
			OR (p.GivingLeaderId != pf.CalculatedGivingLeaderId)
			)
	) x" );
        }

    }
}
