"use strict";
function RunHomeView(viewModel, dataModel) {
    //TODO: This is dead slow code for testing purpose only
    function subscribeArrayChanged (array, addCallback, deleteCallback) {
        var previousValue = undefined;
        array.subscribe(function (beforeChangeValue) {
            previousValue = beforeChangeValue.slice(0);
        }, undefined, 'beforeChange');
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
        var result = _.extendOwn({}, model);
        var date = new Date(Date.parse(model.date));
        result.date = date.toLocaleDateString();
        result.time = date.toLocaleTimeString();
        return result;
    }

    var self = this;

    var data = _.map(viewModel.expenses(), modelToData);

    var dt = $('#expences').DataTable({
        data: data,
        "deferRender": true,
        "columns": [
            { "data": "expenseId" },
            { "data": "date" },
            { "data": "time" },
            { "data": "description" },
            { "data": "amount" },
            { "data": "comment" }
        ],
        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            }
        ]
    });

    subscribeArrayChanged(viewModel.expenses,
		function (addedItem) {
		    dt.row.add(modelToData(addedItem)).draw();
		},
		function (deletedItem) {
		    var rowIdx = dt.column(0).data().indexOf(deletedItem.id);
		    dt.row(rowIdx).remove().draw();
		}
	);
}

