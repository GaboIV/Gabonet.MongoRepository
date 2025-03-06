using Xunit;
using Moq;
using MongoDB.Driver;

namespace Gabonet.MongoRepository.Tests
{
    public class MongoRepositoryTests
    {
        private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly MongoRepository<TestEntity> _repository;

        public MongoRepositoryTests()
        {
            _mockCollection = new Mock<IMongoCollection<TestEntity>>();
            _mockDatabase = new Mock<IMongoDatabase>();

            _mockDatabase.Setup(db => db.GetCollection<TestEntity>(It.IsAny<string>(), null))
                         .Returns(_mockCollection.Object);

            _repository = new MongoRepository<TestEntity>(_mockDatabase.Object, "TestCollection");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoDocumentsExist()
        {
            // Arrange
            var asyncCursor = new Mock<IAsyncCursor<TestEntity>>();
            asyncCursor.Setup(_ => _.Current).Returns(new List<TestEntity>());
            asyncCursor.SetupSequence(_ => _.MoveNext(It.IsAny<System.Threading.CancellationToken>()))
                       .Returns(false);

            _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<TestEntity>>(), 
                It.IsAny<FindOptions<TestEntity>>(), 
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(asyncCursor.Object);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldCallInsertOneAsync()
        {
            // Arrange
            var entity = new TestEntity { Id = "123", Name = "Test Name" };

            // Act
            await _repository.AddAsync(entity);

            // Assert
            _mockCollection.Verify(x => x.InsertOneAsync(entity, null, default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteOneAsync()
        {
            // Arrange
            var id = "123";
            var filter = Builders<TestEntity>.Filter.Eq("_id", id);

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<TestEntity>>(), default), Times.Once);
        }
    }

    // Clase de ejemplo para representar una entidad
    public class TestEntity
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
