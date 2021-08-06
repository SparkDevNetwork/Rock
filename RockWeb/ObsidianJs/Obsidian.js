/**
 * This module enables Obsidian items (page and blocks) to
 * delay execution until all requirements are ready. Currently
 * this includes SystemJS itself and SystemJS having loaded the
 * import-map. Without this delay logic, there were occasional
 * race conditions where SystemJS would fail because it had not
 * loaded the import-map and things like "import 'vue'" would
 * fail.
 */

window.Obsidian = window.Obsidian || {};

( async function ()
{
    const callbacks = [];
    let isReady = false;
    const sleep = () => new Promise( resolve => setTimeout( resolve, 20 ) );
    let currentAttempt = 0;
    const maxAttempts = 10;
    let hasMoreAttempts = true;

    // Allow other code to register callbacks for when System JS is ready
    window.Obsidian.onReady = function ( callback )
    {
        if ( isReady )
        {
            callback();
        }
        else
        {
            callbacks.push( callback );
        }
    };

    // Wait for system itself to be loaded
    while ( !window.System && hasMoreAttempts )
    {
        currentAttempt++;
        hasMoreAttempts = currentAttempt <= maxAttempts;

        if ( hasMoreAttempts )
        {
            await sleep();
        }
        else
        {
            console.log( `SystemJS did not resolve after ${currentAttempt} attempts. No more attempts will be made.` );
        }
    }

    // Configure SystemJS to append .js extension to files without an extension
    const origResolve = System.constructor.prototype.resolve;
    const defaultExtension = '.js';
    const expectedExtensions = [ defaultExtension, '.ts', '.css', '.json' ];

    System.constructor.prototype.resolve = function ( moduleId, ...args )
    {
        const isPackage = moduleId.indexOf( '/' ) === -1 && moduleId.indexOf( '\\' ) === -1;
        const hasExtension = expectedExtensions.some( ext => moduleId.endsWith( ext ) );

        return origResolve.call(
            this,
            ( hasExtension || isPackage ) ? moduleId : `${moduleId}${defaultExtension}`,
            ...args
        );
    };

    // Wait for the import-map to be loaded
    let resolvedAddress = '';

    while ( !resolvedAddress && hasMoreAttempts )
    {
        currentAttempt++;
        hasMoreAttempts = currentAttempt <= maxAttempts;

        try
        {
            resolvedAddress = System.resolve( 'vue' );
        }
        catch ( e )
        {
            if ( hasMoreAttempts )
            {
                await sleep();
            }
            else
            {
                console.log( `Import-Map did not resolve after ${currentAttempt} attempts. No more attempts will be made.` );
            }
        }
    }

    // Execute callbacks
    isReady = true;

    for ( const callback of callbacks )
    {
        callback();
    }
} )();
