// @flow

export const filterProps = (props: Object): Object => {
  const newProps = { ...props };
  delete newProps.id;
  delete newProps.path;

  return newProps;
};

// XXX provide proper flow typing for react
export default (Comp: any) => (props: Object) => (
  <div data-props={JSON.stringify(props)}>
    <Comp {...filterProps(props)} />
  </div>
);

