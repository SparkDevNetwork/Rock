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

import { defineComponent } from 'vue';
import RockForm from '../../../Controls/RockForm';
import CheckBox from '../../../Elements/CheckBox';
import EmailBox from '../../../Elements/EmailBox';
import JavaScriptAnchor from '../../../Elements/JavaScriptAnchor';
import RockButton from '../../../Elements/RockButton';
import TextBox from '../../../Elements/TextBox';

export default defineComponent({
    name: 'Event.RegistrationEntry.Summary',
    components: {
        RockButton,
        TextBox,
        CheckBox,
        EmailBox,
        RockForm,
        JavaScriptAnchor
    },
    methods: {
        onPrevious() {
            this.$emit('previous');
        }
    },
    template: `
<div class="registrationentry-summary">
    <RockForm>
        <div class="well">
            <h4>This Registration Was Completed By</h4>
            <div class="row">
                <div class="col-md-6">
                    <TextBox label="First Name" rules="required" />
                </div>
                <div class="col-md-6">
                    <TextBox label="Last Name" rules="required" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <EmailBox label="Send Confirmation Emails To" rules="required" />
                    <CheckBox label="Should Your Account Be Updated To Use This Email Address?" />
                </div>
            </div>
        </div>

        <div>
            <h4>Payment Summary</h4>
            <div class="clearfix">
                <div class="form-group pull-right">
                    <label class="control-label">Discount Code</label>
                    <div class="input-group">
                        <input type="text" class="form-control input-width-md input-sm" />
                        <JavaScriptAnchor class="btn btn-default btn-sm margin-l-sm">Apply</JavaScriptAnchor>
                    </div>
                </div>
            </div>
            <div class="fee-table">
                <div class="row hidden-xs fee-header">
                    <div class="col-sm-6">
                        <strong>Description</strong>
                    </div>
                    <div class="col-sm-3 fee-value">
                        <strong>Amount</strong>
                    </div>
                </div>
                <div class="row fee-row-cost">
                    <div class="col-sm-6 fee-caption">
                    </div>
                    <div class="col-sm-3 fee-value">
                        <span class="visible-xs-inline">Amount:</span>
                        $ 50.00
                    </div>
                </div>
                <div class="row fee-row-total">
                    <div class="col-sm-6 fee-caption">
                        Total
                    </div>
                    <div class="col-sm-3 fee-value">
                        <span class="visible-xs-inline">Amount:</span>
                        $ 50.00
                    </div>
                </div>
            </div>

            <div class="row fee-totals">
                <div class="col-sm-offset-8 col-sm-4 fee-totals-options">
                    <div class="form-group static-control">
                        <label class="control-label">
                            Total Cost
                        </label>
                        <div class="control-wrapper">
                            <div class="form-control-static">
                                $50.00
                            </div>
                        </div>
                    </div>
                    <div class="form-group static-control">
                        <label class="control-label">Amount Due</label>
                        <div class="control-wrapper">
                            <div class="form-control-static">
                                $50.00
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="actions">
            <RockButton btnType="default" @click="onPrevious">
                Previous
            </RockButton>
            <RockButton btnType="primary" class="pull-right" type="submit">
                Finish
            </RockButton>
        </div>
    </RockForm>
</div>`
});