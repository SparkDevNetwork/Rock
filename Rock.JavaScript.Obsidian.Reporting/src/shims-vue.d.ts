/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/ban-types */
declare module "*.obs" {
    import { DefineComponent } from "vue";

    const Component: DefineComponent<{}, {}, any>;
    export default Component;
}
