(function (window, document, undefined) {
    var Editor = $.fn.dataTable.Editor;
    $.fn.dataTable.Editor.fieldTypes.time = $.extend(true, {}, $.fn.dataTable.Editor.models.fieldType, {
        "create": function(conf) {
            conf._input = $("<input />").attr($.extend({
                type: "text",
                id: Editor.safeId(conf.id),
                'class': "date-picker form-control"
            }, conf.attr || {}));

            if (! conf.dateFormat) {
                conf.dateFormat = $.datepicker.RFC_2822;
            }

            // Allow the element to be attached to the DOM
            setTimeout(function() {
                $(conf._input).timepicker($.extend({
                    dateFormat: conf.dateFormat
                }, conf.opts));
            }, 10);

            return conf._input[0];
        },

        "get": function (conf) {
            return $(conf._input).val();
        },

        "set": function(conf, val) {
            $(conf._input).val(val);
        },

        "enable": function(conf) {
            $.datepicker ?
                conf._input.timepicker("enable") :
                $(conf._input).prop("disable", false);
        },

        "disable": function(conf) {
            $.datepicker ?
                conf._input.timepicker("disable") :
                $(conf._input).prop("disable", true);
        },

        owns: function(conf, node) {
            return $(node).parents("div.ui-datepicker").length || $(node).parents("div.ui-datepicker-header").length ?
                true :
                false;
        }
    });

    $.fn.dataTable.Editor.fieldTypes.date = $.extend(true, {}, $.fn.dataTable.Editor.models.fieldType, {
        "create": function(conf) {
            conf._input = $("<input />").attr($.extend({
                type: "text",
                id: Editor.safeId(conf.id),
                'class': "date-picker form-control"
            }, conf.attr || {}));

            if (! conf.dateFormat) {
                conf.dateFormat = $.datepicker.RFC_2822;
            }

            // Allow the element to be attached to the DOM
            setTimeout(function() {
                $(conf._input).datepicker($.extend({
                    dateFormat: conf.dateFormat
                }, conf.opts));
            }, 10);

            return conf._input[0];
        },

        "get": function(conf) {
            return $(conf._input).val();
        },

        "set": function(conf, val) {
            $.datepicker ?
                conf._input.datepicker("setDate", val).change() :
                $(conf._input).val(val);
        },

        "enable": function(conf) {
            $.datepicker ?
                conf._input.datepicker("enable") :
                $(conf._input).prop("disable", false);
        },

        "disable": function(conf) {
            $.datepicker ?
                conf._input.datepicker("disable") :
                $(conf._input).prop("disable", true);
        },

        owns: function(conf, node) {
            return $(node).parents("div.ui-datepicker").length || $(node).parents("div.ui-datepicker-header").length ?
                true :
                false;
        }
    });


    $.fn.dataTable.Editor.fieldTypes.numeric = $.extend(true, {}, $.fn.dataTable.Editor.models.fieldType, {
        "create": function(conf) {
            conf._input = $("<input/>").attr($.extend({
                id: Editor.safeId(conf.id),
                type: "text"
            }, conf.attr || {}));


            conf._input.keydown(function(e) {
                // Allow: backspace, delete, tab, escape, enter and .
                if ($.inArray(e.keyCode, [46, 44, 8, 9, 27, 13, 110, 190]) !== -1 ||
                    // Allow: Ctrl+A
                    (e.keyCode === 65 && e.ctrlKey === true) ||
                    // Allow: Ctrl+C
                    (e.keyCode === 67 && e.ctrlKey === true) ||
                    // Allow: Ctrl+X
                    (e.keyCode === 88 && e.ctrlKey === true) ||
                    // Allow: home, end, left, right
                    (e.keyCode >= 35 && e.keyCode <= 39)) {
                    // let it happen, don't do anything
                    return;
                }
                // Ensure that it is a number and stop the keypress
                if (e.preventDefault && (e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
            });

            return conf._input[0];
        },

        "get": function(conf) {
            return conf._input.val();
        },

        "set": function(conf, val) {
            conf._input.val(val).trigger("change");
        },

        "enable": function(conf) {
            conf._input.prop("disabled", false);
        },

        "disable": function(conf) {
            conf._input.prop("disabled", true);
        }
    });
}(window, document));