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
// The card's own markup is not mounted here. Mounting any component in this project
// pulls the framework's field type registry, whose packages belong to a sibling
// project that no build or pipeline step installs, so every mounting spec in this
// repository fails to run today. What the card shows in each state is this module,
// and that is what is covered.
import { credentialUnreadableSentence, toChatCardState } from "../../src/Administration/SparkConnectedServices/chatViewModel.partial";

function enabledBag(): Record<string, unknown> {
    return {
        isEnabled: true,
        tenantId: "11111111-1111-4111-8111-111111111111",
        projectUrl: "https://example.supabase.co"
    };
}

describe("spark connected services chat card", () => {
    it("offers to enable a church that has not enabled chat", () => {
        const state = toChatCardState(null);

        expect(state.isEnabled).toBe(false);
        expect(state.isEnableActionShown).toBe(true);
        expect(state.tenantId).toBeNull();
        expect(state.projectUrl).toBeNull();
    });

    it("shows what a church that has enabled chat got, and offers no action", () => {
        const state = toChatCardState(enabledBag());

        expect(state.isEnabled).toBe(true);
        // Enabling twice would mint a second church key and orphan the first, and
        // nothing here can rotate one, so there is no second press to make.
        expect(state.isEnableActionShown).toBe(false);
        expect(state.tenantId).toBe("11111111-1111-4111-8111-111111111111");
        expect(state.projectUrl).toBe("https://example.supabase.co");
    });

    it("says the credentials cannot be read, and still offers no action, when the block says so", () => {
        const state = toChatCardState({ ...enabledBag(), isCredentialUnreadable: true });

        expect(state.isEnabled).toBe(true);
        expect(state.isEnableActionShown).toBe(false);
        expect(state.credentialUnreadableMessage).toBe(credentialUnreadableSentence);
        expect(state.credentialUnreadableMessage).toContain("Spark");
    });

    it("says nothing about credentials in every other state", () => {
        expect(toChatCardState(null).credentialUnreadableMessage).toBeNull();
        expect(toChatCardState(enabledBag()).credentialUnreadableMessage).toBeNull();
        expect(toChatCardState({ ...enabledBag(), isCredentialUnreadable: false }).credentialUnreadableMessage).toBeNull();
    });

    it("carries nothing the card was not meant to render", () => {
        const hostile = { ...enabledBag(), privateKey: "secret-part" };

        const state = toChatCardState(hostile);

        expect(JSON.stringify(state)).not.toContain("secret-part");
    });
});
