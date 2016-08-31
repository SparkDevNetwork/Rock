SELECT *
FROM HtmlContent
WHERE Content LIKE '%GlobalAttribute.%'

SELECT *
FROM AttributeValue
WHERE Value LIKE '%GlobalAttribute.%'

SELECT *
FROM Attribute
WHERE DefaultValue LIKE '%GlobalAttribute.%'

SELECT *
FROM CommunicationTemplate
WHERE MediumDataJson LIKE '%GlobalAttribute.%'
    OR [Subject] LIKE '%GlobalAttribute.%'

SELECT *
FROM SystemEmail
WHERE Title LIKE '%GlobalAttribute.%'
    OR [From] LIKE '%GlobalAttribute.%'
    OR [To] LIKE '%GlobalAttribute.%'
    OR [Cc] LIKE '%GlobalAttribute.%'
    OR [Bcc] LIKE '%GlobalAttribute.%'
    OR [Subject] LIKE '%GlobalAttribute.%'
    OR [Body] LIKE '%GlobalAttribute.%'

SELECT *
FROM WorkflowActionFormAttribute
WHERE PreHtml LIKE '%GlobalAttribute.%'
    OR PostHtml LIKE '%GlobalAttribute.%'

SELECT *
FROM WorkflowActionForm
WHERE Header LIKE '%GlobalAttribute.%'
    OR Footer LIKE '%GlobalAttribute.%'

SELECT *
FROM RegistrationTemplateFormField
WHERE PreText LIKE '%GlobalAttribute.%'
    OR PostText LIKE '%GlobalAttribute.%'

SELECT *
FROM ReportField
WHERE Selection LIKE '%GlobalAttribute.%'
