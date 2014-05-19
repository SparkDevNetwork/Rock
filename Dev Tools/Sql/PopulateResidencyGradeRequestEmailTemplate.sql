DELETE FROM [EmailTemplate] WHERE [Guid] = 'CCEDEC52-EC8A-41BF-9F78-C60418835257'

INSERT INTO [EmailTemplate] ([IsSystem], [PersonId], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) 
VALUES (1, NULL, 'Residency', 'Project Grade Request', 'rock@sparkdevnetwork.com', '', '', '', 'Project Grade Request', 
'{{ EmailHeader }}

{{ Facilitator.FirstName }},<br/><br/>

{{Resident.FullName}} requests that you <a href=''{{ GradeDetailPageUrl }}''>grade</a> {{ Project.Name }} - {{ Project.Description}} 
<br/>
<br/>
Thank-you,<br/>
{{ OrganizationName }}  

{{ EmailFooter }}', 'CCEDEC52-EC8A-41BF-9F78-C60418835257')
