using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Core.Services;
using Tracker.Models;
using Tracker.Models.Account;

namespace Tracker.Core.Tests
{
    [TestClass]
    public class ServerServiceTest
    {
        private ServerService _serverService;
        private UserHelper _helper = new UserHelper();

        [TestInitialize]
        public void Init()
        {
            _serverService = new ServerService(TestConfig.AppUrl);
        }

        [TestMethod]
        public async Task RegisterUserTest()
        {
            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);

            var model = new RegisterModel
            {
                Email = TestConfig.TestUserEmail,
                Password = TestConfig.TestUserPassword,
                Name = "Test User"
            };

            var result = await _serverService.Register(model);

            Assert.IsTrue(result);

            var user = await _helper.UserExists(TestConfig.TestUserEmail);
            Assert.IsNotNull(user);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        public async Task LoginTest()
        {
            await _helper.EnsureUserExists(TestConfig.TestUserEmail);

            var result = await _serverService.Login(TestConfig.TestUserEmail, TestConfig.TestUserPassword);

            Assert.IsTrue(result);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        public async Task LoginFailTest()
        {
            var result = await _serverService.Login(TestConfig.TestUserEmail, "qwerty");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExpensesTest()
        {
            await _helper.EnsureUserExists(TestConfig.TestUserEmail);
            await _helper.AddExpense(TestConfig.TestUserEmail);

            await _serverService.Login(TestConfig.TestUserEmail, TestConfig.TestUserPassword);
            var result = await _serverService.GetExpenses();

            Assert.AreEqual(1, result.Count);

            var expense = result.Single();

            Assert.AreEqual(TestConfig.TestUserPassword, expense.Comment);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        public async Task CreateExpenseTest()
        {
            await _helper.EnsureUserExists(TestConfig.TestUserEmail);

            await _serverService.Login(TestConfig.TestUserEmail, TestConfig.TestUserPassword);
            var result = await _serverService.CreateExpense(new Expense
                    {
                        Amount = 1,
                        Comment = TestConfig.TestUserPassword,
                        Date = DateTime.Now,
                        Description = "TestDescr"
                    });

            Assert.IsNotNull(result);

            var expenses = await _helper.GetExpenses(TestConfig.TestUserEmail);

            Assert.AreEqual(1, expenses.Count);

            var expense = expenses.Single();

            Assert.AreEqual(TestConfig.TestUserPassword, expense.Comment);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        public async Task DeleteExpenseTest()
        {
            await _helper.EnsureUserExists(TestConfig.TestUserEmail);
            var expense = await _helper.AddExpense(TestConfig.TestUserEmail);

            await _serverService.Login(TestConfig.TestUserEmail, TestConfig.TestUserPassword);
            var result = await _serverService.RemoveExpense(expense.ExpenseId);

            Assert.IsNotNull(result);

            var newExpense = await _serverService.GetExpense(expense.ExpenseId);

            Assert.IsNull(newExpense);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        public async Task UpdateExpenseTest()
        {
            await _helper.EnsureUserExists(TestConfig.TestUserEmail);
            var expense = await _helper.AddExpense(TestConfig.TestUserEmail);

            await _serverService.Login(TestConfig.TestUserEmail, TestConfig.TestUserPassword);
            var result = await _serverService.UpdateExpense(new Expense
                    {
                        Amount = 1,
                        Comment = "123",
                        Date = DateTime.Now,
                        Description = "321",
                        ExpenseId = expense.ExpenseId
                    });

            Assert.IsTrue(result);

            var newExpense = await _serverService.GetExpense(expense.ExpenseId);

            Assert.AreEqual("123", newExpense.Comment);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExpensesMustLoginTest()
        {
            await _serverService.GetExpenses();
        }
    }
}
