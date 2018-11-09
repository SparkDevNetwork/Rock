-- This Script can by used to "undo" the changes made by the Tools_PostDeployTestDatabase script.
-- It requires a database that has the correct data (typicallly restored from a previous backup)
-- Before running replace all occurrences of 'BackupProdDb' with the actual name of the database that has correct data.

-- Reset Page Encryption
UPDATE R SET [RequiresEncryption] = B.[RequiresEncryption]
FROM [Page] R
INNER JOIN [BackupProdDb].[dbo].[Page] B ON B.[Id] = R.[Id]
WHERE R.[RequiresEncryption] <> B.[RequiresEncryption]

-- Reset Site Encryption
UPDATE R SET [RequiresEncryption] = B.[RequiresEncryption]
FROM [Site] R
INNER JOIN [BackupProdDb].[dbo].[Site] B ON B.[Id] = R.[Id]
WHERE R.[RequiresEncryption] <> B.[RequiresEncryption]

-- Reset Person Emails
UPDATE R SET [Email] = B.[Email]
FROM [Person] R
INNER JOIN [BackupProdDb].[dbo].[Person] B ON B.[Id] = R.[Id]
WHERE R.[Email] <> B.[Email]

-- Reset the SMTP Medium settings 
DECLARE @SMTPEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.SMTP' )
DECLARE @MailAttributeId int

-- SMTP Server
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'Server' )
UPDATE R SET [Value] = B.[Value]
FROM [AttributeValue] R
INNER JOIN [BackupProdDb].[dbo].[AttributeValue] B ON B.[Id] = R.[Id]
WHERE R.[AttributeId] = @MailAttributeId

-- SMTP Port
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'Port' )
UPDATE R SET [Value] = B.[Value]
FROM [AttributeValue] R
INNER JOIN [BackupProdDb].[dbo].[AttributeValue] B ON B.[Id] = R.[Id]
WHERE R.[AttributeId] = @MailAttributeId

-- SMTP Username
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'UserName' )
UPDATE R SET [Value] = B.[Value]
FROM [AttributeValue] R
INNER JOIN [BackupProdDb].[dbo].[AttributeValue] B ON B.[Id] = R.[Id]
WHERE R.[AttributeId] = @MailAttributeId

-- SMTP Password
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'Password' )
UPDATE R SET [Value] = B.[Value]
FROM [AttributeValue] R
INNER JOIN [BackupProdDb].[dbo].[AttributeValue] B ON B.[Id] = R.[Id]
WHERE R.[AttributeId] = @MailAttributeId

-- SMTP UseSSL
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'UseSSL' )
UPDATE R SET [Value] = B.[Value]
FROM [AttributeValue] R
INNER JOIN [BackupProdDb].[dbo].[AttributeValue] B ON B.[Id] = R.[Id]
WHERE R.[AttributeId] = @MailAttributeId

-- Reset Mail Transport
DECLARE @MailgunEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] ='Rock.Communication.Transport.MailgunSmtp' )
DECLARE @MailEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Medium.Email' )
DECLARE @SMTPEntityTypeGuid varchar(50) = ( SELECT LOWER(CAST([Guid] as varchar(50))) FROM [EntityType] WHERE [Id] = @MailgunEntityTypeId )
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @MailEntityTypeId AND [Key] = 'TransportContainer' )
UPDATE [AttributeValue] SET [Value] = @SMTPEntityTypeGuid WHERE [AttributeId] = @MailAttributeId

-- Reset Job Activation
UPDATE R SET [IsActive] = B.[IsActive]
FROM [ServiceJob] R
INNER JOIN [BackupProdDb].[dbo].[ServiceJob] B ON B.[Id] = R.[Id]
WHERE R.[IsActive] <> B.[IsActive]

-- Reset Banner
UPDATE R SET [PreHtml] = B.[PreHtml]
FROM [Block] R
INNER JOIN [BlockType] T ON T.[Id] = R.[BlockTypeId]
INNER JOIN [BackupProdDb].[dbo].[Block] B ON B.[Id] = R.[Id]
WHERE T.[Path] = '~/Blocks/Core/SmartSearch.ascx'
AND B.[Zone] = 'Header'
AND R.[PreHtml] <> B.[PreHtml]

-- Find any people who may have been added after the last backup. Their emails will need
-- to be updated manually be looking at the history to see what their correct email is.
SELECT [Id], [NickName], [LastName]
FROM [Person]
WHERE [Email] like '%@safety.netz'