using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Invite Entry" )]
    [Category( "CCV > Cms" )]
    [Description( "Block that helps a user send an invite message to somebody using email, text, etc" )]

    [CodeEditorField( "ContentObject", "JSON Dynamic Array that can be used by the Template as a MergeField.", Rock.Web.UI.Controls.CodeEditorMode.JavaScript, order: 0, 
defaultValue: @"[
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

    [CodeEditorField( "Template", "Lava template to render the content.  Use the special <pre>{{{{ EmailTemplate }}}}</pre>' and <pre>{{{{ TextTemplate }}}}</pre> to include the templates from the Email and Text templates", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 1, 
defaultValue: @"
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

    [CodeEditorField( "Email Template", "Lava template which will be used for the <pre>{{{{ EmailTemplate }}}}</pre> which can be used to create a mailto link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 2, 
defaultValue: @"
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

    [CodeEditorField( "Text Template", "Lava template which will be used for the <pre>{{{{ TextTemplate }}}}</pre> which can be used to create an SMS link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 3, 
defaultValue: @"
{% capture smsAll %}
I'm going to Fun Event at {{ item.Name }}. Would you like to join me & some friends? Check out the service times at http://mychurch.com/FunEvent and let's plan to go together!
{% endcapture %}

<a class='btn btn-default' href=""sms:?body={{ smsAll | Trim | EscapeDataString }}"">Text</a>
" )]

    [CodeEditorField( "Alternate Email Template", "Lava template which will be used for the <pre>{{{{ AlternateEmailTemplate }}}}</pre> which can be used to create a mailto link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 4, 
defaultValue: @"

" )]

    [CodeEditorField( "Alternate Text Template", "Lava template which will be used for the <pre>{{{{ AlternateTextTemplate }}}}</pre> which can be used to create an SMS link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 5, 
defaultValue: @"

" )]
    public partial class InviteEntry : church.ccv.Utility.Web.BaseContentBlock
    {
        /// <summary>
        /// Shows the content.
        /// </summary>
        public override void ShowContent()
        {
            lContent.Text = this.GetContentHtml();
        }

        /// <summary>
        /// Gets the content merge fields.
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> GetContentMergeFields()
        {
            var mergeFields = base.GetContentMergeFields();

            var contentObjectJSON = this.GetAttributeValue( "ContentObject" );

            if ( !string.IsNullOrEmpty( contentObjectJSON ) )
            {
                var converter = new ExpandoObjectConverter();

                var contentObject = JsonConvert.DeserializeObject<List<ExpandoObject>>( contentObjectJSON, converter );
                mergeFields.Add( "ContentObject", contentObject );
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the content template.
        /// </summary>
        /// <returns></returns>
        public override string GetContentTemplate()
        {
            var template = this.GetAttributeValue( "Template" ) ?? string.Empty;
            var emailTemplate = this.GetAttributeValue( "EmailTemplate" ) ?? string.Empty;
            var textTemplate = this.GetAttributeValue( "TextTemplate" ) ?? string.Empty;
            var alternateEmailTemplate = this.GetAttributeValue( "AlternateEmailTemplate" ) ?? string.Empty;
            var alternateTextTemplate = this.GetAttributeValue( "AlternateTextTemplate" ) ?? string.Empty;
            template = template
                .Replace( "{{{{ EmailTemplate }}}}", emailTemplate )
                .Replace( "{{{{ TextTemplate }}}}", textTemplate )
                .Replace( "{{{{ AlternateEmailTemplate }}}}", alternateEmailTemplate )
                .Replace( "{{{{ AlternateTextTemplate }}}}", alternateTextTemplate );
            return template;
        }
    }
}