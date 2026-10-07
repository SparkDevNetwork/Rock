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
// The error toast, mounted: two failures can share a code with different sentences, so closing
// one has to say which one it was.
import { mount } from "@vue/test-utils";
import ErrorToast from "../../../../../src/Communication/Chat/ChatShell/components/errorToast.partial.obs";
import { ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

describe("the error toast", () => {
    test("dismissing the second of two errors under one code emits that error, its code and its text", async () => {
        const errors: ChatError[] = [
            { code: "door.first_message_failed", severity: "failed", text: "Your message to Person1 could not be sent." },
            { code: "door.first_message_failed", severity: "failed", text: "Your message to Person2 could not be sent." }
        ];
        const wrapper = mount(ErrorToast, { props: { errors } });

        await wrapper.findAll("button.close")[1].trigger("click");

        expect(wrapper.emitted("dismiss")).toEqual([[errors[1]]]);
    });
});
