
-- Populate Campus
MERGE INTO [Campus] as Target
USING (
VALUES 
    (0, 'Sample Main', '09128AE6-C7E2-41BD-9262-FA40A10A0D26'), 
    (0, 'Sample West', '171920F8-3D5C-499C-86DC-27E77640AC59'), 
    (0, 'Sample East', '3BE9021A-25D7-4E0D-B40C-144EEE577B22') 
) as Source( IsSystem, Name, Guid)
ON Target.Guid = Source.Guid
WHEN MATCHED THEN
    UPDATE SET Name = Source.Name
WHEN NOT MATCHED BY TARGET THEN
    INSERT (IsSystem, Name, Guid) values (IsSystem, Name, Guid);

-- Populate Marketing Campaign
MERGE INTO [MarketingCampaign] as Target
USING (
VALUES
    ('Sample Campaign 1','1A171D52-A28E-4CDA-9FCF-6048F754EB11'),
    ('Sample Campaign 2','6E7E1AB8-B83B-4B66-BDCE-15A57DFA622F'),
    ('Sample Campaign 3','253AE494-B009-4C7C-BA81-315C8E15BD04'),
    ('Sample Campaign 4','29FBE49D-B07D-4C6D-9DB7-F813674C7872'),
    ('Sample Campaign 5','71182C49-44B9-4EA8-8C26-74FE4EE6043B'),
    ('Sample Campaign 6','C5255703-5B5F-46F5-9088-9244DE287D8C')
) as Source (Title, Guid)
ON Target.Guid = Source.Guid
WHEN MATCHED THEN
    UPDATE SET Title = Source.Title
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Title, Guid) values (Title, Guid);



