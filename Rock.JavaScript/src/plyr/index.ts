/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/naming-convention */

import Hls from "hls.js";
import Plyr from "plyr";
import "./custom.css";

// Extend the existing Window interface to allow the assignment of new properties.
// https://stackoverflow.com/a/12709880
declare global {
    interface Window {
        Hls: any;
        Plyr: any;
    }
}

window.Hls = Hls;
window.Plyr = Plyr;
