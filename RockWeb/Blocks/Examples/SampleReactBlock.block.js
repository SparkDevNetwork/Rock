// @flow
import { Component } from "react";

type ISampleReactBlockProps = {
  startingNumber?: number,
};

type ISampleReactBlockState = {
  counter: number,
};

// eslint-disable-next-line
const link = "https://chrome.google.com/webstore/detail/react-developer-tools/fmkadmapgofadopljbjfkapdkoienihi?hl=enj";

export default class SampleReactBlock extends Component {

  props: ISampleReactBlockProps
  state: ISampleReactBlockState = { counter: 1 }

  componentWillMount() {
    if (typeof this.props.startingNumber !== "undefined") {
      this.setState({ counter: this.props.startingNumber });
    }
  }

  increment = () => this.setState(({ counter }) => ({ counter: counter + 1 }))
  decrement = () => this.setState(({ counter }) => ({ counter: counter - 1 }))

  render() {
    return (
      <div>
        <h5>Counter is {this.state.counter}</h5>
        <button type="button" data-spec="increment" onClick={this.increment}>Up</button>
        <button type="button" data-spec="decrement" onClick={this.decrement}>Down</button>
        <div style={{ paddingTop: "10px" }}>
          <p><em>
              This is markup created by a react component. It is initialized with state
              from the server. Open the <a href={link}>React Dev Tools</a> and check out
              the state (counter amount), initialProps (startingNumber), and layout.
            </em></p>
        </div>
      </div>
    );
  }
}

