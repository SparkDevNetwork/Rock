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
import mitt from 'mitt';

/**
 * The bus allows page components to send and receive arbitrary data from other page components.
 */
type LogItem = {
    date: Date;
    message: string;
};

const bus = mitt();
const log: LogItem[] = [];

/**
* Write a log entry that a payload was sent or received.
*/
const writeLog = (msg: string) => {
    log.push({
        date: new Date(),
        message: msg
    });
};

/**
* Send the payload to subscribers listening for the event name
*/
function publish<T>(eventName: string, payload: T) {
    writeLog(`Published ${eventName}`);
    bus.emit(eventName, payload);
}

/**
* Whenever an event is received of eventName, the callback is executed with the message
* payload as a parameter.
*/
function subscribe<T>(eventName: string, callback: (payload: T) => void) {
    writeLog(`Subscribed to ${eventName}`);
    bus.on<T>(eventName, payload => {
        if (payload) {
            callback(payload);
        }
    });
}

export default {
    publish,
    subscribe,
    log
};
