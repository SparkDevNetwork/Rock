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
// The chat's cells that need a time zone with daylight saving, run on their own. A day's start is
// the viewer's calendar, which a daylight saving change makes 23 or 25 hours long, and jest cannot
// change the zone inside a run: so this run pins a zone that has one, set here before any test
// reads a date, and takes only the files named for it, which the main run never matches.
const { resolve } = require("path");

process.env.TZ = "America/Denver";

const base = require("../../../jest.config");

module.exports = {
    ...base,
    rootDir: resolve(__dirname, "../../.."),
    testMatch: ["<rootDir>/tests/Communication/Chat/**/*.zone.ts"]
};
