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
// What a pending or failed message draws before the platform has built its pieces. Who may be
// mentioned is the platform's call, made when the message is stored, so until then a token is
// drawn as plain text: the name the client already holds for that person, else "someone". Never a
// mention, which would claim an admission the platform has not made, and never the raw token.
import { pendingPieces } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";

const lee = "a0000007-0000-4000-8000-000000000000";
const stranger = "a0000009-0000-4000-8000-000000000000";
const names: ReadonlyMap<string, string> = new Map([[lee, "Lee Leader"]]);

/** The text a list of pieces draws, read in order. */
function drawn(pieces: Array<{ text: string }>): string {
    return pieces.map(piece => piece.text).join("");
}

describe("pendingPieces", () => {
    test("a token for a person the client holds a name for draws as that name", () => {
        expect(drawn(pendingPieces(`Hi <@${lee}> there`, names))).toBe("Hi @Lee Leader there");
    });

    test("a token for a person the client holds no name for draws as someone", () => {
        expect(drawn(pendingPieces(`ask <@${stranger}>`, names))).toBe("ask @someone");
    });

    test("a token in capitals is the same person, as the platform reads it", () => {
        expect(drawn(pendingPieces(`Hi <@${lee.toUpperCase()}>`, names))).toBe("Hi @Lee Leader");
    });

    test("the whole-room token draws as @all, in any letter case", () => {
        expect(drawn(pendingPieces("<@all> and <@All>", names))).toBe("@all and @all");
    });

    test("every piece is text, so nothing is drawn as a mention and no raw token is left", () => {
        const pieces = pendingPieces(`<@${lee}><@${stranger}> <@all> <b>x</b>`, names);

        expect(pieces.every(piece => piece.type === "text")).toBe(true);
        expect(drawn(pieces)).toBe("@Lee Leader@someone @all <b>x</b>");
        expect(drawn(pieces)).not.toContain("<@");
    });

    test("a body with no token draws as typed", () => {
        expect(drawn(pendingPieces("just words", names))).toBe("just words");
    });
});
