

function geoDistance(lat1, lng1, lat2, lng2, unit) {
  var radlat1 = Math.PI * lat1/180
  var radlat2 = Math.PI * lat2/180
  var theta = lng1-lng2
  var radtheta = Math.PI * theta/180
  var dist = Math.sin(radlat1) * Math.sin(radlat2) + Math.cos(radlat1) * Math.cos(radlat2) * Math.cos(radtheta);
  dist = Math.acos(dist)
  dist = dist * 180/Math.PI
  dist = dist * 60 * 1.1515
  if (unit=="K") { dist = dist * 1.609344 }
  if (unit=="N") { dist = dist * 0.8684 }
  return dist
}

function findNearestFromQuery(current, locations) {
  var lowest = Number.POSITIVE_INFINITY
  var nearest = {}
  for (var i = 0; i < locations.length; i++) {
    var location = locations[i]
    var distance = geoDistance(current.latitude, current.longitude, location.latitude, location.longitude)
    if (distance < lowest) {
      lowest = distance
      nearest = location
    }
  }
  return nearest
}

function setFundTo(fundId) {
  if (giveForm.fund == '')
    giveForm.fund = fundId
}

function useNearestLocation(data) {
  var nearest = findNearestFromQuery(data, campusFundLocations)
  setFundTo(nearest.account.Id)
}

function setFundToNearestLocation() {
  $.getJSON('https://freegeoip.net/json/', function (data) {
    // If freegeoip cannot properly geocode, then city will be blank.
    if (data.city != "")
      useNearestLocation(data);
  })
}

function setFundToCurrentCampus() {
  var fund = givingFunds.filter(function(fund) { return fund.campus == currentCampusId })[0]
  setFundTo(fund.value)
}

if (currentCampusId) {
  setFundToCurrentCampus()
} else {
  setFundToNearestLocation()
}
