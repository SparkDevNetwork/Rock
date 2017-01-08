import { render } from "react-dom";
import { AppContainer } from "react-hot-loader";
import { flatten, find } from "lodash";

export default class Bootstrap {

  constructor(props) {
    this.props = props;
    if (!this.props.pageId) return;

    this.bootstrapDOM()
      .then(this.render);
  }

  async bootstrapDOM(){

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
    

    const page = await fetch(
      `/api/Pages?$filter=Id eq ${this.props.pageId}&$expand=Blocks,Blocks/BlockType`
    )
      .then((x) => x.json());

    if (!page || !page.length) {
      console.warn(`No page details found for PageId: ${this.props.pageId}`);
      return;
    }

    const pageData = page[0].Blocks.map(({ BlockType, Order, Name, Id, Zone }) => ({
      order: Order,
      name: Name,
      id: Id,
      zone: Zone,
      path: BlockType.Path.replace("~/Blocks/", "").replace(".ascx", ""),
    }));

    // Step 2
    this.blocks = flatten(zones.map(({ id, element }) => 
      [].slice.call(element.querySelectorAll("[id^=\"bid_\"]"))
        .map((block) => ({ zone: id, id: block.id, element: block }))
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
      System.import(`../Blocks/${block.path}.js`)
        .then((component) => {
          const Component = component.default;
          const target = [].slice.call(
            block.element.querySelectorAll("[data-react-block]")
          )[0];
          
          // this is where redux and apollo integration can be shared between trees
          let props = {};
          try {
            props = JSON.parse(target.dataset.initialProps) || {};
          } catch (e) { console.warn(`error parsing initial props for ${block.path}`, e); }

          render(
            <AppContainer><Component {...props} /></AppContainer>
            , target
          );
        })
        .catch((error) => console.warn(`error loading ${block.path}`, error)) 
    });

  }

}
