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

export type InputField = {
    selector: string;
    title: string;
    placeholder: string;
};

type HideableInputField = InputField & {
    display?: "show" | "hide" | "required";
};

type InputFields = {
    ccnumber?: InputField;
    ccexp?: InputField;
    cvv?: HideableInputField;
    checkaccount?: InputField;
    checkaba?: InputField;
    checkname?: InputField;

    [index: string]: InputField | undefined;
};

export type CardTokenResponse = {
    number: string | null;

    bin: string | null;

    exp: string | null;

    hash: string | null;

    type: string | null;
};

export type CheckTokenResponse = {
    name: string | null;

    account: string | null;

    hash: string | null;

    aba: string | null;
};

export type TokenResponse = {
    token?: string;

    card?: CardTokenResponse;

    check?: CheckTokenResponse;

    validationMessage?: string;

    errorMessage?: string;
};

export type TimeoutCallback = () => void;
export type ValidationCallback = (field: string, validated: boolean, message: string) => void;
export type FieldsAvailableCallback = () => void;
export type ResponseCallback = (response: TokenResponse) => void;

export type CollectJSOptions = {
    paymentSelector: string;

    variant: "inline";

    fields: InputFields;

    styleSniffer: boolean;

    customCss: Record<string, string>;

    focusCss: Record<string, string>;

    invalidCss: Record<string, string>;

    placeholderCss: Record<string, string>;

    timeoutDuration?: number;

    timeoutCallback?: TimeoutCallback;

    validationCallback?: ValidationCallback;

    fieldsAvailableCallback?: FieldsAvailableCallback;

    callback: ResponseCallback;
};

type CollectJS = {
    config: CollectJSOptions;

    configure: (options?: CollectJSOptions) => void;

    startPaymentRequest: (event?: Event) => void;

    clearInputs: () => void;

    iframes: Record<string, HTMLIFrameElement>;

    inSubmission: boolean;
};

declare global {
    /* eslint-disable-next-line */
    var CollectJS: CollectJS | undefined;
}

export { };
