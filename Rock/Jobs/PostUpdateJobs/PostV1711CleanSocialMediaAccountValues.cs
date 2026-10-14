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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17.11 to clean up Social Media Account attribute values
    /// that could inject script into the page when they are displayed.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.11 - Clean Social Media Account Values" )]
    [Description( "This job will clean up Social Media Account attribute values that are not safe to display." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of attribute values, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]
    public class PostV1711CleanSocialMediaAccountValues : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// The number of attribute values to load and save at one time.
        /// </summary>
        private const int BatchSize = 1000;

        /// <summary>
        /// Matches a URL scheme at the start of the value, such as "https:".
        /// </summary>
        private static readonly Regex SchemePattern = new Regex( @"^([a-zA-Z][a-zA-Z0-9+.\-]*):", RegexOptions.Compiled | RegexOptions.CultureInvariant );

        /// <summary>
        /// Matches an HTML character reference, such as "&amp;#58" or "&amp;colon;".
        /// </summary>
        private static readonly Regex CharacterReferencePattern = new Regex( @"&(?:#|[a-zA-Z][a-zA-Z0-9]*;)", RegexOptions.Compiled | RegexOptions.CultureInvariant );

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            // The field type is found by its Guid rather than the class so
            // this job doesn't depend on the field type class.
            var fieldTypeId = FieldTypeCache.GetId( SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT.AsGuid() );
            var updatedCount = 0;
            var clearedCount = 0;

            if ( fieldTypeId.HasValue )
            {
                List<int> attributeIds;

                using ( var rockContext = new RockContext() )
                {
                    attributeIds = new AttributeService( rockContext )
                        .Queryable()
                        .Where( a => a.FieldTypeId == fieldTypeId.Value )
                        .Select( a => a.Id )
                        .ToList();
                }

                var lastId = 0;

                while ( attributeIds.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = commandTimeout;

                        var attributeValues = new AttributeValueService( rockContext )
                            .Queryable()
                            .Where( av => attributeIds.Contains( av.AttributeId )
                                && av.Id > lastId
                                && av.Value != null
                                && av.Value != string.Empty )
                            .OrderBy( av => av.Id )
                            .Take( BatchSize )
                            .ToList();

                        if ( !attributeValues.Any() )
                        {
                            break;
                        }

                        lastId = attributeValues.Last().Id;

                        var changedValues = new List<AttributeValue>();

                        foreach ( var attributeValue in attributeValues )
                        {
                            var safeValue = GetSafeUrl( attributeValue.Value );

                            if ( safeValue == attributeValue.Value.Trim() )
                            {
                                continue;
                            }

                            if ( safeValue == null )
                            {
                                attributeValue.Value = string.Empty;
                                clearedCount++;
                            }
                            else
                            {
                                attributeValue.Value = safeValue;
                                updatedCount++;
                            }

                            // Pre and post processing is disabled below, so do
                            // the work it would normally do for us.
                            attributeValue.UpdateValueAsProperties( rockContext );
                            attributeValue.IsPersistedValueDirty = true;
                            attributeValue.ModifiedDateTime = RockDateTime.Now;

                            changedValues.Add( attributeValue );
                        }

                        if ( changedValues.Any() )
                        {
                            rockContext.SaveChanges( true );

                            foreach ( var attributeValue in changedValues )
                            {
                                attributeValue.UpdateCache( EntityState.Modified, rockContext );
                            }
                        }
                    }
                }
            }

            Result = $"Updated {updatedCount:N0} and cleared {clearedCount:N0} Social Media Account values.";

            DeleteJob();
        }

        /// <summary>
        /// Gets a version of the URL that is safe to place inside an HTML
        /// attribute, such as an href. Characters that could end the
        /// attribute are percent-encoded. Values with a scheme other than
        /// http or https, or that contain HTML character references, can't
        /// be made safe.
        /// </summary>
        /// <remarks>
        /// This is a copy of the logic in SocialMediaAccountFieldType so that
        /// this job does not depend on the field type class.
        /// </remarks>
        /// <param name="value">The URL to be made safe.</param>
        /// <returns>The safe URL, or <c>null</c> if the value can't be made safe.</returns>
        internal static string GetSafeUrl( string value )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            var url = value.Trim();

            if ( CharacterReferencePattern.IsMatch( url ) )
            {
                return null;
            }

            var sb = new StringBuilder( url.Length );

            foreach ( var c in url )
            {
                if ( c <= ' ' || c == '\x7F' || c == '"' || c == '\'' || c == '<' || c == '>' || c == '`' )
                {
                    sb.Append( '%' ).Append( ( ( int ) c ).ToString( "X2" ) );
                }
                else
                {
                    sb.Append( c );
                }
            }

            url = sb.ToString();

            // Values without a scheme are relative URLs, such as a bare
            // username when no base URL is configured.
            var schemeMatch = SchemePattern.Match( url );

            if ( schemeMatch.Success )
            {
                var scheme = schemeMatch.Groups[1].Value;

                if ( !scheme.Equals( "http", StringComparison.OrdinalIgnoreCase ) && !scheme.Equals( "https", StringComparison.OrdinalIgnoreCase ) )
                {
                    return null;
                }

                if ( !Uri.TryCreate( url, UriKind.Absolute, out _ ) )
                {
                    return null;
                }
            }

            return url;
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
