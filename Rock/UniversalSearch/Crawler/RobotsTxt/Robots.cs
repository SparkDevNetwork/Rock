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
using System.Linq;

using Rock.UniversalSearch.Crawler.RobotsTxt.Enums;

namespace Rock.UniversalSearch.Crawler.RobotsTxt
{
    /// <summary>
    /// Provides functionality for parsing a robots.txt file's content and querying the rules and directives inside it.
    /// </summary>
    public class Robots 
    {

        /// <summary>
        /// Gets the raw contents of the robots.txt file.
        /// </summary>
        public string Raw { get; private set; }

        /// <summary>
        /// Gets the list of sitemaps declared in the file.
        /// </summary>
        public List<Sitemap> Sitemaps { get; private set; }

        /// <summary>
        /// Indicates whether the file has any lines which can't be understood.
        /// </summary>
        public bool Malformed { get; private set; }

        /// <summary>
        /// Indicates whether the file has any rules.
        /// </summary>
        public bool HasRules { get; private set; }

        /// <summary>
        /// Indicates whether there are any disallowed paths.
        /// </summary>
        public bool IsAnyPathDisallowed { get; private set; }

        /// <summary>
        /// How to support the Allow directive. Defaults to <see cref="RobotsTxt.Enums.AllowRuleImplementation.MoreSpecific"/>.
        /// </summary>
        public AllowRuleImplementation AllowRuleImplementation { get; set; }

        // We could just have a List<Rules>, since Rule is the base class for AccessRule & CrawlDelayRule... 
        // But IsPathAllowed() and CrawlDelay() functions need these specific collections everytime they're called, so
        // it saves us some time to have them pre-populated instead of extracting these lists from a List<Rule> everytime those functions are called.
        private List<AccessRule> globalAccessRules;
        private List<AccessRule> specificAccessRules;
        private List<CrawlDelayRule> crawlDelayRules;

        /// <summary>
        /// Initializes a new <see cref="RobotsTxt.Robots"/> instance for the given robots.txt file content.
        /// </summary>
        public static Robots Load( string content )
        {
            return new Robots( content );
        }

        /// <summary>
        /// Initializes a new <see cref="RobotsTxt.Robots"/> instance for the given robots.txt file content.
        /// </summary>
        /// <param name="content">Content of the robots.txt file.</param>
        public Robots( string content )
        {
            AllowRuleImplementation = AllowRuleImplementation.MoreSpecific;
            HasRules = false;
            IsAnyPathDisallowed = false;
            Malformed = false;
            Raw = content;
            if ( String.IsNullOrWhiteSpace( content ) )
            {
                return;
            }

            string[] lines = content
                .Split( Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries )
                .Where( l => !String.IsNullOrWhiteSpace( l ) )
                .ToArray();
            if ( lines.Length == 0 )
            {
                return;
            }
            readLines( lines );
        }

        private void readLines( string[] lines )
        {
            globalAccessRules = new List<AccessRule>();
            specificAccessRules = new List<AccessRule>();
            crawlDelayRules = new List<CrawlDelayRule>();
            Sitemaps = new List<Sitemap>();
            string userAgent = String.Empty;

            int ruleCount = 0;
            for ( int i = 0; i < lines.Length; i++ )
            {
                var line = lines[i];
                var robotsLine = new Line( line );
                switch ( robotsLine.Type )
                {
                    case LineType.Comment: //ignore the comments
                        continue;
                    case LineType.UserAgent:
                        userAgent = robotsLine.Value;
                        continue;
                    case LineType.Sitemap:
                        Sitemaps.Add( Sitemap.FromLine( robotsLine ) );
                        continue;
                    case LineType.AccessRule:
                    case LineType.CrawlDelayRule:
                        //if there's a rule without user-agent declaration, ignore it
                        if ( String.IsNullOrEmpty( userAgent ) )
                        {
                            Malformed = true;
                            continue;
                        }
                        if ( robotsLine.Type == LineType.AccessRule )
                        {
                            var accessRule = new AccessRule( userAgent, robotsLine, ++ruleCount );
                            if ( accessRule.For.Equals( "*" ) )
                            {
                                globalAccessRules.Add( accessRule );
                            }
                            else
                            {
                                specificAccessRules.Add( accessRule );
                            }
                            if ( !accessRule.Allowed && !String.IsNullOrEmpty( accessRule.Path ) )
                            {
                                // We say !String.IsNullOrEmpty(x.Path) because the rule "Disallow: " means nothing is disallowed.
                                IsAnyPathDisallowed = true;
                            }
                        }
                        else
                        {
                            crawlDelayRules.Add( new CrawlDelayRule( userAgent, robotsLine, ++ruleCount ) );
                        }
                        HasRules = true;
                        continue;
                    case LineType.Unknown:
                        Malformed = true;
                        continue;
                }
            }
        }

        /// <summary>
        /// Checks if the given user-agent can access the given relative path.
        /// </summary>
        /// <param name="userAgent">User agent string.</param>
        /// <param name="path">Relative path.</param>
        /// <exception cref="System.ArgumentException">Thrown when userAgent parameter is null, 
        /// empty or consists only of white-space characters.</exception>
        public bool IsPathAllowed( string userAgent, string path )
        {
            if ( String.IsNullOrWhiteSpace( userAgent ) )
            {
                throw new ArgumentException( "Not a valid user-agent string.", "userAgent" );
            }
            if ( !HasRules || !IsAnyPathDisallowed )
            {
                return true;
            }

            path = normalizePath( path );
            var rulesForThisRobot = specificAccessRules.FindAll( x => userAgent.IndexOf( x.For, StringComparison.InvariantCultureIgnoreCase ) >= 0 );
            if ( globalAccessRules.Count == 0 && rulesForThisRobot.Count == 0 )
            {
                // no rules for this robot
                return true;
            }
            // If there are rules for this robot, we should only check against them. 
            // If not, we check against the global rules.
            // (though some robots ignore the rest after reading the rules for *)
            // We say "String.IsNullOrEmpty(x.Path)" while filtering because "Disallow: " means "Allow all".
            // And the reason we remove the first characters of the paths before calling IsPathMatch() is because the first characters will always be '/',
            // so there is no point having IsPathMatch() compare them.
            var matchingRules = rulesForThisRobot.Count > 0 ?
                rulesForThisRobot.FindAll( x => String.IsNullOrEmpty( x.Path ) || isPathMatch( path.Substring( 1 ), x.Path.Substring( 1 ) ) )
                : globalAccessRules.FindAll( x => String.IsNullOrEmpty( x.Path ) || isPathMatch( path.Substring( 1 ), x.Path.Substring( 1 ) ) );

            if ( matchingRules.Count == 0 )
            {
                return true;
            }

            AccessRule ruleToUse;
            if ( AllowRuleImplementation == AllowRuleImplementation.MoreSpecific )
            {
                ruleToUse = matchingRules.OrderByDescending( x => x.Path.Length ).ThenBy( x => x.Order ).First();
            }
            else
            {
                ruleToUse = matchingRules.OrderBy( x => x.Order ).First();
            }

            switch ( AllowRuleImplementation )
            {
                case AllowRuleImplementation.Standard:
                    return String.IsNullOrEmpty( ruleToUse.Path ) || ruleToUse.Allowed;
                case AllowRuleImplementation.AllowOverrides:
                    // check if there's any allow rule, if not follow the first disallow rule.
                    // (again, "disallow:" means allow. which is why String.IsNullOrEmpty(ruleToUse.Path))
                    return matchingRules.Any( x => x.Allowed ) || String.IsNullOrEmpty( ruleToUse.Path );
                case AllowRuleImplementation.MoreSpecific:
                    return String.IsNullOrEmpty( ruleToUse.Path ) || ruleToUse.Allowed;
            }

            return false;
        }

        /// <summary>
        /// Gets the number of milliseconds to wait between successive requests for this robot.
        /// </summary>
        /// <param name="userAgent">User agent string.</param>
        /// <returns>Returns zero if there's not any matching crawl-delay rules for this robot.</returns>
        /// <exception cref="System.ArgumentException">Thrown when userAgent parameter is null, 
        /// empty or consists only of white-space characters.</exception>
        public long CrawlDelay( string userAgent )
        {
            if ( String.IsNullOrWhiteSpace( userAgent ) )
            {
                throw new ArgumentException( "Not a valid user-agent string.", "userAgent" );
            }
            if ( !HasRules || crawlDelayRules.Count == 0 )
            {
                return 0;
            }
            var rulesForAllRobots = crawlDelayRules.FindAll( x => x.For.Equals( "*" ) );
            var rulesForThisRobot = crawlDelayRules.FindAll( x => x.For.IndexOf( userAgent, StringComparison.InvariantCultureIgnoreCase ) >= 0 );
            if ( rulesForAllRobots.Count == 0 && rulesForThisRobot.Count == 0 )
            {
                return 0;
            }
            return rulesForThisRobot.Count > 0 ? rulesForThisRobot.First().Delay : rulesForAllRobots.First().Delay;
        }

        #region helper methods
        static bool isPathMatch( string path, string rulePath )
        {
            int rulePathLength = rulePath.Length;
            for ( int i = 0; i < rulePathLength; i++ )
            {
                var c = rulePath[i];
                if ( c.Equals( '$' ) && i == rulePathLength - 1 )
                {
                    // If the '$' wildcard is the last character of the rulePath and if the path has one less character than rulePath,
                    // then it means the end of path matched the rulePath.
                    return i == path.Length;
                }
                if ( c.Equals( '*' ) )
                {
                    if ( i == rulePathLength - 1 )
                    {
                        // Return true when '*' is the last char of rulePath because it doesn't matter what the rest of path is in this situation.
                        // (example : when rulePath is "/foo*" and path is "/foobar")
                        return true;
                    }
                    for ( int j = i; j < path.Length; j++ )
                    {
                        // When the '*' wildcard is not the last char,
                        // recursively call the method to see if the part of rulePath after '*' and the rest of the path matches.
                        if ( isPathMatch( path.Substring( j ), rulePath.Substring( i + 1 ) ) )
                        {
                            return true;
                        }
                    }
                    // There's no match between the rest of the paths...
                    return false;
                }
                // When the char is not a wild card, check if path has any chars left to compare to.
                // And we return false when path has no more chars to compare to or if the comparison with the current char fails.
                if ( i >= path.Length || !c.Equals( path[i] ) )
                {
                    return false;
                }
            }
            // Ran out of rulePath characters... If the rest matches, that's good enough.
            // (example : when rulePath is "/foo/" and path is "/foo/bar")
            return path.StartsWith( rulePath, StringComparison.Ordinal );
        }

        static string normalizePath( string path )
        {
            if ( String.IsNullOrWhiteSpace( path ) ) path = "/";
            if ( !path.StartsWith( "/", StringComparison.Ordinal ) )
            {
                path = "/" + path;
            }
            while ( path.IndexOf( "//", StringComparison.Ordinal ) >= 0 )
            {
                path = path.Replace( "//", "/" );
            }
            return path;
        }
        #endregion
    }
}