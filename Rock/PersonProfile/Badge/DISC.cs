﻿// <copyright>
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using System.Diagnostics;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// DISC Badge as implemented from http://www.gregwiens.com/scid/ assessment template.
    /// </summary>
    [Description( "Bade that displays a person's DISC results" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "DISC" )]

    public class DISC : BadgeComponent
    {
        int maxScoreValue = 35;
        
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            //Grab the DISC Scores
            bool isValidDiscScore = true;
            int discStrength = 0;
            int?[] discScores = new int?[] { Person.GetAttributeValue( "NaturalD" ).AsIntegerOrNull(), Person.GetAttributeValue( "NaturalI" ).AsIntegerOrNull(), Person.GetAttributeValue( "NaturalS" ).AsIntegerOrNull(), Person.GetAttributeValue( "NaturalC" ).AsIntegerOrNull() };

            //Validate the DISC Scores, find the strength
            for ( int i = 0; i < discScores.Length; i++ )
            {
                //Does the scores have values?
                if ( !discScores[i].HasValue )
                {
                    isValidDiscScore = false;
                }
                else
                {
                    //Are the scores valid values?
                    if ( ( discScores[i].Value < 0 ) || ( discScores[i].Value > 100 ) )
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

            //Create the badge
            if ( isValidDiscScore )
            {
                //Find the Strength
                String description;
                switch ( discStrength )
                {
                    case 0:
                        description = "D: is bottom line oriented, makes quick decisions, wants direct answers.";
                        break;
                    case 1:
                        description = "I: very people oriented, has a lot of friends, wants opportunity to talk.";
                        break;
                    case 2:
                        description = "S: does not like change, wants limited responsibility and sincere appreciation.";
                        break;
                    case 3:
                        description = "C: is detail oriented, wants no sudden changes, won't make decision.";
                        break;
                    default:
                        description = "DISC Results Summary";
                        break;
                }

                //Badge HTML
                writer.Write( String.Format( "<div class='badge badge-disc badge-id-{0}' data-original-title='{1}'>", badge.Id, description ) );
                writer.Write( "<ul class='badge-disc-chart list-unstyled'>" );
                writer.Write( string.Format( "<li class='badge-disc-d {1}' title='D'><span style='height:{0}%'></span></li>", NormalizeResult(discScores[0].Value), (discStrength == 0) ? "badge-disc-primary" : String.Empty ) );
                writer.Write( string.Format( "<li class='badge-disc-i {1}' title='I'><span style='height:{0}%'></span></li>", NormalizeResult(discScores[1].Value), (discStrength == 1) ? "badge-disc-primary" : String.Empty ) );
                writer.Write( string.Format( "<li class='badge-disc-s {1}' title='S'><span style='height:{0}%'></span></li>", NormalizeResult(discScores[2].Value), ( discStrength == 2 ) ? "badge-disc-primary" : String.Empty ) );
                writer.Write( string.Format( "<li class='badge-disc-c {1}' title='C'><span style='height:{0}%'></span></li>", NormalizeResult(discScores[3].Value), ( discStrength == 3 ) ? "badge-disc-primary" : String.Empty ) );
                writer.Write( "</ul></div>" );
            }
        }

        private int NormalizeResult( int result )
        {
            return (int)Math.Floor( (double)result / (double)maxScoreValue * 100 );
        }
    }
}
