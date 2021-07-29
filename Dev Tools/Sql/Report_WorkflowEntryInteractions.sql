DECLARE @CHANNEL_WORKFLOW_LAUNCHES UNIQUEIDENTIFIER = 'DAA17190-7119-4901-B105-26C6B5E4CDB4'

SELECT ii.Id [InteractionId]
    , wf_workflowtype.Name [WorkflowTypeName]
    , wf.Id [WorkflowId]
    , wf.Name [WorkflowName]
    , wf_activitytype.Name [ActivityTypeName]
    , wf_actiontype.Name [ActionTypeName]
    , ii.Operation
    , ii.InteractionDateTime
    , ii.InteractionLength [TimeOnFormsSeconds]
    , wa_form.Id [WorkflowActionForm.Id]
FROM Interaction ii
JOIN InteractionComponent ii_component
    ON ii.InteractionComponentId = ii_component.Id
JOIN InteractionChannel ii_channel
    ON ii_component.InteractionChannelId = ii_channel.Id
JOIN Workflow wf
    ON ii.EntityId = wf.Id
JOIN WorkflowType wf_workflowtype
    ON wf.WorkflowTypeId = wf_workflowtype.Id
JOIN WorkflowActionType wf_actiontype
    ON ii.RelatedEntityId = wf_actiontype.Id
JOIN WorkflowActivity wf_activity
    ON wf_activity.WorkflowId = wf.Id
JOIN WorkflowAction wf_action
    ON wf_action.ActionTypeId = wf_actiontype.Id
        AND wf_action.ActionTypeId = wf_actiontype.Id
        AND wf_action.ActivityId = wf_activity.Id
JOIN WorkflowActivityType wf_activitytype
    ON wf_activitytype.Id = wf_actiontype.ActivityTypeId
LEFT JOIN WorkflowActionForm wa_form
    ON wf_actiontype.WorkflowFormId = wa_form.Id
WHERE ii_channel.Guid = @CHANNEL_WORKFLOW_LAUNCHES
ORDER BY ii.InteractionDateTime DESC
    , wf_activitytype.[Order]
    , wf_actiontype.[Order]
