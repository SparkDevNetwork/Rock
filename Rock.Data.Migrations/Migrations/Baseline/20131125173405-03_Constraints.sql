ALTER TABLE [dbo].[AIAgent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgent_dbo.BinaryFile_AvatarBinaryFileId] FOREIGN KEY([AvatarBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[AIAgent] CHECK CONSTRAINT [FK_dbo.AIAgent_dbo.BinaryFile_AvatarBinaryFileId]
GO
ALTER TABLE [dbo].[AIAgent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgent_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AIAgent] CHECK CONSTRAINT [FK_dbo.AIAgent_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AIAgent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgent_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AIAgent] CHECK CONSTRAINT [FK_dbo.AIAgent_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AIAgentSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSession_dbo.AIAgent_AIAgentId] FOREIGN KEY([AIAgentId])
REFERENCES [dbo].[AIAgent] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AIAgentSession] CHECK CONSTRAINT [FK_dbo.AIAgentSession_dbo.AIAgent_AIAgentId]
GO
ALTER TABLE [dbo].[AIAgentSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSession_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AIAgentSession] CHECK CONSTRAINT [FK_dbo.AIAgentSession_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[AIAgentSessionAnchor]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSessionAnchor_dbo.AIAgentSession_AIAgentSessionId] FOREIGN KEY([AIAgentSessionId])
REFERENCES [dbo].[AIAgentSession] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AIAgentSessionAnchor] CHECK CONSTRAINT [FK_dbo.AIAgentSessionAnchor_dbo.AIAgentSession_AIAgentSessionId]
GO
ALTER TABLE [dbo].[AIAgentSessionAnchor]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSessionAnchor_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AIAgentSessionAnchor] CHECK CONSTRAINT [FK_dbo.AIAgentSessionAnchor_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[AIAgentSessionHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSessionHistory_dbo.AIAgentSession_AIAgentSessionId] FOREIGN KEY([AIAgentSessionId])
REFERENCES [dbo].[AIAgentSession] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AIAgentSessionHistory] CHECK CONSTRAINT [FK_dbo.AIAgentSessionHistory_dbo.AIAgentSession_AIAgentSessionId]
GO
ALTER TABLE [dbo].[AIAgentSkill]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSkill_dbo.AIAgent_AIAgentId] FOREIGN KEY([AIAgentId])
REFERENCES [dbo].[AIAgent] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AIAgentSkill] CHECK CONSTRAINT [FK_dbo.AIAgentSkill_dbo.AIAgent_AIAgentId]
GO
ALTER TABLE [dbo].[AIAgentSkill]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIAgentSkill_dbo.AISkill_AISkillId] FOREIGN KEY([AISkillId])
REFERENCES [dbo].[AISkill] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AIAgentSkill] CHECK CONSTRAINT [FK_dbo.AIAgentSkill_dbo.AISkill_AISkillId]
GO
ALTER TABLE [dbo].[AIProvider]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIProvider_dbo.EntityType_ProviderComponentEntityTypeId] FOREIGN KEY([ProviderComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AIProvider] CHECK CONSTRAINT [FK_dbo.AIProvider_dbo.EntityType_ProviderComponentEntityTypeId]
GO
ALTER TABLE [dbo].[AIProvider]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIProvider_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AIProvider] CHECK CONSTRAINT [FK_dbo.AIProvider_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AIProvider]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AIProvider_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AIProvider] CHECK CONSTRAINT [FK_dbo.AIProvider_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AISkill]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AISkill_dbo.EntityType_CodeEntityTypeId] FOREIGN KEY([CodeEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AISkill] CHECK CONSTRAINT [FK_dbo.AISkill_dbo.EntityType_CodeEntityTypeId]
GO
ALTER TABLE [dbo].[AISkill]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AISkill_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AISkill] CHECK CONSTRAINT [FK_dbo.AISkill_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AISkill]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AISkill_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AISkill] CHECK CONSTRAINT [FK_dbo.AISkill_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AISkillTool]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AISkillTool_dbo.AISkill_AISkillId] FOREIGN KEY([AISkillId])
REFERENCES [dbo].[AISkill] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AISkillTool] CHECK CONSTRAINT [FK_dbo.AISkillTool_dbo.AISkill_AISkillId]
GO
ALTER TABLE [dbo].[AISkillTool]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AISkillTool_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AISkillTool] CHECK CONSTRAINT [FK_dbo.AISkillTool_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AISkillTool]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AISkillTool_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AISkillTool] CHECK CONSTRAINT [FK_dbo.AISkillTool_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementAttempt]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakAchievementAttempt_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AchievementAttempt] CHECK CONSTRAINT [FK_dbo.StreakAchievementAttempt_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementAttempt]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakAchievementAttempt_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AchievementAttempt] CHECK CONSTRAINT [FK_dbo.StreakAchievementAttempt_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementAttempt]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakAchievementAttempt_dbo.StreakTypeAchievementType_StreakTypeAchievementTypeId] FOREIGN KEY([AchievementTypeId])
REFERENCES [dbo].[AchievementType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AchievementAttempt] CHECK CONSTRAINT [FK_dbo.StreakAchievementAttempt_dbo.StreakTypeAchievementType_StreakTypeAchievementTypeId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AchievementType_dbo.BinaryFile_AlternateImageBinaryFileId] FOREIGN KEY([AlternateImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.AchievementType_dbo.BinaryFile_AlternateImageBinaryFileId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AchievementType_dbo.BinaryFile_ImageBinaryFileId] FOREIGN KEY([ImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.AchievementType_dbo.BinaryFile_ImageBinaryFileId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.EntityType_AchievementEntityTypeId] FOREIGN KEY([ComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.EntityType_AchievementEntityTypeId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.StepStatus_AchievementStepStatusId] FOREIGN KEY([AchievementStepStatusId])
REFERENCES [dbo].[StepStatus] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.StepStatus_AchievementStepStatusId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.StepType_AchievementStepTypeId] FOREIGN KEY([AchievementStepTypeId])
REFERENCES [dbo].[StepType] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.StepType_AchievementStepTypeId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.WorkflowType_AchievementEndWorkflowTypeId] FOREIGN KEY([AchievementFailureWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.WorkflowType_AchievementEndWorkflowTypeId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.WorkflowType_AchievementStartWorkflowTypeId] FOREIGN KEY([AchievementStartWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.WorkflowType_AchievementStartWorkflowTypeId]
GO
ALTER TABLE [dbo].[AchievementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.WorkflowType_AchievementSuccessWorkflowTypeId] FOREIGN KEY([AchievementSuccessWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[AchievementType] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementType_dbo.WorkflowType_AchievementSuccessWorkflowTypeId]
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.StreakTypeAchievementType_PrerequisiteStreakTypeAchievementTypeId] FOREIGN KEY([PrerequisiteAchievementTypeId])
REFERENCES [dbo].[AchievementType] ([Id])
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.StreakTypeAchievementType_PrerequisiteStreakTypeAchievementTypeId]
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.StreakTypeAchievementType_StreakTypeAchievementTypeId] FOREIGN KEY([AchievementTypeId])
REFERENCES [dbo].[AchievementType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AchievementTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StreakTypeAchievementTypePrerequisite_dbo.StreakTypeAchievementType_StreakTypeAchievementTypeId]
GO
ALTER TABLE [dbo].[AdaptiveMessage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessage_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessage] CHECK CONSTRAINT [FK_dbo.AdaptiveMessage_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AdaptiveMessage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessage_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessage] CHECK CONSTRAINT [FK_dbo.AdaptiveMessage_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptation_dbo.AdaptiveMessage_AdaptiveMessageId] FOREIGN KEY([AdaptiveMessageId])
REFERENCES [dbo].[AdaptiveMessage] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptation] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptation_dbo.AdaptiveMessage_AdaptiveMessageId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptation] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptation_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptation] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptation_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.AdaptiveMessageAdaptation_AdaptiveMessageAdaptationId] FOREIGN KEY([AdaptiveMessageAdaptationId])
REFERENCES [dbo].[AdaptiveMessageAdaptation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.AdaptiveMessageAdaptation_AdaptiveMessageAdaptationId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.PersonalizationSegment_PersonalizationSegmentId] FOREIGN KEY([PersonalizationSegmentId])
REFERENCES [dbo].[PersonalizationSegment] ([Id])
GO
ALTER TABLE [dbo].[AdaptiveMessageAdaptationSegment] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageAdaptationSegment_dbo.PersonalizationSegment_PersonalizationSegmentId]
GO
ALTER TABLE [dbo].[AdaptiveMessageCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageCategory_dbo.AdaptiveMessage_AdaptiveMessageId] FOREIGN KEY([AdaptiveMessageId])
REFERENCES [dbo].[AdaptiveMessage] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AdaptiveMessageCategory] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageCategory_dbo.AdaptiveMessage_AdaptiveMessageId]
GO
ALTER TABLE [dbo].[AdaptiveMessageCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AdaptiveMessageCategory_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AdaptiveMessageCategory] CHECK CONSTRAINT [FK_dbo.AdaptiveMessageCategory_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Assessment_dbo.AssessmentType_AssessmentTypeId] FOREIGN KEY([AssessmentTypeId])
REFERENCES [dbo].[AssessmentType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Assessment] CHECK CONSTRAINT [FK_dbo.Assessment_dbo.AssessmentType_AssessmentTypeId]
GO
ALTER TABLE [dbo].[Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Assessment] CHECK CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Assessment] CHECK CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Assessment] CHECK CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_RequesterPersonAliasId] FOREIGN KEY([RequesterPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Assessment] CHECK CONSTRAINT [FK_dbo.Assessment_dbo.PersonAlias_RequesterPersonAliasId]
GO
ALTER TABLE [dbo].[AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AssessmentType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AssessmentType] CHECK CONSTRAINT [FK_dbo.AssessmentType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AssessmentType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AssessmentType] CHECK CONSTRAINT [FK_dbo.AssessmentType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AssetStorageProvider]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AssetStorageSystem_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AssetStorageProvider] CHECK CONSTRAINT [FK_dbo.AssetStorageSystem_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[AssetStorageProvider]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AssetStorageSystem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AssetStorageProvider] CHECK CONSTRAINT [FK_dbo.AssetStorageSystem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AssetStorageProvider]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AssetStorageSystem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AssetStorageProvider] CHECK CONSTRAINT [FK_dbo.AssetStorageSystem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceCheckInSession_AttendanceCheckInSessionId] FOREIGN KEY([AttendanceCheckInSessionId])
REFERENCES [dbo].[AttendanceCheckInSession] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceCheckInSession_AttendanceCheckInSessionId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceCode_AttendanceCodeId] FOREIGN KEY([AttendanceCodeId])
REFERENCES [dbo].[AttendanceCode] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceCode_AttendanceCodeId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceOccurrence_OccurrenceId] FOREIGN KEY([OccurrenceId])
REFERENCES [dbo].[AttendanceOccurrence] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceOccurrence_OccurrenceId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_DeclineReasonValueId] FOREIGN KEY([DeclineReasonValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_DeclineReasonValueId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_QualifierValueId] FOREIGN KEY([QualifierValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_QualifierValueId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_SearchTypeValueId] FOREIGN KEY([SearchTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_SearchTypeValueId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_SourceValueId] FOREIGN KEY([SourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.DefinedValue_SourceValueId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.Device_DeviceId] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Device] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.Device_DeviceId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.Group_SearchResultGroupId] FOREIGN KEY([SearchResultGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.Group_SearchResultGroupId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_CheckedOutByPersonAliasId] FOREIGN KEY([CheckedOutByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_CheckedOutByPersonAliasId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_PresentByPersonAliasId] FOREIGN KEY([PresentByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_PresentByPersonAliasId]
GO
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_ScheduledByPersonAliasId] FOREIGN KEY([ScheduledByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_dbo.Attendance_dbo.PersonAlias_ScheduledByPersonAliasId]
GO
ALTER TABLE [dbo].[AttendanceCheckInSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceCheckInSession_dbo.Device_DeviceId] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Device] ([Id])
GO
ALTER TABLE [dbo].[AttendanceCheckInSession] CHECK CONSTRAINT [FK_dbo.AttendanceCheckInSession_dbo.Device_DeviceId]
GO
ALTER TABLE [dbo].[AttendanceData]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceData_dbo.Attendance_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[Attendance] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttendanceData] CHECK CONSTRAINT [FK_dbo.AttendanceData_dbo.Attendance_Id]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.DefinedValue_AttendanceTypeValueId] FOREIGN KEY([AttendanceTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.DefinedValue_AttendanceTypeValueId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.GroupType_RootGroupTypeId] FOREIGN KEY([RootGroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.GroupType_RootGroupTypeId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[AttendanceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.StepType_StepTypeId] FOREIGN KEY([StepTypeId])
REFERENCES [dbo].[StepType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttendanceOccurrence] CHECK CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.StepType_StepTypeId]
GO
ALTER TABLE [dbo].[Attribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attribute_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Attribute] CHECK CONSTRAINT [FK_dbo.Attribute_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Attribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attribute_dbo.FieldType_FieldTypeId] FOREIGN KEY([FieldTypeId])
REFERENCES [dbo].[FieldType] ([Id])
GO
ALTER TABLE [dbo].[Attribute] CHECK CONSTRAINT [FK_dbo.Attribute_dbo.FieldType_FieldTypeId]
GO
ALTER TABLE [dbo].[Attribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attribute_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attribute] CHECK CONSTRAINT [FK_dbo.Attribute_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Attribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attribute_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Attribute] CHECK CONSTRAINT [FK_dbo.Attribute_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeCategory_dbo.Attribute_AttributeId] FOREIGN KEY([AttributeId])
REFERENCES [dbo].[Attribute] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeCategory] CHECK CONSTRAINT [FK_dbo.AttributeCategory_dbo.Attribute_AttributeId]
GO
ALTER TABLE [dbo].[AttributeCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeCategory_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeCategory] CHECK CONSTRAINT [FK_dbo.AttributeCategory_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[AttributeMatrix]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrix_dbo.AttributeMatrixTemplate_AttributeMatrixTemplateId] FOREIGN KEY([AttributeMatrixTemplateId])
REFERENCES [dbo].[AttributeMatrixTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeMatrix] CHECK CONSTRAINT [FK_dbo.AttributeMatrix_dbo.AttributeMatrixTemplate_AttributeMatrixTemplateId]
GO
ALTER TABLE [dbo].[AttributeMatrix]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrix_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrix] CHECK CONSTRAINT [FK_dbo.AttributeMatrix_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeMatrix]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrix_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrix] CHECK CONSTRAINT [FK_dbo.AttributeMatrix_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeMatrixItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.AttributeMatrixTemplate_AttributeMatrixTemplateId] FOREIGN KEY([AttributeMatrixTemplateId])
REFERENCES [dbo].[AttributeMatrixTemplate] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrixItem] CHECK CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.AttributeMatrixTemplate_AttributeMatrixTemplateId]
GO
ALTER TABLE [dbo].[AttributeMatrixItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.AttributeMatrix_AttributeMatrixId] FOREIGN KEY([AttributeMatrixId])
REFERENCES [dbo].[AttributeMatrix] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrixItem] CHECK CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.AttributeMatrix_AttributeMatrixId]
GO
ALTER TABLE [dbo].[AttributeMatrixItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrixItem] CHECK CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeMatrixItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrixItem] CHECK CONSTRAINT [FK_dbo.AttributeMatrixItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeMatrixTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrixTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrixTemplate] CHECK CONSTRAINT [FK_dbo.AttributeMatrixTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeMatrixTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeMatrixTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeMatrixTemplate] CHECK CONSTRAINT [FK_dbo.AttributeMatrixTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeQualifier]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeQualifier_dbo.Attribute_AttributeId] FOREIGN KEY([AttributeId])
REFERENCES [dbo].[Attribute] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeQualifier] CHECK CONSTRAINT [FK_dbo.AttributeQualifier_dbo.Attribute_AttributeId]
GO
ALTER TABLE [dbo].[AttributeReferencedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeReferencedEntity_dbo.Attribute_AttributeId] FOREIGN KEY([AttributeId])
REFERENCES [dbo].[Attribute] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeReferencedEntity] CHECK CONSTRAINT [FK_dbo.AttributeReferencedEntity_dbo.Attribute_AttributeId]
GO
ALTER TABLE [dbo].[AttributeValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValue_dbo.Attribute_AttributeId] FOREIGN KEY([AttributeId])
REFERENCES [dbo].[Attribute] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeValue] CHECK CONSTRAINT [FK_dbo.AttributeValue_dbo.Attribute_AttributeId]
GO
ALTER TABLE [dbo].[AttributeValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValue_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeValue] CHECK CONSTRAINT [FK_dbo.AttributeValue_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValue_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeValue] CHECK CONSTRAINT [FK_dbo.AttributeValue_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeValueHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValueHistorical_dbo.AttributeValue_AttributeValueId] FOREIGN KEY([AttributeValueId])
REFERENCES [dbo].[AttributeValue] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeValueHistorical] CHECK CONSTRAINT [FK_dbo.AttributeValueHistorical_dbo.AttributeValue_AttributeValueId]
GO
ALTER TABLE [dbo].[AttributeValueHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValueHistorical_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeValueHistorical] CHECK CONSTRAINT [FK_dbo.AttributeValueHistorical_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeValueHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValueHistorical_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AttributeValueHistorical] CHECK CONSTRAINT [FK_dbo.AttributeValueHistorical_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AttributeValueReferencedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AttributeValueReferencedEntity_dbo.AttributeValue_AttributeValueId] FOREIGN KEY([AttributeValueId])
REFERENCES [dbo].[AttributeValue] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AttributeValueReferencedEntity] CHECK CONSTRAINT [FK_dbo.AttributeValueReferencedEntity_dbo.AttributeValue_AttributeValueId]
GO
ALTER TABLE [dbo].[Audit]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Audit_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Audit] CHECK CONSTRAINT [FK_dbo.Audit_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Audit]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Audit_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Audit] CHECK CONSTRAINT [FK_dbo.Audit_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[AuditDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuditDetail_dbo.Audit_AuditId] FOREIGN KEY([AuditId])
REFERENCES [dbo].[Audit] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AuditDetail] CHECK CONSTRAINT [FK_dbo.AuditDetail_dbo.Audit_AuditId]
GO
ALTER TABLE [dbo].[Auth]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Auth_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Auth] CHECK CONSTRAINT [FK_dbo.Auth_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Auth]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Auth_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Auth] CHECK CONSTRAINT [FK_dbo.Auth_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[Auth]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Auth_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Auth] CHECK CONSTRAINT [FK_dbo.Auth_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Auth]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Auth_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Auth] CHECK CONSTRAINT [FK_dbo.Auth_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Auth]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Auth_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Auth] CHECK CONSTRAINT [FK_dbo.Auth_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[AuthAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthAuditLog_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AuthAuditLog] CHECK CONSTRAINT [FK_dbo.AuthAuditLog_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[AuthAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthAuditLog_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[AuthAuditLog] CHECK CONSTRAINT [FK_dbo.AuthAuditLog_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[AuthAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthAuditLog_dbo.PersonAlias_ChangeByPersonAliasId] FOREIGN KEY([ChangeByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AuthAuditLog] CHECK CONSTRAINT [FK_dbo.AuthAuditLog_dbo.PersonAlias_ChangeByPersonAliasId]
GO
ALTER TABLE [dbo].[AuthAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthAuditLog_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthAuditLog] CHECK CONSTRAINT [FK_dbo.AuthAuditLog_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[AuthClaim]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthClaim_dbo.AuthScope_ScopeId] FOREIGN KEY([ScopeId])
REFERENCES [dbo].[AuthScope] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AuthClaim] CHECK CONSTRAINT [FK_dbo.AuthClaim_dbo.AuthScope_ScopeId]
GO
ALTER TABLE [dbo].[AuthClaim]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthClaim_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthClaim] CHECK CONSTRAINT [FK_dbo.AuthClaim_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AuthClaim]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthClaim_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthClaim] CHECK CONSTRAINT [FK_dbo.AuthClaim_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AuthClient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthClient_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthClient] CHECK CONSTRAINT [FK_dbo.AuthClient_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AuthClient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthClient_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthClient] CHECK CONSTRAINT [FK_dbo.AuthClient_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AuthScope]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthScope_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthScope] CHECK CONSTRAINT [FK_dbo.AuthScope_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AuthScope]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AuthScope_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AuthScope] CHECK CONSTRAINT [FK_dbo.AuthScope_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AutomationEvent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationEvent_dbo.AutomationTrigger_AutomationTriggerId] FOREIGN KEY([AutomationTriggerId])
REFERENCES [dbo].[AutomationTrigger] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AutomationEvent] CHECK CONSTRAINT [FK_dbo.AutomationEvent_dbo.AutomationTrigger_AutomationTriggerId]
GO
ALTER TABLE [dbo].[AutomationEvent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationEvent_dbo.EntityType_ComponentEntityTypeId] FOREIGN KEY([ComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AutomationEvent] CHECK CONSTRAINT [FK_dbo.AutomationEvent_dbo.EntityType_ComponentEntityTypeId]
GO
ALTER TABLE [dbo].[AutomationEvent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationEvent_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AutomationEvent] CHECK CONSTRAINT [FK_dbo.AutomationEvent_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AutomationEvent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationEvent_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AutomationEvent] CHECK CONSTRAINT [FK_dbo.AutomationEvent_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[AutomationTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationTrigger_dbo.EntityType_ComponentEntityTypeId] FOREIGN KEY([ComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[AutomationTrigger] CHECK CONSTRAINT [FK_dbo.AutomationTrigger_dbo.EntityType_ComponentEntityTypeId]
GO
ALTER TABLE [dbo].[AutomationTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationTrigger_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AutomationTrigger] CHECK CONSTRAINT [FK_dbo.AutomationTrigger_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[AutomationTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AutomationTrigger_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[AutomationTrigger] CHECK CONSTRAINT [FK_dbo.AutomationTrigger_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.BinaryFile_ResponseDocumentId] FOREIGN KEY([ResponseDocumentId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.BinaryFile_ResponseDocumentId]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.ConnectionRequest] FOREIGN KEY([ConnectionRequestId])
REFERENCES [dbo].[ConnectionRequest] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.ConnectionRequest]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.EntityType_ProcessorEntityTypeId] FOREIGN KEY([ProcessorEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.EntityType_ProcessorEntityTypeId]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BackgroundCheck_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BackgroundCheck] CHECK CONSTRAINT [FK_dbo.BackgroundCheck_dbo.Workflow_WorkflowId]
GO
ALTER TABLE [dbo].[Badge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Badge_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Badge] CHECK CONSTRAINT [FK_dbo.Badge_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Badge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonBadge_dbo.EntityType_EntityTypeId] FOREIGN KEY([BadgeComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Badge] CHECK CONSTRAINT [FK_dbo.PersonBadge_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Badge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonBadge_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Badge] CHECK CONSTRAINT [FK_dbo.PersonBadge_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Badge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonBadge_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Badge] CHECK CONSTRAINT [FK_dbo.PersonBadge_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.BenevolenceType_BenevolenceTypeId] FOREIGN KEY([BenevolenceTypeId])
REFERENCES [dbo].[BenevolenceType] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.BenevolenceType_BenevolenceTypeId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.DefinedValue_ConnectionStatusValueId] FOREIGN KEY([ConnectionStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.DefinedValue_ConnectionStatusValueId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.DefinedValue_RequestStatusValueId] FOREIGN KEY([RequestStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.DefinedValue_RequestStatusValueId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_CaseWorkerPersonAliasId] FOREIGN KEY([CaseWorkerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_CaseWorkerPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_RequestedByPersonAliasId] FOREIGN KEY([RequestedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequest] CHECK CONSTRAINT [FK_dbo.BenevolenceRequest_dbo.PersonAlias_RequestedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.BenevolenceRequest_BenevolenceRequestId] FOREIGN KEY([BenevolenceRequestId])
REFERENCES [dbo].[BenevolenceRequest] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument] CHECK CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.BenevolenceRequest_BenevolenceRequestId]
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument] CHECK CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument] CHECK CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceRequestDocument] CHECK CONSTRAINT [FK_dbo.BenevolenceRequestDocument_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceResult]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceResult_dbo.BenevolenceRequest_BenevolenceRequestId] FOREIGN KEY([BenevolenceRequestId])
REFERENCES [dbo].[BenevolenceRequest] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BenevolenceResult] CHECK CONSTRAINT [FK_dbo.BenevolenceResult_dbo.BenevolenceRequest_BenevolenceRequestId]
GO
ALTER TABLE [dbo].[BenevolenceResult]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceResult_dbo.DefinedValue_ResultTypeValueId] FOREIGN KEY([ResultTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceResult] CHECK CONSTRAINT [FK_dbo.BenevolenceResult_dbo.DefinedValue_ResultTypeValueId]
GO
ALTER TABLE [dbo].[BenevolenceResult]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceResult_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceResult] CHECK CONSTRAINT [FK_dbo.BenevolenceResult_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceResult]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceResult_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceResult] CHECK CONSTRAINT [FK_dbo.BenevolenceResult_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceType] CHECK CONSTRAINT [FK_dbo.BenevolenceType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceType] CHECK CONSTRAINT [FK_dbo.BenevolenceType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.BenevolenceType_BenevolenceTypeId] FOREIGN KEY([BenevolenceTypeId])
REFERENCES [dbo].[BenevolenceType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BenevolenceWorkflow] CHECK CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.BenevolenceType_BenevolenceTypeId]
GO
ALTER TABLE [dbo].[BenevolenceWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceWorkflow] CHECK CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BenevolenceWorkflow] CHECK CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BenevolenceWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BenevolenceWorkflow] CHECK CONSTRAINT [FK_dbo.BenevolenceWorkflow_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[BinaryFile]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFile_dbo.BinaryFileType_BinaryFileTypeId] FOREIGN KEY([BinaryFileTypeId])
REFERENCES [dbo].[BinaryFileType] ([Id])
GO
ALTER TABLE [dbo].[BinaryFile] CHECK CONSTRAINT [FK_dbo.BinaryFile_dbo.BinaryFileType_BinaryFileTypeId]
GO
ALTER TABLE [dbo].[BinaryFile]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFile_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BinaryFile] CHECK CONSTRAINT [FK_dbo.BinaryFile_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BinaryFile]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFile_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BinaryFile] CHECK CONSTRAINT [FK_dbo.BinaryFile_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BinaryFileData]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFileData_dbo.BinaryFile_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[BinaryFile] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BinaryFileData] CHECK CONSTRAINT [FK_dbo.BinaryFileData_dbo.BinaryFile_Id]
GO
ALTER TABLE [dbo].[BinaryFileData]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFileData_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BinaryFileData] CHECK CONSTRAINT [FK_dbo.BinaryFileData_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BinaryFileData]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFileData_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BinaryFileData] CHECK CONSTRAINT [FK_dbo.BinaryFileData_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[BinaryFileType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFileType_dbo.EntityType_StorageEntityTypeId] FOREIGN KEY([StorageEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[BinaryFileType] CHECK CONSTRAINT [FK_dbo.BinaryFileType_dbo.EntityType_StorageEntityTypeId]
GO
ALTER TABLE [dbo].[BinaryFileType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFileType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BinaryFileType] CHECK CONSTRAINT [FK_dbo.BinaryFileType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BinaryFileType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BinaryFileType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BinaryFileType] CHECK CONSTRAINT [FK_dbo.BinaryFileType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Block]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Block_dbo.BlockType_BlockTypeId] FOREIGN KEY([BlockTypeId])
REFERENCES [dbo].[BlockType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Block] CHECK CONSTRAINT [FK_dbo.Block_dbo.BlockType_BlockTypeId]
GO
ALTER TABLE [dbo].[Block]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Block_dbo.Layout_LayoutId] FOREIGN KEY([LayoutId])
REFERENCES [dbo].[Layout] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Block] CHECK CONSTRAINT [FK_dbo.Block_dbo.Layout_LayoutId]
GO
ALTER TABLE [dbo].[Block]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Block_dbo.Page_PageId] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Block] CHECK CONSTRAINT [FK_dbo.Block_dbo.Page_PageId]
GO
ALTER TABLE [dbo].[Block]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Block_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Block] CHECK CONSTRAINT [FK_dbo.Block_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Block]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Block_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Block] CHECK CONSTRAINT [FK_dbo.Block_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Block]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Block_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[Block] CHECK CONSTRAINT [FK_dbo.Block_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[BlockType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BlockType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[BlockType] CHECK CONSTRAINT [FK_dbo.BlockType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[BlockType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BlockType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BlockType] CHECK CONSTRAINT [FK_dbo.BlockType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[BlockType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.BlockType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[BlockType] CHECK CONSTRAINT [FK_dbo.BlockType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.DefinedValue_CampusStatusValueId] FOREIGN KEY([CampusStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.DefinedValue_CampusStatusValueId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.DefinedValue_CampusTypeValueId] FOREIGN KEY([CampusTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.DefinedValue_CampusTypeValueId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.Group_TeamGroupId] FOREIGN KEY([TeamGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.Group_TeamGroupId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.PersonAlias_LeaderPersonAliasId] FOREIGN KEY([LeaderPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.PersonAlias_LeaderPersonAliasId]
GO
ALTER TABLE [dbo].[Campus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Campus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Campus] CHECK CONSTRAINT [FK_dbo.Campus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CampusSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusSchedule_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CampusSchedule] CHECK CONSTRAINT [FK_dbo.CampusSchedule_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[CampusSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusSchedule_dbo.DefinedValue_ScheduleTypeValueId] FOREIGN KEY([ScheduleTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[CampusSchedule] CHECK CONSTRAINT [FK_dbo.CampusSchedule_dbo.DefinedValue_ScheduleTypeValueId]
GO
ALTER TABLE [dbo].[CampusSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusSchedule_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CampusSchedule] CHECK CONSTRAINT [FK_dbo.CampusSchedule_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CampusSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusSchedule_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CampusSchedule] CHECK CONSTRAINT [FK_dbo.CampusSchedule_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CampusSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusSchedule_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CampusSchedule] CHECK CONSTRAINT [FK_dbo.CampusSchedule_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[CampusTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusTopic_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CampusTopic] CHECK CONSTRAINT [FK_dbo.CampusTopic_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[CampusTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusTopic_dbo.DefinedValue_TopicTypeValueId] FOREIGN KEY([TopicTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[CampusTopic] CHECK CONSTRAINT [FK_dbo.CampusTopic_dbo.DefinedValue_TopicTypeValueId]
GO
ALTER TABLE [dbo].[CampusTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusTopic_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CampusTopic] CHECK CONSTRAINT [FK_dbo.CampusTopic_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CampusTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CampusTopic_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CampusTopic] CHECK CONSTRAINT [FK_dbo.CampusTopic_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Category_dbo.Category_ParentCategoryId] FOREIGN KEY([ParentCategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_dbo.Category_dbo.Category_ParentCategoryId]
GO
ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Category_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_dbo.Category_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Category_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_dbo.Category_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Category_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_dbo.Category_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CheckInLabel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CheckInLabel_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CheckInLabel] CHECK CONSTRAINT [FK_dbo.CheckInLabel_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CheckInLabel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CheckInLabel_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CheckInLabel] CHECK CONSTRAINT [FK_dbo.CheckInLabel_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.CommunicationTemplate_CommunicationTemplateId] FOREIGN KEY([CommunicationTemplateId])
REFERENCES [dbo].[CommunicationTemplate] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.CommunicationTemplate_CommunicationTemplateId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.DefinedValue_CommunicationTopicValueId] FOREIGN KEY([CommunicationTopicValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.DefinedValue_CommunicationTopicValueId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.Group_ListGroupId] FOREIGN KEY([ListGroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.Group_ListGroupId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_ReviewerPersonAliasId] FOREIGN KEY([ReviewerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_ReviewerPersonAliasId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_SenderPersonAliasId] FOREIGN KEY([SenderPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.PersonAlias_SenderPersonAliasId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.SystemCommunication_SystemCommunicationId] FOREIGN KEY([SystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.SystemCommunication_SystemCommunicationId]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Communication_dbo.SystemPhoneNumber_SmsFromSystemPhoneNumberId] FOREIGN KEY([SmsFromSystemPhoneNumberId])
REFERENCES [dbo].[SystemPhoneNumber] ([Id])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_dbo.Communication_dbo.SystemPhoneNumber_SmsFromSystemPhoneNumberId]
GO
ALTER TABLE [dbo].[CommunicationAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[CommunicationAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[CommunicationAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.Communication_CommunicationId] FOREIGN KEY([CommunicationId])
REFERENCES [dbo].[Communication] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.Communication_CommunicationId]
GO
ALTER TABLE [dbo].[CommunicationAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationAttachment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlow_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlow] CHECK CONSTRAINT [FK_dbo.CommunicationFlow_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[CommunicationFlow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlow_dbo.DataView_TargetAudienceDataViewId] FOREIGN KEY([TargetAudienceDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlow] CHECK CONSTRAINT [FK_dbo.CommunicationFlow_dbo.DataView_TargetAudienceDataViewId]
GO
ALTER TABLE [dbo].[CommunicationFlow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlow_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlow] CHECK CONSTRAINT [FK_dbo.CommunicationFlow_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlow_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlow] CHECK CONSTRAINT [FK_dbo.CommunicationFlow_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlow_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlow] CHECK CONSTRAINT [FK_dbo.CommunicationFlow_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.CommunicationFlow_CommunicationFlowId] FOREIGN KEY([CommunicationFlowId])
REFERENCES [dbo].[CommunicationFlow] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.CommunicationFlow_CommunicationFlowId]
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.CommunicationTemplate_CommunicationTemplateId] FOREIGN KEY([CommunicationTemplateId])
REFERENCES [dbo].[CommunicationTemplate] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.CommunicationTemplate_CommunicationTemplateId]
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowCommunication_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstance_dbo.CommunicationFlow_CommunicationFlowId] FOREIGN KEY([CommunicationFlowId])
REFERENCES [dbo].[CommunicationFlow] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationFlowInstance] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstance_dbo.CommunicationFlow_CommunicationFlowId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstance_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstance] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstance_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstance_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstance] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstance_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.CommunicationFlowCommunication_CommunicationFlowCommunicationId] FOREIGN KEY([CommunicationFlowCommunicationId])
REFERENCES [dbo].[CommunicationFlowCommunication] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.CommunicationFlowCommunication_CommunicationFlowCommunicationId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.CommunicationFlowInstance_CommunicationFlowInstanceId] FOREIGN KEY([CommunicationFlowInstanceId])
REFERENCES [dbo].[CommunicationFlowInstance] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.CommunicationFlowInstance_CommunicationFlowInstanceId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.Communication_CommunicationId] FOREIGN KEY([CommunicationId])
REFERENCES [dbo].[Communication] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.Communication_CommunicationId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunication] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunication_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.CommunicationFlowInstanceCommunication_CommunicationFlowInstanceComm] FOREIGN KEY([CommunicationFlowInstanceCommunicationId])
REFERENCES [dbo].[CommunicationFlowInstanceCommunication] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.CommunicationFlowInstanceCommunication_CommunicationFlowInstanceComm]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.CommunicationRecipient_CommunicationRecipientId] FOREIGN KEY([CommunicationRecipientId])
REFERENCES [dbo].[CommunicationRecipient] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.CommunicationRecipient_CommunicationRecipientId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceCommunicationConversion] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceCommunicationConversion_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.CommunicationFlowInstance_CommunicationFlowInstanceId] FOREIGN KEY([CommunicationFlowInstanceId])
REFERENCES [dbo].[CommunicationFlowInstance] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.CommunicationFlowInstance_CommunicationFlowInstanceId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.CommunicationRecipient_UnsubscribeCommunicationRecipientId] FOREIGN KEY([UnsubscribeCommunicationRecipientId])
REFERENCES [dbo].[CommunicationRecipient] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.CommunicationRecipient_UnsubscribeCommunicationRecipientId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.PersonAlias_RecipientPersonAliasId] FOREIGN KEY([RecipientPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationFlowInstanceRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationFlowInstanceRecipient_dbo.PersonAlias_RecipientPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.Communication_CommunicationId] FOREIGN KEY([CommunicationId])
REFERENCES [dbo].[Communication] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.Communication_CommunicationId]
GO
ALTER TABLE [dbo].[CommunicationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonalDevice_PersonalDeviceId] FOREIGN KEY([PersonalDeviceId])
REFERENCES [dbo].[PersonalDevice] ([Id])
GO
ALTER TABLE [dbo].[CommunicationRecipient] CHECK CONSTRAINT [FK_dbo.CommunicationRecipient_dbo.PersonalDevice_PersonalDeviceId]
GO
ALTER TABLE [dbo].[CommunicationRecipientActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipientActivity_dbo.CommunicationRecipient_CommunicationRecipientId] FOREIGN KEY([CommunicationRecipientId])
REFERENCES [dbo].[CommunicationRecipient] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationRecipientActivity] CHECK CONSTRAINT [FK_dbo.CommunicationRecipientActivity_dbo.CommunicationRecipient_CommunicationRecipientId]
GO
ALTER TABLE [dbo].[CommunicationRecipientActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipientActivity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationRecipientActivity] CHECK CONSTRAINT [FK_dbo.CommunicationRecipientActivity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationRecipientActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationRecipientActivity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationRecipientActivity] CHECK CONSTRAINT [FK_dbo.CommunicationRecipientActivity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.Communication_RelatedCommunicationId] FOREIGN KEY([RelatedCommunicationId])
REFERENCES [dbo].[Communication] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.Communication_RelatedCommunicationId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.EntityType_RelatedMediumEntityTypeId] FOREIGN KEY([RelatedMediumEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.EntityType_RelatedMediumEntityTypeId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.EntityType_RelatedTransportEntityTypeId] FOREIGN KEY([RelatedTransportEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.EntityType_RelatedTransportEntityTypeId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_FromPersonAliasId] FOREIGN KEY([FromPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_FromPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_ToPersonAliasId] FOREIGN KEY([ToPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.PersonAlias_ToPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationResponse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponse_dbo.SystemPhoneNumber_RelatedSmsFromSystemPhoneNumberId] FOREIGN KEY([RelatedSmsFromSystemPhoneNumberId])
REFERENCES [dbo].[SystemPhoneNumber] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponse] CHECK CONSTRAINT [FK_dbo.CommunicationResponse_dbo.SystemPhoneNumber_RelatedSmsFromSystemPhoneNumberId]
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.CommunicationResponse_CommunicationResponseId] FOREIGN KEY([CommunicationResponseId])
REFERENCES [dbo].[CommunicationResponse] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.CommunicationResponse_CommunicationResponseId]
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationResponseAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationResponseAttachment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.BinaryFile_ImageFileId] FOREIGN KEY([ImageFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.BinaryFile_ImageFileId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.BinaryFile_LogoBinaryFileId] FOREIGN KEY([LogoBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.BinaryFile_LogoBinaryFileId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.BinaryFile_PushImageBinaryFileId] FOREIGN KEY([PushImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.BinaryFile_PushImageBinaryFileId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.PersonAlias_SenderPersonAliasId] FOREIGN KEY([SenderPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.PersonAlias_SenderPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.SystemPhoneNumber_SmsFromSystemPhoneNumberId] FOREIGN KEY([SmsFromSystemPhoneNumberId])
REFERENCES [dbo].[SystemPhoneNumber] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplate] CHECK CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.SystemPhoneNumber_SmsFromSystemPhoneNumberId]
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.CommunicationTemplate_CommunicationTemplateId] FOREIGN KEY([CommunicationTemplateId])
REFERENCES [dbo].[CommunicationTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.CommunicationTemplate_CommunicationTemplateId]
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[CommunicationTemplateAttachment] CHECK CONSTRAINT [FK_dbo.CommunicationTemplateAttachment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.ConnectionType_ConnectionTypeId] FOREIGN KEY([ConnectionTypeId])
REFERENCES [dbo].[ConnectionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionActivityType] CHECK CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.ConnectionType_ConnectionTypeId]
GO
ALTER TABLE [dbo].[ConnectionActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.NoteType_PersonNoteTypeId] FOREIGN KEY([PersonNoteTypeId])
REFERENCES [dbo].[NoteType] ([Id])
GO
ALTER TABLE [dbo].[ConnectionActivityType] CHECK CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.NoteType_PersonNoteTypeId]
GO
ALTER TABLE [dbo].[ConnectionActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionActivityType] CHECK CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionActivityType] CHECK CONSTRAINT [FK_dbo.ConnectionActivityType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.BinaryFile_PhotoId] FOREIGN KEY([PhotoId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunity] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.BinaryFile_PhotoId]
GO
ALTER TABLE [dbo].[ConnectionOpportunity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.ConnectionType_ConnectionTypeId] FOREIGN KEY([ConnectionTypeId])
REFERENCES [dbo].[ConnectionType] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunity] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.ConnectionType_ConnectionTypeId]
GO
ALTER TABLE [dbo].[ConnectionOpportunity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunity] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunity] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.PersonAlias_DefaultConnectorPersonAliasId] FOREIGN KEY([DefaultConnectorPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.PersonAlias_DefaultConnectorPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityCampus] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityCampus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.Group_ConnectorGroupId] FOREIGN KEY([ConnectorGroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.Group_ConnectorGroupId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityConnectorGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupCampus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroup] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroup_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.GroupTypeRole_GroupMemberRoleId] FOREIGN KEY([GroupMemberRoleId])
REFERENCES [dbo].[GroupTypeRole] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.GroupTypeRole_GroupMemberRoleId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionOpportunityGroupConfig] CHECK CONSTRAINT [FK_dbo.ConnectionOpportunityGroupConfig_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionStatus_ConnectionStatusId] FOREIGN KEY([ConnectionStatusId])
REFERENCES [dbo].[ConnectionStatus] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionStatus_ConnectionStatusId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionTypeSource_ConnectionTypeSourceId] FOREIGN KEY([ConnectionTypeSourceId])
REFERENCES [dbo].[ConnectionTypeSource] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionTypeSource_ConnectionTypeSourceId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionType_ConnectionTypeId] FOREIGN KEY([ConnectionTypeId])
REFERENCES [dbo].[ConnectionType] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.ConnectionType_ConnectionTypeId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.Group_AssignedGroupId] FOREIGN KEY([AssignedGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.Group_AssignedGroupId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_ConnectorPersonAliasId] FOREIGN KEY([ConnectorPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_ConnectorPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequest] CHECK CONSTRAINT [FK_dbo.ConnectionRequest_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.ConnectionActivityType_ConnectionActivityTypeId] FOREIGN KEY([ConnectionActivityTypeId])
REFERENCES [dbo].[ConnectionActivityType] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestActivity] CHECK CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.ConnectionActivityType_ConnectionActivityTypeId]
GO
ALTER TABLE [dbo].[ConnectionRequestActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestActivity] CHECK CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionRequestActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.ConnectionRequest_ConnectionRequestId] FOREIGN KEY([ConnectionRequestId])
REFERENCES [dbo].[ConnectionRequest] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestActivity] CHECK CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.ConnectionRequest_ConnectionRequestId]
GO
ALTER TABLE [dbo].[ConnectionRequestActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.PersonAlias_ConnectorPersonAliasId] FOREIGN KEY([ConnectorPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestActivity] CHECK CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.PersonAlias_ConnectorPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestActivity] CHECK CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestActivity] CHECK CONSTRAINT [FK_dbo.ConnectionRequestActivity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.ConnectionRequest_ConnectionRequestId] FOREIGN KEY([ConnectionRequestId])
REFERENCES [dbo].[ConnectionRequest] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory] CHECK CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.ConnectionRequest_ConnectionRequestId]
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.ConnectionStatus_ConnectionStatusId] FOREIGN KEY([ConnectionStatusId])
REFERENCES [dbo].[ConnectionStatus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory] CHECK CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.ConnectionStatus_ConnectionStatusId]
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.ConnectionStatus_PreviousConnectionStatusId] FOREIGN KEY([PreviousConnectionStatusId])
REFERENCES [dbo].[ConnectionStatus] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory] CHECK CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.ConnectionStatus_PreviousConnectionStatusId]
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.PersonAlias_CompletedByPersonAliasId] FOREIGN KEY([CompletedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory] CHECK CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.PersonAlias_CompletedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory] CHECK CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestStatusHistory] CHECK CONSTRAINT [FK_dbo.ConnectionRequestStatusHistory_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.ConnectionRequest_ConnectionRequestId] FOREIGN KEY([ConnectionRequestId])
REFERENCES [dbo].[ConnectionRequest] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.ConnectionRequest_ConnectionRequestId]
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.ConnectionWorkflow_ConnectionWorkflowId] FOREIGN KEY([ConnectionWorkflowId])
REFERENCES [dbo].[ConnectionWorkflow] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.ConnectionWorkflow_ConnectionWorkflowId]
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
GO
ALTER TABLE [dbo].[ConnectionRequestWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionRequestWorkflow_dbo.Workflow_WorkflowId]
GO
ALTER TABLE [dbo].[ConnectionStatus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatus_dbo.ConnectionType_ConnectionTypeId] FOREIGN KEY([ConnectionTypeId])
REFERENCES [dbo].[ConnectionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionStatus] CHECK CONSTRAINT [FK_dbo.ConnectionStatus_dbo.ConnectionType_ConnectionTypeId]
GO
ALTER TABLE [dbo].[ConnectionStatus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionStatus] CHECK CONSTRAINT [FK_dbo.ConnectionStatus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionStatus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionStatus] CHECK CONSTRAINT [FK_dbo.ConnectionStatus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.ConnectionStatus_DestinationStatusId] FOREIGN KEY([DestinationStatusId])
REFERENCES [dbo].[ConnectionStatus] ([Id])
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation] CHECK CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.ConnectionStatus_DestinationStatusId]
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.ConnectionStatus_SourceStatusId] FOREIGN KEY([SourceStatusId])
REFERENCES [dbo].[ConnectionStatus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation] CHECK CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.ConnectionStatus_SourceStatusId]
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation] CHECK CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation] CHECK CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionStatusAutomation] CHECK CONSTRAINT [FK_dbo.ConnectionStatusAutomation_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionType_dbo.Category_SnippetCategoryId] FOREIGN KEY([SnippetCategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[ConnectionType] CHECK CONSTRAINT [FK_dbo.ConnectionType_dbo.Category_SnippetCategoryId]
GO
ALTER TABLE [dbo].[ConnectionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionType_dbo.PageRoute_ConnectionRequestDetailPageRouteId] FOREIGN KEY([ConnectionRequestDetailPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[ConnectionType] CHECK CONSTRAINT [FK_dbo.ConnectionType_dbo.PageRoute_ConnectionRequestDetailPageRouteId]
GO
ALTER TABLE [dbo].[ConnectionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionType_dbo.Page_ConnectionRequestDetailPageId] FOREIGN KEY([ConnectionRequestDetailPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[ConnectionType] CHECK CONSTRAINT [FK_dbo.ConnectionType_dbo.Page_ConnectionRequestDetailPageId]
GO
ALTER TABLE [dbo].[ConnectionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionType] CHECK CONSTRAINT [FK_dbo.ConnectionType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionType] CHECK CONSTRAINT [FK_dbo.ConnectionType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionType_dbo.PersonAlias_OwnerPersonAliasId] FOREIGN KEY([OwnerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionType] CHECK CONSTRAINT [FK_dbo.ConnectionType_dbo.PersonAlias_OwnerPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionTypeSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionTypeSource_dbo.ConnectionType_ConnectionTypeId] FOREIGN KEY([ConnectionTypeId])
REFERENCES [dbo].[ConnectionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionTypeSource] CHECK CONSTRAINT [FK_dbo.ConnectionTypeSource_dbo.ConnectionType_ConnectionTypeId]
GO
ALTER TABLE [dbo].[ConnectionTypeSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionTypeSource_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionTypeSource] CHECK CONSTRAINT [FK_dbo.ConnectionTypeSource_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionTypeSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionTypeSource_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionTypeSource] CHECK CONSTRAINT [FK_dbo.ConnectionTypeSource_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.ConnectionStatus_ManualTriggerFilterConnectionStatusId] FOREIGN KEY([ManualTriggerFilterConnectionStatusId])
REFERENCES [dbo].[ConnectionStatus] ([Id])
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.ConnectionStatus_ManualTriggerFilterConnectionStatusId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.ConnectionType_ConnectionTypeId] FOREIGN KEY([ConnectionTypeId])
REFERENCES [dbo].[ConnectionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.ConnectionType_ConnectionTypeId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.DataView_ExcludeDataViewId] FOREIGN KEY([ExcludeDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.DataView_ExcludeDataViewId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.DataView_IncludeDataViewId] FOREIGN KEY([IncludeDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.DataView_IncludeDataViewId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ConnectionWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ConnectionWorkflow] CHECK CONSTRAINT [FK_dbo.ConnectionWorkflow_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[Contact]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Contact_dbo.BinaryFile_PhotoId] FOREIGN KEY([PhotoId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Contact] CHECK CONSTRAINT [FK_dbo.Contact_dbo.BinaryFile_PhotoId]
GO
ALTER TABLE [dbo].[Contact]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Contact_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Contact] CHECK CONSTRAINT [FK_dbo.Contact_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Contact]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Contact_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Contact] CHECK CONSTRAINT [FK_dbo.Contact_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Contact]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Contact_dbo.PersonAlias_OwnerPersonAliasId] FOREIGN KEY([OwnerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Contact] CHECK CONSTRAINT [FK_dbo.Contact_dbo.PersonAlias_OwnerPersonAliasId]
GO
ALTER TABLE [dbo].[ContactRelationshipChange]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContactRelationshipChanges_dbo.Contact_ContactId] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contact] ([Id])
GO
ALTER TABLE [dbo].[ContactRelationshipChange] CHECK CONSTRAINT [FK_dbo.ContactRelationshipChanges_dbo.Contact_ContactId]
GO
ALTER TABLE [dbo].[ContactTouchpoint]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContactTouchpoint_dbo.Contact_ContactId] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contact] ([Id])
GO
ALTER TABLE [dbo].[ContactTouchpoint] CHECK CONSTRAINT [FK_dbo.ContactTouchpoint_dbo.Contact_ContactId]
GO
ALTER TABLE [dbo].[ContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannel_dbo.Category_ItemTagCategoryId] FOREIGN KEY([ItemTagCategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[ContentChannel] CHECK CONSTRAINT [FK_dbo.ContentChannel_dbo.Category_ItemTagCategoryId]
GO
ALTER TABLE [dbo].[ContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannel_dbo.ContentType_ContentTypeId] FOREIGN KEY([ContentChannelTypeId])
REFERENCES [dbo].[ContentChannelType] ([Id])
GO
ALTER TABLE [dbo].[ContentChannel] CHECK CONSTRAINT [FK_dbo.ContentChannel_dbo.ContentType_ContentTypeId]
GO
ALTER TABLE [dbo].[ContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannel_dbo.DefinedValue_StructuredContentToolValueId] FOREIGN KEY([StructuredContentToolValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[ContentChannel] CHECK CONSTRAINT [FK_dbo.ContentChannel_dbo.DefinedValue_StructuredContentToolValueId]
GO
ALTER TABLE [dbo].[ContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannel_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannel] CHECK CONSTRAINT [FK_dbo.ContentChannel_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannel_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannel] CHECK CONSTRAINT [FK_dbo.ContentChannel_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelAssociation_dbo.ContentChannel_ChildContentChannelId] FOREIGN KEY([ChildContentChannelId])
REFERENCES [dbo].[ContentChannel] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelAssociation] CHECK CONSTRAINT [FK_dbo.ContentChannelAssociation_dbo.ContentChannel_ChildContentChannelId]
GO
ALTER TABLE [dbo].[ContentChannelAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelAssociation_dbo.ContentChannel_ContentChannelId] FOREIGN KEY([ContentChannelId])
REFERENCES [dbo].[ContentChannel] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelAssociation] CHECK CONSTRAINT [FK_dbo.ContentChannelAssociation_dbo.ContentChannel_ContentChannelId]
GO
ALTER TABLE [dbo].[ContentChannelCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelCategory_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContentChannelCategory] CHECK CONSTRAINT [FK_dbo.ContentChannelCategory_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[ContentChannelCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelCategory_dbo.ContentChannel_ContentChannelId] FOREIGN KEY([ContentChannelId])
REFERENCES [dbo].[ContentChannel] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContentChannelCategory] CHECK CONSTRAINT [FK_dbo.ContentChannelCategory_dbo.ContentChannel_ContentChannelId]
GO
ALTER TABLE [dbo].[ContentChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItem_dbo.PersonAlias_ContentLibraryUploadedByPersonAliasId] FOREIGN KEY([ContentLibraryUploadedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItem] CHECK CONSTRAINT [FK_dbo.ContentChannelItem_dbo.PersonAlias_ContentLibraryUploadedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentItem_dbo.ContentChannel_ContentChannelId] FOREIGN KEY([ContentChannelId])
REFERENCES [dbo].[ContentChannel] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItem] CHECK CONSTRAINT [FK_dbo.ContentItem_dbo.ContentChannel_ContentChannelId]
GO
ALTER TABLE [dbo].[ContentChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentItem_dbo.ContentType_ContentTypeId] FOREIGN KEY([ContentChannelTypeId])
REFERENCES [dbo].[ContentChannelType] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItem] CHECK CONSTRAINT [FK_dbo.ContentItem_dbo.ContentType_ContentTypeId]
GO
ALTER TABLE [dbo].[ContentChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentItem_dbo.PersonAlias_ApprovedByPersonAliasId] FOREIGN KEY([ApprovedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItem] CHECK CONSTRAINT [FK_dbo.ContentItem_dbo.PersonAlias_ApprovedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItem] CHECK CONSTRAINT [FK_dbo.ContentItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItem] CHECK CONSTRAINT [FK_dbo.ContentItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.ContentChannelItem_ChildContentChannelItemId] FOREIGN KEY([ChildContentChannelItemId])
REFERENCES [dbo].[ContentChannelItem] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation] CHECK CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.ContentChannelItem_ChildContentChannelItemId]
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.ContentChannelItem_ContentChannelItemId] FOREIGN KEY([ContentChannelItemId])
REFERENCES [dbo].[ContentChannelItem] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation] CHECK CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.ContentChannelItem_ContentChannelItemId]
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation] CHECK CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemAssociation] CHECK CONSTRAINT [FK_dbo.ContentChannelItemAssociation_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItemSlug]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemSlug_dbo.ContentChannelItem_ContentChannelItemId] FOREIGN KEY([ContentChannelItemId])
REFERENCES [dbo].[ContentChannelItem] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemSlug] CHECK CONSTRAINT [FK_dbo.ContentChannelItemSlug_dbo.ContentChannelItem_ContentChannelItemId]
GO
ALTER TABLE [dbo].[ContentChannelItemSlug]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemSlug_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemSlug] CHECK CONSTRAINT [FK_dbo.ContentChannelItemSlug_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelItemSlug]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentChannelItemSlug_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelItemSlug] CHECK CONSTRAINT [FK_dbo.ContentChannelItemSlug_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelType] CHECK CONSTRAINT [FK_dbo.ContentType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentChannelType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentChannelType] CHECK CONSTRAINT [FK_dbo.ContentType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentCollection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentCollection_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentCollection] CHECK CONSTRAINT [FK_dbo.ContentCollection_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentCollection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentCollection_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentCollection] CHECK CONSTRAINT [FK_dbo.ContentCollection_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentCollectionSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.ContentCollection_ContentCollectionId] FOREIGN KEY([ContentCollectionId])
REFERENCES [dbo].[ContentCollection] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContentCollectionSource] CHECK CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.ContentCollection_ContentCollectionId]
GO
ALTER TABLE [dbo].[ContentCollectionSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[ContentCollectionSource] CHECK CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[ContentCollectionSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentCollectionSource] CHECK CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentCollectionSource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentCollectionSource] CHECK CONSTRAINT [FK_dbo.ContentCollectionSource_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentTopic_dbo.ContentTopicDomain_ContentTopicDomainId] FOREIGN KEY([ContentTopicDomainId])
REFERENCES [dbo].[ContentTopicDomain] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContentTopic] CHECK CONSTRAINT [FK_dbo.ContentTopic_dbo.ContentTopicDomain_ContentTopicDomainId]
GO
ALTER TABLE [dbo].[ContentTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentTopic_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentTopic] CHECK CONSTRAINT [FK_dbo.ContentTopic_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentTopic]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentTopic_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentTopic] CHECK CONSTRAINT [FK_dbo.ContentTopic_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentTopicDomain]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentTopicDomain_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentTopicDomain] CHECK CONSTRAINT [FK_dbo.ContentTopicDomain_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ContentTopicDomain]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ContentTopicDomain_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ContentTopicDomain] CHECK CONSTRAINT [FK_dbo.ContentTopicDomain_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.DataViewFilter_DataViewFilterId] FOREIGN KEY([DataViewFilterId])
REFERENCES [dbo].[DataViewFilter] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.DataViewFilter_DataViewFilterId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.EntityType_TransformEntityTypeId] FOREIGN KEY([TransformEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.EntityType_TransformEntityTypeId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[DataView]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataView_dbo.Schedule_PersistedScheduleId] FOREIGN KEY([PersistedScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[DataView] CHECK CONSTRAINT [FK_dbo.DataView_dbo.Schedule_PersistedScheduleId]
GO
ALTER TABLE [dbo].[DataViewFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewFilter_dbo.DataViewFilter_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[DataViewFilter] ([Id])
GO
ALTER TABLE [dbo].[DataViewFilter] CHECK CONSTRAINT [FK_dbo.DataViewFilter_dbo.DataViewFilter_ParentId]
GO
ALTER TABLE [dbo].[DataViewFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewFilter_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[DataViewFilter] CHECK CONSTRAINT [FK_dbo.DataViewFilter_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[DataViewFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewFilter_dbo.DataView_RelatedDataViewId] FOREIGN KEY([RelatedDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[DataViewFilter] CHECK CONSTRAINT [FK_dbo.DataViewFilter_dbo.DataView_RelatedDataViewId]
GO
ALTER TABLE [dbo].[DataViewFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewFilter_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[DataViewFilter] CHECK CONSTRAINT [FK_dbo.DataViewFilter_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[DataViewFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewFilter_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DataViewFilter] CHECK CONSTRAINT [FK_dbo.DataViewFilter_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[DataViewFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewFilter_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DataViewFilter] CHECK CONSTRAINT [FK_dbo.DataViewFilter_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[DataViewPersistedValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DataViewPersistedValue_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DataViewPersistedValue] CHECK CONSTRAINT [FK_dbo.DataViewPersistedValue_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[DefinedType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedType_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[DefinedType] CHECK CONSTRAINT [FK_dbo.DefinedType_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[DefinedType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedType_dbo.FieldType_FieldTypeId] FOREIGN KEY([FieldTypeId])
REFERENCES [dbo].[FieldType] ([Id])
GO
ALTER TABLE [dbo].[DefinedType] CHECK CONSTRAINT [FK_dbo.DefinedType_dbo.FieldType_FieldTypeId]
GO
ALTER TABLE [dbo].[DefinedType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DefinedType] CHECK CONSTRAINT [FK_dbo.DefinedType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[DefinedType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DefinedType] CHECK CONSTRAINT [FK_dbo.DefinedType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[DefinedValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedValue_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[DefinedValue] CHECK CONSTRAINT [FK_dbo.DefinedValue_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[DefinedValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedValue_dbo.DefinedType_DefinedTypeId] FOREIGN KEY([DefinedTypeId])
REFERENCES [dbo].[DefinedType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DefinedValue] CHECK CONSTRAINT [FK_dbo.DefinedValue_dbo.DefinedType_DefinedTypeId]
GO
ALTER TABLE [dbo].[DefinedValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedValue_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DefinedValue] CHECK CONSTRAINT [FK_dbo.DefinedValue_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[DefinedValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DefinedValue_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DefinedValue] CHECK CONSTRAINT [FK_dbo.DefinedValue_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Device_dbo.DefinedValue_DeviceTypeValueId] FOREIGN KEY([DeviceTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_dbo.Device_dbo.DefinedValue_DeviceTypeValueId]
GO
ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Device_dbo.Device_PrinterDeviceId] FOREIGN KEY([PrinterDeviceId])
REFERENCES [dbo].[Device] ([Id])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_dbo.Device_dbo.Device_PrinterDeviceId]
GO
ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Device_dbo.Device_ProxyDeviceId] FOREIGN KEY([ProxyDeviceId])
REFERENCES [dbo].[Device] ([Id])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_dbo.Device_dbo.Device_ProxyDeviceId]
GO
ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Device_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_dbo.Device_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Device_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_dbo.Device_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Device_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_dbo.Device_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[DeviceLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DeviceLocation_dbo.Device_DeviceId] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Device] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DeviceLocation] CHECK CONSTRAINT [FK_dbo.DeviceLocation_dbo.Device_DeviceId]
GO
ALTER TABLE [dbo].[DeviceLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DeviceLocation_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DeviceLocation] CHECK CONSTRAINT [FK_dbo.DeviceLocation_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Document_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_dbo.Document_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Document_dbo.DocumentType_DocumentTypeId] FOREIGN KEY([DocumentTypeId])
REFERENCES [dbo].[DocumentType] ([Id])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_dbo.Document_dbo.DocumentType_DocumentTypeId]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Document_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_dbo.Document_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Document_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_dbo.Document_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[DocumentType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DocumentType_dbo.BinaryFileType_BinaryFileTypeId] FOREIGN KEY([BinaryFileTypeId])
REFERENCES [dbo].[BinaryFileType] ([Id])
GO
ALTER TABLE [dbo].[DocumentType] CHECK CONSTRAINT [FK_dbo.DocumentType_dbo.BinaryFileType_BinaryFileTypeId]
GO
ALTER TABLE [dbo].[DocumentType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DocumentType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[DocumentType] CHECK CONSTRAINT [FK_dbo.DocumentType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[DocumentType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DocumentType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DocumentType] CHECK CONSTRAINT [FK_dbo.DocumentType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[DocumentType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DocumentType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[DocumentType] CHECK CONSTRAINT [FK_dbo.DocumentType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EmailSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EmailSection_dbo.BinaryFile_ThumbnailBinaryFileId] FOREIGN KEY([ThumbnailBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[EmailSection] CHECK CONSTRAINT [FK_dbo.EmailSection_dbo.BinaryFile_ThumbnailBinaryFileId]
GO
ALTER TABLE [dbo].[EmailSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EmailSection_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[EmailSection] CHECK CONSTRAINT [FK_dbo.EmailSection_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[EmailSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EmailSection_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EmailSection] CHECK CONSTRAINT [FK_dbo.EmailSection_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EmailSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EmailSection_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EmailSection] CHECK CONSTRAINT [FK_dbo.EmailSection_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntityCampusFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntityCampusFilter] CHECK CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[EntityCampusFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntityCampusFilter] CHECK CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[EntityCampusFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntityCampusFilter] CHECK CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntityCampusFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntityCampusFilter] CHECK CONSTRAINT [FK_dbo.EntityCampusFilter_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntityIntent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityIntent_dbo.DefinedValue_IntentValueId] FOREIGN KEY([IntentValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntityIntent] CHECK CONSTRAINT [FK_dbo.EntityIntent_dbo.DefinedValue_IntentValueId]
GO
ALTER TABLE [dbo].[EntityIntent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityIntent_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntityIntent] CHECK CONSTRAINT [FK_dbo.EntityIntent_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[EntityIntent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityIntent_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntityIntent] CHECK CONSTRAINT [FK_dbo.EntityIntent_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntityIntent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityIntent_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntityIntent] CHECK CONSTRAINT [FK_dbo.EntityIntent_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntityMetadata]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityMetadata_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntityMetadata] CHECK CONSTRAINT [FK_dbo.EntityMetadata_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[EntitySearch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySearch_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntitySearch] CHECK CONSTRAINT [FK_dbo.EntitySearch_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[EntitySearch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySearch_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntitySearch] CHECK CONSTRAINT [FK_dbo.EntitySearch_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntitySearch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySearch_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntitySearch] CHECK CONSTRAINT [FK_dbo.EntitySearch_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntitySet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySet_dbo.DefinedValue_EntitySetPurposeValueId] FOREIGN KEY([EntitySetPurposeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[EntitySet] CHECK CONSTRAINT [FK_dbo.EntitySet_dbo.DefinedValue_EntitySetPurposeValueId]
GO
ALTER TABLE [dbo].[EntitySet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySet_dbo.EntitySet_ParentEntitySetId] FOREIGN KEY([ParentEntitySetId])
REFERENCES [dbo].[EntitySet] ([Id])
GO
ALTER TABLE [dbo].[EntitySet] CHECK CONSTRAINT [FK_dbo.EntitySet_dbo.EntitySet_ParentEntitySetId]
GO
ALTER TABLE [dbo].[EntitySet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySet_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[EntitySet] CHECK CONSTRAINT [FK_dbo.EntitySet_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[EntitySet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySet_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntitySet] CHECK CONSTRAINT [FK_dbo.EntitySet_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntitySet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySet_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntitySet] CHECK CONSTRAINT [FK_dbo.EntitySet_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntitySetItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySetItem_dbo.EntitySet_EntitySetId] FOREIGN KEY([EntitySetId])
REFERENCES [dbo].[EntitySet] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntitySetItem] CHECK CONSTRAINT [FK_dbo.EntitySetItem_dbo.EntitySet_EntitySetId]
GO
ALTER TABLE [dbo].[EntitySetItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySetItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntitySetItem] CHECK CONSTRAINT [FK_dbo.EntitySetItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntitySetItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntitySetItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EntitySetItem] CHECK CONSTRAINT [FK_dbo.EntitySetItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EntityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityType_dbo.FieldType_MultiValueFieldTypeId] FOREIGN KEY([MultiValueFieldTypeId])
REFERENCES [dbo].[FieldType] ([Id])
GO
ALTER TABLE [dbo].[EntityType] CHECK CONSTRAINT [FK_dbo.EntityType_dbo.FieldType_MultiValueFieldTypeId]
GO
ALTER TABLE [dbo].[EntityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EntityType_dbo.FieldType_SingleValueFieldTypeId] FOREIGN KEY([SingleValueFieldTypeId])
REFERENCES [dbo].[FieldType] ([Id])
GO
ALTER TABLE [dbo].[EntityType] CHECK CONSTRAINT [FK_dbo.EntityType_dbo.FieldType_SingleValueFieldTypeId]
GO
ALTER TABLE [dbo].[EventCalendar]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendar_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventCalendar] CHECK CONSTRAINT [FK_dbo.EventCalendar_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventCalendar]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendar_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventCalendar] CHECK CONSTRAINT [FK_dbo.EventCalendar_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventCalendarContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.ContentChannel_ContentChannelId] FOREIGN KEY([ContentChannelId])
REFERENCES [dbo].[ContentChannel] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventCalendarContentChannel] CHECK CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.ContentChannel_ContentChannelId]
GO
ALTER TABLE [dbo].[EventCalendarContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.EventCalendar_EventCalendarId] FOREIGN KEY([EventCalendarId])
REFERENCES [dbo].[EventCalendar] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventCalendarContentChannel] CHECK CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.EventCalendar_EventCalendarId]
GO
ALTER TABLE [dbo].[EventCalendarContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventCalendarContentChannel] CHECK CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventCalendarContentChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventCalendarContentChannel] CHECK CONSTRAINT [FK_dbo.EventCalendarContentChannel_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventCalendarItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarItem_dbo.EventCalendar_EventCalendarId] FOREIGN KEY([EventCalendarId])
REFERENCES [dbo].[EventCalendar] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventCalendarItem] CHECK CONSTRAINT [FK_dbo.EventCalendarItem_dbo.EventCalendar_EventCalendarId]
GO
ALTER TABLE [dbo].[EventCalendarItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarItem_dbo.EventItem_EventItemId] FOREIGN KEY([EventItemId])
REFERENCES [dbo].[EventItem] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventCalendarItem] CHECK CONSTRAINT [FK_dbo.EventCalendarItem_dbo.EventItem_EventItemId]
GO
ALTER TABLE [dbo].[EventCalendarItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventCalendarItem] CHECK CONSTRAINT [FK_dbo.EventCalendarItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventCalendarItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventCalendarItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventCalendarItem] CHECK CONSTRAINT [FK_dbo.EventCalendarItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItem_dbo.BinaryFile_PhotoId] FOREIGN KEY([PhotoId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[EventItem] CHECK CONSTRAINT [FK_dbo.EventItem_dbo.BinaryFile_PhotoId]
GO
ALTER TABLE [dbo].[EventItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItem_dbo.PersonAlias_ApprovedByPersonAliasId] FOREIGN KEY([ApprovedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItem] CHECK CONSTRAINT [FK_dbo.EventItem_dbo.PersonAlias_ApprovedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItem] CHECK CONSTRAINT [FK_dbo.EventItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItem] CHECK CONSTRAINT [FK_dbo.EventItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemAudience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemAudience_dbo.DefinedValue_DefinedValueId] FOREIGN KEY([DefinedValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[EventItemAudience] CHECK CONSTRAINT [FK_dbo.EventItemAudience_dbo.DefinedValue_DefinedValueId]
GO
ALTER TABLE [dbo].[EventItemAudience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemAudience_dbo.EventItem_EventItemId] FOREIGN KEY([EventItemId])
REFERENCES [dbo].[EventItem] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemAudience] CHECK CONSTRAINT [FK_dbo.EventItemAudience_dbo.EventItem_EventItemId]
GO
ALTER TABLE [dbo].[EventItemAudience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemAudience_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemAudience] CHECK CONSTRAINT [FK_dbo.EventItemAudience_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemAudience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemAudience_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemAudience] CHECK CONSTRAINT [FK_dbo.EventItemAudience_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampus_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrence] CHECK CONSTRAINT [FK_dbo.EventItemCampus_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[EventItemOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampus_dbo.EventItem_EventItemId] FOREIGN KEY([EventItemId])
REFERENCES [dbo].[EventItem] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrence] CHECK CONSTRAINT [FK_dbo.EventItemCampus_dbo.EventItem_EventItemId]
GO
ALTER TABLE [dbo].[EventItemOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampus_dbo.PersonAlias_ContactPersonAliasId] FOREIGN KEY([ContactPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrence] CHECK CONSTRAINT [FK_dbo.EventItemCampus_dbo.PersonAlias_ContactPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrence] CHECK CONSTRAINT [FK_dbo.EventItemCampus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrence] CHECK CONSTRAINT [FK_dbo.EventItemCampus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemOccurrence_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrence] CHECK CONSTRAINT [FK_dbo.EventItemOccurrence_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.ContentChannelItem_ContentChannelItemId] FOREIGN KEY([ContentChannelItemId])
REFERENCES [dbo].[ContentChannelItem] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem] CHECK CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.ContentChannelItem_ContentChannelItemId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.EventItemOccurrence_EventItemOccurrenceId] FOREIGN KEY([EventItemOccurrenceId])
REFERENCES [dbo].[EventItemOccurrence] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem] CHECK CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.EventItemOccurrence_EventItemOccurrenceId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem] CHECK CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrenceChannelItem] CHECK CONSTRAINT [FK_dbo.EventItemOccurrenceChannelItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.EventItemCampus_EventItemCampusId] FOREIGN KEY([EventItemOccurrenceId])
REFERENCES [dbo].[EventItemOccurrence] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap] CHECK CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.EventItemCampus_EventItemCampusId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap] CHECK CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap] CHECK CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap] CHECK CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.RegistrationInstance_RegistrationInstanceId] FOREIGN KEY([RegistrationInstanceId])
REFERENCES [dbo].[RegistrationInstance] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap] CHECK CONSTRAINT [FK_dbo.EventItemCampusGroupMap_dbo.RegistrationInstance_RegistrationInstanceId]
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EventItemOccurrenceGroupMap_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventItemOccurrenceGroupMap] CHECK CONSTRAINT [FK_dbo.EventItemOccurrenceGroupMap_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[ExceptionLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ExceptionLog_dbo.Page_PageId] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExceptionLog] CHECK CONSTRAINT [FK_dbo.ExceptionLog_dbo.Page_PageId]
GO
ALTER TABLE [dbo].[ExceptionLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ExceptionLog_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ExceptionLog] CHECK CONSTRAINT [FK_dbo.ExceptionLog_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ExceptionLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ExceptionLog_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ExceptionLog] CHECK CONSTRAINT [FK_dbo.ExceptionLog_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ExceptionLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ExceptionLog_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExceptionLog] CHECK CONSTRAINT [FK_dbo.ExceptionLog_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[FieldType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FieldType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FieldType] CHECK CONSTRAINT [FK_dbo.FieldType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FieldType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FieldType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FieldType] CHECK CONSTRAINT [FK_dbo.FieldType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialAccount_dbo.BinaryFile_ImageBinaryFileId] FOREIGN KEY([ImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[FinancialAccount] CHECK CONSTRAINT [FK_dbo.FinancialAccount_dbo.BinaryFile_ImageBinaryFileId]
GO
ALTER TABLE [dbo].[FinancialAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialAccount_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[FinancialAccount] CHECK CONSTRAINT [FK_dbo.FinancialAccount_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[FinancialAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialAccount_dbo.DefinedValue_AccountTypeValueId] FOREIGN KEY([AccountTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialAccount] CHECK CONSTRAINT [FK_dbo.FinancialAccount_dbo.DefinedValue_AccountTypeValueId]
GO
ALTER TABLE [dbo].[FinancialAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialAccount_dbo.FinancialAccount_ParentAccountId] FOREIGN KEY([ParentAccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[FinancialAccount] CHECK CONSTRAINT [FK_dbo.FinancialAccount_dbo.FinancialAccount_ParentAccountId]
GO
ALTER TABLE [dbo].[FinancialAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialAccount_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialAccount] CHECK CONSTRAINT [FK_dbo.FinancialAccount_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialAccount_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialAccount] CHECK CONSTRAINT [FK_dbo.FinancialAccount_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialBatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialBatch_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[FinancialBatch] CHECK CONSTRAINT [FK_dbo.FinancialBatch_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[FinancialBatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialBatch_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialBatch] CHECK CONSTRAINT [FK_dbo.FinancialBatch_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialBatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialBatch_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialBatch] CHECK CONSTRAINT [FK_dbo.FinancialBatch_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialGateway]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialGateway_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[FinancialGateway] CHECK CONSTRAINT [FK_dbo.FinancialGateway_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[FinancialGateway]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialGateway_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialGateway] CHECK CONSTRAINT [FK_dbo.FinancialGateway_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialGateway]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialGateway_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialGateway] CHECK CONSTRAINT [FK_dbo.FinancialGateway_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.DefinedValue_CreditCardTypeValueId] FOREIGN KEY([CreditCardTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialPaymentDetail] CHECK CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.DefinedValue_CreditCardTypeValueId]
GO
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.DefinedValue_CurrencyTypeValueId] FOREIGN KEY([CurrencyTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialPaymentDetail] CHECK CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.DefinedValue_CurrencyTypeValueId]
GO
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.FinancialPersonSavedAccount] FOREIGN KEY([FinancialPersonSavedAccountId])
REFERENCES [dbo].[FinancialPersonSavedAccount] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[FinancialPaymentDetail] CHECK CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.FinancialPersonSavedAccount]
GO
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.Location_BillingLocationId] FOREIGN KEY([BillingLocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[FinancialPaymentDetail] CHECK CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.Location_BillingLocationId]
GO
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPaymentDetail] CHECK CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPaymentDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPaymentDetail] CHECK CONSTRAINT [FK_dbo.FinancialPaymentDetail_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPersonBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonBankAccount_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonBankAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonBankAccount_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPersonBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonBankAccount_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonBankAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonBankAccount_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPersonBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonBankAccount_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialPersonBankAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonBankAccount_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.FinancialGateway_FinancialGatewayId] FOREIGN KEY([FinancialGatewayId])
REFERENCES [dbo].[FinancialGateway] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.FinancialGateway_FinancialGatewayId]
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.FinancialPaymentDetail_FinancialPaymentDetailId] FOREIGN KEY([FinancialPaymentDetailId])
REFERENCES [dbo].[FinancialPaymentDetail] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.FinancialPaymentDetail_FinancialPaymentDetailId]
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPersonSavedAccount] CHECK CONSTRAINT [FK_dbo.FinancialPersonSavedAccount_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPledge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPledge_dbo.DefinedValue_PledgeFrequencyValueId] FOREIGN KEY([PledgeFrequencyValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialPledge] CHECK CONSTRAINT [FK_dbo.FinancialPledge_dbo.DefinedValue_PledgeFrequencyValueId]
GO
ALTER TABLE [dbo].[FinancialPledge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPledge_dbo.FinancialAccount_AccountId] FOREIGN KEY([AccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[FinancialPledge] CHECK CONSTRAINT [FK_dbo.FinancialPledge_dbo.FinancialAccount_AccountId]
GO
ALTER TABLE [dbo].[FinancialPledge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPledge_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[FinancialPledge] CHECK CONSTRAINT [FK_dbo.FinancialPledge_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[FinancialPledge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPledge_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPledge] CHECK CONSTRAINT [FK_dbo.FinancialPledge_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPledge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPledge_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPledge] CHECK CONSTRAINT [FK_dbo.FinancialPledge_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialPledge]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialPledge_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialPledge] CHECK CONSTRAINT [FK_dbo.FinancialPledge_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_ForeignCurrencyCodeValueId] FOREIGN KEY([ForeignCurrencyCodeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_ForeignCurrencyCodeValueId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_SourceTypeValueId] FOREIGN KEY([SourceTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_SourceTypeValueId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_TransactionFrequencyValueId] FOREIGN KEY([TransactionFrequencyValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_TransactionFrequencyValueId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_TransactionTypeValueId] FOREIGN KEY([TransactionTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.DefinedValue_TransactionTypeValueId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.FinancialGateway_FinancialGatewayId] FOREIGN KEY([FinancialGatewayId])
REFERENCES [dbo].[FinancialGateway] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.FinancialGateway_FinancialGatewayId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.FinancialPaymentDetail_FinancialPaymentDetailId] FOREIGN KEY([FinancialPaymentDetailId])
REFERENCES [dbo].[FinancialPaymentDetail] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.FinancialPaymentDetail_FinancialPaymentDetailId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.PersonAlias_AuthorizedPersonAliasId] FOREIGN KEY([AuthorizedPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.PersonAlias_AuthorizedPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransaction] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransaction_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.FinancialAccount_AccountId] FOREIGN KEY([AccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.FinancialAccount_AccountId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.FinancialScheduledTransaction_ScheduledTransactionId] FOREIGN KEY([ScheduledTransactionId])
REFERENCES [dbo].[FinancialScheduledTransaction] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.FinancialScheduledTransaction_ScheduledTransactionId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialScheduledTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialScheduledTransactionDetail_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialStatementTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialStatementTemplate_dbo.BinaryFile_LogoBinaryFileId] FOREIGN KEY([LogoBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[FinancialStatementTemplate] CHECK CONSTRAINT [FK_dbo.FinancialStatementTemplate_dbo.BinaryFile_LogoBinaryFileId]
GO
ALTER TABLE [dbo].[FinancialStatementTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialStatementTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialStatementTemplate] CHECK CONSTRAINT [FK_dbo.FinancialStatementTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialStatementTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialStatementTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialStatementTemplate] CHECK CONSTRAINT [FK_dbo.FinancialStatementTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.DefinedValue_NonCashAssetTypeValueId] FOREIGN KEY([NonCashAssetTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.DefinedValue_NonCashAssetTypeValueId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.DefinedValue_SourceTypeValueId] FOREIGN KEY([SourceTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.DefinedValue_SourceTypeValueId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.DefinedValue_TransactionTypeValueId] FOREIGN KEY([TransactionTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.DefinedValue_TransactionTypeValueId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialBatch_BatchId] FOREIGN KEY([BatchId])
REFERENCES [dbo].[FinancialBatch] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialBatch_BatchId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialGateway_FinancialGatewayId] FOREIGN KEY([FinancialGatewayId])
REFERENCES [dbo].[FinancialGateway] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialGateway_FinancialGatewayId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialPaymentDetail_FinancialPaymentDetailId] FOREIGN KEY([FinancialPaymentDetailId])
REFERENCES [dbo].[FinancialPaymentDetail] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialPaymentDetail_FinancialPaymentDetailId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialScheduledTransaction_ScheduledTransactionId] FOREIGN KEY([ScheduledTransactionId])
REFERENCES [dbo].[FinancialScheduledTransaction] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.FinancialScheduledTransaction_ScheduledTransactionId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_AuthorizedPersonAliasId] FOREIGN KEY([AuthorizedPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_AuthorizedPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_ProcessedByPersonAliasId] FOREIGN KEY([ProcessedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransaction] CHECK CONSTRAINT [FK_dbo.FinancialTransaction_dbo.PersonAlias_ProcessedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlert]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.FinancialTransactionAlertType_AlertTypeId] FOREIGN KEY([AlertTypeId])
REFERENCES [dbo].[FinancialTransactionAlertType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialTransactionAlert] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.FinancialTransactionAlertType_AlertTypeId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlert]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.FinancialTransaction_TransactionId] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[FinancialTransaction] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialTransactionAlert] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.FinancialTransaction_TransactionId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlert]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlert] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlert]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlert] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlert]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlert] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlert_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.ConnectionOpportunity_ConnectionOpportunityId] FOREIGN KEY([ConnectionOpportunityId])
REFERENCES [dbo].[ConnectionOpportunity] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.ConnectionOpportunity_ConnectionOpportunityId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.FinancialAccount_FinancialAccountId] FOREIGN KEY([FinancialAccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.FinancialAccount_FinancialAccountId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.Group_AlertSummaryNotificationGroupId] FOREIGN KEY([AlertSummaryNotificationGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.Group_AlertSummaryNotificationGroupId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.SystemCommunication_AccountParticipantSystemCommunicationId] FOREIGN KEY([AccountParticipantSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.SystemCommunication_AccountParticipantSystemCommunicationId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.SystemCommunication_SystemCommunicationId] FOREIGN KEY([SystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.SystemCommunication_SystemCommunicationId]
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionAlertType] CHECK CONSTRAINT [FK_dbo.FinancialTransactionAlertType_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[FinancialTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[FinancialTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.FinancialAccount_AccountId] FOREIGN KEY([AccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.FinancialAccount_AccountId]
GO
ALTER TABLE [dbo].[FinancialTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.FinancialTransaction_TransactionId] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[FinancialTransaction] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.FinancialTransaction_TransactionId]
GO
ALTER TABLE [dbo].[FinancialTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionDetail] CHECK CONSTRAINT [FK_dbo.FinancialTransactionDetail_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionImage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionImage] CHECK CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[FinancialTransactionImage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.FinancialTransaction_TransactionId] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[FinancialTransaction] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialTransactionImage] CHECK CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.FinancialTransaction_TransactionId]
GO
ALTER TABLE [dbo].[FinancialTransactionImage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionImage] CHECK CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionImage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionImage] CHECK CONSTRAINT [FK_dbo.FinancialTransactionImage_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionRefund]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.DefinedValue_RefundReasonValueId] FOREIGN KEY([RefundReasonValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionRefund] CHECK CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.DefinedValue_RefundReasonValueId]
GO
ALTER TABLE [dbo].[FinancialTransactionRefund]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.FinancialTransaction_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[FinancialTransaction] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FinancialTransactionRefund] CHECK CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.FinancialTransaction_Id]
GO
ALTER TABLE [dbo].[FinancialTransactionRefund]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.FinancialTransaction_OriginalTransactionId] FOREIGN KEY([OriginalTransactionId])
REFERENCES [dbo].[FinancialTransaction] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionRefund] CHECK CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.FinancialTransaction_OriginalTransactionId]
GO
ALTER TABLE [dbo].[FinancialTransactionRefund]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionRefund] CHECK CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FinancialTransactionRefund]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FinancialTransactionRefund] CHECK CONSTRAINT [FK_dbo.FinancialTransactionRefund_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Following]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Following_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Following] CHECK CONSTRAINT [FK_dbo.Following_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Following]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Following_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Following] CHECK CONSTRAINT [FK_dbo.Following_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Following]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Following_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Following] CHECK CONSTRAINT [FK_dbo.Following_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Following]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Following_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Following] CHECK CONSTRAINT [FK_dbo.Following_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventNotification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventNotification_dbo.FollowingEventType_FollowingEventTypeId] FOREIGN KEY([FollowingEventTypeId])
REFERENCES [dbo].[FollowingEventType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FollowingEventNotification] CHECK CONSTRAINT [FK_dbo.FollowingEventNotification_dbo.FollowingEventType_FollowingEventTypeId]
GO
ALTER TABLE [dbo].[FollowingEventNotification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventNotification_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventNotification] CHECK CONSTRAINT [FK_dbo.FollowingEventNotification_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventNotification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventNotification_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventNotification] CHECK CONSTRAINT [FK_dbo.FollowingEventNotification_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventSubscription]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.FollowingEventType_EventTypeId] FOREIGN KEY([EventTypeId])
REFERENCES [dbo].[FollowingEventType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FollowingEventSubscription] CHECK CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.FollowingEventType_EventTypeId]
GO
ALTER TABLE [dbo].[FollowingEventSubscription]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventSubscription] CHECK CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventSubscription]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventSubscription] CHECK CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventSubscription]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FollowingEventSubscription] CHECK CONSTRAINT [FK_dbo.FollowingEventSubscription_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventType] CHECK CONSTRAINT [FK_dbo.FollowingEventType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[FollowingEventType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventType_dbo.EntityType_FollowedEntityTypeId] FOREIGN KEY([FollowedEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventType] CHECK CONSTRAINT [FK_dbo.FollowingEventType_dbo.EntityType_FollowedEntityTypeId]
GO
ALTER TABLE [dbo].[FollowingEventType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventType] CHECK CONSTRAINT [FK_dbo.FollowingEventType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingEventType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingEventType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingEventType] CHECK CONSTRAINT [FK_dbo.FollowingEventType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingSuggested]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggested_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FollowingSuggested] CHECK CONSTRAINT [FK_dbo.FollowingSuggested_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[FollowingSuggested]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggested_dbo.FollowingSuggestionType_SuggestionTypeId] FOREIGN KEY([SuggestionTypeId])
REFERENCES [dbo].[FollowingSuggestionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FollowingSuggested] CHECK CONSTRAINT [FK_dbo.FollowingSuggested_dbo.FollowingSuggestionType_SuggestionTypeId]
GO
ALTER TABLE [dbo].[FollowingSuggested]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggested_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingSuggested] CHECK CONSTRAINT [FK_dbo.FollowingSuggested_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingSuggested]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggested_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingSuggested] CHECK CONSTRAINT [FK_dbo.FollowingSuggested_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingSuggested]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggested_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FollowingSuggested] CHECK CONSTRAINT [FK_dbo.FollowingSuggested_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[FollowingSuggestionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggestionType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[FollowingSuggestionType] CHECK CONSTRAINT [FK_dbo.FollowingSuggestionType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[FollowingSuggestionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggestionType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingSuggestionType] CHECK CONSTRAINT [FK_dbo.FollowingSuggestionType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[FollowingSuggestionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FollowingSuggestionType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[FollowingSuggestionType] CHECK CONSTRAINT [FK_dbo.FollowingSuggestionType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.BinaryFile_ChatChannelAvatarBinaryFileId] FOREIGN KEY([ChatChannelAvatarBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.BinaryFile_ChatChannelAvatarBinaryFileId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.DefinedValue_GroupMemberRecordSourceValueId] FOREIGN KEY([GroupMemberRecordSourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.DefinedValue_GroupMemberRecordSourceValueId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.DefinedValue_InactiveReasonValueId] FOREIGN KEY([InactiveReasonValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.DefinedValue_InactiveReasonValueId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.DefinedValue_StatusValueId] FOREIGN KEY([StatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.DefinedValue_StatusValueId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.Group_ParentGroupId] FOREIGN KEY([ParentGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.Group_ParentGroupId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_ArchivedByPersonAliasId] FOREIGN KEY([ArchivedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_ArchivedByPersonAliasId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_GroupAdministratorPersonAliasId] FOREIGN KEY([GroupAdministratorPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_GroupAdministratorPersonAliasId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_ScheduleCoordinatorPersonAliasId] FOREIGN KEY([ScheduleCoordinatorPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.PersonAlias_ScheduleCoordinatorPersonAliasId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[Group]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Group_dbo.SignatureDocumentType_RequiredSignatureDocumentTypeId] FOREIGN KEY([RequiredSignatureDocumentTemplateId])
REFERENCES [dbo].[SignatureDocumentTemplate] ([Id])
GO
ALTER TABLE [dbo].[Group] CHECK CONSTRAINT [FK_dbo.Group_dbo.SignatureDocumentType_RequiredSignatureDocumentTypeId]
GO
ALTER TABLE [dbo].[GroupDemographicType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicType_dbo.EntityType_ComponentEntityTypeId] FOREIGN KEY([ComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[GroupDemographicType] CHECK CONSTRAINT [FK_dbo.GroupDemographicType_dbo.EntityType_ComponentEntityTypeId]
GO
ALTER TABLE [dbo].[GroupDemographicType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicType_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupDemographicType] CHECK CONSTRAINT [FK_dbo.GroupDemographicType_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupDemographicType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupDemographicType] CHECK CONSTRAINT [FK_dbo.GroupDemographicType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupDemographicType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupDemographicType] CHECK CONSTRAINT [FK_dbo.GroupDemographicType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupDemographicValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.EntityType_RelatedEntityTypeId] FOREIGN KEY([RelatedEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[GroupDemographicValue] CHECK CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.EntityType_RelatedEntityTypeId]
GO
ALTER TABLE [dbo].[GroupDemographicValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.GroupDemographicType_GroupDemographicTypeId] FOREIGN KEY([GroupDemographicTypeId])
REFERENCES [dbo].[GroupDemographicType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupDemographicValue] CHECK CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.GroupDemographicType_GroupDemographicTypeId]
GO
ALTER TABLE [dbo].[GroupDemographicValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupDemographicValue] CHECK CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupDemographicValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupDemographicValue] CHECK CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupDemographicValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupDemographicValue] CHECK CONSTRAINT [FK_dbo.GroupDemographicValue_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.Group_ParentGroupId] FOREIGN KEY([ParentGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.Group_ParentGroupId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.PersonAlias_ArchivedByPersonAliasId] FOREIGN KEY([ArchivedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.PersonAlias_ArchivedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupHistorical_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[GroupHistorical] CHECK CONSTRAINT [FK_dbo.GroupHistorical_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[GroupLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocation_dbo.DefinedValue_GroupLocationTypeValueId] FOREIGN KEY([GroupLocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[GroupLocation] CHECK CONSTRAINT [FK_dbo.GroupLocation_dbo.DefinedValue_GroupLocationTypeValueId]
GO
ALTER TABLE [dbo].[GroupLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocation_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocation] CHECK CONSTRAINT [FK_dbo.GroupLocation_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocation_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocation] CHECK CONSTRAINT [FK_dbo.GroupLocation_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[GroupLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupLocation] CHECK CONSTRAINT [FK_dbo.GroupLocation_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocation_dbo.PersonAlias_GroupMemberPersonAliasId] FOREIGN KEY([GroupMemberPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocation] CHECK CONSTRAINT [FK_dbo.GroupLocation_dbo.PersonAlias_GroupMemberPersonAliasId]
GO
ALTER TABLE [dbo].[GroupLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupLocation] CHECK CONSTRAINT [FK_dbo.GroupLocation_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupLocationHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.GroupLocation_GroupLocationId] FOREIGN KEY([GroupLocationId])
REFERENCES [dbo].[GroupLocation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocationHistorical] CHECK CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.GroupLocation_GroupLocationId]
GO
ALTER TABLE [dbo].[GroupLocationHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[GroupLocationHistorical] CHECK CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupLocationHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[GroupLocationHistorical] CHECK CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[GroupLocationHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupLocationHistorical] CHECK CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupLocationHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupLocationHistorical] CHECK CONSTRAINT [FK_dbo.GroupLocationHistorical_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupLocationSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationSchedule_dbo.GroupLocation_GroupLocationId] FOREIGN KEY([GroupLocationId])
REFERENCES [dbo].[GroupLocation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocationSchedule] CHECK CONSTRAINT [FK_dbo.GroupLocationSchedule_dbo.GroupLocation_GroupLocationId]
GO
ALTER TABLE [dbo].[GroupLocationSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationSchedule_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocationSchedule] CHECK CONSTRAINT [FK_dbo.GroupLocationSchedule_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[GroupLocationScheduleConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationScheduleConfig_dbo.GroupLocation_GroupLocationId] FOREIGN KEY([GroupLocationId])
REFERENCES [dbo].[GroupLocation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocationScheduleConfig] CHECK CONSTRAINT [FK_dbo.GroupLocationScheduleConfig_dbo.GroupLocation_GroupLocationId]
GO
ALTER TABLE [dbo].[GroupLocationScheduleConfig]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupLocationScheduleConfig_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupLocationScheduleConfig] CHECK CONSTRAINT [FK_dbo.GroupLocationScheduleConfig_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.GroupMemberScheduleTemplate_ScheduleTemplateId] FOREIGN KEY([ScheduleTemplateId])
REFERENCES [dbo].[GroupMemberScheduleTemplate] ([Id])
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.GroupMemberScheduleTemplate_ScheduleTemplateId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.GroupTypeRole_GroupRoleId] FOREIGN KEY([GroupRoleId])
REFERENCES [dbo].[GroupTypeRole] ([Id])
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.GroupTypeRole_GroupRoleId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.PersonAlias_ArchivedByPersonAliasId] FOREIGN KEY([ArchivedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.PersonAlias_ArchivedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMember]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMember_dbo.Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMember] CHECK CONSTRAINT [FK_dbo.GroupMember_dbo.Person_PersonId]
GO
ALTER TABLE [dbo].[GroupMemberAssignment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.GroupMember_GroupMemberId] FOREIGN KEY([GroupMemberId])
REFERENCES [dbo].[GroupMember] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberAssignment] CHECK CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.GroupMember_GroupMemberId]
GO
ALTER TABLE [dbo].[GroupMemberAssignment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMemberAssignment] CHECK CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupMemberAssignment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberAssignment] CHECK CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[GroupMemberAssignment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberAssignment] CHECK CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberAssignment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberAssignment] CHECK CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberAssignment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberAssignment] CHECK CONSTRAINT [FK_dbo.GroupMemberAssignment_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[GroupMemberHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.GroupMember_GroupMemberId] FOREIGN KEY([GroupMemberId])
REFERENCES [dbo].[GroupMember] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberHistorical] CHECK CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.GroupMember_GroupMemberId]
GO
ALTER TABLE [dbo].[GroupMemberHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.GroupTypeRole_GroupRoleId] FOREIGN KEY([GroupRoleId])
REFERENCES [dbo].[GroupTypeRole] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberHistorical] CHECK CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.GroupTypeRole_GroupRoleId]
GO
ALTER TABLE [dbo].[GroupMemberHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberHistorical] CHECK CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupMemberHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.PersonAlias_ArchivedByPersonAliasId] FOREIGN KEY([ArchivedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberHistorical] CHECK CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.PersonAlias_ArchivedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberHistorical] CHECK CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberHistorical]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberHistorical] CHECK CONSTRAINT [FK_dbo.GroupMemberHistorical_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.GroupMember_GroupMemberId] FOREIGN KEY([GroupMemberId])
REFERENCES [dbo].[GroupMember] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.GroupMember_GroupMemberId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.GroupRequirement_GroupRequirementId] FOREIGN KEY([GroupRequirementId])
REFERENCES [dbo].[GroupRequirement] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.GroupRequirement_GroupRequirementId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_ManuallyCompletedByPersonAliasId] FOREIGN KEY([ManuallyCompletedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_ManuallyCompletedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_OverriddenByPersonAliasId] FOREIGN KEY([OverriddenByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.PersonAlias_OverriddenByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.Workflow_DoesNotMeetWorkflowId] FOREIGN KEY([DoesNotMeetWorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.Workflow_DoesNotMeetWorkflowId]
GO
ALTER TABLE [dbo].[GroupMemberRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.Workflow_WarningWorkflowId] FOREIGN KEY([WarningWorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberRequirement] CHECK CONSTRAINT [FK_dbo.GroupMemberRequirement_dbo.Workflow_WarningWorkflowId]
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate] CHECK CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate] CHECK CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate] CHECK CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[GroupMemberScheduleTemplate] CHECK CONSTRAINT [FK_dbo.GroupMemberScheduleTemplate_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[GroupMemberWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberWorkflowTrigger_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMemberWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.GroupMemberWorkflowTrigger_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupMemberWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberWorkflowTrigger_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMemberWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.GroupMemberWorkflowTrigger_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupMemberWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupMemberWorkflowTrigger_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupMemberWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.GroupMemberWorkflowTrigger_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.Attribute_DueDateAttributeId] FOREIGN KEY([DueDateAttributeId])
REFERENCES [dbo].[Attribute] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.Attribute_DueDateAttributeId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.DataView_AppliesToDataViewId] FOREIGN KEY([AppliesToDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.DataView_AppliesToDataViewId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.GroupRequirementType_GroupRequirementTypeId] FOREIGN KEY([GroupRequirementTypeId])
REFERENCES [dbo].[GroupRequirementType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.GroupRequirementType_GroupRequirementTypeId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.GroupTypeRole_GroupRoleId] FOREIGN KEY([GroupRoleId])
REFERENCES [dbo].[GroupTypeRole] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.GroupTypeRole_GroupRoleId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirement_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirement] CHECK CONSTRAINT [FK_dbo.GroupRequirement_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.DataView_WarningDataViewId] FOREIGN KEY([WarningDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.DataView_WarningDataViewId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.WorkflowType_DoesNotMeetWorkflowTypeId] FOREIGN KEY([DoesNotMeetWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.WorkflowType_DoesNotMeetWorkflowTypeId]
GO
ALTER TABLE [dbo].[GroupRequirementType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupRequirementType_dbo.WorkflowType_WarningWorkflowTypeId] FOREIGN KEY([WarningWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[GroupRequirementType] CHECK CONSTRAINT [FK_dbo.GroupRequirementType_dbo.WorkflowType_WarningWorkflowTypeId]
GO
ALTER TABLE [dbo].[GroupScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupScheduleExclusion_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupScheduleExclusion] CHECK CONSTRAINT [FK_dbo.GroupScheduleExclusion_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupScheduleExclusion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupScheduleExclusion] CHECK CONSTRAINT [FK_dbo.GroupScheduleExclusion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupScheduleExclusion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupScheduleExclusion] CHECK CONSTRAINT [FK_dbo.GroupScheduleExclusion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.DataView_SyncDataViewId] FOREIGN KEY([SyncDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.DataView_SyncDataViewId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.GroupTypeRole_GroupTypeRoleId] FOREIGN KEY([GroupTypeRoleId])
REFERENCES [dbo].[GroupTypeRole] ([Id])
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.GroupTypeRole_GroupTypeRoleId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.SystemCommunication_ExitSystemCommunicationId] FOREIGN KEY([ExitSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.SystemCommunication_ExitSystemCommunicationId]
GO
ALTER TABLE [dbo].[GroupSync]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupSync_dbo.SystemCommunication_WelcomeSystemCommunicationId] FOREIGN KEY([WelcomeSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[GroupSync] CHECK CONSTRAINT [FK_dbo.GroupSync_dbo.SystemCommunication_WelcomeSystemCommunicationId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.DefinedType_GroupStatusDefinedTypeId] FOREIGN KEY([GroupStatusDefinedTypeId])
REFERENCES [dbo].[DefinedType] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.DefinedType_GroupStatusDefinedTypeId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.DefinedValue_GroupMemberRecordSourceValueId] FOREIGN KEY([GroupMemberRecordSourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.DefinedValue_GroupMemberRecordSourceValueId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.DefinedValue_GroupTypePurposeValueId] FOREIGN KEY([GroupTypePurposeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.DefinedValue_GroupTypePurposeValueId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.GroupTypeRole_DefaultGroupRoleId] FOREIGN KEY([DefaultGroupRoleId])
REFERENCES [dbo].[GroupTypeRole] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.GroupTypeRole_DefaultGroupRoleId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.GroupType_InheritedGroupTypeId] FOREIGN KEY([InheritedGroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.GroupType_InheritedGroupTypeId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_AttendanceReminderSystemCommunicationId] FOREIGN KEY([AttendanceReminderSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_AttendanceReminderSystemCommunicationId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_RSVPReminderSystemCommunicationId] FOREIGN KEY([RSVPReminderSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_RSVPReminderSystemCommunicationId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_ScheduleConfirmationSystemCommunicationId] FOREIGN KEY([ScheduleConfirmationSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_ScheduleConfirmationSystemCommunicationId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_ScheduleReminderSystemCommunicationId] FOREIGN KEY([ScheduleReminderSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.SystemCommunication_ScheduleReminderSystemCommunicationId]
GO
ALTER TABLE [dbo].[GroupType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupType_dbo.WorkflowType_ScheduleCancellationWorkflowTypeId] FOREIGN KEY([ScheduleCancellationWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[GroupType] CHECK CONSTRAINT [FK_dbo.GroupType_dbo.WorkflowType_ScheduleCancellationWorkflowTypeId]
GO
ALTER TABLE [dbo].[GroupTypeAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeAssociation_dbo.GroupType_ChildGroupTypeId] FOREIGN KEY([ChildGroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupTypeAssociation] CHECK CONSTRAINT [FK_dbo.GroupTypeAssociation_dbo.GroupType_ChildGroupTypeId]
GO
ALTER TABLE [dbo].[GroupTypeAssociation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeAssociation_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[GroupTypeAssociation] CHECK CONSTRAINT [FK_dbo.GroupTypeAssociation_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupTypeLocationType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeLocationType_dbo.DefinedValue_LocationTypeValueId] FOREIGN KEY([LocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupTypeLocationType] CHECK CONSTRAINT [FK_dbo.GroupTypeLocationType_dbo.DefinedValue_LocationTypeValueId]
GO
ALTER TABLE [dbo].[GroupTypeLocationType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeLocationType_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupTypeLocationType] CHECK CONSTRAINT [FK_dbo.GroupTypeLocationType_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupTypeRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeRole_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupTypeRole] CHECK CONSTRAINT [FK_dbo.GroupTypeRole_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[GroupTypeRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeRole_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupTypeRole] CHECK CONSTRAINT [FK_dbo.GroupTypeRole_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[GroupTypeRole]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupTypeRole_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[GroupTypeRole] CHECK CONSTRAINT [FK_dbo.GroupTypeRole_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[History]  WITH CHECK ADD  CONSTRAINT [FK_dbo.History_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[History] CHECK CONSTRAINT [FK_dbo.History_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[History]  WITH CHECK ADD  CONSTRAINT [FK_dbo.History_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[History] CHECK CONSTRAINT [FK_dbo.History_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[History]  WITH CHECK ADD  CONSTRAINT [FK_dbo.History_dbo.EntityType_RelatedEntityTypeId] FOREIGN KEY([RelatedEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[History] CHECK CONSTRAINT [FK_dbo.History_dbo.EntityType_RelatedEntityTypeId]
GO
ALTER TABLE [dbo].[History]  WITH CHECK ADD  CONSTRAINT [FK_dbo.History_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[History] CHECK CONSTRAINT [FK_dbo.History_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[History]  WITH CHECK ADD  CONSTRAINT [FK_dbo.History_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[History] CHECK CONSTRAINT [FK_dbo.History_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[HistoryLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HistoryLogin_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[HistoryLogin] CHECK CONSTRAINT [FK_dbo.HistoryLogin_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[HistoryLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HistoryLogin_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[HistoryLogin] CHECK CONSTRAINT [FK_dbo.HistoryLogin_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[HistoryLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HistoryLogin_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[HistoryLogin] CHECK CONSTRAINT [FK_dbo.HistoryLogin_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[HistoryLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HistoryLogin_dboSite_SourceSiteId] FOREIGN KEY([SourceSiteId])
REFERENCES [dbo].[Site] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[HistoryLogin] CHECK CONSTRAINT [FK_dbo.HistoryLogin_dboSite_SourceSiteId]
GO
ALTER TABLE [dbo].[HistoryLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HistoryLogin_dboUserLogin_UserLoginId] FOREIGN KEY([UserLoginId])
REFERENCES [dbo].[UserLogin] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[HistoryLogin] CHECK CONSTRAINT [FK_dbo.HistoryLogin_dboUserLogin_UserLoginId]
GO
ALTER TABLE [dbo].[HtmlContent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HtmlContent_dbo.Block_BlockId] FOREIGN KEY([BlockId])
REFERENCES [dbo].[Block] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[HtmlContent] CHECK CONSTRAINT [FK_dbo.HtmlContent_dbo.Block_BlockId]
GO
ALTER TABLE [dbo].[HtmlContent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HtmlContent_dbo.PersonAlias_ApprovedByPersonAliasId] FOREIGN KEY([ApprovedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[HtmlContent] CHECK CONSTRAINT [FK_dbo.HtmlContent_dbo.PersonAlias_ApprovedByPersonAliasId]
GO
ALTER TABLE [dbo].[HtmlContent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HtmlContent_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[HtmlContent] CHECK CONSTRAINT [FK_dbo.HtmlContent_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[HtmlContent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.HtmlContent_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[HtmlContent] CHECK CONSTRAINT [FK_dbo.HtmlContent_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[IdentityVerification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.IdentityVerification_dbo.IdentityVerificationCode_IdentityVerificationCodeId] FOREIGN KEY([IdentityVerificationCodeId])
REFERENCES [dbo].[IdentityVerificationCode] ([Id])
GO
ALTER TABLE [dbo].[IdentityVerification] CHECK CONSTRAINT [FK_dbo.IdentityVerification_dbo.IdentityVerificationCode_IdentityVerificationCodeId]
GO
ALTER TABLE [dbo].[IdentityVerification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.IdentityVerification_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[IdentityVerification] CHECK CONSTRAINT [FK_dbo.IdentityVerification_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[IdentityVerification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.IdentityVerification_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[IdentityVerification] CHECK CONSTRAINT [FK_dbo.IdentityVerification_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[IdentityVerificationCode]  WITH CHECK ADD  CONSTRAINT [FK_dbo.IdentityVerificationCode_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[IdentityVerificationCode] CHECK CONSTRAINT [FK_dbo.IdentityVerificationCode_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[IdentityVerificationCode]  WITH CHECK ADD  CONSTRAINT [FK_dbo.IdentityVerificationCode_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[IdentityVerificationCode] CHECK CONSTRAINT [FK_dbo.IdentityVerificationCode_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Interaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Interaction_dbo.InteractionComponent_InteractionComponentId] FOREIGN KEY([InteractionComponentId])
REFERENCES [dbo].[InteractionComponent] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Interaction] CHECK CONSTRAINT [FK_dbo.Interaction_dbo.InteractionComponent_InteractionComponentId]
GO
ALTER TABLE [dbo].[Interaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Interaction_dbo.InteractionSession_InteractionSessionId] FOREIGN KEY([InteractionSessionId])
REFERENCES [dbo].[InteractionSession] ([Id])
GO
ALTER TABLE [dbo].[Interaction] CHECK CONSTRAINT [FK_dbo.Interaction_dbo.InteractionSession_InteractionSessionId]
GO
ALTER TABLE [dbo].[Interaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Interaction_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Interaction] CHECK CONSTRAINT [FK_dbo.Interaction_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Interaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Interaction_dbo.PersonalDevice_PersonalDeviceId] FOREIGN KEY([PersonalDeviceId])
REFERENCES [dbo].[PersonalDevice] ([Id])
GO
ALTER TABLE [dbo].[Interaction] CHECK CONSTRAINT [FK_dbo.Interaction_dbo.PersonalDevice_PersonalDeviceId]
GO
ALTER TABLE [dbo].[InteractionChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionChannel_dbo.DefinedValue_ChannelTypeMediumValueId] FOREIGN KEY([ChannelTypeMediumValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[InteractionChannel] CHECK CONSTRAINT [FK_dbo.InteractionChannel_dbo.DefinedValue_ChannelTypeMediumValueId]
GO
ALTER TABLE [dbo].[InteractionChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionChannel_dbo.EntityType_ComponentEntityTypeId] FOREIGN KEY([ComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[InteractionChannel] CHECK CONSTRAINT [FK_dbo.InteractionChannel_dbo.EntityType_ComponentEntityTypeId]
GO
ALTER TABLE [dbo].[InteractionChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionChannel_dbo.EntityType_InteractionEntityTypeId] FOREIGN KEY([InteractionEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[InteractionChannel] CHECK CONSTRAINT [FK_dbo.InteractionChannel_dbo.EntityType_InteractionEntityTypeId]
GO
ALTER TABLE [dbo].[InteractionChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionChannel_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionChannel] CHECK CONSTRAINT [FK_dbo.InteractionChannel_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionChannel]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionChannel_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionChannel] CHECK CONSTRAINT [FK_dbo.InteractionChannel_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionComponent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionComponent_dbo.InteractionChannel_InteractionChannelId] FOREIGN KEY([InteractionChannelId])
REFERENCES [dbo].[InteractionChannel] ([Id])
GO
ALTER TABLE [dbo].[InteractionComponent] CHECK CONSTRAINT [FK_dbo.InteractionComponent_dbo.InteractionChannel_InteractionChannelId]
GO
ALTER TABLE [dbo].[InteractionComponent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionComponent_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionComponent] CHECK CONSTRAINT [FK_dbo.InteractionComponent_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionComponent]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionComponent_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionComponent] CHECK CONSTRAINT [FK_dbo.InteractionComponent_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionDeviceType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionDeviceType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionDeviceType] CHECK CONSTRAINT [FK_dbo.InteractionDeviceType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionDeviceType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionDeviceType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionDeviceType] CHECK CONSTRAINT [FK_dbo.InteractionDeviceType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionEntity_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InteractionEntity] CHECK CONSTRAINT [FK_dbo.InteractionEntity_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[InteractionSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSession_dbo.InteractionChannel_InteractionChannelId] FOREIGN KEY([InteractionChannelId])
REFERENCES [dbo].[InteractionChannel] ([Id])
GO
ALTER TABLE [dbo].[InteractionSession] CHECK CONSTRAINT [FK_dbo.InteractionSession_dbo.InteractionChannel_InteractionChannelId]
GO
ALTER TABLE [dbo].[InteractionSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSession_dbo.InteractionDeviceType_DeviceTypeId] FOREIGN KEY([DeviceTypeId])
REFERENCES [dbo].[InteractionDeviceType] ([Id])
GO
ALTER TABLE [dbo].[InteractionSession] CHECK CONSTRAINT [FK_dbo.InteractionSession_dbo.InteractionDeviceType_DeviceTypeId]
GO
ALTER TABLE [dbo].[InteractionSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSession_dbo.InteractionSessionLocation_InteractionSessionLocationId] FOREIGN KEY([InteractionSessionLocationId])
REFERENCES [dbo].[InteractionSessionLocation] ([Id])
GO
ALTER TABLE [dbo].[InteractionSession] CHECK CONSTRAINT [FK_dbo.InteractionSession_dbo.InteractionSessionLocation_InteractionSessionLocationId]
GO
ALTER TABLE [dbo].[InteractionSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSession_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionSession] CHECK CONSTRAINT [FK_dbo.InteractionSession_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSession_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionSession] CHECK CONSTRAINT [FK_dbo.InteractionSession_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionSessionLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.DefinedValue_CountryValueId] FOREIGN KEY([CountryValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[InteractionSessionLocation] CHECK CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.DefinedValue_CountryValueId]
GO
ALTER TABLE [dbo].[InteractionSessionLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.DefinedValue_RegionValueId] FOREIGN KEY([RegionValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[InteractionSessionLocation] CHECK CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.DefinedValue_RegionValueId]
GO
ALTER TABLE [dbo].[InteractionSessionLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionSessionLocation] CHECK CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractionSessionLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractionSessionLocation] CHECK CONSTRAINT [FK_dbo.InteractionSessionLocation_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_ActionBackgroundImageBinaryFileId] FOREIGN KEY([ActionBackgroundImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_ActionBackgroundImageBinaryFileId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_AudienceBackgroundImageBinaryFileId] FOREIGN KEY([AudienceBackgroundImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_AudienceBackgroundImageBinaryFileId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_NoActionHeaderImageBinaryFileId] FOREIGN KEY([NoActionHeaderImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_NoActionHeaderImageBinaryFileId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_PhotoBinaryFileId] FOREIGN KEY([PhotoBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_PhotoBinaryFileId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_WelcomeHeaderImageBinaryFileId] FOREIGN KEY([WelcomeHeaderImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.BinaryFile_WelcomeHeaderImageBinaryFileId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperience]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperience_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperience] CHECK CONSTRAINT [FK_dbo.InteractiveExperience_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.EntityType_ActionEntityTypeId] FOREIGN KEY([ActionEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAction] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.EntityType_ActionEntityTypeId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.EntityType_ResponseVisualEntityTypeId] FOREIGN KEY([ResponseVisualEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAction] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.EntityType_ResponseVisualEntityTypeId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.InteractiveExperience_InteractiveExperienceId] FOREIGN KEY([InteractiveExperienceId])
REFERENCES [dbo].[InteractiveExperience] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InteractiveExperienceAction] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.InteractiveExperience_InteractiveExperienceId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAction] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAction] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAction_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractionSession_InteractionSessionId] FOREIGN KEY([InteractionSessionId])
REFERENCES [dbo].[InteractionSession] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractionSession_InteractionSessionId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractiveExperienceAction_InteractiveExperienceActionId] FOREIGN KEY([InteractiveExperienceActionId])
REFERENCES [dbo].[InteractiveExperienceAction] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractiveExperienceAction_InteractiveExperienceActionId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractiveExperienceOccurrence_InteractiveExperienceOccurrenceId] FOREIGN KEY([InteractiveExperienceOccurrenceId])
REFERENCES [dbo].[InteractiveExperienceOccurrence] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractiveExperienceOccurrence_InteractiveExperienceOccurrenceId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceAnswer] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.InteractiveExperienceAction_CurrentlyShownActionId] FOREIGN KEY([CurrentlyShownActionId])
REFERENCES [dbo].[InteractiveExperienceAction] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.InteractiveExperienceAction_CurrentlyShownActionId]
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.InteractiveExperienceSchedule_InteractiveExperienceScheduleId] FOREIGN KEY([InteractiveExperienceScheduleId])
REFERENCES [dbo].[InteractiveExperienceSchedule] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.InteractiveExperienceSchedule_InteractiveExperienceScheduleId]
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceOccurrence] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceOccurrence_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.InteractiveExperience_InteractiveExperienceId] FOREIGN KEY([InteractiveExperienceId])
REFERENCES [dbo].[InteractiveExperience] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.InteractiveExperience_InteractiveExperienceId]
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceSchedule] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceSchedule_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.InteractiveExperienceSchedule_InteractiveExperienceScheduleId] FOREIGN KEY([InteractiveExperienceScheduleId])
REFERENCES [dbo].[InteractiveExperienceSchedule] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.InteractiveExperienceSchedule_InteractiveExperienceScheduleId]
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[InteractiveExperienceScheduleCampus] CHECK CONSTRAINT [FK_dbo.InteractiveExperienceScheduleCampus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaApplication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaApplication_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LavaApplication] CHECK CONSTRAINT [FK_dbo.LavaApplication_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaApplication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaApplication_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LavaApplication] CHECK CONSTRAINT [FK_dbo.LavaApplication_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaEndpoint]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaEndpoint_dbo.LavaApplication_LavaApplicationId] FOREIGN KEY([LavaApplicationId])
REFERENCES [dbo].[LavaApplication] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LavaEndpoint] CHECK CONSTRAINT [FK_dbo.LavaEndpoint_dbo.LavaApplication_LavaApplicationId]
GO
ALTER TABLE [dbo].[LavaEndpoint]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaEndpoint_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LavaEndpoint] CHECK CONSTRAINT [FK_dbo.LavaEndpoint_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaEndpoint]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaEndpoint_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LavaEndpoint] CHECK CONSTRAINT [FK_dbo.LavaEndpoint_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaShortcode]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaShortcode_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LavaShortcode] CHECK CONSTRAINT [FK_dbo.LavaShortcode_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaShortcode]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaShortcode_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LavaShortcode] CHECK CONSTRAINT [FK_dbo.LavaShortcode_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LavaShortcodeCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaShortcodeCategory_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LavaShortcodeCategory] CHECK CONSTRAINT [FK_dbo.LavaShortcodeCategory_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[LavaShortcodeCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaShortcodeCategory_dbo.LavaShortcode_LavaShortcodeId] FOREIGN KEY([LavaShortcodeId])
REFERENCES [dbo].[LavaShortcode] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LavaShortcodeCategory] CHECK CONSTRAINT [FK_dbo.LavaShortcodeCategory_dbo.LavaShortcode_LavaShortcodeId]
GO
ALTER TABLE [dbo].[Layout]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Layout_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Layout] CHECK CONSTRAINT [FK_dbo.Layout_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Layout]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Layout_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Layout] CHECK CONSTRAINT [FK_dbo.Layout_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Layout]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Layout_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Layout] CHECK CONSTRAINT [FK_dbo.Layout_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[LearningActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivity_dbo.EntityType_ActivityComponentId] FOREIGN KEY([ActivityComponentId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[LearningActivity] CHECK CONSTRAINT [FK_dbo.LearningActivity_dbo.EntityType_ActivityComponentId]
GO
ALTER TABLE [dbo].[LearningActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningActivity] CHECK CONSTRAINT [FK_dbo.LearningActivity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningActivity] CHECK CONSTRAINT [FK_dbo.LearningActivity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClass]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClass_dbo.Group_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[LearningClass] CHECK CONSTRAINT [FK_dbo.LearningClass_dbo.Group_Id]
GO
ALTER TABLE [dbo].[LearningClass]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClass_dbo.LearningCourse_LearningCourseId] FOREIGN KEY([LearningCourseId])
REFERENCES [dbo].[LearningCourse] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningClass] CHECK CONSTRAINT [FK_dbo.LearningClass_dbo.LearningCourse_LearningCourseId]
GO
ALTER TABLE [dbo].[LearningClass]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClass_dbo.LearningGradingSystem_LearningGradingSystemId] FOREIGN KEY([LearningGradingSystemId])
REFERENCES [dbo].[LearningGradingSystem] ([Id])
GO
ALTER TABLE [dbo].[LearningClass] CHECK CONSTRAINT [FK_dbo.LearningClass_dbo.LearningGradingSystem_LearningGradingSystemId]
GO
ALTER TABLE [dbo].[LearningClass]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClass_dbo.LearningSemester_LearningSemesterId] FOREIGN KEY([LearningSemesterId])
REFERENCES [dbo].[LearningSemester] ([Id])
GO
ALTER TABLE [dbo].[LearningClass] CHECK CONSTRAINT [FK_dbo.LearningClass_dbo.LearningSemester_LearningSemesterId]
GO
ALTER TABLE [dbo].[LearningClassActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassActivity_dbo.LearningActivity_LearningActivityId] FOREIGN KEY([LearningActivityId])
REFERENCES [dbo].[LearningActivity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningClassActivity] CHECK CONSTRAINT [FK_dbo.LearningClassActivity_dbo.LearningActivity_LearningActivityId]
GO
ALTER TABLE [dbo].[LearningClassActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassActivity_dbo.LearningClass_LearningClassId] FOREIGN KEY([LearningClassId])
REFERENCES [dbo].[LearningClass] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivity] CHECK CONSTRAINT [FK_dbo.LearningClassActivity_dbo.LearningClass_LearningClassId]
GO
ALTER TABLE [dbo].[LearningClassActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassActivity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivity] CHECK CONSTRAINT [FK_dbo.LearningClassActivity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassActivity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivity] CHECK CONSTRAINT [FK_dbo.LearningClassActivity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassActivity_dbo.WorkflowType_CompletionWorkflowTypeId] FOREIGN KEY([CompletionWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivity] CHECK CONSTRAINT [FK_dbo.LearningClassActivity_dbo.WorkflowType_CompletionWorkflowTypeId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.Communication_SentNotificationCommunicationId] FOREIGN KEY([SentNotificationCommunicationId])
REFERENCES [dbo].[Communication] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.Communication_SentNotificationCommunicationId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.LearningParticipant_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[LearningParticipant] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.LearningParticipant_StudentId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_CompletedByPersonAliasId] FOREIGN KEY([CompletedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_CompletedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_GradedByPersonAliasId] FOREIGN KEY([GradedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_GradedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningActivityCompletion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassActivityCompletion_dbo.LearningClassActivity_LearningClassActivityId] FOREIGN KEY([LearningClassActivityId])
REFERENCES [dbo].[LearningClassActivity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningClassActivityCompletion] CHECK CONSTRAINT [FK_dbo.LearningClassActivityCompletion_dbo.LearningClassActivity_LearningClassActivityId]
GO
ALTER TABLE [dbo].[LearningClassAnnouncement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassAnnouncement_dbo.LearningClass_LearningClassId] FOREIGN KEY([LearningClassId])
REFERENCES [dbo].[LearningClass] ([Id])
GO
ALTER TABLE [dbo].[LearningClassAnnouncement] CHECK CONSTRAINT [FK_dbo.LearningClassAnnouncement_dbo.LearningClass_LearningClassId]
GO
ALTER TABLE [dbo].[LearningClassAnnouncement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassAnnouncement_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassAnnouncement] CHECK CONSTRAINT [FK_dbo.LearningClassAnnouncement_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassAnnouncement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassAnnouncement_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassAnnouncement] CHECK CONSTRAINT [FK_dbo.LearningClassAnnouncement_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassContentPage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassContentPage_dbo.LearningClass_LearningClassId] FOREIGN KEY([LearningClassId])
REFERENCES [dbo].[LearningClass] ([Id])
GO
ALTER TABLE [dbo].[LearningClassContentPage] CHECK CONSTRAINT [FK_dbo.LearningClassContentPage_dbo.LearningClass_LearningClassId]
GO
ALTER TABLE [dbo].[LearningClassContentPage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassContentPage_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassContentPage] CHECK CONSTRAINT [FK_dbo.LearningClassContentPage_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningClassContentPage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningClassContentPage_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningClassContentPage] CHECK CONSTRAINT [FK_dbo.LearningClassContentPage_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningCourse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourse_dbo.BinaryFile_ImageBinaryFileId] FOREIGN KEY([ImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[LearningCourse] CHECK CONSTRAINT [FK_dbo.LearningCourse_dbo.BinaryFile_ImageBinaryFileId]
GO
ALTER TABLE [dbo].[LearningCourse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourse_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[LearningCourse] CHECK CONSTRAINT [FK_dbo.LearningCourse_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[LearningCourse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourse_dbo.LearningProgram_LearningProgramId] FOREIGN KEY([LearningProgramId])
REFERENCES [dbo].[LearningProgram] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningCourse] CHECK CONSTRAINT [FK_dbo.LearningCourse_dbo.LearningProgram_LearningProgramId]
GO
ALTER TABLE [dbo].[LearningCourse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourse_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningCourse] CHECK CONSTRAINT [FK_dbo.LearningCourse_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningCourse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourse_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningCourse] CHECK CONSTRAINT [FK_dbo.LearningCourse_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningCourse]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourse_dbo.WorkflowType_CompletionWorkflowTypeId] FOREIGN KEY([CompletionWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[LearningCourse] CHECK CONSTRAINT [FK_dbo.LearningCourse_dbo.WorkflowType_CompletionWorkflowTypeId]
GO
ALTER TABLE [dbo].[LearningCourseRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.LearningCourse_LearningCourseId] FOREIGN KEY([LearningCourseId])
REFERENCES [dbo].[LearningCourse] ([Id])
GO
ALTER TABLE [dbo].[LearningCourseRequirement] CHECK CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.LearningCourse_LearningCourseId]
GO
ALTER TABLE [dbo].[LearningCourseRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.LearningCourse_RequiredLearningCourseId] FOREIGN KEY([RequiredLearningCourseId])
REFERENCES [dbo].[LearningCourse] ([Id])
GO
ALTER TABLE [dbo].[LearningCourseRequirement] CHECK CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.LearningCourse_RequiredLearningCourseId]
GO
ALTER TABLE [dbo].[LearningCourseRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningCourseRequirement] CHECK CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningCourseRequirement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningCourseRequirement] CHECK CONSTRAINT [FK_dbo.LearningCourseRequirement_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningGradingSystem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningGradingSystem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningGradingSystem] CHECK CONSTRAINT [FK_dbo.LearningGradingSystem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningGradingSystem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningGradingSystem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningGradingSystem] CHECK CONSTRAINT [FK_dbo.LearningGradingSystem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningGradingSystemScale]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningGradingSystemScale_dbo.LearningGradingSystem_LearningGradingSystemId] FOREIGN KEY([LearningGradingSystemId])
REFERENCES [dbo].[LearningGradingSystem] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningGradingSystemScale] CHECK CONSTRAINT [FK_dbo.LearningGradingSystemScale_dbo.LearningGradingSystem_LearningGradingSystemId]
GO
ALTER TABLE [dbo].[LearningGradingSystemScale]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningGradingSystemScale_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningGradingSystemScale] CHECK CONSTRAINT [FK_dbo.LearningGradingSystemScale_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningGradingSystemScale]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningGradingSystemScale_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningGradingSystemScale] CHECK CONSTRAINT [FK_dbo.LearningGradingSystemScale_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningParticipant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningParticipant_dbo.GroupMember_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[GroupMember] ([Id])
GO
ALTER TABLE [dbo].[LearningParticipant] CHECK CONSTRAINT [FK_dbo.LearningParticipant_dbo.GroupMember_Id]
GO
ALTER TABLE [dbo].[LearningParticipant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningParticipant_dbo.LearningClass_LearningClassId] FOREIGN KEY([LearningClassId])
REFERENCES [dbo].[LearningClass] ([Id])
GO
ALTER TABLE [dbo].[LearningParticipant] CHECK CONSTRAINT [FK_dbo.LearningParticipant_dbo.LearningClass_LearningClassId]
GO
ALTER TABLE [dbo].[LearningParticipant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningParticipant_dbo.LearningGradingSystemScale_LearningGradingSystemScaleId] FOREIGN KEY([LearningGradingSystemScaleId])
REFERENCES [dbo].[LearningGradingSystemScale] ([Id])
GO
ALTER TABLE [dbo].[LearningParticipant] CHECK CONSTRAINT [FK_dbo.LearningParticipant_dbo.LearningGradingSystemScale_LearningGradingSystemScaleId]
GO
ALTER TABLE [dbo].[LearningParticipant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningParticipant_dbo.LearningProgramCompletion_LearningProgramCompletionId] FOREIGN KEY([LearningProgramCompletionId])
REFERENCES [dbo].[LearningProgramCompletion] ([Id])
GO
ALTER TABLE [dbo].[LearningParticipant] CHECK CONSTRAINT [FK_dbo.LearningParticipant_dbo.LearningProgramCompletion_LearningProgramCompletionId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.BinaryFile_ImageBinaryFileId] FOREIGN KEY([ImageBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.BinaryFile_ImageBinaryFileId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.LearningGradingSystem_DefaultLearningGradingSystemId] FOREIGN KEY([DefaultLearningGradingSystemId])
REFERENCES [dbo].[LearningGradingSystem] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.LearningGradingSystem_DefaultLearningGradingSystemId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.SystemCommunication_SystemCommunicationId] FOREIGN KEY([SystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.SystemCommunication_SystemCommunicationId]
GO
ALTER TABLE [dbo].[LearningProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgram_dbo.WorkflowType_CompletionWorkflowTypeId] FOREIGN KEY([CompletionWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[LearningProgram] CHECK CONSTRAINT [FK_dbo.LearningProgram_dbo.WorkflowType_CompletionWorkflowTypeId]
GO
ALTER TABLE [dbo].[LearningProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[LearningProgramCompletion] CHECK CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[LearningProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.LearningProgram_LearningProgramId] FOREIGN KEY([LearningProgramId])
REFERENCES [dbo].[LearningProgram] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningProgramCompletion] CHECK CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.LearningProgram_LearningProgramId]
GO
ALTER TABLE [dbo].[LearningProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningProgramCompletion] CHECK CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningProgramCompletion] CHECK CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningProgramCompletion] CHECK CONSTRAINT [FK_dbo.LearningProgramCompletion_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[LearningSemester]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningSemester_dbo.LearningProgram_LearningProgramId] FOREIGN KEY([LearningProgramId])
REFERENCES [dbo].[LearningProgram] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningSemester] CHECK CONSTRAINT [FK_dbo.LearningSemester_dbo.LearningProgram_LearningProgramId]
GO
ALTER TABLE [dbo].[LearningSemester]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningSemester_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningSemester] CHECK CONSTRAINT [FK_dbo.LearningSemester_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[LearningSemester]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LearningSemester_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[LearningSemester] CHECK CONSTRAINT [FK_dbo.LearningSemester_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Location_dbo.BinaryFile_ImageId] FOREIGN KEY([ImageId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_dbo.Location_dbo.BinaryFile_ImageId]
GO
ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Location_dbo.DefinedValue_LocationTypeValueId] FOREIGN KEY([LocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_dbo.Location_dbo.DefinedValue_LocationTypeValueId]
GO
ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Location_dbo.Device_PrinterDeviceId] FOREIGN KEY([PrinterDeviceId])
REFERENCES [dbo].[Device] ([Id])
GO
ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_dbo.Location_dbo.Device_PrinterDeviceId]
GO
ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Location_dbo.Location_ParentLocationId] FOREIGN KEY([ParentLocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_dbo.Location_dbo.Location_ParentLocationId]
GO
ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Location_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_dbo.Location_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Location_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_dbo.Location_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaAccount_dbo.EntityType_ComponentEntityTypeId] FOREIGN KEY([ComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[MediaAccount] CHECK CONSTRAINT [FK_dbo.MediaAccount_dbo.EntityType_ComponentEntityTypeId]
GO
ALTER TABLE [dbo].[MediaAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaAccount_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MediaAccount] CHECK CONSTRAINT [FK_dbo.MediaAccount_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaAccount_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MediaAccount] CHECK CONSTRAINT [FK_dbo.MediaAccount_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaElement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaElement_dbo.MediaFolder_MediaFolderId] FOREIGN KEY([MediaFolderId])
REFERENCES [dbo].[MediaFolder] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MediaElement] CHECK CONSTRAINT [FK_dbo.MediaElement_dbo.MediaFolder_MediaFolderId]
GO
ALTER TABLE [dbo].[MediaElement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaElement_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MediaElement] CHECK CONSTRAINT [FK_dbo.MediaElement_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaElement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaElement_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MediaElement] CHECK CONSTRAINT [FK_dbo.MediaElement_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaFolder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaFolder_dbo.Attribute_ContentChannelAttributeId] FOREIGN KEY([ContentChannelAttributeId])
REFERENCES [dbo].[Attribute] ([Id])
GO
ALTER TABLE [dbo].[MediaFolder] CHECK CONSTRAINT [FK_dbo.MediaFolder_dbo.Attribute_ContentChannelAttributeId]
GO
ALTER TABLE [dbo].[MediaFolder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaFolder_dbo.ContentChannel_ContentChannelId] FOREIGN KEY([ContentChannelId])
REFERENCES [dbo].[ContentChannel] ([Id])
GO
ALTER TABLE [dbo].[MediaFolder] CHECK CONSTRAINT [FK_dbo.MediaFolder_dbo.ContentChannel_ContentChannelId]
GO
ALTER TABLE [dbo].[MediaFolder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaFolder_dbo.MediaAccount_MediaAccountId] FOREIGN KEY([MediaAccountId])
REFERENCES [dbo].[MediaAccount] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MediaFolder] CHECK CONSTRAINT [FK_dbo.MediaFolder_dbo.MediaAccount_MediaAccountId]
GO
ALTER TABLE [dbo].[MediaFolder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaFolder_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MediaFolder] CHECK CONSTRAINT [FK_dbo.MediaFolder_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaFolder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaFolder_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MediaFolder] CHECK CONSTRAINT [FK_dbo.MediaFolder_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MediaFolder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MediaFolder_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[MediaFolder] CHECK CONSTRAINT [FK_dbo.MediaFolder_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[MergeTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MergeTemplate_dbo.BinaryFile_TemplateBinaryFileId] FOREIGN KEY([TemplateBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[MergeTemplate] CHECK CONSTRAINT [FK_dbo.MergeTemplate_dbo.BinaryFile_TemplateBinaryFileId]
GO
ALTER TABLE [dbo].[MergeTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MergeTemplate_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[MergeTemplate] CHECK CONSTRAINT [FK_dbo.MergeTemplate_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[MergeTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MergeTemplate_dbo.EntityType_MergeTemplateTypeEntityTypeId] FOREIGN KEY([MergeTemplateTypeEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[MergeTemplate] CHECK CONSTRAINT [FK_dbo.MergeTemplate_dbo.EntityType_MergeTemplateTypeEntityTypeId]
GO
ALTER TABLE [dbo].[MergeTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MergeTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MergeTemplate] CHECK CONSTRAINT [FK_dbo.MergeTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MergeTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MergeTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MergeTemplate] CHECK CONSTRAINT [FK_dbo.MergeTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MergeTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MergeTemplate_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MergeTemplate] CHECK CONSTRAINT [FK_dbo.MergeTemplate_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.DefinedValue_MeasurementClassificationValueId] FOREIGN KEY([MeasurementClassificationValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.DefinedValue_MeasurementClassificationValueId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.DefinedValue_SourceValueTypeId] FOREIGN KEY([SourceValueTypeId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.DefinedValue_SourceValueTypeId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_AdminPersonAliasId] FOREIGN KEY([AdminPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_AdminPersonAliasId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_StewardPersonAliasId] FOREIGN KEY([MetricChampionPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.PersonAlias_StewardPersonAliasId]
GO
ALTER TABLE [dbo].[Metric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Metric_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[Metric] CHECK CONSTRAINT [FK_dbo.Metric_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[MetricCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricCategory_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MetricCategory] CHECK CONSTRAINT [FK_dbo.MetricCategory_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[MetricCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricCategory_dbo.Metric_MetricId] FOREIGN KEY([MetricId])
REFERENCES [dbo].[Metric] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MetricCategory] CHECK CONSTRAINT [FK_dbo.MetricCategory_dbo.Metric_MetricId]
GO
ALTER TABLE [dbo].[MetricPartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricPartition_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[MetricPartition] CHECK CONSTRAINT [FK_dbo.MetricPartition_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[MetricPartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricPartition_dbo.Metric_MetricId] FOREIGN KEY([MetricId])
REFERENCES [dbo].[Metric] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MetricPartition] CHECK CONSTRAINT [FK_dbo.MetricPartition_dbo.Metric_MetricId]
GO
ALTER TABLE [dbo].[MetricPartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricPartition_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MetricPartition] CHECK CONSTRAINT [FK_dbo.MetricPartition_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MetricPartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricPartition_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MetricPartition] CHECK CONSTRAINT [FK_dbo.MetricPartition_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MetricValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValue_dbo.Metric_MetricId] FOREIGN KEY([MetricId])
REFERENCES [dbo].[Metric] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MetricValue] CHECK CONSTRAINT [FK_dbo.MetricValue_dbo.Metric_MetricId]
GO
ALTER TABLE [dbo].[MetricValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValue_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MetricValue] CHECK CONSTRAINT [FK_dbo.MetricValue_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MetricValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValue_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MetricValue] CHECK CONSTRAINT [FK_dbo.MetricValue_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[MetricValuePartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValuePartition_dbo.MetricPartition_MetricPartitionId] FOREIGN KEY([MetricPartitionId])
REFERENCES [dbo].[MetricPartition] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MetricValuePartition] CHECK CONSTRAINT [FK_dbo.MetricValuePartition_dbo.MetricPartition_MetricPartitionId]
GO
ALTER TABLE [dbo].[MetricValuePartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValuePartition_dbo.MetricValue_MetricValueId] FOREIGN KEY([MetricValueId])
REFERENCES [dbo].[MetricValue] ([Id])
GO
ALTER TABLE [dbo].[MetricValuePartition] CHECK CONSTRAINT [FK_dbo.MetricValuePartition_dbo.MetricValue_MetricValueId]
GO
ALTER TABLE [dbo].[MetricValuePartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValuePartition_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MetricValuePartition] CHECK CONSTRAINT [FK_dbo.MetricValuePartition_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[MetricValuePartition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MetricValuePartition_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[MetricValuePartition] CHECK CONSTRAINT [FK_dbo.MetricValuePartition_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NcoaHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NcoaHistory_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NcoaHistory] CHECK CONSTRAINT [FK_dbo.NcoaHistory_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[NcoaHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NcoaHistory_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NcoaHistory] CHECK CONSTRAINT [FK_dbo.NcoaHistory_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Note_dbo.NoteType_NoteTypeId] FOREIGN KEY([NoteTypeId])
REFERENCES [dbo].[NoteType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_dbo.Note_dbo.NoteType_NoteTypeId]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Note_dbo.Note_ParentNoteId] FOREIGN KEY([ParentNoteId])
REFERENCES [dbo].[Note] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_dbo.Note_dbo.Note_ParentNoteId]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Note_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_dbo.Note_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Note_dbo.PersonAlias_EditedByPersonAliasId] FOREIGN KEY([EditedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_dbo.Note_dbo.PersonAlias_EditedByPersonAliasId]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Note_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_dbo.Note_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteAttachment_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[NoteAttachment] CHECK CONSTRAINT [FK_dbo.NoteAttachment_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[NoteAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteAttachment_dbo.Note_NoteId] FOREIGN KEY([NoteId])
REFERENCES [dbo].[Note] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NoteAttachment] CHECK CONSTRAINT [FK_dbo.NoteAttachment_dbo.Note_NoteId]
GO
ALTER TABLE [dbo].[NoteAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteAttachment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NoteAttachment] CHECK CONSTRAINT [FK_dbo.NoteAttachment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteAttachment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NoteAttachment] CHECK CONSTRAINT [FK_dbo.NoteAttachment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteType_dbo.BinaryFileType_BinaryFileTypeId] FOREIGN KEY([BinaryFileTypeId])
REFERENCES [dbo].[BinaryFileType] ([Id])
GO
ALTER TABLE [dbo].[NoteType] CHECK CONSTRAINT [FK_dbo.NoteType_dbo.BinaryFileType_BinaryFileTypeId]
GO
ALTER TABLE [dbo].[NoteType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[NoteType] CHECK CONSTRAINT [FK_dbo.NoteType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[NoteType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NoteType] CHECK CONSTRAINT [FK_dbo.NoteType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NoteType] CHECK CONSTRAINT [FK_dbo.NoteType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.Group_WatcherGroupId] FOREIGN KEY([WatcherGroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.Group_WatcherGroupId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.NoteType_NoteTypeId] FOREIGN KEY([NoteTypeId])
REFERENCES [dbo].[NoteType] ([Id])
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.NoteType_NoteTypeId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.Note_NoteId] FOREIGN KEY([NoteId])
REFERENCES [dbo].[Note] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.Note_NoteId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NoteWatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NoteWatch_dbo.PersonAlias_WatcherPersonAliasId] FOREIGN KEY([WatcherPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NoteWatch] CHECK CONSTRAINT [FK_dbo.NoteWatch_dbo.PersonAlias_WatcherPersonAliasId]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Notification_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_dbo.Notification_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Notification_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_dbo.Notification_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NotificationMessage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessage_dbo.NotificationMessageType_NotificationMessageTypeId] FOREIGN KEY([NotificationMessageTypeId])
REFERENCES [dbo].[NotificationMessageType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NotificationMessage] CHECK CONSTRAINT [FK_dbo.NotificationMessage_dbo.NotificationMessageType_NotificationMessageTypeId]
GO
ALTER TABLE [dbo].[NotificationMessage]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessage_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessage] CHECK CONSTRAINT [FK_dbo.NotificationMessage_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[NotificationMessageType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessageType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessageType] CHECK CONSTRAINT [FK_dbo.NotificationMessageType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[NotificationMessageType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessageType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessageType] CHECK CONSTRAINT [FK_dbo.NotificationMessageType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[NotificationMessageType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessageType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessageType] CHECK CONSTRAINT [FK_dbo.NotificationMessageType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NotificationMessageType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessageType_dbo.Site_RelatedMobileApplicationSiteId] FOREIGN KEY([RelatedMobileApplicationSiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessageType] CHECK CONSTRAINT [FK_dbo.NotificationMessageType_dbo.Site_RelatedMobileApplicationSiteId]
GO
ALTER TABLE [dbo].[NotificationMessageType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessageType_dbo.Site_RelatedTvApplicationSiteId] FOREIGN KEY([RelatedTvApplicationSiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessageType] CHECK CONSTRAINT [FK_dbo.NotificationMessageType_dbo.Site_RelatedTvApplicationSiteId]
GO
ALTER TABLE [dbo].[NotificationMessageType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationMessageType_dbo.Site_RelatedWebSiteId] FOREIGN KEY([RelatedWebSiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[NotificationMessageType] CHECK CONSTRAINT [FK_dbo.NotificationMessageType_dbo.Site_RelatedWebSiteId]
GO
ALTER TABLE [dbo].[NotificationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationRecipient_dbo.Notification_NotificationId] FOREIGN KEY([NotificationId])
REFERENCES [dbo].[Notification] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NotificationRecipient] CHECK CONSTRAINT [FK_dbo.NotificationRecipient_dbo.Notification_NotificationId]
GO
ALTER TABLE [dbo].[NotificationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationRecipient_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NotificationRecipient] CHECK CONSTRAINT [FK_dbo.NotificationRecipient_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[NotificationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationRecipient_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NotificationRecipient] CHECK CONSTRAINT [FK_dbo.NotificationRecipient_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[NotificationRecipient]  WITH CHECK ADD  CONSTRAINT [FK_dbo.NotificationRecipient_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[NotificationRecipient] CHECK CONSTRAINT [FK_dbo.NotificationRecipient_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Page]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Page_dbo.BinaryFile_IconBinaryFileId] FOREIGN KEY([IconBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Page] CHECK CONSTRAINT [FK_dbo.Page_dbo.BinaryFile_IconBinaryFileId]
GO
ALTER TABLE [dbo].[Page]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Page_dbo.Layout_LayoutId] FOREIGN KEY([LayoutId])
REFERENCES [dbo].[Layout] ([Id])
GO
ALTER TABLE [dbo].[Page] CHECK CONSTRAINT [FK_dbo.Page_dbo.Layout_LayoutId]
GO
ALTER TABLE [dbo].[Page]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Page_dbo.Page_ParentPageId] FOREIGN KEY([ParentPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Page] CHECK CONSTRAINT [FK_dbo.Page_dbo.Page_ParentPageId]
GO
ALTER TABLE [dbo].[Page]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Page_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Page] CHECK CONSTRAINT [FK_dbo.Page_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Page]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Page_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Page] CHECK CONSTRAINT [FK_dbo.Page_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Page]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Page_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[Page] CHECK CONSTRAINT [FK_dbo.Page_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[PageContext]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageContext_dbo.Page_PageId] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PageContext] CHECK CONSTRAINT [FK_dbo.PageContext_dbo.Page_PageId]
GO
ALTER TABLE [dbo].[PageContext]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageContext_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PageContext] CHECK CONSTRAINT [FK_dbo.PageContext_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PageContext]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageContext_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PageContext] CHECK CONSTRAINT [FK_dbo.PageContext_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PageRoute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageRoute_dbo.Page_PageId] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PageRoute] CHECK CONSTRAINT [FK_dbo.PageRoute_dbo.Page_PageId]
GO
ALTER TABLE [dbo].[PageRoute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageRoute_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PageRoute] CHECK CONSTRAINT [FK_dbo.PageRoute_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PageRoute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageRoute_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PageRoute] CHECK CONSTRAINT [FK_dbo.PageRoute_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PageShortLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PageShortLink_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[PageShortLink] CHECK CONSTRAINT [FK_dbo.PageShortLink_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[PageShortLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SiteUrlMap_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PageShortLink] CHECK CONSTRAINT [FK_dbo.SiteUrlMap_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PageShortLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SiteUrlMap_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PageShortLink] CHECK CONSTRAINT [FK_dbo.SiteUrlMap_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PageShortLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SiteUrlMap_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PageShortLink] CHECK CONSTRAINT [FK_dbo.SiteUrlMap_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[PeerNetwork]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PeerNetwork_dbo.DefinedValue_RelationshipTypeValueId] FOREIGN KEY([RelationshipTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[PeerNetwork] CHECK CONSTRAINT [FK_dbo.PeerNetwork_dbo.DefinedValue_RelationshipTypeValueId]
GO
ALTER TABLE [dbo].[PeerNetwork]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PeerNetwork_dbo.Person_SourcePersonId] FOREIGN KEY([SourcePersonId])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PeerNetwork] CHECK CONSTRAINT [FK_dbo.PeerNetwork_dbo.Person_SourcePersonId]
GO
ALTER TABLE [dbo].[PeerNetwork]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PeerNetwork_dbo.Person_TargetPersonId] FOREIGN KEY([TargetPersonId])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PeerNetwork] CHECK CONSTRAINT [FK_dbo.PeerNetwork_dbo.Person_TargetPersonId]
GO
ALTER TABLE [dbo].[PersistedDataset]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersistedDataset_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[PersistedDataset] CHECK CONSTRAINT [FK_dbo.PersistedDataset_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[PersistedDataset]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersistedDataset_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersistedDataset] CHECK CONSTRAINT [FK_dbo.PersistedDataset_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersistedDataset]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersistedDataset_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersistedDataset] CHECK CONSTRAINT [FK_dbo.PersistedDataset_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersistedDataset]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersistedDataset_dbo.Schedule_PersistedScheduleId] FOREIGN KEY([PersistedScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[PersistedDataset] CHECK CONSTRAINT [FK_dbo.PersistedDataset_dbo.Schedule_PersistedScheduleId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.BinaryFile_PhotoId] FOREIGN KEY([PhotoId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.BinaryFile_PhotoId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.Campus_PrimaryCampusId] FOREIGN KEY([PrimaryCampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.Campus_PrimaryCampusId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_EthnicityValueId] FOREIGN KEY([EthnicityValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_EthnicityValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_MaritalStatusValueId] FOREIGN KEY([MaritalStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_MaritalStatusValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_PersonStatusValueId] FOREIGN KEY([ConnectionStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_PersonStatusValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_PreferredLanguageValueId] FOREIGN KEY([PreferredLanguageValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_PreferredLanguageValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RaceValueId] FOREIGN KEY([RaceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RaceValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordSourceValueId] FOREIGN KEY([RecordSourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordSourceValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordStatusReasonValueId] FOREIGN KEY([RecordStatusReasonValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordStatusReasonValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordStatusValueId] FOREIGN KEY([RecordStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordStatusValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordTypeValueId] FOREIGN KEY([RecordTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_RecordTypeValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_ReviewReasonValueId] FOREIGN KEY([ReviewReasonValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_ReviewReasonValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_SuffixValueId] FOREIGN KEY([SuffixValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_SuffixValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_TitleValueId] FOREIGN KEY([TitleValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.DefinedValue_TitleValueId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.FinancialAccount_ContributionFinancialAccountId] FOREIGN KEY([ContributionFinancialAccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.FinancialAccount_ContributionFinancialAccountId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.Group_GivingGroupId] FOREIGN KEY([GivingGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.Group_GivingGroupId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.Group_PrimaryFamilyId] FOREIGN KEY([PrimaryFamilyId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.Group_PrimaryFamilyId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Person_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_dbo.Person_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonAlias]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonAlias_dbo.Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAlias] CHECK CONSTRAINT [FK_dbo.PersonAlias_dbo.Person_PersonId]
GO
ALTER TABLE [dbo].[PersonAliasPersonalization]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonAliasPersonalization_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonAliasPersonalization] CHECK CONSTRAINT [FK_dbo.PersonAliasPersonalization_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonDuplicate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonDuplicate] CHECK CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonDuplicate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_DuplicatePersonAliasId] FOREIGN KEY([DuplicatePersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonDuplicate] CHECK CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_DuplicatePersonAliasId]
GO
ALTER TABLE [dbo].[PersonDuplicate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonDuplicate] CHECK CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonDuplicate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonDuplicate] CHECK CONSTRAINT [FK_dbo.PersonDuplicate_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonPreference]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonPreference_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonPreference] CHECK CONSTRAINT [FK_dbo.PersonPreference_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[PersonPreference]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonPreference_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonPreference] CHECK CONSTRAINT [FK_dbo.PersonPreference_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonPreviousName]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonPreviousName_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonPreviousName] CHECK CONSTRAINT [FK_dbo.PersonPreviousName_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonPreviousName]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonPreviousName_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonPreviousName] CHECK CONSTRAINT [FK_dbo.PersonPreviousName_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonPreviousName]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonPreviousName_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonPreviousName] CHECK CONSTRAINT [FK_dbo.PersonPreviousName_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[PersonScheduleExclusion] CHECK CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[PersonScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonScheduleExclusion] CHECK CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonScheduleExclusion] CHECK CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonScheduleExclusion] CHECK CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonScheduleExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonScheduleExclusion_ParentPersonScheduleExclusionId] FOREIGN KEY([ParentPersonScheduleExclusionId])
REFERENCES [dbo].[PersonScheduleExclusion] ([Id])
GO
ALTER TABLE [dbo].[PersonScheduleExclusion] CHECK CONSTRAINT [FK_dbo.PersonScheduleExclusion_dbo.PersonScheduleExclusion_ParentPersonScheduleExclusionId]
GO
ALTER TABLE [dbo].[PersonSearchKey]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSearchKey_dbo.DefinedValue_SearchTypeValueId] FOREIGN KEY([SearchTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[PersonSearchKey] CHECK CONSTRAINT [FK_dbo.PersonSearchKey_dbo.DefinedValue_SearchTypeValueId]
GO
ALTER TABLE [dbo].[PersonSearchKey]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSearchKey_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonSearchKey] CHECK CONSTRAINT [FK_dbo.PersonSearchKey_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonSearchKey]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSearchKey_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonSearchKey] CHECK CONSTRAINT [FK_dbo.PersonSearchKey_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonSearchKey]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSearchKey_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonSearchKey] CHECK CONSTRAINT [FK_dbo.PersonSearchKey_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonSignal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSignal_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonSignal] CHECK CONSTRAINT [FK_dbo.PersonSignal_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonSignal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSignal_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonSignal] CHECK CONSTRAINT [FK_dbo.PersonSignal_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonSignal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSignal_dbo.PersonAlias_OwnerPersonAliasId] FOREIGN KEY([OwnerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonSignal] CHECK CONSTRAINT [FK_dbo.PersonSignal_dbo.PersonAlias_OwnerPersonAliasId]
GO
ALTER TABLE [dbo].[PersonSignal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSignal_dbo.Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonSignal] CHECK CONSTRAINT [FK_dbo.PersonSignal_dbo.Person_PersonId]
GO
ALTER TABLE [dbo].[PersonSignal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonSignal_dbo.SignalType_SignalTypeId] FOREIGN KEY([SignalTypeId])
REFERENCES [dbo].[SignalType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonSignal] CHECK CONSTRAINT [FK_dbo.PersonSignal_dbo.SignalType_SignalTypeId]
GO
ALTER TABLE [dbo].[PersonToken]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonToken_dbo.Page_PageId] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonToken] CHECK CONSTRAINT [FK_dbo.PersonToken_dbo.Page_PageId]
GO
ALTER TABLE [dbo].[PersonToken]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonToken_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonToken] CHECK CONSTRAINT [FK_dbo.PersonToken_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonViewed]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonViewed_dbo.PersonAlias_TargetPersonAliasId] FOREIGN KEY([TargetPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonViewed] CHECK CONSTRAINT [FK_dbo.PersonViewed_dbo.PersonAlias_TargetPersonAliasId]
GO
ALTER TABLE [dbo].[PersonViewed]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonViewed_dbo.PersonAlias_ViewerPersonAliasId] FOREIGN KEY([ViewerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonViewed] CHECK CONSTRAINT [FK_dbo.PersonViewed_dbo.PersonAlias_ViewerPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalDevice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalDevice_dbo.DefinedValue_PersonalDeviceTypeValueId] FOREIGN KEY([PersonalDeviceTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[PersonalDevice] CHECK CONSTRAINT [FK_dbo.PersonalDevice_dbo.DefinedValue_PersonalDeviceTypeValueId]
GO
ALTER TABLE [dbo].[PersonalDevice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalDevice_dbo.DefinedValue_PlatformValueId] FOREIGN KEY([PlatformValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[PersonalDevice] CHECK CONSTRAINT [FK_dbo.PersonalDevice_dbo.DefinedValue_PlatformValueId]
GO
ALTER TABLE [dbo].[PersonalDevice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalDevice_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalDevice] CHECK CONSTRAINT [FK_dbo.PersonalDevice_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalDevice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalDevice_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalDevice] CHECK CONSTRAINT [FK_dbo.PersonalDevice_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalDevice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalDevice_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalDevice] CHECK CONSTRAINT [FK_dbo.PersonalDevice_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonalDevice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalDevice_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[PersonalDevice] CHECK CONSTRAINT [FK_dbo.PersonalDevice_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[PersonalLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLink] CHECK CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLink] CHECK CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLink] CHECK CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLink]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonalLinkSection_SectionId] FOREIGN KEY([SectionId])
REFERENCES [dbo].[PersonalLinkSection] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonalLink] CHECK CONSTRAINT [FK_dbo.PersonalLink_dbo.PersonalLinkSection_SectionId]
GO
ALTER TABLE [dbo].[PersonalLinkSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSection_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLinkSection] CHECK CONSTRAINT [FK_dbo.PersonalLinkSection_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLinkSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSection_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLinkSection] CHECK CONSTRAINT [FK_dbo.PersonalLinkSection_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLinkSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSection_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLinkSection] CHECK CONSTRAINT [FK_dbo.PersonalLinkSection_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder] CHECK CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder] CHECK CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder] CHECK CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonalLinkSection_SectionId] FOREIGN KEY([SectionId])
REFERENCES [dbo].[PersonalLinkSection] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonalLinkSectionOrder] CHECK CONSTRAINT [FK_dbo.PersonalLinkSectionOrder_dbo.PersonalLinkSection_SectionId]
GO
ALTER TABLE [dbo].[PersonalizationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.DataView_FilterDataViewId] FOREIGN KEY([FilterDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[PersonalizationSegment] CHECK CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.DataView_FilterDataViewId]
GO
ALTER TABLE [dbo].[PersonalizationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalizationSegment] CHECK CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalizationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PersonalizationSegment] CHECK CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PersonalizationSegment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.Schedule_PersistedScheduleId] FOREIGN KEY([PersistedScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
GO
ALTER TABLE [dbo].[PersonalizationSegment] CHECK CONSTRAINT [FK_dbo.PersonalizationSegment_dbo.Schedule_PersistedScheduleId]
GO
ALTER TABLE [dbo].[PersonalizationSegmentCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizationSegmentCategory_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonalizationSegmentCategory] CHECK CONSTRAINT [FK_dbo.PersonalizationSegmentCategory_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[PersonalizationSegmentCategory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizationSegmentCategory_dbo.PersonalizationSegment_PersonalizationSegmentId] FOREIGN KEY([PersonalizationSegmentId])
REFERENCES [dbo].[PersonalizationSegment] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonalizationSegmentCategory] CHECK CONSTRAINT [FK_dbo.PersonalizationSegmentCategory_dbo.PersonalizationSegment_PersonalizationSegmentId]
GO
ALTER TABLE [dbo].[PersonalizedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PersonalizedEntity_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[PersonalizedEntity] CHECK CONSTRAINT [FK_dbo.PersonalizedEntity_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[PhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PhoneNumber_dbo.DefinedValue_NumberTypeValueId] FOREIGN KEY([NumberTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[PhoneNumber] CHECK CONSTRAINT [FK_dbo.PhoneNumber_dbo.DefinedValue_NumberTypeValueId]
GO
ALTER TABLE [dbo].[PhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PhoneNumber_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PhoneNumber] CHECK CONSTRAINT [FK_dbo.PhoneNumber_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PhoneNumber_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PhoneNumber] CHECK CONSTRAINT [FK_dbo.PhoneNumber_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PhoneNumber_dbo.Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PhoneNumber] CHECK CONSTRAINT [FK_dbo.PhoneNumber_dbo.Person_PersonId]
GO
ALTER TABLE [dbo].[PluginMigration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PluginMigration_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PluginMigration] CHECK CONSTRAINT [FK_dbo.PluginMigration_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PluginMigration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PluginMigration_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PluginMigration] CHECK CONSTRAINT [FK_dbo.PluginMigration_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.DefinedValue_LanguageValueId] FOREIGN KEY([LanguageValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.DefinedValue_LanguageValueId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_ApprovedByPersonAliasId] FOREIGN KEY([ApprovedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_ApprovedByPersonAliasId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[PrayerRequest]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_RequestedByPersonAliasId] FOREIGN KEY([RequestedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[PrayerRequest] CHECK CONSTRAINT [FK_dbo.PrayerRequest_dbo.PersonAlias_RequestedByPersonAliasId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.FinancialScheduledTransaction_PaymentPlanFinancialScheduledTransactionId] FOREIGN KEY([PaymentPlanFinancialScheduledTransactionId])
REFERENCES [dbo].[FinancialScheduledTransaction] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.FinancialScheduledTransaction_PaymentPlanFinancialScheduledTransactionId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.Group_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.Group_GroupId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.RegistrationInstance_RegistrationInstanceId] FOREIGN KEY([RegistrationInstanceId])
REFERENCES [dbo].[RegistrationInstance] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.RegistrationInstance_RegistrationInstanceId]
GO
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Registration_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_dbo.Registration_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.DefinedValue_RegistrantRecordSourceValueId] FOREIGN KEY([RegistrantRecordSourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.DefinedValue_RegistrantRecordSourceValueId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.FinancialAccount_AccountId] FOREIGN KEY([AccountId])
REFERENCES [dbo].[FinancialAccount] ([Id])
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.FinancialAccount_AccountId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.PersonAlias_ContactPersonAliasId] FOREIGN KEY([ContactPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.PersonAlias_ContactPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RegistrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationInstance_dbo.WorkflowType_RegistrationWorkflowTypeId] FOREIGN KEY([RegistrationWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[RegistrationInstance] CHECK CONSTRAINT [FK_dbo.RegistrationInstance_dbo.WorkflowType_RegistrationWorkflowTypeId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.GroupMember_GroupMemberId] FOREIGN KEY([GroupMemberId])
REFERENCES [dbo].[GroupMember] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.GroupMember_GroupMemberId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.Registration_RegistrationId] FOREIGN KEY([RegistrationId])
REFERENCES [dbo].[Registration] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.Registration_RegistrationId]
GO
ALTER TABLE [dbo].[RegistrationRegistrant]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.SignatureDocument_SignatureDocumentId] FOREIGN KEY([SignatureDocumentId])
REFERENCES [dbo].[SignatureDocument] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrant] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrant_dbo.SignatureDocument_SignatureDocumentId]
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.RegistrationRegistrant_RegistrationRegistrantId] FOREIGN KEY([RegistrationRegistrantId])
REFERENCES [dbo].[RegistrationRegistrant] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.RegistrationRegistrant_RegistrationRegistrantId]
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.RegistrationTemplateFeeItem_RegistrationTemplateFeeItemId] FOREIGN KEY([RegistrationTemplateFeeItemId])
REFERENCES [dbo].[RegistrationTemplateFeeItem] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.RegistrationTemplateFeeItem_RegistrationTemplateFeeItemId]
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.RegistrationTemplateFee_RegistrationTemplateFeeId] FOREIGN KEY([RegistrationTemplateFeeId])
REFERENCES [dbo].[RegistrationTemplateFee] ([Id])
GO
ALTER TABLE [dbo].[RegistrationRegistrantFee] CHECK CONSTRAINT [FK_dbo.RegistrationRegistrantFee_dbo.RegistrationTemplateFee_RegistrationTemplateFeeId]
GO
ALTER TABLE [dbo].[RegistrationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationSession_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationSession] CHECK CONSTRAINT [FK_dbo.RegistrationSession_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationSession_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationSession] CHECK CONSTRAINT [FK_dbo.RegistrationSession_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationSession_dbo.RegistrationInstance_RegistrationInstanceId] FOREIGN KEY([RegistrationInstanceId])
REFERENCES [dbo].[RegistrationInstance] ([Id])
GO
ALTER TABLE [dbo].[RegistrationSession] CHECK CONSTRAINT [FK_dbo.RegistrationSession_dbo.RegistrationInstance_RegistrationInstanceId]
GO
ALTER TABLE [dbo].[RegistrationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationSession_dbo.Registration_RegistrationId] FOREIGN KEY([RegistrationId])
REFERENCES [dbo].[Registration] ([Id])
GO
ALTER TABLE [dbo].[RegistrationSession] CHECK CONSTRAINT [FK_dbo.RegistrationSession_dbo.Registration_RegistrationId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.DefinedValue_ConnectionStatusValueId] FOREIGN KEY([ConnectionStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.DefinedValue_ConnectionStatusValueId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.DefinedValue_RegistrantRecordSourceValueId] FOREIGN KEY([RegistrantRecordSourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.DefinedValue_RegistrantRecordSourceValueId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.FinancialGateway_FinancialGatewayId] FOREIGN KEY([FinancialGatewayId])
REFERENCES [dbo].[FinancialGateway] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.FinancialGateway_FinancialGatewayId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.SignatureDocumentType_RequiredSignatureDocumentTypeId] FOREIGN KEY([RequiredSignatureDocumentTemplateId])
REFERENCES [dbo].[SignatureDocumentTemplate] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.SignatureDocumentType_RequiredSignatureDocumentTypeId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.WorkflowType_RegistrantWorkflowTypeId] FOREIGN KEY([RegistrantWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.WorkflowType_RegistrantWorkflowTypeId]
GO
ALTER TABLE [dbo].[RegistrationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.WorkflowType_RegistrationWorkflowTypeId] FOREIGN KEY([RegistrationWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplate] CHECK CONSTRAINT [FK_dbo.RegistrationTemplate_dbo.WorkflowType_RegistrationWorkflowTypeId]
GO
ALTER TABLE [dbo].[RegistrationTemplateDiscount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateDiscount_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateDiscount] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateDiscount_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateDiscount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateDiscount_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateDiscount] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateDiscount_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateDiscount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateDiscount_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationTemplateDiscount] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateDiscount_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFee_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFee] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFee_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFee_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFee] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFee_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFee]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFee_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationTemplateFee] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFee_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFeeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFeeItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFeeItem] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFeeItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFeeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFeeItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFeeItem] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFeeItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFeeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFeeItem_dbo.RegistrationTemplateFee_RegistrationTemplateFeeId] FOREIGN KEY([RegistrationTemplateFeeId])
REFERENCES [dbo].[RegistrationTemplateFee] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationTemplateFeeItem] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFeeItem_dbo.RegistrationTemplateFee_RegistrationTemplateFeeId]
GO
ALTER TABLE [dbo].[RegistrationTemplateForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateForm_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateForm] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateForm_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateForm_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateForm] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateForm_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateForm_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationTemplateForm] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateForm_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.Attribute_AttributeId] FOREIGN KEY([AttributeId])
REFERENCES [dbo].[Attribute] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.Attribute_AttributeId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.RegistrationTemplateForm_RegistrationTemplateFormId] FOREIGN KEY([RegistrationTemplateFormId])
REFERENCES [dbo].[RegistrationTemplateForm] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationTemplateFormField] CHECK CONSTRAINT [FK_dbo.RegistrationTemplateFormField_dbo.RegistrationTemplateForm_RegistrationTemplateFormId]
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.GroupType_GroupTypeId] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[GroupType] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement] CHECK CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.GroupType_GroupTypeId]
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement] CHECK CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement] CHECK CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.RegistrationTemplate_RegistrationTemplateId] FOREIGN KEY([RegistrationTemplateId])
REFERENCES [dbo].[RegistrationTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RegistrationTemplatePlacement] CHECK CONSTRAINT [FK_dbo.RegistrationTemplatePlacement_dbo.RegistrationTemplate_RegistrationTemplateId]
GO
ALTER TABLE [dbo].[RelatedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RelatedEntity_dbo.EntityType_SourceEntityTypeId] FOREIGN KEY([SourceEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[RelatedEntity] CHECK CONSTRAINT [FK_dbo.RelatedEntity_dbo.EntityType_SourceEntityTypeId]
GO
ALTER TABLE [dbo].[RelatedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RelatedEntity_dbo.EntityType_TargetEntityTypeId] FOREIGN KEY([TargetEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[RelatedEntity] CHECK CONSTRAINT [FK_dbo.RelatedEntity_dbo.EntityType_TargetEntityTypeId]
GO
ALTER TABLE [dbo].[RelatedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RelatedEntity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RelatedEntity] CHECK CONSTRAINT [FK_dbo.RelatedEntity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RelatedEntity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RelatedEntity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RelatedEntity] CHECK CONSTRAINT [FK_dbo.RelatedEntity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Reminder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Reminder_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Reminder] CHECK CONSTRAINT [FK_dbo.Reminder_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Reminder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Reminder_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Reminder] CHECK CONSTRAINT [FK_dbo.Reminder_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Reminder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Reminder_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Reminder] CHECK CONSTRAINT [FK_dbo.Reminder_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Reminder]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Reminder_dbo.ReminderType_ReminderTypeId] FOREIGN KEY([ReminderTypeId])
REFERENCES [dbo].[ReminderType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Reminder] CHECK CONSTRAINT [FK_dbo.Reminder_dbo.ReminderType_ReminderTypeId]
GO
ALTER TABLE [dbo].[ReminderType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReminderType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[ReminderType] CHECK CONSTRAINT [FK_dbo.ReminderType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[ReminderType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReminderType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ReminderType] CHECK CONSTRAINT [FK_dbo.ReminderType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ReminderType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReminderType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ReminderType] CHECK CONSTRAINT [FK_dbo.ReminderType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.PersonAlias_AuthorizedPersonAliasId] FOREIGN KEY([AuthorizedPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession] CHECK CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.PersonAlias_AuthorizedPersonAliasId]
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession] CHECK CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession] CHECK CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[RemoteAuthenticationSession] CHECK CONSTRAINT [FK_dbo.RemoteAuthenticationSession_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Report_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[Report] CHECK CONSTRAINT [FK_dbo.Report_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Report_dbo.DataView_DataViewId] FOREIGN KEY([DataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[Report] CHECK CONSTRAINT [FK_dbo.Report_dbo.DataView_DataViewId]
GO
ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Report_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Report] CHECK CONSTRAINT [FK_dbo.Report_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Report_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Report] CHECK CONSTRAINT [FK_dbo.Report_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Report_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Report] CHECK CONSTRAINT [FK_dbo.Report_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ReportField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReportField_dbo.EntityType_DataSelectComponentEntityTypeId] FOREIGN KEY([DataSelectComponentEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportField] CHECK CONSTRAINT [FK_dbo.ReportField_dbo.EntityType_DataSelectComponentEntityTypeId]
GO
ALTER TABLE [dbo].[ReportField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReportField_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ReportField] CHECK CONSTRAINT [FK_dbo.ReportField_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ReportField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReportField_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ReportField] CHECK CONSTRAINT [FK_dbo.ReportField_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ReportField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReportField_dbo.Report_ReportId] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Report] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportField] CHECK CONSTRAINT [FK_dbo.ReportField_dbo.Report_ReportId]
GO
ALTER TABLE [dbo].[RequestFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RequestFilter_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RequestFilter] CHECK CONSTRAINT [FK_dbo.RequestFilter_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RequestFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RequestFilter_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RequestFilter] CHECK CONSTRAINT [FK_dbo.RequestFilter_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RequestFilter]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RequestFilter_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[RequestFilter] CHECK CONSTRAINT [FK_dbo.RequestFilter_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[RestAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RestAction_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RestAction] CHECK CONSTRAINT [FK_dbo.RestAction_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RestAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RestAction_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RestAction] CHECK CONSTRAINT [FK_dbo.RestAction_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[RestAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RestAction_dbo.RestController_ControllerId] FOREIGN KEY([ControllerId])
REFERENCES [dbo].[RestController] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RestAction] CHECK CONSTRAINT [FK_dbo.RestAction_dbo.RestController_ControllerId]
GO
ALTER TABLE [dbo].[RestController]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RestController_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RestController] CHECK CONSTRAINT [FK_dbo.RestController_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[RestController]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RestController_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[RestController] CHECK CONSTRAINT [FK_dbo.RestController_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Schedule_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_dbo.Schedule_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Schedule_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_dbo.Schedule_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Schedule_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_dbo.Schedule_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ScheduleCategoryExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ScheduleCategoryExclusion_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ScheduleCategoryExclusion] CHECK CONSTRAINT [FK_dbo.ScheduleCategoryExclusion_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[ScheduleCategoryExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ScheduleCategoryExclusion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ScheduleCategoryExclusion] CHECK CONSTRAINT [FK_dbo.ScheduleCategoryExclusion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ScheduleCategoryExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ScheduleCategoryExclusion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ScheduleCategoryExclusion] CHECK CONSTRAINT [FK_dbo.ScheduleCategoryExclusion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ScheduleDate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ScheduleDate_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ScheduleDate] CHECK CONSTRAINT [FK_dbo.ScheduleDate_dbo.Schedule_ScheduleId]
GO
ALTER TABLE [dbo].[ServiceJob]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceJob_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ServiceJob] CHECK CONSTRAINT [FK_dbo.ServiceJob_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ServiceJob]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceJob_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ServiceJob] CHECK CONSTRAINT [FK_dbo.ServiceJob_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ServiceJobHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceJobHistory_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ServiceJobHistory] CHECK CONSTRAINT [FK_dbo.ServiceJobHistory_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ServiceJobHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceJobHistory_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ServiceJobHistory] CHECK CONSTRAINT [FK_dbo.ServiceJobHistory_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[ServiceJobHistory]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceJobHistory_dbo.ServiceJob_ServiceJobId] FOREIGN KEY([ServiceJobId])
REFERENCES [dbo].[ServiceJob] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ServiceJobHistory] CHECK CONSTRAINT [FK_dbo.ServiceJobHistory_dbo.ServiceJob_ServiceJobId]
GO
ALTER TABLE [dbo].[ServiceLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceLog_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ServiceLog] CHECK CONSTRAINT [FK_dbo.ServiceLog_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[ServiceLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ServiceLog_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[ServiceLog] CHECK CONSTRAINT [FK_dbo.ServiceLog_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignalType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignalType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignalType] CHECK CONSTRAINT [FK_dbo.SignalType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignalType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignalType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignalType] CHECK CONSTRAINT [FK_dbo.SignalType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.BinaryFile_BinaryFileId] FOREIGN KEY([BinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.BinaryFile_BinaryFileId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_AppliesToPersonAliasId] FOREIGN KEY([AppliesToPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_AppliesToPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_AssignedToPersonAliasId] FOREIGN KEY([AssignedToPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_AssignedToPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_SignedByPersonAliasId] FOREIGN KEY([SignedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.PersonAlias_SignedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocument]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocument_dbo.SignatureDocumentType_SignatureDocumentTypeId] FOREIGN KEY([SignatureDocumentTemplateId])
REFERENCES [dbo].[SignatureDocumentTemplate] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SignatureDocument] CHECK CONSTRAINT [FK_dbo.SignatureDocument_dbo.SignatureDocumentType_SignatureDocumentTypeId]
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocumentTemplate_dbo.SystemCommunication_CompletionSystemCommunicationId] FOREIGN KEY([CompletionSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate] CHECK CONSTRAINT [FK_dbo.SignatureDocumentTemplate_dbo.SystemCommunication_CompletionSystemCommunicationId]
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocumentTemplate_dbo.SystemCommunication_InviteSystemCommunicationId] FOREIGN KEY([InviteSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate] CHECK CONSTRAINT [FK_dbo.SignatureDocumentTemplate_dbo.SystemCommunication_InviteSystemCommunicationId]
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.BinaryFileType_BinaryFileTypeId] FOREIGN KEY([BinaryFileTypeId])
REFERENCES [dbo].[BinaryFileType] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate] CHECK CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.BinaryFileType_BinaryFileTypeId]
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.EntityType_ProviderEntityTypeId] FOREIGN KEY([ProviderEntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate] CHECK CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.EntityType_ProviderEntityTypeId]
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate] CHECK CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SignatureDocumentTemplate] CHECK CONSTRAINT [FK_dbo.SignatureDocumentType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_ConfigurationMobilePhoneBinaryFileId] FOREIGN KEY([ConfigurationMobilePhoneBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_ConfigurationMobilePhoneBinaryFileId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_ConfigurationMobileTabletBinaryFileId] FOREIGN KEY([ConfigurationMobileTabletBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_ConfigurationMobileTabletBinaryFileId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_FavIconBinaryFileId] FOREIGN KEY([FavIconBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_FavIconBinaryFileId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_SiteLogoBinaryFileId] FOREIGN KEY([SiteLogoBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_SiteLogoBinaryFileId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_ThumbnailBinaryFileId] FOREIGN KEY([ThumbnailBinaryFileId])
REFERENCES [dbo].[BinaryFile] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.BinaryFile_ThumbnailBinaryFileId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PageRoute_ChangePasswordPageRouteId] FOREIGN KEY([ChangePasswordPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PageRoute_ChangePasswordPageRouteId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PageRoute_CommunicationPageRouteId] FOREIGN KEY([CommunicationPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PageRoute_CommunicationPageRouteId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PageRoute_DefaultPageRouteId] FOREIGN KEY([DefaultPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PageRoute_DefaultPageRouteId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PageRoute_LoginPageRouteId] FOREIGN KEY([LoginPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PageRoute_LoginPageRouteId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PageRoute_PageNotFoundPageRouteId] FOREIGN KEY([PageNotFoundPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PageRoute_PageNotFoundPageRouteId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PageRoute_RegistrationPageRouteId] FOREIGN KEY([RegistrationPageRouteId])
REFERENCES [dbo].[PageRoute] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PageRoute_RegistrationPageRouteId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_ChangePasswordPageId] FOREIGN KEY([ChangePasswordPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_ChangePasswordPageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_CommunicationPageId] FOREIGN KEY([CommunicationPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_CommunicationPageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_DefaultPageId] FOREIGN KEY([DefaultPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_DefaultPageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_LoginPageId] FOREIGN KEY([LoginPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_LoginPageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_MobilePageId] FOREIGN KEY([MobilePageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_MobilePageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_PageNotFoundPageId] FOREIGN KEY([PageNotFoundPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_PageNotFoundPageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.Page_RegistrationPageId] FOREIGN KEY([RegistrationPageId])
REFERENCES [dbo].[Page] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.Page_RegistrationPageId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SiteDomain]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SiteDomain_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SiteDomain] CHECK CONSTRAINT [FK_dbo.SiteDomain_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SiteDomain]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SiteDomain_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SiteDomain] CHECK CONSTRAINT [FK_dbo.SiteDomain_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SiteDomain]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SiteDomain_dbo.Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SiteDomain] CHECK CONSTRAINT [FK_dbo.SiteDomain_dbo.Site_SiteId]
GO
ALTER TABLE [dbo].[SmsAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SmsAction_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SmsAction] CHECK CONSTRAINT [FK_dbo.SmsAction_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SmsAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SmsAction_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SmsAction] CHECK CONSTRAINT [FK_dbo.SmsAction_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SmsAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SmsAction_dbo.SmsPipeline_SmsPipelineId] FOREIGN KEY([SmsPipelineId])
REFERENCES [dbo].[SmsPipeline] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SmsAction] CHECK CONSTRAINT [FK_dbo.SmsAction_dbo.SmsPipeline_SmsPipelineId]
GO
ALTER TABLE [dbo].[SmsPipeline]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SmsPipeline_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SmsPipeline] CHECK CONSTRAINT [FK_dbo.SmsPipeline_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SmsPipeline]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SmsPipeline_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SmsPipeline] CHECK CONSTRAINT [FK_dbo.SmsPipeline_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Snippet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Snippet_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[Snippet] CHECK CONSTRAINT [FK_dbo.Snippet_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[Snippet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Snippet_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Snippet] CHECK CONSTRAINT [FK_dbo.Snippet_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Snippet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Snippet_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Snippet] CHECK CONSTRAINT [FK_dbo.Snippet_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Snippet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Snippet_dbo.PersonAlias_OwnerPersonAliasId] FOREIGN KEY([OwnerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Snippet] CHECK CONSTRAINT [FK_dbo.Snippet_dbo.PersonAlias_OwnerPersonAliasId]
GO
ALTER TABLE [dbo].[Snippet]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Snippet_dbo.SnippetType_SnippetTypeId] FOREIGN KEY([SnippetTypeId])
REFERENCES [dbo].[SnippetType] ([Id])
GO
ALTER TABLE [dbo].[Snippet] CHECK CONSTRAINT [FK_dbo.Snippet_dbo.SnippetType_SnippetTypeId]
GO
ALTER TABLE [dbo].[SnippetType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SnippetType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SnippetType] CHECK CONSTRAINT [FK_dbo.SnippetType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SnippetType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SnippetType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SnippetType] CHECK CONSTRAINT [FK_dbo.SnippetType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.StepProgramCompletion_StepProgramCompletionId] FOREIGN KEY([StepProgramCompletionId])
REFERENCES [dbo].[StepProgramCompletion] ([Id])
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.StepProgramCompletion_StepProgramCompletionId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.StepStatus_StepStatusId] FOREIGN KEY([StepStatusId])
REFERENCES [dbo].[StepStatus] ([Id])
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.StepStatus_StepStatusId]
GO
ALTER TABLE [dbo].[Step]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Step_dbo.StepType_StepTypeId] FOREIGN KEY([StepTypeId])
REFERENCES [dbo].[StepType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Step] CHECK CONSTRAINT [FK_dbo.Step_dbo.StepType_StepTypeId]
GO
ALTER TABLE [dbo].[StepProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgram_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[StepProgram] CHECK CONSTRAINT [FK_dbo.StepProgram_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[StepProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgram_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepProgram] CHECK CONSTRAINT [FK_dbo.StepProgram_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepProgram]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgram_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepProgram] CHECK CONSTRAINT [FK_dbo.StepProgram_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[StepProgramCompletion] CHECK CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[StepProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepProgramCompletion] CHECK CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepProgramCompletion] CHECK CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepProgramCompletion] CHECK CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[StepProgramCompletion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.StepProgram_StepProgramId] FOREIGN KEY([StepProgramId])
REFERENCES [dbo].[StepProgram] ([Id])
GO
ALTER TABLE [dbo].[StepProgramCompletion] CHECK CONSTRAINT [FK_dbo.StepProgramCompletion_dbo.StepProgram_StepProgramId]
GO
ALTER TABLE [dbo].[StepStatus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepStatus_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepStatus] CHECK CONSTRAINT [FK_dbo.StepStatus_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepStatus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepStatus_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepStatus] CHECK CONSTRAINT [FK_dbo.StepStatus_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepStatus]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepStatus_dbo.StepProgram_StepProgramId] FOREIGN KEY([StepProgramId])
REFERENCES [dbo].[StepProgram] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StepStatus] CHECK CONSTRAINT [FK_dbo.StepStatus_dbo.StepProgram_StepProgramId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.DataView_AudienceDataViewId] FOREIGN KEY([AudienceDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.DataView_AudienceDataViewId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.DataView_AutoCompleteDataViewId] FOREIGN KEY([AutoCompleteDataViewId])
REFERENCES [dbo].[DataView] ([Id])
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.DataView_AutoCompleteDataViewId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.DefinedValue_OrganizationalObjectiveValueId] FOREIGN KEY([OrganizationalObjectiveValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.DefinedValue_OrganizationalObjectiveValueId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.MergeTemplate_MergeTemplateId] FOREIGN KEY([MergeTemplateId])
REFERENCES [dbo].[MergeTemplate] ([Id])
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.MergeTemplate_MergeTemplateId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepType_dbo.StepProgram_StepProgramId] FOREIGN KEY([StepProgramId])
REFERENCES [dbo].[StepProgram] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StepType] CHECK CONSTRAINT [FK_dbo.StepType_dbo.StepProgram_StepProgramId]
GO
ALTER TABLE [dbo].[StepTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.StepType_PrerequisiteStepTypeId] FOREIGN KEY([PrerequisiteStepTypeId])
REFERENCES [dbo].[StepType] ([Id])
GO
ALTER TABLE [dbo].[StepTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.StepType_PrerequisiteStepTypeId]
GO
ALTER TABLE [dbo].[StepTypePrerequisite]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.StepType_StepTypeId] FOREIGN KEY([StepTypeId])
REFERENCES [dbo].[StepType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StepTypePrerequisite] CHECK CONSTRAINT [FK_dbo.StepTypePrerequisite_dbo.StepType_StepTypeId]
GO
ALTER TABLE [dbo].[StepWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflow_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflow] CHECK CONSTRAINT [FK_dbo.StepWorkflow_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflow] CHECK CONSTRAINT [FK_dbo.StepWorkflow_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflow_dbo.StepWorkflowTrigger_StepWorkflowTriggerId] FOREIGN KEY([StepWorkflowTriggerId])
REFERENCES [dbo].[StepWorkflowTrigger] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StepWorkflow] CHECK CONSTRAINT [FK_dbo.StepWorkflow_dbo.StepWorkflowTrigger_StepWorkflowTriggerId]
GO
ALTER TABLE [dbo].[StepWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflow_dbo.Step_StepId] FOREIGN KEY([StepId])
REFERENCES [dbo].[Step] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StepWorkflow] CHECK CONSTRAINT [FK_dbo.StepWorkflow_dbo.Step_StepId]
GO
ALTER TABLE [dbo].[StepWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflow_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StepWorkflow] CHECK CONSTRAINT [FK_dbo.StepWorkflow_dbo.Workflow_WorkflowId]
GO
ALTER TABLE [dbo].[StepWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StepWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.StepProgram_StepProgramId] FOREIGN KEY([StepProgramId])
REFERENCES [dbo].[StepProgram] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.StepProgram_StepProgramId]
GO
ALTER TABLE [dbo].[StepWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.StepType_StepTypeId] FOREIGN KEY([StepTypeId])
REFERENCES [dbo].[StepType] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.StepType_StepTypeId]
GO
ALTER TABLE [dbo].[StepWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[StepWorkflowTrigger] CHECK CONSTRAINT [FK_dbo.StepWorkflowTrigger_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[Streak]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[Streak] CHECK CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[Streak]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Streak] CHECK CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Streak]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Streak] CHECK CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Streak]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Streak] CHECK CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.PersonAlias_PersonAliasId]
GO
ALTER TABLE [dbo].[Streak]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.Sequence_SequenceId] FOREIGN KEY([StreakTypeId])
REFERENCES [dbo].[StreakType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Streak] CHECK CONSTRAINT [FK_dbo.SequenceEnrollment_dbo.Sequence_SequenceId]
GO
ALTER TABLE [dbo].[StreakType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Sequence_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StreakType] CHECK CONSTRAINT [FK_dbo.Sequence_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StreakType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Sequence_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StreakType] CHECK CONSTRAINT [FK_dbo.Sequence_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StreakTypeExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.Location_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO
ALTER TABLE [dbo].[StreakTypeExclusion] CHECK CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.Location_LocationId]
GO
ALTER TABLE [dbo].[StreakTypeExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StreakTypeExclusion] CHECK CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[StreakTypeExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[StreakTypeExclusion] CHECK CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[StreakTypeExclusion]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.Sequence_SequenceId] FOREIGN KEY([StreakTypeId])
REFERENCES [dbo].[StreakType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StreakTypeExclusion] CHECK CONSTRAINT [FK_dbo.SequenceOccurrenceExclusion_dbo.Sequence_SequenceId]
GO
ALTER TABLE [dbo].[SystemCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemCommunication_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[SystemCommunication] CHECK CONSTRAINT [FK_dbo.SystemCommunication_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[SystemCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemCommunication_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemCommunication] CHECK CONSTRAINT [FK_dbo.SystemCommunication_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SystemCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemCommunication_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemCommunication] CHECK CONSTRAINT [FK_dbo.SystemCommunication_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SystemCommunication]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemCommunication_dbo.SystemPhoneNumber_SmsFromSystemPhoneNumberId] FOREIGN KEY([SmsFromSystemPhoneNumberId])
REFERENCES [dbo].[SystemPhoneNumber] ([Id])
GO
ALTER TABLE [dbo].[SystemCommunication] CHECK CONSTRAINT [FK_dbo.SystemCommunication_dbo.SystemPhoneNumber_SmsFromSystemPhoneNumberId]
GO
ALTER TABLE [dbo].[SystemEmail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemEmail_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[SystemEmail] CHECK CONSTRAINT [FK_dbo.SystemEmail_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[SystemEmail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemEmail_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemEmail] CHECK CONSTRAINT [FK_dbo.SystemEmail_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SystemEmail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemEmail_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemEmail] CHECK CONSTRAINT [FK_dbo.SystemEmail_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SystemPhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.Group_SmsNotificationGroupId] FOREIGN KEY([SmsNotificationGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[SystemPhoneNumber] CHECK CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.Group_SmsNotificationGroupId]
GO
ALTER TABLE [dbo].[SystemPhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.PersonAlias_AssignedToPersonAliasId] FOREIGN KEY([AssignedToPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemPhoneNumber] CHECK CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.PersonAlias_AssignedToPersonAliasId]
GO
ALTER TABLE [dbo].[SystemPhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemPhoneNumber] CHECK CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[SystemPhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[SystemPhoneNumber] CHECK CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[SystemPhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.Site_MobileApplicationSiteId] FOREIGN KEY([MobileApplicationSiteId])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[SystemPhoneNumber] CHECK CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.Site_MobileApplicationSiteId]
GO
ALTER TABLE [dbo].[SystemPhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.WorkflowType_SmsReceivedWorkflowTypeId] FOREIGN KEY([SmsReceivedWorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
GO
ALTER TABLE [dbo].[SystemPhoneNumber] CHECK CONSTRAINT [FK_dbo.SystemPhoneNumber_dbo.WorkflowType_SmsReceivedWorkflowTypeId]
GO
ALTER TABLE [dbo].[Tag]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Tag_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[Tag] CHECK CONSTRAINT [FK_dbo.Tag_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[Tag]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Tag_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[Tag] CHECK CONSTRAINT [FK_dbo.Tag_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[Tag]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Tag_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Tag] CHECK CONSTRAINT [FK_dbo.Tag_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Tag]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Tag_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Tag] CHECK CONSTRAINT [FK_dbo.Tag_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Tag]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Tag_dbo.PersonAlias_OwnerPersonAliasId] FOREIGN KEY([OwnerPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Tag] CHECK CONSTRAINT [FK_dbo.Tag_dbo.PersonAlias_OwnerPersonAliasId]
GO
ALTER TABLE [dbo].[TaggedItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TaggedItem_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[TaggedItem] CHECK CONSTRAINT [FK_dbo.TaggedItem_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[TaggedItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TaggedItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[TaggedItem] CHECK CONSTRAINT [FK_dbo.TaggedItem_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[TaggedItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TaggedItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[TaggedItem] CHECK CONSTRAINT [FK_dbo.TaggedItem_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[TaggedItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TaggedItem_dbo.Tag_TagId] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tag] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TaggedItem] CHECK CONSTRAINT [FK_dbo.TaggedItem_dbo.Tag_TagId]
GO
ALTER TABLE [dbo].[Theme]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Theme_dbo.DefinedValue_PurposeValueId] FOREIGN KEY([PurposeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[Theme] CHECK CONSTRAINT [FK_dbo.Theme_dbo.DefinedValue_PurposeValueId]
GO
ALTER TABLE [dbo].[Theme]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Theme_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Theme] CHECK CONSTRAINT [FK_dbo.Theme_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Theme]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Theme_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Theme] CHECK CONSTRAINT [FK_dbo.Theme_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[UserLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserLogin_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[UserLogin] CHECK CONSTRAINT [FK_dbo.UserLogin_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[UserLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserLogin_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[UserLogin] CHECK CONSTRAINT [FK_dbo.UserLogin_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[UserLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserLogin_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[UserLogin] CHECK CONSTRAINT [FK_dbo.UserLogin_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[UserLogin]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserLogin_dbo.Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserLogin] CHECK CONSTRAINT [FK_dbo.UserLogin_dbo.Person_PersonId]
GO
ALTER TABLE [dbo].[WebFarmNode]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNode_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNode] CHECK CONSTRAINT [FK_dbo.WebFarmNode_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WebFarmNode]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNode_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNode] CHECK CONSTRAINT [FK_dbo.WebFarmNode_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WebFarmNodeLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNodeLog] CHECK CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WebFarmNodeLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNodeLog] CHECK CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WebFarmNodeLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.WebFarmNode_WebFarmNodeId] FOREIGN KEY([WebFarmNodeId])
REFERENCES [dbo].[WebFarmNode] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WebFarmNodeLog] CHECK CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.WebFarmNode_WebFarmNodeId]
GO
ALTER TABLE [dbo].[WebFarmNodeLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.WebFarmNode_WriterWebFarmNodeId] FOREIGN KEY([WriterWebFarmNodeId])
REFERENCES [dbo].[WebFarmNode] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNodeLog] CHECK CONSTRAINT [FK_dbo.WebFarmNodeLog_dbo.WebFarmNode_WriterWebFarmNodeId]
GO
ALTER TABLE [dbo].[WebFarmNodeMetric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeMetric_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNodeMetric] CHECK CONSTRAINT [FK_dbo.WebFarmNodeMetric_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WebFarmNodeMetric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeMetric_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WebFarmNodeMetric] CHECK CONSTRAINT [FK_dbo.WebFarmNodeMetric_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WebFarmNodeMetric]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WebFarmNodeMetric_dbo.WebFarmNode_WebFarmNodeId] FOREIGN KEY([WebFarmNodeId])
REFERENCES [dbo].[WebFarmNode] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WebFarmNodeMetric] CHECK CONSTRAINT [FK_dbo.WebFarmNodeMetric_dbo.WebFarmNode_WebFarmNodeId]
GO
ALTER TABLE [dbo].[Workflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Workflow_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
REFERENCES [dbo].[Campus] ([Id])
GO
ALTER TABLE [dbo].[Workflow] CHECK CONSTRAINT [FK_dbo.Workflow_dbo.Campus_CampusId]
GO
ALTER TABLE [dbo].[Workflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Workflow_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Workflow] CHECK CONSTRAINT [FK_dbo.Workflow_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[Workflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Workflow_dbo.PersonAlias_InitiatorPersonAliasId] FOREIGN KEY([InitiatorPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Workflow] CHECK CONSTRAINT [FK_dbo.Workflow_dbo.PersonAlias_InitiatorPersonAliasId]
GO
ALTER TABLE [dbo].[Workflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Workflow_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[Workflow] CHECK CONSTRAINT [FK_dbo.Workflow_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[Workflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Workflow_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Workflow] CHECK CONSTRAINT [FK_dbo.Workflow_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[WorkflowAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowAction_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowAction] CHECK CONSTRAINT [FK_dbo.WorkflowAction_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowAction_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowAction] CHECK CONSTRAINT [FK_dbo.WorkflowAction_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowAction_dbo.WorkflowActionType_ActionTypeId] FOREIGN KEY([ActionTypeId])
REFERENCES [dbo].[WorkflowActionType] ([Id])
GO
ALTER TABLE [dbo].[WorkflowAction] CHECK CONSTRAINT [FK_dbo.WorkflowAction_dbo.WorkflowActionType_ActionTypeId]
GO
ALTER TABLE [dbo].[WorkflowAction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowAction_dbo.WorkflowActivity_ActivityId] FOREIGN KEY([ActivityId])
REFERENCES [dbo].[WorkflowActivity] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowAction] CHECK CONSTRAINT [FK_dbo.WorkflowAction_dbo.WorkflowActivity_ActivityId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryCampusStatusValueId] FOREIGN KEY([PersonEntryCampusStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryCampusStatusValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryCampusTypeValueId] FOREIGN KEY([PersonEntryCampusTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryCampusTypeValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryConnectionStatusValueId] FOREIGN KEY([PersonEntryConnectionStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryConnectionStatusValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryGroupLocationTypeValueId] FOREIGN KEY([PersonEntryGroupLocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryGroupLocationTypeValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryRecordSourceValueId] FOREIGN KEY([PersonEntryRecordSourceValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryRecordSourceValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryRecordStatusValueId] FOREIGN KEY([PersonEntryRecordStatusValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntryRecordStatusValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntrySectionTypeValueId] FOREIGN KEY([PersonEntrySectionTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.DefinedValue_PersonEntrySectionTypeValueId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionForm]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.SystemCommunication_NotificationSystemCommunicationId] FOREIGN KEY([NotificationSystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionForm] CHECK CONSTRAINT [FK_dbo.WorkflowActionForm_dbo.SystemCommunication_NotificationSystemCommunicationId]
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.Attribute_AttributeId] FOREIGN KEY([AttributeId])
REFERENCES [dbo].[Attribute] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.Attribute_AttributeId]
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.WorkflowActionFormSection_ActionFormSectionId] FOREIGN KEY([ActionFormSectionId])
REFERENCES [dbo].[WorkflowActionFormSection] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.WorkflowActionFormSection_ActionFormSectionId]
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.WorkflowActionForm_WorkflowActionFormId] FOREIGN KEY([WorkflowActionFormId])
REFERENCES [dbo].[WorkflowActionForm] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActionFormAttribute] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormAttribute_dbo.WorkflowActionForm_WorkflowActionFormId]
GO
ALTER TABLE [dbo].[WorkflowActionFormSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.DefinedValue_SectionTypeValueId] FOREIGN KEY([SectionTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionFormSection] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.DefinedValue_SectionTypeValueId]
GO
ALTER TABLE [dbo].[WorkflowActionFormSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionFormSection] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionFormSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionFormSection] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionFormSection]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.WorkflowActionForm_WorkflowActionFormId] FOREIGN KEY([WorkflowActionFormId])
REFERENCES [dbo].[WorkflowActionForm] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActionFormSection] CHECK CONSTRAINT [FK_dbo.WorkflowActionFormSection_dbo.WorkflowActionForm_WorkflowActionFormId]
GO
ALTER TABLE [dbo].[WorkflowActionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionType_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActionType] CHECK CONSTRAINT [FK_dbo.WorkflowActionType_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[WorkflowActionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionType] CHECK CONSTRAINT [FK_dbo.WorkflowActionType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionType] CHECK CONSTRAINT [FK_dbo.WorkflowActionType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionType_dbo.WorkflowActionForm_WorkflowFormId] FOREIGN KEY([WorkflowFormId])
REFERENCES [dbo].[WorkflowActionForm] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActionType] CHECK CONSTRAINT [FK_dbo.WorkflowActionType_dbo.WorkflowActionForm_WorkflowFormId]
GO
ALTER TABLE [dbo].[WorkflowActionType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActionType_dbo.WorkflowActivityType_ActivityTypeId] FOREIGN KEY([ActivityTypeId])
REFERENCES [dbo].[WorkflowActivityType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActionType] CHECK CONSTRAINT [FK_dbo.WorkflowActionType_dbo.WorkflowActivityType_ActivityTypeId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.Group_AssignedGroupId] FOREIGN KEY([AssignedGroupId])
REFERENCES [dbo].[Group] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.Group_AssignedGroupId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.PersonAlias_AssignedPersonAliasId] FOREIGN KEY([AssignedPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.PersonAlias_AssignedPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.WorkflowActivityType_ActivityTypeId] FOREIGN KEY([ActivityTypeId])
REFERENCES [dbo].[WorkflowActivityType] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.WorkflowActivityType_ActivityTypeId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.WorkflowActivity_ActivatedByActivityId] FOREIGN KEY([ActivatedByActivityId])
REFERENCES [dbo].[WorkflowActivity] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.WorkflowActivity_ActivatedByActivityId]
GO
ALTER TABLE [dbo].[WorkflowActivity]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivity_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActivity] CHECK CONSTRAINT [FK_dbo.WorkflowActivity_dbo.Workflow_WorkflowId]
GO
ALTER TABLE [dbo].[WorkflowActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivityType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivityType] CHECK CONSTRAINT [FK_dbo.WorkflowActivityType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivityType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowActivityType] CHECK CONSTRAINT [FK_dbo.WorkflowActivityType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowActivityType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowActivityType_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowActivityType] CHECK CONSTRAINT [FK_dbo.WorkflowActivityType_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[WorkflowFormBuilderTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowFormBuilderTemplate_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowFormBuilderTemplate] CHECK CONSTRAINT [FK_dbo.WorkflowFormBuilderTemplate_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowFormBuilderTemplate]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowFormBuilderTemplate_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowFormBuilderTemplate] CHECK CONSTRAINT [FK_dbo.WorkflowFormBuilderTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowLog_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
REFERENCES [dbo].[Workflow] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowLog] CHECK CONSTRAINT [FK_dbo.WorkflowLog_dbo.Workflow_WorkflowId]
GO
ALTER TABLE [dbo].[WorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowTrigger_dbo.EntityType_EntityTypeId] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityType] ([Id])
GO
ALTER TABLE [dbo].[WorkflowTrigger] CHECK CONSTRAINT [FK_dbo.WorkflowTrigger_dbo.EntityType_EntityTypeId]
GO
ALTER TABLE [dbo].[WorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowTrigger_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
REFERENCES [dbo].[WorkflowType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkflowTrigger] CHECK CONSTRAINT [FK_dbo.WorkflowTrigger_dbo.WorkflowType_WorkflowTypeId]
GO
ALTER TABLE [dbo].[WorkflowType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowType_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[WorkflowType] CHECK CONSTRAINT [FK_dbo.WorkflowType_dbo.Category_CategoryId]
GO
ALTER TABLE [dbo].[WorkflowType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowType_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowType] CHECK CONSTRAINT [FK_dbo.WorkflowType_dbo.PersonAlias_CreatedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowType_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])
GO
ALTER TABLE [dbo].[WorkflowType] CHECK CONSTRAINT [FK_dbo.WorkflowType_dbo.PersonAlias_ModifiedByPersonAliasId]
GO
ALTER TABLE [dbo].[WorkflowType]  WITH CHECK ADD  CONSTRAINT [FK_dbo.WorkflowType_dbo.WorkflowFormBuilderTemplate_FormBuilderTemplateId] FOREIGN KEY([FormBuilderTemplateId])
REFERENCES [dbo].[WorkflowFormBuilderTemplate] ([Id])
GO
ALTER TABLE [dbo].[WorkflowType] CHECK CONSTRAINT [FK_dbo.WorkflowType_dbo.WorkflowFormBuilderTemplate_FormBuilderTemplateId]
GO
