select * from HtmlContent where Content like '%GlobalAttribute.%'



select * from AttributeValue where Value like '%GlobalAttribute.%'
select * from Attribute where DefaultValue like '%GlobalAttribute.%'




select * from CommunicationTemplate  where MediumDataJson like '%GlobalAttribute.%' or [Subject] like '%GlobalAttribute.%'


select * from SystemEmail  where 
Title like '%GlobalAttribute.%' 
or [From] like '%GlobalAttribute.%'
or [To] like '%GlobalAttribute.%'
or [Cc] like '%GlobalAttribute.%'
or [Bcc] like '%GlobalAttribute.%'
or [Subject] like '%GlobalAttribute.%'
or [Body] like '%GlobalAttribute.%'


select * from WorkflowActionFormAttribute where
PreHtml like '%GlobalAttribute.%' 
or PostHtml like '%GlobalAttribute.%'

select * from WorkflowActionForm where
Header like '%GlobalAttribute.%' 
or Footer like '%GlobalAttribute.%'

select * from RegistrationTemplateFormField where
PreText like '%GlobalAttribute.%' 
or PostText like '%GlobalAttribute.%'


select * from ReportField where
Selection like '%GlobalAttribute.%' 




