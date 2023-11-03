import RegistrationEntry from "../../src/Event/registrationEntry";
import { RegistrationEntryBlockViewModel } from "../../src/Event/RegistrationEntry/types.partial";
import { mockBlockActions, mountBlock } from "../blocks";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { Guid } from "@Obsidian/Types";

function getConfigurationValues(): RegistrationEntryBlockViewModel {
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
            "ec86a84a-b558-409e-879a-5bac0033963d": "False",
            "3db902c3-4c74-4059-9563-d97bf4017fd7": "",
            "6bad5189-e622-4a69-bec1-7217a1998f8d": ""
        }
    };
}

describe("Issue 5547", () => {
    it("First registrant shows conditional field by default", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry, getConfigurationValues(), blockActions);

        // Move to the first registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Find the first registrant.
        const registrant = instance.findAll(".registrationentry-registrant-details")
            .filter(node => node.isVisible())[0];

        const defaultField = registrant.findAll("label")
            .filter(node => node.text().startsWith("If No, please tell us why?"))[0];

        expect(defaultField).toBeDefined();
        expect(defaultField.isVisible()).toBe(true);
    });

    it("First registrant switches conditional field when value changes", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry, getConfigurationValues(), blockActions);

        // Move to the first registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Find the first registrant.
        const registrant = instance.findAll(".registrationentry-registrant-details")
            .filter(node => node.isVisible())[0];

        const noConditionalField = registrant.findAll("label")
            .filter(node => node.text().startsWith("If No, please tell us why?"))[0];

        expect(noConditionalField).toBeDefined();
        expect(noConditionalField.isVisible()).toBe(true);

        await registrant.findAll(".toggle-container a")
            .filter(node => node.text() === "Yes")[0]
            .trigger("click");

        const yesConditionalField = registrant.findAll("label")
            .filter(node => node.text().startsWith("If yes, which flavor do you like best?"))[0];

        expect(registrant.findAll("label").filter(node => node.text().startsWith("If No, please tell us why?")).length).toBe(0);
        expect(yesConditionalField.isVisible()).toBe(true);
    });

    it("Second registrant shows conditional field by default", async () => {
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues
        });

        // Silence console errors about scrollTo() not being implemented in jsdom.
        global.scrollTo = jest.fn();

        const instance = mountBlock(RegistrationEntry, getConfigurationValues(), blockActions);

        // Configure registration for 2 registrants.
        await instance.find(".registrationentry-intro .numberincrement-up").trigger("click");
        const registrantCount = instance.find(".registrationentry-intro .numberincrement-value").text();

        // Make sure we now have two registrants.
        expect(registrantCount).toBe("2");

        // Move to the first registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        // Find the first registrant.
        const firstRegistrant = instance.findAll(".registrationentry-registrant-details")
            .filter(node => node.isVisible())[0];

        // Set values so we can move to the next screen.
        await firstRegistrant.findAll("input")[0].setValue("Ted");
        await firstRegistrant.findAll("input")[1].setValue("Decker");

        const firstDefaultField = firstRegistrant.findAll("label")
            .filter(node => node.text().startsWith("If No, please tell us why?"))[0];

        expect(firstDefaultField).toBeDefined();
        expect(firstDefaultField.isVisible()).toBe(true);

        // Move to the second registrant.
        await instance.findAll(".btn-primary")
            .filter(node => node.text() === "Next" && node.isVisible())[0]
            .trigger("click");

        const secondRegistrant = instance.findAll(".registrationentry-registrant-details")
            .filter(node => node.isVisible())[0];

        // Make sure we advanced to the second registrant.
        expect(secondRegistrant.findAll("input")[0].element.value).toBe("");

        const secondDefaultField = secondRegistrant.findAll("label")
            .filter(node => node.text().startsWith("If No, please tell us why?"))[0];

        // Make sure the second registrant's filtered field is visible by default.
        expect(secondDefaultField).toBeDefined();
        expect(secondDefaultField.isVisible()).toBe(true);
    });
});

/**
 * Configuration values returned by the block to replicate this issue.
 */
const configurationValues: RegistrationEntryBlockViewModel = {
    "currentPersonFamilyGuid": "8611f5e6-c63f-4c2a-ae5c-859cdb350cd0",
    "allowRegistrationUpdates": true,
    "timeoutMinutes": null,
    "session": {
        "registrationSessionGuid": "2dea8731-b754-478d-b8d5-d06aaeec65bd",
        "registrationGuid": null,
        "registrants": [
            {
                "isOnWaitList": false,
                "familyGuid": "2d2ce5a1-d9a3-42a2-aea7-81cac4736035",
                "personGuid": null,
                "fieldValues": {
                    "ec86a84a-b558-409e-879a-5bac0033963d": "False",
                    "3db902c3-4c74-4059-9563-d97bf4017fd7": "",
                    "6bad5189-e622-4a69-bec1-7217a1998f8d": ""
                },
                "cost": 0.0,
                "feeItemQuantities": {},
                "signatureData": null,
                "guid": "9aed944a-1e9e-419b-91a5-3c4ead30fa33"
            }
        ],
        "fieldValues": null,
        "registrar": null,
        "gatewayToken": null,
        "discountCode": null,
        "amountToPayNow": 0.0,
        "discountAmount": 0.0,
        "discountPercentage": 0.0,
        "previouslyPaid": 0.0,
        "savedAccountGuid": null,
        "discountMaxRegistrants": 0
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
                    "guid": "9f017ca7-a18e-48cb-a468-e8529bc50bf5",
                    "fieldSource": 0,
                    "personFieldType": 0,
                    "isRequired": true,
                    "attribute": null,
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "<div class='row'><div class='col-md-6'>",
                    "postHtml": "    </div>",
                    "showOnWaitList": true,
                    "isSharedValue": false
                },
                {
                    "guid": "88e99b84-edd9-41e6-b37d-c5a612984f23",
                    "fieldSource": 0,
                    "personFieldType": 1,
                    "isRequired": true,
                    "attribute": null,
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "    <div class='col-md-6'>",
                    "postHtml": "    </div></div>",
                    "showOnWaitList": true,
                    "isSharedValue": false
                },
                {
                    "guid": "ec86a84a-b558-409e-879a-5bac0033963d",
                    "fieldSource": 4,
                    "personFieldType": 0,
                    "isRequired": false,
                    "attribute": {
                        "fieldTypeGuid": "1edafded-dfe6-4334-b019-6eecba89e05a",
                        "attributeGuid": "0aa6cc8c-8a80-42fe-847b-622294360d17",
                        "name": "Do you like ice cream?",
                        "key": "Doyoulikeicecream",
                        "description": "",
                        "isRequired": false,
                        "order": 0,
                        "categories": [],
                        "configurationValues": {
                            "truetext": "Yes",
                            "falsetext": "No",
                            "BooleanControlType": "2"
                        },
                        "preHtml": "",
                        "postHtml": ""
                    },
                    "visibilityRuleType": 1,
                    "visibilityRules": [],
                    "preHtml": "",
                    "postHtml": "",
                    "showOnWaitList": false,
                    "isSharedValue": false
                },
                {
                    "guid": "3db902c3-4c74-4059-9563-d97bf4017fd7",
                    "fieldSource": 4,
                    "personFieldType": 0,
                    "isRequired": false,
                    "attribute": {
                        "fieldTypeGuid": "9c204cd0-1233-41c5-818a-c5da439445aa",
                        "attributeGuid": "c1d421d7-2461-4547-84c3-58bee6e87b3c",
                        "name": "If No, please tell us why?",
                        "key": "IfNopleasetelluswhy",
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
                    "visibilityRules": [
                        {
                            "comparedToRegistrationTemplateFormFieldGuid": "ec86a84a-b558-409e-879a-5bac0033963d",
                            "comparisonValue": {
                                "comparisonType": 1,
                                "value": "False"
                            }
                        }
                    ],
                    "preHtml": "",
                    "postHtml": "",
                    "showOnWaitList": false,
                    "isSharedValue": false
                },
                {
                    "guid": "6bad5189-e622-4a69-bec1-7217a1998f8d",
                    "fieldSource": 4,
                    "personFieldType": 0,
                    "isRequired": false,
                    "attribute": {
                        "fieldTypeGuid": "9c204cd0-1233-41c5-818a-c5da439445aa",
                        "attributeGuid": "d33c00ce-e050-4ed5-9ad4-cc50e92191f3",
                        "name": "If yes, which flavor do you like best?",
                        "key": "Ifyeswhichflavordoyoulikebest",
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
                    "visibilityRules": [
                        {
                            "comparedToRegistrationTemplateFormFieldGuid": "ec86a84a-b558-409e-879a-5bac0033963d",
                            "comparisonValue": {
                                "comparisonType": 1,
                                "value": "True"
                            }
                        }
                    ],
                    "preHtml": "",
                    "postHtml": "",
                    "showOnWaitList": false,
                    "isSharedValue": false
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
        "fileUrl": "",
        "settings": {}
    },
    "isRedirectGateway": false,
    "registrationTerm": "Registration",
    "spotsRemaining": null,
    "waitListEnabled": false,
    "instanceName": "Issue #5547 Instance",
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
            "value": "802fafa9-58f5-4ccb-b293-997a262d7703",
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
    "showSmsOptIn": false
};
