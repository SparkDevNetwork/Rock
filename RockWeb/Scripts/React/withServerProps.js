// @flow

// XXX provide proper flow typing for react
export default (Comp: any) => (props: Object) => (
  <div data-props={JSON.stringify(props)}>
    <Comp {...props} />
  </div>
);

