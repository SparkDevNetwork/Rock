// @flow
import { render } from "react-dom";
import { AppContainer } from "react-hot-loader";
import { flatten, find } from "lodash";
import { Provider } from "react-redux";

import createStore from "./store";

export type IBlockDetails = {
  element: HTMLElement,
  id: string,
  path: string,
  zone: string,
};

type IBootstrapProps = {
  pageId: number,
};

export default class Bootstrap {
  props: IBootstrapProps;
  store: Object;
  blocks: [IBlockDetails];

  constructor(props: Object) {
    this.props = props;
    if (!this.props.pageId) return;

    this.store = createStore();

    this.bootstrapDOM()
      .then(this.render);
  }

  async bootstrapDOM() {
    /*
     * Steps for bootstrapping a rock page with React components
     *
     * 1. Read the zones from the page-content
     * 2. Load the data for the page
     * 3. For each zone, find all of the blocks
     * 4. Find data-react-block if it exists (if not end)
     * 5. Load the block based on the page id and block id
     * 6. Either copy markup and rebuild it, or render directly into that markup if possible
     * 7. Party
    */

    const root = document.getElementById("page-content");

    // Step 1
    const zones = [].slice.call(root.querySelectorAll("[id^=\"zone-\"]"))
      .map((element) => ({ id: element.id, element }));

    const expand = "$expand=Blocks,Blocks/BlockType";
    // const select = "$select=Blocks/BlockType/Path,Order,Name,Id,Zone";
    const page = await fetch(
      `/api/Pages?$filter=Id eq ${this.props.pageId}&${expand}`
    , { credentials: "same-origin" })
      .then((x) => x.json());

    if (!page || !page.length) {
      // eslint-disable-next-line
      console.warn(`No page details found for PageId: ${this.props.pageId}`);
      return;
    }

    const pageData = page[0].Blocks.map(({ BlockType, Order, Name, Id, Zone }) => ({
      order: Order,
      name: Name,
      id: Id,
      zone: Zone,
      path: BlockType.Path,
    }));

    // Step 2
    this.blocks = flatten(zones.map(({ id, element }) =>
      [].slice.call(element.querySelectorAll("[id^=\"bid_\"]"))
        .map((block) => ({ zone: id, id: block.id, element: block })),
    ))
      // Step 3
      .filter(({ element }) => {
        const zone = [].slice.call(element.querySelectorAll("[data-react-block]"));
        return zone.length > 0;
      })
      // Step 4
      .map((block) => {
        const blockData = find(pageData, ({ zone, id }) => (
          block.id === `bid_${id}` &&
          block.zone === `zone-${zone.toLowerCase()}`
        ));
        if (!blockData) return null;

        return {
          ...block,
          path: blockData.path,
        };
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
        const localPath = path.replace("~/Blocks/", "").replace(".ascx", "");
        // $FlowIgnore
        loader = import(`../../Blocks/${localPath}.block.js`);
      } else if (path.includes("~/Plugins")) {
        const localPath = path.replace("~/Plugins/", "").replace(".ascx", "");
        // $FlowIgnore
        loader = import(`../../Plugins/${localPath}.block.js`);
      } else {
        loader = Promise.reject("Block could not be found on file system");
      }

      loader.then((component) => {
        const Component = component.default;
        const { reducer } = component;

        if (reducer && reducer.key) this.store.inject(reducer.key, reducer);

        const target = [].slice.call(
            block.element.querySelectorAll("[data-react-block]"),
          )[0];

          // this is where redux and apollo integration can be shared between trees
        let props = { block };

        try {
          const { initialProps } = target.dataset;
          props = (initialProps && JSON.parse(initialProps)) || {};
          // eslint-disable-next-line
        } catch (e) { console.warn(`error parsing initial props for ${block.path}`, e); }

        render(
          <AppContainer>
            <Provider store={this.store}>
              <Component {...props} />
            </Provider>
          </AppContainer>
          , target,
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
