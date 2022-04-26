// jquery ready start
$(document).ready(function() {
    // find all font awesome icons
    $('.fa').each(function() {
        var icon = this;
        // get all classes
        var classes = $(this).attr('class');
        // split the classes into an array
        var classesArray = classes.split(' ');
        // loop through the classes
        for (var i = 0; i < classesArray.length; i++) {
            // if the class starts with fa-
            if (classesArray[i].substring(0, 3) == 'fa-') {
                // get the icon name
                var iconName = classesArray[i].substring(3);

                var fileLocation = "/Assets/Fonts/FontAwesome/svgs/solid/" + iconName + ".svg";
                // get the contents of the file
                $.ajax({
                    url: fileLocation,
                }).done(function(result) {
                    // replace the icon with the svg
                    var _html = '<i class="inline-fa-svg">' + new XMLSerializer().serializeToString(result.documentElement); + '</i>';
                    $(icon).replaceWith(_html);
                });

            }
        }
    });
});
