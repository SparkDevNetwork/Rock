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

import { readFileSync } from "fs";
import { resolve } from "path";
import { defaultLimits } from "../../../src/Communication/Chat/ChatShell/composables/useSession.partial";

/*
    9/16/26 - CLAUDE

    The chat platform publishes one generated artifact describing the wire between it and Rock:
    the ordered column list per synced table, and the stable error codes both sides branch on. The
    server half reads it as an embedded resource; this half is where the client will read the same
    file rather than keeping a second copy of the codes that drifts. This spec is what stops the
    file being moved or reshaped without the client noticing.

    Reason: The client and the server must read one contract file, not two copies of it.
*/

type WireContract = {
    tables: { schema: string; name: string; columns: string[] }[];
    error_codes: { families: string[]; pattern: string; codes: string[] };
    limits: Record<string, unknown>;
    wire_hash: string;
};

/** The four synced tables, in the order the platform applies them. */
const expectedTables = ["chat_aliases", "chat_channels", "chat_channel_members", "chat_badges"];

/** The vendored artifact, which lives with the server code that embeds it. */
const contractPath = resolve(__dirname, "../../../../Rock/Communication/Chat/Platform/Contract/chat-wire-contract.json");

function readContract(): WireContract {
    return JSON.parse(readFileSync(contractPath, "utf8")) as WireContract;
}

describe("chat wire contract", () => {
    it("names the four wire tables in order, each with an ordered column list", () => {
        const contract = readContract();

        expect(contract.tables.map(t => t.name)).toEqual(expectedTables);

        for (const table of contract.tables) {
            expect(table.columns.length).toBeGreaterThan(0);
        }
    });

    it("publishes error codes that match the pattern published beside them", () => {
        const contract = readContract();
        const pattern = new RegExp(contract.error_codes.pattern);

        expect(contract.error_codes.codes.length).toBeGreaterThan(0);

        for (const code of contract.error_codes.codes) {
            expect(code).toMatch(pattern);
        }

        // A pattern that accepts anything would pass the loop above without constraining a thing.
        expect("Rpc.UnknownErrorCode").not.toMatch(pattern);
        expect("nope.unknown_error_code").not.toMatch(pattern);
    });

    it("carries the refusals a GIF search or send can answer, which the client branches on", () => {
        const contract = readContract();

        for (const code of ["rpc.gifs_off", "rpc.bad_gif", "rpc.gif_search_busy"]) {
            expect(contract.error_codes.codes).toContain(code);
        }
    });

    it("carries the typing refusals and the typing pace, which the client branches on and defaults to", () => {
        const contract = readContract();

        for (const code of ["rpc.typing_off", "rpc.typing_not_here", "rpc.bad_parent", "authz.not_member"]) {
            expect(contract.error_codes.codes).toContain(code);
        }
        expect(contract.limits).toMatchObject({ typing_interval_ms: 5000, typing_expiry_ms: 7500 });
    });

    // The client's fallback for a limit a platform leaves out is the contract's own value, read
    // from the contract, so the two cannot be moved apart with every pin still green.
    it("holds the client's fallback for every limit the client defaults and the contract names at the contract's value", () => {
        const contract = readContract();
        const shared = Object.keys(defaultLimits).filter(key => key in contract.limits);

        expect(shared).toEqual(expect.arrayContaining(["typing_interval_ms", "typing_expiry_ms"]));
        expect(Object.fromEntries(shared.map(key => [key, defaultLimits[key as keyof typeof defaultLimits]])))
            .toEqual(Object.fromEntries(shared.map(key => [key, contract.limits[key]])));
    });
});
