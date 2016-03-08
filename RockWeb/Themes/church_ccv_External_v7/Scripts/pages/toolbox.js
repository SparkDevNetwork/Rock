
//
// TOOLBOX PAGES (Group, Next Step, etc.)
// --------------------------------------------------


// Import
// -------------------------

// @prepros-prepend "../Vendor/handlebars-v4.0.5.js"
// @prepros-prepend "../Vendor/moment.js"
// @prepros-prepend "../_map.js"

  window.CCV = window.CCV || {}

// Popovers
// -------------------------

$(function () {
  $('[data-toggle="popover"]').popover({
    content: function() {
      return $(this).find('.js-popup-content').html()
    },
    html: true
  })
})


// Badges
// -------------------------

// Handlebars Stuff

//  mostly based on https://gist.github.com/elidupuis/1468937
//  format an ISO date using Moment.js
//  http://momentjs.com/
//  moment syntax example: moment(Date("2011-07-18T15:50:52")).format("MMMM YYYY")
//  usage: {{dateFormat creation_date format="MMMM YYYY"}}
Handlebars.registerHelper('dateFormat', function(context, block) {
  if (window.moment) {
    var f = block.hash.format || "MMM Do, YYYY"
    return moment(context).format(f)
  } else {
    return context   //  moment plugin not available. return data as is.
  }
})

Handlebars.registerHelper('status', function(conditional, options) {
  if(conditional === options.hash.is) {
    return options.fn(this)
  } else {
    return options.inverse(this)
  }
})

// Badge specific

CCV.badge = CCV.badge || {}

CCV.badge.source = $('#badge-template').html()
CCV.badge.template = Handlebars.compile(CCV.badge.source)

CCV.renderBadges = function(data) {
  for (var key in data) {
    if (data.hasOwnProperty(key)) {
      var person = data[key]
      var $badgeHolder = $('.js-badge-bar-holder[data-id='+key+']')
      $badgeHolder.replaceWith(CCV.badge.template(person))
    }
  }
  $('[data-toggle="tooltip"]').tooltip()
}

Sys.Application.add_load(function(){
  $.getJSON('/api/CCV/Badges/StepsBarGroup/'+CURRENT_GROUP_GUID, function(data){
    CCV.renderBadges(data)
  })
})
