UPDATE [EntityType] SET [IndexResultTemplate]=N'{% if IndexDocument.IndexModelType == "Rock.UniversalSearch.IndexModels.PersonIndex" %}

    {% assign url = "~/Person/" | ResolveRockUrl %}
    
    {% if DisplayOptions.Person-Url and DisplayOptions.Person-Url != null and DisplayOptions.Person-Url != '''' %}
        {% assign url = DisplayOptions.Person-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class="row model-cannavigate" data-href="{{ url }}{{ IndexDocument.Id }}">
        <div class="col-sm-1 text-center">
            <div class="photo-round photo-round-sm" style="margin: 0 auto;" data-original="{{ IndexDocument.PhotoUrl | ResolveRockUrl }}&maxwidth=200&maxheight=200&w=100" style="background-image: url({{ ''~/Assets/Images/person-no-photo-male.svg'' |  ResolveRockUrl }}); display: block;"></div>
        </div>
        <div class="col-md-3 col-sm-10">
            <strong>{{ IndexDocument.NickName }} {{ IndexDocument.LastName }} {{ IndexDocument.Suffix }}</strong> 
            <br>
            {% if IndexDocument.Email != null and IndexDocument.Email != '''' %}
                {{ IndexDocument.Email }} <br>
            {% endif %}
    
            {% if IndexDocument.StreetAddress != '''' and IndexDocument.StreetAddress != null %}
                {{ IndexDocument.StreetAddress }}<br>
            {% endif %}
            
            {% if IndexDocument.City != '''' and IndexDocument.City != null %}
                {{ IndexDocument.City }}, {{ IndexDocument.State }} {{ IndexDocument.PostalCode }}
            {% endif %}
        </div>
        <div class="col-md-2">
            Connection Status: <br> 
            {{ IndexDocument.ConnectionStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class="col-md-2">
            Age: <br> 
            {{ IndexDocument.Age }}
        </div>
        <div class="col-md-2">
            Record Status: <br> 
            {{ IndexDocument.RecordStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class="col-md-2">
            Campus: <br> 
            {{ IndexDocument.CampusId | FromCache:''Campus'' | Property:''Name'' }}
        </div>
    </div>

{% elseif IndexDocument.IndexModelType == "Rock.UniversalSearch.IndexModels.BusinessIndex" %}
    {% assign url = "~/Business/" | ResolveRockUrl %}
    
    {% if DisplayOptions.Business-Url and DisplayOptions.Business-Url != null and DisplayOptions.Business-Url != '''' %}
        {% assign url = DisplayOptions.Business-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class="row model-cannavigate" data-href="{{ url }}{{ IndexDocument.Id }}">
        <div class="col-sm-1 text-center">
            <i class="{{ IndexDocument.IconCssClass }} fa-2x"></i>
        </div>
        <div class="col-sm-11">
            <strong>{{ IndexDocument.Name }}</strong> 

            {% if IndexDocument.Contacts != null and IndexDocument.Contacts != '''' %}
                <br>Contacts: {{ IndexDocument.Contacts }}
            {% endif %}
        </div>
    </div>
{% endif %}' WHERE [Guid]='72657ED8-D16E-492E-AC12-144C5E7567E7' AND [IndexResultTemplate] LIKE N'%if IndexDocument.StreetAddress != '' and IndexDocument.StreetAddress != null%'