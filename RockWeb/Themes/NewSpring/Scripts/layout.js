$(document).ready(function(){

	// Dynamic Block Columns based on data-column attribute
	let columns = $('[data-column]:not([data-column=""');

	for (let i = 0; i < columns.length; i++) {
		let elem = columns[i],
            blockWrapper = elem.closest("[id^=bid_]"),
		    columnClasses = elem.dataset.column.split(" ");

        // If there are classes from the data-column attribute, add a js-col class
        if(columnClasses.length>0){
		    columnClasses.push('js-col');
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

    for (let i = 0; i < jscolumns.length; i++) {
        let elem = jscolumns[i],
            prevElem = elem.previousSibling,
            nextElem = elem.nextSibling,
            parentDiv = elem.parentNode;

        // If first iteration or prev element doesn't have js-col, add open row tag
        if(i == 0 || !prevElem.classList.contains('js-col')) {
            // Create new row element
            newRow = document.createElement("div");
            // Assign classes
            newRow.classList.add('js-row','row','row-no-gutters');
            // Insert it before current column
            parentDiv.insertBefore(newRow, elem);
            // newRow.appendChild(elem);
        }

        // Push current column element into array to be dumped into new row
        elemsToMove.push(elem);

        // If last js col, or next item isn't a js column, dump elemsToMove into newrow, and clear it out
        if(i == jscolumns.length || nextElem == null || !nextElem.classList.contains('js-col')) {

            // loop through elemsToMove and dump them into row element
            for (let j = 0; j < elemsToMove.length; j++) {
                // console.log(elemsToMove[j]);
                newRow.appendChild(elemsToMove[j]);
            };

            // Create new container element
            newContainer = document.createElement("div");
            // Assign classes
            newContainer.classList.add('js-container','container-fluid','soft-sides','xs-soft-half-sides');
            // Insert it before new row
            newRow.parentNode.insertBefore(newContainer, newRow);
            // Add new row as child of container
            newContainer.appendChild(newRow);

            // clear out elemsToMove to be used again
            elemsToMove = [];
        }

    }

	var c,
		currentScrollTop = 0,
		body = $('body'),
		navWrapper = document.querySelector('#navigation-wrapper');
		windowHeight = $(window).height(),
		heroHeight = $('.hero').height();
		navHeight = navWrapper.offsetHeight
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
				body.removeClass('nav-transparent');
			} else {
				// remove white background
				body.addClass('nav-transparent');
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
