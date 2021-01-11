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
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction to rebuild streak type data from attendance data
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    [Obsolete( "Use ProcessRebuildStreakType Task instead." )]
    [RockObsolete( "1.13" )]
    public class StreakTypeRebuildTransaction : ITransactionWithProgress
    {
        /// <summary>
        /// Gets the progress. Should report between 0 and 100 to represent the percent complete or
        /// null if the progress cannot be calculated.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        public Progress<int?> Progress { get; private set; }

        /// <summary>
        /// Gets the streak type identifier.
        /// </summary>
        private int StreakTypeId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreakTypeRebuildTransaction"/> class.
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        public StreakTypeRebuildTransaction( int streakTypeId )
        {
            StreakTypeId = streakTypeId;
            Progress = new Progress<int?>();
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            ReportProgress( 0 );
            StreakTypeService.RebuildStreakType( Progress, StreakTypeId, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( errorMessage );
            }

            ReportProgress( 100 );
        }

        /// <summary>
        /// Reports the progress.
        /// </summary>
        /// <param name="progress">The progress.</param>
        private void ReportProgress(int progress)
        {
            ( ( IProgress<int?> ) Progress ).Report( progress );
        }
    }
}