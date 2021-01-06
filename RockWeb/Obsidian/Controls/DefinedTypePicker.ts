import { CommonEntityOption, createCommonEntityPicker } from '../Store/Generators.js';
import store from '../Store/index.js';

export default createCommonEntityPicker(
    'DefinedType',
    () => store.getters['definedTypes/all'].map(dt => ({
        Guid: dt.Guid,
        Id: dt.Id,
        Text: dt.Name
    } as CommonEntityOption))
);
