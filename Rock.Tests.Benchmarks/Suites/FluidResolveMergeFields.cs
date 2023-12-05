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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Benchmarks.Suites
{
    /// <summary>
    /// Performs some basic performance tests on the Fluid lava engine with
    /// some different merge templates.
    /// </summary>
    [MemoryDiagnoser(false)]
    [Attributes.OperationsPerSecondColumn]
    [Attributes.SummaryStyle(true, SizeUnit.KB, TimeUnit.Millisecond)]
    public class FluidResolveMergeFields
    {
        private readonly Dictionary<string, object> _mergeFields = new Dictionary<string, object>();

        private readonly Dictionary<string, object> _complexMergeFields = new Dictionary<string, object>();

        private readonly Person _currentPerson = new Person { NickName = "Ted" };

        [GlobalSetup]
        public void Setup()
        {
            LavaService.RockLiquidIsEnabled = false;
            LavaService.SetCurrentEngine(new FluidEngine());
            GlobalAttributesCache.Get().GetValue("DefaultEnabledLavaCommands");

            _complexMergeFields.Add("Items", Enumerable.Range(1, 100).ToList());
        }

        [Benchmark(Baseline = true)]
        public string EmptyString()
        {
            return "".ResolveMergeFields(_mergeFields, _currentPerson);
        }

        [Benchmark]
        public string NoLavaString()
        {
            return "Hello Anonymous".ResolveMergeFields(_mergeFields, _currentPerson);
        }

        [Benchmark]
        public string SimpleLavaString()
        {
            return "Hello {{ 'Unknown' }}".ResolveMergeFields(_mergeFields, _currentPerson);
        }

        [Benchmark]
        public string NickNameLavaString()
        {
            return "Hello {{ CurrentPerson.NickName }}".ResolveMergeFields(_mergeFields, _currentPerson);
        }

        [Benchmark]
        public string ComplexLavaString()
        {
            return "{% assign value = 0 %}{% for item in Items %}{% assign value = value | Plus:item %}{% endfor %}{{ value }}".ResolveMergeFields(_complexMergeFields, _currentPerson);
        }
    }
}
