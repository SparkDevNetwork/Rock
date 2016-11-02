IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_FinancialTransaction]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_FinancialTransaction
GO

--
CREATE PROCEDURE [dbo].[spAnalytics_ETL_FinancialTransaction]
AS
BEGIN
    SELECT NULL
END
