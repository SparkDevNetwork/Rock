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

using System;
using System.Collections.Generic;

using Rock.Model;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 211, "1.16.4" )]
    public class MigrationRollupsForV17_0_17 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateGoogleMapsLavaShortcodeUp();
            UpdateMetricsDateComponentUp();
            UpdateChartShortCodeLava();
            ChopBlocksUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateGoogleMapsLavaShortcodeDown();
        }

        #region KA: Migration to update Google Maps Lavashortcode

        private void UpdateGoogleMapsLavaShortcodeUp()
        {
            // Update Shortcode
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ ""https://maps.googleapis.com/maps/api/js?libraries=marker&key="" | Append:apiKey }}' %}{% endjavascript %}

{% case markeranimation %}
{% when 'drop' %}
    {% assign markeranimation = 'drop' %}
{% when 'bounce' %}
    {% assign markeranimation = 'bounce' %}
{% else %}
    {% assign markeranimation = null %}
{% endcase %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}

.drop {
  animation: drop 0.3s linear forwards .5s;
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}

.bounce {
  animation: bounce 2s infinite;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %},
	        mapId: '{{ mapId }}'
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
	        var glyph = null;
            if (markers{{ id }}[i][4] != ''){
            	glyph = markers{{ id }}[i][4];
            }
            var pin = new google.maps.marker.PinElement({
                background: '#FE7569',
                borderColor: '#000',
                scale: 1,
                glyph: glyph
            });
            marker = new google.maps.marker.AdvancedMarkerElement({
                position: position,
                map: map,
                title: markers{{ id }}[i][2],
                content: pin.element
            });

	        const content = marker.content;

    	    {% if markeranimation -%}
            // Drop animation should be onetime so remove class once animation ends.
		        {% if markeranimation == 'drop' -%}
                    content.style.opacity = ""0"";
		            content.addEventListener('animationend', (event) => {
                        content.classList.remove('{{ markeranimation }}');
                        content.style.opacity = ""1"";
                    });
                {% endif -%}
                content.classList.add('{{ markeranimation }}');
            {% endif -%}

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );

            // Add MapId attribute
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                null,
                null,
                "Google Maps Id",
                "The map identifier that's associated with a specific map style or feature on your google console, when you reference a map ID, its associated map style is displayed in your map.",
                0,
                "DEFAULT_MAP_ID",
                "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8",
                "core_GoogleMapId" );
        }

        private void UpdateGoogleMapsLavaShortcodeDown()
        {
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ ""https://maps.googleapis.com/maps/api/js?key="" | Append:apiKey }}' %}{% endjavascript %}

{% case markeranimation %}
{% when 'drop' %}
    {% assign markeranimation = 'google.maps.Animation.DROP' %}
{% when 'bounce' %}
    {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
{% else %}
    {% assign markeranimation = 'null' %}
{% endcase %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;

        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            marker = new google.maps.Marker({
                position: position,
                map: map,
                animation: {{ markeranimation }},
                title: markers{{ id }}[i][2],
                icon: markers{{ id }}[i][4]
            });

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";
            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );

            RockMigrationHelper.DeleteAttribute( "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8" );
        }

        #endregion

        #region KA: Migration to Update Metrics to use date component of DateTime for comparisons

        private void UpdateMetricsDateComponentUp()
        {
            // TOTAL WEEKEND ATTENDANCE
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
	INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
	INNER JOIN [Group] g ON g.Id = oa.[GroupId]
	INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
	gt.[AttendanceCountsAsWeekendService] = 1
	AND a.[DidAttend] = 1
	AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]'
WHERE [Guid] = '89553EEE-91F3-4169-9D7C-04A17471E035'" );

            // VOLUNTEER ATTENDANCE
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @ServiceAreaDefinedValueId INT = (SELECT Id FROM dbo.[DefinedValue] WHERE [Guid] = ''36A554CE-7815-41B9-A435-93F3D52A2828'')

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[GroupTypePurposeValueId] = @ServiceAreaDefinedValueId
   AND a.[DidAttend] = 1 
   AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]'
WHERE [Guid] = '4F965AE3-D455-4346-988F-2A2B5E236C0C'" );

            // PRAYER REQUESTS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as PrayerRequests, pr.[CampusId]
FROM dbo.[PrayerRequest] pr
WHERE
   pr.[IsActive] = 1
   AND CONVERT(date, pr.[CreatedDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL pr.[CampusId]'
WHERE [Guid] = '2B5ECA35-47D8-4690-A8AD-72488485F2B4'" );

            // PRAYERS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT  COUNT(*) as Prayers, p.[PrimaryCampusId]
FROM dbo.[Interaction] i 
INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
INNER JOIN [InteractionChannel] ichan ON ichan.[Id] = ic.[InteractionChannelId]
INNER JOIN [PrayerRequest] pr ON pr.[Id] = ic.[EntityId]
INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
WHERE 
   ichan.[Guid] = ''3D49FB99-94D1-4F63-B1A2-30D4FEDE11E9''
   AND i.[Operation] = ''Prayed''
   AND CONVERT(date, i.[InteractionDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]'
WHERE [Guid] = '685B7912-CB17-473B-90C1-2804F221931C'" );

            // BAPTISMS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME= DATEADD(mm, DATEDIFF(mm, 0, dbo.RockGetDate()), 0)
DECLARE @ENDDATE DATETIME = DATEADD(DAY, -1, DATEADD(mm, 1, @STARTDATE));
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''D42763FA-28E9-4A55-A25A-48998D7D7FEF'')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) >= @STARTDATE
AND CONVERT(date, av.[ValueAsDateTime]) < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]'
WHERE [Guid] = '8B63D9D5-A82D-49D4-9AED-2EDBCF60FDEE'" );

            // GIVING
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = '-- =====================================================================================================
-- Description: This metric represents weekly giving to the tithe and should be partitioned by Campus.
-- =====================================================================================================
-- You can edit this to match the TOP LEVEL financial accounts that are considered part of the ''tithe'', but please
-- do not change the remainder of this script:

-- Let them be fetched dynamically like this: 
DECLARE @Accounts VARCHAR(512); 
DECLARE @Date DATETIME = CONVERT(date, dbo.RockGetDate());

SELECT @Accounts = COALESCE(@Accounts + '','', '''') + CAST([Id]  as NVARCHAR(50)) 
FROM [FinancialAccount]
WHERE [IsTaxDeductible] = 1
   AND [ParentAccountId] IS NULL
   AND [IsActive] = 1
   AND (
        ([StartDate] IS NOT NULL AND [EndDate] IS NOT NULL AND @Date BETWEEN CONVERT(date, [StartDate]) AND CONVERT(date, [EndDate]))
		OR ([EndDate] IS NOT NULL AND @Date < CONVERT(date, [EndDate]))
        OR ([StartDate] IS NOT NULL AND @Date > CONVERT(date, [StartDate]))
        OR ([StartDate] IS NULL AND [EndDate] IS NULL)
	)

-- OR 
-- You can manually set them like this as a comma separated list of accounts.
-- (NOTE: Their child accounts will be included):
-- DECLARE @Accounts VARCHAR(100) = ''1,2,3'';
 
-------------------------------------------------------------------------------------------------------
DECLARE @STARTDATE int = FORMAT( DATEADD(DAY, -7, dbo.RockGetDate()), ''yyyyMMdd'' )
DECLARE @ENDDATE int = FORMAT( dbo.RockGetDate(), ''yyyyMMdd'' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )
DECLARE @AccountsWithChildren TABLE (Id INT);
-- Recursively get accounts and their children.
WITH AccountHierarchy AS (
    SELECT [Id]
    FROM dbo.[FinancialAccount] fa
    WHERE [Id] IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@Accounts, '',''))
    UNION ALL
    SELECT e.[Id]
    FROM dbo.[FinancialAccount] e
    INNER JOIN AccountHierarchy ah ON e.[ParentAccountId] = ah.[Id]
)
INSERT INTO @AccountsWithChildren SELECT * FROM AccountHierarchy;

;WITH CTE AS (
    SELECT
	fa.[Name] AS AccountName
    , fa.[CampusId] AS AccountCampusId
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @STARTDATE AND asd.[DateKey] <= @ENDDATE
		AND fa.[Id] IN (SELECT * FROM @AccountsWithChildren)
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY fa.[CampusId], fa.[Name], [PrimaryFamilyId]
)
SELECT
    SUM([GivingAmount]) AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
FROM CTE
GROUP BY [AccountCampusId];'
WHERE [Guid] = '43338e8a-622a-4195-b153-285e570b229d'" );

            // eRA WEEKLY LOSSES
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @EraEndDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''4711D67E-7526-9582-4A8E-1CD7BBE1B3A2'')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''CE5739C5-2156-E2AB-48E5-1337C38B935E'')

SELECT COUNT(*) as eraLosses, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraEndDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 0
)
GROUP BY ALL p.[PrimaryCampusId]'
WHERE [Guid] = '16A3FF64-31F0-4CFF-B5F4-83EEB69E0C25'" );

            // eRA WEEKLY WINS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @EraStartDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''A106610C-A7A1-469E-4097-9DE6400FDFC2'')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''CE5739C5-2156-E2AB-48E5-1337C38B935E'')

SELECT COUNT(*) as eraWins, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraStartDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 1
)
GROUP BY ALL p.[PrimaryCampusId];'
WHERE [Guid] = 'D05D685A-9A88-4375-A563-70BB44FBD237'" );
        }

        #endregion

        #region PA: Migration to update the Chart Lava Short Code

        // PA: Update the Lava for Chart Short Code to include ItemClickUrl setting to the dataitem configuration item.
        private void UpdateChartShortCodeLava()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Documentation]=N'<p>
    Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The 
    chart shortcode allows anyone to create charts with just a few lines of Lava. There are two modes for 
    creating a chart. The first ‘simple’ mode creates a chart with a single series. This option will suffice 
    for most of your charting needs. The second ‘series’ option allows you to create charts with multiple 
    series. Let’s look at each option separately starting with the simple option.
</p>
<h4>Simple Mode</h4>
<p>
    Let’s start by jumping to an example. We’ll then talk about the various configuration options, deal?
</p>
<pre>{[ chart type:''bar'' ]}
    [[ dataitem label:''Small Groups'' value:''45'' ]] [[ enddataitem ]]
    [[ dataitem label:''Serving Groups'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''General Groups'' value:''34'' ]] [[ enddataitem ]]
    [[ dataitem label:''Fundraising Groups'' value:''12'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>
    
<p>    
    As you can see this sample provides a nice-looking bar chart. The shortcode defines the chart type (several other 
    options are available). The [[ dataitem ]] configuration item defines settings for each bar/point on the chart. Each 
    has the following settings:
</p>
<ul>
    <li><strong>label</strong> – The label for the data item.</li>
    <li><strong>value</strong> – The data point for the item.</li><li><b>itemclickurl&nbsp;</b>(Optional)<b>&nbsp;</b>– The url of the page to redirect to when the item is clicked. Generally, the relative url of the target page is provided in this setting.</li>
</ul>
<p>
    The chart itself has quite a few settings for you to consider. These include:
</p>
<ul>
    <li><strong>type</strong> (bar) – The type of chart to display. The valid options include: bar, stackedbar, horizontalBar, line, radar, pie, doughnut, polarArea (think radar meets pie).</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderdash</strong> – This setting defines how the lines on the chart should be displayed. No value makes them display as solid lines. You can make interesting dot/dash patterns by providing an array of numbers representing lines and spaces. For instance, the setting of ''[5, 5]'' would say draw a line of length 5px and then a space of 5px and repeat. You can provide as many numbers as you like to make more complex patterns (but isn''t that getting a little too fancy?)</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>legendposition</strong> (bottom) – This determines where the legend should be displayed.</li>
    <li><strong>legendshow</strong> (false) – Setting determines if the legend should be shown.</li>
    <li><strong>chartheight</strong> (400px) – The height of the chart must be set in pixels.</li>
    <li><strong>chartwidth</strong> (100%) – The width of the chart (can set as either a percentage or pixel size).</li>
    <li><strong>tooltipshow</strong> (true) – Determines if tooltips should be displayed when rolling over data items.</li>
    <li><strong>tooltipbackgroundcolor</strong> (#000) – The background color of the tooltip.</li>
    <li><strong>tooltipfontcolor</strong> (#fff) – The font color of the tooltip.</li>
    <li><strong>fontcolor</strong> (#777) – The font color to use on the chart.</li>
    <li><strong>fontfamily</strong> (sans-serif) – The font to use for the chart.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be.</li>
    <li><strong>pointcolor</strong> (#059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
    <li><strong>curvedlines</strong> (true) – This determines if the lines should be straight between two points or beautifully curved. Based on this description you should be able to determine the default.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart).</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for data items. You can also provide a fill color for each item independently on the [[ dataitem ]] configuration. </li>
    <li><strong>label</strong> – The label to show for the single axis (not often needed in a single axis chart, but hey it''s there.)</li>
    <li><strong>xaxisshow</strong> (true) – Show or hide the x-axis labels. Valid values are ''true'' and ''false''.</li>
    <li><strong>yaxisshow</strong> (true) – Show or hide the y-axis labels. Valid values are ''true'' and ''false''.</li>
    <li><strong>xaxistype</strong> (linear) – The x-axis type. This is primarily used for time based charts. Valid values are ''linear'', ''time'' and ''linearhorizontal0to100''. The linearhorizontal0to100 option makes the horizontal axis scale from 0 to 100.</li>
    <li><strong>yaxismin</strong> (undefined) – The minimum number value of the y-axis. If no value is provided the min value is automatically calculated. To set a chart to always start from zero, rather than using a computed minimum, set the value to 0</li>
		<li><strong>yaxismax</strong> (undefined) – The maximum number value of the y-axis. If no value is provided the max value is automatically calculated.</li>
    <li><strong>yaxisstepsize</strong> (undefined) – If set, the y-axis scale ticks are displayed by a multiple of the defined value. So a yaxisstepsize of 10 means one tick on 10, 20, 30, 40 etc. If no value is provided the step size is automatically computed.</li>
    <li><strong>valueformat</strong> (number) – Format numbers on tooltips and chart axis labels. Valid options include: number (formats to the browser''s locale, in the US adds a thousands comma), currency (adds a comma and currency symbol from the global attribute), percentage (formats number to a percentage in the browser''s locale, expects whole numbers [100 = 100%]), none (no formatting applied).</li>
		<li><strong>X Axis Advanced Options</strong> - Used with the horizontalBar chart type.
		<ul>
		    <li><strong>xaxismin</strong> (undefined) – The minimum number value of the x-axis. If no value is provided the min value is automatically calculated. To set a chart to always start from zero, rather than using a computed minimum, set the value to 0</li>
			<li><strong>xaxismax</strong> (undefined) – The maximum number value of the x-axis. If no value is provided the max value is automatically calculated.</li>
			<li><strong>xaxisstepsize</strong> (undefined) – If set, the x-axis scale ticks are displayed by a multiple of the defined value. So a yaxisstepsize of 10 means one tick on 10, 20, 30, 40 etc. If no value is provided the step size is automatically computed.</li>
		</ul>
		</li>
		
</ul>
<h5>Time Based Charts</h5>
<p>
    If the x-axis of your chart is date/time based you’ll want to set the ''xaxistype'' to ''time'' and provide
    the date in the label field.
</p>
<pre>{[ chart type:''line'' xaxistype:''time'' ]}
    [[ dataitem label:''1/1/2017'' value:''24'']] [[ enddataitem ]]
    [[ dataitem label:''2/1/2017'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''3/1/2017'' value:''42''  ]] [[ enddataitem ]]
    [[ dataitem label:''5/1/2017'' value:''23'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>
<p>
    That should be more than enough settings to get you started on the journey to chart success. But… what about 
    multiple series? Glad you asked…
</p>
<h4>Multiple Series</h4>
<p>
    It’s simple to add multiple series to your charts using the [[ dataset ]] configuration option. Each series is defined 
    by a [[ dataset ]] configuration block. Let’s again start with an example.
</p>
<pre>{[ chart type:''bar'' labels:''2015,2016,2017'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>
<p><img src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/chart-series.jpg"" class=""img-responsive mx-auto"" width=""747"" height=""408"" loading=""lazy""></p>
<p>
    If there is a trick to using series it’s understanding the organization of the data. In our example each [[ dataset ]] 
    is type of group. The data property of the dataset determine the number of groups for each year. The configuration of dataset 
    was created to help you write Lava to dynamically create your charts.
</p>
<p>
    Each of the dataset items have the following configuration options:
</p>
<ul>
    <li><strong>label</strong> – This is the descriptor of the dataset used for the legend.</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for the data set. You should change this to help differentiate the series.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart).</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be. </li>
    <li><strong>pointcolor</strong> (#059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
</ul>
<h5>Time Based Multi-Series Charts</h5>
<p>
    Like their single series brothers, multi-series charts can be line based to by setting
    the xseriestype = ''line'' and providing the dates in the ''label'' setting.
</p>
<pre>{[ chart type:''line'' labels:''1/1/2017,2/1/2017,6/1/2017'' xaxistype:''time'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>
<h4>Gauge Charts</h4>
<p>Gauge charts allow you to create speedometer style charts.</p>
<p><img src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/gauge-chart.png"" class=""img-responsive mx-auto"" width=""500"" height=""268"" loading=""lazy""></p>
<pre>{[ chart type:''tsgauge'' gaugelimits:''0,90,130'' backgroundcolor:''#16c98d,#d4442e'' ]}
  [[ dataitem value:''23'' fillcolor:''#484848'' ]][[ enddataitem ]]
{[ endchart ]}</pre>
<p>
  Each chart has the following configuration options:
</p>
<ul>
  <li><strong>type</strong> – Set to <code>tsgauge</code>.</li>
  <li><strong>gaugelimits</strong> - Gauge limits are comma separated numbers representing each ""band"" of the chart. The first value is the minimum value of the chart, and the last value represents the maximum value.To create additional bands, add additional comma separated numbers between the minimum and maximum value. Note that each comma represents a colored band, and requires an additional background color (detailed below).</li>
  <li><strong>backgroundcolor</strong> – A comma separated list of the color of each ""band"" shown on the chart. A gauge chart will require at least one background color, and additional colors for each additional ""band"".</li>
  <li>
    <strong>dataitem</strong> – A gauge chart has one dataitem that represents the achieved value within the chart.
    <ul>
      <li><strong>value</strong> – The position of the arrow indicator on the chart.</li>
      <li><strong>fillcolor</strong> – The color of the arrow indicator.</li>
			<li><strong>label</strong> (Optional) – Replace the center number, which is the value by default, with the provided label. </li>
    </ul>
  </li>
</ul>',
[Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}
{% assign tooltipvalueformat = valueformat %}
{% assign xvalueformat = ''none'' %}
{%- if type == ''gauge'' or type == ''tsgauge'' -%}
    {%- assign type = ''tsgauge'' -%}
    {% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- elseif type == ''stackedbar'' -%}
    {%- assign type = ''bar'' -%}
    {%- assign xaxistype = ''stacked'' -%}
{%- elseif type == ''horizontalBar'' %}
    {% assign xvalueformat = valueformat %}
    {% assign valueformat = ''none'' %}
{% endif %}
{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}
{%- if type == ''tsgauge'' -%}
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
    {
        backgroundColor: {{ backgroundColor }},
        borderWidth: {{ borderwidth }},
        gaugeData: {
            value: {{dataitems[0].value}},
            {% if dataitems[0].label != '''' %}
                label: ''{{dataitems[0].label}}'',
            {% endif %}
            valueColor: ""{{dataitems[0].fillcolor | Default:''#000000''}}""
        },
        gaugeLimits: {{ gaugeLimits }}
    }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign tooltips = dataitems | Map:''tooltip'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' %}
      {% assign itemclickurls = dataitems | Select:''itemclickurl'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign firstDataItem = dataitems | First  %}
      {% capture seriesData -%}
      {
          fill: {{ filllinearea }},
          backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
          borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
          borderWidth: {{ borderwidth }},
          pointRadius: {{ pointradius }},
          pointBackgroundColor: ''{{ pointcolor }}'',
          pointBorderColor: ''{{ pointbordercolor }}'',
          pointBorderWidth: {{ pointborderwidth }},
          pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
          pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
          pointHoverRadius: ''{{ pointhoverradius }}'',
          {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
          {% if curvedlines == false -%} lineTension: 0,{% endif -%}
          data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
          {% if firstDataItem.tooltip %}
          tooltips: [{{ tooltips }}],
          {% endif %}
      }
      {% endcapture -%}
      {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class=""alert alert-warning"">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''""'' -%}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
      {% endif -%}
      {% assign seriesData = '''' -%}
      {% for dataset in datasets -%}
          {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
          {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
          {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
          {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
          {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
          {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
          {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
          {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
          {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
          {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
          {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
          {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign datasetPointHoverRadius = pointhoverradius %} {% endif -%}
          {%- capture itemData -%}
              {
                  label: ''{{ datasetLabel }}'',
                  fill: {{ datasetFillLineArea }},
                  backgroundColor: ''{{ datasetFillColor }}'',
                  borderColor: ''{{ datasetBorderColor }}'',
                  borderWidth: {{ datasetBorderWidth }},
                  pointRadius: {{ datasetPointRadius }},
                  pointBackgroundColor: ''{{ datasetPointColor }}'',
                  pointBorderColor: ''{{ datasetPointBorderColor }}'',
                  pointBorderWidth: {{ datasetPointBorderWidth }},
                  pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                  pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                  pointHoverRadius: ''{{ datasetPointHoverRadius }}'',
                  {%- if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{%- endif -%}
                  {%- if dataset.curvedlines and dataset.curvedlines == ''false'' -%} lineTension: 0,{%- endif -%}
                  data: [{{ dataset.data }}]
              },
          {% endcapture -%}
          {% assign seriesData = seriesData | Append:itemData -%}
      {% endfor -%}
      {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
  {% endif -%}
{%- endif -%}
<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>
<script>
    var options = {
    maintainAspectRatio: false,
    onClick: function(event, array) { 
        if (array.length > 0) {
            var index = array[0]._index;
            var redirectUrl = data.itemclickurl[index];
            // enable redirection only if a vaild itemclickurl is provided.
            if(data && data.itemclickurl && data.itemclickurl[index]) {
                window.location.href = data.itemclickurl[index];
            }
        }
    },
    hover: {
        onHover: function(event, array) {
            var target = event.target || event.srcElement;
            if (array.length > 0) {
                var index = array[0]._index;
                var redirectUrl = data.itemclickurl[index];
                // enable redirection only if a vaild itemclickurl is provided.
                if(data && data.itemclickurl && data.itemclickurl[index]) {
                    target.style.cursor = ''pointer'';
                    return;
                }
            }
            target.style.cursor = ''default'';
        }
    },scales: {
         yAxes: [{
            ticks: {
                beginAtZero: true
            }
        }]
    },
    {%- if type != ''tsgauge'' -%}
        legend: {
            position: ''{{ legendposition }}'',
            display: {{ legendshow }}
        },
        tooltips: {
            enabled: {{ tooltipshow }}
            {% if tooltipshow %}
            , backgroundColor: ''{{ tooltipbackgroundcolor }}''
            , bodyFontColor: ''{{ tooltipfontcolor }}''
            , titleFontColor: ''{{ tooltipfontcolor }}''
                {% if tooltipvalueformat != '''' and tooltipvalueformat != ''none'' %}
                , callbacks: {
                    label: function(tooltipItem, data) {
                        {% if type == ''pie'' %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% else %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% else %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% when ''number'' %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% else %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% endif %}
                    }
                }
                {% endif %}
            {% endif %}
        }
        {%- else -%}
        events: [],
        showMarkers: false
        {%- endif -%}
        {% if xaxistype == ''time'' %}
            ,scales: {
                xAxes: [{
                    type: ""time"",
                    display: {{ xaxisshow }},
                    scaleLabel: {
                        display: true,
                        labelString: ''Date''
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                    {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                    {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                    {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                    }
                }]
            }
        {% elseif xaxistype == ''linearhorizontal0to100'' %}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    ticks: {
                        min: 0,
                        max: 100
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    gridLines: {
                        display: false
                    }
                }]
            }
        {% elseif xaxistype == ''stacked'' %}
        {% if yaxislabels != '''' %}
            {%- assign yaxislabels = yaxislabels | Split:'','' -%}
            {%- assign yaxislabelcount = yaxislabels | Size -%}
        {% else %}
            {%- assign yaxislabelcount = 0 -%}
        {% endif %}
        ,scales: {
            xAxes: [{
                display: {{ xaxisshow }},
                stacked: true,
                {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                    , ticks: {
                    {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                    {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                    {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                    }
                {% endif %}
            }],
            yAxes: [{
                display: {{ yaxisshow }},
                stacked: true
                {%- if yaxislabelcount > 0 or yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
                , ticks: {
                {% if yaxismin != '''' %}min: {{ yaxismin }}{% endif %}
                {% if yaxismax != '''' %},max: {{ yaxismax }}{% endif %}
                {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                {% if yaxislabelcount > 0 %}
                    ,
                    callback: function(label, index, labels) {
                        switch (label) {
                            {%- for yaxislabel in yaxislabels -%}
                                {%- assign axislabel = yaxislabel | Split:''^'' -%}
                                case {{ axislabel[0] }}: return ''{{axislabel[1]}}'';
                            {%- endfor -%}
                        }
                    }
                {% endif %}
                },
        {% endif %}
            }]
    }
        {%- elseif type != ''pie'' and type != ''tsgauge'' -%}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                        ticks: {
                        {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                        {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                        {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                        {% if xvalueformat != '''' and xvalueformat != ''none'' %}
                            callback: function(label, index, labels) {
                                {% case xvalueformat %}
                                    {% when ''currency'' %}
                                        if (label % 1 === 0) {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                        } else {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                        }
                                    {% when ''percentage'' %}
                                        return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                    {% else %}
                                        return Intl.NumberFormat().format(label);
                                {% endcase %}
                            },
                        {% endif %}
                        }
                    {% endif %}
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    if (label % 1 === 0) {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                    } else {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                    }
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                        {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                        {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                        {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                        },
                }]
            }
        {% endif %}
    };
    {%- if type == ''tsgauge'' -%}
        var data = {
            datasets: [{{ seriesData }}]
        };
    {%- else -%}
        var data = {
            labels: [{{ labels }}],
            datasets: [{{ seriesData }}],
            borderWidth: {{ borderwidth }},
            {% if itemclickurls %}
                itemclickurl: {{ itemclickurls }},
            {% endif %}
        };
    {% endif %}
    Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
    Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";
    var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
    var chart = new Chart(ctx, {
        type: ''{{ type }}'',
        data: data,
        options: options
    });
</script>
'
WHERE [GUID] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        #endregion

        #region PA: Register block attributes for chop job in v1.17.0.30

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        private void RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.MobileLayoutDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.MobileLayoutDetail", "Mobile Layout Detail", "Rock.Blocks.Mobile.MobileLayoutDetail, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "E83C989B-5ECB-4DE4-B5BF-11AF7FC2CCA3" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.EventList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.EventList", "Event List", "Rock.Blocks.Core.EventList, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "3097CE11-E5D7-4708-A576-EF327BE8F6E4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersonalLinkSectionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersonalLinkSectionList", "Personal Link Section List", "Rock.Blocks.Cms.PersonalLinkSectionList, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "55429A67-E6C6-42FE-813B-3EA67A575EB0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.PhotoUpload
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PhotoUpload", "Photo Upload", "Rock.Blocks.Crm.PhotoUpload, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "E9B8A70B-BB59-4044-900F-44150DA73300" );


            // Add/Update Mobile Block Type
            //   Name:Mobile Layout Detail
            //   Category:Mobile
            //   EntityType:Rock.Blocks.Mobile.MobileLayoutDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Mobile Layout Detail", "Edits and configures the settings of a mobile layout.", "Rock.Blocks.Mobile.MobileLayoutDetail", "Mobile", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" );

            // Add/Update Obsidian Block Type
            //   Name:Event List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.EventList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Event List", "Block for viewing list of following events.", "Rock.Blocks.Core.EventList", "Follow", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" );

            // Add/Update Obsidian Block Type
            //   Name:Personal Link Section List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersonalLinkSectionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Personal Link Section List", "Lists personal link section in the system.", "Rock.Blocks.Cms.PersonalLinkSectionList", "CMS", "904DB731-4A40-494C-B52C-95CF0F54C21F" );

            // Add/Update Obsidian Block Type
            //   Name:Photo Upload
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.PhotoUpload
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Photo Upload", "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members.", "Rock.Blocks.Crm.PhotoUpload", "CRM", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" );


            // Attribute for BlockType
            //   BlockType: Event List
            //   Category: Follow
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the following event type details.", 0, @"", "495CBB8D-5AD0-48E6-8801-70EBF1807A1F" );

            // Attribute for BlockType
            //   BlockType: Event List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "86D5B788-D0EE-4838-8B6C-BB3D973512A1" );

            // Attribute for BlockType
            //   BlockType: Event List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BF2A85E5-6D7F-4581-84EE-B3E8681D42EE" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the personal link section details.", 0, @"", "224969C0-CC89-41CB-94C1-F569A97DC293" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: Shared Sections
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Shared Sections", "SharedSection", "Shared Sections", @"When enabled, only shared sections will be displayed.", 1, @"False", "FE13BFB1-1ABA-44CC-825D-DDC24753154C" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "61B4DD25-1D91-444D-929B-EEC86073E506" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2E1D6AC7-A801-4642-8D7C-EC8CA0983458" );

            // Attribute for BlockType
            //   BlockType: Photo Upload
            //   Category: CRM
            //   Attribute: Include Family Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C523CABA-A32C-46A3-A8B4-8F962CDC6A78", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Family Members", "IncludeFamilyMembers", "Include Family Members", @"If checked, other family members will also be displayed allowing their photos to be uploaded.", 0, @"True", "57EBA90F-09FD-4A9F-A6A6-4B5C6F236061" );

            // Attribute for BlockType
            //   BlockType: Photo Upload
            //   Category: CRM
            //   Attribute: Allow Staff
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C523CABA-A32C-46A3-A8B4-8F962CDC6A78", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Staff", "AllowStaff", "Allow Staff", @"If checked, staff members will also be allowed to upload new photos for themselves.", 1, @"False", "5401B675-F6D0-4AAA-B369-601E12F74D0B" );

        }

        // PA: Chop blocks for v1.17.0.30
        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.30",
                blockTypeReplacements: new Dictionary<string, string> {
{ "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "5AA30F53-1B7D-4CA9-89B6-C10592968870" }, // Prayer Request Entry
{ "74B6C64A-9617-4745-9928-ABAC7948A95D", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" }, // Mobile Layout Detail
{ "092BFC5F-A291-4472-B737-0C69EA33D08A", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" }, // Lava Shortcode Detail
{ "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" }, // Event List
{ "0BFD74A8-1888-4407-9102-D3FCEABF3095", "904DB731-4A40-494C-B52C-95CF0F54C21F" }, // Personal Link Section List
{ "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1228F248-6AA1-4871-AF9E-195CF0FDA724" }, // Verify Photo
{ "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "DBFA9E41-FA62-4869-8A44-D03B561433B2" }, // User Login List
{ "7764E323-7460-4CB7-8024-056136C99603", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" }, // Photo Upload


                    // blocks chopped in v1.17.0.29
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report
                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
            } );
        }

        #endregion
    }
}
