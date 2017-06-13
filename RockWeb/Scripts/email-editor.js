var initJquery = function ()
{
  loadScript("./Scripts/dragula.min.js", initEditor);

  // wire up the clicking on components to call the 
  // parent window to handle the properties editing
  $(document).on('click', '.component', function (e)
  {
    e.preventDefault(); // prevent things like links from taking you off the page
    var componentType = "";

    var classNames = $(this).attr("class").toString().split(' ');
    $.each(classNames, function (i, className)
    {
      if (className.startsWith('component-')) {
        componentParts = className.split('-');
        componentType = componentParts[1];
      }
    });

    console.log(componentType);

    // add an id to the element if one does not already exist
    if (!$(this).attr("id")) {
      $(this).attr("id", newGuid());
    }

    // clear the class noting that a component is selected
    $('.component.selected').removeClass('selected');

    // add selected class to this component
    $(this).addClass('selected');

    parent.loadPropertiesPage(componentType, $(this).attr("id"));
  });
};

var initEditor = function ()
{

  // configure and load the dragula script
  var drake = dragula([document.getElementById('editor-toolbar'), document.querySelector('.dropzone')], {
    isContainer: function (el)
    {
      return el.classList.contains('dropzone');
    },
    copy: function (el, source)
    {
      return source === document.getElementById('editor-toolbar')
    },
    accepts: function (el, target)
    {
      return target !== document.getElementById('editor-toolbar')
    },
    ignoreInputTextSelection: true
  })
  .on('drag', function (el)
  {
    var currentBodyClass = document.body.className;
    if (!currentBodyClass.includes('state-drag')) {
      document.body.className += ' state-drag';
    }
  })
  .on('dragend', function (el)
  {
    document.body.className = document.body.className.replace("state-drag", "");
  })
  .on('drop', function (el)
  {
    var state = el.getAttribute('data-state');

    if (state == "template") {
      // replace the template contents
      el.innerHTML = el.getAttribute('data-content');
      el.setAttribute('data-state', 'component');
      el.removeAttribute("data-content");
    }
  });

  // add autoscroll capabilities during dragging
  $(window).mousemove(function (e)
  {
    if (drake.dragging) {
      var clientY = e.clientY;
      var iframeHeight = $(this).height();
      var contentHeight = $(document).height();
      var mousePositionProportion = clientY / iframeHeight;
      var scrollLevel = $(window).scrollTop();

      if (mousePositionProportion > .90 && (scrollLevel + iframeHeight) != contentHeight) {
        scrollLevel += 20;
        $(window).scrollTop(scrollLevel);
      }
      else if (mousePositionProportion < .10 && scrollLevel != 0) {
        scrollLevel -= 20;
        $(window).scrollTop(scrollLevel);
      }
    }
  });
};

// load dependencies
// note dragula will be loaded by the jquery loader to ensure both are fully loaded before dragula is initiated
loadScript("./Scripts/jquery-1.12.4.min.js", initJquery);


// -- Helper Functions

function newGuid()
{
  function s4()
  {
    return Math.floor((1 + Math.random()) * 0x10000)
      .toString(16)
      .substring(1);
  }
  return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
    s4() + '-' + s4() + s4() + s4();
}

function loadScript(url, callback)
{
  // Adding the script tag to the head as suggested before
  var head = document.getElementsByTagName('head')[0];
  var script = document.createElement('script');
  script.type = 'text/javascript';
  script.src = url;

  // Then bind the event to the callback function.
  // There are several events for cross browser compatibility.
  script.onreadystatechange = callback;
  script.onload = callback;

  // Fire the loading
  head.appendChild(script);
}