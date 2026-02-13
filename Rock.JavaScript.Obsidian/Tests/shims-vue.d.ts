declare module "*.obs" {
    import { DefineComponent } from "vue";

    const Component: DefineComponent<{}, {}, any>;
    export default Component;
}
