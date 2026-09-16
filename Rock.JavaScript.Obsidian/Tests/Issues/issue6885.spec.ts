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
import TextColumn from "@Obsidian/Controls/Grid/Columns/textColumn.partial";
import { ColumnDefinition, ExportValueFunction, IGridState } from "@Obsidian/Types/Controls/grid";

describe("Issue 6885", () => {
    const exportValue = (TextColumn.props as unknown as { exportValue: { default: ExportValueFunction } }).exportValue.default;
    const column = { field: "id" } as ColumnDefinition;
    const grid = {} as IGridState;

    it("exports numeric text-column values instead of dropping them", () => {
        expect(exportValue({ id: 12345 }, column, grid)).toBe("12345");
    });

    it("exports boolean values as text", () => {
        expect(exportValue({ id: true }, column, grid)).toBe("true");
        expect(exportValue({ id: false }, column, grid)).toBe("false");
    });

    it("exports string values unchanged", () => {
        expect(exportValue({ id: "abc" }, column, grid)).toBe("abc");
    });

    it("leaves empty cells blank", () => {
        expect(exportValue({ id: null }, column, grid)).toBeUndefined();
        expect(exportValue({ id: undefined }, column, grid)).toBeUndefined();
    });

    it("leaves object values blank instead of exporting [object Object]", () => {
        expect(exportValue({ id: { nickName: "Ted" } }, column, grid)).toBeUndefined();
    });
});
