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

import { SlidingDateRange } from "@Obsidian/Utility/slidingDateRange";
import { PersonFieldBag } from "@Obsidian/ViewModels/Core/Grid/personFieldBag";

export const enum PreferenceKey {
    FilterSendDateRange = "filter-send-date-range",
    FilterUnsubscribeDateRange = "filter-unsubscribe-date-range",
    FilterUnsubscribeLevel = "filter-unsubscribe-level",
}

export type GridSettingsOptions = {
    sendDateRange: SlidingDateRange | null;
    unsubscribeDateRange: SlidingDateRange | null;
    unsubscribeLevel: string;
};

export type Row = {
    idKey: string;
    recipientPerson: PersonFieldBag;
    sendDateTime: string;
    unsubscribeDateTime: string;
    unsubscribeLevel: string;
    communicationName?: string | null;
    topic?: string | null;
    sentByPerson?: PersonFieldBag | null;
};
