using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tracker.Models;
using TrackerWeb.Controllers.API;
using TrackerWeb.Models;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class ExpensesTest : ApiTestBase
    {
        private ExpensesController _controller;
        private ApplicationDbContext _context;
        private IList<Expense> _data;
        private ApplicationUser _user;
        private Mock<ApplicationDbContext> _mockContext;

        [TestInitialize]
        public new void Init()
        {
            base.Init();

            _user = UserManager.FindByEmailAsync(TestConfig.TestUserEmail).Result;

            _data = new List<Expense> 
            { 
                new Expense {Amount = 1, ApplicationUserID = _user.Id, Comment = "AAA", Date = DateTime.Now, Description = "DescAAA"}, 
                new Expense {Amount = 2, ApplicationUserID = _user.Id, Comment = "BBB", Date = DateTime.Now, Description = "DescBBB"}, 
                new Expense {Amount = 3, ApplicationUserID = _user.Id, Comment = "CCC", Date = DateTime.Now, Description = "DescCCC"}, 
                new Expense {Amount = 4, ApplicationUserID = "Some other ID", Comment = "DDD", Date = DateTime.Now, Description = "DescDDD"}, 
            };

            _user.Expenses = _data.Where(d => d.ApplicationUserID == _user.Id).ToList();

            var mockSet = CreateMockDbSet(_data);

            _mockContext = new Mock<ApplicationDbContext>();

            _mockContext.Setup(c => c.Expenses).Returns(mockSet); 

            _context = _mockContext.Object;

            _controller = new ExpensesController(UserManager, _context);
            SetupControllerForTests(_controller);
        }

        [TestMethod]
        public void TestUserManagerGetter()
        {
            var controller = new ExpensesController();
            SetupControllerForTests(controller);
            Assert.AreSame(UserManager, controller.UserManager);
        }

        [TestMethod]
        public void TestDataContextGetter()
        {
            var controller = new ExpensesController();
            SetupControllerForTests(controller);
            var owin = controller.Request.Properties["MS_OwinContext"] as OwinContext;
            owin.Set(_context);
            Assert.AreSame(_context, controller.Context);
        }

        [TestMethod]
        public void TestExpenseModel()
        {
            var expense = new Expense();
            Assert.AreNotEqual(Guid.Empty, expense.ExpenseId);
        }

        [TestMethod]
        public async Task TestGetExpenses()
        {
            var result = await _controller.GetExpenses();
            var expectedResult = _data.Where(d => d.ApplicationUserID == _user.Id).ToList();
            ValidateResult(expectedResult, result.ToList());
        }

        [TestMethod]
        public async Task TestGetExpenseSuccess()
        {
            var expected = _data.First(d => d.ApplicationUserID == _user.Id);
            var result = await _controller.GetExpense(expected.ExpenseId);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<Expense>));
            var contentResult = (OkNegotiatedContentResult<Expense>)result;
            Assert.AreSame(expected, contentResult.Content);
        }

        [TestMethod]
        public async Task TestGetExpenseNotFound()
        {
            var result = await _controller.GetExpense(Guid.NewGuid());
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task TestGetExpenseWrongUser()
        {
            var wrongUserData = _data.First(d => d.ApplicationUserID != _user.Id);
            var result = await _controller.GetExpense(wrongUserData.ExpenseId);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task TestPutExpenseBadModelState()
        {
            var expense = new Expense();
            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.PutExpense(expense.ExpenseId, expense);
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public async Task TestPutExpenseIdMismatch()
        {
            var expense = new Expense();
            var result = await _controller.PutExpense(Guid.NewGuid(), expense);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task TestPutExpenseNotFound()
        {
            var expense = new Expense();
            var result = await _controller.PutExpense(expense.ExpenseId, expense);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task TestPutExpenseWrongUser()
        {
            var expense = _data.First(d => d.ApplicationUserID != _user.Id);
            var result = await _controller.PutExpense(expense.ExpenseId, expense);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task TestPutExpenseSuccess()
        {
            var targetExpense = _data.First(d => d.ApplicationUserID == _user.Id);
            var expense = new Expense {ExpenseId = targetExpense.ExpenseId};
            var result = await _controller.PutExpense(expense.ExpenseId, expense);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            var statusResult = (StatusCodeResult)result;
            Assert.AreEqual(HttpStatusCode.NoContent, statusResult.StatusCode);
            Assert.AreEqual(_user.Id, expense.ApplicationUserID);
            _mockContext.Verify(m => m.SetModified(It.IsAny<object>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task TestPostExpenseBadModelState()
        {
            var expense = new Expense();
            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.PostExpense(expense);
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public async Task TestPostExpenseSuccess()
        {
            var expense = new Expense();
            var result = await _controller.PostExpense(expense);
            Assert.IsInstanceOfType(result, typeof(CreatedAtRouteNegotiatedContentResult<Expense>));
            var contentResult = (CreatedAtRouteNegotiatedContentResult<Expense>) result;
            Assert.AreSame(expense, contentResult.Content);
            Assert.IsTrue(_data.Contains(expense));
            Assert.AreEqual(expense.ApplicationUserID, _user.Id);
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task TestPostExpenseExists()
        {
            _mockContext.Setup(m => m.SaveChangesAsync()).Throws<DbUpdateException>();
            var targetExpense = _data.First(d => d.ApplicationUserID == _user.Id);
            var expense = new Expense { ExpenseId = targetExpense.ExpenseId };
            var result = await _controller.PostExpense(expense);
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        [ExpectedException(typeof(DbUpdateException))]
        public async Task TestPostExpenseSaveError()
        {
            var expense = new Expense();
            _mockContext.Setup(m => m.SaveChangesAsync()).Returns(() =>
            {
                _data.Remove(expense);
                throw new DbUpdateException();
            });
            await _controller.PostExpense(expense);
        }

        [TestMethod]
        public async Task TestDeleteExpenseNotFound()
        {
            var result = await _controller.DeleteExpense(Guid.NewGuid());
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task TestDeleteExpenseWrongUser()
        {
            var wrongUserData = _data.First(d => d.ApplicationUserID != _user.Id);
            var result = await _controller.DeleteExpense(wrongUserData.ExpenseId);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task TestDeleteExpenseSuccess()
        {
            var target = _data.First(d => d.ApplicationUserID == _user.Id);
            var result = await _controller.DeleteExpense(target.ExpenseId);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<Expense>));
            var contentResult = (OkNegotiatedContentResult<Expense>)result;
            Assert.AreSame(target, contentResult.Content);
            Assert.IsFalse(_data.Contains(target));
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        private void ValidateResult(List<Expense> expectedResult, List<Expense> result)
        {
            Assert.AreEqual(expectedResult.Count, result.Count);

            expectedResult.Sort(ExpenseSorter);
            result.Sort(ExpenseSorter);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreSame(expectedResult[i], result[i], string.Format("Elements in position {0} don't match", i));
            }
        }

        private int ExpenseSorter(Expense expense, Expense expense1)
        {
            return expense.ExpenseId.CompareTo(expense1.ExpenseId);
        }

        private DbSet<T> CreateMockDbSet<T>(IList<T> source) where T : class
        {
            var queryable = source.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IDbAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<T>(queryable.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<T>(queryable.Provider));

            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Returns<T>(t =>
            {
                source.Add(t);
                return t;
            });
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Returns<T>(t =>
            {
                source.Remove(t);
                return t;
            });

            var keyGetter = GetKeyGetter(typeof (T));

            Func<object[], T> find = objects => queryable.FirstOrDefault(item => objects.Any(o => o.Equals(keyGetter(item))));

            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(objects => find(objects));
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns<object[]>(objects => Task.FromResult(find(objects)));
            mockSet.Setup(m => m.FindAsync(It.IsAny<CancellationToken>(), It.IsAny<object[]>())).Returns<CancellationToken, object[]>((ct, objects) => Task.FromResult(find(objects)));

            return mockSet.Object;
        }

        private Func<object, object> GetKeyGetter(Type target)
        {
            var typeinfo = target.GetTypeInfo();
            var keyProperty =
                typeinfo.DeclaredProperties.FirstOrDefault(
                    p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof (KeyAttribute)));

            if (keyProperty == null)
            {
                //should look for key here in the way Entity Framework does it
                throw new NotImplementedException();
            }
            var method = keyProperty.GetGetMethod();

            return t => method.Invoke(t, new object[0]);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            _controller.Dispose();
            _context.Dispose();
            base.Cleanup();
        }

        protected override string GetControllerPath()
        {
            return "http://localhost/api/Expenses";
        }
    }
}
