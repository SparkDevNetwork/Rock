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
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

namespace Rock.Tests.Benchmarks.Attributes
{
    /// <summary>
    /// Defines the summary style to use when displaying the results of the test.
    /// </summary>
    internal class SummaryStyleAttribute : System.Attribute, IConfigSource
    {
        /// <inheritdoc/>
        public IConfig Config { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryStyleAttribute"/> class.
        /// </summary>
        /// <param name="printUnitsInHeader">If set to <c>true</c> then the unit of measurement will be printed in the header.</param>
        /// <param name="sizeUnit">The size unit for the results.</param>
        /// <param name="timeUnit">The time unit for the results.</param>
        /// <param name="printUnitsInContent">If set to <c>true</c> then the unit of measurement will be printed in the content.</param>
        /// <param name="printZeroValuesInContent">If set to <c>true</c> then zero values will be printed in the content.</param>
        /// <param name="maxParameterColumnWidth">Maximum width of the parameter column.</param>
        /// <param name="ratioStyle">The ratio style.</param>
        public SummaryStyleAttribute(bool printUnitsInHeader, SizeUnit sizeUnit, TimeUnit timeUnit, bool printUnitsInContent = true, bool printZeroValuesInContent = false, int maxParameterColumnWidth = 20, RatioStyle ratioStyle = RatioStyle.Value)
        {
            BenchmarkDotNet.Columns.SizeUnit su;
            Perfolizer.Horology.TimeUnit tu;

            switch (sizeUnit)
            {
                case SizeUnit.B:
                    su = BenchmarkDotNet.Columns.SizeUnit.B;
                    break;

                case SizeUnit.KB:
                    su = BenchmarkDotNet.Columns.SizeUnit.KB;
                    break;

                case SizeUnit.MB:
                    su = BenchmarkDotNet.Columns.SizeUnit.MB;
                    break;

                case SizeUnit.GB:
                    su = BenchmarkDotNet.Columns.SizeUnit.GB;
                    break;

                case SizeUnit.TB:
                    su = BenchmarkDotNet.Columns.SizeUnit.TB;
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException(nameof(sizeUnit));
            }

            switch (timeUnit)
            {
                case TimeUnit.Nanosecond:
                    tu = Perfolizer.Horology.TimeUnit.Nanosecond;
                    break;

                case TimeUnit.Microsecond:
                    tu = Perfolizer.Horology.TimeUnit.Microsecond;
                    break;

                case TimeUnit.Millisecond:
                    tu = Perfolizer.Horology.TimeUnit.Millisecond;
                    break;

                case TimeUnit.Second:
                    tu = Perfolizer.Horology.TimeUnit.Second;
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException(nameof(timeUnit));
            }

            Config = ManualConfig.CreateEmpty()
                .WithSummaryStyle(new SummaryStyle(null, printUnitsInHeader, su, tu, printUnitsInContent, printZeroValuesInContent, maxParameterColumnWidth, ratioStyle));
        }
    }
}
