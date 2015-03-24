function HomeViewModel(app, dataModel) {
    function simpleAjax(url, method, data, success, error) {
        $.ajax({
            method: method,
            url: url,
            contentType: "application/json; charset=utf-8",
            data: data,
            headers: {
                'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
            },
            success: function(data) {
                if (success != null) {
                    success(data);
                }
            },
            error: function(jqXHR, textStatus, errorThrown) {
                if (error != null) {
                    error(jqXHR.responseJSON);
                }
            }
        });

    }

    var self = this;

    self.expenses = ko.betterObservableArray();
    self.addExpense = function (data, success, error) {
        simpleAjax(app.dataModel.userExpensesUrl, "POST", JSON.stringify(data),
            function(data) {
                self.expenses.push(data);
                if (success != null) {
                    success(data);
                }
            }, error);
    }

    self.deleteExpense = function (id, success, error) {
        simpleAjax(app.dataModel.userExpensesUrl + "/" + id, "DELETE", undefined,
            function (data) {
                self.expenses.remove(function(item) {
                    return item.id === id;
                });
                if (success != null) {
                    success(data);
                }
            }, error);
    }

    self.updateExpense = function (id, data, success, error) {
        simpleAjax(app.dataModel.userExpensesUrl + "/" + id, "PUT", JSON.stringify(data),
            function (data) {
                var target = _.first(self.expenses(), function(item) {
                    return item.id === id;
                });
                _.extendOwn(target, data);
                if (success != null) {
                    success(data);
                }
            }, error);
    }

    Sammy(function () {
        this.get('#home', function () {
            RunHomeView(self, dataModel);
            simpleAjax(app.dataModel.userExpensesUrl, "GET", undefined,
                function(data) {
                    self.expenses.setValue(data);
                });
        });
        this.get('/', function () { this.app.runRoute('get', '#home') });
    });

    return self;
}

app.addViewModel({
    name: "Home",
    bindingMemberName: "home",
    factory: HomeViewModel
});
