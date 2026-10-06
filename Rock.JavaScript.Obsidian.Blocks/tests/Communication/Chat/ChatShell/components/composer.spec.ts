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
// The composer itself, mounted: what is left in its box after a send is what the person sees,
// so it is read from the box rather than from the rule the box is meant to follow.
import { mount } from "@vue/test-utils";
import Composer from "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";

async function sendFrom(isTextKeptOnSend: boolean): Promise<{ box: string, sent: unknown[] }> {
    const wrapper = mount(Composer, { props: { isTextKeptOnSend } });
    await wrapper.get("[data-testid='composer-input']").setValue("hello");
    await wrapper.get("form").trigger("submit");

    return {
        box: (wrapper.get("[data-testid='composer-input']").element as HTMLTextAreaElement).value,
        sent: wrapper.emitted("send") ?? []
    };
}

describe("the composer", () => {
    test("keeps the text in its box when the shell keeps it, as a draft's first message does", async () => {
        const { box, sent } = await sendFrom(true);

        expect(sent).toEqual([["hello"]]);
        expect(box).toBe("hello");
    });

    test("empties its box after an ordinary send", async () => {
        const { box, sent } = await sendFrom(false);

        expect(sent).toEqual([["hello"]]);
        expect(box).toBe("");
    });
});
