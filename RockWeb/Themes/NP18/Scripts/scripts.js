jQuery(function($){

function scrollToAnchor(hash) {
	//window.console.log(hash);
	//window.console.log(hash.slice(1));
	if (document.getElementById(hash.slice(1))) {
    var target = $(hash),
        headerHeight = $("header nav.primary").height() + 5; // Get fixed header height

    target = target.length ? target : $('[name=' + hash.slice(1) +']');

    if (target.length)
    {
        $('html,body').animate({
            scrollTop: target.offset().top - headerHeight
        }, 100);
        return false;
    }
  }
}

if(window.location.hash) {
  scrollToAnchor(window.location.hash);
}


$("a[href*=\\#]:not([href=\\#])").click(function()
{
    if (location.pathname.replace(/^\//,'') == this.pathname.replace(/^\//,'')
        || location.hostname == this.hostname)
    {

        scrollToAnchor(this.hash);
    }
});
  	
	//set class on fixed header after scrolling
	var didScroll = true;
	var lastScrollTop = 0;
	var delta = 5;
	
	$(window).scroll(function(){
	    didScroll = true;
	});
	
	setInterval(function() {
	    if (didScroll) {
	        hasScrolled();
	        didScroll = false;
	    }
	}, 250);
	
	function hasScrolled() {
	    var st = $(this).scrollTop();
			var navbarHeight = $('header nav').outerHeight() + $('header .navbar-subheader').outerHeight();
			var headerImageHeight = $('.headerImage').outerHeight();
			var pageSubnavHeight = $('.subnav').outerHeight();
	    //console.log(st);
	    // Make sure they scroll more than delta
	    if(Math.abs(lastScrollTop - st) <= delta)
	        return;
	    
	    // If they scrolled down and are past the navbar, add class .nav-up.
	    // This is necessary so you never see what is "behind" the navbar.
	    if (st > 10){
	        $('header').addClass('opaque');
	    } else {
	        $('header').removeClass('opaque');
	    }

	    if (st >= (headerImageHeight - navbarHeight)){
	        $('.subnav').addClass('navbar-fixed-top').css('top',navbarHeight+'px');
	        $('.headerImage').css('margin-bottom',pageSubnavHeight+'px');
	        //$('.addthis_toolbox.vertical').addClass('navbar-fixed-top').css({'top':navbarHeight+'px','right':'15%'});
	    } else {
	        $('.subnav').removeClass('navbar-fixed-top').css('top','auto');
	        $('.headerImage').css('margin-bottom',0);
	        //$('.addthis_toolbox.vertical').removeClass('navbar-fixed-top').css({'top':0,'right':0});
	    }
	    
	    lastScrollTop = st;
	}

/**
	$('.navbar-subheader .church-menu-opener').on('click touchstart', function(e){
		e.preventDefault();
		$('#navbar-main').show().addClass('in');
		$('#churches-mega-menu').show().addClass('open');
	});
  $('#navbar-main .menu-closer').on('click touchstart', function(e){
    e.preventDefault();
			$('#navbar-main').hide().removeClass('in');	
  });
	$('#main-menu a').on('click touchstart', function(){
		if ($('#churches-mega-menu').hasClass('open')) {
			$('#churches-mega-menu').hide().removeClass('open');	
			$('#menu_churches, #menu_church').removeClass('open');
		}
		if ($('#search-mega-menu').hasClass('open')) {
			$('#search-mega-menu').hide().removeClass('open');	
		}
	});
	$('#menu_churches a, #menu_church a').on('click touchstart', function(){
		$('.dropdown').removeClass('open');
		$(this).parent().addClass("open");
		$('#churches-mega-menu').show().addClass('open');
	});
	$("#churches-mega-menu").mouseleave(function(){
    $(this).hide();
    $('#menu_churches, #menu_church').removeClass('open');
	});
  $('#churches-mega-menu .menu-closer').on('click touchstart', function(e){
    e.preventDefault();
    $('#churches-mega-menu').hide();
    $('#menu_churches, #menu_church').removeClass('open');
  });
**/
	$('.navbar-subheader').on('click touchstart','.menu-opener', function(e){
		e.preventDefault();
		$('#navbar-main').show().addClass('open');
	});
  $('#navbar-main').on('click','.closer', function(e){
    e.preventDefault();
    $('#navbar-main').hide().removeClass('open');
  });
	$('.navbar-subheader').on('click', '.search-opener', function(e){
		e.preventDefault();
		$('#search-layer').show().addClass('open');
	});
  $('#ipl_search').on('click','a', function(e){
    e.preventDefault();
		$('#search-layer').show().addClass('open');
    $('.searchbox input').focus();
  });
  $('#search-layer').on('click','.closer', function(e){
    e.preventDefault();
    $('#search-layer').hide().removeClass('open');
  });
  $('#navbar-main').on('click','li.has-children > a', function(e){
	  if($('#navbar-main').hasClass('open')) {
		  var childmenu = $(this).next();
		  var backtext = $(this).text();
		  if ($(childmenu).hasClass('child-menu')) {
		    e.preventDefault();
			  $(childmenu).prepend("<li class='backtext'><a>"+backtext+"</a></li>");
	      $(childmenu).show().animate({
	        'left': '0'
	      },175,'swing');
	    }
	  }
  });
  $('#navbar-main').on('click','li.has-children ul.child-menu li a', function(e){
	  if($(this).parents().hasClass('backtext')) {
	    e.preventDefault();
		  var parentmenu = $(this).parents('.child-menu');
      $(parentmenu).animate({
        'left': '100%'
      },175,'swing',function(){
	      $(parentmenu).hide();
	      $('li.backtext').remove();	      
      });
	  }
  });

 /**   
  $('select.sorter, select.filter').select2({
      //allowClear: true,
      minimumResultsForSearch: 20
  });
  $('select.select-church').select2({
      theme: "hollow",
      minimumResultsForSearch: 20
  });
  $('.tier2-item').each(function(){
	  $(this).hover(function(){
		  $(this).find('.select-overlay').css('visibility','visible');
		});
  });
	$('select.sermon-list, select.select-church').on('change', function(){
		var url = $(this).val(); // get selected value
		if (url) { // require a URL
		    window.location = url; // redirect
		}
		return false;
	});
**/
	var windowWidth = $(window).width();
	if (windowWidth >= 992) {
		$('.messages-grid .sermondetail').each(function(){
			var parentContainer = $(this).closest('.sermons-container');
			$(this).appendTo(parentContainer);
		});
	}
	$('.series-link').on('click touchstart', function(e){
		e.preventDefault();
		window.console.log(e);
		$('.sermondetail').hide();
		var seriesblock = $(this).data('openid');
    var container = $(this).closest('.sermon');
		if ($("#"+seriesblock).hasClass('open')) {
			$('.sermon').removeClass('open');		
			$('.sermondetail').removeClass('open');		
		} else {
			$('.sermon').removeClass('open');		
			$('.sermondetail').removeClass('open');		
			$(container).addClass('open');			
			$("#"+seriesblock).addClass('open').slideDown();
			scrollToAnchor("#"+seriesblock);	
		}
	});
  $('.sermondetail .closer').on('click touchstart', function(e){
    e.preventDefault();
    var container = $(this).closest('.sermondetail');
		$('.sermon').removeClass('open');		
    $(container).removeClass('open').hide();
  });
  
	function init() {
	  $('.notification-bar').each(function(){
		  $('body').css({'padding-top':$(this).outerHeight()});
		  $('.navbar-subheader').css({'top':$(this).outerHeight()});	  
	  });
  	if (windowWidth < 992) {
			var navbarHeight = $('header nav').outerHeight() + $('header .navbar-subheader').outerHeight();
			$('body').css('margin-top',navbarHeight);
		} else {
			$('body').css('margin-top',0);
		}
	}

  $(window).resize(function(){
    init();
    didScroll = true;
	});
	init();
});
