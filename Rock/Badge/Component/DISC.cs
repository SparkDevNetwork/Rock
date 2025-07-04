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
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;

using Rock.Attribute;
#if REVIEW_NET5_0_OR_GREATER
using Rock.Configuration;
#endif
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// DISC Badge as implemented from http://www.gregwiens.com/scid/ assessment template.
    /// </summary>
    [Description( "Bade that displays a person's DISC results" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "DISC" )]

    [LinkedPage( "DISC Result Detail", "Page to show the details of the DISC assessment results. If blank no link is created.", false )]
    [Rock.SystemGuid.EntityTypeGuid( "6D29DB44-EDC5-42AA-B42C-482BC0920AD0")]
    public class DISC : BadgeComponent
    {
        /// <summary>
        /// The max value of a Natural DISC score.
        /// </summary>
        private const int MAX = 100;

        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person person ) )
            {
                return;
            }

            // Grab the DISC Scores
            bool isValidDiscScore = true;
            int discStrength = 0;
            int?[] discScores = new int?[] { person.GetAttributeValue( "NaturalD" ).AsIntegerOrNull(), person.GetAttributeValue( "NaturalI" ).AsIntegerOrNull(), person.GetAttributeValue( "NaturalS" ).AsIntegerOrNull(), person.GetAttributeValue( "NaturalC" ).AsIntegerOrNull() };

            // Validate the DISC Scores, find the strength
            for ( int i = 0; i < discScores.Length; i++ )
            {
                // Does the scores have values?
                if ( !discScores[i].HasValue )
                {
                    isValidDiscScore = false;
                }
                else
                {
                    // Are the scores valid values?
                    if ( ( discScores[i].Value < 0 ) || ( discScores[i].Value > MAX ) )
                    {
                        isValidDiscScore = false;
                    }
                    else
                    {
                        if ( discScores[i].Value > discScores[discStrength].Value )
                        {
                            discStrength = i;
                        }
                    }
                }
            }

            // Create the badge
            if ( isValidDiscScore )
            {
                // Find the DISC Personality Type / Strength
                String description = string.Empty;
                string personalityType = person.GetAttributeValue( "PersonalityType" );
                if ( !string.IsNullOrEmpty( personalityType ) )
                {
                    var personalityValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE.AsGuid() ).DefinedValues.Where( v => v.Value == personalityType ).FirstOrDefault();
                    if ( personalityValue != null )
                    {
                        description = personalityValue.Description;
                    }
                }

                // create url for link to details if configured
                string detailPageUrl = string.Empty;
                if ( !String.IsNullOrEmpty( GetAttributeValue( badge, "DISCResultDetail" ) ) )
                {
                    int pageId = PageCache.Get( Guid.Parse( GetAttributeValue( badge, "DISCResultDetail" ) ) ).Id;
#if REVIEW_WEBFORMS
                    detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( String.Format( "~/page/{0}?Person={1}", pageId, person.UrlEncodedKey ) );
#else
                    detailPageUrl = RockApp.Current.ResolveRockUrl( String.Format( "~/page/{0}?Person={1}", pageId, person.UrlEncodedKey ) );
#endif
                    writer.Write( "<a href='{0}'>", detailPageUrl );
                }

                //Badge HTML
                writer.Write( String.Format( "<div class='rockbadge rockbadge-disc rockbadge-id-{0}' data-toggle='tooltip' data-original-title='{1}'>", badge.Id, description ) );
                writer.Write( "<ul class='rockbadge-disc-chart list-unstyled'>" );
                writer.Write( string.Format( "<li class='rockbadge-disc-d {1}' title='D'><span style='height:{0}%'></span></li>", Math.Floor( ( double ) ( ( double ) discScores[0].Value / ( double ) MAX ) * 100 ), ( discStrength == 0 ) ? "rockbadge-disc-primary" : String.Empty ) );
                writer.Write( string.Format( "<li class='rockbadge-disc-i {1}' title='I'><span style='height:{0}%'></span></li>", Math.Floor( ( double ) ( ( double ) discScores[1].Value / ( double ) MAX ) * 100 ), ( discStrength == 1 ) ? "rockbadge-disc-primary" : String.Empty ) );
                writer.Write( string.Format( "<li class='rockbadge-disc-s {1}' title='S'><span style='height:{0}%'></span></li>", Math.Floor( ( double ) ( ( double ) discScores[2].Value / ( double ) MAX ) * 100 ), ( discStrength == 2 ) ? "rockbadge-disc-primary" : String.Empty ) );
                writer.Write( string.Format( "<li class='rockbadge-disc-c {1}' title='C'><span style='height:{0}%'></span></li>", Math.Floor( ( double ) ( ( double ) discScores[3].Value / ( double ) MAX ) * 100 ), ( discStrength == 3 ) ? "rockbadge-disc-primary" : String.Empty ) );
                writer.Write( "</ul></div>" );

                if ( !String.IsNullOrEmpty( detailPageUrl ) )
                {
                    writer.Write( "</a>" );
                }
            }
            else
            {
                var rockContext = new RockContext();
                var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.DISC.AsGuid() );
                var lastRequestDate = new AssessmentService( rockContext )
                   .Queryable()
                   .AsNoTracking()
                   .Where( a => a.PersonAlias != null
                                && a.PersonAlias.PersonId == person.Id
                                && a.Status == AssessmentRequestStatus.Pending
                                && a.AssessmentTypeId == assessmentType.Id
                                && a.RequestedDateTime.HasValue )
                   .Select( a => a.RequestedDateTime )
                   .OrderByDescending( a => a )
                   .FirstOrDefault();

                bool recentRequest = lastRequestDate.HasValue && lastRequestDate.Value > ( RockDateTime.Now.AddDays( -30 ) );

                if ( recentRequest )
                {
                    writer.Write( String.Format( "<div class='rockbadge rockbadge-disc rockbadge-id-{0}' data-toggle='tooltip' data-original-title='A DISC request was made on {1}'>", badge.Id, lastRequestDate.Value.ToShortDateString() ) );
                    writer.Write( "<ul class='rockbadge-disc-chart list-unstyled'>" );
                    writer.Write( string.Format( "<li class='badge-disc-d badge-disc-disabled' title='D'><span style='height:{0}%'></span></li>", 80 ) );
                    writer.Write( string.Format( "<li class='badge-disc-i badge-disc-disabled' title='I'><span style='height:{0}%'></span></li>", 20 ) );
                    writer.Write( string.Format( "<li class='badge-disc-s badge-disc-disabled' title='S'><span style='height:{0}%'></span></li>", 60 ) );
                    writer.Write( string.Format( "<li class='badge-disc-c badge-disc-disabled' title='C'><span style='height:{0}%'></span></li>", 10 ) );
                    writer.Write( "</ul><div class='requested'>R</div></div>" );
                }
            }
        }
    }
}
