using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using Moq;
using StackExchange.Redis;
using Cache.Core.Definitions;
using System.Threading.Tasks;
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
            // Act
            var response = redisCache.GetUnderlyingDatabase()
                as IDatabase;

            // Assert
            response.Should().BeSameAs(database.Object);
        }

        [Fact]
        public void GetUnderlyingDatabaseType_ReturnsIDatabaseType()
        {
            // Act
            var response = redisCache.GetUnderlyingDatabaseType();

            // Assert
            response.Should().Be(typeof(IDatabase));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exists_ReturnsCorrectResponse(bool cacheContainsKey)
        {
            // Arrange
            database.Setup(d => d.KeyExists(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(cacheContainsKey);

            // Act
            var response = redisCache.Exists(It.IsAny<string>());

            // Assert
            response.Should().Be(cacheContainsKey);
        }

        [Fact]
        public void Exists_ChecksCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            redisCache.Exists(cacheKey);

            // Assert
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
            // Arrange
            database.Setup(d => d.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(cacheContainsKey);

            // Act
            var response = await redisCache.ExistsAsync(It.IsAny<string>());

            // Assert
            response.Should().Be(cacheContainsKey);
        }

        [Fact]
        public async Task ExistsAsync_ChecksCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            await redisCache.ExistsAsync(cacheKey);

            // Assert
            database.Verify(d => d.KeyExistsAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void GetGeneric_RetrievesFromCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            redisCache.Get<SimpleClass>(cacheKey);

            // Assert
            database.Verify(d => d.StringGet(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void GetGeneric_WhenValueCached_ReturnsDeserializedValue()
        {
            // Arrange
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(serializedValue);

            // Act
            var response = redisCache.Get<SimpleClass>(It.IsAny<string>());

            // Assert
            response.Should().Equals(serializedValue);
        }

        [Fact]
        public void GetGeneric_WhenValueNotCached_ReturnsNull()
        {
            // Arrange
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            // Act
            var response = redisCache.Get<SimpleClass>(It.IsAny<string>());

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void GetGeneric_WhenValueTypeNotCached_ReturnsDefault()
        {
            // Arrange
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            // Act
            var response = redisCache.Get<int>(It.IsAny<string>());

            // Assert
            response.Should().Equals(default(int));
        }

        [Fact]
        public async Task GetAsyncGeneric_RetrievesFromCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            await redisCache.GetAsync<SimpleClass>(cacheKey);

            // Assert
            database.Verify(d => d.StringGetAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task GetAsyncGeneric_WhenValueCached_ReturnsDeserializedValue()
        {
            // Arrange
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(serializedValue);

            // Act
            var response = await redisCache.GetAsync<SimpleClass>(It.IsAny<string>());

            // Assert
            response.Should().Equals(serializedValue);
        }

        [Fact]
        public async Task GetAsyncGeneric_WhenValueNotCached_ReturnsNull()
        {
            // Arrange
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var response = await redisCache.GetAsync<SimpleClass>(It.IsAny<string>());

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task GetAsyncGeneric_WhenValueTypeNotCached_ReturnsDefault()
        {
            // Arrange
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var response = await redisCache.GetAsync<int>(It.IsAny<string>());

            // Assert
            response.Should().Equals(default(int));
        }

        [Fact]
        public void Get_RetrievesFromCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            redisCache.Get(cacheKey, typeof(SimpleClass));

            // Assert
            database.Verify(d => d.StringGet(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void Get_WhenValueCached_ReturnsDeserializedValue()
        {
            // Arrange
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(serializedValue);

            // Act
            var response = redisCache.Get(It.IsAny<string>(), typeof(SimpleClass));

            // Assert
            response.Should().Equals(serializedValue);
        }

        [Fact]
        public void Get_WhenValueNotCached_ReturnsNull()
        {
            // Arrange
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            // Act
            var response = redisCache.Get(It.IsAny<string>(), typeof(SimpleClass));

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void Get_WhenValueTypeNotCached_ReturnsDefault()
        {
            // Arrange
            database.Setup(d => d.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            // Act
            var response = redisCache.Get(It.IsAny<string>(), typeof(int));

            // Assert
            response.Should().Equals(default(int));
        }

        [Fact]
        public async Task GetAsync_RetrievesFromCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            await redisCache.GetAsync(cacheKey, typeof(SimpleClass));

            // Assert
            database.Verify(d => d.StringGetAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task GetAsync_WhenValueCached_ReturnsDeserializedValue()
        {
            // Arrange
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(serializedValue);

            // Act
            var response = await redisCache.GetAsync(It.IsAny<string>(), typeof(SimpleClass));

            // Assert
            response.Should().Equals(serializedValue);
        }

        [Fact]
        public async Task GetAsync_WhenValueNotCached_ReturnsNull()
        {
            // Arrange
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var response = await redisCache.GetAsync(It.IsAny<string>(), typeof(SimpleClass));

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_WhenValueTypeNotCached_ReturnsDefault()
        {
            // Arrange
            database.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var response = await redisCache.GetAsync(It.IsAny<string>(), typeof(int));

            // Assert
            response.Should().Equals(default(int));
        }

        [Fact]
        public void Set_SerializesObject()
        {
            // Arrange
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);

            // Act
            redisCache.Set(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            // Assert
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
            // Arrange
            var instance = fixture.Create<int>();
            var serializedValue = SerializeObject(instance);

            // Act
            redisCache.Set(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            // Assert
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
            // Arrange
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            // Act
            redisCache.Set(cacheKey, It.IsAny<RedisValue>(), ttl);

            // Assert
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
            // Arrange
            var instance = fixture.Create<SimpleClass>();
            var serializedValue = SerializeObject(instance);

            // Act
            await redisCache.SetAsync(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            // Assert
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
            // Arrange
            var instance = fixture.Create<int>();
            var serializedValue = SerializeObject(instance);

            // Act
            await redisCache.SetAsync(It.IsAny<string>(), instance, It.IsAny<TimeSpan>());

            // Assert
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
            // Arrange
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            // Act
            await redisCache.SetAsync(cacheKey, It.IsAny<RedisValue>(), ttl);

            // Assert
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
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            redisCache.Remove(cacheKey);

            // Assert
            database.Verify(d => d.KeyDelete(
                It.Is<RedisKey>(s => s == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task RemoveAsync_DeletesFromCache()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();

            // Act
            await redisCache.RemoveAsync(cacheKey);

            // Assert
            database.Verify(d => d.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public void SetExpirationTime_SetsExpirationForKey()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            // Act
            redisCache.SetExpirationTime(cacheKey, ttl);

            // Assert
            database.Verify(d => d.KeyExpire(
                It.Is<RedisKey>(k => k == (RedisKey)cacheKey),
                It.Is<TimeSpan>(t => t == ttl),
                It.Is<CommandFlags>(c => c == CommandFlags.None)
                ));
        }

        [Fact]
        public async Task SetExpirationTimeAsync_SetsExpirationForKey()
        {
            // Arrange
            var cacheKey = fixture.Create<string>();
            var ttl = fixture.Create<TimeSpan>();

            // Act
            await redisCache.SetExpirationTimeAsync(cacheKey, ttl);

            // Assert
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
