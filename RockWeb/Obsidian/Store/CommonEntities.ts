// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
import { generateCommonEntityModule } from './Generators';

export type CommonEntity = {
    namespace: string;
    apiUrl: string;
};

// The common entity configs that will be used with generateCommonEntityModule to create store modules
export const commonEntities: CommonEntity[] = [
    { namespace: 'campuses', apiUrl: '/api/v2/CommonEntities/Campuses' },
    { namespace: 'definedTypes', apiUrl: '/api/v2/CommonEntities/DefinedTypes' }
];
export const commonEntityModules = {};

// Generate a module for each config
for (const commonEntity of commonEntities) {
    commonEntityModules[commonEntity.namespace] = generateCommonEntityModule(commonEntity);
}