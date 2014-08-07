/*
<doc>
	<summary>
 		This stored procedure detects potential duplicate person records and stores the results in [PersonDuplicate]
	</summary>
	
	<remarks>	
		Uses the following constants:
			##TODO##
	</remarks>
	<code>
		EXEC [dbo].[spCrm_PersonDuplicateFinder]
	</code>
</doc>
*/

SET STATISTICS TIME ON

/*
ALTER PROCEDURE [dbo].[spCrm_PersonDuplicateFinder]
AS
*/
BEGIN
 
/* Find Duplicates by looking at people with the exact same email address  */   
    DECLARE @PersonDuplicateByEmailTable TABLE  (
      Id INT NOT NULL IDENTITY(1,1),
      Email NVARCHAR(MAX) NOT NULL,
      PersonAliasId INT NOT NULL
      );   

    INSERT INTO @PersonDuplicateByEmailTable
    SELECT 
        e.Email [Email] 
        ,pa.Id [PersonAliasId]
    FROM
    (
        SELECT 
            [a].[Email]
        FROM
        (
            SELECT 
                [Email]
                ,COUNT(*) [EmailCount]
            FROM 
                [Person] [p]
            WHERE 
                [Email] IS NOT NULL 
            AND
                [Email] != ''
            GROUP BY 
                [Email]
        ) [a]
        WHERE [a].[EmailCount] > 1
    ) [e]
    JOIN Person p on p.Email = e.Email
    JOIN PersonAlias pa on pa.PersonId = p.Id
    order by Email

    MERGE [PersonDuplicate] as target
    USING (
        SELECT 
            a.PersonAliasId,
            b.PersonAliasId [DuplicatePersonAliasId],
            a.Email [Email] 
        FROM 
            @PersonDuplicateByEmailTable a
        join @PersonDuplicateByEmailTable b on a.email = b.email
        and b.Id != a.Id  
        ) AS source (PersonAliasId, DuplicatePersonAliasId, Email)
    ON (target.PersonAliasId = source.PersonAliasId) and (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED THEN
        UPDATE SET [Score] = 2, [ScoreDetail] = 'Email:' + source.Email
        WHEN NOT MATCHED THEN
            INSERT (PersonAliasId, DuplicatePersonAliasId, Score, ScoreDetail, IsConfirmedAsNotDuplicate, [Guid])
            VALUES (source.PersonAliasId, source.DuplicatePersonAliasId, 2, 'Email:' + source.Email, 0, NEWID());




END

/*
select * from PersonDuplicate order by ScoreDetail
*/

