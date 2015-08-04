  -- JE: Change name and icon of the event detail page
  UPDATE [Page]	SET 
    [InternalName] = 'Event Detail', 
    [BrowserTitle] = 'Event Detail', 
    [PageTitle] = 'Event Detail', 
    [IconCssClass] = 'fa fa-calendar-check-o'
  WHERE [Guid] = '7FB33834-F40A-4221-8849-BB8C06903B04'

 -- JE: Fix Mime Type on Check-in Labels
 UPDATE [BinaryFile] SET [MimeType] = 'text/plain'
 WHERE [BinaryFileTypeId] = (SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = 'DE0E5C50-234B-474C-940C-C571F385E65F')

-- JE: Add truncate filters to allergy and legal notes
UPDATE [AttributeValue] SET [Value] = '{% if Person.Allergy != '''' %}{{Person.Allergy | Truncate:100,''...'' }}{% endif %}'
WHERE [Guid] = '4315A58E-6514-49A8-B80C-22AC7710AC19' AND [ModifiedDateTime] IS NULL

UPDATE [AttributeValue] SET [Value] = '{% if Person.LegalNotes != '''' %}{{Person.LegalNotes  | Truncate:100,''...'' }}{% endif %}'
WHERE [Guid] = '89C604FA-61A9-4255-AE1F-B6381B23603F' AND [ModifiedDateTime] IS NULL