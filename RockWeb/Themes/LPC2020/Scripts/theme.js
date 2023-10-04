function BindNavEvents() {

	$(document).ready(function() {

		$(".et_pb_scroll_top").length && 
		( $(window).scroll(function() {
			$(this).scrollTop() > 800 ? $(".et_pb_scroll_top").show().removeClass("et-hidden").addClass("et-visible") : $(".et_pb_scroll_top").removeClass("et-visible").addClass("et-hidden")
			}),
			$(".et_pb_scroll_top").click(function() {
				$("html, body").animate({
					scrollTop: 0
				}, 800)
			})
		)

		$('a.js-site-search').click( function() {
			var $search = $('div.search-panel');
			var searchWasVisible = $search.is(':visible');
			$search.fadeToggle();
			var scroll = $(window).scrollTop();
			$(".navbar-fixed-top").toggleClass("fixed", scroll > 100 || !searchWasVisible );
			if ( !searchWasVisible ) {
				$('input#site-search').focus();
			}
			return false;
		});
		
		$(document).on('keypress','input#site-search', function(e) {
			if ( e.which == 13 ) {
				var href = '/search?Q=' + $(this).val();
				window.location.href = href;
				return false;
			}
		});
/*
		var prevScroll = $(window).scrollTop();
		($(window).scroll(function() {
			var currScroll = $(window).scrollTop();
			var navbarToggleVisible = $('.navbar-toggle').is(':visible');
			var searchVisible = $('div.search-panel').is(':visible');
			//console.log(navbarToggleVisible + ":" + currScroll);
			if ( currScroll < 100 || searchVisible || navbarToggleVisible ) {
				$(".navbar-fixed-top").removeClass("fixed");
			} else {
				$(".navbar-fixed-top").addClass("fixed");
			}
			
			
			if ( prevScroll > currScroll || currScroll < 50 ) {
				$('#fixed').fadeIn();
			}
			else {
				$('#fixed').fadeOut();
			}
			
			prevScroll = currScroll;
		}));
		
	
		(function() {
			var timeout = null, 
			fixed = $('#fixed'),
			win=$(window),
			prevScroll = win.scrollTop(),
			check_scroll = function() {
				var scroll = win.scrollTop();
				var searchVisible = $('div.search-panel').is(':visible');
				var navbarToggleVisible = $('.navbar-toggle').is(':visible');
				alert ( navbarToggleVisible );
				if ( ( scroll > 100 || searchVisible ) && !navbarToggleVisible ) {
					fixed.addClass("fixed");
				} else {
					fixed.removeClass("fixed");
				}
				timeout=null;
			}
			
			win.scroll(function() {
				if (timeout) {
					// only do once every 50ms
					return;
				}
				timeout = setTimeout(check_scroll, 50);
			});
	   
		})
	*/
	

	/*
	$("ul.navbar-nav li.dropdown").mouseenter( function() {
		$(this).find("ul.dropdown-menu").show();
	});

	$("ul.navbar-nav li.dropdown").mouseleave( function() {
		$(this).find("ul.dropdown-menu").hide();
	});
	*/
	
	});
}
