"use strict";
(function(window, undefined) {
    var initialized = false;

    window.RunHomeView = function(viewModel, dataModel) {
        var self = this;
        if (initialized) return;
        initialized = true;

        function modelToData(model) {
            var data = _.extendOwn({}, model);
            var m = moment.utc(data.date);
            data.date = m.format(dataModel.dateFormat);
            data.time = m.format(dataModel.timeFormat);
            return data;
        }

        function dataToModel(data) {
            var model = _.extendOwn({}, data);
            delete model.time;
            model.date = data.date + "T" + data.time + "Z";
            return model;
        }

        function create(data, successCallback, errorCallback) {
            var vm = dataToModel(data.data);
            delete vm.expenseId;
            viewModel.addExpense(vm, function() {
                successCallback(null);
            }, errorCallback);
        }

        function edit(data, successCallback, errorCallback) {
            viewModel.updateExpense(data.id, dataToModel(data.data), successCallback, errorCallback);
        }

        function remove(data, successCallback, errorCallback) {
            viewModel.deleteExpense(data.id, successCallback, errorCallback);
        }

        var ajaxActions = { create: create, edit: edit, remove: remove }

        function editorAction(method, url, data, successCallback, errorCallback) {
            ajaxActions[data.action](data, function(row) {
                successCallback({ row: row });
            }, function(dataObject) {
                if (dataObject != null) {
                    var dtObject = { error: dataObject.message, row: data.data, fieldErrors: [] };
                    var modelState = dataObject.modelState;
                    _.each(_.keys(modelState), function(key) {
                        var parts = key.split(".");
                        if (parts.length === 2 && parts[0] === "expense") {
                            dtObject.fieldErrors.push({ name: parts[1], status: modelState[key][0] });
                            if (parts[1] === "date") {
                                dtObject.fieldErrors.push({ name: "time", status: modelState[key][0] });
                            }
                        }
                    });
                    successCallback(dtObject);
                } else {
                    errorCallback();
                }
            });
        }

        var data = _.map(viewModel.expenses(), modelToData);

        var editor = new $.fn.dataTable.Editor({
            table: "#expences",
            idSrc: "expenseId",
            fields: [
                {
                    name: "expenseId",
                    type: "hidden"
                }, {
                    label: "Date:",
                    name: "date",
                    type: "date",
                    dateFormat: $.datepicker.ISO_8601
                }, {
                    label: "Time:",
                    name: "time",
                    type: "time",
                    opts: {
                        timeFormat: dataModel.timeFormat,
                        showSecond: false,
                        showTimezone: false,
                        addSliderAccess: true,
                        sliderAccessArgs: { touchonly: false }
                    }
                }, {
                    label: "Description:",
                    name: "description"
                }, {
                    label: "Amount:",
                    name: "amount"
                }, {
                    label: "Comment:",
                    name: "comment"
                }
            ],
            ajax: editorAction
        });

        $("#expences").on("click", "tbody td:not(:first-child)", function(e) {
            editor.inline(this, {
                buttons: { label: '&gt;', fn: function() { this.submit(); } }
            });
            $("div.DTE_Inline_Buttons").addClass("input-group-addon");
        });

        var table = $("#expences").DataTable({
            data: data,
            deferRender: true,
            lengthChange: false,
            pageLength: 25,
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Search"
            },
            columns: [
                { data: null, defaultContent: "", orderable: false },
                { data: "expenseId" },
                { data: "date" },
                { data: "time" },
                { data: "description" },
                { data: "amount" },
                { data: "comment" }
            ],
            order: [2, "desc"],
            columnDefs: [
                {
                    targets: [1],
                    visible: false,
                    searchable: false
                }, {
                    targets: [2],
                    orderData: [2, 3]
                }, {
                    targets: [3],
                    orderData: [3, 2]
                }
            ]
        });

        var tableTools = new $.fn.dataTable.TableTools(table, {
            sRowSelect: "os",
            sRowSelector: "td:first-child",
            aButtons: [
                { sExtends: "editor_create", editor: editor },
                { sExtends: "editor_edit", editor: editor },
                { sExtends: "editor_remove", editor: editor }
            ]
        });

        $(tableTools.fnContainer()).appendTo("#expences_wrapper .col-sm-6:eq(0)");

        viewModel.expenses.subscribe(function(addedItem) {
            table.row.add(modelToData(addedItem)).draw();
        }, null, "add");

        viewModel.expenses.subscribe(function(removedItem) {
            var rowIdx = table.column(1).data().indexOf(removedItem.id);
            table.row(rowIdx).remove().draw();
        }, null, "remove");
    }
})(window);