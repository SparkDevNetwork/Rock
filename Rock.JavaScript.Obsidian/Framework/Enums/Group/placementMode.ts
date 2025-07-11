//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

/** Represents the different Group Placement modes. */
export const PlacementMode = {
    /** The Group Placement Block is in Template Mode. */
    TemplateMode: 0,

    /** The Group Placement Block is in Instance Mode. */
    InstanceMode: 1,

    /** The Group Placement Block is in Group Mode. */
    GroupMode: 2,

    /** The Group Placement Block is in Entity Set Mode. */
    EntitySetMode: 3
} as const;

/** Represents the different Group Placement modes. */
export const PlacementModeDescription: Record<number, string> = {
    0: "Template Mode",

    1: "Instance Mode",

    2: "Group Mode",

    3: "Entity Set Mode"
};

/** Represents the different Group Placement modes. */
export type PlacementMode = typeof PlacementMode[keyof typeof PlacementMode];
