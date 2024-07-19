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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import RockButton from "./rockButton";
import TextBox from "./textBox";
import EmailBox from "./emailBox";
import { loadJavaScriptAsync } from "@Obsidian/Utility/page";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ElectronicSignatureValue } from "@Obsidian/ViewModels/Controls/electronicSignatureValue";
import RockForm from "./rockForm";
import { useStore } from "@Obsidian/PageState";
// LPC CODE
const store = useStore();

/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    var lang = typeof store.state.pageParameters["lang"] === 'string' ? store.state.pageParameters["lang"] : "";

    if (lang != "es") {
        lang = "en";
    }

    return lang;
}
// END LPC CODE
// #region SignaturePad library types.

interface ISignaturePadOptions {
    penColor?: string | undefined;
    backgroundColor?: string | undefined;
}

declare class SignaturePad {
    constructor(canvas: HTMLCanvasElement, options?: ISignaturePadOptions);
    clear: () => void;
    toDataURL: (type?: string) => string;
    off: () => void;
    addEventListener: (type: string, listener: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions) => void;
    removeEventListener: (type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | EventListenerOptions) => void;
}

declare global {
    /* eslint-disable-next-line */
    var SignaturePad: SignaturePad | undefined;
}

// #endregion

// Start loading the signature pad script so that it is available for us
// to use later when the control becomes visible.
const signaturePadPromise = loadJavaScriptAsync("/Scripts/signature_pad/signature_pad.umd.min.js", () => !!window.SignaturePad);

export default defineComponent({
    name: "ElectronicSignature",

    components: {
        RockButton,
        RockForm,
        TextBox,
        EmailBox
    },

    props: {
        modelValue: {
            type: Object as PropType<ElectronicSignatureValue>,
            required: false
        },

        isDrawn: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        documentTerm: {
            type: String as PropType<string>,
            default: "document"
        }
    },

    emits: {
        "update:modelValue": (_data: ElectronicSignatureValue) => true,
        "signed": () => true
    },

    setup(props, { emit }) {
        // #region Values

        const store = useStore();

        const signatureData = ref(props.modelValue?.signatureData ?? "");
        const signedByName = ref(props.modelValue?.signedByName ?? "");
        const signedByEmail = ref(props.modelValue?.signedByEmail ?? store.state.currentPerson?.email ?? "");

        const signatureCanvas = ref<HTMLCanvasElement | null>(null);
        const signatureCanvasContainer = ref<HTMLElement | null>(null);
        const isSigning = ref(true);

        let signaturePad: SignaturePad | null = null;

        // #endregion

        // #region Computed Values

        const signedByEmailLabel = computed((): string => {
            return getLang() == 'es' ? 'Por favor ingresa un email a continuaci\u00f3n a donde podamos enviar una copia del registro.' : `Please enter an email address below where we can send a copy of the ${props.documentTerm.toLowerCase()} to.`;
        });

        // #endregion

        // #region Functions

        /**
         * Resize the signature canvas element to be the width of the container.
         * This also handles keeping the signature pad content in sync after
         * the resize operation happens.
         */
        const resizeSignatureCanvas = (): void => {
            if (signaturePad === null || signatureCanvas.value === null || signatureCanvasContainer.value === null) {
                return;
            }

            // If the window is resized, that'll affect the drawing canvas
            // also, if there is an existing signature, it'll get messed up, so clear it and
            // make them sign it again. See additional details why
            // https://github.com/szimek/signature_pad
            let containerWidth = signatureCanvasContainer.value.clientWidth;
            if (containerWidth === 0) {
                containerWidth = 400;
            }

            // Note the suggestion  https://github.com/szimek/signature_pad#handling-high-dpi-screens
            // to re-calculate the ratio based on window.devicePixelRatio isn't needed.
            // We can just use the width() of the container and use fixed height of 100.
            const ratio = 1;
            signatureCanvas.value.width = containerWidth * ratio;
            signatureCanvas.value.height = 100 * ratio;
            signatureCanvas.value.getContext("2d")?.scale(ratio, ratio);

            // Clear the signature content so it isn't all skewed.
            signaturePad.clear();
        };

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the individual clicks the clear button.
         */
        const onClearClick = (): void => {
            // Clear the signature contents out of the UI.
            signatureData.value = "";
            signaturePad?.clear();
        };

        /**
         * Event handler for when the form has validated and can be submitted.
         * This is called indirectly in response to the "sign" and "complete" buttons.
         */
        const onSubmit = (): void => {
            if (isSigning.value) {
                // "sign" was clicked, move to the complete stage.
                isSigning.value = false;

                if (!signedByName.value && store.state.currentPerson) {
                    signedByName.value = store.state.currentPerson.fullName ?? "";
                }
            }
            else {
                // "complete" was clicked, store the new value and emit events.
                const newValue: ElectronicSignatureValue = {
                    signatureData: signatureData.value,
                    signedByName: signedByName.value,
                    signedByEmail: signedByEmail.value
                };

                emit("update:modelValue", newValue);
                emit("signed");
            }
        };

        // #endregion

        // Watch for changes to the model value and update our internal
        // signature values.
        watch(() => props.modelValue, () => {
            let isChanged = false;
            if (props.modelValue?.signatureData === signatureData.value && props.modelValue?.signedByName === signedByName.value && props.modelValue?.signedByEmail === signedByName.value) {
                return;
            }

            isChanged ||= updateRefValue(signatureData, props.modelValue?.signatureData ?? "");
            isChanged ||= updateRefValue(signedByName, props.modelValue?.signedByName ?? "");
            isChanged ||= updateRefValue(signedByEmail, props.modelValue?.signedByEmail ?? "");

            // Reset back to the sign state if anything actually changed.
            if (isChanged) {
                isSigning.value = true;

                if (signaturePad !== null) {
                    signaturePad.clear();
                }
            }
        });

        // Watch for the signature canvas to appear or disappear and setup or
        // tear down the signature pad object as needed.
        watch(signatureCanvas, async () => {
            if (signatureCanvas.value !== null) {
                await signaturePadPromise;

                signaturePad = new SignaturePad(signatureCanvas.value, {
                    backgroundColor: "white",
                    penColor: "black"
                });

                signaturePad.addEventListener("endStroke", () => {
                    signatureData.value = signaturePad?.toDataURL("image/png") ?? "";
                });

                resizeSignatureCanvas();
            }
            else {
                signaturePad?.off();
                signaturePad = null;
            }
        });

        // Listen for window resize events so we can update the canvas size.
        window.addEventListener("resize", () => resizeSignatureCanvas());

        return {
            isSigning,
            onClearClick,
            onSubmit,
            signatureCanvas,
            signatureCanvasContainer,
            signedByEmail,
            signedByEmailLabel,
            signedByName,
            signatureData,
        };
    },
    // LPC CODE
    methods: {
        getLang
    },
    // END LPC CODE

    // Template modified by LPC for language support
    template: `
<div>
    <div v-if="isSigning" class="signature-entry">
        <RockForm @submit="onSubmit">
            <div v-if="isDrawn" class="signature-entry-drawn">
                <div v-show="false">
                    <TextBox :modelValue="signatureData" label="Signature" rules="required" />
                </div>

                <span class="signature-entry-instructions text-muted small">Use mouse or finger to sign below.</span>

                <div ref="signatureCanvasContainer" class="signature-entry-drawn-container position-relative d-flex align-items-end">
                    <div class="signature-entry-canvas-col">
                        <canvas ref="signatureCanvas" class="e-signature-pad" style="border-bottom: 1px solid #c4c4c4;"></canvas>
                    </div>

                    <div class="signature-entry-clear-col">
                        <a class="btn btn-link p-1 p-md-2 text-color" title="Clear Signature" @click.prevent="onClearClick"><i class="fa fa-2x fa-undo"></i></a>
                    </div>
                </div>
            </div>

            <div v-else class="signature-entry-typed">
                <TextBox v-model="signedByName"
                    :label="getLang() == 'es' ? 'Escribir Nombre' : 'Type Name'"
                    rules="required" />
            </div>

            <div class="signature-entry-agreement">
                {{getLang() == 'es' ? 'Al hacer click en el bot&oacute;n de Firmar a continuaci&oacute;n, acepto el registro anterior y entiendo que es una representaci&oacute;n legal de mi firma.' : 'By clicking the sign button below, I agree to the above document and understand this is a legal representation of my signature.'}}
            </div>

            <div class="text-right">
                <RockButton type="submit" btnType="primary" btnSize="xs">
                    {{getLang() == 'es' ? 'Firmar' : 'Sign'}}</RockButton>
            </div>
        </RockForm>
    </div>

    <div v-else class="signature-entry-complete">
        <RockForm @submit="onSubmit">
            <TextBox v-if="isDrawn"
                v-model="signedByName"
                :label="getLang() == 'es' ? 'Nombre Legal' : 'Please enter your legal name'"
                rules="required" />

            <EmailBox v-model="signedByEmail"
                :label="signedByEmailLabel"
                rules="required" />

            <div class="text-right">
                <RockButton type="submit" btnType="primary" btnSize="xs">{{getLang() == 'es' ? 'Completar' : 'Complete'}}</RockButton>
            </div>
        </RockForm>
    </div>
</div>
`
});

