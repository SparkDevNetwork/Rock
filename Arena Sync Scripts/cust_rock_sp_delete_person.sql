CREATE PROC [dbo].[cust_rock_sp_delete_person]
@PersonId int

AS

DECLARE @Id int

SELECT
	 @Id = [foreign_key]
FROM [core_person] WITH (NOLOCK)
WHERE [person_id] = @PersonId

IF @Id IS NOT NULL
BEGIN

	DELETE [RockChMS].[dbo].[corePerson] 
	WHERE [Id] = @Id
	
END
