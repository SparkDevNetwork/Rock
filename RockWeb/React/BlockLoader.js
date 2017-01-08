import { Component } from "react";


export default class BlockLoader extends Component {
  
  state = { components: [] }
  static defaultProps = { blocks: [], pageId: null }

  async componentWillMount() {
    if (!this.props.pageId) return;

    const page = await fetch(
      `/api/Pages?$filter=Id eq ${this.props.pageId}&$expand=Blocks,Blocks/BlockType`
    )
      .then((x) => x.json());

    if (!page || !page.length) {
      console.warn(`No page details found for PageId: ${this.props.pageId}`);
      return;
    }

    const blocks = page[0].Blocks.map(({ BlockType, Order, Name, Id, Zone }) => ({
      order: Order,
      name: Name,
      id: Id,
      zone: Zone,
      path: BlockType.Path.replace("~/Blocks/", "").replace(".ascx", ""),
    }));
     
    this.loadBlocks(blocks);
  }

  componentWillReceiveProps() {
    // XXX this should only be in development
    this.loadBlocks(this.state.blocks);
  }
  
  loadBlocks = (blocks) => {
    const imports = blocks
      .sort((a, b) => {
        if (a.order < b.order) return -1;
        if (a.order > b.order) return 1;
        return 0;
      })
      .map(({ path }) =>
        System.import(`../Blocks/${path}.js`).catch((error) => null)
      )
      
    Promise.all(imports)
      .then((x) => x.filter(x => !!x))
      .then((components) => {
        this.setState({
          components: components.map((x => x.default)),
          blocks,
        });
      })
      .catch((error) => {
        console.warn(error);
      })
  }

  render() {
    return (
      <div>
        {this.state.components.map((Block, index) => <Block key={index} />)}
      </div>
    )
  }
}

