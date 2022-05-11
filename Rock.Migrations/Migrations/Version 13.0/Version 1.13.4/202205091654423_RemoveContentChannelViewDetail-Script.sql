DECLARE @LaunchWorkflowConditionId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'E5EFC23D-E030-496C-A9A4-D2BF4181CB49' )
DECLARE @LaunchWorkflowOnlyIfIndividualLoggedInId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'EB298724-07D5-42AF-B4BF-82420AF6A657' )
DECLARE @WriteInteractionOnlyIfIndividualLoggedInId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '63B254F7-E19C-48FD-A93F-AFEE19C1ED21' )
DECLARE @LogInteractionsId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3503170E-DD5E-4F51-9699-DCEA80C8C64C' )
DECLARE @SetPageTitleId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '406D4BB0-9BE3-4047-99C9-EAB5592B0942' )
DECLARE @OutputCacheDurationId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '7A9CBC44-FF60-464D-983A-61BD009F9C95' )
DECLARE @LavaTemplateId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '47C56661-FB70-4703-9781-8651B8B49485' )
DECLARE @ContentChannelQueryParameterId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '39CC148D-B905-4560-96DD-C5151DC344DE' )
DECLARE @ContentChannelId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'E8921151-6392-4FFD-A1F4-67A6AAD69776' )
DECLARE @EnabledLavaCommandsId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8E741F29-A5D1-433B-A520-25C65B349216' )
DECLARE @TwitterImageAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39' )
DECLARE @TwitterCardId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'D0C4618E-1F92-4107-A22F-8D638FD73E19' )
DECLARE @TwitterDescriptionAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '32DE419C-062E-45FE-9BBE-CAE104A11491' )
DECLARE @TwitterTitleAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'CE43C275-44CA-4DA6-92CB-FAAFB1F886CF' )
DECLARE @WorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '61361765-4762-4017-A58D-6CFCDD3CADC1' )

DECLARE @ContentChannelViewDetailId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '63659EBE-C5AF-4157-804A-55C7D565110E' )
DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @AttributeId int
DECLARE @AttributeValue NVARCHAR(MAX)

DECLARE @BlockId int, @PageId int
DECLARE cursor_table CURSOR FOR SELECT a.[Id] as BlockId,a.[PageId] FROM [Block] a INNER JOIN [BlockType] b on a.[BlockTypeId]=b.[Id] WHERE [Path]='~/Blocks/Cms/ContentChannelViewDetail.ascx'
OPEN cursor_table
FETCH NEXT FROM cursor_table INTO @BlockId, @PageId
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
	IF @PageId IS NOT NULL AND @BlockId IS NOT NULL AND @AttributeId IS NOT NULL
	BEGIN
		SET @AttributeValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
		DECLARE @BlockGuid UNIQUEIDENTIFIER
		SET @BlockGuid = NEWID()
		-- Delete old block
		DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockId
		DELETE [Block] WHERE [Id] = @BlockId

		-- Insert replacement block
		INSERT INTO [dbo].[Block]
		([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml])
		VALUES
		(0, @PageId, NULL , @ContentChannelViewDetailId, 'Main', 0, 'Content Channel Dynamic', NULL ,0, @BlockGuid, NULL ,NULL)

		DECLARE @B_ContentChannelViewDetailId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = @BlockGuid )
		INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
		(0, @LaunchWorkflowConditionId, @B_ContentChannelViewDetailId, '1', NEWID()),
		(0, @LaunchWorkflowOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', NEWID()),
		(0, @WriteInteractionOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', NEWID()),
		(0, @LogInteractionsId, @B_ContentChannelViewDetailId, 'False', NEWID()),
		(0, @SetPageTitleId, @B_ContentChannelViewDetailId, 'False', NEWID()),
		(0, @OutputCacheDurationId, @B_ContentChannelViewDetailId, '', NEWID()),
		(0, @LavaTemplateId, @B_ContentChannelViewDetailId, @AttributeValue, '4D917C54-E2C8-4554-B99A-B169C49EC019'),
		(0, @ContentChannelQueryParameterId, @B_ContentChannelViewDetailId, '', NEWID()),
		(0, @ContentChannelId, @B_ContentChannelViewDetailId, '8e213bb1-9e6f-40c1-b468-b3f8a60d5d24', NEWID()),
		(0, @EnabledLavaCommandsId, @B_ContentChannelViewDetailId, '', NEWID()),
		(0, @TwitterImageAttributeId, @B_ContentChannelViewDetailId, '', NEWID()),
		(0, @TwitterCardId, @B_ContentChannelViewDetailId, 'none', NEWID()),
		(0, @TwitterDescriptionAttributeId, @B_ContentChannelViewDetailId, '', NEWID()),
		(0, @TwitterTitleAttributeId, @B_ContentChannelViewDetailId, '', NEWID()),
		(0, @WorkflowTypeId, @B_ContentChannelViewDetailId, '', NEWID())
	END
	FETCH NEXT FROM cursor_table INTO @BlockId, @PageId
END
CLOSE cursor_table
DEALLOCATE cursor_table

DELETE FROM [BlockType] WHERE [Path]='~/Blocks/Cms/ContentChannelViewDetail.ascx'