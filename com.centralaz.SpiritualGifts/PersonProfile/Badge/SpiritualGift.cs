// <copyright>
// Copyright by Central Christian Church
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using System.Diagnostics;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.PersonProfile.Badge
{
    /// <summary>
    /// Spiritual Gift Badge as implemented from http://positivepublications.com/ assessment template.
    /// </summary>
    [Description( "Bade that displays a person's spiritual gift results" )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "SpiritualGift" )]

    [LinkedPage( "Spiritual Gift Result Detail", "Page to show the details of the Spiritual Gift assessment results. If blank no link is created.", false )]
    public class SpiritualGift : Rock.PersonProfile.BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {

            // Find the Spiritual Gift Personality Type / Strength
            String description = string.Empty;
            string gifting = Person.GetAttributeValue( "Gifting" );
            DefinedValueCache giftingValue = null;
            if ( !string.IsNullOrEmpty( gifting ) )
            {
                giftingValue = DefinedTypeCache.Read( com.centralaz.SpiritualGifts.SystemGuid.DefinedType.SPRITUAL_GIFTS_DEFINED_TYPE.AsGuid() ).DefinedValues.Where( v => v.Value == gifting ).FirstOrDefault();
                if ( giftingValue != null )
                {
                    description = giftingValue.Description;
                }
            }

            // create url for link to details if configured
            string detailPageUrl = string.Empty;
            if ( !String.IsNullOrEmpty( GetAttributeValue( badge, "SpiritualGiftResultDetail" ) ) )
            {
                int pageId = Rock.Web.Cache.PageCache.Read( Guid.Parse( GetAttributeValue( badge, "SpiritualGiftResultDetail" ) ) ).Id;
                detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( String.Format( "~/page/{0}?Person={1}", pageId, Person.UrlEncodedKey ) );
            }

            //Determine icon
            string iconClass = string.Empty;
            if ( giftingValue != null )
            {
                switch ( giftingValue.Value )
                {
                    case "Prophecy":
                        iconClass = "badge-icon fa fa-cloud-download";
                        break;
                    case "Ministry":
                        iconClass = "badge-icon fa fa-globe";
                        break;
                    case "Teaching":
                        iconClass = "badge-icon fa fa-compass";
                        break;
                    case "Encouragement":
                        iconClass = "badge-icon fa fa-fire";
                        break;
                    case "Giving":
                        iconClass = "badge-icon fa fa-diamond";
                        break;
                    case "Leadership":
                        iconClass = "badge-icon fa fa-bullhorn";
                        break;
                    case "Mercy":
                        iconClass = "badge-icon fa fa-life-ring";
                        break;
                }

                //Badge HTML
                writer.Write( "<div class='badge' data-toggle='tooltip' data-original-title='Spiritual Gift is {0}' ><a href='{1}'>", giftingValue.Value, !String.IsNullOrEmpty( detailPageUrl ) ? detailPageUrl  : "#" );
                writer.Write( String.Format( "<i class='{0}'></i>", iconClass ) );
                writer.Write( "</a></div>" );
            }
        }

    }
}

