-- =============================================
ALTER FUNCTION [dbo].[_church_ccv_ufnUtility_GetGeneralFinanceAccountIds]()
RETURNS TABLE 
AS
RETURN 
(
	SELECT Id
	FROM FinancialAccount
	WHERE id IN (
		 498 -- Peoria
		,609 -- Surprise
		,690 -- Scottsdale
		,708 -- East Valley
		,727 -- Anthem
		,745 -- Avondale
	)
)