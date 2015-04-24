$(document).ready(function(){
  if ($('.block-instance.welcome').length) {
    $('.logo').removeClass('hide')
    $one = $('.js-bg-1')
    $two = $('.js-bg-2')
    switchBg()
  }
})

var $one,
    $two,
    currentBg = 0,
    delay = 3000,
    bgs = 8,
    lastBg = 5

function randomIntFromInterval(min,max) {
  return Math.floor(Math.random()*(max-min+1)+min)
}

function randBg() {
  var bg = randomIntFromInterval(1,bgs)
  function newBg() {
    bg = randomIntFromInterval(1,bgs)
    return checkBg()
  }
  function checkBg() {
    if (bg != lastBg) {
      lastBg = bg
      return "/Themes/church_ccv_checkin_kids/Assets/Images/" + bg + ".jpg"
    }
    else
      return newBg()
  }
  return checkBg()
}

function switchBg() {
  switch (currentBg) {
    case 1:
      $two.css('background-image', 'url('+randBg()+')').ready(function(){
        $one.removeClass('in')
        setTimeout(switchBg, delay*1.5)
      })
      currentBg = 2
      break;
    case 2:
      $one.css('background-image', 'url('+randBg()+')').ready(function(){
        $one.addClass('in')
        setTimeout(switchBg, delay*1.5)
      })
      currentBg = 1
      break;
    default:
      // $one.css('background-image', 'url('+randBg()+')').ready(function(){
        $one.addClass('in')
        setTimeout(function(){$two.addClass('in')}, delay)
        setTimeout(switchBg, delay*1.5)
      // })
      currentBg = 1
  }
}