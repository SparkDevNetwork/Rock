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

import { HttpResult } from "@Obsidian/Types/Utility/http";
import { AddressControlBag } from "@Obsidian/ViewModels/Controls/addressControlBag";
import { AddressControlValidateAddressOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/AddressControlValidateAddressOptionsBag";
import { AddressControlValidateAddressResultsBag } from "@Obsidian/ViewModels/Rest/Controls/AddressControlValidateAddressResultsBag";
import { post } from "./http";

export function getDefaultAddressControlModel(): AddressControlBag {
    return {
        state: "AZ",
        country: "US"
    };
}

export function validateAddress(address: AddressControlValidateAddressOptionsBag): Promise<HttpResult<AddressControlValidateAddressResultsBag>> {
    return post<AddressControlValidateAddressResultsBag>("/api/v2/Controls/AddressControlValidateAddress", undefined, address);
}

export function getAddressString(address: AddressControlBag): Promise<HttpResult<string>> {
    return post<string>("/api/v2/Controls/AddressControlGetStreetAddressString", undefined, address);
}