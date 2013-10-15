(function ($) {
    'use strict';
    var RockTree = function (element, options) {
            this.$el = $(element);
            this.options = options;
            this.events = $({});
        },
		_findNodeById = function (id, array) {
		    var currentNode,
				node;

		    if (!array || typeof array.length !== 'number') {
		        return null;
		    }

		    for (var i = 0; i < array.length; i++) {
		        currentNode = array[i];

		        if (currentNode.id.toString() === id.toString()) {
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
        _clearSelectedNodes = function (array) {
            var currentNode,
                i;
            
            if (!array || typeof array.length !== 'number') {
                return;
            }

            for (i = 0; i < array.length; i++) {
                currentNode = array[i];
                currentNode.isSelected = false;
                
                if (currentNode.hasChildren && currentNode.children) {
                    _clearSelectedNodes(currentNode.children);
                }
            }
        },
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

    RockTree.prototype = {
        constructor: RockTree,
        init: function () {
            var promise = this.fetch(this.options.id),
				self = this;

            this.showLoading(this.$el);
            promise.done(function () {
                if (self.options.selectedIds && typeof self.options.selectedIds.length === 'number') {
                    self.clear();
                    self.setSelected(self.options.selectedIds);
                }

                self.render();
                self.discardLoading(self.$el);
                self.initTreeEvents();
            });

            promise.fail(function (msg) {
                self.renderError(msg);
                self.discardLoading(self.$el);
                self.initErrorEvents();
            });
        },
        fetch: function (id) {
            var self = this,
                startingNode = _findNodeById(id, this.nodes),
                dfd = $.Deferred(),
                toExpand = [],
                inProgress = {},
                onProgressNotification = function () {
                    var numberInQueue = Object.keys(inProgress).length;

                    if (toExpand.length === 0 && numberInQueue === 0 && dfd.state() !== 'resolved') {
                        dfd.resolve();
                    }
                },
                getNodes = function (parentId, parentNode) {
                    var restUrl = self.options.restUrl + parentId;

                    if (self.options.restParams) {
                        restUrl += '/' + self.options.restParams;
                    }
                    
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
                        });
                };

            if (this.options.restUrl) {
                if (this.options.expandedIds && typeof this.options.expandedIds.length === 'number') {
                    toExpand = this.options.expandedIds;

                    dfd.progress(onProgressNotification);

                    this.events.on('nodes:dataBound', function () {
                        // Pop the top item off the "stack" to de-queue it...
                        var currentId = toExpand.shift(),
                            currentNode;

                        if (!currentId) {
                            return;
                        }

                        currentNode = _findNodeById(currentId, self.nodes);

                        if (!currentNode) {
                            return;
                        }

                        currentNode.isOpen = true;
                        inProgress[currentId] = currentId;
                        getNodes(currentId, currentNode).done(function () {
                            delete inProgress[currentId];
                            dfd.notify();
                        });
                    });
                }

                this.events.on('nodes:dataBound', onProgressNotification);

                getNodes(id, startingNode);
            } else if (this.options.local) {
                try {
                    this.dataBind(this.options.local);
                    dfd.resolve();
                } catch (e) {
                    dfd.reject(e);
                }
            } else {
                this.nodes = _mapFromHtml(this.$el, this.options.mapping.include);;
                dfd.resolve();
            }

            return dfd.promise();
        },
        dataBind: function (data, parentNode) {
            var nodeArray,
                i;

            if (!data || typeof this.options.mapping.mapData !== 'function') {
                throw 'Unable to load data!';
            }

            nodeArray = this.options.mapping.mapData(data);
            
            for (i = 0; i < nodeArray.length; i++) {
                nodeArray[i].isOpen = false;
                nodeArray[i].isSelected = false;
            }

            // If a parent node is supplied, append the result set to the parent node.
            if (parentNode) {
                parentNode.children = nodeArray;
            // Otherwise the result set would be the root array.
            } else {
                this.nodes = nodeArray;
            }

            this.events.trigger('nodes:dataBound');
            this.$el.trigger('rockTree:dataBound');
            return nodeArray;
        },
        render: function () {
            var self = this,
				$ul = $('<ul/>'),
				renderNode = function ($list, node) {
				    var $li = $('<li/>'),
						$childUl,
						includeAttrs = self.options.mapping.include,
				        folderCssClass = node.isOpen ? self.options.iconClasses.branchOpen : self.options.iconClasses.branchClosed,
				        leafCssClass = node.iconCssClass || self.options.iconClasses.leaf;

				    $li.addClass('rock-tree-item')
						.addClass(node.hasChildren ? 'rock-tree-folder' : 'rock-tree-leaf')
						.attr('data-id', node.id)
						.attr('data-parent-id', node.parentId);

				    // Include any configured custom data-* attributes to be decorated on the <li>
				    for (var i = 0; i < includeAttrs.length; i++) {
				        $li.attr('data-' + includeAttrs[i], node[includeAttrs[i]]);
				    }

				    $li.append('<span class="rock-tree-name"> ' + node.name + '</span>');
				    
                    if (node.isSelected) {
                        $li.find('.rock-tree-name').addClass('selected');
                    }

				    if (node.hasChildren) {
				        $li.prepend('<i class="rock-tree-icon ' + folderCssClass + '"></i>');

				        if (node.iconCssClass) {
				            $li.find('.rock-tree-name').prepend('<i class="' + node.iconCssClass + '"></i>');
				        }
				    } else {
				        if (leafCssClass) {
				            $li.find('.rock-tree-name').prepend('<i class="' + leafCssClass + '"></i>');
				        }
				    }

				    $list.append($li);

				    if (node.hasChildren && node.children) {
				        $childUl = $('<ul/>');
				        $childUl.addClass('rock-tree-children');

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
            $ul.addClass('rock-tree');
            this.$el.append($ul);

            $.each(this.nodes, function (idex, node) {
                renderNode($ul, node);
            });

            this.$el.trigger('rockTree:rendered');
        },
        renderError: function (msg) {
            var $warning = $('<div class="alert alert-warning"/>').append('<p/>');
            $warning.find('p')
                .append('<strong><i class="icon-warning-sign"></i> Uh oh! </strong>')
                .append(msg);
            this.$el.html($warning);
        },
        showLoading: function ($element) {
            $element.append(this.options.loadingHtml);
        },
        discardLoading: function ($element) {
            $element.find('.rock-tree-loading').remove();
        },
        clear: function () {
            this.selectedNodes = [];
            _clearSelectedNodes(this.nodes);
            this.render();
        },
        setSelected: function (array) {
            var currentNode,
                i;

            for (i = 0; i < array.length; i++) {
                currentNode = _findNodeById(array[i], this.nodes);

                if (currentNode) {
                    currentNode.isSelected = true;
                }
            }
        },
        initTreeEvents: function () {
            var self = this;

            // Expanding or collapsing a node...
            this.$el.on('click', '.rock-tree-folder > .rock-tree-icon', function (e) {
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
                    self.$el.trigger('rockTree:close');
                } else {
                    node.isOpen = true;
                    $icon.removeClass(closedClass).addClass(openClass);
                    
                    if (node.hasChildren && !node.children) {
                        self.showLoading($icon.parent('li'));
                        self.fetch(node.id).done(function () {
                            self.render();
                            self.$el.trigger('rockTree:open');
                        });
                    } else {
                        $ul.show();
                        self.$el.trigger('rockTree:open');
                    }
                }
            });

            // Selecting a node...
            this.$el.on('click', '.rock-tree-item > span', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $rockTree = $(this).parents('.rock-tree'),
                    $item = $(this),
                    id = $item.parent('li').attr('data-id'),
                    node = _findNodeById(id, self.nodes),
                    selectedNodes = [],
                    onSelected = self.options.onSelected,
                    i;

                if (!self.options.multiselect) {
                    $rockTree.find('.selected').removeClass('selected');
                    _clearSelectedNodes(self.nodes);
                }

                node.isSelected = true;
                $item.toggleClass('selected');
                $rockTree.find('.selected').parent('li').each(function (idx, li) {
                    var $li = $(li);
                    selectedNodes.push({
                        id: $li.attr('data-id'),
                        name: $li.find('span').text()
                    });
                });

                self.selectedNodes = selectedNodes;
                self.$el.trigger('rockTree:selected', id);
                
                if (!onSelected || typeof onSelected.length !== 'number') {
                    return;
                }

                for (i = 0; i < onSelected.length; i++) {
                    $(document).trigger(onSelected[i], id);
                }
            });
        },
        initErrorEvents: function () {
            var self = this;

            this.$el.on('click', '.rock-tree-reset', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.init();
            });
        }
    };

    $.fn.rockTree = function (options) {
        var settings = $.extend(true, {}, $.fn.rockTree.defaults, options);

        return this.each(function () {
            var $el = $(this),
				data = $el.data('rockTree'),
				rockTree = data ? data : new RockTree(this, settings);

            $el.data('rockTree', rockTree);
            rockTree.init();
        });
    };

    $.fn.rockTree.defaults = {
        id: 0,
        selectedIds: null,
        expandedIds: null,
        restUrl: null,
        local: null,
        multiselect: false,
        loadingHtml: '<span class="rock-tree-loading"><i class="icon-refresh icon-spin"></i></span>',
        iconClasses: {
            branchOpen: 'icon-folder-open',
            branchClosed: 'icon-folder-close',
            leaf: ''
        },
        mapping: {
            include: [],
            mapData: _mapArrayDefault
        },
        onSelected: []
    };
}(jQuery));