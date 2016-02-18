// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using UAParser;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Invite Entry" )]
    [Category( "CCV > Cms" )]
    [Description( "Block that helps a user send an invite message to somebody using email, text, etc" )]

    [CodeEditorField( "ContentObject", "JSON Array Object that can be used by the Template as a MergeField.", Rock.Web.UI.Controls.CodeEditorMode.JavaScript, order: 0, defaultValue: @"[
    { 
        ""DateTime"": ""02/17/2016 7:00PM"",
        ""Location"": ""The Grill"",
        ""Campuses"": ""Peoria,Surprise,Anthem""
    },
    { 
        ""DateTime"": ""02/19/2016 9:30AM"",
        ""Location"": ""Conference Room A"",
        ""Campuses"": null
    },
    { 
        ""DateTime"": ""02/27/2016 11:30AM"",
        ""Location"": ""Conference Room B"",
        ""Campuses"": ""Surprise, Anthem""
    }    
]
" )]

    [CodeEditorField( "Template", "Lava template to render the content.  Use the special <pre>{{{{ EmailTemplate }}}}</pre>' and <pre>{{{{ TextTemplate }}}}</pre> to include the templates from the Email and Text templates", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 1, defaultValue: @"
{% for item in ContentObject %}
    <ul>
        <li>Date is {{ item.DateTime | Date:'M/d/yyyy' }} </li>
        <li>Time is {{ item.DateTime | Date:'h:mm tt' }} </li>
        <li>Location is {{ item.Location }} </li>
        {% assign campusList = item.Campuses | Split:',' %}
        {% for campus in campusList %}
            Campus: {{ campus }}<br/>            
        {% endfor %}
        
        {{{{ TextTemplate }}}} <br/>
        {{{{ EmailTemplate }}}}
    </ul>
{% endfor %}

<pre>
DeviceFamily: {{ DeviceFamily }}
OSFamily: {{ OSFamily  }}
</pre>
" )]

    [CodeEditorField( "Email Template", "Lava template which will be used for the <pre>{{{{ EmailTemplate }}}}</pre> which can be used to create a mailto link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 2, defaultValue: @"
{% capture subject %}
Fun Event at {{ item.Location }}
{% endcapture %}

{% capture body %}
You are invited to go to Fun Event with me on the {{ Context.Campus.Name }} campus at {{ item.Location }}  @ {{ item.DateTime }}
Wanna Go? There will be lots of fun stuff to do!

Your friend,

{{ CurrentPerson.NickName }}
{% endcapture %}

<a class='btn btn-default' href=""mailto:?subject={{ subject | Trim | EscapeDataString }}&body={{ body | EscapeDataString }}"">Email</a>
" )]

    [CodeEditorField( "Text Template", "Lava template which will be used for the <pre>{{{{ TextTemplate }}}}</pre> which can be used to create an SMS link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 3, defaultValue: @"
{% capture smsAll %}
I'm going to Fun Event on the {{ Context.Campus.Name }} campus. Would you like to join me & some friends? Check out the service times at http://mychurch.com/FunEvent and let's plan to go together!
{% endcapture %}

{% case OSFamily %}
  {% when 'android' %}
    {% assign sep = '?' %}
  {% when 'ios' %}
    {% assign sep = '&' %}
  {% else %}
    {% assign sep = '&' %}
{% endcase %}

<a class='btn btn-default' href=""sms:{{ sep }}body={{ smsAll | Trim | EscapeDataString }}"">Text</a>
" )]

    [BooleanField( "Show Debug", "Show Lava Objects and Help", order: 4 )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 3600, "", order: 5 )]
    [TextField( "Cache Key", "Additional CacheKey to use when caching the content using Context Lava Merge Fields. For example: <pre>Campus={{ Context.Campus.Guid }}</pre>", order: 5 )]
    public partial class InviteEntry : RockBlock
    {
        private bool _flushCache = false;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += InviteEntry_BlockUpdated;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the InviteEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void InviteEntry_BlockUpdated( object sender, EventArgs e )
        {
            _flushCache = true;
            ShowContent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                ShowContent();
            }
        }

        /// <summary>
        /// Shows the content.
        /// </summary>
        public void ShowContent()
        {
            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );

            var contextObjects = new Dictionary<string, object>();
            foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
            {
                var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                if ( contextEntity != null && contextEntity is DotLiquid.ILiquidizable )
                {
                    var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                    if ( type != null )
                    {
                        contextObjects.Add( type.Name, contextEntity );
                    }
                }
            }

            if ( contextObjects.Any() )
            {
                mergeFields.Add( "Context", contextObjects );
            }

            var cacheKey = this.GetAttributeValue( "CacheKey" ) ?? string.Empty;
            cacheKey = string.Format( "InviteEntry:{0},CacheKey:{1}", this.BlockCache.Guid, cacheKey.ResolveMergeFields( mergeFields ) );

            if ( _flushCache )
            {
                this.FlushCacheItem( cacheKey );
                _flushCache = false;
            }

            var cachedContent = this.GetCacheItem( cacheKey ) as string;
            if ( string.IsNullOrEmpty( cachedContent ) )
            {
                if ( CurrentPerson != null )
                {
                    mergeFields.Add( "CurrentPerson", CurrentPerson );
                }

                mergeFields.Add( "Campuses", CampusCache.All() );
                mergeFields.Add( "PageParameter", PageParameters() );

                var contentObjectJSON = this.GetAttributeValue( "ContentObject" );

                var contentObject = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>( contentObjectJSON );
                mergeFields.Add( "ContentObject", contentObject );

                Parser uaParser = Parser.GetDefault();

                ClientInfo client = uaParser.Parse( this.Request.UserAgent );
                mergeFields.Add( "DeviceFamily", client.Device.Family );
                mergeFields.Add( "OSFamily", client.OS.Family.ToLower() );

                var template = this.GetAttributeValue( "Template" ) ?? string.Empty;
                var textTemplate = this.GetAttributeValue( "TextTemplate" ) ?? string.Empty;
                var emailTemplate = this.GetAttributeValue( "EmailTemplate" ) ?? string.Empty;
                template = template
                    .Replace( "{{{{ EmailTemplate }}}}", emailTemplate )
                    .Replace( "{{{{ TextTemplate }}}}", textTemplate );

                if ( this.GetAttributeValue( "ShowDebug" ).AsBoolean() && this.IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
                {
                    lContent.Text = mergeFields.lavaDebugInfo();
                }
                else
                {
                    lContent.Text = template.ResolveMergeFields( mergeFields );
                    var cacheDuration = this.GetAttributeValue( "CacheDuration" ).AsInteger();
                    if ( cacheDuration > 0 )
                    {
                        this.AddCacheItem( cacheKey, lContent.Text );
                    }
                }
            }
            else
            {
                lContent.Text = cachedContent;
            }
        }
    }
}