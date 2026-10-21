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
// Removing someone from a room, asked over the frame from a member row's menu. The plain confirm
// names the person and what leaving means by the room's kind, with their face, name and campus.
// The room's last leader is never removed without someone to lead next: a wide dialog searches the
// room's members and offers them as one radiogroup, "New leader", and Remove waits for a pick,
// then names who will lead. The door's refusal shows in that dialog in its own words.
//
// The component these tests are written to, removeMember.partial.obs:
//
//   props: member: RosterMember | null (who is being removed; null draws nothing), isChannel,
//          channelName, isLastLeader: boolean, choices: RosterMember[] (who may lead next),
//          query: string, refusal: string | null, isRemoving: boolean, personAliasGuid: string,
//          campusNames: Record<string, string>
//   emits: close, confirm(replacementPersonAliasGuid: string | null), query(text)
//   marks: the dialog's own (dialog, dialog-cancel, dialog-confirm, dialog-preview), remove-preview,
//          leader-search, leader-choices, leader-choice, leader-radio, leader-none,
//          leader-refusal, leader-confirm
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import RemoveMember from "../../../../../src/Communication/Chat/ChatShell/components/removeMember.partial.obs";
import { RosterMember } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const downtown = "d0000001-0000-4000-8000-000000000000";

/** One row of chat_list_members, exactly its keys. */
function member(n: number, nickName: string, lastName: string, extra: Partial<RosterMember> = {}): RosterMember {
    return {
        person_alias_guid: `a000000${n}-0000-4000-8000-000000000000`,
        nick_name: nickName,
        last_name: lastName,
        avatar_url: null,
        is_leader: false,
        details: null,
        can_message: true,
        ...extra
    };
}

const tom = member(2, "Tom", "Hale", { is_leader: true });
const grace = member(3, "Grace", "Kim", { details: { campus_id: downtown, badge_keys: [] } });
const anna = member(4, "Anna", "Reyes", { details: { campus_id: downtown, badge_keys: [] } });
const self = member(1, "Jordan", "Lee");

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(RemoveMember, {
        props: {
            member: grace,
            isChannel: true,
            channelName: "young-adults",
            isLastLeader: false,
            choices: [],
            query: "",
            refusal: null,
            isRemoving: false,
            personAliasGuid: me,
            campusNames: { [downtown]: "Downtown" },
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

/** What a person reads of an element, each element's words apart; a face's initials and icons are hidden from them. */
function spaced(node: Node): string {
    if (node instanceof Element && node.getAttribute("aria-hidden") === "true") {
        return "";
    }
    return node.nodeType === Node.TEXT_NODE ? node.textContent ?? "" : Array.from(node.childNodes).map(child => ` ${spaced(child)} `).join("");
}

const read = (element: Element): string => spaced(element).replace(/\s+/g, " ").replace(/ ([.,;?])/g, "$1").trim();
const text = (w: VueWrapper, mark: string): string => read(w.get(`[data-testid='${mark}']`).element);

describe("the plain confirm", () => {
    test("names the person, what leaving a channel means, and their face, name and campus", () => {
        const w = draw();

        expect(w.get(".chat-dialog-title").text()).toBe("Remove Grace Kim?");
        expect(w.get(".chat-dialog-body").text()).toBe("Grace will leave young-adults and its group in Rock, and won't see new messages here unless someone adds them back.");
        expect(text(w, "remove-preview")).toBe("Grace Kim Downtown");
        expect(text(w, "dialog-confirm")).toBe("Remove");
    });

    test("a group says this conversation", () => {
        const w = draw({ isChannel: false, channelName: "Hike planning", member: tom });

        expect(w.get(".chat-dialog-body").text()).toBe("Tom will leave this conversation and won't see new messages unless someone adds them back.");
    });

    test("Remove asks with no one to lead next, and Cancel closes", async () => {
        const w = draw();

        await w.get("[data-testid='dialog-confirm']").trigger("click");
        expect(w.emitted("confirm")).toEqual([[null]]);

        await w.get("[data-testid='dialog-cancel']").trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("with no one to remove, nothing is drawn", () => {
        const w = draw({ member: null });

        expect(w.find("[data-testid='dialog']").exists()).toBe(false);
    });
});

describe("the last leader", () => {
    function drawLeader(props: Record<string, unknown> = {}): VueWrapper {
        return draw({ member: tom, isLastLeader: true, choices: [self, grace, anna], ...props });
    }

    test("asks who leads next, from a search of the room's members", () => {
        const w = drawLeader();

        expect(w.get(".chat-dialog").classes()).toContain("is-wide");
        expect(w.get(".chat-dialog-title").text()).toBe("Remove Tom Hale?");
        expect(w.get(".chat-dialog-body").text()).toBe("Tom is the only leader of young-adults. Choose who leads it next; they'll get the leader role in Rock.");
        const search = w.get("[data-testid='leader-search']");
        expect(search.attributes("placeholder")).toBe("Find a member");
        expect(search.attributes("aria-label")).toBe("Find a member to lead");
        expect(w.get("[data-testid='leader-choices']").attributes("role")).toBe("radiogroup");
        expect(w.get("[data-testid='leader-choices']").attributes("aria-label")).toBe("New leader");
        expect(w.findAll("[data-testid='leader-choice']").map(c => read(c.element))).toEqual(["Jordan Lee (you)", "Grace Kim Downtown", "Anna Reyes Downtown"]);
    });

    test("typing asks for that name", async () => {
        const w = drawLeader();

        await w.get("[data-testid='leader-search']").setValue("an");
        expect(w.emitted("query")).toEqual([["an"]]);
    });

    test("no match says so", () => {
        const w = drawLeader({ query: "zz", choices: [] });

        expect(text(w, "leader-none")).toBe("No one in young-adults matches “zz”.");
    });

    test("Remove waits for a pick, then names who will lead and asks with them", async () => {
        const w = drawLeader();
        const confirm = w.get("[data-testid='leader-confirm']");
        expect(confirm.attributes("disabled")).toBeDefined();
        expect(confirm.text()).toBe("Remove");

        await w.findAll("[data-testid='leader-radio']")[2].setValue(true);
        expect(w.findAll("[data-testid='leader-choice']")[2].classes()).toContain("is-on");
        expect(confirm.attributes("disabled")).toBeUndefined();
        expect(confirm.text()).toBe("Remove and make Anna leader");

        await confirm.trigger("click");
        expect(w.emitted("confirm")).toEqual([[anna.person_alias_guid]]);
    });

    test("while the door works the button is busy, and a refusal shows in the dialog in its words", async () => {
        const w = drawLeader({ isRemoving: true });
        await w.findAll("[data-testid='leader-radio']")[1].setValue(true);
        expect(w.get("[data-testid='leader-confirm']").attributes("aria-busy")).toBe("true");
        expect(w.get("[data-testid='leader-confirm']").text()).toBe("Removing");

        await w.setProps({ isRemoving: false, refusal: "Choose who leads this channel next before removing its last leader." });
        await nextTick();
        const refusal = w.get("[data-testid='leader-refusal']");
        expect(read(refusal.element)).toBe("Choose who leads this channel next before removing its last leader.");
        expect(refusal.attributes("role")).toBe("alert");
    });
});
