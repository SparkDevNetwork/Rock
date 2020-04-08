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

using NuGet;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// NuGet specific extension methods
    /// </summary>
    public static class NuGetExtensionsMethods
    {
        /// <summary>
        /// Flattens the specified dependency sets.
        /// </summary>
        /// <param name="dependencySets">The dependency sets.</param>
        /// <returns></returns>
        public static string Flatten( this IEnumerable<PackageDependencySet> dependencySets )
        {
            var dependencies = new List<dynamic>();

            foreach ( var dependencySet in dependencySets )
            {
                if ( dependencySet.Dependencies.Count == 0 )
                {
                    dependencies.Add(
                        new
                        {
                            Id = (string)null,
                            VersionSpec = (string)null,
                            TargetFramework =
                        dependencySet.TargetFramework == null ? null : VersionUtility.GetShortFrameworkName( dependencySet.TargetFramework )
                        } );
                }
                else
                {
                    foreach ( var dependency in dependencySet.Dependencies.Select( d => new { d.Id, d.VersionSpec, dependencySet.TargetFramework } ) )
                    {
                        dependencies.Add(
                            new
                            {
                                dependency.Id,
                                VersionSpec = dependency.VersionSpec == null ? null : dependency.VersionSpec.ToString(),
                                TargetFramework =
                            dependency.TargetFramework == null ? null : VersionUtility.GetShortFrameworkName( dependency.TargetFramework )
                            } );
                    }
                }
            }
            return FlattenDependencies( dependencies );
        }

        /// <summary>
        /// Flattens the specified dependencies.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        /// <returns></returns>
        public static string Flatten( this ICollection<PackageDependency> dependencies )
        {
            return
                FlattenDependencies(
                    dependencies.Select(
                        d => new
                        {
                            d.Id,
                            VersionSpec = d.VersionSpec.ToStringSafe(),
                            TargetFramework = "" //d.TargetFramework.ToStringSafe()
                        } ) );
        }

        private static string FlattenDependencies( IEnumerable<dynamic> dependencies )
        {
            return String.Join(
                "|", dependencies.Select( d => String.Format( System.Globalization.CultureInfo.InvariantCulture, "{0}:{1}:{2}", d.Id, d.VersionSpec, d.TargetFramework ) ) );
        }
    }
}
