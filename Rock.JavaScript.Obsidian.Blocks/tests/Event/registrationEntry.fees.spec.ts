import { HttpResult } from "@Obsidian/Types/Utility/http";
import RegistrationEntry from "../../src/Event/registrationEntry.obs";
import { mockBlockActions, mountBlock } from "../blocks";
import { Guid } from "@Obsidian/Types";
import { getMatching, waitFor } from "../utils";
import { flushPromises } from "@vue/test-utils";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";

function getConfigurationValues(): RegistrationEntryInitializationBox {
    // This is weird, but we have to do this because the block actually
    // modifies the configuration values which is non-standard. And
    // also so we can modify the configuration per test.
    return JSON.parse(JSON.stringify(configurationValues));
}

function getDefaultAttributeFieldValues(): HttpResult<Record<Guid, unknown>> {
    return {
        isSuccess: true,
        isError: false,
        statusCode: 200,
        errorMessage: null,
        data: {
        }
    };
}

const blockActions = mockBlockActions({
    GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
});

describe("RegistrationEntry Fees", () => {
    beforeAll(() => {
        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();
    });

    describe("Checkbox Fee", () => {
        it("Auto-selects when required and available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": null,
                        "countRemaining": null
                    }
                ],
                "allowMultiple": false,
                "isRequired": true,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option and for it to be checked.
            const checkboxControl = await waitFor(() => {
                return getMatching(firstRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            expect(checkboxControl.find("input").element.checked).toBe(true);
        });

        it("Does not auto-select when not required", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": null,
                        "countRemaining": null
                    }
                ],
                "allowMultiple": false,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option and for it to be checked.
            const checkboxControl = await waitFor(() => {
                return getMatching(firstRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            expect(checkboxControl.find("input").element.checked).toBe(false);
        });

        it("Does not auto-select when no available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": false,
                "isRequired": true,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            // Make sure it is checked, as expected.
            expect(firstFee.find("input").element.checked).toBe(true);

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option.
            const secondFee = await waitFor(() => {
                return getMatching(secondRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            // Make sure it is not selected.
            expect(secondFee.find("input").element.checked).toBe(false);
        });

        it("Disables when no available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": false,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option and then check it.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await firstFee.find("input").setValue(true);

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option and for it to be checked.
            const checkboxControl = await waitFor(() => {
                return getMatching(secondRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            expect(checkboxControl.find("input").element.disabled).toBe(true);
        });

        it("Does not prevent moving forward when not checked but is required", async () => {
            // Note: The WebForms block did not enforce the required property on
            // checkbox fees. So the Obsidian version is doing the same. This may
            // be considered a bug and if so the unit test should be updated.
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 0,
                        "countRemaining": 0
                    }
                ],
                "allowMultiple": false,
                "isRequired": true,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Attempt to move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            expect(firstRegistrant.isVisible()).toBe(false);
        });
    });

    // #region NumberUpDown Fees

    describe("NumberUpDown Fee", () => {
        it("Auto-sets when required and available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": null,
                        "countRemaining": null
                    }
                ],
                "allowMultiple": true,
                "isRequired": true,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option and for it to be checked.
            const numberControl = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            expect(numberControl.find("span.numberincrement-value").text()).toBe("1");
        });

        it("Does not auto-set when not required", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": null,
                        "countRemaining": null
                    }
                ],
                "allowMultiple": true,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option and for it to be checked.
            const numberControl = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            expect(numberControl.find("span.numberincrement-value").text()).toBe("0");
        });

        it("Does not auto-set when no available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 0,
                        "countRemaining": 0
                    }
                ],
                "allowMultiple": true,
                "isRequired": true,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option and for it to be checked.
            const numberControl = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            expect(numberControl.find("span.numberincrement-value").text()).toBe("0");
        });

        it("Disables up button when no available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": true,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option and increment it.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down", node => node.text().startsWith("Test Fee"));
            });

            await firstFee.find<HTMLButtonElement>(".numberincrement-up").trigger("click");

            // Make sure the value incremented.
            expect(firstFee.find(".numberincrement-value").text() === "1");

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option.
            const secondFee = await waitFor(() => {
                return getMatching(secondRegistrant, "div.number-up-down", node => node.text().startsWith("Test Fee"));
            });

            expect(secondFee.find(".numberincrement-up").classes()).toContain("disabled");
        });

        it("Prevents moving forward when no value entered but is required", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 0,
                        "countRemaining": 0
                    }
                ],
                "allowMultiple": true,
                "isRequired": true,
                "discountApplies": false,
                "hideWhenNoneRemaining": false
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Attempt to move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");


            const alertElement = firstRegistrant.find("div.alert-validation");

            expect(alertElement.find("li").text()).toBe("Test Fee is required");
        });
    });

    describe("Hide When None Remaining", () => {
        it("Hides Checkbox when no quantity remaining", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": false,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": true
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.rock-check-box", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            // Check the fee so it uses the remaining quantity.
            await firstFee.find("input").setValue(true);

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.get(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Make sure the additional options area is empty.
            expect(secondRegistrant.get(".registration-additional-options").text()).toBe("Additional Options");
        });

        it("Hides NumberUpDown when no quantity remaining", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Test Fee",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": true,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": true
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.get(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            // Check the fee so it uses the remaining quantity.
            await firstFee.find(".numberincrement-up").trigger("click");

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.get(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Make sure the additional options area is empty.
            expect(secondRegistrant.get(".registration-additional-options").text()).toBe("Additional Options");
        });

        it("Hides DropDownList item when no quantity remaining", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Fee 1",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    },
                    {
                        "name": "Fee 2",
                        "cost": 0.00,
                        "guid": "4d107e3a-5d87-4a73-b75e-d563ae08b8c7",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": false,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": true
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                const fee = firstRegistrant.findAllComponents({ name: "DropDownList" })
                    .filter(c => c.props("label") === "Test Fee")[0];

                expect(fee).toBeTruthy();

                return fee;
            });

            // Select the first fee. I can't find a better way to do this
            // because Ant Select does not have an actual HTML element backing
            // it that we can change to trigger the update.
            firstFee.vm.$emit("update:modelValue", "fc790108-be66-4ca1-83c7-90faa7dc9a68");

            // Because we are modifying data directly, wait for all the computations
            // to finish in Vue.
            await flushPromises();

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option.
            const secondFee = await waitFor(() => {
                const fee = secondRegistrant.findAllComponents({ name: "DropDownList" })
                    .filter(c => c.props("label") === "Test Fee")[0];

                expect(fee).toBeTruthy();

                return fee;
            });

            // Make sure the fee only has the one remaining item.
            expect(secondFee.props("items").length).toBe(1);
        });

        it("Hides DropDownList when no item quantities remaining", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Fee 1",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    },
                    {
                        "name": "Fee 2",
                        "cost": 0.00,
                        "guid": "4d107e3a-5d87-4a73-b75e-d563ae08b8c7",
                        "originalCountRemaining": 0,
                        "countRemaining": 0
                    }
                ],
                "allowMultiple": false,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": true
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                const fee = firstRegistrant.findAllComponents({ name: "DropDownList" })
                    .filter(c => c.props("label") === "Test Fee")[0];

                expect(fee).toBeTruthy();

                return fee;
            });

            // Select the first fee. I can't find a better way to do this
            // because Ant Select does not have an actual HTML element backing
            // it that we can change to trigger the update.
            firstFee.vm.$emit("update:modelValue", "fc790108-be66-4ca1-83c7-90faa7dc9a68");

            // Because we are modifying data directly, wait for all the computations
            // to finish in Vue.
            await flushPromises();

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Make sure the additional options area is empty.
            expect(secondRegistrant.get(".registration-additional-options").text()).toBe("Additional Options");
        });

        it("Hides NumberUpDownGroup item when no available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Fee 1",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    },
                    {
                        "name": "Fee 2",
                        "cost": 0.00,
                        "guid": "4d107e3a-5d87-4a73-b75e-d563ae08b8c7",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    }
                ],
                "allowMultiple": true,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": true
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down-group", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            // Increment the fee so it uses the remaining quantity.
            await firstFee.find(".numberincrement-up").trigger("click");

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Wait to find the test fee option.
            const secondFee = await waitFor(() => {
                const fee = secondRegistrant.findAllComponents({ name: "NumberUpDownGroup" })
                    .filter(c => c.get("label").text() === "Test Fee")[0];

                expect(fee).toBeTruthy();

                return fee;
            });

            // Make sure the fee only has the one remaining item.
            expect(secondFee.props("options").length).toBe(1);
        });

        it("Hides NumberUpDownGroup item when no available quantity", async () => {
            const configuration = getConfigurationValues();

            if (!configuration.fees) {
                configuration.fees = [];
            }

            configuration.fees.push({
                "name": "Test Fee",
                "guid": "74b1dd21-40ff-4f2c-8e30-be01547dccc7",
                "items": [
                    {
                        "name": "Fee 1",
                        "cost": 0.00,
                        "guid": "fc790108-be66-4ca1-83c7-90faa7dc9a68",
                        "originalCountRemaining": 1,
                        "countRemaining": 1
                    },
                    {
                        "name": "Fee 2",
                        "cost": 0.00,
                        "guid": "4d107e3a-5d87-4a73-b75e-d563ae08b8c7",
                        "originalCountRemaining": 0,
                        "countRemaining": 0
                    }
                ],
                "allowMultiple": true,
                "isRequired": false,
                "discountApplies": false,
                "hideWhenNoneRemaining": true
            });

            const instance = mountBlock(RegistrationEntry, configuration, blockActions);

            // Wait for the block to become ready.
            waitFor(() => {
                expect(instance.find(".registrationentry-intro").isVisible()).toBe(true);
            });

            // Configure registration for 2 registrants.
            await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
            const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

            // Make sure we now have two registrants.
            expect(registrantCount).toBe("2");

            // Move to the first registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const firstRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(1)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Set values so we can move to the next screen.
            await firstRegistrant.findAll("input")[0].setValue("Ted");
            await firstRegistrant.findAll("input")[1].setValue("Decker");

            // Wait to find the test fee option.
            const firstFee = await waitFor(() => {
                return getMatching(firstRegistrant, "div.number-up-down-group", node => node.text().startsWith("Test Fee"));
            });

            await flushPromises();

            // Increment the fee so it uses the remaining quantity.
            await firstFee.find(".numberincrement-up").trigger("click");

            // Move to the second registrant.
            await instance.findAll(".btn-primary")
                .filter(node => node.text() === "Next" && node.isVisible())[0]
                .trigger("click");

            // Wait for the registrant to be visible.
            const secondRegistrant = await waitFor(() => {
                const registrant = instance.find(".registrationentry-registrant > div:nth-child(2)");
                expect(registrant.isVisible()).toBe(true);

                return registrant;
            });

            // Make sure the additional options area is empty.
            expect(secondRegistrant.get(".registration-additional-options").text()).toBe("Additional Options");
        });
    });
});


const configurationValues: RegistrationEntryInitializationBox = {
    "allowRegistrationUpdates": true,
    "timeoutMinutes": null,
    "session": {
        "registrationSessionGuid": "082a8a26-8784-47d5-8fd1-e20dd515489e",
        "registrationGuid": null,
        "registrants": [
            {
                "isOnWaitList": false,
                "familyGuid": "8965a84d-c5fb-43d9-8dae-133f0c2a4283",
                "personGuid": null,
                "fieldValues": {},
                "cost": 0.0,
                "feeItemQuantities": {},
                "signatureData": null,
                "guid": "b6f4d812-59bf-43bf-930d-9e8bd6a6ccbe"
            }
        ],
        "fieldValues": {},
        "registrar": null,
        "gatewayToken": null,
        "discountCode": null,
        "amountToPayNow": 0.0,
        "discountAmount": 0.0,
        "discountMaxRegistrants": 0.0,
        "discountPercentage": 0.0,
        "previouslyPaid": 0.0
    },
    "isUnauthorized": false,
    "instructionsHtml": "",
    "registrantTerm": "person",
    "pluralRegistrantTerm": "people",
    "pluralFeeTerm": "additional options",
    "registrantForms": [
        {
            "fields": [
                {
                    "guid": "f6f19316-0efb-4012-aa99-e563fbea65b4",
                    "fieldSource": 0,
                    "personFieldType": 0,
                    "isRequired": true,
                    "attribute": null,
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "<div class='row'><div class='col-md-6'>",
                    "postHtml": "    </div>",
                    "showOnWaitList": true,
                    "isSharedValue": false,
                    "isLockedIfValuesExist": false
                },
                {
                    "guid": "8e47dcdb-7c6c-4f8d-8b4b-6ab893ad1115",
                    "fieldSource": 0,
                    "personFieldType": 1,
                    "isRequired": true,
                    "attribute": null,
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "    <div class='col-md-6'>",
                    "postHtml": "    </div></div>",
                    "showOnWaitList": true,
                    "isSharedValue": false,
                    "isLockedIfValuesExist": false
                }
            ]
        }
    ],
    "fees": [],
    "familyMembers": [],
    "registrationAttributeTitleEnd": "Registration Information",
    "registrationAttributeTitleStart": "Registration Information",
    "registrationAttributesStart": [],
    "registrationAttributesEnd": [],
    "maxRegistrants": 10,
    "registrantsSameFamily": 0,
    "forceEmailUpdate": false,
    "registrarOption": 0,
    "cost": 0.00,
    "gatewayControl": {
        "fileUrl": "/Obsidian/Controls/TestGatewayControl.js",
        "settings": {}
    },
    "isRedirectGateway": false,
    "registrationTerm": "Registration",
    "spotsRemaining": null,
    "waitListEnabled": false,
    "instanceName": "Fee Unit Test Instance",
    "pluralRegistrationTerm": "Registrations",
    "amountDueToday": null,
    "initialAmountToPay": null,
    "hasDiscountsAvailable": false,
    "redirectGatewayUrl": "",
    "loginRequiredToRegister": false,
    "successViewModel": null,
    "startAtBeginning": true,
    "isExistingRegistration": false,
    "gatewayGuid": "6432d2d2-32ff-443d-b5b3-fb6c8414c3ad",
    "campuses": [
        {
            "value": "76882ae3-1ce8-42a6-a2b6-8c0b29cf8cf8",
            "text": "Main Campus",
            "category": null,
            "disabled": null
        }
    ],
    "maritalStatuses": [
        {
            "value": "5fe5a540-7d9f-433e-b47e-4229d1472248",
            "text": "Married",
            "category": null,
            "disabled": null
        },
        {
            "value": "96b57219-fe47-48eb-a2b7-4850b5fa7371",
            "text": "Unknown",
            "category": null,
            "disabled": null
        },
        {
            "value": "f19fc180-fe8f-4b72-a59c-8013e3b0eb0d",
            "text": "Single",
            "category": null,
            "disabled": null
        },
        {
            "value": "3b689240-24c2-434b-a7b9-a4a6cba7928c",
            "text": "Divorced",
            "category": null,
            "disabled": null
        }
    ],
    "connectionStatuses": [
        {
            "value": "41540783-d9ef-4c70-8f1d-c9e83d91ed5f",
            "text": "Member",
            "category": null,
            "disabled": null
        },
        {
            "value": "39f491c5-d6ac-4a9b-8ac0-c431cb17d588",
            "text": "Attendee",
            "category": null,
            "disabled": null
        },
        {
            "value": "b91ba046-bc1e-400c-b85d-638c1f4e0ce2",
            "text": "Visitor",
            "category": null,
            "disabled": null
        },
        {
            "value": "8ebc0ceb-474d-4c1b-a6ba-734c3a9ab061",
            "text": "Participant",
            "category": null,
            "disabled": null
        },
        {
            "value": "368dd475-242c-49c4-a42c-7278be690cc2",
            "text": "Prospect",
            "category": null,
            "disabled": null
        }
    ],
    "grades": [
        {
            "value": "0fed3291-51f3-4eed-886d-1d3df826beac",
            "text": "K",
            "category": null,
            "disabled": null
        },
        {
            "value": "6b5cdfbd-9882-4ebb-a01a-7856bcd0cf61",
            "text": "1st",
            "category": null,
            "disabled": null
        },
        {
            "value": "e475d0ca-5979-4c76-8788-d91adf595e10",
            "text": "2nd",
            "category": null,
            "disabled": null
        },
        {
            "value": "23cc6288-78ed-4849-afc9-417e0da5a4a9",
            "text": "3rd",
            "category": null,
            "disabled": null
        },
        {
            "value": "f0f98b9c-e6be-4c42-b8f4-0d8ab1a18847",
            "text": "4th",
            "category": null,
            "disabled": null
        },
        {
            "value": "3d8cdbc8-8840-4a7e-85d0-b7c29a019ebb",
            "text": "5th",
            "category": null,
            "disabled": null
        },
        {
            "value": "2d702ed8-7046-4da5-affa-9633a211f594",
            "text": "6th",
            "category": null,
            "disabled": null
        },
        {
            "value": "3fe728ac-be25-409a-98cb-3cfce5fa063b",
            "text": "7th",
            "category": null,
            "disabled": null
        },
        {
            "value": "d58d70af-3ccc-4d4e-bfaf-2014d8579d60",
            "text": "8th",
            "category": null,
            "disabled": null
        },
        {
            "value": "2a130e04-3712-427a-8bb0-473eb8ff8924",
            "text": "9th",
            "category": null,
            "disabled": null
        },
        {
            "value": "e04e3f62-ef5c-4860-8f32-1c152ca1700a",
            "text": "10th",
            "category": null,
            "disabled": null
        },
        {
            "value": "78f7d773-8244-4995-8bc4-ad6f6a7b7820",
            "text": "11th",
            "category": null,
            "disabled": null
        },
        {
            "value": "c49bd3af-ff94-4a7c-99e1-08503a3c746e",
            "text": "12th",
            "category": null,
            "disabled": null
        }
    ],
    "enableSaveAccount": true,
    "savedAccounts": [],
    "registrationInstanceNotFoundMessage": null,
    "isInlineSignatureRequired": false,
    "isSignatureDrawn": false,
    "signatureDocumentTerm": null,
    "signatureDocumentTemplateName": null,
    "races": [
        {
            "value": "52e12ebe-1fce-4b95-a677-aeeede9b1745",
            "text": "White",
            "category": null,
            "disabled": null
        },
        {
            "value": "3760ba55-3d68-4f55-aeef-0ac9f39d1730",
            "text": "Black or African American",
            "category": null,
            "disabled": null
        },
        {
            "value": "c734961e-43a7-4fb9-999e-b60d694268b4",
            "text": "American Indian or Alaska Native",
            "category": null,
            "disabled": null
        },
        {
            "value": "fcdc15df-b298-4067-ae8a-431e42da6f7e",
            "text": "Asian",
            "category": null,
            "disabled": null
        },
        {
            "value": "b46f3250-34a1-46e5-8171-9c8ed4fa0845",
            "text": "Native Hawaiian or Pacific Islander",
            "category": null,
            "disabled": null
        },
        {
            "value": "e364d2de-81a0-4f9c-8ecf-96cc68009251",
            "text": "Other",
            "category": null,
            "disabled": null
        }
    ],
    "ethnicities": [
        {
            "value": "05762be9-32d4-4c30-9cf1-e1513c5c8360",
            "text": "Hispanic or Latino",
            "category": null,
            "disabled": null
        },
        {
            "value": "2d1ef4cf-19e5-46bc-b4b1-591cff57e0d8",
            "text": "Not Hispanic or Latino",
            "category": null,
            "disabled": null
        }
    ],
    "hideProgressBar": false,
    "showSmsOptIn": false,
    "isPaymentPlanAllowed": false,
    "isPaymentPlanConfigured": false
};
