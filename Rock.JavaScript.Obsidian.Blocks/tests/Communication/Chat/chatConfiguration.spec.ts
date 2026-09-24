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
// The form's own markup is not mounted here. Mounting any component in this project
// pulls the framework's field type registry, whose packages belong to a sibling
// project that no build or pipeline step installs, so every mounting spec in this
// repository fails to run today. What the form shows and what it sends back is this
// module, and that is what is covered.
import { credentialUnreadableSentence as cardSentence } from "../../../src/Administration/SparkConnectedServices/chatViewModel.partial";
import { credentialUnreadableSentence, hasUnsavedChanges, toBag, toEmptyState, toFormModel } from "../../../src/Communication/Chat/ChatConfiguration/viewModel.partial";

function bag(): Record<string, unknown> {
    return {
        areChatProfilesVisible: true,
        isOpenDirectMessagingAllowed: false,
        minimumAge: 13,
        directMessageAccessDataView: { value: "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb", text: "Members" },
        chatBadgeDataViews: [{ value: "cccccccc-cccc-4ccc-8ccc-cccccccccccc", text: "Staff" }],
        projectUrl: "https://example.supabase.co",
        publishableKey: "sb_publishable_test",
        tenantId: "11111111-1111-4111-8111-111111111111",
        kid: "platform-kid-1",
        isChurchKeyPresent: true
    };
}

describe("chatConfiguration view model", () => {
    it("shows every church owned setting the box carried", () => {
        const form = toFormModel(bag());

        expect(form.areChatProfilesVisible).toBe(true);
        expect(form.isOpenDirectMessagingAllowed).toBe(false);
        expect(form.minimumAge).toBe(13);
        expect(form.directMessageAccessDataView?.value).toBe("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");
        expect(form.chatBadgeDataViews).toHaveLength(1);
    });

    it("sends every church owned setting back, edits included", () => {
        const form = toFormModel(bag());
        form.areChatProfilesVisible = false;
        form.minimumAge = 16;
        form.chatBadgeDataViews = [];

        const sent = toBag(form);

        expect(sent.areChatProfilesVisible).toBe(false);
        expect(sent.minimumAge).toBe(16);
        expect(sent.chatBadgeDataViews).toHaveLength(0);
        expect(sent.isOpenDirectMessagingAllowed).toBe(false);
        expect(sent.directMessageAccessDataView?.value).toBe("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");
    });

    it("keeps the platform issued half out of the form altogether", () => {
        const form = toFormModel(bag()) as Record<string, unknown>;

        for (const name of ["projectUrl", "publishableKey", "tenantId", "kid"]) {
            expect(form[name]).toBeUndefined();
        }
    });

    it("sends back nothing the server did not ask it to own", () => {
        const sent = toBag(toFormModel(bag())) as Record<string, unknown>;

        expect(Object.keys(sent).sort()).toEqual([
            "areChatProfilesVisible",
            "chatBadgeDataViews",
            "directMessageAccessDataView",
            "isOpenDirectMessagingAllowed",
            "minimumAge"
        ]);
    });

    it("never carries a signing key in either direction", () => {
        const form = JSON.stringify(toFormModel(bag()));
        const sent = JSON.stringify(toBag(toFormModel(bag())));

        for (const shape of [form, sent]) {
            expect(shape).not.toContain("privateKey");
            expect(shape).not.toContain("\"d\"");
        }
    });
});

describe("chatConfiguration unsaved edits", () => {
    it("reads as saved when the form matches what was loaded", () => {
        expect(hasUnsavedChanges(toFormModel(bag()), toFormModel(bag()))).toBe(false);
    });

    it("reads as unsaved after an edit to any setting the form owns", () => {
        const saved = toFormModel(bag());
        const edits: Array<(form: ReturnType<typeof toFormModel>) => void> = [
            form => { form.areChatProfilesVisible = false; },
            form => { form.isOpenDirectMessagingAllowed = true; },
            form => { form.minimumAge = 16; },
            form => { form.minimumAge = null; },
            form => { form.directMessageAccessDataView = null; },
            form => { form.directMessageAccessDataView = { value: "dddddddd-dddd-4ddd-8ddd-dddddddddddd", text: "Leaders" }; },
            form => { form.chatBadgeDataViews = []; },
            form => { form.chatBadgeDataViews = [...form.chatBadgeDataViews, { value: "eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee", text: "Elders" }]; }
        ];

        for (const edit of edits) {
            const form = toFormModel(bag());
            edit(form);

            expect(hasUnsavedChanges(form, saved)).toBe(true);
        }
    });

    it("reads as saved again once the edited form is what was saved", () => {
        const form = toFormModel(bag());
        form.minimumAge = 16;
        form.chatBadgeDataViews = [];

        const saved = toFormModel(toBag(form));

        expect(hasUnsavedChanges(form, saved)).toBe(false);
    });

    it("does not count a picker's label as an edit, since only the identifier is stored", () => {
        const form = toFormModel(bag());
        form.directMessageAccessDataView = { value: "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb", text: "" };

        expect(hasUnsavedChanges(form, toFormModel(bag()))).toBe(false);
    });
});

describe("chat configuration when chat cannot run here", () => {
    it("points at Connected Services when chat was never enabled", () => {
        const state = toEmptyState({ isCredentialUnreadable: false });

        expect(state.isConnectedServicesLinkShown).toBe(true);
        expect(state.message).not.toBe(credentialUnreadableSentence);
        expect(toEmptyState(null).isConnectedServicesLinkShown).toBe(true);
    });

    it("says the credentials cannot be read, and does not point back at the card, when chat was enabled", () => {
        // The card has nothing to offer this organization, so pointing at it is a loop.
        const state = toEmptyState({ isCredentialUnreadable: true });

        expect(state.message).toBe(credentialUnreadableSentence);
        expect(state.isConnectedServicesLinkShown).toBe(false);
    });

    it("says the same one sentence as the Connected Services card, naming Spark as the contact", () => {
        expect(credentialUnreadableSentence).toBe(cardSentence);
        expect(credentialUnreadableSentence).toContain("Spark");
        // A semicolon joins two clauses of one sentence; only a full stop, question or exclamation ends one.
        expect(credentialUnreadableSentence.split(/[.!?]\s/).length).toBe(1);
    });
});
