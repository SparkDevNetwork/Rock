-- CREATE FUNCTION FOR REMOVING NON-ALPHA CHARACTERS
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnUtility_RemoveNonAlphaCharacters]') AND type = 'FN')
DROP FUNCTION [dbo].[ufnUtility_RemoveNonAlphaCharacters]
GO

CREATE FUNCTION [dbo].[ufnUtility_RemoveNonAlphaCharacters](@Temp VarChar(1000))
RETURNS VARCHAR(1000)

AS

BEGIN

    DECLARE @KeepValues as VARCHAR(50)
    SET @KeepValues = '%[^a-z]%'
    WHILE PatIndex(@KeepValues, @Temp) > 0
        SET @Temp = Stuff(@Temp, PatIndex(@KeepValues, @Temp), 1, '')

    RETURN @Temp
END
GO

/* Recreate RockUser so it points to the current server's Logins */
DROP USER [RockUser]
GO
CREATE USER [RockUser] FOR LOGIN [RockUser]
GO
ALTER ROLE [db_owner] ADD MEMBER [RockUser]
GO

-- TURN OFF SSL FOR ALL PAGES
UPDATE [Page] SET [RequiresEncryption] = 0

-- TURN OFF SSL FOR ALL SITES
UPDATE [Site] SET [RequiresEncryption] = 0

-- INACTIVATE JOBS
UPDATE [ServiceJob] SET [IsActive] = 0

-- BLANK OUT EMAILS
UPDATE [Person]
set [Email] = LOWER(dbo.[ufnUtility_RemoveNonAlphaCharacters]([NickName])) + LOWER(dbo.[ufnUtility_RemoveNonAlphaCharacters]([LastName])) + '@safety.netz'
WHERE [Email] IS NOT NULL

-- Update the Mail Medium/Transport settings to use SMTP with Localhost/25
DECLARE @SMTPEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.SMTP' )
DECLARE @MailEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Medium.Email' )
DECLARE @MailAttributeId int

-- SMTP Server
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'Server' )
UPDATE [AttributeValue] SET [Value] = 'localhost' WHERE [AttributeId] = @MailAttributeId

-- SMTP Port
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'Port' )
UPDATE [AttributeValue] SET [Value] = '25' WHERE [AttributeId] = @MailAttributeId

-- SMTP Username
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'UserName' )
UPDATE [AttributeValue] SET [Value] = '' WHERE [AttributeId] = @MailAttributeId

-- SMTP Password
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'Password' )
UPDATE [AttributeValue] SET [Value] = '' WHERE [AttributeId] = @MailAttributeId

-- SMTP UseSSL
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @SMTPEntityTypeId AND [Key] = 'UseSSL' )
UPDATE [AttributeValue] SET [Value] = 'False' WHERE [AttributeId] = @MailAttributeId

-- Mail Transport
DECLARE @SMTPEntityTypeGuid varchar(50) = ( SELECT LOWER(CAST([Guid] as varchar(50))) FROM [EntityType] WHERE [Id] = @SMTPEntityTypeId )
SET @MailAttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @MailEntityTypeId AND [Key] = 'TransportContainer' )
UPDATE [AttributeValue] SET [Value] = @SMTPEntityTypeGuid WHERE [AttributeId] = @MailAttributeId
