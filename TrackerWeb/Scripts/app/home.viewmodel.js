function HomeViewModel(app, dataModel) {
    var self = this;

    self.expenses = ko.betterObservableArray();
    self.addExpense = function(data, success, error) {
        $.ajax({
            method: 'post',
            url: app.dataModel.userExpensesUrl,
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(data),
            headers: {
                'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
            },
            success: function (data) {
                self.expenses.push(data);
                if (success != null) {
                    success(data);
                }
            },
            error: function () {
                if (error != null) {
                    error();
                }
            }
        });
    }

    Sammy(function () {
        this.get('#home', function () {
            RunHomeView(self, dataModel);
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            $.ajax({
                method: 'get',
                url: app.dataModel.userExpensesUrl,
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                },
                success: function (data) {
                    self.expenses.setValue(data);
                }
            });
            //self.addExpense(new Date().toISOString(), "My expense", 0, "My Comment");
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
