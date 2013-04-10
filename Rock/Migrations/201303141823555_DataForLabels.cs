//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class DataForLabels : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Get the entitytype for workflow file
    DECLARE @WorkflowTypeEntityTypeId int
    SELECT @WorkflowTypeEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowType'
    IF @WorkflowTypeEntityTypeId IS NULL
    BEGIN
        INSERT INTO [EntityType] ([Name], [FriendlyName], [Guid])
        VALUES ('Rock.Model.WorkflowType', 'Workflow Type', '3da4aeda-ad50-4fbf-9f80-d9d88a1d0a4a')
        SET @WorkflowTypeEntityTypeId = SCOPE_IDENTITY()
    END

    -- Add 'System Workflows' category for workflow types
    DECLARE @CategoryId int
    INSERT INTO [Category] (IsSystem, EntityTypeId, Name, Guid, IconCssClass)
        VALUES (1, @WorkflowTypeEntityTypeId, 'System Workflows', '7DA7915A-3EE2-4534-95DD-3A92501EDD93', 'icon-cogs')
    SET @CategoryId = SCOPE_IDENTITY()

    -- Add Workflow Type for Zebra labels
    DECLARE @WorkflowTypeId int
    INSERT INTO [WorkflowType] (IsSystem, IsActive, Name, Description, CategoryId, [Order], WorkTerm, IsPersisted, LoggingLevel, Guid)
        VALUES (1, 1, 'Parse Zebra Label', 'Parses an uploaded Zebra ZPL file to find merge fields', @CategoryId, 0, 'Workflow', 0, 0, 'F820779B-38BE-4730-9AC5-093B53F054EB')
    SET @WorkflowTypeId = SCOPE_IDENTITY()

    -- Add Workflow Activity for parsing file
    DECLARE @WorkflowActivityTypeId int
    INSERT INTO [WorkflowActivityType] (IsActive, WorkflowTypeId, Name, Description, IsActivatedWithWorkflow, [Order], Guid)
        VALUES (1, @WorkflowTypeId, 'Parse File', 'Parses the Zebra label file', 1, 0, '12FF793E-8111-4464-B42C-EAF131149261')
    SET @WorkflowActivityTypeId = SCOPE_IDENTITY()

    -- Get the entitytype for zebra parse action
    DECLARE @ZebraParseActionEntityTypeId int
    SELECT @ZebraParseActionEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.ParseZebraLabel'
    IF @ZebraParseActionEntityTypeId IS NULL
    BEGIN
        INSERT INTO [EntityType] ([Name], [FriendlyName], [Guid])
        VALUES ('Rock.Workflow.Action.ParseZebraLabel', 'Parse Zebra Label', 'E5A7E121-B8E6-4F86-A624-015B39441910')
        SET @ZebraParseActionEntityTypeId = SCOPE_IDENTITY()
    END

    -- Add the workflow action type for zebra parse
    INSERT INTO [WorkflowActionType] (ActivityTypeId, Name, [Order], EntityTypeId, IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, Guid)
        VALUES (@WorkflowActivityTypeId, 'Parse Label File', 0, @ZebraParseActionEntityTypeId, 1, 1, '693130A2-59FC-4477-A065-9A4524A9B28A');

" );
    
            UpdateFieldType( "Campus", "List of Campuses", "Rock", "Rock.Field.Types.CampusFieldType", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED" );
            UpdateFieldType( "Email Template", "List of Email Templates", "Rock", "Rock.Field.Types.EmailTemplateFieldType", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956" );
            UpdateFieldType( "File", "Field used to display or upload a new binary file of a specific type", "Rock", "Rock.Field.Types.FileFieldType", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A" );
            UpdateFieldType( "Group Type", "List of Group Types", "Rock", "Rock.Field.Types.GroupTypeFieldType", "18E29E23-B43B-4CF7-AE41-C85672C09F50" );
            UpdateFieldType( "Key Value List", "Expandable list of Key/Value pairs.  Value can either be free form text, or a list from a specified Defined Type", "Rock", "Rock.Field.Types.KeyValueListFieldType", "73B02051-0D38-4AD9-BF81-A2D477DE4F70" );
            UpdateFieldType( "Binary File Type", "List of binary file types", "Rock", "Rock.Field.Types.BinaryFileTypeFieldType", "09EC7F0D-3505-4090-B010-ABA68CB9B904" );
            UpdateFieldType( "Workflow Type", "List of workflow types", "Rock", "Rock.Field.Types.WorkflowTypeFieldType", "46A03F59-55D3-4ACE-ADD5-B4642225DD20" );
            UpdateFieldType( "Binary File", "Lists a dropdown of existing binary files of a specific type", "Rock", "Rock.Field.Types.BinaryFileFieldType", "C403E219-A56B-439E-9D50-9302DFE760CF" );
            
            DeleteDefinedType("E4D289A9-70FA-4381-913E-2A757AD11147");
            AddDefinedType( "Check-in", "Merge Fields", "Available merge field values for check-in labels", "E4D289A9-70FA-4381-913E-2A757AD11147" );
            AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Security Code", "The security code generated during check-in", "9DCB69C4-1B9B-4D31-B8B6-8C9DBFA9933B", true );
            AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "First Name", "The first name of the person who checked in", "E2DB40F4-A064-429E-9A76-C520E7E7A43A", true );
            AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Last Name", "The last name of the peron who checked in", "85B71255-19F9-4443-A7AE-EF670385DC71", true );
            AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Birthday Image", "Upcoming Birthday indicator", "C6AA76B5-3E7F-4E14-905E-36173E60949D", true );
            AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Schedule Times", "The schedule(s) that person checked into", "18B24BF8-26DD-43BB-A54D-8B10C57EA740", true );

            DeleteDefinedType( "1EBCDB30-A89A-4C14-8580-8289EC2C7742" );
            AddDefinedType( "Check-in", "CheckIn Search Type", "The ways to search for a family in the check-in application", "1EBCDB30-A89A-4C14-8580-8289EC2C7742" );
            AddDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Phone Number", "Search for family based on phone number", "F3F66040-C50F-4D13-9652-780305FFFE23", true );
            AddDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Barcode", "Search for family based on assigned code", "9A66BFCD-0F16-4EAE-BE35-B3FAF4B817BE", true );

            DeleteDefinedType( "0368B637-327A-4F5E-80C2-832079E482EE" );
            AddDefinedType( "Global", "Device Type", "The types of devices", "0368B637-327A-4F5E-80C2-832079E482EE" );
            AddDefinedValue( "0368B637-327A-4F5E-80C2-832079E482EE", "Check-in Kiosk", "Computer or Tablet used for check-in", "BC809626-1389-4543-B8BB-6FAC79C27AFD", true );
            AddDefinedValue( "0368B637-327A-4F5E-80C2-832079E482EE", "Giving Kiosk", "Computer used as a giving kiosk", "64A1DBE5-10AD-42F1-A9BA-646A781D4112", true );
            AddDefinedValue( "0368B637-327A-4F5E-80C2-832079E482EE", "Printer", "Printer", "8284B128-E73B-4863-9FC2-43E6827B65E6", true );
            AddDefinedValue( "0368B637-327A-4F5E-80C2-832079E482EE", "Digital Signage", "Computer used to display promoted content", "01B585B1-389D-4C86-A9DA-267A8564699D", true );

            // Add the Attribute and values for the merge field code
            AddEntityAttribute( "Rock.Model.DefinedValue", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "DefinedTypeId", "??", "MergeField", "", "The .Liquid syntax for accessing values from the check-in state object", 0, "", "51EB8583-55EA-4431-8B66-B5BD0F83D81E" );
            Sql( @"
    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '51EB8583-55EA-4431-8B66-B5BD0F83D81E')
    UPDATE [Attribute]
        SET [EntityTypeQualifierValue] = (SELECT CAST([Id] as varchar) FROM [DefinedType] WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147'), [IsGridColumn] = 1
        WHERE [Id] = @AttributeId
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
        VALUES (1, @AttributeId, 
            (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '9DCB69C4-1B9B-4D31-B8B6-8C9DBFA9933B'),
            0, '{{ person.SecurityCode }}', 'A1EA9242-EF24-41F7-9094-4DA8BEB9F3B7');
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
        VALUES (1, @AttributeId, 
            (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'E2DB40F4-A064-429E-9A76-C520E7E7A43A'),
            1, '{{ person.Person.FirstName }}', '240B5E6D-CE63-439D-BB40-3A8A3EC4F99E');
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
        VALUES (1, @AttributeId, 
            (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '85B71255-19F9-4443-A7AE-EF670385DC71'),
            2, '{{ person.Person.LastName }}', 'EE773F9D-D5E5-408D-9315-75A48F775053');
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
        VALUES (1, @AttributeId, 
            (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'C6AA76B5-3E7F-4E14-905E-36173E60949D'),
            3, '{% if person.Person.DaysToBirthday <= 7 %}B{% endif %}', '0BFB850D-920C-415E-9684-086DAB950731');
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
        VALUES (1, @AttributeId, 
            (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '18B24BF8-26DD-43BB-A54D-8B10C57EA740'),
            4, '{% for location in groupType.Locations %}{% for group in location.Groups %}{% for schedule in group.Schedules %}{{ schedule.Schedule.Name }} {% endfor %}{% endfor %}{% endfor %}', '0E5AD48B-4B80-48C5-8F38-1D9C7F6532BC');
" );

            // Add the binary file type, and attribute for the Check-in Label file type
            DeleteAttribute( "CE57450F-634A-420A-BF5A-B43E9B20ABF2" ); // Merge Code Attribute
            AddEntityAttribute( "Rock.Model.BinaryFile", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "BinaryFileTypeId", "??", "Merge Codes", "", "The merge codes used by the label.  The Key (left column) is the field found in the label file.  The Value (right column) is the Check-in data that should be displayed in that location.", 0, "", "CE57450F-634A-420A-BF5A-B43E9B20ABF2" );
            Sql( @"
    DECLARE @FileTypeId int
    SET @FileTypeId = (SELECT [Id] FROM [BinaryFileType] WHERE [Guid] = 'DE0E5C50-234B-474C-940C-C571F385E65F')
    IF @FileTypeId IS NULL
    BEGIN
        INSERT INTO [BinaryFileType] (IsSystem, Name, Description, IconCssClass, Guid)
            VALUES (1, 'Check-in Label', 'Labels used for check-in', 'icon-print', 'DE0E5C50-234B-474C-940C-C571F385E65F');
        SET @FileTypeId = SCOPE_IDENTITY()
    END

    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'CE57450F-634A-420A-BF5A-B43E9B20ABF2')
    UPDATE [Attribute]
        SET [EntityTypeQualifierValue] = CAST(@FileTypeId as varchar)
        WHERE [Id] = @AttributeId

    DECLARE @DefinedTypeId int
    SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147')
    INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
        VALUES (1, @AttributeId, 'definedtype', CAST(@DefinedTypeId as varchar), '89048786-1D23-4790-A240-9585AF5F4AAF')
" );
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Check-in Labels", "", "Default", "7C093A63-F2AC-4FE3-A826-8BF06D204EA2" );
            AddPage( "7C093A63-F2AC-4FE3-A826-8BF06D204EA2", "Check-in Label", "", "Default", "B565FDF8-959F-4AC8-ACDF-3B1B5CFE79F5" );
            AddBlockType( "Administration - Binary File List", "", "~/Blocks/Administration/BinaryFileList.ascx", "26541C8A-9E54-4723-A739-21FAA5191014" );
            AddBlockType( "Administration - Binary File Detail", "", "~/Blocks/Administration/BinaryFileDetail.ascx", "B97B2E51-5C9C-459B-999F-C7797DAD8B69" );
            AddBlock( "7C093A63-F2AC-4FE3-A826-8BF06D204EA2", "26541C8A-9E54-4723-A739-21FAA5191014", "Binary File List", "", "Content", 0, "353279FD-C730-4BB2-A493-B850E4DC1DDE" );
            AddBlock( "B565FDF8-959F-4AC8-ACDF-3B1B5CFE79F5", "B97B2E51-5C9C-459B-999F-C7797DAD8B69", "Binary File Detail", "", "Content", 0, "F52CEDB1-F822-485C-9A1C-BA6D05383FAA" );
            AddBlockTypeAttribute( "26541C8A-9E54-4723-A739-21FAA5191014", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "9F9FEE39-2602-4045-8D6C-8F06577E9736" );
            AddBlockTypeAttribute( "26541C8A-9E54-4723-A739-21FAA5191014", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Binary File Type", "BinaryFileType", "", "", 0, "", "07C43868-F930-41F3-9C03-5644DD47718E" );
            AddBlockTypeAttribute( "B97B2E51-5C9C-459B-999F-C7797DAD8B69", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Binary File Type", "ShowBinaryFileType", "", "", 0, "False", "F82BB6A2-5E7A-43FE-9901-A6587DA5A392" );
            AddBlockTypeAttribute( "B97B2E51-5C9C-459B-999F-C7797DAD8B69", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow to activate for any new file uploaded", 0, "", "27DDFD07-835C-4735-B42D-DD45629E465F" );

            // Attrib Value for Binary File List:Detail Page Guid
            AddBlockAttributeValue( "353279FD-C730-4BB2-A493-B850E4DC1DDE", "9F9FEE39-2602-4045-8D6C-8F06577E9736", "b565fdf8-959f-4ac8-acdf-3b1b5cfe79f5" );
            // Attrib Value for Binary File List:Binary File Type
            AddBlockAttributeValue( "353279FD-C730-4BB2-A493-B850E4DC1DDE", "07C43868-F930-41F3-9C03-5644DD47718E", "de0e5c50-234b-474c-940c-c571f385e65f" );
            // Attrib Value for Binary File Detail:Show Binary File Type
            AddBlockAttributeValue( "F52CEDB1-F822-485C-9A1C-BA6D05383FAA", "F82BB6A2-5E7A-43FE-9901-A6587DA5A392", "False" );
            // Attrib Value for Binary File Detail:Workflow
            AddBlockAttributeValue( "F52CEDB1-F822-485C-9A1C-BA6D05383FAA", "27DDFD07-835C-4735-B42D-DD45629E465F", "f820779b-38be-4730-9ac5-093b53f054eb" );

            Sql( @"
UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-print'
WHERE [Guid] = '7C093A63-F2AC-4FE3-A826-8BF06D204EA2'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "9F9FEE39-2602-4045-8D6C-8F06577E9736" ); // Detail Page Guid
            DeleteAttribute( "07C43868-F930-41F3-9C03-5644DD47718E" ); // Binary File Type
            DeleteAttribute( "F82BB6A2-5E7A-43FE-9901-A6587DA5A392" ); // Show Binary File Type
            DeleteAttribute( "27DDFD07-835C-4735-B42D-DD45629E465F" ); // Workflow
            DeleteBlock( "353279FD-C730-4BB2-A493-B850E4DC1DDE" ); // Binary File List
            DeleteBlock( "F52CEDB1-F822-485C-9A1C-BA6D05383FAA" ); // Binary File Detail
            DeleteBlockType( "26541C8A-9E54-4723-A739-21FAA5191014" ); // Administration - Binary File List
            DeleteBlockType( "B97B2E51-5C9C-459B-999F-C7797DAD8B69" ); // Administration - Binary File Detail
            DeletePage( "B565FDF8-959F-4AC8-ACDF-3B1B5CFE79F5" ); // Check-in Label
            DeletePage( "7C093A63-F2AC-4FE3-A826-8BF06D204EA2" ); // Check-in Labels

            DeleteAttribute( "51EB8583-55EA-4431-8B66-B5BD0F83D81E" );
            DeleteDefinedType( "0368B637-327A-4F5E-80C2-832079E482EE" );
            DeleteDefinedType( "1EBCDB30-A89A-4C14-8580-8289EC2C7742" );
            DeleteDefinedType( "E4D289A9-70FA-4381-913E-2A757AD11147" );

            Sql( @"
    DELETE [WorkflowActionType] WHERE [Guid] = '693130A2-59FC-4477-A065-9A4524A9B28A'
    DELETE [WorkflowActivityType] WHERE [Guid] = '12FF793E-8111-4464-B42C-EAF131149261'
    DELETE [WorkflowType] WHERE [Guid] = 'F820779B-38BE-4730-9AC5-093B53F054EB'
    DELETE [Category] WHERE [Guid] = '7DA7915A-3EE2-4534-95DD-3A92501EDD93'
" );
        }
    }
}
