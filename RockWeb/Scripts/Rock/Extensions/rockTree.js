﻿(function ($) {
    'use strict';
    
    // Private RockTree "class" that will represent instances of the node tree in memory
    // and provide all functionality needed by instances of a treeview control.
    var RockTree = function (element, options) {
            this.$el = $(element);
            this.options = options;
            this.selectedNodes = [];
        
            // Create an object-based event aggregator (not DOM-based)
            // for internal eventing
            this.events = $({});
        },
        
        // Generic recursive utility function to find a node in the tree by its id
		_findNodeById = function (id, array) {
		    var currentNode,
				node;

		    if (!array || typeof array.length !== 'number') {
		        return null;
		    }

		    for (var i = 0; i < array.length; i++) {
		        currentNode = array[i];
		        
		        // remove surrounding single quotes from id if they exist
		        var idCompare = id.toString().replace(/(^')|('$)/g, '');
                
		        if (currentNode.id.toString() === idCompare) {
		            return currentNode;
		        } else if (currentNode.hasChildren) {
		            node = _findNodeById(id, currentNode.children || []);

		            if (node) {
		                return node;
		            }
		        }
		    }

		    return null;
		},
        
        // Default utility function to attempt to map a Rock.Web.UI.Controls.Pickers.TreeViewItem
        // to a more standard JS object.
		_mapArrayDefault = function (arr) {
		    return $.map(arr, function (item) {
		        var node = {
		            id: item.Guid || item.Id,
		            name: item.Name || item.Title,
		            iconCssClass: item.IconCssClass,
		            parentId: item.ParentId,
		            hasChildren: item.HasChildren
		        };

		        if (item.Children && typeof item.Children.length === 'number') {
		            node.children = _mapArrayDefault(item.Children);
		        }

		        return node;
		    });
		},
        
        // Utility function that attempts to derive a node tree structure given an HTML element
        _mapFromHtml = function ($el, attrs) {
            var nodes = [],
                $ul = $el.children('ul');

            $ul.children('li').each(function () {
                var $li = $(this),
                    node = {
                        id: $li.attr('data-id'),
                        name: $li.children('span').first().html(),
                        hasChildren: $li.children('ul').length > 0,
                        isOpen: $li.attr('data-expanded') === 'true'
                    };
                
                if (attrs && typeof attrs.length === 'number') {
                    for (var i = 0; i < attrs.length; i++) {
                        node[attrs[i]] = $li.attr('data-' + attrs[i]);
                    }   
                }

                if (node.hasChildren) {
                    node.children = _mapFromHtml($li, attrs);
                }
                
                nodes.push(node);
            });

            return nodes;
        };

    // Prototype declaration for RockTree, holds all new functionality of the tree
    RockTree.prototype = {
        constructor: RockTree,
        init: function () {
            // Load data into tree asynchronously
            var promise = this.fetch(this.options.id),
				self = this;

            this.showLoading(this.$el);
            
            // If Selected Ids is set, pre-select those nodes
            promise.done(function () {
                if (self.options.selectedIds && typeof self.options.selectedIds.length === 'number') {
                    self.clear();
                    self.setSelected(self.options.selectedIds);
                }

                self.render();
                self.discardLoading(self.$el);
                self.initTreeEvents();
            });

            // If attempt to load data fails, display error message
            promise.fail(function (msg) {
                self.renderError(msg);
                self.discardLoading(self.$el);
            });
        },
        fetch: function (id) {
            var self = this,
                startingNode = _findNodeById(id, this.nodes),
                
                // Using a jQuery Deferred to control when this operation will get returned to the caller.
                // Since the fetch operation may span multiple AJAX requests, we need a good way to control
                // how the caller will be notified of completion.
                dfd = $.Deferred(),
                
                // Create a queue of Ids to expand the corresponding nodes
                toExpand = [],
                
                // Create a "queue" or hash of AJAX calls that are currently in progress
                inProgress = {},
                
                // Handler function to determine whether or not the fetch operation is complete.
                onProgressNotification = function () {
                    var numberInQueue = Object.keys(inProgress).length;

                    // If we've drained the queue of all items to prefetch,
                    // and there are no requests in queue currentling being fetched,
                    // and we have not already resolved the deferred, return
                    // control to the caller.
                    if (toExpand.length === 0 && numberInQueue === 0 && dfd.state() !== 'resolved') {
                        dfd.resolve();
                    }
                },
                
                // Wrapper function around jQuery.ajax. Appends a handler to databind the
                // resulting JSON from the server and returns the promise
                getNodes = function (parentId, parentNode) {
                    var restUrl = self.options.restUrl + parentId;

                    if (self.options.restParams) {
                        restUrl += self.options.restParams;
                    }

                    self.clearError();
                    
                    return $.ajax({
                            url: restUrl,
                            dataType: 'json',
                            contentType: 'application/json'
                        })
                        .done(function (data) {
                            try {
                                self.dataBind(data, parentNode);
                            } catch (e) {
                                dfd.reject(e);
                            }
                        })
                        .fail(function (jqXHR, textStatus, errorThrown) {
                            self.renderError(jqXHR.responseJSON ? jqXHR.responseJSON.ExceptionMessage : errorThrown);
                        });
                };

            if (this.options.restUrl) {
                if (this.options.expandedIds && typeof this.options.expandedIds.length === 'number') {
                    toExpand = this.options.expandedIds;

                    // Listen for progress on the Deferred and pass it the handler to
                    // check if we're "done"
                    dfd.progress(onProgressNotification);

                    // Listen to internal databound event
                    this.events.on('nodes:dataBound', function () {
                        // Pop the top item off the "stack" to de-queue it...
                        var currentId = toExpand.shift(),
                            currentNode;

                        if (!currentId) {
                            return;
                        }

                        currentNode = _findNodeById(currentId, self.nodes);
                        while (currentNode == null && toExpand.length > 0) {
                            // if we can't find it, try the next one until we find one or run out of expanded ids
                            currentId = toExpand.shift();
                            currentNode = _findNodeById(currentId, self.nodes);
                        }

                        if (!currentNode) {
                            return;
                        }

                        // If we find the node, make sure it's expanded, and fetch its children
                        currentNode.isOpen = true;
                        
                        // Queue up current node
                        inProgress[currentId] = currentId;
                        getNodes(currentId, currentNode).done(function () {
                            // Dequeue on completion
                            delete inProgress[currentId];
                            // And notify the Deferred of progress
                            dfd.notify();
                        });
                    });
                }

                // When databound, check to see if fetching is complete
                this.events.on('nodes:dataBound', onProgressNotification);
                
                // Get initial node's data
                getNodes(id, startingNode);
            } else if (this.options.local) {
                // Assuming there is local data defined, attempt to databind it
                try {
                    this.dataBind(this.options.local);
                    dfd.resolve();
                } catch (e) {
                    dfd.reject(e);
                }
            } else {
                // Otherwise attempt to databind on HTML of the current element
                this.nodes = _mapFromHtml(this.$el, this.options.mapping.include);;
                dfd.resolve();
            }

            return dfd.promise();
        },
        
        // Attempt to load data returned by `fetch` into the current rockTree's
        // node data structure
        dataBind: function (data, parentNode) {
            var nodeArray,
                i;

            if (!data || typeof this.options.mapping.mapData !== 'function') {
                throw 'Unable to load data!';
            }

            // Call configured `mapData` function. If it wasn't overridden by the user,
            // `_mapArrayDefault` will be called.
            nodeArray = this.options.mapping.mapData(data);
            
            for (i = 0; i < nodeArray.length; i++) {
                nodeArray[i].isOpen = false;
            }

            // If a parent node is supplied, append the result set to the parent node.
            if (parentNode) {
                parentNode.children = nodeArray;
            // Otherwise the result set would be the root array.
            } else {
                this.nodes = nodeArray;
            }

            // Trigger "internal" databound event and trigger "public" databound event
            // via the $el to notify the DOM
            this.events.trigger('nodes:dataBound');
            this.$el.trigger('rockTree:dataBound');
            return nodeArray;
        },
        
        // Recursively render out each node in the DOM via the `$el` property
        render: function () {
            var self = this,
				$ul = $('<ul/>'),
				renderNode = function ($list, node) {
				    var $li = $('<li/>'),
						$childUl,
						includeAttrs = self.options.mapping.include,
				        folderCssClass = node.isOpen ? self.options.iconClasses.branchOpen : self.options.iconClasses.branchClosed,
				        leafCssClass = node.iconCssClass || self.options.iconClasses.leaf;

				    $li.addClass('rocktree-item')
						.addClass(node.hasChildren ? 'rocktree-folder' : 'rocktree-leaf')
						.attr('data-id', node.id)
						.attr('data-parent-id', node.parentId);

				    // Include any configured custom data-* attributes to be decorated on the <li>
				    for (var i = 0; i < includeAttrs.length; i++) {
				        $li.attr('data-' + includeAttrs[i], node[includeAttrs[i]]);
				    }

				    $li.append('<span class="rocktree-name"> ' + node.name + '</span>');
				    
				    for (var i = 0; i < self.selectedNodes.length; i++) {
				        if (self.selectedNodes[i].id == node.id) {
				            $li.find('.rocktree-name').addClass('selected');
				            break;
				        }
				    }

				    if (node.hasChildren) {
				        $li.prepend('<i class="rocktree-icon ' + folderCssClass + '"></i>');

				        if (node.iconCssClass) {
				            $li.find('.rocktree-name').prepend('<i class="' + node.iconCssClass + '"></i>');
				        }
				    } else {
				        if (leafCssClass) {
				            $li.find('.rocktree-name').prepend('<i class="' + leafCssClass + '"></i>');
				        }
				    }

				    $list.append($li);

				    if (node.hasChildren && node.children) {
				        $childUl = $('<ul/>');
				        $childUl.addClass('rocktree-children');

                        if (!node.isOpen) {
                            $childUl.hide();
                        }
				        
				        $li.append($childUl);

				        $.each(node.children, function (index, childNode) {
				            renderNode($childUl, childNode);
				        });
				    }
				};

            // Clear tree and prepare to re-render
            this.$el.empty();
            $ul.addClass('rocktree');
            this.$el.append($ul);

            $.each(this.nodes, function (index, node) {
                renderNode($ul, node);
            });

            this.$el.trigger('rockTree:rendered');
        },

        // clear the error message
        clearError: function () {
            this.$el.siblings('.js-rocktree-alert').remove();
        },
        
        // Render Bootstrap alert displaying the error message.
        renderError: function (msg) {
            this.clearError();
            this.discardLoading(this.$el);
            var $warning = $('<div class="alert alert-danger alert-dismissable js-rocktree-alert"/>');
            $warning.append('<button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>')
                .append('<strong><i class="fa fa-exclamation-triangle"></i> Error </strong>')
                .append(msg);
            $warning.insertBefore(this.$el);
        },
        
        // Show loading spinner
        showLoading: function ($element) {
            $element.append(this.options.loadingHtml);
        },
        
        // Remove loading spinner
        discardLoading: function ($element) {
            $element.find('.rocktree-loading').remove();
        },
        
        // Clears all selected nodes
        clear: function () {
            this.selectedNodes = [];
            this.render();
        },
        
        // Sets selected nodes given an array of ids
        setSelected: function (array) {
            this.selectedNodes = [];

            var currentNode,
                i;

            for (i = 0; i < array.length; i++) {
                currentNode = _findNodeById(array[i], this.nodes);

                if (currentNode) {
                    this.selectedNodes.push(currentNode);

                    // trigger the node as selected
                    this.$el.trigger('rockTree:selected', currentNode.id);
                }
            }
        },
        
        // Wire up DOM events for rockTree instance
        initTreeEvents: function () {
            var self = this;

            // remove event to make sure it doesn't get attached multiple times
            this.$el.off('click');

            // Expanding or collapsing a node...
            this.$el.on('click', '.rocktree-folder > .rocktree-icon', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $icon = $(this),
					$ul = $icon.siblings('ul'),
					id = $icon.parent('li').attr('data-id'),
					node = _findNodeById(id, self.nodes),
					openClass = self.options.iconClasses.branchOpen,
					closedClass = self.options.iconClasses.branchClosed;

                if (node.isOpen) {
                    $ul.hide();
                    node.isOpen = false;
                    $icon.removeClass(openClass).addClass(closedClass);
                    self.$el.trigger('rockTree:collapse');
                } else {
                    node.isOpen = true;
                    $icon.removeClass(closedClass).addClass(openClass);
                    
                    // If the node has children, but they haven't been loaded yet,
                    // attempt to load them first, then re-render
                    if (node.hasChildren && !node.children) {
                        self.showLoading($icon.parent('li'));
                        self.fetch(node.id).done(function () {
                            self.render();
                            self.$el.trigger('rockTree:expand');
                        });
                    } else {
                        $ul.show();
                        self.$el.trigger('rockTree:expand');
                    }
                }
            });

            // Selecting a node...
            this.$el.on('click', '.rocktree-item > span', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $rockTree = $(this).parents('.rocktree'),
                    $item = $(this),
                    id = $item.parent('li').attr('data-id'),
                    node = _findNodeById(id, self.nodes),
                    selectedNodes = [],
                    onSelected = self.options.onSelected,
                    i;

                // If multi-select is disabled, clear all previous selections
                if (!self.options.multiselect) {
                    $rockTree.find('.selected').removeClass('selected');
                }

                $item.toggleClass('selected');
                $rockTree.find('.selected').parent('li').each(function (idx, li) {
                    var $li = $(li);
                    selectedNodes.push({
                        id: $li.attr('data-id'),
                        // get the li text excluding child text
                        name: $li.contents(':not(ul)').text()
                    });
                });

                self.selectedNodes = selectedNodes;
                self.$el.trigger('rockTree:selected', id);
                self.$el.trigger('rockTree:itemClicked', id);
                
                // If there is an array of other events to trigger on select,
                // loop through them and trigger each, passing along the
                // currently selected node's id
                if (!onSelected || typeof onSelected.length !== 'number') {
                    return;
                }

                for (i = 0; i < onSelected.length; i++) {
                    $(document).trigger(onSelected[i], id);
                }
            });
        }
    };

    // jQuery plugin definition
    $.fn.rockTree = function (options) {
        // Make a deep copy of all configuration settings passed in 
        // and merge it with a deep copy of the defaults defined below
        var settings = $.extend(true, {}, $.fn.rockTree.defaults, options);

        // For each element matching the selector, attempt to get an instance
        // of RockTree from $el.data, if not present, create a new instance
        // of RockTree and stash it there, then initialize the tree.
        return this.each(function () {
            var $el = $(this);
            var rockTree = $el.data('rockTree');
            
            if (!rockTree) {
                // create a new rocktree
                rockTree = new RockTree(this, settings);
            }
            else
            {
                // use the existing rocktree but update the settings and clean up selectedNodes
                rockTree.options = settings;
                rockTree.selectedNodes = [];
            }

            $el.data('rockTree', rockTree);
            rockTree.init();
        });
    };

    // Default values to be merged upon initialization of the jQuery plugin
    $.fn.rockTree.defaults = {
        id: 0,
        selectedIds: null,
        expandedIds: null,
        restUrl: null,
        restParams: null,
        local: null,
        multiselect: false,
        loadingHtml: '<span class="rocktree-loading"><i class="fa fa-refresh fa-spin"></i></span>',
        iconClasses: {
            branchOpen: 'fa fa-fw fa-caret-down',
            branchClosed: 'fa fa-fw fa-caret-right',
            leaf: ''
        },
        mapping: {
            include: [],
            mapData: _mapArrayDefault
        },
        onSelected: []
    };
}(jQuery));