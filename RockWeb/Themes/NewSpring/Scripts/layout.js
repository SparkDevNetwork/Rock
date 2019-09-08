document.addEventListener("DOMContentLoaded", function() {
	function calculateLayout() {
		// Get window width to determine if nav will be fixed to top/bottom
		var windowWidth = window.innerWidth;

		var content = document.querySelector('#content');

		var navigation = document.querySelector('#navigation');
		if(navigation != null) {
			 var navigationHeight = navigation.offsetHeight;
		}

		var navigationSecondary = document.querySelector('#navigation-secondary');
		if(navigationSecondary != null) {
			var navigationSecondaryHeight = navigationSecondary.offsetHeight;
		}

		if(windowWidth > 667) {
			// Desktop
			content.style.marginBottom = '0';
			
			if(navigationSecondary) {
				navigationSecondary.style.marginTop = navigationHeight + "px";
				content.style.marginTop = navigationHeight + navigationSecondaryHeight + "px";
			} else {
				content.style.marginTop = navigationHeight + "px";
			}
		} else {
			// Mobile
			content.style.marginBottom = navigationHeight + "px";

			if(navigationSecondary) {
				navigationSecondary.style.marginTop = 0;
				content.style.marginTop = navigationSecondaryHeight + "px";
			} else {
				content.style.marginTop = 0;
			}
		}
		
	}

	// Initial setup layout
	calculateLayout();

	window.addEventListener("resize", function(){
		// Recalculate layout on window resize
		calculateLayout();
	});

});