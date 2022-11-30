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

import { loadJavaScriptAsync } from "./page";

// Disable certain checks as they are needed to interface with existing JS file.
/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-explicit-any */

/** A generic set a server functions with no type checking. */
export type GenericServerFunctions = {
    [name: string]: (...args: unknown[]) => unknown;
};

/** A set of specific server functions that conform to an interface. */
export type ServerFunctions<T> = {
    [K in keyof T]: T[K] extends Function ? T[K] : never;
};

/**
 * An object that allows RealTime communication between the browser and the Rock
 * server over a specific topic.
 */
export interface ITopic<TServer extends ServerFunctions<TServer> = GenericServerFunctions> {
    /**
     * Allows messages to be sent to the server. Any property access is treated
     * like a message function whose property name is the message name.
     */
    server: TServer;

    /**
     * Gets the connection identifier for this topic. This will be the same for
     * all topics, but that should not be relied on staying that way in the future.
     */
    get connectionId(): string | null;

    /**
     * Gets a value that indicates if the topic is currently reconnecting.
     */
    get isReconnecting(): boolean;

    /**
     * Gets a value that indicates if the topic is disconnected and will no
     * longer try to connect to the server.
     */
    get isDisconnected(): boolean;

    /**
     * Registers a handler to be called when a message with the given name
     * is received.
     *
     * @param messageName The message name that will trigger the handler.
     * @param handler The handler to be called when a message is received.
     */
    on(messageName: string, handler: ((...args: any[]) => void)): void;

    /**
     * Registers a callback to be called when the connection has been lost
     * and then reconnected. This means a new connection identifier is now
     * in use and any state information has been lost.
     *
     * @param callback The callback to be called.
     */
    onReconnect(callback: ((server: TServer) => void) | ((server: TServer) => PromiseLike<void>)): void;

    /**
     * Registers a callback to be called when the connection has been lost
     * and will no longer try to reconnect.
     *
     * @param callback The callback to be called.
     */
    onDisconnect(callback: (() => void) | (() => PromiseLike<void>)): void;
}

interface IRockRealTimeStatic {
    getTopic<TServer extends ServerFunctions<TServer>>(identifier: string): Promise<ITopic<TServer>>;
}

let libraryObject: IRockRealTimeStatic | null = null;
let libraryPromise: Promise<boolean> | null = null;

/**
 * Gets the real time object from window.Rock.RealTime. If it is not available
 * then an exception will be thrown.
 *
 * @returns An instance of IRockRealTimeStatic.
 */
async function getRealTimeObject(): Promise<IRockRealTimeStatic> {
    if (libraryObject) {
        return libraryObject;
    }

    if (!libraryPromise) {
        libraryPromise = loadJavaScriptAsync("/Scripts/Rock/realtime.js", () => !!window["Rock"]?.["RealTime"]);
    }

    if (!await libraryPromise) {
        throw new Error("Unable to load RealTime library.");
    }

    libraryObject = window["Rock"]?.["RealTime"] as IRockRealTimeStatic;

    return libraryObject;
}

/**
 * Connects to a specific topic in the Rock RealTime system and returns an
 * instance to a proxy that handles sending to and receiving messages from
 * that specific topic.
 *
 * @param identifier The identifier of the topic to be connected to.
 *
 * @returns A proxy to handle communication with the topic.
 */
export async function getTopic<TServer extends ServerFunctions<TServer>>(identifier: string): Promise<ITopic<TServer>> {
    const realTime = await getRealTimeObject();

    return realTime.getTopic(identifier);
}
