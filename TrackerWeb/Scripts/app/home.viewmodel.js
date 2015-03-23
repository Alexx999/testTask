function HomeViewModel(app, dataModel) {
    var self = this;

    self.expenses = ko.observableArray([]);
    self.addExpense = function(date, description, amount, comment) {
        var data = { date: date, description: description, amount: amount, comment: comment };
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
                    self.expenses(data);
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
