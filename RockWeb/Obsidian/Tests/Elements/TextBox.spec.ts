import { mount } from '@vue/test-utils';
import TextBox from '../../Elements/TextBox';

describe('TextBox', () => {
    it('Does not render label when not passed', async () => {
        const wrapper = mount(TextBox, {
            propsData: {
                modelValue: '',
                label: ''
            }
        });

        const labels = wrapper.findAll('label');
        expect(labels.length).toBe(0);
    });

    it('Renders label when passed', async () => {
        const labelText = 'This is the label';

        const wrapper = mount(TextBox, {
            propsData: {
                modelValue: '',
                label: labelText
            }
        });

        const labels = wrapper.findAll('label');
        expect(labels.length).toBe(1);
        expect(labels[0].text()).toBe(labelText);
    });

    it('Shows a countdown', async () => {
        const text = 'This is some text';
        const maxLength = 20;
        const charsRemaining = maxLength - text.length;

        const wrapper = mount(TextBox, {
            propsData: {
                modelValue: text,
                label: '',
                showCountDown: true,
                maxLength
            }
        });

        const countdownElements = wrapper.findAll('em.badge');
        expect(countdownElements.length).toBe(1);
        expect(countdownElements[0].text()).toBe(charsRemaining.toString());
    });
});
