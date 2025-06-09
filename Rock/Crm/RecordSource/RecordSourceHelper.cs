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

using Rock.Attribute;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Crm.RecordSource
{
    /// <summary>
    /// Helper class for tracking the source of records (e.g. People) coming into Rock.
    /// </summary>
    /// <remarks>
    /// This is an internal API that supports the Rock infrastructure and not
    /// subject to the same compatibility standards as public APIs. It may be
    /// changed or removed without notice in any release. You should only use
    /// it directly in your code with extreme caution and knowing that doing so
    /// can result in application failures when updating to a new Rock release.
    /// </remarks>
    [RockInternal( "18.0" )]
    public static class RecordSourceHelper
    {
        /// <summary>
        /// The page parameter keys used by this helper.
        /// </summary>
        private static class PageParameterKey
        {
            public const string RecordSource = "RecordSource";
        }

        /// <summary>
        /// The cookie names used by this helper.
        /// </summary>
        private static class CookieName
        {
            public const string RecordSource = "ROCK_RECORD_SOURCE";
        }

        /// <summary>
        /// Gets the session-scoped identifier of the Record Source Type <see cref="DefinedValue"/>, to be used for any
        /// records that are created within this session.
        /// </summary>
        /// <returns>
        /// An identifier representing the session-scoped Record Source Type <see cref="DefinedValue"/>, or
        /// <see langword="null"/> if not found.
        /// </returns>
        public static int? GetSessionRecordSourceValueId()
        {
            var rockRequestContext = RockRequestContextAccessor.Current;
            if ( rockRequestContext != null )
            {
                var allowIntegerIdentifier = !rockRequestContext.Page?.Layout?.Site?.DisablePredictableIds ?? false;

                var recordSourceParam = rockRequestContext.GetPageParameter( PageParameterKey.RecordSource );
                if ( recordSourceParam.IsNotNullOrWhiteSpace() )
                {
                    // Is the page parameter a defined value identifier (ID, GUID, IdKey)?
                    var recordSourceValue = DefinedValueCache.Get( recordSourceParam, allowIntegerIdentifier );

                    if ( recordSourceValue != null )
                    {
                        // Ensure it's a valid defined value of the record source defined type.
                        if ( recordSourceValue.DefinedType.Guid.Equals( Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE.AsGuid() ) )
                        {
                            return recordSourceValue.Id;
                        }
                    }
                    else
                    {
                        // If not an identifier, is the provided key a valid record source [DefinedValue].[Value]?
                        recordSourceValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE.AsGuid() )
                            ?.GetDefinedValueFromValue( recordSourceParam );

                        if ( recordSourceValue != null )
                        {
                            return recordSourceValue.Id;
                        }
                    }
                }

                var recordSourceValueIdKey = rockRequestContext.GetCookieValue( CookieName.RecordSource );
                if ( recordSourceValueIdKey.IsNotNullOrWhiteSpace() )
                {
                    var recordSourceValue = DefinedValueCache.GetByIdKey( recordSourceValueIdKey );
                    if ( recordSourceValue != null )
                    {
                        return recordSourceValue.Id;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to set the record source session cookie using the provided delegate.
        /// </summary>
        /// <param name="addOrUpdateCookie">
        /// A delegate to add or update the cookie. The first argument is the cookie name, and the second argument is
        /// the value to set.
        /// </param>
        /// <remarks>
        /// The delegate will be called if a valid record source is found in page parameters or cookies.
        /// </remarks>
        public static void TrySetRecordSourceSessionCookie( Action<string, string> addOrUpdateCookie )
        {
            if ( addOrUpdateCookie == null )
            {
                return;
            }

            var recordSourceValueId = GetSessionRecordSourceValueId();
            if ( !recordSourceValueId.HasValue )
            {
                return;
            }

            var recordSourceValueIdKey = DefinedValueCache.Get( recordSourceValueId.Value )?.IdKey;
            if ( recordSourceValueIdKey.IsNullOrWhiteSpace() )
            {
                // Should never happen.
                return;
            }

            addOrUpdateCookie( CookieName.RecordSource, recordSourceValueIdKey );
        }
    }
}
