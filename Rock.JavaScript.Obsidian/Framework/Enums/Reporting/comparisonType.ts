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

/**
 * The type of comparison operations that can be used when building custom
 * filtering logic.
 */
export const ComparisonType = {
    /* No comparison performed. */
    None: 0x0,

    /** The two values must be equal to each other. */
    EqualTo: 0x1,

    /** The two values must not be equal to each other. */
    NotEqualTo: 0x2,

    /** The left value must start with the right value. */
    StartsWith: 0x4,

    /** The left value must contain the right value. */
    Contains: 0x8,

    /** The left value must not contain the right value. */
    DoesNotContain: 0x10,

    /** The left value must be a blank-like value. */
    IsBlank: 0x20,

    /** The left value must not be a blank-like value. */
    IsNotBlank: 0x40,

    /** The left value must be greater than the right value. */
    GreaterThan: 0x80,

    /** The left value must be greater than or equal to the right value. */
    GreaterThanOrEqualTo: 0x100,

    /** The left value must be less than the right value. */
    LessThan: 0x200,

    /** The left value must be less than or equal to the right value. */
    LessThanOrEqualTo: 0x400,

    /** The left value must end with the right value. */
    EndsWith: 0x800,

    /** The left value must fall between a set of values specified by the right value. */
    Between: 0x1000,

    /** The left value must match the regular expression defined in the right value. */
    RegularExpression: 0x2000
};

/**
 * The type of comparison operations that can be used when building custom
 * filtering logic.
 */
export type ComparisonType = number;
