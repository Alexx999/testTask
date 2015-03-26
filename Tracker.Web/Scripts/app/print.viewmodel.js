﻿function PrintViewModel(app, dataModel) {
    var self = this;

    function validateDates() {
        var startVal = self.startDate();
        var endVal = self.endDate();
        if (startVal == null || endVal == null) return false;
        if (startVal > endVal) return false;
        return true;
    }
    
    function dateChanged() {
        if (!validateDates()) return;

        var allData = app.expenses();
        var startDate = self.startDate();
        var endDate = self.endDate();

        var datesInRange = _.filter(allData, function(item) {
            var date = new Date(item.date);
            if (date > endDate || date < startDate) {
                return false;
            }
            return true;
        });

        var converted = _.map(datesInRange, function(item) {
            var newVal = _.extendOwn({}, item);
            var m = moment(item.date);
            newVal.dateOnly = m.format(dataModel.dateFormat);
            newVal.time = m.format(dataModel.timeFormat);
            return newVal;
        });

        self.items(converted);
    }

    self.startDate = ko.observable();
    self.endDate = ko.observable();

    self.pickerVisible = ko.observable(false);

    self.showPicker = function() {
        self.pickerVisible(true);
    }

    self.hidePicker = function () {
        self.pickerVisible(false);
    }

    self.datesValid = ko.pureComputed(function () {
        return validateDates();
    });

    self.start = ko.pureComputed({
        read: function() {
            var value = self.startDate();
            if (value == null) return "";
            var m = moment(value);
            if (m.isValid()) {
                return m.format(dataModel.dateFormat);
            }
            return "";
        },
        write: function(value) {
            var m = moment(value, dataModel.dateFormat);
            if (m.isValid()) {
                self.startDate(m.toDate());
            }
        },
        owner: this
    });

    self.end = ko.pureComputed({
        read: function() {
            var value = self.endDate();
            if (value == null) return "";
            var m = moment(value);
            if (m.isValid()) {
                return m.format(dataModel.dateFormat);
            }
            return "";
        },
        write: function(value) {
            var m = moment(value, dataModel.dateFormat);
            if (m.isValid()) {
                self.endDate(m.toDate());
            }
        },
        owner: this
    });
    
    self.items = ko.observableArray([]);
    
    self.startDate.subscribe(function() {
        dateChanged();
    });
    self.endDate.subscribe(function () {
        dateChanged();
    });

    Sammy(function() {
        this.get('#print', function() {
            app.view(app.print());
            RunPrintView(self, dataModel);
        });
    });

    return self;
}

app.addViewModel({
	name: "Print",
	bindingMemberName: "print",
	factory: PrintViewModel
});