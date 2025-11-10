-- Add a temporary table to track reserved slugs
CREATE TABLE #UsedSlugs (
    Slug NVARCHAR(400)
);

-- Update WorkflowTypes one at a time to ensure unique slugs
DECLARE @Id INT;
DECLARE @Name NVARCHAR(400);
DECLARE @BaseSlug NVARCHAR(400);
DECLARE @Slug NVARCHAR(400);
DECLARE @Suffix INT;
DECLARE @MaxLength INT = 400;
DECLARE @MaxSlugGenerationAttempts INT = 999;
DECLARE @SuffixText NVARCHAR(10);
DECLARE @BaseLength INT;

DECLARE WorkflowCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT Id, Name
    FROM WorkflowType
    WHERE Slug IS NULL OR Slug = '';

OPEN WorkflowCursor;
FETCH NEXT FROM WorkflowCursor INTO @Id, @Name;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Step 1: Normalize and clean up the slug (similar to MakeSlugValid)
    SET @Slug = LOWER(LTRIM(RTRIM(@Name)));

    -- Replace common HTML entities and symbols with hyphens
    SET @Slug = REPLACE(@Slug, '&nbsp;', '-');
    SET @Slug = REPLACE(@Slug, '&#160;', '-');
    SET @Slug = REPLACE(@Slug, '&ndash;', '-');
    SET @Slug = REPLACE(@Slug, '&#8211;', '-');
    SET @Slug = REPLACE(@Slug, '&mdash;', '-');
    SET @Slug = REPLACE(@Slug, '&#8212;', '-');
    SET @Slug = REPLACE(@Slug, '_', '-');
    SET @Slug = REPLACE(@Slug, ' ', '-');

    -- Remove invalid characters (keeping only a-z, 0-9, and -)
    -- This uses a trick with XML to strip unwanted characters
    WHILE PATINDEX('%[^a-z0-9-]%', @Slug) > 0
        SET @Slug = STUFF(@Slug, PATINDEX('%[^a-z0-9-]%', @Slug), 1, '');

    -- Remove multiple hyphens
    WHILE CHARINDEX('--', @Slug) > 0
        SET @Slug = REPLACE(@Slug, '--', '-');

    -- Trim trailing hyphens
    WHILE RIGHT(@Slug, 1) = '-'
        SET @Slug = LEFT(@Slug, LEN(@Slug) - 1);

    -- Ensure length does not exceed max
    IF LEN(@Slug) > @MaxLength
        SET @Slug = LEFT(@Slug, @MaxLength);

    -- Fallback to workflowtype-<id> if the sanitized name resolves to an empty string
    IF @Slug = ''
        SET @Slug = 'workflowtype-' + CAST(@Id AS NVARCHAR);

    SET @BaseSlug = @Slug;
    SET @Suffix = 0;

    -- Step 2: Ensure uniqueness
    WHILE EXISTS (
        SELECT 1 FROM WorkflowType WHERE Slug = @Slug AND Id != @Id
        UNION
        SELECT 1 FROM #UsedSlugs WHERE Slug = @Slug
    )
    BEGIN
        -- Prevent infinite slug suffix attempts
        IF @Suffix > @MaxSlugGenerationAttempts 
        BEGIN
            RAISERROR('Could not generate unique slug for WorkflowType Id = %d (Base: %s)', 16, 1, @Id, @BaseSlug);
            BREAK;
        END

        SET @Suffix = @Suffix + 1;
        SET @SuffixText = '-' + CAST(@Suffix AS NVARCHAR);
        SET @BaseLength = @MaxLength - LEN(@SuffixText);

        SET @Slug = LEFT(@BaseSlug, @BaseLength) + @SuffixText;
    END

    -- Update the record
    UPDATE WorkflowType
    SET Slug = @Slug
    WHERE Id = @Id;

    -- Reserve the slug
    INSERT INTO #UsedSlugs (Slug) VALUES (@Slug);

    FETCH NEXT FROM WorkflowCursor INTO @Id, @Name;
END

CLOSE WorkflowCursor;
DEALLOCATE WorkflowCursor;

DROP TABLE #UsedSlugs;