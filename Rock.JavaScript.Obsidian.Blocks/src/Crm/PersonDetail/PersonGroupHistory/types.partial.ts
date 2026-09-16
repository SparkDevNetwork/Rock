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

// The types below mirror the property and method names dictated by the external
// rock-swimlanes library (PascalCase data keys, snake_case methods) and augment the
// global Window, so they intentionally do not follow Rock's camelCase conventions.
/* eslint-disable @typescript-eslint/naming-convention */

/**
 * The timeline view modes supported by the swimlanes visualization.
 */
export const enum ViewMode {
    Month = "Month",
    Year = "Year"
}

/**
 * A single membership period within a swimlanes group, in the PascalCase
 * shape the rock-swimlanes library reads.
 */
export type SwimlanesTask = {
    GroupName?: string | null;
    StartDateTime?: string | null;
    StopDateTime?: string | null;
    IsLeader: boolean;
    GroupRoleName?: string | null;
};

/**
 * A single group lane, in the PascalCase shape the rock-swimlanes library reads.
 */
export type SwimlanesGroup = {
    GroupId: string;
    GroupTypeId: number;
    GroupTypeColor?: string | null;
    GroupTypeName?: string | null;
    StartStopHistory: SwimlanesTask[];
};

/**
 * The subset of the rock-swimlanes instance API used by this block.
 */
export type SwimlanesInstance = {
    change_view_mode(mode: string): void;
    refresh(lanes: SwimlanesGroup[]): void;
    clear(): void;
};

declare global {
    interface Window {
        /** The rock-swimlanes constructor, loaded at runtime from /Scripts/rock-swimlanes.min.js. */
        Swimlanes: new (element: HTMLElement, lanes: SwimlanesGroup[], options: { view_mode: string }) => SwimlanesInstance;

        /** The current mouse position the swimlanes library reads to position its popup. */
        currentMousePos: { x: number; y: number };
    }
}
