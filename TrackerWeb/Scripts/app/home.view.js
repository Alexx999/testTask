"use strict";
function RunHomeView(viewModel, dataModel) {
    var self = this;

    //TODO: This is dead slow code for testing purpose only
    function subscribeArrayChanged (array, addCallback, deleteCallback) {
        var previousValue = undefined;
        array.subscribe(function (beforeChangeValue) {
            previousValue = beforeChangeValue.slice(0);
        }, undefined, "beforeChange");
        array.subscribe(function (latestValue) {
            var editScript = ko.utils.compareArrays(previousValue, latestValue);
            for (var i = 0, j = editScript.length; i < j; i++) {
                switch (editScript[i].status) {
                    case "retained":
                        break;
                    case "deleted":
                        if (deleteCallback)
                            deleteCallback(editScript[i].value);
                        break;
                    case "added":
                        if (addCallback)
                            addCallback(editScript[i].value);
                        break;
                }
            }
            previousValue = undefined;
        });
    };

    function modelToData(model) {
        return model;
    }

    function dataToModel(data) {
        var result = _.extendOwn({}, data);
        var date = new Date(Date.parse(model.date));
        result.date = date.toLocaleDateString();
        result.time = date.toLocaleTimeString();
        return result;
    }

    function create(data, callback) {
        
    }

    function edit(data, callback) {
        
    }

    function remove(data, callback) {
        
    }

    var ajaxActions = { create: create, edit: edit, remove: remove }

    function editorAction(method, url, data, successCallback, errorCallback) {
        ajaxActions[data.action](dataToModel(data.data), function () {
            successCallback({ row: null });
        });
    }

    var data = _.map(viewModel.expenses(), modelToData);

    var editor = new $.fn.dataTable.Editor({
        table: "#expences",
        fields: [
            {
                label: "Date:",
                name: "date",
                type: "date"
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

    $("#expences").on("click", "tbody td:not(:first-child)", function (e) {
        editor.inline(this);
    });

    var table = $("#expences").DataTable({
        data: data,
        deferRender: true,
        lengthChange: false,
        columns: [
            { data: null, defaultContent: "", orderable: false },
            { data: "expenseId" },
            { data: "date" },
            { data: "date" },
            { data: "description" },
            { data: "amount" },
            { data: "comment" }
        ],
        order: [2, "asc"],
        columnDefs: [
            {
                targets: [1],
                visible: false,
                searchable: false
            }, {
                targets: [2],
                render: function(data, type, full) {
                    if (data) {
                        return new Date(data).toLocaleDateString();
                    }
                    return "";
                }
            }, {
                targets: [3],
                render: function(data, type, full) {
                    if (data) {
                        return new Date(data).toLocaleTimeString();
                    }
                    return "";
                }
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

    viewModel.expenses.subscribe(function (addedItem) {
        table.row.add(modelToData(addedItem)).draw();
    }, null, "add");

    viewModel.expenses.subscribe(function (removedItem) {
        var rowIdx = table.column(1).data().indexOf(removedItem.id);
        table.row(rowIdx).remove().draw();
    }, null, "remove");
}

