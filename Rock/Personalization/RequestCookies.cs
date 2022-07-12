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
namespace Rock.Personalization
{
    /// <summary>.
    /// The Cookie Keys for Rock Visitor Tracking an Personalization.
    /// </summary>
    public static class RequestCookieKey
    {
        /// <summary>
        /// The cookie key for .ROCK_VISITOR_KEY.
        /// Expires after X days.
        /// <para>
        /// This will be the <see cref="Rock.Data.IEntity.IdKey">IdKey</see> of a <see cref="Rock.Model.PersonAlias">PersonAlias</see> record.
        /// </para>
        /// </summary>
        public static readonly string ROCK_VISITOR_KEY = $".ROCK_VISITOR_KEY";

        /// <summary>
        /// The cookie key for .ROCK_VISITOR_CREATED_DATETIME.
        /// Expires after X days.
        /// <para>
        /// The value is the UTC DateTime of when the Visitor PersonAlias was created ( ISO-8601 format ).
        /// </para>
        /// </summary>
        public static readonly string ROCK_VISITOR_CREATED_DATETIME = $".ROCK_VISITOR_CREATED_DATETIME";

        /// <summary>
        /// The cookie key for .ROCK_VISITOR_LASTSEEN.
        /// Expires after X days.
        ///  Tracks the last page load of the visitor. This is used to determine how long since they were last here.
        /// </summary>
        public static readonly string ROCK_VISITOR_LASTSEEN = $".ROCK_VISITOR_LASTSEEN";

        /// <summary>
        /// The cookie key for .ROCK_FIRSTTIME_VISITOR.
        /// Cookie is only valid for session.
        /// <para>
        /// This will have a value of True if this is the first time the visitor has visited the site.
        /// </para>
        /// </summary>
        public static readonly string ROCK_FIRSTTIME_VISITOR = $".ROCK_FIRSTTIME_VISITOR";

        /// <summary>
        /// The cookie key for .ROCK_SESSION_START_DATETIME.
        /// Cookie is only valid for session.
        /// <para>
        /// The value is the UTC DateTime of when the Session started for the CurrentPerson/Visitor ( ISO-8601 format ).
        /// </para>
        /// </summary>
        public static readonly string ROCK_SESSION_START_DATETIME = $".ROCK_SESSION_START_DATETIME";

        /// <summary>
        /// The cookie key for .ROCK_SEGMENT_FILTERS.
        /// Cookie is only valid for session.
        /// <para>
        /// A comma-delimited list of <seealso cref="Rock.Model.PersonalizationSegment">PersonalizationSegment</seealso> <see cref="Rock.Data.IEntity.IdKey">IdKeys</see> that the current
        /// person/visitor meets the filter for.
        /// </para>
        /// </summary>
        public static readonly string ROCK_SEGMENT_FILTERS = $".ROCK_SEGMENT_FILTERS";
    }
}
