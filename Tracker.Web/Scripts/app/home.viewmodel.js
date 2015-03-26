function HomeViewModel(app, dataModel) {

    var self = this;

    self.expenses = app.expenses;

    self.addExpense = function (data, success, error) {
        app.simpleAjax(app.dataModel.userExpensesUrl, "POST", JSON.stringify(data),
            function(data) {
                self.expenses.push(data);
                if (success != null) {
                    success(data);
                }
            }, error);
    }

    self.deleteExpense = function (id, success, error) {
        app.simpleAjax(app.dataModel.userExpensesUrl + "/" + id, "DELETE", undefined,
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
        app.simpleAjax(app.dataModel.userExpensesUrl + "/" + id, "PUT", JSON.stringify(data),
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
            app.view(app.home());
            RunHomeView(self, dataModel);
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
