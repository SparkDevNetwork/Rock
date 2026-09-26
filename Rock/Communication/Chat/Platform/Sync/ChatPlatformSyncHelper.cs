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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Blocks;
using Rock.Bus.Locking;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Session;
using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tasks;
using Rock.ViewModels.Blocks.Communication.Chat.ChatSyncNow;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What the Chat Platform Sync job says to the chat platform and how it reads the answer, and
    /// Sync Now for the Chat Configuration and Group Type Detail blocks.
    /// </summary>
    internal static class ChatPlatformSyncHelper
    {
        #region Constants

        private const string RowCountsHeader = "x-sync-counts";

        private const string IdentityMarksHeader = "x-sync-marks";

        private const string ReadTimeHeader = "x-sync-read-at";

        private const string RockVersionHeader = "x-sync-rock-version";

        private const string ContractHeader = "x-sync-contract";

        private const string UrgentHeader = "x-sync-urgent";

        // Added by the transport, so every attempt of one submission carries the same id.
        private const string SubmissionIdHeader = "x-sync-submission-id";

        // A tick is a hundred nanoseconds and the platform stores microseconds.
        private const long TicksPerMicrosecond = 10L;

        private static readonly string NeverEnabledMessage = "Chat is not set up for this church, so there is nothing to sync.";

        private static readonly string UnreadableKeyMessage = "Chat is set up for this church, but this installation cannot read the signing key it was given, so it cannot sync.";

        private static readonly string ForbiddenMessage = "You are not authorized to sync chat from here.";

        private static readonly string MissingJobMessage = "The Chat Platform Sync job is missing, so there is nothing to run.";

        private static readonly string AlreadyRunningMessage = "A sync is already running. Press Sync Now again when it has finished.";

        private static readonly string WaitingMessage = "Waiting for the sync to start.";

        private static readonly string RunningMessage = "The sync is running.";

        private static readonly string OvertakenMessage = "The schedule started a sync at the same moment, and it submitted nothing because the chat platform asked for a backoff. Press Sync Now again.";

        // The longest a person's run can take, so the screen never gives up on one that will finish:
        // the projection's timeout, every exchange and submission attempt and one status read at the
        // request timeout, and the manual poll. An estimate that leaves out the short retry waits.
        private static readonly int SyncNowBudgetMilliseconds = ( int ) ( TimeSpan.FromSeconds( ChatPlatformSync.ProjectionTimeoutSeconds )
            + TimeSpan.FromTicks( PlatformClient.RequestTimeout.Ticks * ( ( 2 * PlatformClient.TransportAttempts ) + 1 ) )
            + PollBudget.Manual.Duration ).TotalMilliseconds;

        // What Rock records for a run that ended well; anything else is a run that did not.
        private const string SuccessStatus = "Success";

        private static readonly Guid SyncJobGuid = Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC_JOB.AsGuid();

        #endregion Constants

        #region Submission headers

        /// <summary>
        /// The payload's section names, in the order the contract lists them.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <returns>The section names.</returns>
        internal static IList<string> GetPayloadSections( JObject contract )
        {
            var sections = contract["payload"] == null ? null : contract["payload"]["sections"];

            if ( sections == null )
            {
                throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a submission" );
            }

            return sections.Select( s => s.Value<string>() ).ToList();
        }

        /// <summary>
        /// Builds every header one submission carries beside its body, save the submission id.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="readAtUtc">The UTC time the projection was read at.</param>
        /// <param name="marks">The identity seed of each table read, by the contract's mark key.</param>
        /// <param name="rowCounts">The rows written, by section.</param>
        /// <param name="rockVersion">The version of Rock producing the payload.</param>
        /// <param name="isUrgent">Whether a person started this run and is waiting on it.</param>
        /// <returns>The headers, keyed by name.</returns>
        internal static IDictionary<string, string> BuildSubmissionHeaders( JObject contract, DateTime readAtUtc, IDictionary<string, long> marks, IDictionary<string, int> rowCounts, string rockVersion, bool isUrgent )
        {
            if ( contract == null )
            {
                throw new ArgumentNullException( nameof( contract ) );
            }

            if ( rowCounts == null )
            {
                throw new ArgumentNullException( nameof( rowCounts ) );
            }

            if ( rockVersion.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "the version of Rock producing a payload is recorded with the submission and cannot be blank", nameof( rockVersion ) );
            }

            // An artifact edited after it was generated has a column order nobody agreed to, and the
            // platform would refuse it on every cycle under a code that points at no file.
            var publishedHash = contract["wire_hash"] == null ? null : contract["wire_hash"].Value<string>();
            var computedHash = ChatWireContract.HashColumnLists( contract );

            if ( publishedHash != computedHash )
            {
                throw new InvalidOperationException(
                    "the wire contract this submission would be built from does not hash to the value written inside it, so nothing here can say what column order the payload is in" );
            }

            RequireCountKeysAreTheSections( contract );

            var headers = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
            {
                { ReadTimeHeader, FormatReadTime( readAtUtc ) },
                { RowCountsHeader, BuildKeyedHeader( contract, RowCountsHeader, rowCounts.ToDictionary( c => c.Key, c => ( long ) c.Value ) ) },
                { IdentityMarksHeader, BuildKeyedHeader( contract, IdentityMarksHeader, marks ) },
                { RockVersionHeader, rockVersion },
                { ContractHeader, computedHash }
            };

            // Absent rather than false on a scheduled run: absence is ordinary priority.
            if ( isUrgent )
            {
                headers.Add( UrgentHeader, "1" );
            }

            RequireTheContractsHeaderSet( contract, headers );

            return headers;
        }

        /// <summary>
        /// Builds a header whose value is a JSON object keyed exactly by the contract's key set.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="headerName">The header.</param>
        /// <param name="values">The values, by key.</param>
        /// <returns>The header value.</returns>
        /// <remarks>
        /// The keys are read from the contract, never typed here, because the platform compares
        /// them exactly. Key order is not part of the set.
        /// </remarks>
        internal static string BuildKeyedHeader( JObject contract, string headerName, IDictionary<string, long> values )
        {
            if ( values == null )
            {
                throw new ArgumentNullException( nameof( values ) );
            }

            var keys = GetHeaderKeys( contract, headerName );

            // A key with no value is, for the counts, a projection that did not run; sending it as
            // zero would read as a church with none of that row.
            var unsupplied = keys.Except( values.Keys ).ToList();

            if ( unsupplied.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract asks for {0} in the {1} header, which nothing here supplies",
                    string.Join( ", ", unsupplied ),
                    headerName ) );
            }

            var unlisted = values.Keys.Except( keys ).ToList();

            if ( unlisted.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract no longer lists {0} in the {1} header, which this assembly still supplies",
                    string.Join( ", ", unlisted ),
                    headerName ) );
            }

            var header = new JObject();

            foreach ( var key in keys )
            {
                header[key] = values[key];
            }

            return header.ToString( Formatting.None );
        }

        /// <summary>
        /// Formats the read time the whole payload is judged by.
        /// </summary>
        /// <param name="readAtUtc">The UTC time the projection was read at.</param>
        /// <returns>The header value.</returns>
        internal static string FormatReadTime( DateTime readAtUtc )
        {
            if ( readAtUtc.Kind != DateTimeKind.Utc )
            {
                throw new ArgumentException( "the read time a payload is judged by has to be taken in UTC, because it is compared against times the platform holds in UTC", nameof( readAtUtc ) );
            }

            // Truncated towards the past: the platform refuses a row whose stored time is not
            // strictly older, so a value rounded up could let a stale write win.
            var truncated = new DateTime( readAtUtc.Ticks - ( readAtUtc.Ticks % TicksPerMicrosecond ), DateTimeKind.Utc );

            return truncated.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture );
        }

        /// <summary>
        /// One header entry's key set, as the contract lists it.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="headerName">The header.</param>
        /// <returns>The keys.</returns>
        private static IList<string> GetHeaderKeys( JObject contract, string headerName )
        {
            var headers = contract["submit_headers"];

            if ( headers == null )
            {
                throw new InvalidOperationException( "the chat wire contract describes no submit headers" );
            }

            var header = headers.Children<JObject>().FirstOrDefault( h => h["name"] != null && h["name"].Value<string>() == headerName );

            if ( header == null )
            {
                throw new InvalidOperationException( string.Format( "the chat wire contract describes no {0} header", headerName ) );
            }

            if ( header["keys"] == null )
            {
                throw new InvalidOperationException( string.Format( "the chat wire contract does not carry the key set of {0} as data, so this header could only be built from prose about it", headerName ) );
            }

            return header["keys"].Select( k => k.Value<string>() ).ToList();
        }

        /// <summary>
        /// Refuses a contract whose row-count keys are not its payload sections.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        private static void RequireCountKeysAreTheSections( JObject contract )
        {
            var sections = GetPayloadSections( contract );
            var keys = GetHeaderKeys( contract, RowCountsHeader );
            var disagreements = keys.Except( sections ).Concat( sections.Except( keys ) ).ToList();

            if ( disagreements.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract's row-count keys and payload sections disagree about {0}",
                    string.Join( ", ", disagreements ) ) );
            }
        }

        /// <summary>
        /// Holds a built header set against the contract's own list: every required header but the
        /// submission id is present, and nothing is sent that the contract does not list.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="headers">The headers as built.</param>
        private static void RequireTheContractsHeaderSet( JObject contract, IDictionary<string, string> headers )
        {
            var listed = contract["submit_headers"];

            if ( listed == null )
            {
                throw new InvalidOperationException( "the chat wire contract describes no submit headers" );
            }

            var entries = listed.Children<JObject>().ToList();

            var missing = entries
                .Where( h => h["required"] != null && h["required"].Value<bool>() )
                .Select( h => h["name"].Value<string>() )
                .Where( name => !string.Equals( name, SubmissionIdHeader, StringComparison.OrdinalIgnoreCase ) )
                .Where( name => !headers.ContainsKey( name ) )
                .ToList();

            if ( missing.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract requires the {0} header, which nothing here builds",
                    string.Join( ", ", missing ) ) );
            }

            var names = entries.Select( h => h["name"].Value<string>() ).ToList();
            var unlisted = headers.Keys.Where( name => !names.Contains( name, StringComparer.OrdinalIgnoreCase ) ).ToList();

            if ( unlisted.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the {0} header is not one the chat wire contract lists",
                    string.Join( ", ", unlisted ) ) );
            }
        }

        #endregion Submission headers

        #region Outcome

        /// <summary>
        /// What a sync run has to say about its submission.
        /// </summary>
        /// <param name="acknowledgement">What came back from the submission.</param>
        /// <param name="polled">What a status read found, or null where none could be made.</param>
        /// <returns>Whether the run failed, and a sentence saying why or what happened.</returns>
        /// <remarks>
        /// "Not yet applied" is not a failure: the queue often has not reached a submission when its
        /// run ends, and a run red on most cycles teaches an administrator to stop reading it.
        /// </remarks>
        internal static (bool IsFailure, string Message) Resolve( Acknowledgement acknowledgement, Outcome polled )
        {
            if ( acknowledgement.IsTransportFailure )
            {
                return (true, "the chat platform could not be reached: "
                    + ( acknowledgement.TransportDetail.IsNullOrWhiteSpace() ? "no reason was given" : acknowledgement.TransportDetail ));
            }

            // The gateway answers a token it cannot verify in its own shape, with no status, and
            // that is about this church's credential, not this version of Rock.
            if ( !acknowledgement.Status.HasValue && ( acknowledgement.HttpStatusCode == 401 || acknowledgement.HttpStatusCode == 403 ) )
            {
                return (true, "the chat platform refused this church's credential" + Reason( acknowledgement.ErrorCode ));
            }

            if ( !acknowledgement.Status.HasValue )
            {
                return (true, "the chat platform answered with nothing this version of Rock can read" + Reason( acknowledgement.ErrorCode ));
            }

            // A refused submission never reaches the queue, so there is nothing to poll for.
            if ( acknowledgement.Status.Value == SubmissionStatus.Refused )
            {
                return (true, "the chat platform refused this restatement" + Reason( acknowledgement.ErrorCode ));
            }

            if ( polled != null && polled.Status.HasValue && polled.Status.Value != SubmissionStatus.Accepted )
            {
                return (!IsJobSuccess( polled.Status.Value ),
                    "this restatement was " + WireValueFor( polled.Status.Value ) + Reason( polled.ErrorCode ));
            }

            // Named, so the previous cycle's result is never read as this one's, and information
            // only: this run was accepted. A previous one still at accepted was replaced by a newer
            // submission before the queue reached it, or is still waiting, so it has no result.
            var previous = acknowledgement.PreviousOutcome;
            if ( previous != null && previous.Status.HasValue )
            {
                return (false,
                    "this restatement was submitted, not yet applied. The previous submission, "
                        + previous.SubmissionId + ", was "
                        + ( previous.Status.Value == SubmissionStatus.Accepted ? "superseded or still queued" : WireValueFor( previous.Status.Value ) )
                        + Reason( previous.ErrorCode ));
            }

            return (false, "this restatement was submitted, not yet applied");
        }

        /// <summary>
        /// Whether a sync run that saw this status worked. Accepted counts: the restatement is stored
        /// and queued, and when the queue reaches it is not the run's fault.
        /// </summary>
        /// <param name="status">The recorded status.</param>
        /// <returns>True where the run worked.</returns>
        internal static bool IsJobSuccess( SubmissionStatus status )
        {
            return status == SubmissionStatus.Accepted || status == SubmissionStatus.Applied;
        }

        /// <summary>
        /// The label the wire carries for this status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>The wire label.</returns>
        internal static string WireValueFor( SubmissionStatus status )
        {
            return status.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The status a wire label names, or null where this build of Rock does not know it, so a
        /// label added later reads as unknown rather than as the nearest guess.
        /// </summary>
        /// <param name="value">The wire label.</param>
        /// <returns>The status, or null.</returns>
        internal static SubmissionStatus? ParseStatus( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            SubmissionStatus parsed;
            if ( !Enum.TryParse( value, true, out parsed ) || !Enum.IsDefined( typeof( SubmissionStatus ), parsed ) )
            {
                return null;
            }

            // Enum.TryParse takes a number too, which the wire never carries.
            return WireValueFor( parsed ) == value.ToLowerInvariant() ? parsed : ( SubmissionStatus? ) null;
        }

        /// <summary>
        /// The named reason, where there is one, as a clause.
        /// </summary>
        /// <param name="errorCode">The code, or null.</param>
        /// <returns>The clause, or an empty string.</returns>
        private static string Reason( string errorCode )
        {
            return errorCode.IsNullOrWhiteSpace() ? string.Empty : ": " + errorCode;
        }

        #endregion Outcome

        #region Row values

        /// <summary>
        /// Writes one value in the form the platform parses it from.
        /// </summary>
        /// <param name="writer">Where the value is written.</param>
        /// <param name="value">The value.</param>
        /// <param name="section">The payload section, for a failure message.</param>
        internal static void WriteValue( JsonWriter writer, object value, string section )
        {
            if ( value == null || value == DBNull.Value )
            {
                writer.WriteNull();
                return;
            }

            if ( value is Guid )
            {
                // Lowercase and hyphenated is the form that compares equal to a stored uuid.
                writer.WriteValue( ( ( Guid ) value ).ToString( "D" ) );
                return;
            }

            if ( value is DateTime )
            {
                var time = ( DateTime ) value;

                // The platform reads a time with no offset as UTC; the caller converts.
                if ( time.Kind != DateTimeKind.Utc )
                {
                    throw new InvalidOperationException( string.Format(
                        "a {0} row carries a time that is not UTC, so the platform would read it in its own zone and the value would be wrong by this church's offset",
                        section ) );
                }

                writer.WriteValue( time.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture ) );
                return;
            }

            var guids = value as IEnumerable<Guid>;

            if ( guids != null )
            {
                // The drain reads anything that is not a JSON array as an empty one.
                writer.WriteStartArray();

                foreach ( var guid in guids )
                {
                    writer.WriteValue( guid.ToString( "D" ) );
                }

                writer.WriteEndArray();
                return;
            }

            if ( value is string text )
            {
                writer.WriteValue( WithoutControlCharacters( text ) );
                return;
            }

            if ( value is bool || value is int || value is long || value is short || value is byte || value is decimal || value is double )
            {
                writer.WriteValue( value );
                return;
            }

            // Anything else would be serialized in a shape nobody chose.
            throw new InvalidOperationException( string.Format(
                "a {0} row carries a {1}, which has no agreed form on the wire",
                section,
                value.GetType().Name ) );
        }

        /// <summary>
        /// The text without control characters below a space other than tab, carriage return and
        /// line feed. The platform parses the body as jsonb, which refuses an escaped NUL.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The same string where there is nothing to take out, which is nearly always.</returns>
        private static string WithoutControlCharacters( string text )
        {
            var i = 0;
            while ( i < text.Length && !IsStrippedCharacter( text[i] ) )
            {
                i++;
            }

            if ( i == text.Length )
            {
                return text;
            }

            // Carries on from the first one found, so the string is still read once.
            var kept = new StringBuilder( text, 0, i, text.Length );
            for ( ; i < text.Length; i++ )
            {
                if ( !IsStrippedCharacter( text[i] ) )
                {
                    kept.Append( text[i] );
                }
            }

            return kept.ToString();
        }

        /// <summary>
        /// Whether a character is one <see cref="WithoutControlCharacters"/> takes out.
        /// </summary>
        private static bool IsStrippedCharacter( char character )
        {
            return character < ' ' && character != '\t' && character != '\r' && character != '\n';
        }

        /// <summary>
        /// Moves a stored time, which is in the organization's zone, to UTC.
        /// </summary>
        /// <param name="stored">The time as Rock stores it, or null.</param>
        /// <param name="organizationTimeZone">The zone Rock's stored times are in.</param>
        /// <returns>The same instant in UTC, or null.</returns>
        internal static DateTime? ToUtc( object stored, TimeZoneInfo organizationTimeZone )
        {
            if ( stored == null || stored == DBNull.Value )
            {
                return null;
            }

            var time = ( DateTime ) stored;

            if ( time.Kind == DateTimeKind.Utc )
            {
                return time;
            }

            time = DateTime.SpecifyKind( time, DateTimeKind.Unspecified );

            // A time the clocks skip going forward does not exist and cannot be converted, so it
            // becomes the first minute that does.
            if ( organizationTimeZone.IsInvalidTime( time ) )
            {
                time = time.AddTicks( -( time.Ticks % TimeSpan.TicksPerMinute ) );

                while ( organizationTimeZone.IsInvalidTime( time ) )
                {
                    time = time.AddMinutes( 1 );
                }
            }

            // Treated as UTC it would be wrong by the church's offset, lifting a ban early behind UTC.
            return TimeZoneInfo.ConvertTimeToUtc( time, organizationTimeZone );
        }

        /// <summary>
        /// Splits the joined badge keys into the list the wire carries.
        /// </summary>
        /// <param name="joined">The keys as the procedure returned them.</param>
        /// <returns>The keys; empty, never null, for a person with none.</returns>
        internal static IList<Guid> ReadBadgeKeys( object joined )
        {
            var text = joined as string;

            if ( string.IsNullOrWhiteSpace( text ) )
            {
                return new List<Guid>();
            }

            var keys = new List<Guid>();

            foreach ( var part in text.Split( ',' ) )
            {
                var trimmed = part.Trim();

                if ( trimmed.Length == 0 )
                {
                    continue;
                }

                Guid key;

                // Refused rather than dropped: a dropped key is a badge that quietly stops appearing.
                if ( !Guid.TryParse( trimmed, out key ) )
                {
                    throw new InvalidOperationException( string.Format( "the badge key {0} is not an identifier", trimmed ) );
                }

                keys.Add( key );
            }

            return keys;
        }

        /// <summary>
        /// The colour pair a badge is drawn with.
        /// </summary>
        /// <param name="highlightColor">The colour the church configured, in whatever form.</param>
        /// <returns>The background and the foreground, both null when the colour cannot be read.</returns>
        internal static Tuple<string, string> ReadBadgeColors( object highlightColor )
        {
            var text = ( highlightColor as string ?? string.Empty ).Trim();

            // Free text in Rock. An uncoloured badge still renders; a refused submission takes the
            // church down for the cycle.
            if ( text.Length == 0 || text[0] != '#' )
            {
                return Tuple.Create( ( string ) null, ( string ) null );
            }

            var digits = text.Substring( 1 );

            // The platform accepts only the six digit form.
            if ( digits.Length == 3 )
            {
                digits = new string( new[] { digits[0], digits[0], digits[1], digits[1], digits[2], digits[2] } );
            }

            if ( digits.Length != 6 || !digits.All( Uri.IsHexDigit ) )
            {
                return Tuple.Create( ( string ) null, ( string ) null );
            }

            var red = int.Parse( digits.Substring( 0, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            var green = int.Parse( digits.Substring( 2, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            var blue = int.Parse( digits.Substring( 4, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );

            var luminance = RelativeLuminance( red, green, blue );

            // Black or white by the accessibility contrast ratio, so a colour near the boundary gets
            // the answer a checker would give.
            var contrastWithWhite = 1.05 / ( luminance + 0.05 );
            var contrastWithBlack = ( luminance + 0.05 ) / 0.05;

            var foreground = contrastWithWhite >= contrastWithBlack ? "#ffffff" : "#000000";

            return Tuple.Create( "#" + digits.ToLowerInvariant(), foreground );
        }

        /// <summary>
        /// The relative luminance of a colour, 0 for black and 1 for white.
        /// </summary>
        private static double RelativeLuminance( int red, int green, int blue )
        {
            return ( 0.2126 * Straighten( red ) ) + ( 0.7152 * Straighten( green ) ) + ( 0.0722 * Straighten( blue ) );
        }

        /// <summary>
        /// One channel, 0 to 255, taken out of the display curve onto 0 to 1.
        /// </summary>
        private static double Straighten( int channel )
        {
            var value = channel / 255.0;

            return value <= 0.03928 ? value / 12.92 : Math.Pow( ( value + 0.055 ) / 1.055, 2.4 );
        }

        #endregion Row values

        #region Sync Now

        /// <summary>
        /// Answers a press of Sync Now. A press asks Rock to run the job now, as the Jobs
        /// Administration page does, so the run is the job's own and is recorded in its history.
        /// </summary>
        /// <param name="rockContext">The block's context, for reading the job tables.</param>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        internal static BlockActionResult RequestSyncNow( RockContext rockContext, bool isAuthorized )
        {
            return ToActionResult( RequestSyncNow( ChatPlatformConfigurationService.Read(), isAuthorized, () => ReadJob( rockContext ), QueueRunNow ) );
        }

        /// <summary>
        /// Answers a check on a Sync Now press already made.
        /// </summary>
        /// <param name="rockContext">The block's context, for reading the job tables.</param>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="runMarker">The marker the press returned.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        internal static BlockActionResult GetSyncNowStatus( RockContext rockContext, bool isAuthorized, int runMarker )
        {
            return ToActionResult( GetSyncNowStatus( isAuthorized, runMarker, () => ReadRunAfter( rockContext, runMarker ) ) );
        }

        /// <summary>
        /// Decides a press.
        /// </summary>
        /// <param name="configuration">The church's chat settings as stored.</param>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="readJob">Reads what the job tables say about the sync job, or null when its row is missing.</param>
        /// <param name="queueRunNow">Asks Rock to run the job with the given id now.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        /// <remarks>
        /// Authority is asked first, so a caller who may not press learns nothing about chat's state.
        /// </remarks>
        internal static SyncNowResult RequestSyncNow( ChatPlatformConfiguration configuration, bool isAuthorized, Func<JobSnapshot> readJob, Action<int> queueRunNow )
        {
            if ( !isAuthorized )
            {
                return new SyncNowResult { RefusalMessage = ForbiddenMessage, IsForbidden = true };
            }

            var stored = configuration ?? new ChatPlatformConfiguration();
            if ( !stored.IsConfigured )
            {
                return new SyncNowResult { RefusalMessage = stored.HasBeenEnabled ? UnreadableKeyMessage : NeverEnabledMessage };
            }

            // Read only now: reading the job probes its lock, and a probe as the schedule fires takes
            // the lock and costs the church that run.
            var job = readJob();
            if ( job == null )
            {
                return new SyncNowResult { RefusalMessage = MissingJobMessage };
            }

            if ( job.IsRunning )
            {
                // Refused rather than followed: Rock drops a second run while one holds the lock, the
                // holder read the church before this press, and the lock outlives the run's record
                // while Rock sends the job's notification.
                return new SyncNowResult { RefusalMessage = AlreadyRunningMessage };
            }

            queueRunNow( job.JobId );

            return SyncNowWaiting( job.LatestRunId ?? 0 );
        }

        /// <summary>
        /// Decides a check on a press already made.
        /// </summary>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="runMarker">The marker the press returned.</param>
        /// <param name="readRun">Reads the first run of the sync job recorded after that marker, or null when there is none yet.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        internal static SyncNowResult GetSyncNowStatus( bool isAuthorized, int runMarker, Func<RunSnapshot> readRun )
        {
            if ( !isAuthorized )
            {
                return new SyncNowResult { RefusalMessage = ForbiddenMessage, IsForbidden = true };
            }

            var run = readRun();

            // Checked here as well as by whoever read the run, because the one thing this must never do
            // is report a run that ended before the press as the press's own result.
            if ( run == null || run.Id <= runMarker )
            {
                return SyncNowWaiting( runMarker );
            }

            if ( !run.HasEnded )
            {
                return new SyncNowResult
                {
                    Status = new ChatSyncNowStatusBag { RunMarker = runMarker, Message = RunningMessage }
                };
            }

            // When the schedule fires first and takes the lock, Rock drops the press's run without a
            // record, so this run is the schedule's. It read the church after the press, so its result
            // answers it, unless it skipped for the backoff and sent nothing.
            if ( run.StatusMessage != null && run.StatusMessage.StartsWith( ChatPlatformSync.BackoffSkipPrefix, StringComparison.Ordinal ) )
            {
                return new SyncNowResult
                {
                    Status = new ChatSyncNowStatusBag { RunMarker = runMarker, IsFinished = true, IsFailure = true, Message = OvertakenMessage }
                };
            }

            return new SyncNowResult
            {
                Status = new ChatSyncNowStatusBag
                {
                    RunMarker = runMarker,
                    IsFinished = true,
                    IsFailure = !string.Equals( run.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase ),
                    Message = run.StatusMessage
                }
            };
        }

        /// <summary>
        /// The block's answer to what a press or a check decided, as the block's own
        /// <c>ActionForbidden</c>, <c>ActionBadRequest</c> and <c>ActionOk</c> would build it.
        /// </summary>
        /// <param name="result">The decision.</param>
        /// <returns>A refusal, or the status.</returns>
        internal static BlockActionResult ToActionResult( SyncNowResult result )
        {
            if ( result.IsForbidden )
            {
                return new BlockActionResult( HttpStatusCode.Forbidden ) { Error = result.RefusalMessage };
            }

            if ( result.IsRefused )
            {
                return new BlockActionResult( HttpStatusCode.BadRequest ) { Error = result.RefusalMessage };
            }

            return new BlockActionResult( HttpStatusCode.OK, result.Status, typeof( ChatSyncNowStatusBag ) );
        }

        /// <summary>
        /// Reads what a press needs to know about the sync job.
        /// </summary>
        /// <param name="rockContext">The context to read through.</param>
        /// <returns>The job as it stands, or null when its row is missing.</returns>
        private static JobSnapshot ReadJob( RockContext rockContext )
        {
            var jobId = new ServiceJobService( rockContext ).GetId( SyncJobGuid );
            if ( !jobId.HasValue )
            {
                return null;
            }

            // History before the lock probe, and the order matters. Rock records a run only after it
            // takes the lock, so with the lock free any newer record belongs to a run that started
            // after this read. Probed first, a run starting in between would become the marker, and
            // the press would wait on a record that already exists.
            var latestRunId = new ServiceJobHistoryService( rockContext ).Queryable()
                .Where( history => history.ServiceJobId == jobId.Value )
                .OrderByDescending( history => history.Id )
                .Select( history => ( int? ) history.Id )
                .FirstOrDefault();

            return new JobSnapshot
            {
                JobId = jobId.Value,
                IsRunning = IsJobLocked( jobId.Value ),
                LatestRunId = latestRunId
            };
        }

        /// <summary>
        /// Reads the first run of the sync job recorded after the given marker.
        /// </summary>
        /// <param name="rockContext">The context to read through.</param>
        /// <param name="runMarker">The marker a press returned.</param>
        /// <returns>The run, or null when none has been recorded since, or the job's row is missing.</returns>
        private static RunSnapshot ReadRunAfter( RockContext rockContext, int runMarker )
        {
            // By the job's guid through the history's own join, so a check every few seconds is one
            // query rather than two.
            return new ServiceJobHistoryService( rockContext ).Queryable()
                .Where( history => history.ServiceJob.Guid == SyncJobGuid && history.Id > runMarker )
                .OrderBy( history => history.Id )
                .Select( history => new RunSnapshot
                {
                    Id = history.Id,
                    HasEnded = history.StopDateTime.HasValue,
                    Status = history.Status,
                    StatusMessage = history.StatusMessage
                } )
                .FirstOrDefault();
        }

        /// <summary>
        /// Asks Rock to run the job now, exactly as the Jobs Administration page does.
        /// </summary>
        /// <param name="jobId">The job's id.</param>
        /// <remarks>
        /// That page's request gives the run its own scheduler, whose name tells the job a person is
        /// waiting, so it is marked urgent and not held back by the backoff.
        /// </remarks>
        private static void QueueRunNow( int jobId )
        {
            new ProcessRunJobNow.Message { JobId = jobId }.Send();
        }

        /// <summary>
        /// Whether a run of the job holds its lock right now, on any server.
        /// </summary>
        /// <param name="jobId">The job's id.</param>
        /// <returns>True when a run holds it.</returns>
        /// <remarks>
        /// The lock, not the history, because a run cut off by a restart leaves its record open forever.
        /// </remarks>
        private static bool IsJobLocked( int jobId )
        {
            var lockProvider = RockApp.Current.GetRequiredService<IDistributedLockProvider>();

            using ( var probe = lockProvider.TryAcquire( typeof( RockTriggerListener ), jobId.ToString(), TimeSpan.Zero ) )
            {
                return !probe.IsAcquired;
            }
        }

        /// <summary>
        /// A press that has not yet reached a run that ended.
        /// </summary>
        /// <param name="runMarker">The marker the press is reported from.</param>
        /// <returns>The answer.</returns>
        private static SyncNowResult SyncNowWaiting( int runMarker )
        {
            return new SyncNowResult
            {
                Status = new ChatSyncNowStatusBag { RunMarker = runMarker, Message = WaitingMessage, BudgetMilliseconds = SyncNowBudgetMilliseconds }
            };
        }

        #endregion Sync Now

        #region Transport support

        /// <summary>
        /// A response body as JSON, or null where it is not JSON. Dates are left as written, because
        /// Json.NET would turn an instant with an offset into a local time.
        /// </summary>
        private static JObject ReadBody( HttpResponseMessage response )
        {
            string text;
            try
            {
                text = response.Content == null ? null : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }

            if ( text.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                using ( var reader = new JsonTextReader( new StringReader( text ) ) { DateParseHandling = DateParseHandling.None } )
                {
                    return JObject.Load( reader );
                }
            }
            catch ( JsonException )
            {
                // A gateway that answered in its own words rather than the project's.
                return null;
            }
        }

        /// <summary>
        /// A guid from the wire, or null.
        /// </summary>
        private static Guid? ReadGuid( JToken token )
        {
            Guid parsed;
            return Guid.TryParse( ( string ) token, out parsed ) ? parsed : ( Guid? ) null;
        }

        /// <summary>
        /// An instant from the wire, or null. Parsed round-trip, so the platform's offset is kept.
        /// </summary>
        private static DateTimeOffset? ReadTime( JToken token )
        {
            var value = ( string ) token;
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            DateTimeOffset parsed;
            if ( !DateTimeOffset.TryParse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out parsed ) )
            {
                return null;
            }

            return parsed;
        }

        /// <summary>
        /// Reads one recorded outcome block, or null where there is none. A status read and the
        /// previous outcome an acknowledgement carries are the same shape.
        /// </summary>
        private static Outcome ReadOutcome( JObject outcome, Guid fallbackSubmissionId )
        {
            if ( outcome == null )
            {
                return null;
            }

            return new Outcome
            {
                SubmissionId = ReadGuid( outcome["submission_id"] ) ?? fallbackSubmissionId,
                Status = ParseStatus( ( string ) outcome["status"] ),
                ErrorCode = ( string ) outcome["error_code"]
            };
        }

        /// <summary>
        /// The innermost message of a transport failure; the wrappers above it name this code rather
        /// than what went wrong.
        /// </summary>
        private static string Describe( Exception exception )
        {
            var innermost = exception;
            while ( innermost.InnerException != null )
            {
                innermost = innermost.InnerException;
            }

            return innermost.Message;
        }

        #endregion Transport support

        #region Nested types

        /// <summary>
        /// What the chat platform recorded about one submission, in its own vocabulary.
        /// </summary>
        internal enum SubmissionStatus
        {
            Accepted,

            Refused,

            Applied,

            Failed
        }

        /// <summary>
        /// What became of one submission: a status read, or the previous submission's result carried
        /// on an acknowledgement. The platform builds both from the same history row.
        /// </summary>
        internal class Outcome
        {
            /// <summary>
            /// The submission this describes.
            /// </summary>
            public Guid SubmissionId { get; set; }

            /// <summary>
            /// What was recorded, or null where the platform named a status this build does not know.
            /// </summary>
            public SubmissionStatus? Status { get; set; }

            /// <summary>
            /// The named reason a refused or failed submission carries.
            /// </summary>
            public string ErrorCode { get; set; }
        }

        /// <summary>
        /// What came back from a submission, for a refusal exactly as for an acceptance.
        /// </summary>
        internal sealed class Acknowledgement : Outcome
        {
            /// <summary>
            /// The HTTP status the answer arrived with, or null where no answer arrived.
            /// </summary>
            public int? HttpStatusCode { get; set; }

            /// <summary>
            /// The church's previous submission and what became of it.
            /// </summary>
            public Outcome PreviousOutcome { get; set; }

            /// <summary>
            /// The time the platform would rather not hear from this church before.
            /// </summary>
            public DateTimeOffset? SyncBackoffUntil { get; set; }

            /// <summary>
            /// Why the submission got no answer the platform wrote.
            /// </summary>
            public string TransportDetail { get; set; }

            /// <summary>
            /// True where no answer arrived at all.
            /// </summary>
            public bool IsTransportFailure => !HttpStatusCode.HasValue;

            /// <summary>
            /// Whether this is the platform's own word, and so worth saving. Silence saved as "no
            /// backoff" would lift advice the platform had given.
            /// </summary>
            public bool CarriesBackoffAdvice => !IsTransportFailure && Status.HasValue;
        }

        /// <summary>
        /// How long a run waits to learn what became of its submission. Every figure is an estimate.
        /// </summary>
        internal sealed class PollBudget
        {
            /// <summary>
            /// A person is waiting and has nothing to fall back on, so it waits longer.
            /// </summary>
            public static readonly PollBudget Manual = new PollBudget( TimeSpan.FromSeconds( 3 ), 20, TimeSpan.FromSeconds( 60 ) );

            /// <summary>
            /// Nobody is watching, and the previous outcome is there to fall back on.
            /// </summary>
            public static readonly PollBudget Scheduled = new PollBudget( TimeSpan.FromSeconds( 5 ), 6, TimeSpan.FromSeconds( 30 ) );

            /// <summary>
            /// How long to wait between reads.
            /// </summary>
            public readonly TimeSpan Interval;

            /// <summary>
            /// How many reads at most.
            /// </summary>
            public readonly int MaxAttempts;

            /// <summary>
            /// How long the whole wait may take, which bounds a run of slow reads.
            /// </summary>
            public readonly TimeSpan Duration;

            private PollBudget( TimeSpan interval, int maxAttempts, TimeSpan duration )
            {
                Interval = interval;
                MaxAttempts = maxAttempts;
                Duration = duration;
            }
        }

        /// <summary>
        /// What a Sync Now press or check comes to.
        /// </summary>
        internal sealed class SyncNowResult
        {
            /// <summary>
            /// Why nothing was done, or null where the answer is a status.
            /// </summary>
            public string RefusalMessage { get; set; }

            /// <summary>
            /// Whether the refusal is about the caller's authority rather than the state of chat.
            /// </summary>
            public bool IsForbidden { get; set; }

            /// <summary>
            /// Where the press has got to, when it was not refused.
            /// </summary>
            public ChatSyncNowStatusBag Status { get; set; }

            /// <summary>
            /// Whether nothing was done.
            /// </summary>
            public bool IsRefused => RefusalMessage != null;
        }

        /// <summary>
        /// What Rock's job tables say about the sync job at the moment of a press.
        /// </summary>
        internal sealed class JobSnapshot
        {
            /// <summary>
            /// The job's id.
            /// </summary>
            public int JobId { get; set; }

            /// <summary>
            /// Whether a run of the job holds its lock right now, on any server.
            /// </summary>
            public bool IsRunning { get; set; }

            /// <summary>
            /// The id of the newest run in the job's history, or null when it has never run.
            /// </summary>
            public int? LatestRunId { get; set; }
        }

        /// <summary>
        /// One run of the sync job as its history records it.
        /// </summary>
        internal sealed class RunSnapshot
        {
            /// <summary>
            /// The history record's id.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Whether the run has ended.
            /// </summary>
            public bool HasEnded { get; set; }

            /// <summary>
            /// The status Rock recorded for the run.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// The result the run left.
            /// </summary>
            public string StatusMessage { get; set; }
        }

        /// <summary>
        /// One run's conversation with the chat platform: the credential exchange, the submission and
        /// the status reads, over one HttpClient that lives as long as the run.
        /// </summary>
        /// <remarks>
            /// Nothing here throws at its caller over an answer. The platform refuses with a 422 and its
            /// history row committed, so the body of a 422 is read like any other.
        /// </remarks>
        internal sealed class PlatformClient : IDisposable
        {
            // The submission surface takes the body as one raw text value; a JSON content type is
            // parsed on the way in and the function is never reached.
            private const string SubmitPath = "/rest/v1/rpc/sync_submit";

            private const string StatusPath = "/rest/v1/rpc/sync_status";

            private const string ExchangePath = "/functions/v1/token-exchange";

            // Estimates, revisited when the platform is measured at full scale.
            internal const int TransportAttempts = 3;

            private static readonly TimeSpan TransportRetryDelay = TimeSpan.FromSeconds( 2 );

            // HttpClient's own default, named so the Sync Now budget can count it.
            internal static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds( 100 );

            // A platform token lasts about five minutes, and slow submission attempts followed by the
            // poll can outlast it. An estimate: enough for one more request to arrive before expiry.
            private static readonly TimeSpan TokenRefreshMargin = TimeSpan.FromSeconds( 60 );

            // Closer than this is the time an answer took to arrive, not a clock worth naming.
            private static readonly TimeSpan ClockSkewWorthReporting = TimeSpan.FromSeconds( 30 );

            // Postgres's code for a statement cancelled by its timeout. The data API runs the call in
            // one transaction, so it rolled back whole and the same submission id may be sent again.
            private const string StatementTimeoutCode = "57014";

            private readonly ChatPlatformConfiguration _configuration;

            private readonly HttpClient _httpClient;

            /// <summary>
            /// Creates the client for one run.
            /// </summary>
            /// <param name="configuration">The church's chat settings.</param>
            /// <param name="handler">The transport, or null for the ordinary one.</param>
            public PlatformClient( ChatPlatformConfiguration configuration, HttpMessageHandler handler = null )
            {
                if ( configuration == null )
                {
                    throw new ArgumentNullException( nameof( configuration ) );
                }

                _configuration = configuration;
                _httpClient = handler == null ? new HttpClient() : new HttpClient( handler );
                _httpClient.Timeout = RequestTimeout;

                Wait = duration => Thread.Sleep( duration );
                Clock = () => DateTime.UtcNow;
            }

            /// <summary>
            /// How the client waits. Replaced where a caller does not want a real wait.
            /// </summary>
            public Action<TimeSpan> Wait { get; set; }

            /// <summary>
            /// Where the client reads the time, for the elapsed half of a poll budget.
            /// </summary>
            public Func<DateTime> Clock { get; set; }

            /// <summary>
            /// The platform token the submission and status reads are made under. Set by
            /// <see cref="SignIn"/>; never the church token, which the data API cannot verify.
            /// </summary>
            public string PlatformToken { get; internal set; }

            /// <summary>
            /// When the platform token expires by <see cref="Clock"/>, or null where the exchange did
            /// not say, in which case the token is never exchanged again.
            /// </summary>
            public DateTime? PlatformTokenExpiresAtUtc { get; internal set; }

            /// <summary>
            /// Mints the church's sync token and exchanges it for a platform token.
            /// </summary>
            /// <param name="failure">Why there is no token, when there is none.</param>
            /// <returns>True where the client now holds a platform token.</returns>
            /// <remarks>
            /// Called after the church is read, because a church token lasts minutes and the platform
            /// will not exchange one with under two left, and again by the client itself when the
            /// platform token is near its end.
            /// </remarks>
            public bool SignIn( out string failure )
            {
                var minted = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = _configuration } );

                if ( !minted.Success )
                {
                    failure = "this church could not sign a request to the chat platform";
                    return false;
                }

                return Exchange( minted.ChurchToken, out failure );
            }

            /// <summary>
            /// Exchanges a church token for a platform token.
            /// </summary>
            /// <param name="churchToken">The church's sync token.</param>
            /// <param name="failure">Why there is no token, when there is none.</param>
            /// <returns>True where the client now holds a platform token.</returns>
            public bool Exchange( string churchToken, out string failure )
            {
                PlatformToken = null;
                PlatformTokenExpiresAtUtc = null;

                if ( churchToken.IsNullOrWhiteSpace() )
                {
                    failure = "this church could not sign a request to the chat platform";
                    return false;
                }

                string refusal = null;

                // The same attempts as a submission: the exchange changes nothing, so it is always
                // safe to repeat.
                var unreached = SendWithRetry(
                    () =>
                    {
                        var request = new HttpRequestMessage( HttpMethod.Post, Url( ExchangePath ) );
                        request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + churchToken );
                        request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
                        return request;
                    },
                    ( response, statusCode, body ) =>
                    {
                        var token = ( string ) body?["access_token"];

                        if ( response.IsSuccessStatusCode && token.IsNotNullOrWhiteSpace() )
                        {
                            PlatformToken = token;

                            var expiresIn = ( int? ) body["expires_in"];
                            PlatformTokenExpiresAtUtc = expiresIn.HasValue ? Clock().AddSeconds( expiresIn.Value ) : ( DateTime? ) null;
                            return;
                        }

                        var code = ( string ) body?["error"]?["code"];
                        refusal = "the chat platform refused this church's credential: "
                            + ( code.IsNotNullOrWhiteSpace() ? code : "HTTP " + statusCode )
                            + DescribeClockSkew( code, response.Headers.Date );
                    } );

                failure = unreached == null ? refusal : "the chat platform could not be reached to exchange this church's credential: " + unreached;
                return PlatformToken != null;
            }

            /// <summary>
            /// Sends a request until an answer arrives that says the call completed, waiting between
            /// attempts, and reads that answer.
            /// </summary>
            /// <param name="buildRequest">Builds the request for one attempt, because a request message cannot be sent twice.</param>
            /// <param name="read">Reads a completed answer from the response, its status code and its body.</param>
            /// <returns>Why the last attempt got no completed answer, or null where one did and was read.</returns>
            private string SendWithRetry( Func<HttpRequestMessage> buildRequest, Action<HttpResponseMessage, int, JObject> read )
            {
                string lastFailure = null;

                for ( var attempt = 0; attempt < TransportAttempts; attempt++ )
                {
                    if ( attempt > 0 )
                    {
                        Wait( TransportRetryDelay );
                    }

                    HttpResponseMessage response;
                    try
                    {
                        using ( var request = buildRequest() )
                        {
                            response = _httpClient.SendAsync( request ).GetAwaiter().GetResult();
                        }
                    }
                    catch ( Exception exception )
                    {
                        lastFailure = Describe( exception );
                        continue;
                    }

                    using ( response )
                    {
                        var statusCode = ( int ) response.StatusCode;
                        var body = ReadBody( response );

                        var unfinished = DescribeUnfinishedAnswer( statusCode, body );
                        if ( unfinished != null )
                        {
                            lastFailure = unfinished;
                            continue;
                        }

                        read( response, statusCode, body );
                        return null;
                    }
                }

                return lastFailure;
            }

            /// <summary>
            /// Exchanges again when the platform token is near its end, keeping the token held where
            /// the exchange fails, since it may still be good for the request about to be made.
            /// </summary>
            private void RefreshTokenIfExpiring()
            {
                var isExpiring = PlatformTokenExpiresAtUtc.HasValue && PlatformTokenExpiresAtUtc.Value - Clock() < TokenRefreshMargin;
                if ( !isExpiring )
                {
                    return;
                }

                var heldToken = PlatformToken;
                var heldExpiry = PlatformTokenExpiresAtUtc;

                if ( !SignIn( out _ ) )
                {
                    PlatformToken = heldToken;
                    PlatformTokenExpiresAtUtc = heldExpiry;
                }
            }

            /// <summary>
            /// Where a refusal is about the church token's times, how far this server's clock is from
            /// the platform's, as a clause; otherwise an empty string.
            /// </summary>
            /// <remarks>
            /// The church token's times come from this server's clock, so a clock minutes out is
            /// refused on every run, and the code alone does not say the clock is what to fix.
            /// </remarks>
            private string DescribeClockSkew( string code, DateTimeOffset? platformTime )
            {
                var isAboutTokenTimes = code == "auth.stale_token" || code == "auth.invalid_token";
                if ( !isAboutTokenTimes || !platformTime.HasValue )
                {
                    return string.Empty;
                }

                var offset = platformTime.Value.UtcDateTime - Clock();
                if ( offset.Duration() <= ClockSkewWorthReporting )
                {
                    return string.Empty;
                }

                return string.Format( CultureInfo.InvariantCulture,
                    ", and this server's clock is {0:0} s {1} the chat platform",
                    offset.Duration().TotalSeconds,
                    offset > TimeSpan.Zero ? "behind" : "ahead of" );
            }

            /// <summary>
            /// Why an answer says the call never completed, so the same request may be sent again, or
            /// null where it is an answer to read.
            /// </summary>
            private static string DescribeUnfinishedAnswer( int statusCode, JObject body )
            {
                if ( statusCode == 502 || statusCode == 503 || statusCode == 504 )
                {
                    return "it answered HTTP " + statusCode;
                }

                if ( statusCode == 500 && ( string ) body?["code"] == StatementTimeoutCode )
                {
                    return "it was busy and cancelled the call at its statement timeout (" + StatementTimeoutCode + ")";
                }

                return null;
            }

            /// <summary>
            /// Submits one restatement and reads the acknowledgement, whatever status it arrives with.
            /// </summary>
            /// <param name="submissionId">The idempotency key, used for every attempt.</param>
            /// <param name="payload">The body, as the UTF-8 bytes it was written as.</param>
            /// <param name="headers">The submission headers.</param>
            /// <returns>The acknowledgement. Never null, and never an exception.</returns>
            /// <remarks>
            /// The same id on every attempt: a request that timed out may have arrived, and the
            /// platform answers a repeated id with the outcome it already recorded.
            /// </remarks>
            public Acknowledgement Submit( Guid submissionId, ArraySegment<byte> payload, IDictionary<string, string> headers )
            {
                Acknowledgement acknowledgement = null;

                var unreached = SendWithRetry(
                    () =>
                    {
                        RefreshTokenIfExpiring();
                        return BuildSubmitRequest( submissionId, payload, headers );
                    },
                    ( response, statusCode, body ) => acknowledgement = ReadAcknowledgement( submissionId, statusCode, body ) );

                return acknowledgement ?? new Acknowledgement { SubmissionId = submissionId, TransportDetail = unreached };
            }

            /// <summary>
            /// Reads what became of a submission, waiting inside the budget for the queue to reach it.
            /// </summary>
            /// <param name="submissionId">The submission to read.</param>
            /// <param name="budget">How long to keep asking.</param>
            /// <returns>The outcome, or null where nothing readable came back at all.</returns>
            public Outcome Poll( Guid submissionId, PollBudget budget )
            {
                var start = Clock();
                Outcome outcome = null;
                var attempts = 0;

                while ( true )
                {
                    // A read that could not be made leaves the last one standing.
                    var read = ReadStatus( submissionId );
                    if ( read != null )
                    {
                        outcome = read;
                    }

                    attempts++;

                    if ( outcome != null && outcome.Status.HasValue && outcome.Status.Value != SubmissionStatus.Accepted )
                    {
                        return outcome;
                    }

                    if ( attempts >= budget.MaxAttempts )
                    {
                        return outcome;
                    }

                    Wait( budget.Interval );

                    if ( Clock() - start >= budget.Duration )
                    {
                        return outcome;
                    }
                }
            }

            /// <summary>
            /// Reads what became of a submission, once.
            /// </summary>
            /// <param name="submissionId">The submission to read.</param>
            /// <returns>The outcome, or null where nothing readable came back.</returns>
            public Outcome ReadStatus( Guid submissionId )
            {
                RefreshTokenIfExpiring();

                HttpResponseMessage response;
                try
                {
                    response = _httpClient.SendAsync( BuildStatusRequest( submissionId ) ).GetAwaiter().GetResult();
                }
                catch
                {
                    // The run falls back to what the acknowledgement already told it.
                    return null;
                }

                using ( response )
                {
                    // A status this build does not know is no read at all.
                    var outcome = ReadOutcome( ReadBody( response ), submissionId );

                    return outcome != null && outcome.Status.HasValue ? outcome : null;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _httpClient.Dispose();
            }

            /// <summary>
            /// One submission request. Built per attempt, because a request message cannot be sent
            /// twice; the body buffer is wrapped, not copied.
            /// </summary>
            private HttpRequestMessage BuildSubmitRequest( Guid submissionId, ArraySegment<byte> payload, IDictionary<string, string> headers )
            {
                var content = new ByteArrayContent( payload.Array, payload.Offset, payload.Count );
                content.Headers.ContentType = new MediaTypeHeaderValue( "text/plain" ) { CharSet = "utf-8" };

                var request = new HttpRequestMessage( HttpMethod.Post, Url( SubmitPath ) )
                {
                    Content = content
                };

                foreach ( var header in headers )
                {
                    request.Headers.TryAddWithoutValidation( header.Key, header.Value );
                }

                request.Headers.TryAddWithoutValidation( SubmissionIdHeader, submissionId.ToString() );
                AddCredentials( request );

                return request;
            }

            /// <summary>
            /// The request that asks what became of one submission.
            /// </summary>
            private HttpRequestMessage BuildStatusRequest( Guid submissionId )
            {
                var body = new JObject { ["p_submission_id"] = submissionId.ToString() };

                var request = new HttpRequestMessage( HttpMethod.Post, Url( StatusPath ) )
                {
                    Content = new StringContent( body.ToString( Formatting.None ), Encoding.UTF8, "application/json" )
                };

                AddCredentials( request );

                return request;
            }

            /// <summary>
            /// The project's key and the platform token every data call carries.
            /// </summary>
            private void AddCredentials( HttpRequestMessage request )
            {
                request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + PlatformToken );
            }

            /// <summary>
            /// A platform address, with one slash between the project and the path however the
            /// project url was typed.
            /// </summary>
            private string Url( string path )
            {
                return ( _configuration.ProjectUrl ?? string.Empty ).TrimEnd( '/' ) + path;
            }

            /// <summary>
            /// Reads the acknowledgement out of a response, for a refusal exactly as for an acceptance.
            /// </summary>
            private static Acknowledgement ReadAcknowledgement( Guid submissionId, int statusCode, JObject body )
            {
                if ( body == null )
                {
                    return new Acknowledgement
                    {
                        SubmissionId = submissionId,
                        HttpStatusCode = statusCode,
                        TransportDetail = "the chat platform answered with something that is not an acknowledgement"
                    };
                }

                return new Acknowledgement
                {
                    SubmissionId = ReadGuid( body["submission_id"] ) ?? submissionId,
                    Status = ParseStatus( ( string ) body["status"] ),
                    ErrorCode = ReadErrorCode( body ),
                    HttpStatusCode = statusCode,
                    PreviousOutcome = ReadOutcome( body["previous_outcome"] as JObject, Guid.Empty ),
                    SyncBackoffUntil = ReadTime( body["sync_backoff_until"] )
                };
            }

            /// <summary>
            /// The named reason, from the acknowledgement's own field or from the error shape of the
            /// submissions that cannot own a history row and so raise.
            /// </summary>
            private static string ReadErrorCode( JObject body )
            {
                var code = ( string ) body["error_code"];
                if ( code.IsNotNullOrWhiteSpace() )
                {
                    return code;
                }

                var raised = ( string ) body["message"];

                return raised.IsNotNullOrWhiteSpace() ? raised : null;
            }
        }

        #endregion Nested types
    }
}
