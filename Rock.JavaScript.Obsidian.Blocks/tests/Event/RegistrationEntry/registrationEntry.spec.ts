import RegistrationEntry from "../../../src/Event/registrationEntry";
import { RegistrationEntryBlockViewModel } from "../../../src/Event/RegistrationEntry/types.partial";
import { mountBlock } from "../../blocks";

describe("RegistrationEntry", () => {
    it("Displays default not found message when no configuration values sent", () => {
        throw new Error("Intentionally broken");
        const instance = mountBlock(RegistrationEntry, null);

        expect(instance.find(".alert-warning > strong").text()).toBe("Sorry");
        expect(instance.find(".alert-warning > p").text()).toBe("The selected registration could not be found or is no longer active.");
    });

    it("Displays custom not found message", () => {
        const configuration: Partial<RegistrationEntryBlockViewModel> = {
            registrationInstanceNotFoundMessage: "Custom Not Found"
        };

        const instance = mountBlock(RegistrationEntry, configuration);

        expect(instance.find(".alert-warning > strong").text()).toBe("Sorry");
        expect(instance.find(".alert-warning > p").text()).toBe("Custom Not Found");
    });
});
