using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Improving.MediatR.Cache;

namespace Improving.MediatR.Tests.Cache
{
    [TestClass]
    public class CacheHandlerTests
    {
        private IWindsorContainer _container;
        private IMediator _mediator;

        [TestInitialize]
        public void TestInitialize()
        {
            _container = new WindsorContainer()
                .Install(FromAssembly.This(),
                         new MediatRInstaller(Classes.FromThisAssembly())
                         );
            _mediator = _container.Resolve<IMediator>();

            StockQuoteHandler.Called = 0;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _container.Dispose();
        }

        [TestMethod]
        public async Task Should_Make_Initial_Request()
        {
            Assert.AreEqual(0, StockQuoteHandler.Called);
            var getQuote = new GetStockQuote("AAPL");
            var quote    = await _mediator.SendAsync(getQuote.Cached());
            Assert.AreEqual(1, StockQuoteHandler.Called);
        }

        [TestMethod]
        public async Task Should_Cache_Initial_Response()
        {
            Assert.AreEqual(0, StockQuoteHandler.Called);
            var getQuote = new GetStockQuote("AAPL");
            var quote1   = await _mediator.SendAsync(getQuote.Cached());
            var quote2   = await _mediator.SendAsync(getQuote.Cached());
            Assert.AreEqual(1, StockQuoteHandler.Called);
            Assert.AreEqual(quote1.Value, quote2.Value);
        }

        [TestMethod]
        public async Task Should_Refresh_Response_If_Old()
        {
            Assert.AreEqual(0, StockQuoteHandler.Called);
            var getQuote = new GetStockQuote("AAPL");
            await _mediator.SendAsync(getQuote.Cached());
            await Task.Delay(TimeSpan.FromSeconds(.2));
            await _mediator.SendAsync(getQuote.Cached(TimeSpan.FromSeconds(.1)));
            Assert.AreEqual(2, StockQuoteHandler.Called);
        }

        [TestMethod]
        public async Task Should_Invalidate_Cache()
        {
            Assert.AreEqual(0, StockQuoteHandler.Called);
            var getQuote = new GetStockQuote("AAPL");
            var quote1 = await _mediator.SendAsync(getQuote.Cached());
            var quote2 = await _mediator.SendAsync(getQuote.Cached());
            var quote3 = await _mediator.SendAsync(getQuote.InvalidateCache());
            var quote4 = await _mediator.SendAsync(getQuote.Cached());
            Assert.AreEqual(2, StockQuoteHandler.Called);
            Assert.AreEqual(quote1.Value, quote2.Value);
            Assert.AreEqual(quote1.Value, quote3.Value);
            Assert.AreEqual(quote1.Value, quote3.Value);
        }


        [TestMethod]
        public async Task Should_Not_Cache_Exceptions()
        {
            Assert.AreEqual(0, StockQuoteHandler.Called);
            var getQuote = new GetStockQuote("EX");
            try
            {
                await _mediator.SendAsync(getQuote.Cached());
                Assert.Fail("Expected exception");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Stock Exchange is down", ex.Message);
            }
            try
            {
                await _mediator.SendAsync(getQuote.Cached());
                Assert.Fail("Expected exception");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Stock Exchange is down", ex.Message);
            }
            Assert.AreEqual(2, StockQuoteHandler.Called);
        }
    }

    public class GetStockQuote 
        : Request.WithResponse<StockQuote>,
          IEquatable<GetStockQuote>
    {
        public GetStockQuote(string symbol)
        {
            Symbol = symbol;
        }

        public string Symbol { get; set; }

        public bool Equals(GetStockQuote other)
        {
            return Symbol == other.Symbol;
        }
    }

    public class StockQuote
    {
        public string Symbol { get; set; }

        public double Value  { get; set; }
    }

    public class StockQuoteHandler :
        IAsyncRequestHandler<GetStockQuote, StockQuote>
    {
        public static int Called;

        private readonly Random random = new Random();

        public Task<StockQuote> Handle(GetStockQuote quote)
        {
            ++Called;

            if (quote.Symbol == "EX")
                throw new Exception("Stock Exchange is down");

            return Task.FromResult(new StockQuote
            {
                Symbol = quote.Symbol,
                Value  = random.NextDouble() * 10.0
            });
        }
    }
}
