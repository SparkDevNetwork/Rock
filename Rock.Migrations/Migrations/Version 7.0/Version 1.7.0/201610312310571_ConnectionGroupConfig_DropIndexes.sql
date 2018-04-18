IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Note'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Note]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Note'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Note]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Note'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Note]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionRequest'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionRequest'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionRequest'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('NoteType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [NoteType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('NoteType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [NoteType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('NoteType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [NoteType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FollowingEventNotification'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FollowingEventNotification]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FollowingEventNotification'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FollowingEventNotification]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FollowingEventNotification'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FollowingEventNotification]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionOpportunity'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionOpportunity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionOpportunity'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionOpportunity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionOpportunity'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionOpportunity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RestAction'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RestAction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RestAction'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RestAction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RestAction'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RestAction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionOpportunityCampus'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionOpportunityCampus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionOpportunityCampus'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionOpportunityCampus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionOpportunityCampus'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionOpportunityCampus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RestController'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RestController]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RestController'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RestController]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RestController'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RestController]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PersonViewed'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PersonViewed]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PersonViewed'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PersonViewed]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PersonViewed'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PersonViewed]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionOpportunityGroup'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionOpportunityGroup]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionOpportunityGroup'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionOpportunityGroup]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionOpportunityGroup'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionOpportunityGroup]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EntitySet'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EntitySet]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EntitySet'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EntitySet]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EntitySet'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EntitySet]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PrayerRequest'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PrayerRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PrayerRequest'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PrayerRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PrayerRequest'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PrayerRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PersonPreviousName'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PersonPreviousName]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PersonPreviousName'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PersonPreviousName]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PersonPreviousName'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PersonPreviousName]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EntitySetItem'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EntitySetItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EntitySetItem'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EntitySetItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EntitySetItem'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EntitySetItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ReportField'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ReportField]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ReportField'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ReportField]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ReportField'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ReportField]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionActivityType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionActivityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionActivityType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionActivityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionActivityType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionActivityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Report'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Report]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Report'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Report]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Report'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Report]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('AttendanceCode'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [AttendanceCode]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('AttendanceCode'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [AttendanceCode]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('AttendanceCode'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [AttendanceCode]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionStatus'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionStatus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionStatus'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionStatus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionStatus'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionStatus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Notification'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Notification]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Notification'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Notification]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Notification'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Notification]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PageViewSession'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PageViewSession]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PageViewSession'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PageViewSession]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PageViewSession'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PageViewSession]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ServiceJob'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ServiceJob]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ServiceJob'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ServiceJob]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ServiceJob'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ServiceJob]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionWorkflow'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionWorkflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionWorkflow'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionWorkflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionWorkflow'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionWorkflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('NotificationRecipient'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [NotificationRecipient]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('NotificationRecipient'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [NotificationRecipient]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('NotificationRecipient'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [NotificationRecipient]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PageViewUserAgent'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PageViewUserAgent]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PageViewUserAgent'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PageViewUserAgent]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PageViewUserAgent'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PageViewUserAgent]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ServiceLog'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ServiceLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ServiceLog'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ServiceLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ServiceLog'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ServiceLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Device'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Device]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Device'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Device]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Device'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Device]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionRequestActivity'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionRequestActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionRequestActivity'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionRequestActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionRequestActivity'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionRequestActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupMemberWorkflowTrigger'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupMemberWorkflowTrigger]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupMemberWorkflowTrigger'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupMemberWorkflowTrigger]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupMemberWorkflowTrigger'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupMemberWorkflowTrigger]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('TaggedItem'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [TaggedItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('TaggedItem'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [TaggedItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('TaggedItem'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [TaggedItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('DefinedValue'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [DefinedValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('DefinedValue'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [DefinedValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('DefinedValue'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [DefinedValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionRequestWorkflow'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionRequestWorkflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionRequestWorkflow'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionRequestWorkflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionRequestWorkflow'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionRequestWorkflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Tag'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Tag]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Tag'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Tag]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Tag'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Tag]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('DefinedType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [DefinedType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('DefinedType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [DefinedType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('DefinedType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [DefinedType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowAction'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowAction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowAction'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowAction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowAction'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowAction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FieldType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FieldType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FieldType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FieldType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FieldType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FieldType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PersonBadge'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PersonBadge]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PersonBadge'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PersonBadge]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PersonBadge'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PersonBadge]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventItemAudience'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventItemAudience]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventItemAudience'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventItemAudience]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventItemAudience'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventItemAudience]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowActionType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowActionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowActionType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowActionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowActionType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowActionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Location'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Location]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Location'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Location]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Location'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Location]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BackgroundCheck'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BackgroundCheck]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BackgroundCheck'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BackgroundCheck]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BackgroundCheck'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BackgroundCheck]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventItem'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventItem'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventItem'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowActivityType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowActivityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowActivityType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowActivityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowActivityType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowActivityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupLocation'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupLocation]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupLocation'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupLocation]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupLocation'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupLocation]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventCalendarItem'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventCalendarItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventCalendarItem'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventCalendarItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventCalendarItem'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventCalendarItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Group'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Group]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Group'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Group]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Group'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Group]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventCalendar'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventCalendar]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventCalendar'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventCalendar]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventCalendar'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventCalendar]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowActivity'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowActivity'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowActivity'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Campus'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Campus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Campus'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Campus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Campus'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Campus]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PageView'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PageView]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PageView'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PageView]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PageView'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PageView]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventItemOccurrence'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventItemOccurrence]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventItemOccurrence'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventItemOccurrence]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventItemOccurrence'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventItemOccurrence]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Workflow'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Workflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Workflow'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Workflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Workflow'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Workflow]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowLog'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowLog'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowLog'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupTypeRole'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupTypeRole]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupTypeRole'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupTypeRole]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupTypeRole'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupTypeRole]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowTrigger'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowTrigger]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowTrigger'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowTrigger]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowTrigger'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowTrigger]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BinaryFile'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BinaryFile]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BinaryFile'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BinaryFile]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BinaryFile'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BinaryFile]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Following'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Following]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Following'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Following]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Following'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Following]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BinaryFileType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BinaryFileType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BinaryFileType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BinaryFileType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BinaryFileType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BinaryFileType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EntityType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EntityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EntityType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EntityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EntityType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EntityType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ContentChannel'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ContentChannel]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ContentChannel'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ContentChannel]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ContentChannel'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ContentChannel]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BinaryFileData'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BinaryFileData]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BinaryFileData'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BinaryFileData]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BinaryFileData'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BinaryFileData]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ContentChannelType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ContentChannelType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ContentChannelType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ContentChannelType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ContentChannelType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ContentChannelType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ContentChannelItem'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ContentChannelItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ContentChannelItem'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ContentChannelItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ContentChannelItem'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ContentChannelItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupMember'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupMember]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupMember'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupMember]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupMember'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupMember]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Person'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Person]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Person'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Person]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Person'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Person]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('CommunicationTemplate'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [CommunicationTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('CommunicationTemplate'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [CommunicationTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('CommunicationTemplate'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [CommunicationTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('SystemEmail'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [SystemEmail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('SystemEmail'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [SystemEmail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('SystemEmail'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [SystemEmail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PhoneNumber'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PhoneNumber]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PhoneNumber'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PhoneNumber]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PhoneNumber'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PhoneNumber]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('UserLogin'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [UserLogin]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('UserLogin'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [UserLogin]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('UserLogin'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [UserLogin]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Schedule'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Schedule]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Schedule'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Schedule]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Schedule'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Schedule]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Category'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Category]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Category'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Category]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Category'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Category]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('AttributeQualifier'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [AttributeQualifier]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('AttributeQualifier'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [AttributeQualifier]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('AttributeQualifier'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [AttributeQualifier]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Attribute'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Attribute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Attribute'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Attribute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Attribute'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Attribute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('CommunicationRecipientActivity'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [CommunicationRecipientActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('CommunicationRecipientActivity'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [CommunicationRecipientActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('CommunicationRecipientActivity'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [CommunicationRecipientActivity]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('AttributeValue'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [AttributeValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('AttributeValue'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [AttributeValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('AttributeValue'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [AttributeValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Audit'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Audit]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Audit'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Audit]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Audit'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Audit]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventItemOccurrenceGroupMap'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventItemOccurrenceGroupMap]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventItemOccurrenceGroupMap'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventItemOccurrenceGroupMap]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventItemOccurrenceGroupMap'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventItemOccurrenceGroupMap]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Auth'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Auth]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Auth'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Auth]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Auth'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Auth]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationInstance'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationInstance]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationInstance'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationInstance]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationInstance'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationInstance]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Block'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Block]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Block'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Block]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Block'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Block]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Registration'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Registration]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Registration'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Registration]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Registration'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Registration]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BlockType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BlockType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BlockType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BlockType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BlockType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BlockType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationRegistrant'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationRegistrant]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationRegistrant'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationRegistrant]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationRegistrant'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationRegistrant]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('MetricPartition'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [MetricPartition]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('MetricPartition'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [MetricPartition]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('MetricPartition'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [MetricPartition]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Layout'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Layout]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Layout'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Layout]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Layout'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Layout]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Metric'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Metric]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Metric'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Metric]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Metric'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Metric]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationRegistrantFee'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationRegistrantFee]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationRegistrantFee'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationRegistrantFee]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationRegistrantFee'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationRegistrantFee]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupScheduleExclusion'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupScheduleExclusion]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupScheduleExclusion'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupScheduleExclusion]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupScheduleExclusion'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupScheduleExclusion]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('MetricValuePartition'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [MetricValuePartition]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('MetricValuePartition'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [MetricValuePartition]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('MetricValuePartition'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [MetricValuePartition]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Page'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Page]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Page'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Page]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Page'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Page]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('MetricValue'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [MetricValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('MetricValue'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [MetricValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('MetricValue'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [MetricValue]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationTemplateFee'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationTemplateFee]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationTemplateFee'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationTemplateFee]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationTemplateFee'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationTemplateFee]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PageContext'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PageContext]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PageContext'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PageContext]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PageContext'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PageContext]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ConnectionOpportunityConnectorGroup'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ConnectionOpportunityConnectorGroup]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ConnectionOpportunityConnectorGroup'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ConnectionOpportunityConnectorGroup]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ConnectionOpportunityConnectorGroup'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ConnectionOpportunityConnectorGroup]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationTemplate'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationTemplate'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationTemplate'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PageRoute'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PageRoute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PageRoute'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PageRoute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PageRoute'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PageRoute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationTemplateDiscount'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationTemplateDiscount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationTemplateDiscount'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationTemplateDiscount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationTemplateDiscount'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationTemplateDiscount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Site'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Site]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Site'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Site]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Site'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Site]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationTemplateForm'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationTemplateForm]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationTemplateForm'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationTemplateForm]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationTemplateForm'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationTemplateForm]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('SiteDomain'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [SiteDomain]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('SiteDomain'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [SiteDomain]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('SiteDomain'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [SiteDomain]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('RegistrationTemplateFormField'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [RegistrationTemplateFormField]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('RegistrationTemplateFormField'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [RegistrationTemplateFormField]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('RegistrationTemplateFormField'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [RegistrationTemplateFormField]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventItemOccurrenceChannelItem'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventItemOccurrenceChannelItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventItemOccurrenceChannelItem'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventItemOccurrenceChannelItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventItemOccurrenceChannelItem'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventItemOccurrenceChannelItem]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('CommunicationRecipient'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [CommunicationRecipient]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('CommunicationRecipient'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [CommunicationRecipient]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('CommunicationRecipient'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [CommunicationRecipient]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('EventCalendarContentChannel'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [EventCalendarContentChannel]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('EventCalendarContentChannel'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [EventCalendarContentChannel]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('EventCalendarContentChannel'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [EventCalendarContentChannel]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BenevolenceRequest'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BenevolenceRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BenevolenceRequest'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BenevolenceRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BenevolenceRequest'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BenevolenceRequest]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('Communication'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [Communication]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('Communication'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [Communication]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('Communication'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [Communication]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BenevolenceResult'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BenevolenceResult]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BenevolenceResult'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BenevolenceResult]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BenevolenceResult'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BenevolenceResult]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('DataViewFilter'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [DataViewFilter]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('DataViewFilter'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [DataViewFilter]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('DataViewFilter'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [DataViewFilter]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('DataView'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [DataView]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('DataView'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [DataView]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('DataView'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [DataView]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('MetricCategory'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [MetricCategory]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('MetricCategory'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [MetricCategory]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('MetricCategory'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [MetricCategory]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ExceptionLog'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ExceptionLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ExceptionLog'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ExceptionLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ExceptionLog'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ExceptionLog]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialAccount'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialAccount'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialAccount'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialBatch'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialBatch]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialBatch'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialBatch]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialBatch'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialBatch]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialTransaction'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialTransaction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialTransaction'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialTransaction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialTransaction'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialTransaction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('BenevolenceRequestDocument'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [BenevolenceRequestDocument]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('BenevolenceRequestDocument'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [BenevolenceRequestDocument]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('BenevolenceRequestDocument'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [BenevolenceRequestDocument]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialTransactionImage'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialTransactionImage]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialTransactionImage'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialTransactionImage]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialTransactionImage'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialTransactionImage]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowActionFormAttribute'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowActionFormAttribute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowActionFormAttribute'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowActionFormAttribute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowActionFormAttribute'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowActionFormAttribute]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialPaymentDetail'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialPaymentDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialPaymentDetail'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialPaymentDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialPaymentDetail'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialPaymentDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('MergeTemplate'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [MergeTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('MergeTemplate'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [MergeTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('MergeTemplate'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [MergeTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialTransactionRefund'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialTransactionRefund]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialTransactionRefund'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialTransactionRefund]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialTransactionRefund'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialTransactionRefund]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('WorkflowActionForm'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [WorkflowActionForm]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('WorkflowActionForm'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [WorkflowActionForm]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('WorkflowActionForm'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [WorkflowActionForm]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialScheduledTransaction'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialScheduledTransaction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialScheduledTransaction'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialScheduledTransaction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialScheduledTransaction'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialScheduledTransaction]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialScheduledTransactionDetail'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialScheduledTransactionDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialScheduledTransactionDetail'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialScheduledTransactionDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialScheduledTransactionDetail'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialScheduledTransactionDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('ContentChannelItemAssociation'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [ContentChannelItemAssociation]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('ContentChannelItemAssociation'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [ContentChannelItemAssociation]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('ContentChannelItemAssociation'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [ContentChannelItemAssociation]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialTransactionDetail'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialTransactionDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialTransactionDetail'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialTransactionDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialTransactionDetail'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialTransactionDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('AuditDetail'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [AuditDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('AuditDetail'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [AuditDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('AuditDetail'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [AuditDetail]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FollowingSuggested'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FollowingSuggested]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FollowingSuggested'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FollowingSuggested]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FollowingSuggested'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FollowingSuggested]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialPersonBankAccount'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialPersonBankAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialPersonBankAccount'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialPersonBankAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialPersonBankAccount'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialPersonBankAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PluginMigration'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PluginMigration]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PluginMigration'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PluginMigration]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PluginMigration'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PluginMigration]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FollowingSuggestionType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FollowingSuggestionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FollowingSuggestionType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FollowingSuggestionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FollowingSuggestionType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FollowingSuggestionType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialPersonSavedAccount'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialPersonSavedAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialPersonSavedAccount'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialPersonSavedAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialPersonSavedAccount'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialPersonSavedAccount]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FollowingEventSubscription'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FollowingEventSubscription]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FollowingEventSubscription'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FollowingEventSubscription]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FollowingEventSubscription'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FollowingEventSubscription]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialGateway'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialGateway]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialGateway'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialGateway]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialGateway'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialGateway]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FinancialPledge'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FinancialPledge]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FinancialPledge'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FinancialPledge]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FinancialPledge'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FinancialPledge]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('FollowingEventType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [FollowingEventType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('FollowingEventType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [FollowingEventType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('FollowingEventType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [FollowingEventType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('HtmlContent'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [HtmlContent]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('HtmlContent'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [HtmlContent]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('HtmlContent'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [HtmlContent]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('SignatureDocumentTemplate'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [SignatureDocumentTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('SignatureDocumentTemplate'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [SignatureDocumentTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('SignatureDocumentTemplate'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [SignatureDocumentTemplate]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('PersonAlias'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [PersonAlias]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('PersonAlias'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [PersonAlias]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('PersonAlias'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [PersonAlias]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('SignatureDocument'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [SignatureDocument]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('SignatureDocument'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [SignatureDocument]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('SignatureDocument'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [SignatureDocument]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupRequirement'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupRequirement]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupRequirement'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupRequirement]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupRequirement'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupRequirement]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupRequirementType'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupRequirementType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupRequirementType'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupRequirementType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupRequirementType'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupRequirementType]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID('GroupMemberRequirement'))
BEGIN
	DROP INDEX [IX_ForeignId] ON [GroupMemberRequirement]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID('GroupMemberRequirement'))
BEGIN
	DROP INDEX [IX_ForeignGuid] ON [GroupMemberRequirement]
END
IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID('GroupMemberRequirement'))
BEGIN
	DROP INDEX [IX_ForeignKey] ON [GroupMemberRequirement]
END