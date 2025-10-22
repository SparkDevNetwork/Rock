using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Lists page visits for a specific person." )]
        [AgentPurpose( "Retrieves media views for a specific person, optionally filtered by date and/or site." )]
        [AgentUsage( "The results are paginated (and the 'PageNumber' parameter is required.)" )]
        [AgentToolGuid( "AB6CB80C-352A-F895-4233-09BA9DA69CCC" )]
        public RockToolResult ListMediaViewsForPerson( string personIdKey, int pageNumber = 1, DateTime? startDate = null, DateTime? endDate = null )
        {
            // Validate person
            var personId = IdHasher.Instance.GetId( personIdKey );
            if ( !personId.HasValue || personId <= 0 )
            {
                RockToolResult.Error( "The personIdKey is not valid. Please provide a valid value." );
            }

            // Validate date range
            if ( startDate.HasValue && endDate.HasValue && startDate > endDate )
            {
                RockToolResult.Error( "Invalid date range. Start date cannot be after end date." );
            }

            // Defaults: past year → now
            if ( !startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
                startDate = endDate.Value.AddYears( -1 );
            }
            else if ( startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
            }

            // Paging
            var basePageSize = 100;
            var offset = ( pageNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            // Run query
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@StartDate", startDate),
                    GetParameterValueOrDbNull("@EndDate", endDate),
                    new SqlParameter("@PageSize",  take),    // request N+1
                    new SqlParameter("@OffsetRows", offset), // offset uses base size
                };

                var rows = AgentRequestContext.RockContext.Database
                    .SqlQuery<MediaViewResult>( _mediaViewsDataSql, parameters.ToArray() )
                    .ToList();

                var hasMore = rows.Count > basePageSize;
                if ( hasMore )
                {
                    rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
                }

                var meta = new Dictionary<string, object>
                {
                    { "personKey", personIdKey },
                    { "startDate", startDate },
                    { "endDate", endDate },
                    { "pageNumber", pageNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", rows.Count },
                    { "hasMore", hasMore }
                };

                if ( !rows.Any() )
                {
                    return RockToolResult.NoData()
                        .WithMetadata( meta );
                }

                // Do some quick clean-up of the media data
                CleanMediaViews( rows );

                return RockToolResult.Success( rows )
                    .WithMetadata( meta );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "ListMediaViewsForPerson failed for PersonId={PersonId}", personId );
                return RockToolResult.Error( "Failed to retrieve media views. " + ex.Message );
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Cleans up the media views by determining the medium and adjusting the viewing location URL.
        /// </summary>
        /// <param name="mediaViews"></param>
        private void CleanMediaViews( List<MediaViewResult> mediaViews )
        {
            foreach ( var media in mediaViews )
            {
                if ( media.ViewingLocationUrl.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( media.ViewingLocationUrl.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                {
                    // If the URL starts with http, we can assume it's a full URL.
                    media.Medium = "Web";
                }
                else
                {
                    // Otherwise, we assume it's mobile (e71a7c63-f510-434b-945b-f30c1c18df9d?CategoryGuid=24ae4f53-1bb8-4637-ae0e-5db0b06856b3).
                    var urlParts = media.ViewingLocationUrl.Split( '?' );

                    if ( urlParts.Length != 2 )
                    {
                        continue;
                    }

                    var page = PageCache.Get( urlParts[0].AsGuid() );

                    if ( page == null )
                    {
                        continue;
                    }

                    media.ViewingLocationUrl = $"{page.Site} - {page.PageTitle}";
                    media.Medium = "Mobile";
                }
            }
        }

        #endregion

        #region SQL

        private const string _mediaViewsDataSql = @"
            SELECT 
                me.[Id] AS [MediaElementId]
                , i.[InteractionDateTime] AS [ViewDateTime]
                , i.[ChannelCustomIndexed1] AS [Medium]
                , CAST( ROUND( i.[InteractionLength], 0 ) AS int ) AS [PercentWatched]
                , me.[DurationSeconds] AS [MediaLengthInSeconds]
                , CAST( ROUND(me.[DurationSeconds] * i.[InteractionLength] / 100, 0) AS int) AS [DurationWatchedInSeconds]
                , me.[Name] AS [MediaElementName]
                , mf.[Name] AS [MediaFolderName]
                , ma.[Name] AS [MediaAccountName]
                , i.[InteractionSummary] AS [ViewingLocationUrl]
            FROM [Interaction] i
                INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
                INNER JOIN [InteractionChannel] ich ON ich.[Id] = ic.[InteractionChannelId]
                INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
                INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
                INNER JOIN [MediaElement] me ON me.[Id] = ic.[EntityId]
                INNER JOIN [MediaFolder] mf ON mf.[Id] = me.[MediaFolderId]
                INNER JOIN [MediaAccount] ma ON ma.[Id] = mf.[MediaAccountId]
            WHERE   
                ich.[Guid] = 'd5b9bdaf-6e52-40d5-8e74-4e23973df159'
                AND p.[Id] = @PersonId
                AND i.[InteractionDateTime] >= @StartDate
                AND i.[InteractionDateTime] <= @EndDate
            ORDER BY 
                i.[InteractionDateTime] DESC
                , i.[Id] DESC
            OFFSET @OffsetRows ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";

        #endregion
    }
}
