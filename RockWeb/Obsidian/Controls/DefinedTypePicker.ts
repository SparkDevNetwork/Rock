import { createCommonEntityPicker } from "../Store/generators.js";
import store from '../Store/index.js';

export default createCommonEntityPicker(
    'DefinedType',
    () => store.getters['definedTypes/all'].map(dt => ({
        key: dt.Guid,
        value: dt.Guid,
        text: dt.Name
    }))
);
