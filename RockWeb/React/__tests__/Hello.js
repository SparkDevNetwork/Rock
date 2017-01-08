
import renderer from "react-test-renderer";

import Hello from "../Hello";

it("should render Hello World", () => {
  const hello = renderer.create(<Hello />);
  expect(hello).toMatchSnapshot();
});
