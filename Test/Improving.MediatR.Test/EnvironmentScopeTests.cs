using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Improving.MediatR.Environment;

namespace Improving.MediatR.Tests
{
    [TestClass]
    public class EnvironmentScopeTests
    {
        [TestMethod]
        public void Should_Create_Ambient_EnvironmentScope()
        {
            using (new EnvironmentScope())
            {
            }
        }

        [TestMethod]
        public void Should_Add_Items_To_Ambient_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                envScope.Add(cache);
                Assert.AreSame(cache, envScope.Get(typeof(Cache)));
            }
        }

        [TestMethod]
        public void Should_Verify_Items_In_Ambient_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                Assert.IsFalse(envScope.Contains(typeof(Cache)));
                envScope.Add(cache);
                Assert.IsTrue(envScope.Contains(typeof(Cache)));
            }
        }

        [TestMethod]
        public void Should_Verify_Array_In_Ambient_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                Assert.IsFalse(envScope.Contains(typeof(Cache[])));
                envScope.Add(cache);
                Assert.IsTrue(envScope.Contains(typeof(Cache[])));
            }
        }

        [TestMethod]
        public void Should_Verify_List_In_Ambient_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                Assert.IsFalse(envScope.Contains(typeof(List<Cache>)));
                envScope.Add(cache);
                Assert.IsTrue(envScope.Contains(typeof(List<Cache>)));
            }
        }

        [TestMethod]
        public void Should_Retrieve_Arrays_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache1 = new Cache();
                var cache2 = new Cache();
                envScope.Add(cache1, cache2);
                var caches = (Cache[])envScope.Get(typeof(Cache[]));
                Assert.AreEqual(2, caches.Length);
                CollectionAssert.Contains(caches, cache1);
                CollectionAssert.Contains(caches, cache1);
            }
        }

        [TestMethod]
        public void Should_Retrieve_Collections_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache1 = new Cache();
                var cache2 = new Cache();
                envScope.Add(cache1, cache2);
                var caches = ((IEnumerable<Cache>)envScope.Get(typeof(IEnumerable<Cache>))).ToArray();
                Assert.AreEqual(2, caches.Length);
                CollectionAssert.Contains(caches, cache1);
                CollectionAssert.Contains(caches, cache1);
            }
        }

        [TestMethod]
        public void Should_Retrieve_Lists_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache1 = new Cache();
                var cache2 = new Cache();
                envScope.Add(cache1, cache2);
                var caches = (List<Cache>) envScope.Get(typeof (List<Cache>));
                Assert.AreEqual(2, caches.Count);
                Assert.IsTrue(caches.Contains(cache1));
                Assert.IsTrue(caches.Contains(cache2));
            }
        }

        [TestMethod]
        public void Should_Override_With_Nested_EnvironmentScope()
        {
            using (var outerScope = new EnvironmentScope())
            {
                var cacheOuter = new Cache();
                outerScope.Add(cacheOuter);
                using (var innerScope = new EnvironmentScope())
                {
                    var cacheInner = new Cache();
                    innerScope.Add(cacheInner);
                    Assert.AreSame(cacheInner, innerScope.Get(typeof(Cache)));
                }
                Assert.AreSame(cacheOuter, outerScope.Get(typeof(Cache)));
            }
        }

        [TestMethod]
        public void Should_Combine_Array_With_Nested_EnvironmentScope()
        {
            using (var outerScope = new EnvironmentScope())
            {
                var cacheOuter = new Cache();
                outerScope.Add(cacheOuter);
                using (var innerScope = new EnvironmentScope())
                {
                    var cacheInner = new Cache();
                    innerScope.Add(cacheInner);
                    var caches = (Cache[])innerScope.Get(typeof(Cache[]));
                    Assert.AreEqual(2, caches.Length);
                    CollectionAssert.Contains(caches, cacheInner);
                    CollectionAssert.Contains(caches, cacheOuter);
                }
                var cachesOuter = (Cache[])outerScope.Get(typeof(Cache[]));
                Assert.AreEqual(1, cachesOuter.Length);
                CollectionAssert.Contains(cachesOuter, cacheOuter);
            }
        }

        [TestMethod]
        public void Should_Combine_List_With_Nested_EnvironmentScope()
        {
            using (var outerScope = new EnvironmentScope())
            {
                var cacheOuter = new Cache();
                outerScope.Add(cacheOuter);
                using (var innerScope = new EnvironmentScope())
                {
                    var cacheInner = new Cache();
                    innerScope.Add(cacheInner);
                    var caches = (List<Cache>)innerScope.Get(typeof(List<Cache>));
                    Assert.AreEqual(2, caches.Count);
                    Assert.IsTrue(caches.Contains(cacheInner));
                    Assert.IsTrue(caches.Contains(cacheOuter));
                }
                var cachesOuter = (List<Cache>)outerScope.Get(typeof(List<Cache>));
                Assert.AreEqual(1, cachesOuter.Count);
                Assert.IsTrue(cachesOuter.Contains(cacheOuter));
            }
        }

        [TestMethod]
        public void Should_Dispose_Ambient_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                envScope.Add(cache);
                Assert.AreSame(cache, envScope.Get(typeof(Cache)));
            }

            using (var envScope = new EnvironmentScope())
            {
                Assert.IsNull(envScope.Get(typeof(Cache)));
            }
        }

        [TestMethod]
        public void Should_Accept_Use_If_Within_Environment_Scope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                Env.Use(cache);
                Assert.AreSame(cache, envScope.Get(typeof(Cache)));
            }
        }

        [TestMethod,
        ExpectedException(typeof(InvalidOperationException))]
        public void Should_Reject_Use_If_Not_Within_Environment_Scope()
        {
            Env.Use(new Cache());
        }

        [TestMethod]
        public void Should_Get_Key_Value_From_Ambient_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                envScope["MyCache"] = cache;
                Assert.AreSame(cache, envScope.GetKey<Cache>("MyCache"));
            }
        }

        [TestMethod]
        public void Should_Get_Key_Value_From_Parent_EnvironmentScope()
        {
            using (var outerScope = new EnvironmentScope())
            {
                var cacheOuter = new Cache();
                outerScope["MyCache"] = cacheOuter;
                using (var innerScope = new EnvironmentScope())
                {
                    var cacheInner = new Cache();
                    innerScope["MyCache"] = cacheInner;
                    Assert.AreSame(cacheInner, innerScope.GetKey<Cache>("MyCache"));
                }
                Assert.AreSame(cacheOuter, outerScope.GetKey<Cache>("MyCache"));
            }
        }

        [TestMethod]
        public void Should_Clear_Item_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                envScope.Clear(typeof(Cache));
                Assert.AreSame(EnvironmentScope.Null, envScope.Get(typeof(Cache)));
            }
        }

        [TestMethod]
        public void Should_Clear_Contravariantly_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                envScope.Clear(typeof(LRUCache));
                Assert.AreSame(EnvironmentScope.Null, envScope.Get(typeof(Cache)));
            }

            using (var envScope = new EnvironmentScope())
            {
                envScope.Clear(typeof(Cache));
                Assert.IsNull(envScope.Get(typeof(LRUCache)));
            }
        }

        [TestMethod]
        public void Should_Prefer_Item_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new LRUCache();
                envScope.Add(cache);
                envScope.Clear(typeof (Cache));
                Assert.AreSame(cache, envScope.Get(typeof(Cache)));
            }
        }

        [TestMethod]
        public void Should_Clear_If_Remove_Item_From_EnvironmentScope()
        {
            using (var envScope = new EnvironmentScope())
            {
                var cache = new Cache();
                envScope.Add(cache);
                envScope.Remove(cache);
                envScope.Clear(typeof(Cache));
                Assert.AreSame(EnvironmentScope.Null, envScope.Get(typeof(Cache)));
            }
        }

        class Cache
        {
        }

        class LRUCache : Cache
        {     
        }
    }
}
