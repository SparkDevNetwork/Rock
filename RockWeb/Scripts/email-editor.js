(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};

  Rock.controls.emailEditor = (function ()
  {
    var exports = {
      initialize: function (options)
      {
        if (!options.id) {
          throw 'id is required';
        }

        var self = this;

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
          if ($(el).attr('data-state') == 'component') {
            self.handleComponentClick(el, event);
          }
          $('body').addClass('state-drag');
        })
        .on('dragend', function (el)
        {
          $('body').removeClass('state-drag');
        })
        .on('drop', function (el)
        {
          if ($(el).attr('data-state') == 'template') {
            // replace the template contents
            el.innerHTML = $(el).attr('data-content');
            $(el).attr('data-state', 'component');
            el.removeAttribute("data-content");
          }
        });

        // add autoscroll capabilities during dragging
        $(window).mousemove(function (e)
        {
          if (drake.dragging) {
            var $iframeEl = $(window.frameElement);
            var $parentWindow = $(this.parent.window);
            var clientY = e.clientY + $iframeEl.offset().top;
            var parentWindowHeight = $parentWindow.height();
            var scrollLevel = $parentWindow.scrollTop();
            var mousePositionProportion = (clientY - scrollLevel) / parentWindowHeight;

            if (mousePositionProportion > .90) {
              scrollLevel += 20;
              $parentWindow.scrollTop(scrollLevel);
            }
            else if (mousePositionProportion < .10 && scrollLevel != 0) {
              scrollLevel -= 20;
              $parentWindow.scrollTop(scrollLevel);
            }
          }
        });

        this.initializeEventHandlers();
      },
      initializeEventHandlers: function ()
      {
        var self = this;
        // wire up the clicking on components to call the 
        // parent window to handle the properties editing
        $(document).on('click', '.component', function (e)
        {
          if ($(this).attr('data-state') == 'component') {
            self.handleComponentClick(this, e);
          }
        });
      },
      handleComponentClick: function (el, e)
      {
        e.preventDefault(); // prevent things like links from taking you off the page

        e.stopImmediatePropagation(); // prevent event from bubbling up to parents
        var componentType = "";

        var classNames = $(el).attr("class").toString().split(' ');
        $.each(classNames, function (i, className)
        {
          if (className.indexOf('component-') == 0) {
            var componentParts = className.split('-');
            componentType = componentParts[1];
          }
        });

        // clear the class noting that a component is selected
        $('.component.selected').removeClass('selected');

        // add selected class to this component
        $(el).addClass('selected');

        parent.loadPropertiesPage(componentType, $(el));
      }
    };

    return exports;
  }());
}(jQuery));

Rock.controls.emailEditor.initialize({ id: 'editor-toolbar' });

