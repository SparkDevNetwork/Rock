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

/**
 * This is a helper type that allows you to get intellisense on a value but
 * still allow any other value to be supplied.
 *
 * For example, doing LiteralUnion<"success", "fail"> would mean the editor
 * will suggest the value "success" and "fail", but you can still put in any
 * other string value you want.
 */
export type LiteralUnion<T extends U, U = string> = T | (U & Record<never, never>);