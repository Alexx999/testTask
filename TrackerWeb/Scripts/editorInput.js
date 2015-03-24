(function (window, document, undefined) {
    var Editor = $.fn.dataTable.Editor;
    $.fn.dataTable.Editor.fieldTypes.date = $.extend(true, {}, $.fn.dataTable.Editor.models.fieldType, {
        "create": function(conf) {
            conf._input = $('<input />').attr($.extend({
                type: 'text',
                id: Editor.safeId(conf.id),
                'class': 'jqueryui'
            }, conf.attr || {}));

            if (! conf.dateFormat) {
                conf.dateFormat = $.datepicker.RFC_2822;
            }

            if (conf.dateImage === undefined) {
                conf.dateImage = "../../images/calender.png";
            }

            // Allow the element to be attached to the DOM
            setTimeout(function() {
                $(conf._input).datepicker($.extend({
                    showOn: "both",
                    dateFormat: conf.dateFormat,
                    buttonImage: conf.dateImage,
                    buttonImageOnly: true
                }, conf.opts));
                $('#ui-datepicker-div').css('display', 'none');
            }, 10);

            conf._input.on("focusout", function () {
                $(conf._input).datepicker("hide");
            });

            return conf._input[0];
        },

        // use default get method as will work for both

        "set": function(conf, val) {
            $.datepicker ?
                conf._input.datepicker("setDate", val).change() :
                $(conf._input).val(val);
        },

        "enable": function(conf) {
            $.datepicker ?
                conf._input.datepicker("enable") :
                $(conf._input).prop('disable', false);
        },

        "disable": function(conf) {
            $.datepicker ?
                conf._input.datepicker("disable") :
                $(conf._input).prop('disable', true);
        },

        owns: function(conf, node) {
            return $(node).parents('div.ui-datepicker').length || $(node).parents('div.ui-datepicker-header').length ?
                true :
                false;
        }
    });
}(window, document));