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

        await wrapper.findAll("button[aria-label='Dismiss']")[1].trigger("click");

        expect(wrapper.emitted("dismiss")).toEqual([[errors[1]]]);
    });
});

describe("the toast's pill", () => {
    afterEach(() => {
        jest.useRealTimers();
    });

    test("speaks through one status region, which is there before anything is said in it", async () => {
        const wrapper = mount(ErrorToast, { props: { errors: [] } });
        const region = wrapper.get("[data-testid='error-toast']");
        expect(region.attributes("role")).toBe("status");

        await wrapper.setProps({ errors: [{ code: "rpc.rate_limited", severity: "quota", text: "Slow down a little." }] });

        expect(region.get("[data-code='rpc.rate_limited']").text()).toBe("Slow down a little.");
    });

    test("stays past 2.6 seconds when it is a failure, since the person may need to act on it", () => {
        jest.useFakeTimers();
        const error: ChatError = { code: "rpc.rate_limited", severity: "quota", text: "Slow down a little." };
        const wrapper = mount(ErrorToast, { props: { errors: [error] } });

        jest.advanceTimersByTime(60_000);

        expect(wrapper.emitted("dismiss")).toBeUndefined();
        expect(wrapper.find("[data-code='rpc.rate_limited']").exists()).toBe(true);
    });

    test("lets a failure go from its own dismiss button, which a keyboard can reach", async () => {
        const error: ChatError = { code: "rpc.rate_limited", severity: "quota", text: "Slow down a little." };
        const wrapper = mount(ErrorToast, { props: { errors: [error] } });
        const button = wrapper.get("[data-code='rpc.rate_limited'] button[aria-label='Dismiss']");
        expect(button.attributes("type")).toBe("button");

        await button.trigger("click");

        expect(wrapper.emitted("dismiss")).toEqual([[error]]);
    });

    test("draws a notice such as being back online, and lets it go after the same 2.6 seconds", async () => {
        jest.useFakeTimers();
        const notice = { code: "back-online", text: "You're back online", icon: "check" };
        const wrapper = mount(ErrorToast, { props: { errors: [], notices: [] } });

        await wrapper.setProps({ notices: [notice] });
        expect(wrapper.get("[data-code='back-online']").text()).toBe("You're back online");

        expect(wrapper.find("[data-code='back-online'] button").exists()).toBe(false);

        jest.advanceTimersByTime(2599);
        expect(wrapper.emitted("dismissNotice")).toBeUndefined();

        jest.advanceTimersByTime(1);
        expect(wrapper.emitted("dismissNotice")).toEqual([[notice]]);
    });
});
