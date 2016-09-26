using Rock;
using Rock.Web.Cache;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace church.ccv.Badges.NextSteps
{
    [Description( "Displays the NextSteps Taken for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "NextSteps Taken" )]
    public class NextStepsTaken : Rock.PersonProfile.BadgeComponent
    {
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            writer.Write( string.Format( @"<div class='badge badge-stepstaken badge-icon badge-id-{0}' data-toggle='tooltip' data-original-title='{1} has not taken any steps'>
                    <i class='fa fa-road'></i><div class='steps-thisyear'></div><div class='steps-52weeks'></div>
            </div>", 
                badge.Id // 0
                , Person.NickName // 1
            ) );

            writer.Write( string.Format(
                    @"
                    <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/StepsTaken/{0}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var badge = $('.badge-stepstaken.badge-id-{1}');
                                        
                                        badge.find('.steps-thisyear').html(data.StepsThisYear);                                                
                                        badge.find('.steps-52weeks').html( ' / ' + data.StepsIn52Weeks);
                                        
                                        if ( data.StepsThisYear != 0 || data.StepsIn52Weeks != 0) {{
                                            var thisYearLabel = 'steps';
                                            var twelveMonthsLabel = 'steps';

                                            if (data.StepsThisYear == 1) {{
                                                thisYearLabel = 'step';
                                            }}

                                            if (data.StepsIn52Weeks == 1) {{
                                                twelveMonthsLabel = 'step';
                                            }}

                                            badge.attr('data-original-title', '{2} has taken ' + data.StepsThisYear + ' ' + thisYearLabel  + ' in {3} and ' + data.StepsIn52Weeks + ' ' + twelveMonthsLabel + ' in the last 12 months');
                                        }}
                                    }}
                                }},
                        }});
                    }});
                    </script>
                
                    ",
                     Person.Guid, // 0
                     badge.Id, // 1
                     Person.NickName, // 2
                     RockDateTime.Now.Year //3
                    ));
        }

        private int GetPageIdFromLinkedPageAttribute(string attributeKey, PersonBadgeCache badge )
        {
            int pageId = -1;

            Guid pageGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( badge, attributeKey ), out pageGuid ) )
            {
                pageId = Rock.Web.Cache.PageCache.Read( pageGuid ).Id;
            }

            return pageId;
        }
    }
}