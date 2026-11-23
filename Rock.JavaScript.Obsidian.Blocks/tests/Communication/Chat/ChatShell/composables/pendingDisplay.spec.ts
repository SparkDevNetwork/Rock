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
import { pendingPieces } from "../../../../../src/Communication/Chat/ChatShell/heldMessage.partial";
import { DisplayPiece, EmojiGlyph } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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

    test("no piece is a mention piece and no raw token is left", () => {
        const pieces = pendingPieces(`<@${lee}><@${stranger}> <@all> <b>x</b>`, names);

        expect(pieces.some(piece => piece.type === "mention")).toBe(false);
        expect(drawn(pieces)).toBe("@Lee Leader@someone @all <b>x</b>");
        expect(drawn(pieces)).not.toContain("<@");
    });

    test("a body with no token draws as typed", () => {
        expect(drawn(pendingPieces("just words", names))).toBe("just words");
    });

    // The pieces below are the ones the platform stores for the same body (chat_display_pieces),
    // written out by hand, so the pending row and the stored row draw the same thing.
    const catalog: EmojiGlyph[] = [
        { code: "tada", emoji: "🎉", image_url: null, label: "party popper" },
        { code: "cedar", emoji: null, image_url: "https://cdn.example/cedar.png", label: "cedar" }
    ];

    test("a link is a link piece, as the platform cuts it, with trailing punctuation left as text", () => {
        expect(pendingPieces("see https://cedarhill.church/hike.", names, { emoji: catalog })).toEqual([
            { type: "text", text: "see " },
            { type: "link", text: "https://cedarhill.church/hike", url: "https://cedarhill.church/hike" },
            { type: "text", text: "." }
        ]);
    });

    test("a known shortcode is the catalog's glyph; an unknown one or one in capitals stays text", () => {
        expect(pendingPieces(":tada: yay :nope: :TADA: :cedar:", names, { emoji: catalog })).toEqual([
            { type: "emoji", text: ":tada:", code: "tada", emoji: "🎉", label: "party popper" },
            { type: "text", text: " yay :nope: :TADA: " },
            { type: "emoji", text: ":cedar:", code: "cedar", image_url: "https://cdn.example/cedar.png", label: "cedar" }
        ]);
    });

    test("with no catalog held a shortcode waits for the platform as text", () => {
        expect(pendingPieces("ok :tada:", names)).toEqual([{ type: "text", text: "ok :tada:" }]);
    });

    test("a person picked in this send draws as a pending chip, never a mention piece", () => {
        const picked: DisplayPiece[] = [{ type: "mention", text: "@Lee Leader", person_alias_guid: lee, avatar_url: "https://cdn.example/lee.jpg" }];

        expect(pendingPieces(`hi <@${lee}>!`, new Map(), { mentions: picked })).toEqual([
            { type: "text", text: "hi " },
            { type: "pending-mention", text: "@Lee Leader", person_alias_guid: lee, avatar_url: "https://cdn.example/lee.jpg" },
            { type: "text", text: "!" }
        ]);
    });

    test("@all picked in this send draws as the pending room chip", () => {
        const picked: DisplayPiece[] = [{ type: "mention", text: "@all", all: true }];

        expect(pendingPieces("<@all> meet", names, { mentions: picked })).toEqual([
            { type: "pending-mention", text: "@all", all: true },
            { type: "text", text: " meet" }
        ]);
    });

    // A fixed list the platform's rule answers, kept in step with chat_display_pieces.
    test.each([
        ["https://a.org/x_(y)) end", "", "https://a.org/x_(y)", ") end"],
        ["(https://a.org/x)", "(", "https://a.org/x", ")"],
        ["https://a.org/q?x=1&y=2!?", "", "https://a.org/q?x=1&y=2", "!?"],
        ["http://a.org:8080/p, next", "", "http://a.org:8080/p", ", next"],
        ["HTTPS://A.ORG/Up;", "", "HTTPS://A.ORG/Up", ";"]
    ])("the link rule matches the platform's on %s", (body, before, url, rest) => {
        expect(pendingPieces(body, names)).toEqual([
            ...(before ? [{ type: "text", text: before }] : []),
            { type: "link", text: url, url },
            { type: "text", text: rest }
        ]);
    });

    test("an address with nothing after the scheme is text", () => {
        expect(pendingPieces("go https:// now", names)).toEqual([{ type: "text", text: "go https:// now" }]);
    });
});
