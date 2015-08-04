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