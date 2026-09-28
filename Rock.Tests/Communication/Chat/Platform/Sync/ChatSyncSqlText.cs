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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What the shipped projection procedure says about itself, read out of its own text.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is scaffolding for the tests that use it and nothing in the run path uses it. The
    ///         job takes its column names from the data reader at the moment it reads, so a second
    ///         copy of the order in shipped code would be a third place for the order to be wrong.
    ///     </para>
    ///     <para>
    ///         The text is the file the migration creates the procedure from, linked into this
    ///         assembly. It is cut into statements the way the server runs them, and each statement is
    ///         one of three kinds: the one that writes the channel mark, the ones that return a result
    ///         set, and the staging between them. The result sets come back in the order they are
    ///         written, which is the order the job reads them: the moment and the identity marks
    ///         first, then one per section in the contract's order. The two sets only a scoped call
    ///         returns each sit in an IF, so they count with the staging, and the result sets here
    ///         are the ones every call returns.
    ///     </para>
    /// </remarks>
    internal static class ChatSyncSqlText
    {
        /// <summary>
        /// The columns that decide whether a group is a chat channel. A second copy of the rule, so
        /// that a test can hold the stamping statement and the staging query to the same words.
        /// </summary>
        public static readonly string[] QualificationColumns = new[]
        {
            "ChatChannelFirstEnabledDateTime",
            "IsChatAllowed",
            "IsChatEnabledOverride",
            "IsChatEnabledForAllGroups"
        };

        private static readonly Regex _projectedAlias = new Regex( @"(?i:\bAS)\s+\[(?<name>[a-z0-9_]+)\]", RegexOptions.Compiled );

        private static readonly Regex _comment = new Regex( @"/\*.*?\*/|--[^\r\n]*", RegexOptions.Compiled | RegexOptions.Singleline );

        private static readonly Regex _intoTemporaryTable = new Regex( @"\bINTO\s+#", RegexOptions.Compiled | RegexOptions.IgnoreCase );

        private static readonly Lazy<IList<string>> _statements = new Lazy<IList<string>>( ReadStatements );

        /// <summary>
        /// The whole procedure, as the migration ships it.
        /// </summary>
        public static string Procedure
        {
            get
            {
                using ( var stream = typeof( ChatSyncSqlText ).Assembly.GetManifestResourceStream( "spChat_SyncProjection.sql" ) )
                {
                    if ( stream == null )
                    {
                        throw new InvalidOperationException( "the projection procedure's text is not linked into this test assembly" );
                    }

                    using ( var reader = new StreamReader( stream, Encoding.UTF8 ) )
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// The statement that writes the channel mark.
        /// </summary>
        public static string Stamp
        {
            get
            {
                return _statements.Value.Single( IsStamp );
            }
        }

        /// <summary>
        /// Every statement that neither writes the mark nor returns a result set, in order.
        /// </summary>
        public static string Staging
        {
            get
            {
                return string.Join( ";" + Environment.NewLine, _statements.Value.Where( s => !IsStamp( s ) && !IsResultSet( s ) ) );
            }
        }

        /// <summary>
        /// The statements that return a result set, in the order the server returns them.
        /// </summary>
        public static IList<string> ResultSets
        {
            get
            {
                return _statements.Value.Where( IsResultSet ).ToList();
            }
        }

        /// <summary>
        /// The statement that returns one payload section.
        /// </summary>
        /// <param name="section">The section, as the wire contract names it.</param>
        /// <returns>The statement.</returns>
        public static string Section( string section )
        {
            var sections = JObject.Parse( ChatWireContract.Json )["payload"]["sections"]
                .Select( s => s.Value<string>() )
                .ToList();

            var position = sections.IndexOf( section );

            if ( position < 0 )
            {
                throw new ArgumentException( string.Format( "the contract has no {0} section", section ), nameof( section ) );
            }

            // The first result set is the moment and the identity marks, so section n is n + 1.
            return ResultSets[position + 1];
        }

        /// <summary>
        /// The columns a section's statement returns, in the order it returns them.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>The column names.</returns>
        public static IList<string> SectionColumns( string section )
        {
            return _projectedAlias.Matches( Section( section ) )
                .Cast<Match>()
                .Select( m => m.Groups["name"].Value )
                .ToList();
        }

        /// <summary>
        /// The procedure cut into its statements, with the comments taken out first so that a
        /// semicolon in a sentence does not end a statement.
        /// </summary>
        private static IList<string> ReadStatements()
        {
            return _comment.Replace( Procedure, string.Empty )
                .Split( ';' )
                .Select( s => s.Trim() )
                .Where( s => s.Length > 0 )
                .ToList();
        }

        private static bool IsStamp( string statement )
        {
            return statement.StartsWith( "UPDATE", StringComparison.OrdinalIgnoreCase );
        }

        private static bool IsResultSet( string statement )
        {
            return statement.StartsWith( "SELECT", StringComparison.OrdinalIgnoreCase ) && !_intoTemporaryTable.IsMatch( statement );
        }
    }
}
