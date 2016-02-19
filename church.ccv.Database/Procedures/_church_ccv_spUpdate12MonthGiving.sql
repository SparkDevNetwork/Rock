/*
<doc>
	<summary>
 		This stored proc updates the 'Giving In Last 12 Months' person attribute.
	</summary>

	<returns>
		
	</returns>
	
	<remarks>	
		
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spUpdate12MonthGiving] 'GivingInLast12Months'
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[_church_ccv_spUpdate12MonthGiving]
	@AttributeKey varchar(50)
AS
BEGIN

	-- delete all current attribute values
	DELETE FROM [AttributeValue] 
		WHERE [AttributeId] = (SELECT TOP 1 [Id] 
									FROM [Attribute]
									WHERE [Key] = @AttributeKey 
										AND [EntityTypeId] = 15 -- person
							   )
	
	DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = @AttributeKey AND [EntityTypeId] = 15) -- person

	-- insert new values
	INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
	SELECT 0, @AttributeId, p.[Id], gs.[GivingAmount], newid()
	FROM [Person] p
	INNER JOIN
		(SELECT p.[GivingId], SUM(ftd.[Amount]) AS [GivingAmount]  
			FROM [FinancialTransactionDetail] ftd
				INNER JOIN [FinancialTransaction] ft ON ft.[Id] = ftd.[TransactionId]
				INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
				INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
			WHERE 
				ftd.[AccountId] in (498, 609, 690, 708, 745, 727) -- UPDATE GENERAL FUND LIST HERE
				AND ft.[TransactionDateTime] >= DATEADD(year, -1, getdate())
			GROUP BY p.[GivingId]
		) gs ON gs.[GivingId] = p.[GivingId]
		

END