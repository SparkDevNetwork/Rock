declare
-- pick the Key you want to search for
  @attributeKey nvarchar(max) = 'Assignee'
DECLARE @origValue NVARCHAR(max) = CONCAT (
                'Workflow.'
                ,@attributeKey
                )
            ,@newValue NVARCHAR(max) = CONCAT (
                'Workflow | Attribute:'''
                ,@attributeKey
                ,''''
                )
            ,@likeValue NVARCHAR(max) = CONCAT (
                '% Workflow.'
                ,@attributeKey
                ,' %'
                )

select Value, Replace(Value, @origValue,@newValue) [UpdatedValue], CHARINDEX('{% if', Value) [Has 'If' Clause]  from AttributeValue where Value like @likeValue
select Content, Replace(Content, @origValue,@newValue) [UpdatedValue], CHARINDEX('{% if', Content) [Has 'If' Clause]  from HtmlContent where Content like @likeValue
select Header, Replace(Header, @origValue,@newValue) [UpdatedValue], CHARINDEX('{% if', Header) [Has 'If' Clause]  from WorkflowActionForm where Header like @likeValue
select Footer, Replace(Footer, @origValue,@newValue) [UpdatedValue], CHARINDEX('{% if', Footer) [Has 'If' Clause]  from WorkflowActionForm where Footer like @likeValue

-- uncomment this if you are confident that the replacement looks correct and that it isn't gonna mess up an '{% If' statement
-- Update AttributeValue Set Value = Replace(Value, @origValue,@newValue) where Value like @likeValue
-- Update WorkflowActionForm Set Header = Replace(Header, @origValue,@newValue) where Header like @likeValue



