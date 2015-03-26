"use strict";
(function(window, undefined) {
    var initialized = false;

    window.RunPrintView = function(viewModel, dataModel) {
        var self = this;
        if (initialized) return;
        initialized = true;

        var startDate;
        var endDate;

        var selectCurrentWeek = function() {
            window.setTimeout(function() {
                $("#week-picker").find(".ui-datepicker-current-day a").addClass("ui-state-active");
            }, 1);
        }

        $("#week-picker").datepicker({
            dateFormat: $.datepicker.ISO_8601,
            showOtherMonths: true,
            selectOtherMonths: true,
            onSelect: function(dateText, inst) {
                var date = $(this).datepicker("getDate");
                startDate = new Date(date.getFullYear(), date.getMonth(), date.getDate() - date.getDay());
                endDate = new Date(date.getFullYear(), date.getMonth(), date.getDate() - date.getDay() + 6);
                var dateFormat = inst.settings.dateFormat || $.datepicker._defaults.dateFormat;
                viewModel.start($.datepicker.formatDate(dateFormat, startDate, inst.settings));
                viewModel.end($.datepicker.formatDate(dateFormat, endDate, inst.settings));

                selectCurrentWeek();
                viewModel.hidePicker();
            },
            beforeShowDay: function(date) {
                var cssClass = "";
                if (date >= startDate && date <= endDate)
                    cssClass = "ui-datepicker-current-day";
                return [true, cssClass];
            },
            onChangeMonthYear: function(year, month, inst) {
                selectCurrentWeek();
            }
        });


        $("#week-picker .ui-datepicker-calendar tr").on("mousemove", function() { $(this).find("td a").addClass("ui-state-hover"); });
        $("#week-picker .ui-datepicker-calendar tr").on("mouseleave", function() { $(this).find("td a").removeClass("ui-state-hover"); });

        $(".date-form .date-picker").datepicker({ dateFormat: $.datepicker.ISO_8601 });
    }
})(window);
