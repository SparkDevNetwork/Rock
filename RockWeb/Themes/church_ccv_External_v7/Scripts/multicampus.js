
//
// MULTI CAMPUS (Like Christmas and Easter pages)
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "Vendor/mobile-detect.js"


// Methods & Helpers
// -------------------------

function setActiveCampus(campusSC, saveCampus) {

  $('.multicampus-item.is-active').removeClass('is-active is-open')
    .find('.multicampus-collapse').addClass('collapse').removeClass('in')

  $('[data-campus='+campusSC+']').addClass('is-active is-open')
    .insertBefore('.multicampus-item:first').find('.multicampus-collapse')
    .addClass('in')

  if (saveCampus) {
    var campusId = findIdByShortCode(campusSC)
    Rock.utility.setContext('campuses', campusId)
  }
}
function findIdByShortCode(campusSC) {
  return CCV.locations.filter(function (campus) { return campus.shortcode == campusSC })[0].id
}
function findShortCodeById(campusId) {
  return CCV.locations.filter(function (campus) { return campus.id == campusId })[0].shortcode
}


// General Page & UI Setup
// -------------------------

// Only show text invite on mobile
// Only show facebook invite on computers
var md = new MobileDetect(window.navigator.userAgent)
if (md.mobile()) {
  $('.js-sms').removeClass('hide')
} else {
  $('.js-fb').removeClass('hide')
}

// Bind UI
$('body').on('click', '.multicampus .multicampus-title', function(){
  var $this = $(this)
  $this.siblings('.multicampus-collapse').collapse('toggle')
  $this.parents('.multicampus-item').toggleClass('is-open')
})

// Invite google analytics tracking
$('body').on('click', '.js-track-sms', function(){
  var label = $(this).data('label')
  trackEvent('Text Invite', label)
})
$('body').on('click', '.js-track-email', function(){
  var label = $(this).data('label')
  trackEvent('Email Invite', label)
})
$('body').on('click', '.js-track-fb', function(){
  var label = $(this).data('label')
  trackEvent('Facebook Invite', label)
})


// Find Nearest Campus
// -------------------------

if(navigator.geolocation) {
  $('.js-find-nearest-container').html('<button type="button" class="btn btn-default find-nearest" onClick="findNearestCampus(this)"><i class="loading-icon"></i> Find Nearest Campus</button>')
}

function findNearestCampus(trigger) {
  var $trigger = $(trigger)
  var service = new google.maps.DistanceMatrixService()

  function parseResults(response, status) {
    if (status === google.maps.DistanceMatrixStatus.OK) {

      var lowest = Number.POSITIVE_INFINITY,
        lowestArrayIndex,
        tmp,
        responseArray = response.rows[0].elements

      for (var i = 0; i < responseArray.length; i++) {
        tmp = responseArray[i].duration.value
        if (tmp < lowest) {
          lowest = tmp
          lowestArrayIndex = i
        }
      }

      $trigger.removeClass('is-loading')
      setActiveCampus(CCV.locations[lowestArrayIndex].shortcode, true)
      trackEvent('Find Nearest Campus', CCV.locations[lowestArrayIndex].name)
    }
  }

  function allLocationsGeoArray() {
    var r = []
    for (var i = 0; i < CCV.locations.length; i++) {
      var location = CCV.locations[i].geo
      var geo = new google.maps.LatLng(location.lat, location.lng)
      r.push(geo)
    }
    return r
  }

  if(navigator.geolocation) {
    $trigger.addClass('is-loading')
    navigator.geolocation.getCurrentPosition(function(p) {
      var userLocation = new google.maps.LatLng(p.coords.latitude, p.coords.longitude)
      service.getDistanceMatrix(
        {
          origins: [userLocation],
          destinations: allLocationsGeoArray(),
          travelMode: google.maps.TravelMode.DRIVING
        }, parseResults
      )
    })
  }
}
