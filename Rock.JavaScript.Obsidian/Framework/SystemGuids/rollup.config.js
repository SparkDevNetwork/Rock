import { generateAutoBundles } from "../../build/rolluplib";
import * as path from "path";

// eslint-disable-next-line no-undef
const dirname = __dirname;

export default generateAutoBundles(dirname, path.join(dirname, "..", "..", "dist", "Framework", "SystemGuids"));
