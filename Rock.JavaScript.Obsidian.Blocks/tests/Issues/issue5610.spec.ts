import { HttpResult } from "@Obsidian/Types/Utility/http";
import RegistrationEntry from "../../src/Event/registrationEntry.obs";
import { mockBlockActions, mountBlock } from "../blocks";
import { waitFor } from "../utils";
import { Guid } from "@Obsidian/Types";
import { flushPromises } from "@vue/test-utils";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";

function getConfigurationValues(): RegistrationEntryInitializationBox {
    // This is weird, but we have to do this because the block actually
    // modifies the configuration values which is non-standard.
    return JSON.parse(JSON.stringify(configurationValues));
}

function getDefaultAttributeFieldValues(): HttpResult<Record<Guid, unknown>> {
    return {
        isSuccess: true,
        isError: false,
        statusCode: 200,
        errorMessage: null,
        data: {
            "f8d52608-9763-43c6-98c4-d2a6e89e4f95": "",
            "8d0045c6-ded4-4771-9164-f17bbefaa338": ""
        }
    };
}

describe("Issue 5610", () => {
    it("Selecting family member overrides person attributes", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry,
            getConfigurationValues(),
            blockActions, {
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
        await waitFor(() => {
            expect(instance.find(".registrationentry-registrant > div:nth-child(1)").isVisible()).toBe(true);
        });

        // Clear the family member selection. I can't find a better way to do
        // this because Ant Select does not have an actual HTML element backing
        // it that we can change to trigger the update.
        instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAllComponents({name: "DropDownList"})[0]
            .vm
            .$emit("update:modelValue", "");

        // Because we are modifying data directly, wait for all the computations
        // to finish in Vue.
        await flushPromises();

        // Set values so we can move to the next screen.
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[0]
            .setValue("Ted");
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[1]
            .setValue("Decker");

        // Set the common value field.
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[2]
            .setValue("Common Person Attribute");

        // Move to the second registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Wait for the registrant to be visible.
        await waitFor(() => {
            expect(instance.find(".registrationentry-registrant > div:nth-child(2)").isVisible()).toBe(true);
        });

        // Check that the new common value is still set.
        expect(instance.get(".registrationentry-registrant > div:nth-child(2)")
            .findAll<HTMLInputElement>(".rock-text-box input")[2]
            .element.value
        ).toBe("Common Person Attribute");

        // Select the family member. I can't find a better way to do this
        // because Ant Select does not have an actual HTML element backing
        // it that we can change to trigger the update.
        const secondRegistrant = instance.findAllComponents({name: "Event.RegistrationEntry.Registrant"})[1];
        instance.get(".registrationentry-registrant > div:nth-child(2)")
            .findAllComponents({name: "DropDownList"})[0]
            .vm
            .$emit("update:modelValue", secondRegistrant.vm.familyMemberOptions[0].value);

        // Because we are modifying data directly, wait for all the computations
        // to finish in Vue.
        await flushPromises();

        // Do the final check. The person attribute should be blank and the
        // registrant attribute should still have the value.
        expect(instance.get(".registrationentry-registrant > div:nth-child(2)")
            .findAll<HTMLInputElement>(".rock-text-box input")[2]
            .element.value
        ).toBe("");
    });

    it("Selecting family member preserves registrant attributes", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry,
            getConfigurationValues(),
            blockActions, {
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
        await waitFor(() => {
            expect(instance.find(".registrationentry-registrant > div:nth-child(1)").isVisible()).toBe(true);
        });

        // Clear the family member selection. I can't find a better way to do
        // this because Ant Select does not have an actual HTML element backing
        // it that we can change to trigger the update.
        instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAllComponents({name: "DropDownList"})[0]
            .vm
            .$emit("update:modelValue", "");

        // Because we are modifying data directly, wait for all the computations
        // to finish in Vue.
        await flushPromises();

        // Set values so we can move to the next screen.
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[0]
            .setValue("Ted");
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[1]
            .setValue("Decker");

        // Set the common value field.
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[3]
            .setValue("Common Registrant Attribute");

        // Move to the second registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Wait for the registrant to be visible.
        await waitFor(() => {
            expect(instance.find(".registrationentry-registrant > div:nth-child(2)").isVisible()).toBe(true);
        });

        // Check that the new common value is still set.
        expect(instance.get(".registrationentry-registrant > div:nth-child(2)")
            .findAll<HTMLInputElement>(".rock-text-box input")[3]
            .element.value
        ).toBe("Common Registrant Attribute");

        // Select the family member. I can't find a better way to do this
        // because Ant Select does not have an actual HTML element backing
        // it that we can change to trigger the update.
        const secondRegistrant = instance.findAllComponents({name: "Event.RegistrationEntry.Registrant"})[1];
        instance.get(".registrationentry-registrant > div:nth-child(2)")
            .findAllComponents({name: "DropDownList"})[0]
            .vm
            .$emit("update:modelValue", secondRegistrant.vm.familyMemberOptions[0].value);

        // Because we are modifying data directly, wait for all the computations
        // to finish in Vue.
        await flushPromises();

        // Do the final check. The registrant attribute should still have the value.
        expect(instance.get(".registrationentry-registrant > div:nth-child(2)")
            .findAll<HTMLInputElement>(".rock-text-box input")[3]
            .element.value
        ).toBe("Common Registrant Attribute");
    });

    it("Clearing family member clears person attributes", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry,
            getConfigurationValues(),
            blockActions, {
        });

        // Move to the first registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Wait for the registrant to be visible.
        await waitFor(() => {
            expect(instance.find(".registrationentry-registrant > div:nth-child(1)").isVisible()).toBe(true);
        });

        // Set the person attribute field value.
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[2]
            .setValue("Common Person Attribute");

        // Clear the family member selection. I can't find a better way to do
        // this because Ant Select does not have an actual HTML element backing
        // it that we can change to trigger the update.
        instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAllComponents({name: "DropDownList"})[0]
            .vm
            .$emit("update:modelValue", "");

        // Because we are modifying data directly, wait for all the computations
        // to finish in Vue.
        await flushPromises();

        // Do the final check. The registrant attribute should still have the value.
        expect(instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll<HTMLInputElement>(".rock-text-box input")[2]
            .element.value
        ).toBe("");
    });

    it("Clearing family member preserves registrant attributes", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry,
            getConfigurationValues(),
            blockActions, {
        });

        // Move to the first registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Wait for the registrant to be visible.
        await waitFor(() => {
            expect(instance.find(".registrationentry-registrant > div:nth-child(1)").isVisible()).toBe(true);
        });

        // Set the registrant attribute field value.
        await instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll(".rock-text-box input")[3]
            .setValue("Common Registrant Attribute");

        // Clear the family member selection. I can't find a better way to do
        // this because Ant Select does not have an actual HTML element backing
        // it that we can change to trigger the update.
        instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAllComponents({name: "DropDownList"})[0]
            .vm
            .$emit("update:modelValue", "");

        // Because we are modifying data directly, wait for all the computations
        // to finish in Vue.
        await flushPromises();

        // Do the final check. The registrant attribute should still have the value.
        expect(instance.get(".registrationentry-registrant > div:nth-child(1)")
            .findAll<HTMLInputElement>(".rock-text-box input")[3]
            .element.value
        ).toBe("Common Registrant Attribute");
    });
});

/**
 * Configuration values returned by the block to replicate this issue.
 */
const configurationValues: RegistrationEntryInitializationBox = {
    "allowRegistrationUpdates": true,
    "timeoutMinutes": null,
    "session": {
        "registrationSessionGuid": "32c9c0e2-f378-46a7-8a36-9b211dbc2d5e",
        "registrationGuid": null,
        "registrants": [
            {
                "isOnWaitList": false,
                "familyGuid": "8611f5e6-c63f-4c2a-ae5c-859cdb350cd0",
                "personGuid": "ad28da19-4af1-408f-9090-2672f8376f27",
                "fieldValues": {
                    "17815a98-592f-471e-9303-dd0aeab502e0": "Admin",
                    "4ab11894-2b0a-43a4-89ba-d2f69935fb67": "Admin",
                    "8d0045c6-ded4-4771-9164-f17bbefaa338": ""
                },
                "cost": 0.0,
                "feeItemQuantities": {},
                "signatureData": null,
                "guid": "58445a45-46c9-4fdd-9610-d2524cdbb076"
            }
        ],
        "fieldValues": null,
        "registrar": null,
        "gatewayToken": null,
        "discountCode": null,
        "amountToPayNow": 0.0,
        "discountAmount": 0.0,
        "discountMaxRegistrants": 0,
        "discountPercentage": 0.0,
        "previouslyPaid": 0.0,
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
                    "guid": "17815a98-592f-471e-9303-dd0aeab502e0",
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
                    "guid": "4ab11894-2b0a-43a4-89ba-d2f69935fb67",
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
                },
                {
                    "guid": "f8d52608-9763-43c6-98c4-d2a6e89e4f95",
                    "fieldSource": 1,
                    "personFieldType": 0,
                    "isRequired": false,
                    "attribute": {
                        "fieldTypeGuid": "9c204cd0-1233-41c5-818a-c5da439445aa",
                        "attributeGuid": "dbd192c9-0aa1-46ec-92ab-a3da8e056d31",
                        "name": "Allergy",
                        "key": "Allergy",
                        "description": "The item(s) this person is allergic to.",
                        "isRequired": false,
                        "order": 0,
                        "categories": [
                            {
                                "guid": "752dc692-836e-4a3e-b670-4325cd7724bf",
                                "name": "Childhood Information",
                                "order": 0
                            },
                            {
                                "guid": "672715d8-f632-4cc7-b7da-c65758438835",
                                "name": "Check-in Manager Child Attributes",
                                "order": 0
                            },
                            {
                                "guid": "367571d1-62d3-4948-b588-c0fdce00cf27",
                                "name": "Check-in Manager Roster Attributes",
                                "order": 0
                            }
                        ],
                        "configurationValues": {},
                        "preHtml": null,
                        "postHtml": null
                    },
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "",
                    "postHtml": "",
                    "showOnWaitList": false,
                    "isSharedValue": true,
                    "isLockedIfValuesExist": false
                },
                {
                    "guid": "8d0045c6-ded4-4771-9164-f17bbefaa338",
                    "fieldSource": 4,
                    "personFieldType": 0,
                    "isRequired": false,
                    "attribute": {
                        "fieldTypeGuid": "9c204cd0-1233-41c5-818a-c5da439445aa",
                        "attributeGuid": "2d60d52e-0932-4a8b-90b7-ec8e7c8dea62",
                        "name": "Registrant Common",
                        "key": "RegistrantCommon",
                        "description": "",
                        "isRequired": false,
                        "order": 0,
                        "categories": [],
                        "configurationValues": {
                            "ispassword": "False",
                            "maxcharacters": "",
                            "showcountdown": "False"
                        },
                        "preHtml": "",
                        "postHtml": ""
                    },
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "",
                    "postHtml": "",
                    "showOnWaitList": false,
                    "isSharedValue": true,
                    "isLockedIfValuesExist": false
                }
            ]
        }
    ],
    "fees": [],
    "familyMembers": [
        {
            "guid": "ad28da19-4af1-408f-9090-2672f8376f27",
            "familyGuid": "8611f5e6-c63f-4c2a-ae5c-859cdb350cd0",
            "fullName": "Admin Admin",
            "fieldValues": {
                "17815a98-592f-471e-9303-dd0aeab502e0": "Admin",
                "4ab11894-2b0a-43a4-89ba-d2f69935fb67": "Admin",
                "8d0045c6-ded4-4771-9164-f17bbefaa338": ""
            }
        }
    ],
    "registrationAttributeTitleEnd": "Registration Information",
    "registrationAttributeTitleStart": "Registration Information",
    "registrationAttributesStart": [],
    "registrationAttributesEnd": [],
    "maxRegistrants": 10,
    "registrantsSameFamily": 2,
    "forceEmailUpdate": false,
    "registrarOption": 0,
    "cost": 0.00,
    "gatewayControl": {
        "fileUrl": "",
        "settings": {}
    },
    "isRedirectGateway": false,
    "registrationTerm": "Registration",
    "spotsRemaining": null,
    "waitListEnabled": false,
    "instanceName": "Issue 5610 Instance",
    "pluralRegistrationTerm": "Registrations",
    "amountDueToday": null,
    "initialAmountToPay": null,
    "hasDiscountsAvailable": false,
    "redirectGatewayUrl": "",
    "loginRequiredToRegister": false,
    "successViewModel": null,
    "startAtBeginning": true,
    "isExistingRegistration": false,
    "gatewayGuid": null,
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
    "savedAccounts": null,
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
