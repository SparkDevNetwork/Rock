import { generateCommonEntityModule } from './Generators.js';

export type CommonEntity = {
    namespace: string;
    apiUrl: string;
};

// The common entity configs that will be used with generateCommonEntityModule to create store modules
export const commonEntities: CommonEntity[] = [
    { namespace: 'campuses', apiUrl: '/api/obsidian/v1/commonentities/campuses' },
    { namespace: 'definedTypes', apiUrl: '/api/obsidian/v1/commonentities/definedTypes' }
];
export const commonEntityModules = {};

// Generate a module for each config
for (const commonEntity of commonEntities) {
    commonEntityModules[commonEntity.namespace] = generateCommonEntityModule(commonEntity);
}