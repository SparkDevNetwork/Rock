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

import { ModuleType } from "./moduleTypes.partial";

/** Tells the page frame that a module type has started dragging from the sidebar. */
export type ModuleTypeDragStartRequest = {
    moduleType: ModuleType;
};

/** Tells the page frame where the pointer is, in coordinates relative to the frame. */
export type ModuleTypeDragOverRequest = {
    clientX: number;
    clientY: number;
};

/** Tells the page frame that the dragged module type was dropped at a point, in coordinates relative to the frame. */
export type ModuleTypeDropRequest = {
    clientX: number;
    clientY: number;
};

/** Tells the page frame that the pointer left it while dragging. */
export type ModuleTypeDragLeaveRequest = {
    type: "MODULE_TYPE_DRAG_LEAVE_REQUEST";
};

/** Tells the page frame that the drag has finished, whether or not it was dropped. */
export type ModuleTypeDragEndRequest = {
    type: "MODULE_TYPE_DRAG_END_REQUEST";
};
