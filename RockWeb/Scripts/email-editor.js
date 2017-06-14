var initJquery = function ()
{
  loadScript("./Scripts/dragula.min.js", initEditor);

  // wire up the clicking on components to call the 
  // parent window to handle the properties editing
  $(document).on('click', '.component', function (e)
  {
    if ($(this).data('state') == 'component') {
      handleComponentClick(this, e);
    }
  });
};

var handleComponentClick = function (el, e)
{
  e.preventDefault(); // prevent things like links from taking you off the page
  var componentType = "";

  var classNames = $(el).attr("class").toString().split(' ');
  $.each(classNames, function (i, className)
  {
    if (className.indexOf('component-') == 0) {
      componentParts = className.split('-');
      componentType = componentParts[1];
    }
  });

  console.log(componentType);

  // add an id to the element if one does not already exist
  if (!$(el).attr("id")) {
    $(el).attr("id", newGuid());
  }

  // clear the class noting that a component is selected
  $('.component.selected').removeClass('selected');

  // add selected class to this component
  $(el).addClass('selected');

  parent.loadPropertiesPage(componentType, $(el).attr("id"));
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
    if ($(el).data('state') == 'component') {
      handleComponentClick(el, event);
    }
    $('body').addClass('state-drag');
  })
  .on('dragend', function (el)
  {
    $('body').removeClass('state-drag');
  })
  .on('drop', function (el)
  {
    if ($(el).data('state') == 'template') {
      // replace the template contents
      el.innerHTML = $(el).data('content');
      $(el).attr('data-state', 'component');
      el.removeAttribute("data-content");
    }
  });

  // add autoscroll capabilities during dragging
  $(window).mousemove(function (e)
  {
    if (drake.dragging) {
      var $iframeEl = $(window.frameElement);
      var $parentWindow =$(this.parent.window);
      var clientY = e.clientY + $iframeEl.offset().top;
      var parentWindowHeight = $parentWindow.height();
      var scrollLevel = $parentWindow.scrollTop();
      var mousePositionProportion = (clientY - scrollLevel) / parentWindowHeight;

      if (mousePositionProportion > .90 ) {
        scrollLevel += 20;
        $parentWindow.scrollTop(scrollLevel);
      }
      else if (mousePositionProportion < .10 && scrollLevel != 0) {
        scrollLevel -= 20;
        $parentWindow.scrollTop(scrollLevel);
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