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
// Edit in place: the message's text in the shared chip box, with Cancel and Save under it. Where
// there is a keyboard, a line beside them names its keys; a phone has neither Escape nor a
// separate Enter, so there the line is not drawn.
import { mount, VueWrapper } from "@vue/test-utils";
import MessageEditor from "../../../../../src/Communication/Chat/ChatShell/components/messageEditor.partial.obs";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { providedSession } from "../sessionShapes";

const message = { body: "See you Sunday", pieces: null } as unknown as TimelineMessage;

let wrapper: VueWrapper | null = null;

function draw(isPhone: boolean): VueWrapper {
    wrapper = mount(MessageEditor, {
        props: { message },
        global: { provide: { ...providedSession({ isPhone }) } },
        attachTo: document.body
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the editor's foot", () => {
    test("names Escape and Enter beside Cancel and Save where there is a keyboard", () => {
        const foot = draw(false).get("[data-testid='message-editor-foot']");

        expect(foot.text()).toContain("Esc to cancel, Enter to save");
        expect(foot.find("[data-testid='edit-cancel']").exists()).toBe(true);
        expect(foot.find("[data-testid='edit-save']").exists()).toBe(true);
    });

    test("draws no key names on a phone, only Cancel and Save", () => {
        const foot = draw(true).get("[data-testid='message-editor-foot']");

        expect(foot.find("kbd").exists()).toBe(false);
        expect(foot.text()).not.toContain("to save");
        expect(foot.find("[data-testid='edit-cancel']").exists()).toBe(true);
        expect(foot.find("[data-testid='edit-save']").exists()).toBe(true);
    });
});
