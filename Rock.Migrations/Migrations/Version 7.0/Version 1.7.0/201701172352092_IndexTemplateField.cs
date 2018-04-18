// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class IndexTemplateField : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntityType", "IndexResultTemplate", c => c.String());

            Sql( @" UPDATE [EntityType]
  SET [IndexResultTemplate] = '<div class=""row model-cannavigate"" data-href=""{{ IndexDocument.Url }}"">
    <div class=""col-sm-1 text-center"">
        <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
    </div>
    <div class=""col-sm-11"">
        <strong>{{ IndexDocument.PageTitle }}</strong> <br>
        {{ IndexDocument.Content | TruncateWords:50 }}
    </div>
</div>'
  WHERE [Name] = 'Rock.Model.Site'

    UPDATE [EntityType]
  SET [IndexResultTemplate] = '{% if IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.PersonIndex"" %}

    {% assign url = ""~/Person/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Person-Url and DisplayOptions.Person-Url != null and DisplayOptions.Person-Url != '''' %}
        {% assign url = DisplayOptions.Person-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class=""row model-cannavigate"" data-href=""{{ url }}{{ IndexDocument.Id }}"">
        <div class=""col-sm-1 text-center"">
            <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
        </div>
        <div class=""col-md-3 col-sm-10"">
            <strong>{{ IndexDocument.NickName}} {{ IndexDocument.LastName}} {{ IndexDocument.Suffix }}</strong> 
            <br>
            {% if IndexDocument.Email != null and IndexDocument.Email != '' %}
                {{ IndexDocument.Email }} <br>
            {% endif %}
    
            {% if IndexDocument.StreetAddress != '' and IndexDocument.StreetAddress != null %}
                {{ IndexDocument.StreetAddress }}<br>
            {% endif %}
            
            {% if IndexDocument.City != '' and IndexDocument.City != null %}
                {{ IndexDocument.City }}, {{ IndexDocument.State }} {{ IndexDocument.PostalCode }}
            {% endif %}
        </div>
        <div class=""col-md-2"">
            Connection Status: <br> 
            {{ IndexDocument.ConnectionStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class=""col-md-2"">
            Age: <br> 
            {{ IndexDocument.Age }}
        </div>
        <div class=""col-md-2"">
            Record Status: <br> 
            {{ IndexDocument.RecordStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class=""col-md-2"">
            Campus: <br> 
            {{ IndexDocument.CampusId | FromCache:''Campus'' | Property:''Name'' }}
        </div>
    </div>

{% elseif IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.BusinessIndex"" %}
    {% assign url = ""~/Business/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Business-Url and DisplayOptions.Business-Url != null and DisplayOptions.Business-Url != '' %}
        {% assign url = DisplayOptions.Business-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class=""row model-cannavigate"" data-href=""{{ url }}{{ IndexDocument.Id }}"">
        <div class=""col-sm-1 text-center"">
            <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
        </div>
        <div class=""col-sm-11"">
            <strong>{{ IndexDocument.Name}}</strong> 

            {% if IndexDocument.Contacts != null and IndexDocument.Contacts != '' %}
                <br>Contacts: {{ IndexDocument.Contacts }}
            {% endif %}
        </div>
    </div>
{% endif %}'
  WHERE [Name] = 'Rock.Model.Person'

    UPDATE [EntityType]
  SET [IndexResultTemplate] = '{% assign url = ""~/Group/"" | ResolveRockUrl %}

{% if DisplayOptions.Group-Url and DisplayOptions.Group-Url != null and DisplayOptions.Group-Url != '''' %}
    {% assign url = DisplayOptions.Group-Url | ResolveRockUrl %}
{% endif %}

<div class=""row model-cannavigate"" data-href=""{{ url }}/{{ IndexDocument.Id }}"">
    <div class=""col-sm-1 text-center"">
        <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
    </div>
    <div class=""col-sm-11"">
        <strong>{{ IndexDocument.Name }}</strong> <small>({{ IndexDocument.GroupTypeName }})</small>
        {% if IndexDocument.Description != null and IndexDocument.Description != '''' %}
            <br> 
            {{ IndexDocument.Description }}
        {% endif %}
    </div>
</div>'
  WHERE [Name] = 'Rock.Model.Group'

      UPDATE [EntityType]
  SET [IndexResultTemplate] = '{% assign itemId =  IndexDocument.Id %}
{% assign url = ""~/ContentChannelItem/"" | ResolveRockUrl | Append:itemId %}

{% if DisplayOptions.ChannelItem-Url and DisplayOptions.ChannelItem-Url != null and DisplayOptions.ChannelItem-Url != '''' %}
    {% assign url = DisplayOptions.ChannelItem-Url | ResolveRockUrl %}
{% endif %}

<div class=""row model-cannavigate"" data-href=""{{ url }}"">
    <div class=""col-sm-1 text-center"">
        <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
    </div>
    <div class=""col-sm-11"">
        <strong>{{ IndexDocument.Title }}</strong> <small>({{ IndexDocument.ContentChannel }})</small>
        {% if IndexDocument.Summary != null and IndexDocument.Summary != '''' %}
            <br> 
            {{ IndexDocument.Summary }}
        {% endif %}
        {% if IndexDocument.SummaryText != null and IndexDocument.SummaryText != '''' %}
            <br> 
            {{ IndexDocument.SummaryText }}
        {% endif %}
    </div>
</div>'
  WHERE [Name] = 'Rock.Model.ContentChannelItem'" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.EntityType", "IndexResultTemplate");
        }
    }
}
