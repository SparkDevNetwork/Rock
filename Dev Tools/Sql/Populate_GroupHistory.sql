/* NOTE: This script requires the following:
- At least one GroupType has GroupHistory enabled (for example Small Group)
- The 'Process Group History' Rock Job has run at least once (to create a GroupHistorical record)
*/

declare 
	@personId int = (select top 1 Id from Person where NickName = 'Ted' and LastName = 'Decker') /*Get PersonId for Ted Decker*/

delete from GroupMemberHistorical where GroupMemberId in (select Id from GroupMember where PersonId = @personId)

DECLARE @FakeDatesTable TABLE (
	[EffectiveDateTime] DATETIME
	,[ExpireDateTime] DATETIME
	,[IsArchived] BIT
	,[CurrentRowIndicator] BIT
	)
DECLARE @StartDateTime DATETIME = DateAdd(year, - 10, SysDateTime())
	,@StopDateTime DATETIME = SysDateTime()
DECLARE @EffectiveDateTime DATETIME = @StartDateTime
	,@ExpireDateTime DATETIME
	,@IsArchived BIT = 0
	,@CurrentRowIndicator BIT = 0

set nocount on
WHILE (@EffectiveDateTime < @StopDateTime)
BEGIN
	SET @ExpireDateTime = dateadd(MI, floor(RAND() * 180*24*60), @EffectiveDateTime)
	if @ExpireDateTime >= @StopDateTime begin
	  set @ExpireDateTime = '9999-01-01 00:00:00.000'
	  set @CurrentRowIndicator = 1
	end

	INSERT INTO @FakeDatesTable (
		EffectiveDateTime
		,ExpireDateTime
		,IsArchived
		,CurrentRowIndicator
		)
	VALUES (
		@EffectiveDateTime
		,@ExpireDateTime
		,@IsArchived
		,@CurrentRowIndicator
		);

	SET @EffectiveDateTime = @ExpireDateTime;
	SET @IsArchived = case when @IsArchived = 1 then 0 else 1 end
END

set nocount off

INSERT INTO [dbo].[GroupMemberHistorical] (
	[GroupMemberId]
	,[GroupId]
	,[GroupRoleId]
	,[GroupRoleName]
	,[IsLeader]
	,[GroupMemberStatus]
	,[IsArchived]
	,[ArchivedDateTime]
	,[ArchivedByPersonAliasId]
	,[InactiveDateTime]
	,[EffectiveDateTime]
	,[ExpireDateTime]
	,[CurrentRowIndicator]
	,[CreatedDateTime]
	,[Guid]
	)
SELECT gm.[Id] [GroupMemberId]
	,gm.[GroupId]
	,gm.[GroupRoleId]
	,r.[Name] [GroupRoleName]
	,r.[IsLeader]
	,gm.[GroupMemberStatus]
	,fd.[IsArchived]
	,gm.[ArchivedDateTime]
	,gm.[ArchivedByPersonAliasId]
	,gm.[InactiveDateTime]
	,fd.[EffectiveDateTime]
	,fd.[ExpireDateTime]
	,fd.[CurrentRowIndicator]
	,fd.[EffectiveDateTime] [CreatedDateTime]
	,NEWID() [Guid]
FROM GroupMember gm
join GroupTypeRole r on gm.GroupRoleId = r.Id
join @FakeDatesTable fd on 1=1
where gm.PersonId = @personId
and gm.GroupId in (select GroupId from GroupHistorical)
and gm.Id not in (select GroupMemberId from GroupMemberHistorical)
order by gm.Id




