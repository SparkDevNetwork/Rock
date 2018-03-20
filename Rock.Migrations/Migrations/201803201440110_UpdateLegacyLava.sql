-- Update AttributeValues
----------------------------------------------------------------------------------------------------------------

-- Fix NameValue AttributeValue for Request Set Name for the External Inquiryworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ Workflow | Attribute:''FirstName'' }} {{ Workflow | Attribute:''LastName'' }} ( {{ Workflow | Attribute:''Topic'' }} )'
WHERE AttributeId in (select Id from Attribute where [Guid] = '93852244-A667-4749-961A-D47F88675BE4')
	AND [Value] = '{{ Workflow.FirstName }} {{ Workflow.LastName }} ( {{ Workflow.Topic }} )';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Assign Worker Notify Worker for the Facilities Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following Facilities Request has been submitted by {{ Workflow | Attribute:''Requester'' }}:</p>

<h4><a href=''{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following Facilities Request has been submitted by {{ Workflow.Requester }}:</p>

<h4><a href=''{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow.Details }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Assign Worker Notify Worker for the IT Supportworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following IT Support Request has been submitted by {{ Workflow | Attribute:''Requester'' }}:</p>

<h4><a href=''{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following IT Support Request has been submitted by {{ Workflow.Requester }}:</p>

<h4><a href=''{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow.Details }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Complete Notify Originator for the Person Data Errorworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>{{ Workflow | Attribute:''ReportedBy'' }},</p>
<p>The data error that you reported for {{ Workflow.Name }} has been completed.<p>

<h4>Details:</h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

{% if Workflow | Attribute:''Resolution'' != Empty %}

    <h4>Resolution:</h4>
    <p>{{ Workflow | Attribute:''Resolution'' }}</p>

{% endif %}

{{ ''Global'' | Attribute:''EmailFooter'' }}

'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>{{ Workflow.ReportedBy }},</p>
<p>The data error that you reported for {{ Workflow.Name }} has been completed.<p>

<h4>Details:</h4>
<p>{{ Workflow.Details }}</p>

{% if Workflow.Resolution != Empty %}

    <h4>Resolution:</h4>
    <p>{{ Workflow.Resolution }}</p>

{% endif %}

{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Complete Notify Requester for the Facilities Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following Facilities Request has been completed by {{ Workflow | Attribute:''Worker'' }}:</p>

<h4>{{ Workflow.Name }}</h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

<h4>Resolution</h4>
<p>{{ Workflow | Attribute:''Resolution'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following Facilities Request has been completed by {{ Workflow.Worker }}:</p>

<h4>{{ Workflow.Name }}</h4>
<p>{{ Workflow.Details }}</p>

<h4>Resolution</h4>
<p>{{ Workflow.Resolution }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Complete Notify Requester for the IT Supportworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following IT Support Request has been completed by {{ Workflow | Attribute:''Worker'' }}:</p>

<h4>{{ Workflow.Name }}</h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

<h4>Resolution</h4>
<p>{{ Workflow | Attribute:''Resolution'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>The following IT Support Request has been completed by {{ Workflow.Worker }}:</p>

<h4>{{ Workflow.Name }}</h4>
<p>{{ Workflow.Details }}</p>

<h4>Resolution</h4>
<p>{{ Workflow.Resolution }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix SQLQuery AttributeValue for Launch From Person Profile Check for Opt Out for the Photo Requestworkflow
UPDATE [AttributeValue]
SET [Value] = 'DECLARE @PersonAliasGuid uniqueidentifier = ''{{ Workflow | Attribute:''Person'',''RawValue'' }}''

SELECT  CASE
   WHEN EXISTS ( SELECT 1
      FROM [GroupMember] GM
      INNER JOIN [Group] G ON GM.[GroupId] = G.[Id] AND G.[Guid] = ''2108EF9C-10DC-4466-973D-D25AAB7818BE''
	  INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid
      WHERE GM.PersonId = PA.[PersonId]
      AND GM.GroupMemberStatus = 0 )
    THEN ''True''
    ELSE ''False''
    END'
WHERE AttributeId in (select Id from Attribute where [Guid] = 'F3B9908B-096F-460B-8320-122CF046D1F9')
	AND [Value] = 'DECLARE @PersonAliasGuid uniqueidentifier = ''{{ Workflow.Person_unformatted }}''

SELECT  CASE
   WHEN EXISTS ( SELECT 1
      FROM [GroupMember] GM
      INNER JOIN [Group] G ON GM.[GroupId] = G.[Id] AND G.[Guid] = ''2108EF9C-10DC-4466-973D-D25AAB7818BE''
	  INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid
      WHERE GM.PersonId = PA.[PersonId]
      AND GM.GroupMemberStatus = 0 )
    THEN ''True''
    ELSE ''False''
    END';
----------------------------------------------------------------------------------------------------------------

-- Fix Value AttributeValue for Launch From Person Profile Set Warning for the Photo Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '<div class="alert alert-warning">{{ Workflow | Attribute:''Person'' }} has previously opted out from photo requests.  Make sure you want to override this preference.</div>'
WHERE AttributeId in (select Id from Attribute where [Guid] = 'E5272B11-A2B8-49DC-860D-8D574E2BC15C')
	AND [Value] = '<div class="alert alert-warning">{{ Workflow.Person }} has previously opted out from photo requests.  Make sure you want to override this preference.</div>';
----------------------------------------------------------------------------------------------------------------

-- Fix Subject AttributeValue for Launch From Person Profile Send Email Action for the Photo Requestworkflow
UPDATE [AttributeValue]
SET [Value] = 'Photo Request from {{ Workflow | Attribute:''Sender'' }}' 
WHERE AttributeId in (select Id from Attribute where [Guid] = '5D9B13B6-CD96-4C7C-86FA-4512B9D28386')
	AND [Value] = 'Photo Request from {{ Workflow.Sender }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Launch From Person Profile Send Email Action for the Photo Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>{{ Person.NickName }},</p>

<p>{{ Workflow | Attribute:''CustomMessage'' | NewlineToBr }}</p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}PhotoRequest/Upload/{{ Person.UrlEncodedKey }}">Upload Photo </a></p>

<p>Your picture will remain confidential and will only be visible to staff and volunteers in a leadership position within {{ ''Global'' | Attribute:''OrganizationName'' }}.</p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}PhotoRequest/OptOut/{{ Person.UrlEncodedKey }}">I prefer not to receive future photo requests.</a></p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person.UrlEncodedKey }}">I&#39;m no longer involved with {{ ''Global'' | Attribute:''OrganizationName'' }}. Please remove me from all future communications.</a></p>

<p>-{{ Workflow | Attribute:''Sender'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ GlobalAttribute.EmailStyles }}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>{{ Person.NickName }},</p>

<p>{{ Workflow.CustomMessage | NewlineToBr }}</p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}PhotoRequest/Upload/{{ Person.UrlEncodedKey }}">Upload Photo </a></p>

<p>Your picture will remain confidential and will only be visible to staff and volunteers in a leadership position within {{ ''Global'' | Attribute:''OrganizationName'' }}.</p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}PhotoRequest/OptOut/{{ Person.UrlEncodedKey }}">I prefer not to receive future photo requests.</a></p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person.UrlEncodedKey }}">I&#39;m no longer involved with {{ ''Global'' | Attribute:''OrganizationName'' }}. Please remove me from all future communications.</a></p>

<p>-{{ Workflow.Sender }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}';

----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Approved Email Requester for the Position Approvalworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow | Attribute:''PositionTitle''}} position has been 
    approved by {{Workflow | Attribute:''Approver''}}. HR will be getting with you soon to arrange next steps
    for the posting process.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow.PositionTitle}} position has been 
    approved by {{Workflow.Approver}}. HR will be getting with you soon to arrange next steps
    for the posting process.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Approved Email HR for the Position Approvalworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow | Attribute:''PositionTitle''}} position has been 
    approved by {{Workflow | Attribute:''Approver''}}. Please follow up with {{Workflow | Attribute:''Requester''}} with
    next steps for posting the position.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}' 
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow.PositionTitle}} position has been 
    approved by {{Workflow.Approver}}. Please follow up with {{Workflow.Requester}} with
    next steps for posting the position.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Denied Email Requester for the Position Approvalworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow | Attribute:''PositionTitle''}} position was not approved by
    {{Workflow | Attribute:''Approver''}}. HR will be getting with you soon to arrange next steps
    for this process.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow.PositionTitle}} position was not approved by
    {{Workflow.Approver}}. HR will be getting with you soon to arrange next steps
    for this process.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Subject AttributeValue for Approved Email Requester for the Position Approvalworkflow
-- Fix Subject AttributeValue for Approved Email HR for the Position Approvalworkflow
-- Fix Subject AttributeValue for Denied Email Requester for the Position Approvalworkflow
-- Fix Subject AttributeValue for Denied Email HR for the Position Approvalworkflow
UPDATE [AttributeValue]
SET [Value] = 'UPDATE: {{Workflow | Attribute:''PositionTitle''}}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '5D9B13B6-CD96-4C7C-86FA-4512B9D28386')
	AND [Value] = 'UPDATE: {{Workflow.PositionTitle}}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Denied Email HR for the Position Approvalworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow | Attribute:''PositionTitle''}} position was not 
    approved by {{Workflow | Attribute:''Approver''}}. Please follow up with {{Workflow | Attribute:''Requester''}} with
    next steps for this process.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow.PositionTitle}} position was not 
    approved by {{Workflow.Approver}}. Please follow up with {{Workflow.Requester}} with
    next steps for this process.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix SQLQuery AttributeValue for Initial Request Set Warning for the Background Checkworkflow
UPDATE [AttributeValue]
SET [Value] = 'SELECT ISNULL( (
    SELECT 
        CASE WHEN DATEADD(year, 1, AV.[ValueAsDateTime]) > GETDATE() THEN ''True'' ELSE ''False'' END
    FROM [AttributeValue] AV
        INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
        INNER JOIN [PersonAlias] P ON P.[PersonId] = AV.[EntityId]
    WHERE AV.[ValueAsDateTime] IS NOT NULL
        AND A.[Guid] = ''{{ Workflow | Attribute:''DateAttribute'',''RawValue'' }}''
        AND P.[Guid] = ''{{ Workflow | Attribute:''Person'',''RawValue'' }}''
), ''False'')'
WHERE AttributeId in (select Id from Attribute where [Guid] = 'F3B9908B-096F-460B-8320-122CF046D1F9')
	AND [Value] = 'SELECT ISNULL( (
    SELECT 
        CASE WHEN DATEADD(year, 1, AV.[ValueAsDateTime]) > GETDATE() THEN ''True'' ELSE ''False'' END
    FROM [AttributeValue] AV
        INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
        INNER JOIN [PersonAlias] P ON P.[PersonId] = AV.[EntityId]
    WHERE AV.[ValueAsDateTime] IS NOT NULL
        AND A.[Guid] = ''{{ Workflow.DateAttribute_unformatted }}''
        AND P.[Guid] = ''{{ Workflow.Person_unformatted }}''
), ''False'')';
----------------------------------------------------------------------------------------------------------------

-- Fix Subject AttributeValue for Complete Request Notify Requester for the Background Checkworkflow
UPDATE [AttributeValue]
SET [Value] = 'Background Check for {{ Workflow | Attribute:''Person'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '5D9B13B6-CD96-4C7C-86FA-4512B9D28386')
	AND [Value] = 'Background Check for {{ Workflow.Person }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Complete Request Notify Requester for the Background Checkworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>{{ Person.FirstName }},</p>
<p>The background check for {{ Workflow | Attribute:''Person'' }} has been completed.</p>
<p>Result: {{ Workflow | Attribute:''ReportStatus'' | Upcase }}<p/>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D') 
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>{{ Person.FirstName }},</p>
<p>The background check for {{ Workflow.Person }} has been completed.</p>
<p>Result: {{ Workflow.ReportStatus | Upcase }}<p/>

{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix SQLQuery AttributeValue for Launch From Person Profile Check for Opt Out for the DISC Requestworkflow
UPDATE [AttributeValue]
SET [Value] = 'DECLARE @PersonAliasGuid uniqueidentifier = ''{{ Workflow | Attribute:''Person'',''RawValue'' }}''

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[EmailPreference] <> 0 )
    THEN ''True''
    ELSE ''False''
    END'
WHERE AttributeId in (select Id from Attribute where [Guid] = 'F3B9908B-096F-460B-8320-122CF046D1F9') 
AND [Value] = 'DECLARE @PersonAliasGuid uniqueidentifier = ''{{ Workflow.Person_unformatted }}''

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[EmailPreference] <> 0 )
    THEN ''True''
    ELSE ''False''
    END';
----------------------------------------------------------------------------------------------------------------

-- Fix Value AttributeValue for Launch From Person Profile Set Warning for the DISC Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '<div class="alert alert-warning">{{ Workflow | Attribute:''Person'' }} has previously opted out from email and bulk requests.  Make sure you want to override this preference.</div>'
WHERE AttributeId in (select Id from Attribute where [Guid] = 'E5272B11-A2B8-49DC-860D-8D574E2BC15C')
	AND [Value] = '<div class="alert alert-warning">{{ Workflow.Person }} has previously opted out from email and bulk requests.  Make sure you want to override this preference.</div>';
----------------------------------------------------------------------------------------------------------------

-- Fix Subject AttributeValue for Launch From Person Profile Send Email Action for the DISC Requestworkflow
UPDATE [AttributeValue]
SET [Value] = 'DISC Assessment Request from {{ Workflow | Attribute:''Sender'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '5D9B13B6-CD96-4C7C-86FA-4512B9D28386')
	AND [Value] = 'DISC Assessment Request from {{ Workflow.Sender }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Launch From Person Profile Send Email Action for the DISC Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>Hi {{ Person.NickName }}!</p>

<p>{{ Workflow | Attribute:''CustomMessage'' | NewlineToBr }}</p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}DISC/{{ Person.UrlEncodedKey }}">Take Personality Assessment</a></p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person.UrlEncodedKey }}">I&#39;m no longer involved with {{ ''Global'' | Attribute:''OrganizationName'' }}. Please remove me from all future communications.</a></p>

<p>- {{ Workflow | Attribute:''Sender'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ GlobalAttribute.EmailStyles }}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>Hi {{ Person.NickName }}!</p>

<p>{{ Workflow.CustomMessage | NewlineToBr }}</p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}DISC/{{ Person.UrlEncodedKey }}">Take Personality Assessment</a></p>

<p><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person.UrlEncodedKey }}">I&#39;m no longer involved with {{ ''Global'' | Attribute:''OrganizationName'' }}. Please remove me from all future communications.</a></p>

<p>- {{ Workflow.Sender }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix SQLQuery AttributeValue for Launch From Person Profile Set No Email Warning for the DISC Requestworkflow
UPDATE [AttributeValue]
SET [Value] = 'DECLARE @PersonAliasGuid uniqueidentifier = ''{{ Workflow | Attribute:''Person'',''RawValue'' }}''

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[IsEmailActive] <> 0 AND P.[Email] IS NOT NULL AND P.[Email] != '''' )
    THEN ''True''
    ELSE ''False''
    END'
WHERE AttributeId in (select Id from Attribute where [Guid] = 'F3B9908B-096F-460B-8320-122CF046D1F9')
	AND [Value] = 'DECLARE @PersonAliasGuid uniqueidentifier = ''{{ Workflow.Person_unformatted }}''

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[IsEmailActive] <> 0 AND P.[Email] IS NOT NULL AND P.[Email] != '''' )
    THEN ''True''
    ELSE ''False''
    END';
----------------------------------------------------------------------------------------------------------------

-- Fix Value AttributeValue for Launch From Person Profile Set No Email Warning Message for the DISC Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '<div class="alert alert-warning margin-t-sm">{{ Workflow | Attribute:''Person'' }} does not have an active email address. Please add an address to their record.</div>' 
WHERE AttributeId in (select Id from Attribute where [Guid] = 'E5272B11-A2B8-49DC-860D-8D574E2BC15C')
	AND [Value] = '<div class="alert alert-warning margin-t-sm">{{ Workflow.Person }} does not have an active email address. Please add an address to their record.</div>';
----------------------------------------------------------------------------------------------------------------

-- Fix Subject AttributeValue for Assign Worker Notify Worker for the Profile Change Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ Workflow | Attribute:''Requester'' }} Profile Change Request' 
WHERE AttributeId in (select Id from Attribute where [Guid] = '5D9B13B6-CD96-4C7C-86FA-4512B9D28386')
	AND [Value] = '{{ Workflow.Requester }} Profile Change Request';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Assign Worker Notify Worker for the Profile Change Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<h4>Profile Request Change for {{ Workflow | Attribute:''Requester'' }}</h4>
<p>The following change request has been submitted by {{ Workflow | Attribute:''Requester'' }} from the external website:</p>
<p>{{ Workflow | Attribute:''Request'' }}</p>

<p><a href=''{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></p>


{{ ''Global'' | Attribute:''EmailFooter'' }}

' 
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<h4>Profile Request Change for {{ Workflow.Requester }}</h4>
<p>The following change request has been submitted by {{ Workflow.Requester }} from the external website:</p>
<p>{{ Workflow.Request }}</p>

<p><a href=''{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></p>


{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Complete Notify Requester for the Profile Change Requestworkflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>Your profile change request has been completed by {{ Workflow | Attribute:''Worker'' }}:</p>

<strong>Request</strong>
<p>{{ Workflow | Attribute:''Request'' }}</p>

<strong>Resolution</strong>
<p>{{ Workflow | Attribute:''Resolution'' }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

' 
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>Your profile change request has been completed by {{ Workflow.Worker }}:</p>

<strong>Request</strong>
<p>{{ Workflow.Request }}</p>

<strong>Resolution</strong>
<p>{{ Workflow.Resolution }}</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}

';

----------------------------------------------------------------------------------------------------------------

-- Update Known GUID if the affected field has not already been changed.
----------------------------------------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------

-- Fix the Subject on the Giving Receipt System Email
UPDATE[SystemEmail]
SET [Subject] = 'Giving Receipt from {{ ''Global'' | Attribute:''OrganizationName''}}'
WHERE [Guid] = '7dbf229e-7dee-a684-4929-6c37312a0039'
	AND [subject] = 'Giving Receipt from {{ GlobalAttribute.OrganizationName}}';
----------------------------------------------------------------------------------------------------------------

-- Fix Capture Notes Header for the External Inquiry WorkflowActionForm
UPDATE [WorkflowActionForm]
SET [Header] = '<h4>{{ Workflow | Attribute:''Topic'' }} Inquiry from {{ Workflow | Attribute:''FirstName'' }} {{ Workflow | Attribute:''LastName'' }}</h4>
<p>The following inquiry has been submitted by a visitor to our website.</p>'
WHERE [Guid] = 'eb7034ba-6300-434f-832f-37983b9df154'
	AND [Header] = '<h4>{{ Workflow.Topic }} Inquiry from {{ Workflow.FirstName }} {{ Workflow.LastName }}</h4>
<p>The following inquiry has been submitted by a visitor to our website.</p>';
----------------------------------------------------------------------------------------------------------------

-- Fix Warning Message (custom message) Header on the photo requst WorkflowActionForm
UPDATE [WorkflowActionForm]
SET [Header] = '{{ Workflow | Attribute:''WarningMessage'' }}
'
WHERE [Guid] = '61dcbfb1-9ce8-4356-82d8-d69afe68a58c'
	AND [Header] = '{{ Workflow.WarningMessage }}
';
----------------------------------------------------------------------------------------------------------------

-- Fix HR Entry Header for the Position Approval Workflow
UPDATE [WorkflowActionForm] SET [Header] = '<h1>HR Entry</h1>
<p>
    {{ Workflow | Attribute:''Requestor'' }} has requested a new {{ Workflow | Attribute:''PositionTitle'' }} 
    position. Please complete the addition items below so that this position 
    can move on for approval.
</p>'
WHERE [Guid] = '17083abd-c595-4fe8-bcc6-4cf2cf739af3'
	AND [Header] = '<h1>HR Entry</h1>
<p>
    {{ Workflow.Requestor }} has requested a new {{ Workflow.PositionTitle }} 
    position. Please complete the addition items below so that this position 
    can move on for approval.
</p>';
----------------------------------------------------------------------------------------------------------------

-- Fix Approval Entry Header for the Position Approval Workflow
UPDATE [WorkflowActionForm]
SET [Header] = '<h1>Position Approval</h1>
<p>
    {{ Workflow | Attribute:''Requestor'' }} has requested a new {{ Workflow | Attribute:''PositionTitle'' }} position. 
    Please approve or deny this request.
</p>'
WHERE [Guid] = '59102a3b-24c8-4b4f-ba76-eeb692087adc'
	AND [Header] = '<h1>Position Approval</h1>
<p>
    {{ Workflow.Requestor }} has requested a new {{ Workflow.PositionTitle }} position. 
    Please approve or deny this request.
</p>';
----------------------------------------------------------------------------------------------------------------

-- Fix Initial Request Get Details Header for the Background Checkworkflow
UPDATE [WorkflowActionForm]
SET [Header] = '<h1>Background Request Details</h1>
<p>
    {{CurrentPerson.NickName}}, please complete the form below to start the background
    request process.
</p>
{% if Workflow | Attribute:''WarnOfRecent'' == ''Yes'' %}
    <div class=''alert alert-warning''>
        Notice: It''s been less than a year since this person''s last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />'
WHERE [Guid] = '328b74e5-6058-4c4e-9ef8-ec10985f18a8'
	AND [Header] = '<h1>Background Request Details</h1>
<p>
    {{CurrentPerson.NickName}}, please complete the form below to start the background
    request process.
</p>
{% if Workflow.WarnOfRecent == ''Yes'' %}
    <div class=''alert alert-warning''>
        Notice: It''s been less than a year since this person''s last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />';
----------------------------------------------------------------------------------------------------------------

-- Fix Launch From Person Profile Custom Message Header for the DISC Requestworkflow
UPDATE [WorkflowActionForm]
SET [Header] = 'Hi &#123;&#123; Person.NickName &#125;&#125;!

{{ Workflow | Attribute:''WarningMessage'' }}
{{ Workflow | Attribute:''NoEmailWarning'' }}'
WHERE [Guid] = '4afab342-d584-4b79-b038-a99c0c469d74'
	AND [Header] = 'Hi &#123;&#123; Person.NickName &#125;&#125;!

{{ Workflow.WarningMessage }}
{{ Workflow.NoEmailWarning }}';
