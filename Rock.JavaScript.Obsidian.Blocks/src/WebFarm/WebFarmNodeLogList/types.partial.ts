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

export const enum PreferenceKey {
    FilterDateRange = "filter-date-range",

    FilterNodeName = "filter-node-name",

    FilterWriterNodeName = "filter-writer-node-name",

    FilterSeverity = "filter-severity",

    FilterEventType = "filter-event-type",

    FilterText = "filter-text"
}

export type GridSettingsOptions = {
    dateRange?: string | null;

    nodeName?: string | null;

    writerNodeName?: string | null;

    severity: string | null;

    eventType: string | null;

    text: string | null;
};
