class RockMediaPlayer {

    constructor( elementId ) {
        
        // Create and setup media player
        this.player = new Plyr( elementId);
        
        // Define and configure properties
        this.trackWatch = false;
        this.timerId = null;
        this.map = null;
        this.mapSize = null;
        this.percentWatched = 0;
        this.hiddenInputIdPercentage = '';
        this.hiddenInputIdMap = '';
        this.previousPlayBit = null;
        this.appendToMap = true;
        this.resumePlaying = true;
        this.elementId = elementId;
        this.writeInteraction = 'always';
        
        // Define play event
        this.player.on( 'play', event => {
            
            // Check that player is prepped. This will happen on the 'canplay' event for HTML5
            // but will need to be checked here for YouTube and Vimeo as they do not support that
            // event.
            if ( this.mapSize == null )
            {
                this.prepareForPlay();
            }
            
            // Start play timer
            if ( this.trackWatch )
            {
                this.timerId = setInterval( () => this.trackPlay(), 1000 );
            }
        });
        
        // Define pause event
        this.player.on( 'pause', event => {
            
            // Clear timer
            clearInterval( this.timerId );
            
            // Check if we need to write a watch bit. Not checking here can lead to gaps in the map
            // depending on the timing of the play/pause. 
            this.markBitWatched();
            
            // TODO: Send map to interaction

            this.writeDebugMessage( "Event 'pause' called." );
        });
        
        // Define ended event
        this.player.on( 'ended', event => {
            
            // TODO: Send map to interaction
            this.writeDebugMessage( "Event 'ended' called." );
        });

        // Define loadeddata event
        this.player.on( 'loadeddata', event => {
            
            this.writeDebugMessage( "Event 'loadeddata' called." + this.player.duration );
            this.prepareForPlay();
        });

        // Define ready event
        this.player.on( 'ready', event => {
            
            this.writeDebugMessage( "Event 'ready' called." );
        });
        
        // Define canplay event. Note this is run only for HTML5 usage. Youtube and Vimeo
        // will not use this method. They will run this logic on play. This means that the 
        // the srub bar won't be in the proper resume location for YouTube and Vimeo until
        // playback starts.
        this.player.once( 'canplay', event => {
            this.writeDebugMessage( "Event 'canplay' called." );
            
            //this.prepareForPlay();
        });
    }
    
       
    

    // Getter to return a run-length encoded representation of the map as a string.
    
    get getRleMap() 
    {        
        return this.arrayToRle( this.map );
    }
    
    // Setter to take a rle string and updates the map.
    set setRleMap( value ) 
    {
        this.map = rleToArray( value );
        
        // Check that the resulting map is the correct size, otherwise use a blank map
        this.validateMap();
    }

    /* #region Player Methods */

    // Initializes maps and sets resume location if requested
    prepareForPlay() {
        this.writeDebugMessage( "Preparing the player." );
        
        this.initializeMap();
            
        this.setResume();
    }

    // Method: If resumePlaying is enabled, sets the start position at the last place the individual watched.
    setResume() {
    
        // Change that resume was configured
        if ( this.resumePlaying == false )
        {
            return;
        }
        
        let startPosition = 0;
        
        for ( let i = 0; i < this.map.length; i++) {
            if ( this.map[i] == 0 )
            {
            startPosition = i;
            break;
            }
        }
        
        this.player.currentTime = parseInt(startPosition);
        this.writeDebugMessage( 'Set starting position at: ' + startPosition  );
    }

    // Method: Called each second during the watch.
    trackPlay() {
        this.markBitWatched();
    }

    // Method: Marks the current second as watched.
    markBitWatched() 
    {
        var playBit = Math.floor( this.player.currentTime ) - 1;
        
        // Make sure to not double count. This can occur with timings of play/pause.
        if ( playBit == this.previousPlayBit ) {
            return;
        }
        
        var currentValue = this.map[playBit];

        if ( currentValue < 9 )
        {
            this.map[playBit] = ++currentValue;
        }
        
        var unwatchedItemCount = this.map.filter(item => item == 0).length;
        
        this.percentWatched = 1 - ( unwatchedItemCount / this.mapSize );
        
        // Update hidden fields
        if ( this.hiddenInputIdPercentage != '' )
        {
            document.getElementById( this.hiddenInputIdPercentage ).value = this.percentWatched;
        }
        
        if ( this.hiddenInputIdMap != '' )
        {
            document.getElementById( this.hiddenInputIdMap ).value = this.map.join('');
        }
        

        this.writeDebugMessage( this.map.join('') );
        
        let currentRleMap = this.getRleMap;
        this.writeDebugMessage( 'RLE: ' + currentRleMap );

        this.writeDebugMessage( 'Player Time: ' + this.player.currentTime + ' Current Time: ' + playBit + '; Percent Watched: ' + this.percentWatched + '; Unwatched Items: ' + unwatchedItemCount + '; Map Size: ' + this.mapSize );

        this.previousPlayBit = playBit;
    }
    /* #endregion */

    /* #region Map Methods */
    
    // Method: Marks the current second as watched.
    initializeMap()
    {
        // Load an existing map if requested and it exists
        if ( this.appendToMap ) {
            this.loadExistingMap();
        }
        else 
        {
            // Create blank map
            this.createBlankMap();
        }
        
        // Write debug message
        this.writeDebugMessage( 'Init Map (' + this.mapSize + '): ' + this.map.join('') );
    }

    // Method: Creates a blank map of the size of the current video
    createBlankMap() 
    {
        this.mapSize = Math.floor( this.player.duration ) - 1;

        // Duration will be -1 if the video does not exist
        if ( this.mapSize < 0 )
        {
            this.mapSize = 0;
        }
        
        this.map = new Array( this.mapSize ).fill( 0 );
        
        this.writeDebugMessage( 'Blank map created of size: ' + this.mapSize );
    }

    // Method: Load an existing map using the precedence order of:
    // 1. Id of provided hidden field
    // 2. Map property provided as a string (we'll convert it to an array)
    // 3. Otherwise create a blank map
    loadExistingMap()
    {
        this.writeDebugMessage( 'Loading existing map.' );
        
        let existingMapString = ''
        
        // If a hidden map input was provided then use that
        if ( this.hiddenInputIdMap != '' && document.getElementById( this.hiddenInputIdMap ).value != '' )
        {
            existingMapString = document.getElementById( this.hiddenInputIdMap ).value;
            this.writeDebugMessage( 'Using map from hidden field: ' + existingMapString );
        }
        else if ( this.map != null && !(this.map instanceof Array) )
        {
            // Map was provided as a string to the map property, we'll need to convert it to an array
            existingMapString = this.map;
            this.writeDebugMessage( 'Map provided in .map property: ' + existingMapString );
        }
        else {
            // There's no existing map to use
            this.createBlankMap();
            this.writeDebugMessage( 'No previous map provided, creating a blank map.' );
            return;
        }
        
        // If existing map has a comma the format is assumed to be run length encoded
        if ( existingMapString.indexOf(',') > -1 ) 
        { 
            this.setRleMap = existingMapString;
            return;
        }
        
        // Split the string into an array
        this.map = existingMapString.split( '' );
        
        // Ensure the array is the same length as the video otherwise 
        // ignore the map. TODO: might re-thing what to do with an invalid length in the future
        this.mapSize = Math.floor( this.player.duration ) - 1;
        if ( this.map.length != this.mapSize ) 
        {
            this.writeDebugMessage( 'Provided map size (' + this.map.length + ') did not match the video (' + this.mapSize + '). Using a blank map.' );
            this.createBlankMap();
        }
    }
    /* #endregion */

    /* #region Helper Methods */

    // Method: Write debug message if debugging is enabled
    writeDebugMessage( message ) 
    {
        if ( this.debug ) {
            console.log( 'RMP(' + this.elementId + '):' + message );
        }
    }

    // Checks that the size of the map array matches the length of the video. If
    // not it creates a new blank map.
    validateMap()
    {
        if ( this.map.length != this.mapSize ) 
        {
            this.writeDebugMessage( 'Provided map size (' + this.map.length + ') did not match the video (' + this.mapSize + '). Using a blank map.' );
            this.createBlankMap();
        }
    }

    // Takes an array and returns the RLE string for it
    // RLE mapping is that segments are separated by commas. The last character of the segment is the value
    // Example:  1100011 = 21,30,21 (two ones, three zeros, two ones).
    arrayToRle( value )
    {
        // Ensure passed value is an array
        if ( !Array.isArray(value) )
        {
            return '';
        }
        
        let encoding = [],
            previous,
            i,
            count;
            
        for (count = 1, previous = value[0], i = 1; i < value.length; i++) {
            if (value[i] !== previous) {
                encoding.push( "" + count + previous );
                count = 1;
                previous = value[i];
            } else {
                count++;
            }
        }
        
        // Add last pair
        encoding.push( "" + count + previous);
        
        return encoding.join(',');
    }

    // Takes a RLE string and returns an unencoded array
    rleToArray( value )
    {
        let unencoded = [];
        
        let rleArray = value.split(',');
        
        for( var i = 0; i < rleArray.length; i++ )
        {
            let components = rleArray[i];
            
            let value = parseInt( components[ components.length - 1 ] );
            let size = parseInt( components.substring(0, components.length - 1) );
        
            let segment = new Array( size ).fill( value );

            unencoded.push( ...segment );
        }

        return unencoded;
    }

    /* #endregion */
}