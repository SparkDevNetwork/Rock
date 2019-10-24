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


DECLARE @P_ItemDetailId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '56F1DC05-3D7D-49B6-9A30-5CF271C687F4' )
DECLARE @OLD_B_ContentChannelViewDetailId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '7173AA95-15AF-49C5-933D-004717A3FF3C' )

SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
IF @P_ItemDetailId IS NOT NULL AND @OLD_B_ContentChannelViewDetailId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    SET @AttributeValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @OLD_B_ContentChannelViewDetailId )
	IF CHARINDEX(@AttributeValue, '{% include ''~~/Assets/Lava/AdDetails.lava'' %}') > 0
	BEGIN
		-- Delete old block
        DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @OLD_B_ContentChannelViewDetailId
        DELETE [Block] WHERE [Guid] = '7173AA95-15AF-49C5-933D-004717A3FF3C'

		-- Insert replacement block
		INSERT INTO [dbo].[Block]
		([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml])
		VALUES
		(0, @P_ItemDetailId, NULL , @ContentChannelViewDetailId, 'Main', 0, 'Content Channel Dynamic', NULL ,0, '0C828414-8AEF-4B43-AAEF-B200544A2197', NULL ,NULL)

		DECLARE @B_ContentChannelViewDetailId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '0C828414-8AEF-4B43-AAEF-B200544A2197' )
		INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
		(0, @LaunchWorkflowConditionId, @B_ContentChannelViewDetailId, '1', '88FD1397-3332-4A02-8AF8-581CA70780F7'),
		(0, @LaunchWorkflowOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', '1672A167-A7CD-4FD3-92EA-3DFDFF1EB274'),
		(0, @WriteInteractionOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', 'F6945011-98EE-4C83-87EF-9F76A5AA32A8'),
		(0, @LogInteractionsId, @B_ContentChannelViewDetailId, 'False', '0AAB92AD-F109-48D4-935F-66DE5656F407'),
		(0, @SetPageTitleId, @B_ContentChannelViewDetailId, 'False', 'BF3CC157-DEED-4E5D-8DC2-BF0CF263DA22'),
		(0, @OutputCacheDurationId, @B_ContentChannelViewDetailId, '', 'EB2CBDFC-8F06-4A33-ADC6-F367AC1AC51F'),
		(0, @LavaTemplateId, @B_ContentChannelViewDetailId, '{% assign detailImageGuid = Item | Attribute:''DetailImage'',''RawValue'' %}
{% if detailImageGuid != '''' %}
  <img alt="{{ Item.Title }}" src="/GetImage.ashx?Guid={{ detailImageGuid }}" class="title-image img-responsive">
{% endif %}
<h1>{{ Item.Title }}</h1>{{ Item.Content }}', '4D917C54-E2C8-4554-B99A-B169C49EC019'),
		(0, @ContentChannelQueryParameterId, @B_ContentChannelViewDetailId, '', 'FE1D35A4-B98F-48F7-BB28-71220BFD88BB'),
		(0, @ContentChannelId, @B_ContentChannelViewDetailId, '8e213bb1-9e6f-40c1-b468-b3f8a60d5d24', '7B3B8ACA-70DC-40E7-8FE2-40B010B41778'),
		(0, @EnabledLavaCommandsId, @B_ContentChannelViewDetailId, '', '0415D4C7-6807-4412-B482-7FAFE7E22299'),
		(0, @TwitterImageAttributeId, @B_ContentChannelViewDetailId, '', 'B334BFC1-04B0-424B-8DA5-609690BEC3B0'),
		(0, @TwitterCardId, @B_ContentChannelViewDetailId, 'none', '31BC4B50-07D2-407A-8592-FFA593D89B78'),
		(0, @TwitterDescriptionAttributeId, @B_ContentChannelViewDetailId, '', '764958FB-DE10-4E99-9901-C1ADDB7459CE'),
		(0, @TwitterTitleAttributeId, @B_ContentChannelViewDetailId, '', '18323E46-3BD5-4EA5-B11F-2C544CD8F1FF'),
		(0, @WorkflowTypeId, @B_ContentChannelViewDetailId, '', '2E87B646-35A2-4457-A30E-20C432D2B4ED')
	END
END


DECLARE @P_BlogDetailsId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '2D0D0FB0-68C4-47E1-8BC6-98F931497F5E' )
DECLARE @OLD_B_BlogDetailsId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'C146309D-E282-4FD5-94D9-CD4D0853AF09' )

SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
IF @P_BlogDetailsId IS NOT NULL AND @OLD_B_BlogDetailsId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    SET @AttributeValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @OLD_B_BlogDetailsId )
	IF CHARINDEX(@AttributeValue, '{% include ''~~/Assets/Lava/BlogItemDetail.lava'' %}') > 0
	BEGIN
		-- Delete old block
        DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @OLD_B_BlogDetailsId
        DELETE [Block] WHERE [Guid] = 'C146309D-E282-4FD5-94D9-CD4D0853AF09'

		-- Insert replacement block
		INSERT INTO [dbo].[Block]
		([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml])
		VALUES
		(0, @P_BlogDetailsId, NULL , @ContentChannelViewDetailId, 'Main', 0, 'Blog Details', NULL ,0, '70DCBB50-0978-4B24-9382-CDD9BEED5ADB', NULL ,NULL)

		DECLARE @B_BlogDetailsId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '70DCBB50-0978-4B24-9382-CDD9BEED5ADB' )
		INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
		(0, @LaunchWorkflowConditionId, @B_BlogDetailsId, '1', '49207A88-F1AA-4041-B1DC-3E1632A3FB9A'),
		(0, @LaunchWorkflowOnlyIfIndividualLoggedInId, @B_BlogDetailsId, 'False', '71E17D2B-F4BC-46B3-8BA5-39F147E68ACF'),
		(0, @WriteInteractionOnlyIfIndividualLoggedInId, @B_BlogDetailsId, 'False', '2EA97F05-A8B9-4C08-9C59-6261923E7D08'),
		(0, @LogInteractionsId, @B_BlogDetailsId, 'False', '6B79BEB2-276A-4CD9-B3E8-3FB6B8429FA5'),
		(0, @SetPageTitleId, @B_BlogDetailsId, 'True', '52C75062-96AF-40C4-9DAC-06F5544D3F44'),
		(0, @OutputCacheDurationId, @B_BlogDetailsId, '', '4D58B598-7E25-4D74-90C1-B9E270DECC0C'),
		(0, @LavaTemplateId, @B_BlogDetailsId, '<h1>{{ Item.Title }}</h1>
{{ Item.Content }}', '72F2158E-C2EF-4C1F-A043-C8E14AF4287B'),
		(0, @ContentChannelQueryParameterId, @B_BlogDetailsId, '', 'E9FC1144-209C-4563-9C6F-78C76423319A'),
		(0, @ContentChannelId, @B_BlogDetailsId, '2b408da7-bdd1-4e71-b6ac-f22d786b605f', '49B00CF7-636C-4A6E-B367-2B51AC26D0D2'),
		(0, @EnabledLavaCommandsId, @B_BlogDetailsId, '', 'F00B8F86-4A25-4E92-8540-98E30C60BB21'),
		(0, @TwitterImageAttributeId, @B_BlogDetailsId, '', '06520482-58B6-4712-92FF-751A5749C204'),
		(0, @TwitterCardId, @B_BlogDetailsId, 'none', '79ADF93F-D98C-4800-A98F-D130CC406B1F'),
		(0, @TwitterDescriptionAttributeId, @B_BlogDetailsId, '', 'B12AE877-5CD6-4E41-9CAD-97618530FA96'),
		(0, @TwitterTitleAttributeId, @B_BlogDetailsId, '', '9FEEEB09-3363-4042-8AAC-D54BE44C8D58'),
		(0, @WorkflowTypeId, @B_BlogDetailsId, '', '92792EB8-B388-4BDC-9916-958C8D499F49')
	END
END


DECLARE @P_MessageDetailId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'BB83C51D-65C7-4F6C-BA24-A496167C9B11' )
SET @OLD_B_ContentChannelViewDetailId = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '353BF738-D47A-4799-886D-F3055C98DBDC' )

SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
IF @P_MessageDetailId IS NOT NULL AND @OLD_B_ContentChannelViewDetailId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    SET @AttributeValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @OLD_B_ContentChannelViewDetailId )
	IF CHARINDEX(@AttributeValue, '{% include ''~~/Assets/Lava/PodcastMessageDetail.lava'' %}') > 0
	BEGIN
		-- Delete old block
        DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @OLD_B_ContentChannelViewDetailId
        DELETE [Block] WHERE [Guid] = '353BF738-D47A-4799-886D-F3055C98DBDC'

		-- Insert replacement block
		INSERT INTO [dbo].[Block]
		([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml])
		VALUES
		(0, @P_MessageDetailId, NULL , @ContentChannelViewDetailId, 'Main', 0, 'Content Channel View Detail', NULL ,0, '71D998C7-9F27-4B8A-937A-64C5EFC4783A', NULL ,NULL)

		SET @B_ContentChannelViewDetailId = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '71D998C7-9F27-4B8A-937A-64C5EFC4783A' )
		INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
		(0, @LaunchWorkflowConditionId, @B_ContentChannelViewDetailId, '1', 'CB85C100-6D83-4186-A1E4-E2813C0D0197'),
		(0, @LaunchWorkflowOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', '895819A8-2F4A-4D83-A175-7FAB08C2F240'),
		(0, @WriteInteractionOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', '937F5E50-91C1-4C9E-B0D5-4B13C58F8AEA'),
		(0, @LogInteractionsId, @B_ContentChannelViewDetailId, 'False', '0F91BDF9-D77A-4A5C-A742-6D29BBE7E13E'),
		(0, @SetPageTitleId, @B_ContentChannelViewDetailId, 'True', 'CF95091B-2C55-448A-B30B-EFBDFA94997C'),
		(0, @OutputCacheDurationId, @B_ContentChannelViewDetailId, '', 'E992D726-D714-4AAA-B1D1-8E8B6482A140'),
		(0, @LavaTemplateId, @B_ContentChannelViewDetailId, '	{% assign videoLink = Item | Attribute:''VideoLink'',''RawValue'' %}
	{% assign videoEmbed = Item | Attribute:''VideoEmbed'' %}
	{% assign audioLink = Item | Attribute:''AudioLink'',''RawValue'' %}
	
	<article class="message-detail">
		
		{% if videoEmbed != '''' %}
			{{ videoEmbed }}
		{% endif %}
		
		<h1>{{ Item.Title }}</h1>
	
		<p>
			<strong> {{ item | Attribute:''Speaker'' }} - {{ Item.StartDateTime | Date:''M/d/yyyy'' }}</strong>
		</p>
	
		<div class="row">
			<div class="col-md-8">
				{{ Item.Content }}
			</div>
			<div class="col-md-4">
				{% if videoLink != '''' or audioLink != '''' %}
					<div class="panel panel-default">
						<div class="panel-heading">Downloads &amp; Resources</div>
						<div class="list-group">
							
							{% if videoLink != '''' %}
								<a href="{{ videoLink }}" class="list-group-item"><i class="fa fa-film"></i> Video Download</a>
							{% endif %}
							
							{% if audioLink != '''' %}
								<a href="{{ audioLink }}" class="list-group-item"><i class="fa fa-volume-up"></i> Audio Download</a>
							{% endif %}
							
						</div>
					</div>
				{% endif %}
			</div>
		</row>
	</article>', '09DDEC42-456F-4280-9B7D-6F23BA73038C'),
		(0, @ContentChannelQueryParameterId, @B_ContentChannelViewDetailId, '', '9D23D12C-C97C-46B2-9AAF-DB649F9D9618'),
		(0, @ContentChannelId, @B_ContentChannelViewDetailId, '0a63a427-e6b5-2284-45b3-789b293c02ea', '0B3DA7D7-2354-4837-86AA-A8B724AF469A'),
		(0, @EnabledLavaCommandsId, @B_ContentChannelViewDetailId, '', 'A15CA15F-0541-46C2-B362-E32BAD4E651C'),
		(0, @TwitterImageAttributeId, @B_ContentChannelViewDetailId, '', '1F6D4584-D256-40D6-86D5-6EBF3BE9C0B5'),
		(0, @TwitterCardId, @B_ContentChannelViewDetailId, 'none', '1BDA500A-4F50-444F-AED7-DA46B52A94CE'),
		(0, @TwitterDescriptionAttributeId, @B_ContentChannelViewDetailId, '', '4517E488-840F-4AD2-AC17-1D89861D8621'),
		(0, @TwitterTitleAttributeId, @B_ContentChannelViewDetailId, '', 'C27B3EDF-09AD-4CA3-AB48-EE8F6A2C7A84'),
		(0, @WorkflowTypeId, @B_ContentChannelViewDetailId, '', 'C40F2DB9-7FB1-455D-98B0-C65A98C08B65')
	END
END



DECLARE @P_SeriesDetailId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7669A501-4075-431A-9828-565C47FD21C8' )
SET @OLD_B_ContentChannelViewDetailId = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'B35B99F5-7479-44C7-9C58-54B3A56BC233' )

SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
IF @P_SeriesDetailId IS NOT NULL AND @OLD_B_ContentChannelViewDetailId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    SET @AttributeValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @OLD_B_ContentChannelViewDetailId )
	IF CHARINDEX(@AttributeValue, '{% include ''~~/Assets/Lava/PodcastSeriesDetail.lava'' %}') > 0
	BEGIN
		-- Delete old block
        DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @OLD_B_ContentChannelViewDetailId
        DELETE [Block] WHERE [Guid] = 'B35B99F5-7479-44C7-9C58-54B3A56BC233'

		-- Insert replacement block
		INSERT INTO [dbo].[Block]
		([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml])
		VALUES
		(0, @P_SeriesDetailId, NULL , @ContentChannelViewDetailId, 'Main', 0, 'Content Channel View Detail', NULL ,0, '847E12E0-A7FC-4BD5-BD7E-1E9D435510E7', NULL ,NULL)

		SET @B_ContentChannelViewDetailId = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '847E12E0-A7FC-4BD5-BD7E-1E9D435510E7' )
		INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
		(0, @LaunchWorkflowConditionId, @B_ContentChannelViewDetailId, '1', '611247A2-15CD-4C0F-AA87-8CD490C8A0F3'),
		(0, @LaunchWorkflowOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', '179B2422-30C8-4912-B35F-41BB23942A73'),
		(0, @WriteInteractionOnlyIfIndividualLoggedInId, @B_ContentChannelViewDetailId, 'False', 'F7C51048-B948-4549-845E-01BEAB6E40A2'),
		(0, @LogInteractionsId, @B_ContentChannelViewDetailId, 'False', '8E1F81C5-E6E5-4A9B-B3FD-527A3D5B6FEF'),
		(0, @SetPageTitleId, @B_ContentChannelViewDetailId, 'True', '23745839-F022-4577-A443-FFB82764DAAF'),
		(0, @OutputCacheDurationId, @B_ContentChannelViewDetailId, '', '5C8DE0BE-2591-4C17-A6E7-D2004E2217D4'),
		(0, @LavaTemplateId, @B_ContentChannelViewDetailId, '<style>
	.series-banner {
		height: 220px;
		background-size: cover;
		background-position: center center;
		background-repeat: no-repeat;
	}
	
	@media (min-width: 992px) {
		.series-banner {
			height: 420px;
		}
	}
	
	.series-title{
		margin-bottom: 4px;
	}
	
	.series-dates {
		opacity: .6;
	}
	
	.messages-title {
		font-size: 24px;
	}
	
	.messages {
		font-size: 18px;
	}
</style>

{% if Item  %}
	
	<article class="series-detail">
		{% assign seriesImageGuid = Item | Attribute:''SeriesImage'',''RawValue'' %}
		<div class="series-banner" style="background-image: url(''/GetImage.ashx?Guid={{ seriesImageGuid }}'');" ></div>

		<h1 class="series-title">{{ Item.Title }}</h1>
		<p class="series-dates">
			<strong>{{ Item.StartDateTime | Date:''M/d/yyyy'' }} 
				{% if Item.StartDateTime != Item.ExpireDateTime %}
					- {{ Item.ExpireDateTime | Date:''M/d/yyyy'' }}
				{% endif %}
			</strong>
		</p>

		
		<script>function fbs_click() { u = location.href; t = document.title; window.open(''http://www.facebook.com/sharer.php?u='' + encodeURIComponent(u) + ''&t='' + encodeURIComponent(t), ''sharer'', ''toolbar=0,status=0,width=626,height=436''); return false; }</script>
		<script>function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace(''END:VEVENT'', ''SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT''); var element = document.createElement(''a''); element.setAttribute(''href'', ''data:text/plain;charset=utf-8,'' + encodeURIComponent(text)); element.setAttribute(''download'', ''{{ Event.Name }}.ics''); element.style.display = ''none''; document.body.appendChild(element); element.click(); document.body.removeChild(element); }</script>
		<ul class="socialsharing">
			<li>
				<a href="http://www.facebook.com/share.php?u=<url>" onclick="return fbs_click()" target="_blank" class="socialicon socialicon-facebook" title="" data-original-title="Share via Facebook">
					<i class="fa fa-fw fa-facebook"></i>
				</a>
			</li>
			<li>
				<a href="http://twitter.com/home?status={{ ''Global'' | Page:''Url'' | EscapeDataString }}" class="socialicon socialicon-twitter" title="" data-original-title="Share via Twitter">
					<i class="fa fa-fw fa-twitter"></i>
				</a>
			</li>
			<li>
				<a href="mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ ''Global'' | Page:''Url'' }}"  class="socialicon socialicon-email" title="" data-original-title="Share via Email">
					<i class="fa fa-fw fa-envelope-o"></i>
				</a>
			</li>
		</ul>
		
		<div class="margin-t-lg">
			{{ Item.Content }}
		</div>
		
		<h4 class="messages-title margin-t-lg">In This Series</h4>
		<ol class="messages">
			{% for message in Item.ChildItems %}
				<li>
					<a href="/page/' + CONVERT(nvarchar(max), @P_MessageDetailId) +'?Item={{ message.ChildContentChannelItem.Id }}">
						{{ message.ChildContentChannelItem.Title }}
					</a>
        </li>
			{% endfor %}
		</ol>
		
	</article>
	
{% else %}
	<h1>Could not find series.</h1>
{% endif %}', '99742113-661B-46D4-BAC6-D1FA9629DFE5'),
		(0, @ContentChannelQueryParameterId, @B_ContentChannelViewDetailId, '', '4C938747-219B-4D7B-92D1-7189BE28FA28'),
		(0, @ContentChannelId, @B_ContentChannelViewDetailId, 'e2c598f1-d299-1baa-4873-8b679e3c1998', '6EDC2FCA-298E-4830-8EA2-6A1249ABE916'),
		(0, @EnabledLavaCommandsId, @B_ContentChannelViewDetailId, '', '55827233-C360-4E8F-924B-106B7AA29650'),
		(0, @TwitterImageAttributeId, @B_ContentChannelViewDetailId, '', '652ABA6A-1D6C-4644-8FCA-8175EFD38D06'),
		(0, @TwitterCardId, @B_ContentChannelViewDetailId, 'none', 'A214C860-61A8-4A5D-8A17-2BB46C298C3D'),
		(0, @TwitterDescriptionAttributeId, @B_ContentChannelViewDetailId, '', 'BA12FB0B-F302-47C6-A7D4-9689B9444FA3'),
		(0, @TwitterTitleAttributeId, @B_ContentChannelViewDetailId, '', '33AB29AC-27E2-46BD-936D-6EDFEF3EED97'),
		(0, @WorkflowTypeId, @B_ContentChannelViewDetailId, '', 'E53370EB-AD03-4A62-B627-C6DAFD00E371')
	END
END