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
    public partial class Rollup_20240905 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveEmojisUp();
            EnablePageViewsForSelfServiceKioskUp();
            RenameFieldCategorizedDefinedValueFieldTypeUp();

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameFieldCategorizedDefinedValueFieldTypeDown();
        }

        #region KH: Remove Emojis From Person Names

        private void RemoveEmojisUp()
        {
            Sql( @"
CREATE OR ALTER FUNCTION dbo.RemoveEmojis(@InputString NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @StartIndex INT = 1;
    DECLARE @CurrentChar NCHAR(1);
    DECLARE @NextChar NCHAR(1);
    DECLARE @CurrentCode INT;
    DECLARE @NextCode INT;
    DECLARE @Result NVARCHAR(MAX) = N'';
    DECLARE @IsEmoji BIT = 0;

    WHILE @StartIndex <= LEN(@InputString)
    BEGIN
        SET @CurrentChar = SUBSTRING(@InputString, @StartIndex, 1);
        SET @NextChar = SUBSTRING(@InputString, @StartIndex + 1, 1);
        SET @CurrentCode = UNICODE(@CurrentChar);
        SET @NextCode = UNICODE(@NextChar);

        SET @IsEmoji = CASE
            WHEN @CurrentCode BETWEEN 127744 AND 129750 THEN 1
            WHEN @CurrentCode BETWEEN 126980 AND 127569 THEN 1
            WHEN @CurrentCode BETWEEN 0xD800 AND 0xDBFF AND @NextCode BETWEEN 0xDC00 AND 0xDFFF THEN 1
            WHEN @CurrentCode = 0x200D THEN 1
            WHEN @CurrentCode = 0xFE0F THEN 1
            WHEN @CurrentCode BETWEEN 8205 AND 8252 THEN 1
            WHEN @CurrentCode BETWEEN 9728 AND 10175 THEN 1
            ELSE 0
        END

        IF @IsEmoji = 0
        BEGIN
            SET @Result = @Result + @CurrentChar;
        END
        ELSE IF @CurrentCode BETWEEN 0xD800 AND 0xDBFF AND @NextCode BETWEEN 0xDC00 AND 0xDFFF
        BEGIN
            SET @StartIndex = @StartIndex + 1;
        END

        SET @StartIndex = @StartIndex + 1;
    END

    RETURN @Result;
END
" );

            Sql( @"
-- For efficiency and avoiding having to create a loop, a temp table will be created that needs to contain as many rows as we have concatenate characters.
-- 50 (FirstName) + 50 (LastName) + 50 (MiddleName) + 50 (NickName) = 200

-- Use common table expression to insert N rows using recursion
DECLARE @row_table TABLE(id int)
;WITH cteInsertNRows(n) AS 
(
	SELECT 1 
	UNION ALL
	SELECT n + 1
	FROM cteInsertNRows WHERE n < 200
)
INSERT INTO @row_table SELECT * FROM cteInsertNRows
OPTION (MAXRECURSION 200)

;WITH PersonNames AS (
    SELECT
        Id,
        FirstName,
        NickName,
        MiddleName,
        LastName
    FROM
        Person
),
CharCheck AS (
    SELECT
        Id,
        FirstName,
        NickName,
        MiddleName,
        LastName,
        UNICODE(SUBSTRING(FirstName, Number, 1)) AS CodePointFN,
        UNICODE(SUBSTRING(NickName, Number, 1)) AS CodePointNN,
        UNICODE(SUBSTRING(MiddleName, Number, 1)) AS CodePointMN,
        UNICODE(SUBSTRING(LastName, Number, 1)) AS CodePointLN,
        Number
    FROM
        PersonNames
    CROSS APPLY (
        SELECT TOP (
            LEN(ISNULL(FirstName,'')) +
            LEN(ISNULL(NickName,'')) +
            LEN(ISNULL(MiddleName,'')) +
            LEN(ISNULL(LastName,''))
        )
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Number
		FROM @row_table
    ) AS Numbers
)
UPDATE Person
SET
    FirstName = dbo.RemoveEmojis(CharCheck.FirstName),
    NickName = dbo.RemoveEmojis(CharCheck.NickName),
    MiddleName = dbo.RemoveEmojis(CharCheck.MiddleName),
    LastName = dbo.RemoveEmojis(CharCheck.LastName)
FROM
    Person
    INNER JOIN CharCheck ON Person.Id = CharCheck.Id
WHERE
    -- Check for emoji ranges
    (CodePointFN BETWEEN 127744 AND 129750 OR CodePointFN BETWEEN 126980 AND 127569 OR
     CodePointNN BETWEEN 127744 AND 129750 OR CodePointNN BETWEEN 126980 AND 127569 OR
     CodePointMN BETWEEN 127744 AND 129750 OR CodePointMN BETWEEN 126980 AND 127569 OR
     CodePointLN BETWEEN 127744 AND 129750 OR CodePointLN BETWEEN 126980 AND 127569)
    OR
    -- Check for surrogate pairs
    (CodePointFN BETWEEN 0xD800 AND 0xDBFF AND UNICODE(SUBSTRING(CharCheck.FirstName, Number + 1, 1)) BETWEEN 0xDC00 AND 0xDFFF) OR
    (CodePointNN BETWEEN 0xD800 AND 0xDBFF AND UNICODE(SUBSTRING(CharCheck.NickName, Number + 1, 1)) BETWEEN 0xDC00 AND 0xDFFF) OR
    (CodePointMN BETWEEN 0xD800 AND 0xDBFF AND UNICODE(SUBSTRING(CharCheck.MiddleName, Number + 1, 1)) BETWEEN 0xDC00 AND 0xDFFF) OR
    (CodePointLN BETWEEN 0xD800 AND 0xDBFF AND UNICODE(SUBSTRING(CharCheck.LastName, Number + 1, 1)) BETWEEN 0xDC00 AND 0xDFFF)
    OR
    -- Check for Zero Width Joiner and Variation Selector-16
    CodePointFN = 0x200D OR CodePointFN = 0xFE0F OR
    CodePointNN = 0x200D OR CodePointNN = 0xFE0F OR
    CodePointMN = 0x200D OR CodePointMN = 0xFE0F OR
    CodePointLN = 0x200D OR CodePointLN = 0xFE0F;

DROP FUNCTION dbo.RemoveEmojis
" );
        }

        #endregion

        #region TL (NA): Add Migration to Enable Page Views for the "Self-Service Kiosk (Preview)" site (in v16.7)

        private void EnablePageViewsForSelfServiceKioskUp()
        {
            Sql( @"UPDATE [Site] SET [EnablePageViews] = 1 WHERE [Guid] = '05e96f7b-b75e-4987-825a-b6f51f8d9caa'" );
        }

        #endregion

        #region PA: Rename Categorized Defined Value to Defined Value (Categorized)

        private void RenameFieldCategorizedDefinedValueFieldTypeUp()
        {
            RockMigrationHelper.UpdateFieldType( "Defined Value (Categorized)", "", "Rock", "Rock.Field.Types.CategorizedDefinedValueFieldType", "3217C31F-85B6-4E0D-B6BE-2ADB0D28588D" );
        }

        private void RenameFieldCategorizedDefinedValueFieldTypeDown()
        {
            RockMigrationHelper.UpdateFieldType( "Categorized Defined Value", "", "Rock", "Rock.Field.Types.CategorizedDefinedValueFieldType", "3217C31F-85B6-4E0D-B6BE-2ADB0D28588D" );
        }

        #endregion
    }
}
