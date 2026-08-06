SELECT
    [oc].[Id],
    ISNULL( [p].[InternalName], '(no page)' ) AS [Page],
    ISNULL( [b].[Name], '(no block)' ) AS [BlockName],
    DATALENGTH( [oc].[Source] ) / 2 AS [SourceChars],
    CAST( DATALENGTH( [oc].[Source] ) / 2048.0 AS DECIMAL(10,2) ) AS [SourceKB],
    DATALENGTH( [oc].[CompiledContent] ) / 2 AS [CompiledChars],
    CAST( DATALENGTH( [oc].[CompiledContent] ) / 2048.0 AS DECIMAL(10,2) ) AS [CompiledKB],
    CAST( CASE
        WHEN ISNULL( DATALENGTH( [oc].[Source] ), 0 ) = 0 THEN NULL
        ELSE DATALENGTH( [oc].[CompiledContent] ) * 1.0 / DATALENGTH( [oc].[Source] )
    END AS DECIMAL(10,2) ) AS [ExpansionRatio],
    -- Measured on Jint 4.15.3: roughly 450 ms fixed plus 120 ms per KB of source.
    CAST( 450 + ( DATALENGTH( [oc].[Source] ) / 2048.0 * 120 ) AS INT ) AS [EstCompileMs],
    [oc].[CompiledVueVersion] AS [VueVersion],
    [oc].[CompiledDateTime],
    CASE
        WHEN [oc].[Source] IS NULL OR DATALENGTH( [oc].[Source] ) = 0 THEN 'No source'
        WHEN [oc].[CompiledContent] IS NULL THEN 'NOT COMPILED (renders blank)'
        ELSE 'Compiled'
    END AS [Status]
FROM [ObsidianContent] AS [oc]
LEFT JOIN [Block] AS [b] ON [b].[Id] = [oc].[BlockId]
LEFT JOIN [Page] AS [p] ON [p].[Id] = [b].[PageId]
ORDER BY DATALENGTH( [oc].[Source] ) DESC;
