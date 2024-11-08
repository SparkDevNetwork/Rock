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

namespace Rock.CloudPrint.Shared
{
    internal class PipeRequest
    {
        public int Type { get; set; }
    }

    internal class PipeStatusResponse
    {
        public bool IsConnected { get; set; }

        public DateTimeOffset StartedDateTime { get; set; }

        public DateTimeOffset? ConnectedDateTime { get; set; }

        public int TotalLabelsPrinted { get; set; }
    }
}
