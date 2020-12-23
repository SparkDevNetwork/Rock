import { createCommonEntityPicker } from "../Store/controlGenerator.js";

export default createCommonEntityPicker(
    'DefinedType',
    store => store.getters['definedTypes/all'].map(dt => ({
        key: dt.Guid,
        value: dt.Guid,
        text: dt.Name
    }))
);
