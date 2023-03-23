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

import { GroupAttendanceDetailAttendanceBag } from "@Obsidian/ViewModels/Blocks/Groups/GroupAttendanceDetail/groupAttendanceDetailAttendanceBag";

export type AttendanceFilterByDelegate = (attendance: GroupAttendanceDetailAttendanceBag) => boolean;

export type AttendanceGroupByDelegate = (attendance: GroupAttendanceDetailAttendanceBag) => string;

export type AttendanceSortByDelegate = (attendance1: GroupAttendanceDetailAttendanceBag, attendance2: GroupAttendanceDetailAttendanceBag) => number;

export type SwitchPosition = "on" | "off";

export type Switch = {
    /**
     * Determines if the switch is disabled.
     *
     * Use `enable()` and `disable()` to control this property.
     */
    readonly isDisabled: boolean,

    /**
     * Determines if the switch is on or off.
     *
     * Returns `true` if `!isDisabled && position === "on"`.
     *
     * The switch `position` can always be changed to `"on"` or `"off"`, but…
     *
     * …when `isDisabled`, `isOn` will always be `false`
     *
     * …when `!isDisabled`, `isOn` will only be `true` if the switch position is `"on"`
     */
    readonly isOn: boolean,

    /**
     * Determines the switch position.
     *
     * Use `turnOn()` and `turnOff()` to control this property.
     */
    readonly position: SwitchPosition;

    /**
     * Enables the switch.
     *
     * Calling this will turn on the switch if it is also in the `"on"` position.
     */
    enable(): void,

    /**
     * Disables the switch.
     *
     * Calling this will turn off the switch regardless of its "on" or "off" position.
     */
    disable(): void,

    /**
     * Turns the switch to the on position.
     *
     * Calling this will turn on the switch if it is enabled.
     */
    on(): void,

    /**
     * Turns the switch to the off position.
     *
     * Calling this will turn off the switch regardless of enabled/disabled state.
     */
    off(): void,

    /**
     * Returns a wrapped version of the function `f` that, when invoked, will only invoke the original `f` if this switch is on.
     */
    connectToFunc<Request>(f: (r: Request) => Promise<void>): typeof f
};

export type ModalOptionsBag = {
    get isOpen(): boolean;
    set isOpen(value: boolean);
    onSave(): void | PromiseLike<void>;
    text?: string;
    saveText?: string;
    cancelText?: string;
};