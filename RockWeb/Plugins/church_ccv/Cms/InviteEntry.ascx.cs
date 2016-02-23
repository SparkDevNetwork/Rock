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
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

    [CodeEditorField( "ContentObject", "JSON Dynamic Array that can be used by the Template as a MergeField.", Rock.Web.UI.Controls.CodeEditorMode.JavaScript, order: 0, defaultValue: @"[
  {
    ""Name"": ""Anthem""
  },
  {
    ""Name"": ""Avondale""
  },
  {
    ""Name"": ""East Valley""
  },
  {
    ""Name"": ""Peoria"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""5:30 pm"",
          ""7:00 pm""
        ]
      },
      {
        ""Date"": ""Saturday 3/26"",
        ""Times"": [
          ""4:30 pm"",
          ""6:00 pm""
        ]
      },
      {
        ""Date"": ""Sunday 3/27"",
        ""Times"": [
          ""6:30 am sunrise"",
          ""9:00 am"",
          ""10:30 am"",
          ""12:00 pm""
        ]
      }
    ]
  },
  {
    ""Name"": ""Surprise""
  },
  {
    ""Name"": ""Scottsdale""
  }
]
" )]

    [CodeEditorField( "Template", "Lava template to render the content.  Use the special <pre>{{{{ EmailTemplate }}}}</pre>' and <pre>{{{{ TextTemplate }}}}</pre> to include the templates from the Email and Text templates", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 1, defaultValue: @"
{% for item in ContentObject %}
    <ul>
        <li>Name is {{ item.Name }}</li>
        <li>Services are 
            <ul>
                {% for service in item.Services %}
                    <li>Date: {{ service.Date | Date:'M/d/yyyy' }} 
                        <ul>
                    {% for time in service.Times %}
                        <li>Time: {{ time }} </li>
                    {% endfor %}
                        </ul>
                    </li>
                {% endfor %}        
            </ul>
        </li>

        {{{{ TextTemplate }}}} <br/>
        {{{{ EmailTemplate }}}}
    </ul>
    
    <hr>
{% endfor %}

<pre>
DeviceFamily: {{ DeviceFamily }}
OSFamily: {{ OSFamily  }}
</pre>
" )]

    [CodeEditorField( "Email Template", "Lava template which will be used for the <pre>{{{{ EmailTemplate }}}}</pre> which can be used to create a mailto link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 2, defaultValue: @"
{% capture subject %}
Fun Event at {{ item.Name }}
{% endcapture %}

{% capture body %}
You are invited to go to Fun Event with me at the {{ Context.Campus.Name }} campus.
Wanna Go? There will be lots of fun stuff to do!

Which date works best for you?
{% for service in item.Services %}
    Date: {{ service.Date | Date:'M/d/yyyy' }} 
    {% for time in service.Times %}Time: {{ time }}{% endfor %}
{% endfor %}        

Your friend,
{{ CurrentPerson.NickName }}
{% endcapture %}

<a class='btn btn-default' href=""mailto:?subject={{ subject | Trim | EscapeDataString }}&body={{ body | EscapeDataString }}"">Email</a>
" )]

    [CodeEditorField( "Text Template", "Lava template which will be used for the <pre>{{{{ TextTemplate }}}}</pre> which can be used to create an SMS link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 3, defaultValue: @"
{% capture smsAll %}
I'm going to Fun Event at {{ item.Name }}. Would you like to join me & some friends? Check out the service times at http://mychurch.com/FunEvent and let's plan to go together!
{% endcapture %}

<a class='btn btn-default' href=""sms:?body={{ smsAll | Trim | EscapeDataString }}"">Text</a>
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

            Parser uaParser = Parser.GetDefault();

            ClientInfo client = uaParser.Parse( this.Request.UserAgent );
            mergeFields.Add( "DeviceFamily", client.Device.Family );
            mergeFields.Add( "OSFamily", client.OS.Family.ToLower() );

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


                var converter = new ExpandoObjectConverter();

                var contentObject = JsonConvert.DeserializeObject<List<ExpandoObject>>( contentObjectJSON, converter ); //JsonConvert.DeserializeObject<List<Dictionary<string, object>>>( contentObjectJSON );
                mergeFields.Add( "ContentObject", contentObject );

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
                        this.AddCacheItem( cacheKey, lContent.Text, cacheDuration );
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