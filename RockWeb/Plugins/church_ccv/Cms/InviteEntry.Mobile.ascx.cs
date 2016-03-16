using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Invite Entry - Mobile " )]
    [Category( "CCV > Cms" )]
    [Description( "Block that helps a user send an invite message to somebody using email, text, etc from a Mobile browser" )]

    [CodeEditorField( "ContentObject", "JSON Dynamic Array that can be used by the Template as a MergeField.", Rock.Web.UI.Controls.CodeEditorMode.JavaScript, order: 0,
defaultValue: @"[
  {
    ""Name"": ""Anthem"",
    ""ShortCode"": ""ATH"",
    ""Address1"": ""39905 N Gavilan Peak Pkwy"",
    ""Address2"": ""Anthem, AZ 85086"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""6:00 pm""
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
          ""10:30 am special"",
          ""12:00 pm""
        ]
      }
    ]
  },
  {
    ""Name"": ""Avondale"",
    ""ShortCode"": ""AVD"",
    ""Address1"": ""1565 N 113th Ave"",
    ""Address2"": ""Avondale, AZ 85392"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""6:00 pm""
        ]
      },
      {
        ""Date"": ""Saturday 3/26"",
        ""Times"": [
          ""4:30 pm special"",
          ""6:00 pm""
        ]
      },
      {
        ""Date"": ""Sunday 3/27"",
        ""Times"": [
          ""6:30 am sunrise"",
          ""9:30 am"",
          ""11:00 am special""
        ]
      }
    ]
  },
  {
    ""Name"": ""East Valley"",
    ""ShortCode"": ""EAV"",
    ""Address1"": ""1330 S Crismon Rd"",
    ""Address2"": ""Mesa, AZ 85209"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""6:00 pm""
        ]
      },
      {
        ""Date"": ""Saturday 3/26"",
        ""Times"": [
          ""5:00 pm special""
        ]
      },
      {
        ""Date"": ""Sunday 3/27"",
        ""Times"": [
          ""6:30 am sunrise"",
          ""9:30 am special"",
          ""11:00 am special""
        ]
      }
    ]
  },
  {
    ""Name"": ""Peoria"",
    ""ShortCode"": ""PEO"",
    ""Address1"": ""7007 W Happy Valley Rd"",
    ""Address2"": ""Peoria, AZ 85383"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""5:30 pm special"",
          ""7:00 pm special""
        ]
      },
      {
        ""Date"": ""Saturday 3/26"",
        ""Times"": [
          ""4:30 pm special"",
          ""6:00 pm special hearing""
        ]
      },
      {
        ""Date"": ""Sunday 3/27"",
        ""Times"": [
          ""6:30 am sunrise"",
          ""9:00 am special hearing"",
          ""10:30 am special"",
          ""12:00 pm special""
        ]
      }
    ]
  },
  {
    ""Name"": ""Scottsdale"",
    ""ShortCode"": ""SCO"",
    ""Address1"": ""19030 N Pima Rd"",
    ""Address2"": ""Scottsdale, AZ 85255"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""6:00 pm""
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
          ""9:30 am special"",
          ""11:00 am""
        ]
      }
    ]
  },
  {
    ""Name"": ""Surprise"",
    ""ShortCode"": ""SUR"",
    ""Address1"": ""14787 W Cholla St"",
    ""Address2"": ""Surprise, AZ 85379"",
    ""Services"": [
      {
        ""Date"": ""Friday 3/25"",
        ""Times"": [
          ""6:00 pm""
        ]
      },
      {
        ""Date"": ""Saturday 3/26"",
        ""Times"": [
          ""4:30 pm special"",
          ""6:00 pm""
        ]
      },
      {
        ""Date"": ""Sunday 3/27"",
        ""Times"": [
          ""6:30 am sunrise"",
          ""9:00 am special"",
          ""10:30 am special hearing"",
          ""12:00 pm""
        ]
      }
    ]
  }
]
" )]

    [CodeEditorField( "Template", "Lava template to render the content.  Use the special <pre>{{{{ EmailTemplate }}}}</pre>' and <pre>{{{{ TextTemplate }}}}</pre> to include the templates from the Email and Text templates", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 1,
defaultValue: @"
{% comment %}
CACHE KEYS:
Campus={{ Context.Campus.Guid }}|OSFamily={{ OSFamily }}
{% endcomment %}
<style>
  html, body {
    height: 100%;
    margin: 0;
  }
  .wrapper {
    position: relative;
  }
  article.page {
    position: absolute;
    width: 100%;
    padding-bottom: 60px;
  }
  .fade {
    z-index: -999;
    visibility: hidden;
  }
  .fade.in {
    z-index: 999;
    visibility: visible;
  }
</style>

{% assign trackEvents = false %}
{% assign GoogleCampaignName = 'Easter Page' %}
{% assign inviteBtnClasses = 'btn btn-default btn-block' %}
{% assign inviteAllBtnClasses = 'btn btn-default btn-block' %}
{% assign preUrl = 'external:' %}

{% assign item = ContentObject | Where: ""ShortCode"", Context.Campus.ShortCode | First %}

<div class=""wrapper"">

  <article id=""ChoosePage"" class=""page fade in"">
    <p>
      &nbsp;
    </p>
    <p class=""lead"">How would you like to send your invite?</p>
    <p><a href=""#EmailPage"" data-label=""Email Invite {{ Context.Campus.Name }}"" class=""btn btn-default btn-block js-change-page"">Email</a></p>
    <p><a href=""#SMSPage"" data-label=""SMS Invite {{ Context.Campus.Name }}"" class=""btn btn-default btn-block js-change-page"">Text Message</a></p>
  </article>

  <article id=""EmailPage"" class=""page fade"">
    <div class=""row margin-t-lg"">
      <div class=""col-xs-7"">
        <h4 class=""margin-t-none"">Email Invite</h4>
      </div>
      <div class=""col-xs-5 text-right"">
        <a href=""#ChoosePage"" data-label=""Back Button {{ Context.Campus.Name }}"" class=""js-change-page btn btn-default btn-xs""><i class=""fa fa-angle-left""></i> Back</a>
      </div>
    </div>

    <p class=""lead"">Which service are you attending?</p>

    {% for day in item.Services %}
      <h5>{{ day.Date }}</h5>
      <div class=""row"">
        {% for time in day.Times %}
          {% if time contains 'sunrise' %}
            {% assign hasSunrise = true %}
          {% endif %}
          {% if time contains 'special' %}
            {% assign hasSpecial = true %}
          {% endif %}
          {% if time contains 'hearing' %}
            {% assign hasHearing = true %}
          {% endif %}
          {% assign rawTime = time | Remove: 'sunrise' | Remove: ' special' | Remove: ' hearing' %}
          <div class=""col-xs-6 margin-b-lg"">{{{{ EmailTemplate }}}}</div>
        {% endfor %}
      </div>
    {% endfor %}
    <p>
      &nbsp;
    </p>
    {{{{ AlternateEmailTemplate }}}}

  </article>

  <article id=""SMSPage"" class=""page fade"">
    <div class=""row margin-t-lg"">
      <div class=""col-xs-7"">
        <h4 class=""margin-t-none"">Text Invite</h4>
      </div>
      <div class=""col-xs-5 text-right"">
        <a href=""#ChoosePage"" data-label=""Back Button {{ Context.Campus.Name }}"" class=""js-change-page btn btn-default btn-xs""><i class=""fa fa-angle-left""></i> Back</a>
      </div>
    </div>

    <p class=""lead"">Which service are you attending?</p>

    {% for day in item.Services %}
      <h5>{{ day.Date }}</h5>
      <div class=""row"">
        {% for time in day.Times %}
          {% assign rawTime = time | Remove: 'sunrise' | Remove: ' special' | Remove: ' hearing' %}
          <div class=""col-xs-6 margin-b-lg"">{{{{ TextTemplate }}}}</div>
        {% endfor %}
      </div>
    {% endfor %}
    <p>
      &nbsp;
    </p>
    {{{{ AlternateTextTemplate }}}}

  </article>

</div>


<script type=""text/javascript"">
  function trackEvent(action, label) {
    {% if trackEvents %}
      if (label) {
        ga('send', {
          hitType: 'event',
          eventCategory: '{{ GoogleCampaignName }}',
          eventAction: action,
          eventLabel: label
        });
      } else {
        ga('send', {
          hitType: 'event',
          eventCategory: '{{ GoogleCampaignName }}',
          eventAction: action
        });
      }
    {% else %}
      if (label) {
        console.log('{{ GoogleCampaignName }}, '+action+', '+label)
      } else {
        console.log('{{ GoogleCampaignName }}, '+action)
      }
    {% endif %}
  }

  $('body').on('click', '.js-track-sms', function(){
    var label = $(this).data('label')
    trackEvent('Text Invite', label)
  })
  $('body').on('click', '.js-track-email', function(){
    var label = $(this).data('label')
    trackEvent('Email Invite', label)
  })
  $('body').on('click', '.js-track-fb', function(){
    var label = $(this).data('label')
    trackEvent('Facebook Invite', label)
  })

  $('.js-change-page').click(function(e){
    e.preventDefault()
    var $this = $(this)
    var nextPage = $this.attr('href')
    $('.page.fade.in').removeClass('in')
    $(nextPage).addClass('in')
    trackEvent('Page View', $this.data('label'))
  })
</script>
" )]

    [CodeEditorField( "Email Template", "Lava template which will be used for the <pre>{{{{ EmailTemplate }}}}</pre> which can be used to create a mailto link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 2, required: false,
defaultValue: @"
{% assign inviteBtnClasses = inviteBtnClasses | Default: 'btn btn-default' %}

{% capture subject %}
Join me for Easter at CCV
{% endcapture %}

{% capture body %}I'm going to Easter service at CCV. Would you like to join me? I'll be attending the {{ rawTime }} service on {{ day.Date }} on the {{ item.Name }} campus. Let’s go together!

http://ccveaster.com
{% endcapture %}

<a class=""{{ inviteBtnClasses }} js-track-email"" data-label=""{{ item.Name }} {{ day.Date }} {{ rawTime }}"" href=""{{ preUrl }}mailto:?subject={{ subject | Trim | EscapeDataString }}&body={{ body | EscapeDataString }}"">{{ rawTime }}</a>
" )]

    [CodeEditorField( "Text Template", "Lava template which will be used for the <pre>{{{{ TextTemplate }}}}</pre> which can be used to create an SMS link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 3, required: false,
defaultValue: @"
{% assign inviteBtnClasses = inviteBtnClasses | Default: 'btn btn-default' %}

{% capture smsAll %}
Come to Easter at CCV with me! I'll be going to the {{ rawTime }} service on {{ day.Date }} at the {{ item.Name }} campus. You should join me! http://ccveaster.com
{% endcapture %}

{% case OSFamily %}
  {% when 'android' %}
    {% assign sep = '?' %}
  {% when 'ios' %}
    {% assign sep = '&' %}
  {% else %}
    {% assign sep = '&' %}
{% endcase %}

<a class=""{{ inviteBtnClasses }} js-track-sms"" data-label=""{{ item.Name }} {{ day.Date }} {{ rawTime }}"" href=""{{ preUrl }}sms:{{ sep }}body={{ smsAll | Trim | EscapeDataString }}"">{{ rawTime }}</a>
" )]

    [CodeEditorField( "Alternate Email Template", "Lava template which will be used for the <pre>{{{{ AlternateEmailTemplate }}}}</pre> which can be used to create a mailto link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 4, required: false,
defaultValue: @"
{% assign inviteAllBtnClasses = inviteAllBtnClasses | Default: 'btn btn-default btn-block' %}

{% capture subject %}
Join me for Easter at CCV
{% endcapture %}

{% capture body %}
I'm going to Easter service at CCV on the {{ item.Name }} Campus. Would you like to join me? Check out the service times at http://ccveaster.com and let's plan to go together!

{% for day in item.Services %}{{ day.Date }}{% for time in day.Times %}{% assign _time = time %}{% if time contains 'sunrise' %}{% assign _time = _time | Replace: 'sunrise','(Sunrise Service)' %}{% endif %}{% if time contains 'special' %}{% assign _time = _time | Replace: 'special','*' %}{% endif %}{% if time contains 'hearing' %}{% assign _time = _time | Replace: ' hearing','*' %}{% endif %}
- {{ _time }}{% endfor %}

{% endfor %}{% if hasSpecial and hasHearing %}* Special needs services available
** Sign language translation & special needs services available{% elsif hasSpecial %}* Special needs services available{% endif %}
{% endcapture %}

<a class=""{{ inviteAllBtnClasses }} js-track-email"" data-label=""{{ item.Name }} Don't know yet"" href=""{{ preUrl }}mailto:?subject={{ subject | Trim | EscapeDataString }}&body={{ body | Trim | EscapeDataString }}"">I don't know yet</a>
" )]

    [CodeEditorField( "Alternate Text Template", "Lava template which will be used for the <pre>{{{{ AlternateTextTemplate }}}}</pre> which can be used to create an SMS link.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 5, required: false,
defaultValue: @"
{% assign inviteAllBtnClasses = inviteAllBtnClasses | Default: 'btn btn-default btn-block' %}

{% capture smsAll %}
I'm going to Easter service at CCV on the {{ item.Name }} campus. Would you like to join me? Check out the service times at http://ccveaster.com and let's plan to go together!
{% endcapture %}

{% case OSFamily %}
  {% when 'android' %}
    {% assign sep = '?' %}
  {% when 'ios' %}
    {% assign sep = '&' %}
  {% else %}
    {% assign sep = '&' %}
{% endcase %}

<a class=""{{ inviteAllBtnClasses }} js-track-sms"" data-label=""{{ item.Name }} Don't know yet"" href=""{{ preUrl }}sms:{{ sep }}body={{ smsAll | Trim | EscapeDataString }}"">I don't know yet</a>
" )]
    public partial class InviteEntryMobile : church.ccv.Utility.Web.BaseContentBlock
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
