var $menuTrigger = $('.navbar-subnav .menu-opener');
var $topNav = $('.menu-wrapper');
var $openLevel = $('.js-openLevel');
var $closeLevel = $('.js-closeLevel');
var $closeLevelTop = $('.js-closeLevelTop');
var $navLevel = $('.js-pushNavLevel');
var self = this;
this.$topNav = $topNav;
this.$openLevel = $openLevel;

function openPushNav() {
  this.$topNav.addClass('isOpen');
  $('body').addClass('pushNavIsOpen');
}

function closePushNav() {
  this.$topNav.removeClass('isOpen');
 this.$openLevel.siblings().removeClass('isOpen');
  $('body').removeClass('pushNavIsOpen');
}

this.$menuTrigger.on('click touchstart', function(e) {
  e.preventDefault();
  if ($topNav.hasClass('isOpen')) {
    self.closePushNav();
  } else {
    self.openPushNav();
  }
});

$($openLevel).on('click touchstart', function(){
  $(this).next($navLevel).addClass('isOpen');
});

$($closeLevel).on('click touchstart', function(){
  $(this).closest($navLevel).removeClass('isOpen');
});

$($closeLevelTop).on('click touchstart', function(){
  self.closePushNav();
});

$('.screen').click(function() {
  	closePushNav();
});