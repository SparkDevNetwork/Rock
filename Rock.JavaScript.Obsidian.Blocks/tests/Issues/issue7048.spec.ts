import { HttpResult } from "@Obsidian/Types/Utility/http";
import { InvokeBlockActionFunc } from "@Obsidian/Types/Utility/block";
import RegistrationEntry from "../../src/Event/registrationEntry.obs";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { RegistrationEntryCostSummaryBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryCostSummaryBag";
import { RegistrationCostSummaryType } from "@Obsidian/Enums/Event/registrationCostSummaryType";
import { mockBlockActions, mountBlock } from "../blocks";
import { waitFor } from "../utils";
import { Guid } from "@Obsidian/Types";

type BlockInstance = ReturnType<typeof mountBlock<typeof RegistrationEntry>>;

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
        data: {}
    };
}

/**
 * The result the HTTP layer produces when a request never gets a response
 * (connection dropped, request aborted). It does not throw; it resolves to
 * an error result with no data and no message.
 */
function getNetworkFailureResult<T>(): HttpResult<T> {
    return {
        isSuccess: false,
        isError: true,
        statusCode: 0,
        errorMessage: null,
        data: null
    };
}

function getCalculateCostSuccessResult(): HttpResult<RegistrationEntryCostSummaryBag[]> {
    return {
        isSuccess: true,
        isError: false,
        statusCode: 200,
        errorMessage: null,
        data: [
            {
                type: RegistrationCostSummaryType.Cost,
                description: "Ted Decker",
                cost: 50,
                discountedCost: 50,
                minimumPaymentAmount: 50,
                defaultPaymentAmount: 50,
                registrationRegistrantGuid: "9aed944a-1e9e-419b-91a5-3c4ead30fa33"
            }
        ]
    };
}

/**
 * Mounts the block and walks through the intro and registrant steps to the
 * summary step, which is where the cost is calculated.
 */
async function mountAtSummaryStep(blockActions: InvokeBlockActionFunc): Promise<BlockInstance> {
    const instance = mountBlock(RegistrationEntry, getConfigurationValues(), blockActions);

    // Move to the first registrant.
    await instance.findAll(".btn-primary")
        .filter(node => node.text() === "Next" && node.isVisible())[0]
        .trigger("click");

    await waitFor(() => {
        expect(instance.find(".registrationentry-registrant").exists()).toBe(true);
    });

    // Fill in the registrant so we can move to the summary.
    const firstRegistrant = instance.findAll(".registrationentry-registrant > div")[0];
    await firstRegistrant.findAll("input")[0].setValue("Ted");
    await firstRegistrant.findAll("input")[1].setValue("Decker");

    await instance.findAll(".btn-primary")
        .filter(node => node.text() === "Next" && node.isVisible())[0]
        .trigger("click");

    await waitFor(() => {
        expect(instance.find(".registrationentry-summary").exists()).toBe(true);
    });

    return instance;
}

describe("Issue 7048", () => {
    test("Failed cost calculation shows an error and disables submission", async () => {
        const submitRegistration = jest.fn(() => getNetworkFailureResult<unknown>());
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues,
            CheckDiscountCode: () => getNetworkFailureResult(),
            CalculateCost: () => getNetworkFailureResult(),
            SubmitRegistration: submitRegistration
        });

        const instance = await mountAtSummaryStep(blockActions);

        await waitFor(() => {
            expect(instance.find(".registrationentry-summary .alert-danger").text()).toContain("could not be calculated");
        });

        // The cost table must not render as if the registration were free.
        expect(instance.find(".registrationentry-summary .fee-table").exists()).toBe(false);

        // The submit button must stay disabled until the cost is known.
        const submitButton = instance.find(".registrationentry-summary button[type='submit']");
        expect(submitButton.attributes("disabled")).toBeDefined();
        expect(submitRegistration).not.toHaveBeenCalled();
    });

    test("Retrying a failed cost calculation restores the cost and enables submission", async () => {
        let calculateCostResult: HttpResult<RegistrationEntryCostSummaryBag[]> = getNetworkFailureResult();
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues,
            CheckDiscountCode: () => getNetworkFailureResult(),
            CalculateCost: () => calculateCostResult
        });

        const instance = await mountAtSummaryStep(blockActions);

        await waitFor(() => {
            expect(instance.find(".registrationentry-summary .alert-danger").exists()).toBe(true);
        });

        // The connection is back; retry the calculation.
        calculateCostResult = getCalculateCostSuccessResult();
        await instance.findAll(".registrationentry-summary .alert-danger button")
            .filter(node => node.text() === "Retry")[0]
            .trigger("click");

        await waitFor(() => {
            expect(instance.find(".registrationentry-summary .fee-table").exists()).toBe(true);
        });

        expect(instance.find(".registrationentry-summary .alert-danger").exists()).toBe(false);

        const submitButton = instance.find(".registrationentry-summary button[type='submit']");
        expect(submitButton.attributes("disabled")).toBeUndefined();

        // A $50 cost means a payment must be collected on the next step.
        expect(submitButton.text()).toBe("Next");
    });

    test("Submitting while the cost is unavailable does not post the registration", async () => {
        const submitRegistration = jest.fn(() => getNetworkFailureResult<unknown>());
        const blockActions = mockBlockActions({
            GetDefaultAttributeFieldValues: getDefaultAttributeFieldValues,
            CheckDiscountCode: () => getNetworkFailureResult(),
            CalculateCost: () => getNetworkFailureResult(),
            SubmitRegistration: submitRegistration
        });

        const instance = await mountAtSummaryStep(blockActions);

        await waitFor(() => {
            expect(instance.find(".registrationentry-summary .alert-danger").exists()).toBe(true);
        });

        // Fill in the registrar so the form itself is valid.
        const registrarInputs = instance.findAll(".registrationentry-summary .well input");
        await registrarInputs[0].setValue("Ted");
        await registrarInputs[1].setValue("Decker");
        await registrarInputs[2].setValue("ted@rocksolidchurchdemo.com");

        // Submit the form directly, bypassing the disabled button, the way an
        // implicit (Enter key) submission would.
        await instance.find(".registrationentry-summary form").trigger("submit");

        await waitFor(() => {
            expect(instance.find(".registrationentry-summary").text()).toContain("has not been calculated yet");
        });

        expect(submitRegistration).not.toHaveBeenCalled();
    });
});

/**
 * Configuration values returned by the block to replicate this issue: a
 * single-registrant template with a $50 cost and a non-redirect gateway.
 */
const configurationValues: RegistrationEntryInitializationBox = {
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
                    "isSharedValue": false,
                    "isLockedIfValuesExist": false
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
    "cost": 50.00,
    "gatewayControl": {
        "fileUrl": "/Obsidian/Controls/Internal/testGatewayControl.obs",
        "settings": {}
    },
    "isRedirectGateway": false,
    "registrationTerm": "Registration",
    "spotsRemaining": null,
    "waitListEnabled": false,
    "instanceName": "Issue #7048 Instance",
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
    "hideProgressBar": false,
    "showSmsOptIn": false,
    "isPaymentPlanAllowed": false,
    "isPaymentPlanConfigured": false,
    "disableCaptchaSupport": true
};
