-- Generate INSERTs for AISkill
SET NOCOUNT ON;
DECLARE @Sql NVARCHAR(MAX) = '';
SELECT @Sql = @Sql + CHAR(13) + CHAR(10) + 'SET IDENTITY_INSERT [dbo].[AISkill] ON;'

SELECT @Sql = @Sql + CHAR(13) + CHAR(10) +
    'INSERT INTO [dbo].[AISkill] ([Id], [Name], [Description], [UsageHint], [CodeEntityTypeId], [AdditionalSettingsJson], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignId], [ForeignGuid], [ForeignKey]) VALUES (' +
    CAST([Id] AS VARCHAR) + ', ' +
    'N''' + REPLACE(ISNULL([Name], ''), '''', '''''') + ''', ' +
    'N''' + REPLACE(ISNULL([Description], ''), '''', '''''') + ''', ' +
    'N''' + REPLACE(ISNULL([UsageHint], ''), '''', '''''') + ''', ' +
    'NULL' + ', ' +
    'N''' + REPLACE(ISNULL([AdditionalSettingsJson], ''), '''', '''''') + ''', ' +
    'NULL' + ', ' +
    'NULL' + ', ' +
    'NULL' + ', ' +
    'NULL' + ', ' +
    'N''' + ISNULL(CONVERT(VARCHAR(36), [Guid]), '') + ''', ' +
    COALESCE(CAST([ForeignId] AS VARCHAR), 'NULL') + ', ' +
    CASE WHEN [ForeignGuid] IS NULL THEN 'NULL' ELSE 'N''' + CONVERT(VARCHAR(36), [ForeignGuid]) + '''' END + ', ' +
    'N''' + REPLACE(ISNULL([ForeignKey], ''), '''', '''''') + '''' +
    ');'
FROM [dbo].[AISkill]
WHERE [CodeEntityTypeId] IS NULL;

SELECT @Sql = @Sql + CHAR(13) + CHAR(10) + 'SET IDENTITY_INSERT [dbo].[AISkill] OFF;'

-- Generate INSERTs for AISkillFunction
SELECT @Sql = @Sql + CHAR(13) + CHAR(10) + 'SET IDENTITY_INSERT [dbo].[AISkillFunction] ON;'

SELECT @Sql = @Sql + CHAR(13) + CHAR(10) +
    'INSERT INTO [dbo].[AISkillFunction] ([Id], [AISkillId], [Name], [Description], [UsageHint], [FunctionType], [AdditionalSettingsJson], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignId], [ForeignGuid], [ForeignKey]) VALUES (' +
    CAST([Id] AS VARCHAR) + ', ' +
    CAST([AISkillId] AS VARCHAR) + ', ' +
    'N''' + REPLACE(ISNULL([Name], ''), '''', '''''') + ''', ' +
    'N''' + REPLACE(ISNULL([Description], ''), '''', '''''') + ''', ' +
    'N''' + REPLACE(ISNULL([UsageHint], ''), '''', '''''') + ''', ' +
    COALESCE(CAST([FunctionType] AS VARCHAR), 'NULL') + ', ' +
    'N''' + REPLACE(ISNULL([AdditionalSettingsJson], ''), '''', '''''') + ''', ' +
    'NULL' + ', ' +
    'NULL' + ', ' +
    'NULL' + ', ' +
    'NULL' + ', ' +
    'N''' + ISNULL(CONVERT(VARCHAR(36), [Guid]), '') + ''', ' +
    COALESCE(CAST([ForeignId] AS VARCHAR), 'NULL') + ', ' +
    CASE WHEN [ForeignGuid] IS NULL THEN 'NULL' ELSE 'N''' + CONVERT(VARCHAR(36), [ForeignGuid]) + '''' END + ', ' +
    'N''' + REPLACE(ISNULL([ForeignKey], ''), '''', '''''') + '''' +
    ');'
FROM [dbo].[AISkillFunction]
WHERE [AISkillId] IN (SELECT [Id] FROM [dbo].[AISkill] WHERE [CodeEntityTypeId] IS NULL);

SELECT @Sql = @Sql + CHAR(13) + CHAR(10) + 'SET IDENTITY_INSERT [dbo].[AISkillFunction] OFF;';

SELECT @Sql AS [InsertStatements];
