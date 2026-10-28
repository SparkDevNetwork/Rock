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
// The block is read as written rather than mounted: it pulls in most of the framework's
// controls, and what is pinned here is the help an administrator reads beside the chat setting.
import { readFileSync } from "fs";
import { resolve } from "path";

describe("Edit Person's chat profile visibility", () => {
    const source = readFileSync(resolve(__dirname, "../../../src/Crm/PersonDetail/editPerson.obs"), "utf8");

    test("its help says what others in chat see when it is on", () => {
        const control = source.slice(source.indexOf("v-model=\"chatProfileVisibility\""));
        const help = /help="([^"]*)"/.exec(control)?.[1];

        expect(help).toBe("Whether others in chat see this person's email, mobile phone, badges and profile card template.");
    });
});
