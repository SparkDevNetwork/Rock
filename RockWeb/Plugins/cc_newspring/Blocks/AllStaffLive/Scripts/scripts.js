var AllStaffLive = function () {

     var loadStyles = function () {
         var relPath = '../plugins/cc_newspring/Blocks/AllStaffLive/Styles/styles.css';
         var styleLink = $('<link>').attr('rel', 'stylesheet').attr('href', relPath);
         $('head').append(styleLink);
     };

     var startLivePlayer = function () {
         "use strict";

         $("#main-content").addClass("live-feed-playing");
         $(".audio_player").hide();
         $(".video_player").hide();
         $(".live_player").fadeIn();

         switch (localIP) {
             case false:
                 live_player.Player.create("live_feed", "ZjeTJwajryMI8LMaVC3hFYS1xs3z3TA8");
                 break;
             case true:
                 var $script1,
                     $script2,
                     $key;

                 $(".live_player_wrapper").hide();

                 $key = $("<script />").html("jwplayer.key='mwCKA03G7aslUzJIhA70xg2O95qY3xcq9CXBpOiPMV0='").appendTo("body");

                 jwplayer("live_jw_player").setup({
                     width: "100%",
                     aspectratio: "16:9",
                     flashplayer: "//s3.amazonaws.com/staff.newspring.cc/jwplayer/jwplayer.flash.swf",
                     sources: [{
                         file: "rtmp://cen-wowza001.ad.newspring.cc:1935/live/mp4:townhall"
                     }]
                 });
                 break;
         }
     };

     return {
         init: function () {
             loadStyles();
             startLivePlayer();
         }
     };
}();


$(document).ready(AllStaffLive.init);