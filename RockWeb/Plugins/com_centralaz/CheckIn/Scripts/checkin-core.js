Sys.Application.add_load(function () {

    bindPersonSelect();

    ///
    /// Use for handling selecting multiple people on the PersonSelect block.
    ///
    function bindPersonSelect () {
        var selectedCount = 0,
            $nextButton = $("a[id$='_lbNext']"),
            attendees = [],
            ids,
            $attendeeList = $("input[id$='hfSelectedPeopleIds']");

        function addAttendee(id) {
            ids = $attendeeList.val();
            attendees = ids.length > 0 ? ids.split(',') : [];
            attendees.push(id);
            ids = attendees.join(',');
            $attendeeList.val(ids);
        }

        function removeAttendee(id) {
            var i;
            ids = $attendeeList.val();
            attendees = ids.length > 0 ? ids.split(',') : [];
            i = $.inArray(id, attendees);

            if (i > -1) {
                attendees.splice(i, 1);
            }

            ids = attendees.join(',');
            $attendeeList.val(ids);
        }

        /// When selecting a person, add a checkbox class and keep track of
        /// how many are selected so the next button can be enabled/disabled
        /// as needed.
        $("a[id$='lbSelect']").click(function (e) {
            var $that = $(this);

            if ($that.hasClass('js-dataButton')) {
                addAttendee($that.next().val());
                $that.removeClass('js-dataButton btn-default').addClass('js-dataButtonSelected btn-primary');
                $that.append("<div class='js-select-checkbox' style='float: right; display: inline-block;'><i class='fa fa-check-square-o'></i></div>");
                selectedCount += 1;
            } else {
                removeAttendee($that.next().val());
                $that.find("div").remove();
                $that.removeClass('js-dataButtonSelected btn-primary').addClass('js-dataButton btn-default');
                selectedCount -= 1;
            }

            $nextButton = $("a[id$='_lbNext']");

            if (selectedCount > 0) {
                $nextButton.prop('disabled', false);
                $nextButton.removeClass('disabled btn-default').addClass('btn-primary');
            } else {
                $nextButton.prop('disabled', true);
                $nextButton.addClass('disabled btn-default').removeClass('btn-primary');
            }

            return false;
        });
    }
});

