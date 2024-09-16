import RegistrationEntry from "../../src/Event/registrationEntry.obs";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { mountBlock } from "../blocks";

describe("RegistrationEntry", () => {
    it("Displays default not found message when no configuration values sent", () => {
        const instance = mountBlock(RegistrationEntry, null);

        expect(instance.find(".alert-warning > strong").text()).toBe("Sorry");
        expect(instance.find(".alert-warning > p").text()).toBe("The selected registration could not be found or is no longer active.");
    });

    it("Displays custom not found message", () => {
        const configuration: Partial<RegistrationEntryInitializationBox> = {
            registrationInstanceNotFoundMessage: "Custom Not Found"
        };

        const instance = mountBlock(RegistrationEntry, configuration);

        expect(instance.find(".alert-warning > strong").text()).toBe("Sorry");
        expect(instance.find(".alert-warning > p").text()).toBe("Custom Not Found");
    });
});
