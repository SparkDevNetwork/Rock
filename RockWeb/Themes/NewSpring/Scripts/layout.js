$(document).ready(function(){

    // Dynamic Block Columns based on data-column attribute
	let columns = $('[data-column]:not([data-column=""');

	for (let i = 0; i < columns.length; i++) {
		let elem = columns[i],
            blockWrapper = elem.closest("[id^=bid_]"),
		    columnClasses = elem.dataset.column.split(" ");

        // If there are classes from the data-column attribute, add a js-col class
        if(columnClasses.length>0){
		    columnClasses.push('js-col','js-col-' + i);
        }

        // Create new js column element
		let column = document.createElement('div');
        // Add classes from data attribute to new js column element
        for (let j = 0; j < columnClasses.length; j++) {
            let className = columnClasses[j];
            if(className && className != ''){
                column.classList.add(className);
            }
        }
        // Insert new js column element into dom before block wrapper
		blockWrapper.parentNode.insertBefore(column, blockWrapper);
        // Move block wrapper element inside of new js column element
		column.appendChild(blockWrapper);
	}




    // Dynamic Rows & Containers
    let jscolumns = $('[class*=js-col]'),
        elemsToMove = [],
        newRow = null;

    // Loop through jscolumns
    for (let i = 0; i < jscolumns.length; i++) {
        let elem = $(jscolumns[i]),
            prev = elem.prevAll('.js-col'),
            next = elem.nextAll('.js-col'),
            parentDiv = elem.parent()[0];

        // Push current column element into array to be dumped into new row
        elemsToMove.push(elem);

        // If there is no next element, build the container/row and put it into the DOM
        if(next.length === 0) {

            // Create new row element
            newRow = document.createElement("div");

            // Assign row classes
            newRow.classList.add('js-row','row','row-no-gutters');

            // Create new container element
            let newContainer = document.createElement("div");

            // Assign container classes
            newContainer.classList.add('js-container','container-fluid','soft-sides','xs-soft-half-sides');

            // Insert newRow into newContainer
            newContainer.append(newRow);

            // Insert newContainer back into DOM
            parentDiv.insertBefore(newContainer, elemsToMove[0].get(0));

            // Loop through elemsToMove and add them to newRow
            for (let j = 0; j < elemsToMove.length; j++) {
                let elemToMove = elemsToMove[j].get(0);
                newRow.appendChild(elemToMove);
            }

            // Reset elemsToMove array and keep going
            elemsToMove = [];

        }
    }






	var c,
		currentScrollTop = 0,
		body = $('body'),
		navWrapper = document.querySelector('#navigation-wrapper');
		windowHeight = $(window).height(),
		heroHeight = $('.hero').height(),
		navHeight = navWrapper.offsetHeight,
        adminBarHeight = $('#cms-admin-footer').height(),
		transparentNav = $('.js-classes').hasClass('nav-transparent');

	if(!transparentNav) {
		body.css('padding-top',navHeight);
	}

	/* full height elements (bc lame webkit bug) */
    expandVH100Elements();
    window.onresize = expandVH100Elements;

    function expandVH100Elements() {
        $('.vh-100').height(windowHeight);
    }

	$(window).scroll(function () {
		var a = $(window).scrollTop();
        var b = navWrapper.offsetHeight;

		currentScrollTop = a;

		if (c < currentScrollTop && a > b) {
			navWrapper.style.marginTop = b*-1 + 'px'
        } else if (c > currentScrollTop && !(a <= b)) {
			navWrapper.style.marginTop = 0 + 'px'
        }

		if (transparentNav) {
			if (a > heroHeight - b) {
				// add white background
				$('.js-classes').removeClass('nav-transparent');
			} else {
				// remove white background
				$('.js-classes').addClass('nav-transparent');
			}
		}

        c = currentScrollTop;
	});

    // If page loads mid-page, hide nav
	if($(window).scrollTop() > 100) {
        navWrapper.style.marginTop = navWrapper.offsetHeight * -1 + 'px';
    }

	// function calculateLayout() {
	// 	var adminBar = document.querySelector('#cms-admin-footer');
	// 	var navigation = document.querySelector('#navigation');
	// 	var secondaryNavigation = document.querySelector('#navigation-secondary');
	// 	var offsetHeight = navigation.offsetHeight + secondaryNavigation.offsetHeight;

	// 	var firstRow = $('#navigation-secondary + .row');

	// 	var featureZone = document.querySelector('#zone-feature');

	// 	// featureZone.style.marginTop = offsetHeight + 'px';

	// }

	// // Initial setup layout
	// calculateLayout();

	// window.addEventListener("resize", function(){
	// 	// Recalculate layout on window resize
	// 	calculateLayout();
	// });

});
