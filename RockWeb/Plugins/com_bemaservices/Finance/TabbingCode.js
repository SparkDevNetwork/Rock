/* This code is intended to be added to an HTML block under the PledgeDetail block on the page to create new pledge entries.
 * It is designed to allow faster pledge entry without using the mouse.
 * 
 * The first entry needs to be entered with the mouse to select the account and dates.  From there on, the name ca be typed, tab used to select the person, 
 * enter pressed to close the select, tab used to get to the frequency, the first letter typed to select the frequency, tab used to get to amount,
 * amount entered and then the enter key will press the Save & New button to start the process again.
 */
<script>

    var prm = Sys.WebForms.PageRequestManager.getInstance ();
    
prm.add_endRequest(function() {

        loadscript();

    });
    
    function showPersonPicker($obj)
{
    if ( $( '.picker-menu, .dropdown-menu' ).css ( 'display' ) == 'none' )
        {
        $('.picker-person .picker-label').click();
    }
}

var numaccounts = 0;

function loadscript()
{
    $(".picker-person .picker-label").unbind('focus');
    $( '.picker-person .picker-label' ).on ( 'focus', function (){
        showPersonPicker($(this));
    });

    $( "[id$='_tbAmount']" ).unbind ( 'keydown' );
    $( "[id$='_tbAmount']" ).on ( 'keydown', function ( e ) {
        var keyCode = e.keyCode || e.which;

    console.log ( 'Key Pressed (tbAmount):' + keyCode + ':' + e.shiftKey );

    if ( keyCode == 13 )
    {
        e.preventDefault();
        console.log ($( "[id$='_btnSaveNew']" ).html () );
        $( "[id$='_btnSaveNew']" ).get ( 0 ).click ();
    }
});

    $( ".picker-person .picker-search" ).unbind ( 'keydown' );
    $( ".picker-person .picker-search" ).on ( 'keydown', function ( e ) {
        var keyCode = e.keyCode || e.which;

        console.log ( 'Key Pressed:' + keyCode + ':' + e.shiftKey );

        if ( keyCode == 9 )
        {
            e.preventDefault();
            if ($( '.scroll-container-picker input:checked' ).length )
            {
                var next = $( '.scroll-container-picker input:checked' ).parent ().parent ().parent ().next ( 'li' ).find ( 'input:radio' );
                var prev = $( '.scroll-container-picker input:checked' ).parent ().parent ().parent ().prev ( 'li' ).find ( 'input:radio' );

                if ( ( e.shiftKey && keyCode == 9 ) || keyCode == 38 )
                {
                    if ( prev.length )
                    {
                        prev.click();
                    }
                }
                else
                {
                    if ( next.length )
                    {
                        next.click();
                    }
                }

            } else
            {
                if ( ( e.shiftKey && keyCode == 9 ) || keyCode == 38 )
                {
                    $('.scroll-container-picker input:last').click();
                }
                else
                {
                    $('.scroll-container-picker input:first').click();
                }
            }
        }
        else if ( keyCode == 13 )
        {
            e.preventDefault();
            $( '.scroll-container-picker input:checked' ).click ();
        }
        else if ( keyCode == 27 )
        {
            e.preventDefault();
            $( '[id*="_ppSelectNew_btnCancel"]' ).click ();
            var len = $( "[id*='ddlIndividual']" ).find ( "option" ).length;
            if ( len > 0 )
            {
                $("select[id*='_dvpFrequencyType']").click();
            }
        }
    });

}

$(document).ready( function(){
    loadscript();
    showPersonPicker ($( '.picker-label:focus' ) );
});

</script>