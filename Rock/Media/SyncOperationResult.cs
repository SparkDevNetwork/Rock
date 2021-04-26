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
using System.Collections.Generic;
using System.Linq;

namespace Rock.Media
{
    /// <summary>
    /// The status result from the various synchronization methods in
    /// <see cref="IMediaAccountComponent"/>.
    /// </summary>
    public sealed class SyncOperationResult
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess => !Errors.Any();

        /// <summary>
        /// Gets or sets the errors that occurred.
        /// </summary>
        /// <value>
        /// The errors that occurred.
        /// </value>
        public IList<string> Errors { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncOperationResult"/> class
        /// that indicates a successful run.
        /// </summary>
        public SyncOperationResult()
        {
            Errors = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncOperationResult"/> class.
        /// </summary>
        /// <param name="errors">The errors that occurred during processing.</param>
        public SyncOperationResult( IEnumerable<string> errors )
        {
            Errors = errors.ToList();
        }

        #endregion
    }
}
