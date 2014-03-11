-- Set default idle redirect attribute value to 9999
update [Attribute] set DefaultValue = '9999' where Guid = 'A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD'
-- Set all idle redirect attribute values to 9999
update AttributeValue set Value='9999'
where AttributeId in ( select Id from [Attribute] where Guid = 'A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD' )
