
import { mount } from "enzyme";
import { mountToJson } from "enzyme-to-json";

import SampleReactBlock from "../SampleReactBlock.block";

it("has a default state of 1", () => {
  const wrapper = mount(<SampleReactBlock />);
  expect(wrapper.state().counter).toEqual(1);
});

it("can be set an initial state from props", () => {
  const wrapper = mount(<SampleReactBlock startingNumber={10} />);
  expect(wrapper.state().counter).toEqual(10);
});

it("has the ability to increment when clicking the increment button", () => {
  const wrapper = mount(<SampleReactBlock />);
  const initialNumber = wrapper.state().counter;
  const button = wrapper.find("[data-spec=\"increment\"]");
  button.simulate("click");
  expect(wrapper.state().counter).toEqual(initialNumber + 1);
});

it("has the ability to decrement when clicking the decrement button", () => {
  const wrapper = mount(<SampleReactBlock />);
  const initialNumber = wrapper.state().counter;
  const button = wrapper.find("[data-spec=\"decrement\"]");
  button.simulate("click");
  expect(wrapper.state().counter).toEqual(initialNumber - 1);
});

it("matches the expected UI structure", () => {
  const wrapper = mount(<SampleReactBlock />);
  expect(mountToJson(wrapper)).toMatchSnapshot();
});
