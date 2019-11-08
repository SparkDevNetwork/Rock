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
    public partial class Rollup_1107 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateGoogleStaticMapLavaShortCode();
            MoveNamelessPersonBlock();
            UpdateParralaxShortcode();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

                /// <summary>
        /// Update the Google Static Map Lava Shortcode with new features and documentation typo corrections.
        /// </summary>
        private void UpdateGoogleStaticMapLavaShortCode()
        {
            // update style parameter handling
            string lavaTemplate = @"{% assign url = url | Append:'&style=' | Append:style %}";

            string newLavaTemplate = @"{% assign styleItems = style | Split:',' -%}
        {% for styleItem in styleItems -%}
            {% assign url = url | Append:'&style=' | Append:styleItem %}
            {% endfor -%}";

            SqlUpdateShortCode( lavaTemplate, newLavaTemplate, "Markup", "2DD53FE6-6EB2-4EC8-A965-3F71054F7983" );

            // update styles documentation
            string documentation = "The returned map can be styled in an infinite (or close to) number of ways. See the Google Static Map documentation";
            string newDocumentation = "The returned map can be styled in an infinite (or close to) number of ways. Use commas to delimit multiple styles. See the Google Static Map documentation";
            SqlUpdateShortCode( documentation, newDocumentation, "Documentation", "2DD53FE6-6EB2-4EC8-A965-3F71054F7983" );

            // update typo in the apikey warning message
            SqlUpdateShortCode(
                "There is not Google API key defined. Please add your key under",
                "There is no Google API key defined. Please add your key under",
                "Markup",
                "2DD53FE6-6EB2-4EC8-A965-3F71054F7983" );

            // update marker handling to add new 'precision' feature
            string existingLavaSnippet = @"{% assign markerContent = markerContent | Append:'|' | Append:marker.location | Trim -%}";

            string newLavaSnippet = @"{% comment %}
    // If given, handle adjusting the precision on a given lat and long 
    {% endcomment %}
    {% assign mLocation = marker.location %}
    {% if marker.precision and marker.precision != '' %}
        {% capture precision %}0.{% for i in (1..marker.precision) %}0{% endfor %}{% endcapture %}

        {% assign latLong = marker.location | Split:',' %}
        {% assign latLongSize = latLong | Size %}
        {% if latLongSize == 2 %}
            {% capture mLocation %}{{ latLong[0] | AsDecimal | Format:precision }},{{ latLong[1] | AsDecimal | Format:precision }}{% endcapture %}
        {% endif %}
    {% endif %}
    {% assign markerContent = markerContent | Append:'|' | Append:mLocation | Trim -%}";

            SqlUpdateShortCode( existingLavaSnippet, newLavaSnippet, "Markup", "2DD53FE6-6EB2-4EC8-A965-3F71054F7983" );

            // update documentation with new 'precision' feature
            var existingLocationDocumentation = "<li><strong>location</strong> – The location (address) of the pin.</li>";
            var newLocationDocumentation = @"<li><strong>location</strong> – The location (address) of the pin as either an address or lat,long pair.</li>
    <li><strong>precision</strong> (optional for use with lat/long location) - Used to reduce the number of decimal places of the location lat/long to obscure the exact position.</li>
";
            SqlUpdateShortCode( existingLocationDocumentation, newLocationDocumentation, "Documentation", "2DD53FE6-6EB2-4EC8-A965-3F71054F7983" );

        }

        /// <summary>
        /// Creates the needed SQL to update a Shortcode record's given column using the matching existing string/value
        /// and the new string/value.  Be mindful to write your existingString and newString so that it is run-twice-safe.
        /// </summary>
        /// <param name="existingString">The existing string.</param>
        /// <param name="newString">The new string.</param>
        /// <param name="column">The column.</param>
        /// <param name="shortcodeGuid">The shortcode unique identifier.</param>
        private void SqlUpdateShortCode( string existingString, string newString, string column, string shortcodeGuid )
        {
            existingString = existingString.Replace( "'", "''" );
            newString = newString.Replace( "'", "''" );
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( $"{column}" );
            Sql( $@"
            UPDATE [LavaShortcode] 
            SET [{column}] =  REPLACE( {targetColumn}
                ,'{existingString}'
                ,'{newString}' )
            WHERE [Guid]='{shortcodeGuid}' AND {targetColumn} NOT LIKE '%{newString}%'"
           );
        }

        /// <summary>
        /// SK: Move NamelessPerson Block Under the CRM directory
        /// </summary>
        private void MoveNamelessPersonBlock()
        {
            RockMigrationHelper.UpdateBlockTypeByGuid( "Nameless Person List", "List unmatched phone numbers with an option to link to a person that has the same phone number.", "~/Blocks/Crm/NamelessPersonList.ascx", "CRM", "41AE0574-BE1E-4656-B45D-2CB734D1BE30" );
        }

        /// <summary>
        /// GJ: Update Parallax Shortcode to Use Latest JS Libraries
        /// </summary>
        private void UpdateParralaxShortcode()
        {
            Sql( @"
                UPDATE [LavaShortcode] 
                SET [Markup] = N'{{ ''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.11.1/jarallax.min.js'' | AddScriptLink }}
{% if videourl != '''' -%}
    {{ ''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.11.1/jarallax-video.min.js'' | AddScriptLink }}
{% endif -%}

{% assign id = uniqueid -%} 
{% assign bodyZindex = zindex | Plus:1 -%}

{% assign speed = speed | AsInteger %}

{% if speed > 0 -%}
    {% assign speed = speed | Times:''.01'' -%}
    {% assign speed = speed | Plus:''1'' -%}
{% elseif speed == 0 -%}
    {% assign speed = 1 -%}
{% else -%}
    {% assign speed = speed | Times:''.02'' -%}
    {% assign speed = speed | Plus:''1'' -%}
{% endif -%}


 
{% if videourl != ''''- %}
    <div id=""{{ id }}"" class=""jarallax"" data-jarallax-video=""{{ videourl }}"" data-type=""{{ type }}"" data-speed=""{{ speed }}"" data-img-position=""{{ position }}"" data-object-position=""{{ position }}"" data-background-position=""{{ position }}"" data-zindex=""{{ bodyZindex }}"" data-no-android=""{{ noandroid }}"" data-no-ios=""{{ noios }}"">
{% else- %} 
    <div id=""{{ id }}"" data-jarallax class=""jarallax"" data-type=""{{ type }}"" data-speed=""{{ speed }}"" data-img-position=""{{ position }}"" data-object-position=""{{ position }}"" data-background-position=""{{ position }}"" data-zindex=""{{ bodyZindex }}"" data-no-android=""{{ noandroid }}"" data-no-ios=""{{ noios }}"">
        <img class=""jarallax-img"" src=""{{ image }}"" alt="""">
{% endif -%}

        {% if blockContent != '''' -%}
            <div class=""parallax-content"">
                {{ blockContent }}
            </div>
        {% else- %}
            {{ blockContent }}
        {% endif -%}
    </div>

{% stylesheet %}
#{{ id }} {
    /* eventually going to change the height using media queries with mixins using sass, and then include only the classes I want for certain parallaxes */
    min-height: {{ height }};
    background: transparent;
    position: relative;
    z-index: 0;
}

#{{ id }} .jarallax-img {
    position: absolute;
    object-fit: cover;
    /* support for plugin https://github.com/bfred-it/object-fit-images */
    font-family: ''object-fit: cover;'';
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}

#{{ id }} .parallax-content{
    display: inline-block;
    margin: {{ contentpadding }};
    color: {{ contentcolor }};
    text-align: {{ contentalign }};
	width: 100%;
}
{% endstylesheet %}' 
            WHERE ([Guid]='4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4')" );
        }
    }
}
