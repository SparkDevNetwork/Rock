<script>
    function calledFunction() {
        $( ".smartsearch-type li[data-target*='Search']" ).each(function( index ) {
            var originalTarget = $(this).data("target");
            var newTarget = originalTarget;
            if(newTarget.indexOf("Person/Search") > -1){
                newTarget = newTarget.replace("Person/Search","Person/Legacy685/Search");
                $(this).attr("data-target",newTarget);

            }
            else{
                $(this).hide();
            }
            console.log( originalTarget + ": " +newTarget );
          });
    }

    $( document ).ready( function() {
        calledFunction();
    });

    $(function(){
        Sys.Application.add_load(function () {
            calledFunction();
        });
    });

    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest( function() {
        calledFunction();
    });
</script>