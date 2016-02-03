using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;

namespace church.ccv.Badges.Person
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description( "Displays the Steps Bar for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Steps Bar" )]

    [LinkedPage("Baptism Registration Page", "The page to link to for registering for baptism.")]
    [IntegerField("Baptism Event Id", "The event id to use for pulling upcoming baptisms.")]
    public class StepsBar : Rock.PersonProfile.BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            int baptismRegistrationPageId = -1;

            Guid baptismPageGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( badge, "BaptismRegistrationPage" ), out baptismPageGuid ) )
            {
                baptismRegistrationPageId = Rock.Web.Cache.PageCache.Read( baptismPageGuid ).Id;
            }

            writer.Write( string.Format( @"<div class='badge-group badge-group-steps js-badge-group-steps badge-id-{0}'>
                <a class='badge badge-baptism badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not baptised.' data-container='body' href='/page/{4}?PersonGuid={2}&EventItemId={3}'>
                    <i class='icon ccv-baptism'></i>
                </a>

                <div class='badge badge-membership badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not a member.' data-container='body'>
                    <i class='icon ccv-membership'></i>
                </div>
                <div class='badge badge-worship badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not an eRA.' data-container='body'>
                    <i class='icon ccv-worship'></i>
                </div>
                <div class='badge badge-connect badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not connected to a neighborhood group.' data-container='body'>
                    <i class='icon ccv-connect'></i>
                </div>
                <div class='badge badge-tithe badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not tithing.' data-container='body'>
                    <i class='icon ccv-tithe'></i>
                </div>
                <div class='badge badge-serve badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not serving.' data-container='body'>
                    <i class='icon ccv-serve'></i>
                </div>
                <div class='badge badge-share badge-icon step-nottaken' data-toggle='tooltip' data-original-title='Coming Soon...' data-container='body'>
                    <i class='icon ccv-share'></i>
                </div>
                <div class='badge badge-coach badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not coaching.' data-container='body'>
                    <i class='icon ccv-coach'></i>
                </div>
            </div>", 
                badge.Id // 0
                , Person.NickName // 1
                , Person.Guid // 2
                , GetAttributeValue(badge, "BaptismEventId") // 3
                , baptismRegistrationPageId // 4
            ) );
            
            writer.Write(string.Format("<script src='{0}' type='text/javascript'></script>", System.Web.VirtualPathUtility.ToAbsolute( "~/Plugins/church_ccv/Badges/Scripts/steps-badge.js" ) ) );

            /*writer.Write( string.Format(
                @"
                    <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/StepsBar/{0}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        
                                        var $badge = $('.js-badge-group-steps.badge-id-{1}');

                                        

                                        alert('IsWorshipper' + data.IsWorshipper);

                                        $.each(data, function() {{
                                            //alert('IsWorshipper' + this.IsWorshipper);
                                        }});

                                    }}
                                }},
                        }});
                    }});
                    </script>",
                 Person.Guid.ToString(),
                 badge.Id ) );*/
        }
    }
}