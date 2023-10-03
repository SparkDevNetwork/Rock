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

namespace Rock.Tests.Integration
{
    /// <summary>
    /// Specifies the database initialization state needed to execute a set of tests.
    /// </summary>
    public enum DatabaseInitializationStateSpecifier
    {
        /// <summary>
        /// The associated tests do not require a database.
        /// </summary>
        None,
        /// <summary>
        /// The associated tests require a newly-initialized database containing no additional data.
        /// </summary>
        New,
        /// <summary>
        /// The associated tests require a database containing the Rock sample data.
        /// </summary>
        SampleData,
        /// <summary>
        /// The associated tests require a valid Rock database which may contain any pre-existing data.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Sets the requirement that the tests in a class require an initialized sample database.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    internal class DatabaseInitializationStateAttribute : System.Attribute
    {
        /// <summary>
        /// The database state required to execute the tests.
        /// </summary>
        public DatabaseInitializationStateSpecifier RequiredState { get; private set; }

        #region Constructors

        /// <summary>
        /// Apply the attribute.
        /// </summary>
        /// <param name="requiredState"></param>
        public DatabaseInitializationStateAttribute( DatabaseInitializationStateSpecifier requiredState )
        {
            RequiredState = requiredState;
        }

        #endregion
    }
}