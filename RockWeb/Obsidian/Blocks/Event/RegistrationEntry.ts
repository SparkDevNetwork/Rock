import { BlockAction } from '../../Controls/RockBlock.js';
import RockButton from '../../Elements/RockButton.js';
import { BlockSettings } from '../../index.js';
import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Loading from '../../Controls/Loading.js';

enum Step {
    Start = 0
}

type StartPanelData = {
    InstructionsMarkup: string;
    MaxRegistrants: number;
    SlotsAvailable: number;
    WaitListEnabled: boolean;
};

export default defineComponent({
    name: 'Event.RegistrationEntry',
    components: {
        Loading,
        RockButton
    },
    setup() {
        return {
            blockSettings: inject('blockSettings') as BlockSettings,
            blockAction: inject('blockAction') as BlockAction
        };
    },
    data() {
        return {
            isLoading: false,
            currentStep: Step.Start,
            startPanelData: null as StartPanelData | null
        };
    },
    methods: {
        goToNext() {
            this.currentStep++;
        },
        async setLoadingWhileAwaiting<T>(callback: () => Promise<T>) {
            this.isLoading = true;
            const result = await callback();
            this.isLoading = false;
            return result;
        },
        async showStartPanel() {
            this.startPanelData = (await this.setLoadingWhileAwaiting(async () => await this.blockAction<StartPanelData>('GetStartPanelData', {
                slug: this.slug,
                registrationInstanceId: this.registrationInstanceId,
                registrationId: this.registrationId
            }))).data;
        }
    },
    computed: {
        slug(): string | null {
            return store.getters.pageParameter('Slug') || null;
        },
        registrationInstanceId(): number | null {
            return store.getters.pageParameter('RegistrationInstanceId') || null;
        },
        registrationId(): number | null {
            return store.getters.pageParameter('RegistrationId') || null;
        },
        showHowMany(): boolean {
            if (!this.startPanelData) {
                return false;
            }

            let max = this.startPanelData.MaxRegistrants;

            if (!this.startPanelData.WaitListEnabled && this.startPanelData.SlotsAvailable < max) {
                max = this.startPanelData.SlotsAvailable;
            }

            return max > MinRegistrants;
        }
    },
    watch: {
        currentStep: {
            immediate: true,
            async handler() {
                switch (this.currentStep) {
                    case Step.Start:
                        await this.showStartPanel();
                        return;
                    default:
                        throw new Error(`${this.currentStep} is not a valid step`);
                }
            }
        }
    },
    template: `
<Loading :isLoading="isLoading">
    <div v-if="currentStep === 0" class="registrationentry-intro">
        <div v-if="instructionsMarkup" v-html="instructionsMarkup" class="text-left"></div>

        <!--asp:Panel ID="pnlHowMany" runat="server" Visible="false" CssClass="registrationentry-intro">
            <h1>How many <asp:Literal ID="lRegistrantTerm" runat="server" /> will you be registering?</h1>
            <Rock:NumberUpDown ID="numHowMany"  runat="server" CssClass="input-lg" OnNumberUpdated="numHowMany_NumberUpdated"  />
        </asp:Panel-->

        <div class="actions">
            <RockButton class="btn-primary pull-right" @click="goToNext">
                Next
            </RockButton>
        </div>
    </div>
</Loading>`
});
