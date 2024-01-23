import { mount } from "@vue/test-utils";
import Toggle from "../../Framework/Controls/toggle.obs";
import JavaScriptAnchor from "../../Framework/Controls/javaScriptAnchor.obs";
import assert = require("assert");

describe("Toggle", () => {
    it("should show default css on ON BUTTON active if no onButtonActiveCssClass prop provided", () => {
        const wrapper = mount(Toggle, {
            props: {
                modelValue: true,
                label: "",
                trueText: "TrueText",
                falseText: "FalseText"
            }
        });
        const onButton = wrapper.findAllComponents(JavaScriptAnchor)[0];
        assert.deepStrictEqual(onButton.classes(), ["active", "btn", "btn-primary"]);
    });

    it("should show default css on OFF BUTTON active if no offButtonActiveCssClass prop provided", () => {
        const wrapper = mount(Toggle, {
            props: {
                modelValue: false,
                label: "",
                trueText: "TrueText",
                falseText: "FalseText"
            }
        });
        const offButton = wrapper.findAllComponents(JavaScriptAnchor)[1];
        assert.deepStrictEqual(offButton.classes(), ["active", "btn", "btn-primary"]);
    });

    it("should show css on ON BUTTON active if onButtonActiveCssClass prop provided", () => {
        const wrapper = mount(Toggle, {
            props: {
                modelValue: true,
                label: "",
                trueText: "TrueText",
                falseText: "FalseText",
                onButtonActiveCssClass: "btn-success"
            }
        });
        const onButton = wrapper.findAllComponents(JavaScriptAnchor)[0];
        assert.deepStrictEqual(onButton.classes(), ["active", "btn", "btn-primary", "btn-success"]);
    });

    it("should show css on OFF BUTTON active if offButtonActiveCssClass prop provided", () => {
        const wrapper = mount(Toggle, {
            props: {
                modelValue: false,
                label: "",
                trueText: "TrueText",
                falseText: "FalseText",
                offButtonActiveCssClass: "btn-warning"
            }
        });
        const offButton = wrapper.findAllComponents(JavaScriptAnchor)[1];
        assert.deepStrictEqual(offButton.classes(), ["active", "btn", "btn-primary", "btn-warning"]);
    });
});