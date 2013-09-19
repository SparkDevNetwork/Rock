(function ($) {
    'use strict';

    // Data container "class" to generically represent data from the server.
    var RockTree = function (element, options) {
            this.el = element;
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
		};

    RockTree.prototype = {
        constructor: RockTree,
        init: function () {
            var promise = this.fetch(this.options.id),
				self = this;

            this.showLoading(this.el);
            promise.done(function () {
                self.render();
                self.discardLoading(self.el);
                self.initTreeEvents();
            });

            promise.fail(function (msg) {
                self.renderError(msg);
                self.discardLoading(self.el);
                self.initErrorEvents();
            });
        },
        fetch: function (id) {
            var self = this,
				parentNode = _findNodeById(id, this.nodes),
				dfd = $.Deferred(),
				request,
				nodes;

            if (this.options.remote) {
                request = $.ajax({
                    url: Rock.settings.get('baseUrl') + '/' + this.options.remote + '/' + id,
                    dataType: 'json',
                    contentType: 'application/json'
                });

                request.done(function (data) {
                    try {
                        nodes = self.dataBind(data, parentNode);
                        dfd.resolve(nodes);
                    } catch (er) {
                        dfd.reject(er);
                    }
                });
            } else if (this.options.local) {
                try {
                    nodes = this.dataBind(this.options.local);
                    dfd.resolve(nodes);
                } catch (ex) {
                    dfd.reject(ex);
                }
            } else {
                dfd.reject('No server endpoint or local data configured!');
            }

            return dfd.promise();
        },
        dataBind: function (data, parentNode) {
            var self = this,
				nodeArray,
				mapArray = function (arr) {
				    var items,
						nodes = [],
						i;

				    // If there is a custom mapping function defined, defer to its logic...
				    if (typeof self.options.mapping.mapData === 'function') {
				        items = self.options.mapping.mapData(arr);

				        for (i = 0; i < items.length; i++) {
				            nodes.push(items[i]);
				        }

				        return nodes;
				    }

				    // Otherwise, attempt to make best guesses at given the data structure.
				    return $.map(arr, function (item) {
				        var node = {
				            id: item.Id,
				            name: item.Name || item.Title,
				            parentId: item.ParentId,
				            hasChildren: item.HasChildren
				        };

				        if (item.Children && typeof item.Children.length === 'number') {
				            node.children = mapArray(item.Children);
				        }

				        return node;
				    });
				};

            if (!data) {
                throw 'Unable to load data!';
            }

            nodeArray = mapArray(data);

            if (parentNode) {
                parentNode.chidren = nodeArray;
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
						iconClassName = node.hasChildren ? self.options.iconClasses.branchClosed : self.options.iconClasses.leaf,
						includeAttrs = self.options.mapping.include;

				    $li.addClass('rock-tree-item')
						.addClass(node.hasChildren ? 'rock-tree-folder' : '')
						.attr('data-id', node.id)
						.attr('data-parent-id', node.parentId);

				    // Include any configured custom data-* attributes to be decorated on the <li>
				    for (var i = 0; i < includeAttrs.length; i++) {
				        $li.attr('data-' + includeAttrs[i], node[includeAttrs[i]]);
				    }

				    $li.append('<i class="rock-tree-icon ' + iconClassName + '"></i>');
				    $li.append('<span class="rock-tree-name"> ' + node.name + '</span>');
				    $list.append($li);

				    if (node.hasChildren && node.children) {
				        $childUl = $('<ul/>');
				        $childUl.addClass('rock-tree-children');
				        $childUl.hide();
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
        showLoading: function (element) {
            $(element).append(this.options.loadingHtml);
        },
        discardLoading: function (element) {
            $(element).find('.rock-tree-loading').remove();
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

                if ($icon.hasClass(openClass)) {
                    $icon.removeClass(openClass).addClass(closedClass);
                    $ul.hide();
                } else {
                    $icon.removeClass(closedClass).addClass(openClass);

                    // If the node has children, but they've not been fetched from the server yet...
                    if (node.hasChldren && !node.children) {
                        self.showLoading($icon.parent('li'));
                        self.fetch(node.id).done(function () {
                            self.render();
                            self.discardLoading();
                            $ul = $icon.siblings('ul');
                            $ul.show();
                        });
                    } else {
                        $ul.show();
                    }
                }
            });

            this.$el.on('click', '.rock-tree-item > span', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $rockTree = $(this).parents('.rock-tree'),
					$item = $(this),
					id = $item.parent('li').attr('data-id'),
					selectedIds = [];

                if (!self.options.multiselect) {
                    $rockTree.find('.selected').removeClass('selected');
                }

                $item.toggleClass('selected');
                $rockTree.find('.selected').parent('li').each(function (i, li) {
                    selectedIds.push($(li).attr('data-id'));
                });

                self.selectedIds = selectedIds;
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
        var settings = $.extend(true, $.fn.rockTree.defaults, options);

        return this.each(function () {
            var $el = $(this),
				data = $el.data('rockTree'),
				rockTree;

            if (!data) {
                rockTree = new RockTree(this, settings);
            } else {
                rockTree = data;
            }

            $el.data('rockTree', rockTree);
            rockTree.init();
        });
    };

    $.fn.rockTree.defaults = {
        remote: '',
        local: null,
        id: 0,
        multiselect: false,
        loadingHtml: '<span class="rock-tree-loading"><i class="icon-refresh icon-spin"></i>Loading...</span>',
        iconClasses: {
            branchOpen: 'icon-folder-open',
            branchClosed: 'icon-folder-close',
            leaf: 'icon-file-alt'
        },
        mapping: {
            include: [],
            mapData: null
        }
    };
}(jQuery));