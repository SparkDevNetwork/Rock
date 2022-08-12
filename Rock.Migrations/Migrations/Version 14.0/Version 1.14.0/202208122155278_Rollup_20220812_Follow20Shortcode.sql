DELETE FROM [LavaShortcode] WHERE ([Guid]='1E6785C0-7D92-49A7-9E15-68E113399152')
INSERT INTO [LavaShortcode] ([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid]) VALUES (N'Follow Icon', N'Add an icon with the ability to follow any entity with a click.', N'<div class="alert alert-info"><strong>Heads Up!</strong> Enabling the following API requires changes to Rock security. <a href="#followingsecuritydocs">See documentation below</a></div>

<p>Basic Usage:</p>
    <pre><code>{[ followicon entitytypeguid:''GUID'' entityguid:''GUID'' purposekey:''like'' isfollowed:''true'' ]}
    &lt;div class="followitem"&gt;
        &lt;i class="followicon"&gt;&lt;/i&gt;
    &lt;/div&gt;
{[ endfollowicon ]}</code></pre>
    
    <h4 id="shortcode-options">Shortcode Options</h4>
    <ul>
    <li><strong>entitytypeguid </strong>– The Guid of the Entity Type.</li>
    <li><strong>entityguid </strong>– The Guid of the Entity. </li>
    <li><strong>purposekey</strong> – An optional purpose key.</li>
    <li><strong>isfollowed</strong> (false) – A boolean to define if the entity is followed by the current person.</li>
    <li><strong>suppresswarnings</strong> (false) – Optionally, set to true to hide alerts when security and other errors are triggered.</li></ul>
   

    <h4 id="shortcode-output">Output</h4>
    <p>The output of the shortcode is a <code>followicon</code> div with a class of <code>isfollowed</code> that is added when the entity is followed.
    </p><pre><code>&lt;div class="followicon js-followicon" data-entitytype="{{entitytypeguid}}" data-entity="{{entityguid}}" data-followed="{{isfollowed}}"&gt;
    &lt;!-- Content --&gt;
&lt;/div&gt;</code></pre>

<h4 id="followingsecuritydocs">Security Configuration</h4>
<p>To use this shortcode, some security changes are required to allow all authenticated users to follow entities.</p>
<ol>
<li>Go to <a href="/admin/security/rest" target="_blank">Admin Tools > Security > REST Controllers</a></li><li>Locate the row "Followings", and open the REST Controller Actions detail page.</li>
<li>From this screen locate the following methods, and allow "All Authenticated Users" for the edit verb on the following methods:
<ul>
<li><strong>DELETE</strong> <code>api/Followings/{entityTypeGuid}/{entityGuid}?purposeKey={purposeKey}</code></li>
<li><strong>POST</strong> <code>api/Followings/{entityTypeGuid}/{entityGuid}?purposeKey={purposeKey}</code></li>
</ul>
</li>
</ol>
<p>No other security changes are recommended.</p>', '1', '1', N'followicon', N'{% if CurrentPerson %}
        {% assign entitytypeguid = entitytypeguid | Trim %}
        {% assign entityguid = entityguid | Trim %}
        {% assign entitytypeid = entitytypeid | Trim %}
        {% assign entityid = entityid | Trim %}
        {% if entitytypeguid != '''' and entityguid != '''' %}
        {% assign entitytype = entitytypeguid %}
        {% assign entity = entityguid %}
        {% else %}
        {% assign entitytype = entitytypeid %}
        {% assign entity = entityid %}
        {% endif %}
        {% assign purposekey = purposekey | Trim %}
        {% assign suppresswarnings = suppresswarnings | AsBoolean %}
        {% assign isfollowed = isfollowed | AsBoolean %}
    
        {% if entitytype != '''' and entity != '''' %}
            <div class="followicon js-followicon {% if isfollowed %}isfollowed{% endif %}" data-entitytype="{{ entitytype }}" data-entity="{{ entity }}" {% if purposekey != '''' %}data-purpose-key="{{ purposekey }}"{% endif %} data-followed="{{ isfollowed }}">
                {{ blockContent }}
            </div>
    
            {% javascript id:''followicon'' disableanonymousfunction:''true''%}
                $( document ).ready(function() {
                    $(''.js-followicon'').click(function(e) {
                        e.preventDefault();
                        var icon = $(this);
                        var entityType = icon.data(''entitytype'');
                        var entity = icon.data(''entity'');
                        var purpose = icon.data(''purpose-key'');
                        if (purpose != undefined && purpose != '''') {
                            purpose = ''?purposeKey='' + purpose;
                        } else {
                            purpose = '''';
                        }
                        icon.toggleClass(''isfollowed'');
                        if ( icon.hasClass(''isfollowed'') ) {
                            var actionType = ''POST'';
                        } else {
                            var actionType = ''DELETE'';
                        }
                        $.ajax({
                            url: ''/api/Followings/'' + entityType + ''/'' + entity + purpose,
                            type: actionType,
                            statusCode: {
                                201: function() {
                                    icon.attr(''data-followed'', ''true'');
                                },
                                204: function() {
                                    icon.attr(''data-followed'', ''false'');
                                },
                                500: function() {
                                    {% unless suppresswarnings %}
                                    alert(''Error: Check your Rock security settings and try again.'');
                                    {% endunless %}
                                }
                            },
                            error: function() {
                                icon.toggleClass(''isfollowing'');
                            }
                        });
                    });
                });
            {% endjavascript %}
        {% else %}
            <!-- Follow Icon Shortcode is missing entitytype and/or entity. Note: Guids or Ids must be provided  -->
        {% endif %}
    {% endif %}
    ', '2', N'', N'entitytypeguid^|entityguid^|entitytypeid^|entityid^|purposekey^|isfollowed^false|suppresswarnings^false|entitytypeguid^|entityguid^', '1E6785C0-7D92-49A7-9E15-68E113399152');
