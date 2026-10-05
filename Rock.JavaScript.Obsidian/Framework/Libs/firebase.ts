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
// What chat's web push needs from Firebase: the app and the messaging token a browser registers
// with. Only the page loads it, and only once push is on; the push worker itself needs none of it.
export { initializeApp } from "@firebase/app";
export { getMessaging, getToken } from "@firebase/messaging";
