﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace Rock.Utility
{
    /// <summary>
    /// A semantic version implementation.
    /// Conforms to v2.0.0 of http://semver.org/
    /// </summary>
    [Serializable]
    public sealed class RockSemanticVersion : IComparable<RockSemanticVersion>, IComparable, ISerializable
    {
        static Regex parseEx =
            new Regex( @"^(?<major>\d+)" +
                @"(\.(?<minor>\d+))?" +
                @"(\.(?<patch>\d+))?" +
                @"(\-(?<pre>[0-9A-Za-z\-\.]+))?" +
                @"(\+(?<build>[0-9A-Za-z\-\.]+))?$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture );

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSemanticVersion" /> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private RockSemanticVersion( SerializationInfo info, StreamingContext context )
        {
            if ( info == null ) throw new ArgumentNullException( "info" );
            var semVersion = Parse( info.GetString( "SemVersion" ) );
            Major = semVersion.Major;
            Minor = semVersion.Minor;
            Patch = semVersion.Patch;
            Prerelease = semVersion.Prerelease;
            Build = semVersion.Build;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSemanticVersion" /> class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        /// <param name="prerelease">The prerelease version (eg. "alpha").</param>
        /// <param name="build">The build eg ("nightly.232").</param>
        public RockSemanticVersion( int major, int minor = 0, int patch = 0, string prerelease = "", string build = "" )
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;

            // strings are interned to be able to compare by reference in equals method
            this.Prerelease = String.Intern( prerelease ?? "" );
            this.Build = String.Intern( build ?? "" );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSemanticVersion"/> class.
        /// </summary>
        /// <param name="version">The <see cref="System.Version"/> that is used to initialize 
        /// the Major, Minor, Patch and Build properties.</param>
        public RockSemanticVersion( Version version )
        {
            version = version ?? new Version();

            this.Major = version.Major;
            this.Minor = version.Minor;

            if ( version.Revision >= 0 )
            {
                this.Patch = version.Revision;
            }

            this.Prerelease = String.Intern( "" );

            if ( version.Build > 0 )
            {
                this.Build = String.Intern( version.Build.ToString() );
            }
            else
            {
                this.Build = String.Intern( "" );
            }
        }

        /// <summary>
        /// Parses the specified string to a semantic version.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="strict">If set to <c>true</c> minor and patch version are required, else they default to 0.</param>
        /// <returns>The SemVersion object.</returns>
        /// <exception cref="System.InvalidOperationException">When a invalid version string is passed.</exception>
        public static RockSemanticVersion Parse( string version, bool strict = false )
        {
            var match = parseEx.Match( version );
            if ( !match.Success )
                throw new ArgumentException( "Invalid version.", "version" );

            var major = Int32.Parse( match.Groups["major"].Value, CultureInfo.InvariantCulture );

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if ( minorMatch.Success )
                minor = Int32.Parse( minorMatch.Value, CultureInfo.InvariantCulture );
            else if ( strict )
                throw new InvalidOperationException( "Invalid version (no minor version given in strict mode)" );

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if ( patchMatch.Success )
                patch = Int32.Parse( patchMatch.Value, CultureInfo.InvariantCulture );
            else if ( strict )
                throw new InvalidOperationException( "Invalid version (no patch version given in strict mode)" );

            var prerelease = match.Groups["pre"].Value;
            var build = match.Groups["build"].Value;

            return new RockSemanticVersion( major, minor, patch, prerelease, build );
        }

        /// <summary>
        /// Parses the specified string to a semantic version.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="semver">When the method returns, contains a SemVersion instance equivalent 
        /// to the version string passed in, if the version string was valid, or <c>null</c> if the 
        /// version string was not valid.</param>
        /// <param name="strict">If set to <c>true</c> minor and patch version are required, else they default to 0.</param>
        /// <returns><c>False</c> when a invalid version string is passed, otherwise <c>true</c>.</returns>
        public static bool TryParse( string version, out RockSemanticVersion semver, bool strict = false )
        {
            try
            {
                semver = Parse( version, strict );
                return true;
            }
            catch ( Exception )
            {
                semver = null;
                return false;
            }
        }

        /// <summary>
        /// Tests the specified versions for equality.
        /// </summary>
        /// <param name="versionA">The first version.</param>
        /// <param name="versionB">The second version.</param>
        /// <returns>If versionA is equal to versionB <c>true</c>, else <c>false</c>.</returns>
        public static bool Equals( RockSemanticVersion versionA, RockSemanticVersion versionB )
        {
            if ( ReferenceEquals( versionA, null ) )
                return ReferenceEquals( versionB, null );
            return versionA.Equals( versionB );
        }

        /// <summary>
        /// Compares the specified versions.
        /// </summary>
        /// <param name="versionA">The version to compare to.</param>
        /// <param name="versionB">The version to compare against.</param>
        /// <returns>If versionA &lt; versionB <c>-1</c>, if versionA &gt; versionB <c>1</c>,
        /// if versionA is equal to versionB <c>0</c>.</returns>
        public static int Compare( RockSemanticVersion versionA, RockSemanticVersion versionB )
        {
            if ( ReferenceEquals( versionA, null ) )
                return ReferenceEquals( versionB, null ) ? 0 : -1;
            return versionA.CompareTo( versionB );
        }

        /// <summary>
        /// Make a copy of the current instance with optional altered fields. 
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        /// <param name="prerelease">The prerelease text.</param>
        /// <param name="build">The build text.</param>
        /// <returns>The new version object.</returns>
        public RockSemanticVersion Change( int? major = null, int? minor = null, int? patch = null,
            string prerelease = null, string build = null )
        {
            return new RockSemanticVersion(
                major ?? this.Major,
                minor ?? this.Minor,
                patch ?? this.Patch,
                prerelease ?? this.Prerelease,
                build ?? this.Build );
        }

        /// <summary>
        /// Gets the major version.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        public int Major { get; private set; }

        /// <summary>
        /// Gets the minor version.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        public int Minor { get; private set; }

        /// <summary>
        /// Gets the patch version.
        /// </summary>
        /// <value>
        /// The patch version.
        /// </value>
        public int Patch { get; private set; }

        /// <summary>
        /// Gets the pre-release version.
        /// </summary>
        /// <value>
        /// The pre-release version.
        /// </value>
        public string Prerelease { get; private set; }

        /// <summary>
        /// Gets the build version.
        /// </summary>
        /// <value>
        /// The build version.
        /// </value>
        public string Build { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var version = "" + Major + "." + Minor + "." + Patch;
            if ( !String.IsNullOrEmpty( Prerelease ) )
                version += "-" + Prerelease;
            if ( !String.IsNullOrEmpty( Build ) )
                version += "+" + Build;
            return version;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the 
        /// other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has these meanings: Value Meaning Less than zero 
        ///  This instance precedes <paramref name="obj" /> in the sort order. 
        ///  Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. i
        ///  Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        public int CompareTo( object obj )
        {
            return CompareTo( (RockSemanticVersion)obj );
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the 
        /// other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has these meanings: Value Meaning Less than zero 
        ///  This instance precedes <paramref name="other" /> in the sort order. 
        ///  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. i
        ///  Greater than zero This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo( RockSemanticVersion other )
        {
            if ( ReferenceEquals( other, null ) )
                return 1;

            var r = this.CompareByPrecedence( other );
            if ( r != 0 )
                return r;

            r = CompareComponent( this.Build, other.Build );
            return r;
        }

        /// <summary>
        /// Compares to semantic versions by precedence. This does the same as a Equals, but ignores the build information.
        /// </summary>
        /// <param name="other">The semantic version.</param>
        /// <returns><c>true</c> if the version precedence matches.</returns>
        public bool PrecedenceMatches( RockSemanticVersion other )
        {
            return CompareByPrecedence( other ) == 0;
        }

        /// <summary>
        /// Compares to semantic versions by precedence. This does the same as a Equals, but ignores the build information.
        /// </summary>
        /// <param name="other">The semantic version.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has these meanings: Value Meaning Less than zero 
        ///  This instance precedes <paramref name="other" /> in the version precedence.
        ///  Zero This instance has the same precedence as <paramref name="other" />. i
        ///  Greater than zero This instance has creater precedence as <paramref name="other" />.
        /// </returns>
        public int CompareByPrecedence( RockSemanticVersion other )
        {
            if ( ReferenceEquals( other, null ) )
                return 1;

            var r = this.Major.CompareTo( other.Major );
            if ( r != 0 ) return r;

            r = this.Minor.CompareTo( other.Minor );
            if ( r != 0 ) return r;

            r = this.Patch.CompareTo( other.Patch );
            if ( r != 0 ) return r;

            r = CompareComponent( this.Prerelease, other.Prerelease, true );
            return r;
        }

        static int CompareComponent( string a, string b, bool lower = false )
        {
            var aEmpty = String.IsNullOrEmpty( a );
            var bEmpty = String.IsNullOrEmpty( b );
            if ( aEmpty && bEmpty )
                return 0;

            if ( aEmpty )
                return lower ? 1 : -1;
            if ( bEmpty )
                return lower ? -1 : 1;

            var aComps = a.Split( '.' );
            var bComps = b.Split( '.' );

            var minLen = Math.Min( aComps.Length, bComps.Length );
            for ( int i = 0; i < minLen; i++ )
            {
                var ac = aComps[i];
                var bc = bComps[i];
                int anum, bnum;
                var isanum = Int32.TryParse( ac, out anum );
                var isbnum = Int32.TryParse( bc, out bnum );
                if ( isanum && isbnum )
                    return anum.CompareTo( bnum );
                if ( isanum )
                    return -1;
                if ( isbnum )
                    return 1;
                var r = String.CompareOrdinal( ac, bc );
                if ( r != 0 )
                    return r;
            }

            return aComps.Length.CompareTo( bComps.Length );
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( obj, null ) )
                return false;

            if ( ReferenceEquals( this, obj ) )
                return true;

            var other = (RockSemanticVersion)obj;

            // do string comparison by reference (possible because strings are interned in ctor)
            return this.Major == other.Major &&
                this.Minor == other.Minor &&
                this.Patch == other.Patch &&
                ReferenceEquals( this.Prerelease, other.Prerelease ) &&
                ReferenceEquals( this.Build, other.Build );
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = this.Major.GetHashCode();
                result = result * 31 + this.Minor.GetHashCode();
                result = result * 31 + this.Patch.GetHashCode();
                result = result * 31 + this.Prerelease.GetHashCode();
                result = result * 31 + this.Build.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
        /// <exception cref="System.ArgumentNullException">info</exception>
        [SecurityPermission( SecurityAction.Demand, SerializationFormatter = true )]
        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            if ( info == null ) throw new ArgumentNullException( "info" );
            info.AddValue( "SemVersion", ToString() );
        }

        /// <summary>
        /// Implicit conversion from string to SemVersion.
        /// </summary>
        /// <param name="version">The semantic version.</param>
        /// <returns>The SemVersion object.</returns>
        public static implicit operator RockSemanticVersion( string version )
        {
            return RockSemanticVersion.Parse( version );
        }

        /// <summary>
        /// The override of the equals operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator ==( RockSemanticVersion left, RockSemanticVersion right )
        {
            return RockSemanticVersion.Equals( left, right );
        }

        /// <summary>
        /// The override of the un-equal operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is not equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator !=( RockSemanticVersion left, RockSemanticVersion right )
        {
            return !RockSemanticVersion.Equals( left, right );
        }

        /// <summary>
        /// The override of the greater operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator >( RockSemanticVersion left, RockSemanticVersion right )
        {
            return RockSemanticVersion.Compare( left, right ) == 1;
        }

        /// <summary>
        /// The override of the greater than or equal operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than or equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator >=( RockSemanticVersion left, RockSemanticVersion right )
        {
            return left == right || left > right;
        }

        /// <summary>
        /// The override of the less operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator <( RockSemanticVersion left, RockSemanticVersion right )
        {
            return RockSemanticVersion.Compare( left, right ) == -1;
        }

        /// <summary>
        /// The override of the less than or equal operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than or equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator <=( RockSemanticVersion left, RockSemanticVersion right )
        {
            return left == right || left < right;
        }
    }
}