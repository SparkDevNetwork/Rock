-- JE: Update following suggestion email to fix hard coded page id
-- delete the page route if it exists
DELETE FROM [PageRoute] WHERE [Guid] = '3F3B0DE8-FDAB-499B-4706-68C36DD4DF84'

-- add route
DECLARE @SuggestionPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '50BAAD66-46AB-4968-AFD6-254C536ACEC8')
INSERT INTO [PageRoute]
	([IsSystem], [PageId], [Route], [Guid])
VALUES
	(1, @SuggestionPageId, 'FollowingSuggestionList', '3F3B0DE8-FDAB-499B-4706-68C36DD4DF84')

-- update the suggestion system email
UPDATE [SystemEmail]
	SET [Body] = '
{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    Listed below are some suggestions for you to review. You can choose <a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}FollowingSuggestionList">to follow</a> (or ignore) any of these suggestions from your Rock dashboard.
</p>

{% for suggestion in Suggestions %}
    <h4>{{ suggestion.SuggestionType.Name }}</h4>
    <table cellpadding="25">
    {% for notice in suggestion.Notices %}
        {{ notice }}
    {% endfor %}
    </table>
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
	WHERE [Guid] = '8F5A9400-AED2-48A4-B5C8-C9B5D5669F4C'

