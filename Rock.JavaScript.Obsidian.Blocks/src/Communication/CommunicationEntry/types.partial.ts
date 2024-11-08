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

export type MediumType = "email" | "sms" | "push" | "unknown";

export type LabelType = "label-default" | "label-success" | "label-warning" | "label-info" | "label-danger";

export enum AgeClassification {
    Unknown = 0,
    Adult = 1,
    Child = 2
}

export type PersonSearchResult = {
    guid?: string | null;
    primaryAliasGuid?: string | null;
    name?: string | null;
    isActive?: boolean;
    isDeceased?: boolean;
    isBusiness?: boolean;
    imageUrl?: string | null;
    age?: number | null;
    formattedAge?: string | null;
    ageClassification?: AgeClassification;
    gender?: string | null;
    connectionStatus?: string | null;
    connectionStatusColor?: string | null;
    recordStatus?: string | null;
    email?: string | null;
    spouseName?: string | null;
    spouseNickName?: string | null;
    address?: string | null;
    phoneNumbers?: PersonSearchPhoneNumber[] | null;
    campusShortCode?: string | null;
};

export type PersonSearchPhoneNumber = {
    type?: string | null;
    number?: string | null;
    isUnlisted?: boolean;
};

export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "unknown";

export type BreakpointHelper = {
    breakpoint: Breakpoint;

    isXs: boolean;
    isSm: boolean;
    isMd: boolean;
    isLg: boolean;
    isXl: boolean;

    isXsOrSmaller: boolean;
    isSmOrSmaller: boolean;
    isMdOrSmaller: boolean;
    isLgOrSmaller: boolean;
    isXlOrSmaller: boolean;

    isXsOrLarger: boolean;
    isSmOrLarger: boolean;
    isMdOrLarger: boolean;
    isLgOrLarger: boolean;
    isXlOrLarger: boolean;
};