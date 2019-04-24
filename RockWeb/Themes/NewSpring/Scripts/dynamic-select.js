document.addEventListener("DOMContentLoaded", function(){
    // Get all dynamic selects
    var dynamicSelects = document.querySelectorAll('[data-dynamic-select]');

    // Loop through them and bind change events
    for (var i = 0; i < dynamicSelects.length; i++) {
        var selectElement = dynamicSelects[i];
        
        // On change of dynamic select, update the href of corresponding dynamic link
        selectElement.addEventListener('change', (event) => {
            var target = event.target;
            var selectUID = target.dataset.dynamicSelect;
            var submitElement = document.querySelector('[data-dynamic-link="'+selectUID+'"]');

            if(submitElement.classList.contains('disabled')) {
                document.querySelector('[data-dynamic-link="'+selectUID+'"]').classList.remove('disabled');
            };

            var selectedHref = target.options[target.selectedIndex].value;
            submitElement.href=selectedHref;

            if(selectedHref == '') {
                document.querySelector('[data-dynamic-link="'+selectUID+'"]').classList.add('disabled');
            }
        });
    }
});