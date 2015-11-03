DECLARE @NoteTypeEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '337EED57-D4AB-4EED-BBDB-0CB3A467DBCC')
DECLARE @GroupMemberNoteTypeId int = (SELECT TOP 1 [Id] FROM [NoteType] WHERE [Guid] = 'FFFC3644-60CD-4D14-A714-E8DCC202A0E1')

DECLARE @StaffGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4')
DECLARE @StaffLikeGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745')

IF NOT EXISTS (SELECT * FROM [Auth] WHERE [EntityTypeId] = @NoteTypeEntityTypeId AND [EntityId] = @GroupMemberNoteTypeId AND [Action] = 'Edit' AND [AllowOrDeny] = 'A' AND [GroupId] = @StaffGroupId)
BEGIN

	INSERT INTO [Auth]
		([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
	VALUES
		(@NoteTypeEntityTypeId, @GroupMemberNoteTypeId, 0, 'Edit', 'A', '', @StaffGroupId, '12716032-3F6A-679E-4592-B7A0130517F5')

END

IF NOT EXISTS (SELECT * FROM [Auth] WHERE [EntityTypeId] = @NoteTypeEntityTypeId AND [EntityId] = @GroupMemberNoteTypeId AND [Action] = 'Edit' AND [AllowOrDeny] = 'A' AND [GroupId] = @StaffLikeGroupId)
BEGIN

	INSERT INTO [Auth]
		([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
	VALUES
		(@NoteTypeEntityTypeId, @GroupMemberNoteTypeId, 0, 'Edit', 'A', '', @StaffLikeGroupId, '0B3990E1-EA2B-64A6-4CA1-3A1252D87E32')

END