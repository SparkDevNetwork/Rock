import { mount } from '@vue/test-utils'
import TextBox from '../../Elements/TextBox';

describe('TextBox', () => {
  it('Does not render label when not passed', async () => {
    const wrapper = mount(TextBox)
    await wrapper.setProps({label: ''});

    const labels = wrapper.findAll('label');
    expect(labels.length).toBe(0);
  });

  it('Renders label when passed', async () => {
    const wrapper = mount(TextBox)
    const labelText = 'This is the label'
    await wrapper.setProps({label: labelText});

    const labels = wrapper.findAll('label');
    expect(labels.length).toBe(1);
    expect(labels[0].text()).toBe(labelText);
  });
})
