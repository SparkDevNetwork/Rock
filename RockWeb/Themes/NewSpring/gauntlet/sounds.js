document.addEventListener('DOMContentLoaded', function () {

  function play(id) {
      let audio = document.getElementById(id);
      audio.play();
  }

  const buttons = $('.gauntlet .btn');
  buttons.each(function(e){
      let button = buttons[e];

      button.addEventListener('mouseenter', (e) => {
          play('audioDown');
      });

      button.addEventListener('mouseleave', (e) => {
          play('audioUp');
      });
  })

}, false);
