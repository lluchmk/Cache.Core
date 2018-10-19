using AutoFixture;
using AutoFixture.AutoMoq;
using Cache.Core.Definitions;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;
using static Newtonsoft.Json.JsonConvert;

namespace Cache.Core.Tests.Implementations
{
    public class RedisCacheTests
    {
        private IFixture fixture;

        private Mock<IDatabase> database = new Mock<IDatabase>();

        private RedisCache redisCache;

        public RedisCacheTests()
        {
            fixture = new Fixture()
                .Customize(new AutoMoqCustomization());

            database = fixture.Freeze<Mock<IDatabase>>();

            redisCache = fixture.Create<RedisCache>();
        }

        [Fact]
        public void GetUnderlyingDatabase_ReturnsIDatabaseInstance()
        {
            var response = redisCache.GetUnderlyingDatabase()
                as IDatabase;

            response.Should().BeSameAs(database.Object);
        }

        [Fact]
        public void GetUnderlyingDatabaseType_ReturnsIDatabaseType()
        {
            var response = redisCache.GetUnderlyingDatabaseType();

            response.Should().Be(typeof(IDatabase));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exists_ReturnsCorrectResponse(bool cacheContainsKey)
        {
            database.Setup(d => d.KeyExists(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(cacheContainsKey);

            var response = redisCache.Exists(It.IsAny<string>());

            response.Should().Be(cacheContainsKey);
        }

        [Fact]
        public void Exists_ChecksCache()
        {
            var cacheKey = fixture.Create<string>();

            redisCache.Exists(cacheKey);

            database.Verify(d => d.KeyExists(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExistsAsync_ReturnsCorrectResponse(bool cacheContainsKey)
        {
            database.Setup(d => d.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(cacheContainsKey);

            var response = await redisCache.ExistsAsync(It.IsAny<string>());

            response.Should().Be(cacheContainsKey);
        }

        [Fact]
        public async Task ExistsAsync_ChecksCache()
        {
            var cacheKey = fixture.Create<string>();

            await redisCache.ExistsAsync(cacheKey);

            database.Verify(d => d.KeyExistsAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void GetGeneric_RetrievesFromCache()
        {
            var cacheKey = fixture.Create<string>();

            redisCache.Get<SimpleClass>(cacheKey);

            database.Verify(d => d.StringGet(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void GetGeneric_WhenValueCached_ReturnsDeserializedValue()
        {
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(serializedValue);

            var response = redisCache.Get<SimpleClass>(It.IsAny<string>());

            response.Should().Equals(serializedValue);
        }

        [Fact]
        public void GetGeneric_WhenValueNotCached_ReturnsNull()
        {
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            var response = redisCache.Get<SimpleClass>(It.IsAny<string>());

            response.Should().BeNull();
        }

        [Fact]
        public void GetGeneric_WhenValueTypeNotCached_ReturnsDefault()
        {
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            var response = redisCache.Get<int>(It.IsAny<string>());

            response.Should().Equals(default(int));
        }

        [Fact]
        public async Task GetAsyncGeneric_RetrievesFromCache()
        {
            var cacheKey = fixture.Create<string>();

            await redisCache.GetAsync<SimpleClass>(cacheKey);

            database.Verify(d => d.StringGetAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task GetAsyncGeneric_WhenValueCached_ReturnsDeserializedValue()
        {
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(serializedValue);

            var response = await redisCache.GetAsync<SimpleClass>(It.IsAny<string>());

            response.Should().Equals(serializedValue);
        }

        [Fact]
        public async Task GetAsyncGeneric_WhenValueNotCached_ReturnsNull()
        {
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var response = await redisCache.GetAsync<SimpleClass>(It.IsAny<string>());

            response.Should().BeNull();
        }

        [Fact]
        public async Task GetAsyncGeneric_WhenValueTypeNotCached_ReturnsDefault()
        {
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var response = await redisCache.GetAsync<int>(It.IsAny<string>());

            response.Should().Equals(default(int));
        }

        [Fact]
        public void Get_RetrievesFromCache()
        {
            var cacheKey = fixture.Create<string>();

            redisCache.Get(cacheKey, typeof(SimpleClass));

            database.Verify(d => d.StringGet(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void Get_WhenValueCached_ReturnsDeserializedValue()
        {
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(serializedValue);

            var response = redisCache.Get(It.IsAny<string>(), typeof(SimpleClass));

            response.Should().Equals(serializedValue);
        }

        [Fact]
        public void Get_WhenValueNotCached_ReturnsNull()
        {
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            var response = redisCache.Get(It.IsAny<string>(), typeof(SimpleClass));

            response.Should().BeNull();
        }

        [Fact]
        public void Get_WhenValueTypeNotCached_ReturnsDefault()
        {
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            var response = redisCache.Get(It.IsAny<string>(), typeof(int));

            response.Should().Equals(default(int));
        }

        [Fact]
        public async Task GetAsync_RetrievesFromCache()
        {
            var cacheKey = fixture.Create<string>();

            await redisCache.GetAsync(cacheKey, typeof(SimpleClass));

            database.Verify(d => d.StringGetAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task GetAsync_WhenValueCached_ReturnsDeserializedValue()
        {
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(serializedValue);

            var response = await redisCache.GetAsync(It.IsAny<string>(), typeof(SimpleClass));

            response.Should().Equals(serializedValue);
        }

        [Fact]
        public async Task GetAsync_WhenValueNotCached_ReturnsNull()
        {
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var response = await redisCache.GetAsync(It.IsAny<string>(), typeof(SimpleClass));

            response.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_WhenValueTypeNotCached_ReturnsDefault()
        {
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var response = await redisCache.GetAsync(It.IsAny<string>(), typeof(int));

            response.Should().Equals(default(int));
        }

        [Fact]
        public void Set_SerializesObject()
        {
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);

            redisCache.Set(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            database.Verify(d => d.StringSet(
                It.IsAny<RedisKey>(),
                It.Is<RedisValue>(v => v == serializedValue),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
                ));
        }

        [Fact]
        public void Set_SerializesValue()
        {
            var instance = fixture.Create<int>();
            var serializedValue = SerializeObject(instance);

            redisCache.Set(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            database.Verify(d => d.StringSet(
                It.IsAny<RedisKey>(),
                It.Is<RedisValue>(v => v == serializedValue),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
                ));
        }

        [Fact]
        public void Set_SetsCache()
        {
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            redisCache.Set(cacheKey, It.IsAny<RedisValue>(), ttl);

            database.Verify(d => d.StringSet(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.IsAny<RedisValue>(),
                It.Is<TimeSpan>(t => t == ttl),
                It.Is<When>(w => w == When.Always),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task SetAsync_SerializesObject()
        {
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);

            await redisCache.SetAsync(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            database.Verify(d => d.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.Is<RedisValue>(v => v == serializedValue),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
                ));
        }

        [Fact]
        public async Task SetAsync_SerializesValue()
        {
            var instance = fixture.Create<int>();
            var serializedValue = SerializeObject(instance);

            await redisCache.SetAsync(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            database.Verify(d => d.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.Is<RedisValue>(v => v == serializedValue),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
                ));
        }

        [Fact]
        public async Task SetAsync_SetsCache()
        {
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            await redisCache.SetAsync(cacheKey, It.IsAny<RedisValue>(), ttl);

            database.Verify(d => d.StringSetAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.IsAny<RedisValue>(),
                It.Is<TimeSpan>(t => t == ttl),
                It.Is<When>(w => w == When.Always),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void Remove_DeletesFromCache()
        {
            var cacheKey = fixture.Create<string>();

            redisCache.Remove(cacheKey);

            database.Verify(d => d.KeyDelete(
                It.Is<RedisKey>(s => s == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task RemoveAsync_DeletesFromCache()
        {
            var cacheKey = fixture.Create<string>();

            await redisCache.RemoveAsync(cacheKey);

            database.Verify(d => d.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void SetExpirationTime_SetsExpirationForKey()
        {
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            redisCache.SetExpirationTime(cacheKey, ttl);

            database.Verify(d => d.KeyExpire(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<TimeSpan>(t => t == ttl),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task SetExpirationTimeAsync_SetsExpirationForKey()
        {
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            await redisCache.SetExpirationTimeAsync(cacheKey, ttl);

            database.Verify(d => d.KeyExpireAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<TimeSpan>(t => t == ttl),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        private class SimpleClass
        {
            public int Property { get; set; }
        }
    }
}
