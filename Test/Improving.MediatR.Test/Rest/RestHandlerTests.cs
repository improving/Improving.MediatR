using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using FluentValidation;
using MediatR;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using Improving.MediatR.Rest;
using Improving.MediatR.Rest.Delete;
using Improving.MediatR.Rest.Get;
using Improving.MediatR.Rest.Post;
using Improving.MediatR.Rest.Put;

namespace Improving.MediatR.Tests.Rest
{
    [TestClass]
    public class RestHandlerTests
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
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            appBuilder.UseWebApi(config);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _container.Dispose();
        }

        #region Get

        [TestMethod]
        public async Task Should_Perform_Rest_Get()
        {
            using (WebApp.Start<RestHandlerTests>("http://localhost:9000/"))
            {
                var player = await _mediator.GetAsync<Player>("http://localhost:9000/player/1");
                Assert.AreEqual("Ronaldo", player.Name);
            }
        }

        [TestMethod]
        public async Task Should_Perform_Rest_Get_With_Resource()
        {
            using (WebApp.Start<RestHandlerTests>("http://localhost:9000/"))
            {
                var player = await _mediator
                    .GetAsync<Player, Player>(new Player { Id = 1 },
                    "http://localhost:9000/player/{Id}");
                Assert.AreEqual(1, player.Id);
                Assert.AreEqual("Ronaldo", player.Name);
            }
        }

        [TestMethod]
        public async Task Should_Validate_Rest_Get_With_Resource()
        {
            try
            {
                await _mediator
                 .GetAsync<Player, Player>(new Player { Id = -1 },
                 "http://localhost:9000/player/{Id}");
                Assert.Fail("Should have raised ValidationException");
            }
            catch (ValidationException vex)
            {
                var failures = vex.Errors.ToList();
                Assert.AreEqual(1, failures.Count);
                Assert.AreEqual("Resource.Id", failures[0].PropertyName);
                Assert.AreEqual(-1, failures[0].AttemptedValue);
                Assert.AreEqual("'Resource. Id' must be greater than '0'.", failures[0].ErrorMessage);
            }
        }

        [TestMethod]
        public void Should_Compare_Get_For_Equality()
        {
            var get1 = new GetRequest<Player, Player>();
            var get2 = new GetRequest<Player, Player>
            {
                BaseAddress = "http://localhost:2000"
            };
            var get3 = new GetRequest<Player, Player>(new Player
            {
                Id   = 1,
                Name = "Robert Lewandowski"
            });
            var get4 = new GetRequest<Player, Player>(new Player
            {
                Id = 4,
                Name = "Gareth Bale"
            });
            var get5 = new GetRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            Assert.AreNotEqual(get1, get2);
            Assert.AreNotEqual(get1, get3);
            Assert.AreNotEqual(get3, get4);
            Assert.AreEqual(get3, get5);
            Assert.AreNotEqual(get1, new PostRequest<Player, Player>());
        }

        #endregion

        #region Post

        [TestMethod]
        public async Task Should_Perform_Rest_Post()
        {
            using (WebApp.Start<RestHandlerTests>("http://localhost:9000/"))
            {
                var player = await _mediator
                    .PostAsync<Player, Player>(new Player
                    {
                        Name = "Craig"
                    }, "http://localhost:9000/player");
                Assert.AreEqual(1, player.Id);
                Assert.AreEqual("Craig", player.Name);
            }
        }

        [TestMethod]
        public void Should_Compare_Post_For_Equality()
        {
            var post1 = new PostRequest<Player, Player>();
            var post2 = new PostRequest<Player, Player>
            {
                BaseAddress = "http://localhost:2000"
            };
            var post3 = new PostRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            var post4 = new PostRequest<Player, Player>(new Player
            {
                Id = 4,
                Name = "Gareth Bale"
            });
            var post5 = new PostRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            Assert.AreNotEqual(post1, post2);
            Assert.AreNotEqual(post1, post3);
            Assert.AreNotEqual(post3, post4);
            Assert.AreEqual(post3, post5);
            Assert.AreNotEqual(post1, new PutRequest<Player, Player>());
        }

        #endregion

        #region Put

        [TestMethod]
        public async Task Should_Perform_Rest_Put()
        {
            using (WebApp.Start<RestHandlerTests>("http://localhost:9000/"))
            {
                var player = await _mediator
                    .PutAsync<Player, Player>(new Player
                    {
                        Id   = 2,
                        Name = "Sean"
                    }, "http://localhost:9000/player");
                Assert.AreEqual(2, player.Id);
                Assert.AreEqual("Sean", player.Name);
            }
        }

        [TestMethod]
        public void Should_Compare_Put_For_Equality()
        {
            var put1 = new PutRequest<Player, Player>();
            var put2 = new PutRequest<Player, Player>
            {
                BaseAddress = "http://localhost:2000"
            };
            var put3 = new PutRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            var put4 = new PutRequest<Player, Player>(new Player
            {
                Id = 4,
                Name = "Gareth Bale"
            });
            var put5 = new PutRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            Assert.AreNotEqual(put1, put2);
            Assert.AreNotEqual(put1, put3);
            Assert.AreNotEqual(put3, put4);
            Assert.AreEqual(put3, put5);
            Assert.AreNotEqual(put1, new GetRequest<Player, Player>());
        }

        #endregion

        #region Delete

        [TestMethod]
        public async Task Should_Perform_Rest_Delete()
        {
            using (WebApp.Start<RestHandlerTests>("http://localhost:9000/"))
            {
                await _mediator
                    .DeleteAsync<Player, string>(new Player { Id = 2 }, 
                       "http://localhost:9000/player/{Id}");
            }
        }

        [TestMethod]
        public void Should_Compare_Delete_For_Equality()
        {
            var delete1 = new DeleteRequest<Player, Player>();
            var delete2 = new DeleteRequest<Player, Player>
            {
                BaseAddress = "http://localhost:2000"
            };
            var delete3 = new DeleteRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            var delete4 = new DeleteRequest<Player, Player>(new Player
            {
                Id = 4,
                Name = "Gareth Bale"
            });
            var delete5 = new DeleteRequest<Player, Player>(new Player
            {
                Id = 1,
                Name = "Robert Lewandowski"
            });
            Assert.AreNotEqual(delete1, delete2);
            Assert.AreNotEqual(delete1, delete3);
            Assert.AreNotEqual(delete3, delete4);
            Assert.AreEqual(delete3, delete5);
            Assert.AreNotEqual(delete1, new PostRequest<Player, Player>());
        }

        #endregion

        [TestMethod]
        public async Task Should_Override_Get_Handler()
        {
            using (WebApp.Start<RestHandlerTests>("http://localhost:9000/"))
            {
                var player = await _mediator
                    .Use(new CachedPlayerHandler())
                    .GetAsync<Player>("http://localhost:9000/player/1");
                Assert.AreEqual("Matthew", player.Name);
            }
        }
    }

    class CachedPlayerHandler
        : RestHandler<GetRequest<Unit, Player>, GetResponse<Player>, Player>
    {
        public override Task<GetResponse<Player>> Handle(GetRequest<Unit, Player> message)
        {
            return Task.FromResult(new GetResponse<Player>(new Player { Name = "Matthew" }));
        }
    }

    public class Player : Request.WithResponse<Player>
    {
        public int    Id   { get; set; }

        public string Name { get; set; }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other))
                return true;

            var otherPlayer = other as Player;
            return otherPlayer != null
                   && Id == otherPlayer.Id
                   && Equals(Name, otherPlayer.Name);
        }

        public override int GetHashCode()
        {
            return (Name?.GetHashCode() ?? 0) * 31 + Id;
        }
    }

    public class ValidPlayerGet : AbstractValidator<GetRequest<Player, Player>>
    {
        public ValidPlayerGet()
        {
            RuleFor(gp => gp.Resource.Id).GreaterThan(0);
        }    
    }

    public class PlayersController : ApiController
    {
        [HttpGet, Route("player/{id}")]
        public Player GetPlayer(int id)
        {
            return new Player { Id = id, Name = "Ronaldo" };
        }

        [HttpPost, Route("player")]
        public Player CreatePlayer(Player player)
        {
            player.Id = 1;
            return player;
        }

        [HttpPut, Route("player")]
        public Player UpdatePlayer(Player player)
        {
            return player;
        }

        [HttpDelete, Route("player/{id}")]
        public void DeletePlayer(int id)
        {
        }
    }
}
