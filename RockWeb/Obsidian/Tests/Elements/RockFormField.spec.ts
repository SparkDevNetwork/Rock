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
import { mount } from '@vue/test-utils';
import TextBox from '../../Elements/TextBox';
import DatePicker from '../../Elements/DatePicker';

const elements = [TextBox, DatePicker];

for(const element of elements) {
    describe(element.name, () => {
        it('Does not render label when not passed', async () => {
            const wrapper = mount(element);
            await wrapper.setProps({
                modelValue: '',
                label: ''
            });

            const labels = wrapper.findAll('label');
            expect(labels.length).toBe(0);
        });

        it('Renders label when passed', async () => {
            const labelText = 'This is the label';

            const wrapper = mount(element);
            await wrapper.setProps({
                modelValue: '',
                label: labelText
            });

            const labels = wrapper.findAll('label');
            expect(labels.length).toBe(1);
            expect(labels[0].text()).toBe(labelText);
        });
    });
}