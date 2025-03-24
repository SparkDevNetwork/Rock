import AntSelect from "ant-design-vue/lib/select";
import AntAutoComplete from "ant-design-vue/lib/auto-complete";
import * as Axios from "axios";
import { DateTime, FixedOffsetZone } from "luxon";
import * as Mitt from "mitt";
import * as Vue from "vue";
import * as TSLib from "tslib";

// This shrinks the bundle by 11KB over just importing all of luxon.
const Luxon = {
    DateTime,
    FixedOffsetZone
};

// Only include the components we are actually going to use.
const AntDesignVue = {
    Select: AntSelect,
    AutoComplete: AntAutoComplete
};

export {
    AntDesignVue, // 284KB
    Axios, // 13.7KB
    Luxon, // 60KB
    Mitt, // 374b
    Vue, // 127.2KB
    TSLib, // 6.8KB
};
