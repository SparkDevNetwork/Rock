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
using BenchmarkDotNet.Attributes;

namespace Rock.Tests.UnitTests.Performance.Attributes
{
    /// <summary>
    /// Includes the Operations Per Second column in the summary report.
    /// </summary>
    internal class OperationsPerSecondColumnAttribute : ColumnConfigBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationsPerSecondColumnAttribute"/> class.
        /// </summary>
        public OperationsPerSecondColumnAttribute()
            : base( BenchmarkDotNet.Columns.StatisticColumn.OperationsPerSecond )
        {
        }
    }
}
