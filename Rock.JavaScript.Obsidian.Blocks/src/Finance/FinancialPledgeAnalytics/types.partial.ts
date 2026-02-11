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
import { NumberRangeModelValue } from "@Obsidian/Types/Controls/numberRangeBox";

export const enum NavigationUrlKey {
    ParentPage = "ParentPage",
    PersonDetailPage = "PersonDetailPage"
}

export type FilterSettings = {
    pledgeDateRange?: SlidingDateRange | null
    givingDateRange?: SlidingDateRange | null
    pledgeAmountRange?: NumberRangeModelValue | undefined;
    givingAmountRange?: NumberRangeModelValue | undefined;
    percentCompleteRange?: NumberRangeModelValue | undefined;
};

export const enum PreferenceKey {
    FilterAccounts = "apAccounts",
    FilterPledgeDateRange = "drpDateRange",
    FilterGivingDateRange = "FilterGivingDateRange",
    FilterPledgeAmount = "nrePledgeAmount",
    FilterPercentComplete = "nrePercentComplete",
    FilterAmountGiven = "nreAmountGiven",
    FilterInclude = "Include"
}
