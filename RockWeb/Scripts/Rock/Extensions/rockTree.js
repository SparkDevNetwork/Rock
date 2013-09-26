(function ($) {
    'use strict';

    // Data container "class" to generically represent data from the server.
    var RockTree = function (element, options) {
            this.$el = $(element);
            this.options = options;
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
		};;

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
                parentNode = _findNodeById(id, this.nodes),
                dfd = $.Deferred(),
                request,
                nodes;

            if (this.options.restUrl) {
                request = $.ajax({
                    url: this.options.restUrl + id,
                    dataType: 'json',
                    contentType: 'application/json'
                });

                request.done(function (data) {
                    try {
                        nodes = self.dataBind(data, parentNode);
                        dfd.resolve(nodes);
                    } catch (e) {
                        dfd.reject(e);
                    }
                });
            } else if (this.options.local) {
                try {
                    nodes = this.dataBind(this.options.local);
                    dfd.resolve(nodes);
                } catch (e) {
                    dfd.reject(e);
                }
            } else {
                dfd.reject('No server endpoint or local data configured!');
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

            this.$el.trigger('dataBound');
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
						.addClass(node.hasChildren ? 'rock-tree-folder' : '')
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
				        $li.prepend('<i class="rock-tree-icon ' + leafCssClass + '"></i>');
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

            this.$el.trigger('rendered');
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
                    self.$el.trigger('close');
                } else {
                    node.isOpen = true;
                    $icon.removeClass(closedClass).addClass(openClass);
                    
                    if (node.hasChildren && !node.children) {
                        self.showLoading($icon.parent('li'));
                        self.fetch(node.id).done(function () {
                            self.render();
                            self.$el.trigger('open');
                        });
                    } else {
                        $ul.show();
                        self.$el.trigger('open');
                    }
                }
            });

            this.$el.on('click', '.rock-tree-item > span', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $rockTree = $(this).parents('.rock-tree'),
                    $item = $(this),
                    id = $item.parent('li').attr('data-id'),
                    node = _findNodeById(id, self.nodes),
                    selectedNodes = [];

                if (!self.options.multiselect) {
                    $rockTree.find('.selected').removeClass('selected');
                    _clearSelectedNodes(self.nodes);
                }

                node.isSelected = true;
                $item.toggleClass('selected');
                $rockTree.find('.selected').parent('li').each(function (i, li) {
                    var $li = $(li);
                    selectedNodes.push({
                        id: $li.attr('data-id'),
                        name: $li.find('span').text()
                    });
                });

                self.selectedNodes = selectedNodes;
                self.$el.trigger('selected', id);
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
        restUrl: null,
        local: null,
        multiselect: false,
        loadingHtml: '<span class="rock-tree-loading"><i class="icon-refresh icon-spin"></i>Loading...</span>',
        iconClasses: {
            branchOpen: 'icon-folder-open',
            branchClosed: 'icon-folder-close',
            leaf: 'icon-file-alt'
        },
        mapping: {
            include: [],
            mapData: _mapArrayDefault
        }
    };
}(jQuery));