document.addEventListener("DOMContentLoaded", function() {
	var appendQueryInstances = document.querySelectorAll("[data-append-query]");

	for (var i = 0; i < appendQueryInstances.length; i++) {

		if (appendQueryInstances[i].tagName == 'INPUT') {
			// Handle as form

			// Get parent form of input
			var form = appendQueryInstances[i].form;

			// Bind to form submit
			form.addEventListener("submit", function(e){
		        e.preventDefault();
		        var destination = e.target.querySelector("[data-append-query]").dataset.appendQuery;
		        var queryValue = e.target.querySelector("[data-append-query]").value;
		        var url = destination + encodeURIComponent(queryValue);
		        window.location.href = url;
		    });
			

		} else {
			// Handle as link

			// Bind to link click
			appendQueryInstances[i].addEventListener("click", function(e){
		        e.preventDefault();
		        var destination = e.target.href;
		        var queryValue = e.target.dataset.appendQuery;
		        var url = destination + '?' + queryValue;
		        window.location.href = url;
		    });

		}
	}
});