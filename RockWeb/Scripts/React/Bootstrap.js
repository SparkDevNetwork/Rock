// @flow
import { render } from "react-dom";
import { AppContainer } from "react-hot-loader";
import { flatten } from "lodash";
// import { Provider } from "react-redux";

// import createStore from "./store";
import withProps from "./withServerProps";

export type IBlockDetails = {
  element: HTMLElement,
  id: string,
  path: string,
  zone: string,
};

export default class Bootstrap {
  // store: Object;
  blocks: [IBlockDetails];

  constructor() {
    // this.store = createStore();

    this.bootstrapDOM()
      .then(this.render);
  }

  async bootstrapDOM() {
    /*
     * Steps for bootstrapping a rock page with React components
     *
     * 1. Read the zones from the page-content
     * 2. For each zone, find all of the blocks
     * 3. Find id=react_ if it exists (if not end)
     * 5. Load the block based on the and block id
     * 6. Either copy markup and rebuild it, or render directly into that markup if possible
     * 7. Party
    */

    const root = document.getElementById("page-content");

    // Step 1
    const zones = [].slice.call(root.querySelectorAll("[id^=\"zone-\"]"))
      .map((element) => ({ id: element.id, element }));

    // Step 2
    this.blocks = flatten(zones.map(({ id, element }) =>
      [].slice.call(element.querySelectorAll("[id^=\"bid_\"]"))
        .map((block) => ({ zone: id, id: block.id, element: block })),
    ))
      // Step 3
      .filter(({ element }) => {
        const zone = [].slice.call(element.querySelectorAll("[id^=\"react_\"]"));
        return zone.length > 0;
      })
      // Step 4
      .map((block) => {
        const target = [].slice.call(
            block.element.querySelectorAll("[id^=\"react_\"]"),
        )[0];

        try {
          const props = JSON.parse(
            target.firstChild.dataset.props.replace(/&quote;/gmi, "\""),
          );

          return {
            ...block,
            path: `~${props.path}`,
            props,
            target,
          };
        } catch (e) {
          // eslint-disable-next-line
          console.warn(`error parsing props for block_id: ${block.id}`, e);
        }

        return false;
      })
      .filter((x) => !!x)
      ;
  }

  render = () => {
    this.blocks.forEach((block) => {
      const { path } = block;
      let loader;

      // This is used to limit webpacks bundle scope
      // otherwise it will look in all folder for js files to bundle
      // XXX I think this can be done in the webpack config so we can strip the
      // file type and import it directly
      if (path.includes("~/Blocks")) {
        const localPath = path.replace("~/Blocks/", "");
        // $FlowIgnore
        loader = import(`../../Blocks/${localPath}.block.js`);
      } else if (path.includes("~/Plugins")) {
        const localPath = path.replace("~/Plugins/", "");
        // $FlowIgnore
        loader = import(`../../Plugins/${localPath}.block.js`);
      } else {
        loader = Promise.reject("Block could not be found on file system");
      }

      loader.then((component) => {
        const Component = withProps(component.default);
        // const { reducer } = component;

        // if (reducer && reducer.key) this.store.inject(reducer.key, reducer);

        // this is where redux and apollo integration can be shared between trees
        render(
          <AppContainer>
            <Component {...block.props} />
          </AppContainer>
          , block.target,
        );
      })
        .catch(
          // eslint-disable-next-line
          (error) => console.warn(`error loading ${block.path.replace(".ascx", "")}`,
          error,
        ));
    });
  }

}
