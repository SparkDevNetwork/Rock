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
using System.ComponentModel.Composition;
using System.Data.SqlTypes;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs Lava and sets an attribute's value to the result.
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Creates a new short link." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Short Link" )]

    [SiteField(
        "Site",
        Description = "The site to use for the generated short url",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Site,
        ShorteningSitesOnly = true )]

    [WorkflowTextOrAttribute(
        "Token",
        "Token",
        Description = "The token to use for the short link. This is the unique value that will be appended to the site's domain to make the link unique. If left blank, a random token will be generated. <span class='tip tip-lava'></span>",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" },
        IsRequired = false,
        Key = AttributeKey.Token,
        Order = 1 )]

    [WorkflowTextOrAttribute(
        "Target URL",
        "Target Url",
        Description = "The URL that the short link will redirect to. <span class='tip tip-lava'></span>",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType" },
        IsRequired = true,
        Key = AttributeKey.Url,
        Order = 2 )]

    [CategoryField(
        "Category",
        Description = "The category to use for the generated short url",
        EntityTypeName = "Rock.Model.PageShortLink",
        IsRequired = false,
        Key = AttributeKey.Category,
        Order = 3 )]

    [BooleanField(
        "Is Pinned",
        Description = "This is the boolean value used to indicate if the short link is pinned.",
        Key = AttributeKey.IsPinned,
        Order = 4 )]

    [WorkflowAttribute(
        "Attribute",
        Description = "The attribute to store the generated short link's URL to.",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType" },
        IsRequired = false,
        Key = AttributeKey.Attribute,
        Order = 5 )]

    [IntegerField(
        "Random Token Length",
        Description = "The number of characters to use when generating a random unique token.",
        DefaultIntegerValue = 7,
        IsRequired = false,
        Key = AttributeKey.RandomTokenLength,
        Order = 6 )]

    [BooleanField(
        "Allow Token Re-use",
        Description = "If a short link already exists with the same token, should it be updated to the new URL? If this is not allowed, this action will fail due to existing short link.",
        DefaultBooleanValue = true,
        Key = AttributeKey.Overwrite,
        Order = 7 )]

    [IntegerField(
        "Link Expiration",
        Description = "Sets the number of days before the short link expires. Once expired, the link and its tracking data will be permanently deleted. Leave blank for no expiration.",
        IsRequired = false,
        Key = AttributeKey.LinkExpiration,
        Order = 8 )]

    [Rock.SystemGuid.EntityTypeGuid( "AA995907-DAC1-4B7A-ACEF-AEC6CD057E72")]
    public class CreateShortLink : ActionComponent
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private class AttributeKey
        {
            public const string LinkExpiration = "LinkExpiration";
            public const string Site = "Site";
            public const string Token = "Token";
            public const string RandomTokenLength = "RandomTokenLength";
            public const string Url = "Url";
            public const string Category = "Category";
            public const string IsPinned = "IsPinned";
            public const string Overwrite = "Overwrite";
            public const string Attribute = "Attribute";
        }

        #endregion
        
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var service = new PageShortLinkService( rockContext );

            // Get the merge fields
            var mergeFields = GetMergeFields( action );

            // Get the site
            int siteId = GetAttributeValue( action, AttributeKey.Site, true ).AsInteger();
            SiteCache site = SiteCache.Get( siteId );
            if ( site == null )
            {
                errorMessages.Add( string.Format( "Invalid Site Value" ) );
                return false;
            }

            // Get the token
            string token = GetAttributeValue( action, AttributeKey.Token, true ).ResolveMergeFields( mergeFields );
            if ( token.IsNullOrWhiteSpace() )
            {
                int tokenLen = GetAttributeValue( action, AttributeKey.RandomTokenLength ).AsIntegerOrNull() ?? 7;
                token = service.GetUniqueToken( site.Id, tokenLen );
            }

            // Get the target url
            var url = GetAttributeValue( action, AttributeKey.Url, true ).ResolveMergeFields( mergeFields ).RemoveCrLf().Trim();
            if ( url.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "A valid Target URL was not specified." );
                return false;
            }

            var categoryId = GetAttributeValue( action, AttributeKey.Category, true ).AsIntegerOrNull();

            if ( categoryId.HasValue )
            {
                var category = CategoryCache.Get( categoryId.Value );
                if ( category == null || category.EntityTypeId != EntityTypeCache.GetId<PageShortLink>() )
                {
                    categoryId = null;
                }
            }

            var isPinned = GetAttributeValue( action, AttributeKey.IsPinned, true ).AsBoolean();

            var expireInDays = GetAttributeValue( action, AttributeKey.LinkExpiration, true ).AsIntegerOrNull();
            /*
                1/29/2026 - JMH

                Negative values are allowed for the "Link Expiration" setting and indicate that the short link
                has already expired. This situation occurs when an individual edits an expired short link
                but does not modify the expiration value, which remains negative. An individual may also set
                a negative expiration value intentionally to have the short link expire immediately.

                These expired links are still eligible for automatic cleanup by the Rock Cleanup job.

                Reason: Preserve the ability to edit expired short links without forcing a reset of the expiration logic.
            */
            var expireDate = expireInDays.HasValue
                ? AddDaysSqlSafe( RockDateTime.Today, expireInDays.Value )
                : ( DateTime? ) null;

            // Save the short link
            var link = service.GetByToken( token, site.Id );
            if ( link != null )
            {
                if ( !GetAttributeValue( action, AttributeKey.Overwrite ).AsBoolean() )
                {
                    errorMessages.Add( string.Format( "The selected token ('{0}') already exists. Please specify a unique token, or configure action to allow token re-use.", token ) );
                    return false;
                }
                else
                {
                    link.Url = url;
                }
            }
            else
            {
                link = new PageShortLink
                {
                    SiteId = site.Id,
                    Token = token,
                    Url = url,
                    CategoryId = categoryId,
                    IsPinned = isPinned,
                    ExpireDate = expireDate
                };

                service.Add( link );
            }

            rockContext.SaveChanges();

            // Save the resulting short link url
            var attribute = AttributeCache.Get( GetAttributeValue( action, AttributeKey.Attribute ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                string shortLink = link.ShortLinkUrl;

                SetWorkflowAttributeValue( action, attribute.Guid, shortLink );
                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, shortLink ) );
            }

            return true;
        }

        /// <summary>
        /// Adds the specified number of days to the given <see cref="DateTime"/>,
        /// clamping the result to the valid SQL Server <c>datetime</c> range.
        /// </summary>
        /// <remarks>
        /// SQL Server <c>datetime</c> values must be between
        /// <see cref="SqlDateTime.MinValue"/> (1753-01-01) and
        /// <see cref="SqlDateTime.MaxValue"/> (9999-12-31 23:59:59.997).
        ///
        /// If adding the specified number of days would result in a value greater than
        /// <see cref="SqlDateTime.MaxValue"/>, this method returns that maximum value.
        /// If the result would be less than <see cref="SqlDateTime.MinValue"/>,
        /// it returns that minimum value.
        ///
        /// This method prevents exceptions when generating DateTime values that will
        /// be persisted to SQL Server.
        /// </remarks>
        /// <param name="dateTime">The date and time value to which days are added.</param>
        /// <param name="days">The number of days to add. Can be negative.</param>
        /// <returns>
        /// A SQL Server safe <see cref="DateTime"/> value representing the result
        /// of adding <paramref name="days"/> to <paramref name="dateTime"/>.
        /// </returns>
        private static DateTime AddDaysSqlSafe( DateTime dateTime, int days )
        {
            var sqlMin = ( DateTime )SqlDateTime.MinValue;
            var sqlMax = ( DateTime )SqlDateTime.MaxValue;

            var maxDays = ( sqlMax - dateTime ).TotalDays;
            var minDays = ( sqlMin - dateTime ).TotalDays;

            if ( minDays <= days && days <= maxDays )
            {
                return dateTime.AddDays( days );
            }
            else if ( days > 0 )
            {
                return sqlMax;
            }
            else
            {
                return sqlMin;
            }
        }
    }
}