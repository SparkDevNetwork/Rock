(function ($) {
    'use strict';

    // Data container "class" to generically represent data from the server.
    var RockTreeNode = function (options) {
            this.id = options.id;
            this.name = options.name;
            this.parentId = options.parentId;
            this.hasChildren = options.hasChildren;
        },
		RockTree = function (element, options) {
		    this.$el = $(element);
		    this.options = options;
		},
		_findNodeById = function (array, id) {
		    var currentNode,
				node;

		    if (!array || typeof array.length !== 'number') {
		        return null;
		    }

		    for (var i = 0; i < array.length; i++) {
		        currentNode = array[i];

		        // Double equals used deliberately here to avoid needing
		        // to cast between int or string, while also allowing guids
		        // to be used.
		        if (currentNode.id == id || currentNode.Id == id) {
		            return currentNode;
		        } else if (currentNode.hasChildren) {
		            node = _findNodeById(currentNode.children, id);

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

            this.$el.html(this.options.loadingHtml);
            promise.done(function (data) {
                self.render(data, null);
                self.initTreeEvents();
            });
        },
        fetch: function (id) {
            var promise,
				dfd,
				data,
				parentNode;

            if (this.options.remote) {
                promise = $.ajax({
                    url: Rock.settings.get('baseUrl') + '/' + this.options.remote + '/' + id,
                    type: 'GET',
                    dataType: 'json',
                    contentType: 'application/json'
                });
            } else {
                // If the remote has not been set, see if local has. Create a new deferred and return its promise.
                dfd = $.Deferred();
                promise = dfd.promise();

                if (this.options.local) {
                    if (!id) {
                        data = this.options.local;
                    } else {
                        parentNode = _findNodeById(this.options.local, id);
                        data = parentNode ? (parentNode.children || null) : null;
                    }

                    setTimeout(function () { dfd.resolve(data); }, 2000);
                } else {
                    dfd.resolve(null);
                }
            }

            return promise;
        },
        render: function (data, parentNode) {
            var self = this,
				promise = this.populateNodes(data);

            promise.done(function (nodes) {
                // If this is the first time the page is loading, parentNode should be null,
                // otherwise it'll be populated with the id of the parent being "opened"
                if (!parentNode) {
                    self.nodes = nodes;
                } else {
                    parentNode.children = nodes;
                }

                var parentId = parentNode ? parentNode.id : nodes[0].parentId,
					$container = parentId ? self.$el.find('[data-id="' + parentId + '"]') : self.$el,
					$ul = $('<ul/>');
                $container.find('ul').remove();
                $ul.addClass(!parentNode ? 'rock-tree' : 'rock-tree-children'); //.appendTo($container);

                $.each(nodes, function (index, node) {
                    //var $ul = $container.find('ul'),
                    var $li = $('<li/>'),
						iconClassName = node.hasChildren ? 'glyphicon-folder-close' : 'glyphicon-file';

                    $li.addClass('rock-tree-item').addClass(node.hasChildren ? 'rock-tree-folder' : '').data('rockTreeNode', node);
                    $li.attr('data-id', node.id).attr('data-parent-id', node.parentId);
                    $li.append('<i class="glyphicon ' + iconClassName + '"></i>');
                    $li.append('<span class="rock-tree-name"> ' + node.name + '</span>');
                    $ul.append($li);
                });

                $container.append($ul);
                $container.find('.rock-tree-loading').remove();
            });

            promise.fail(function (msg) {
                var $warning = $('<div class="alert alert-warning"/>')
					.append('<p/>');
                $warning.find('p')
                    .append('<strong><i class="glyphicon glyphicon-warning-sign"></i> Uh oh! </strong>')
                    .append(msg);
                self.$el.html($warning);
            });


        },
        populateNodes: function (data) {
            var dfd = $.Deferred(),
				nodeArray = [];

            if (!data) {
                dfd.reject('We weren\'t able to load any data.');
                return dfd.promise();
            }

            $.each(data, function (index, value) {
                nodeArray.push(new RockTreeNode({
                    id: value.Id,
                    name: value.Name || value.Title,
                    parentId: value.ParentId,
                    hasChildren: value.HasChildren
                }));

                if (index === data.length - 1) {
                    dfd.resolve(nodeArray);
                }
            });

            return dfd.promise();
        },
        initTreeEvents: function () {
            var self = this;

            this.$el.on('click', '.rock-tree-folder > .glyphicon-folder-open, .rock-tree-folder > .glyphicon-folder-close', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $icon = $(this),
					$ul = $icon.siblings('ul'),
					itemId = $icon.parent('li').attr('data-id'),
					node;

                if ($icon.hasClass('glyphicon-folder-open')) {
                    $icon.removeClass('glyphicon-folder-open').addClass('glyphicon-folder-close');
                    $ul.hide();
                } else {
                    node = _findNodeById(self.nodes, itemId);

                    if (!node.children) {
                        // Attempt to get child nodes of the parent
                        $icon.parent('li').append(self.options.loadingHtml);
                        self.fetch(itemId).done(function (data) {
                            self.render(data, node);
                            $ul = $icon.siblings('ul');
                            $ul.show();
                        });
                    } else {
                        $ul.show();
                    }

                    $icon.removeClass('glyphicon-folder-close').addClass('glyphicon-folder-open');

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
                $rockTree.trigger('selected', id);
            });
        }
    };

    $.fn.rockTree = function (options) {
        var settings = $.extend($.fn.rockTree.defaults, options);

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
        loadingHtml: '<span class="rock-tree-loading"><i class="glyphicon glyphicon-refresh icon-spin"></i>Loading...</span>'
    };
}(jQuery));