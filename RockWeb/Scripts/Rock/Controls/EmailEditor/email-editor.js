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
        self.editorToolbarContent = $('#'+ options.id).find('.js-editor-toolbar-content')[0];
        self.editorToolbarStructure = $('#' + options.id).find('.js-editor-toolbar-structure')[0];
        self.componentSelected = options.componentSelected;

        // configure and load the dragula script for content components
        self.contentDrake = dragula([self.editorToolbarContent, document.querySelector('.dropzone')], {
          isContainer: function (el)
          {
            return el.classList.contains('dropzone');
          },
          copy: function (el, source)
          {
            return source === self.editorToolbarContent
          },
          accepts: function (el, target)
          {
            return target !== self.editorToolbarContent
          },
          invalid: function (el, handle)
          {
            var isStructureRelated = $(el).closest($(self.editorToolbarStructure)).length // is it from the structures toolbar
              || $(el).closest('.component').hasClass('component-section'); // is it an existing structure component in the email
            return isStructureRelated;
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

        // configure and load the dragula script for structure components
        self.structureDrake = dragula([self.editorToolbarStructure, document.querySelector('.structure-dropzone')], {
          isContainer: function (el)
          {
            return el.classList.contains('structure-dropzone');
          },
          copy: function (el, source)
          {
            return source === self.editorToolbarStructure
          },
          accepts: function (el, target)
          {
            return target !== self.editorToolbarStructure
          },
          invalid: function (el, handle)
          {
            var isStructureRelated = $(el).closest($(self.editorToolbarStructure)).length // is it from the structures toolbar
              || $(el).closest('.component').hasClass('component-section'); // is it an existing structure component in the email
            return !isStructureRelated;
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

        $('.js-editor-content-btn').on('click', function (e)
        {
          $('.js-editor-content-btn').removeClass('btn-default').addClass('btn-primary');
          $('.js-editor-structure-btn').removeClass('btn-primary').addClass('btn-default');
          $('.js-editor-content-toolbar').show();
          $('.js-editor-structure-toolbar').hide();
        });

        $('.js-editor-structure-btn').on('click', function (e)
        {
          $('.js-editor-content-btn').removeClass('btn-primary').addClass('btn-default');
          $('.js-editor-structure-btn').removeClass('btn-default').addClass('btn-primary');
          $('.js-editor-content-toolbar').hide();
          $('.js-editor-structure-toolbar').show();
        });
        

        // add autoscroll capabilities during dragging
        $(window).mousemove(function (e)
        {
          if (self.contentDrake.dragging) {
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
      },
      handleComponentClick: function (el, e)
      {
        var self = this;
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

        if (self.componentSelected) {
          self.componentSelected(componentType, $(el));
        }
      }
    };

    return exports;
  }());
}(jQuery));



