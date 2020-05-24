namespace AbstractBuilder
{
    using System;
    using System.Drawing;
    using System.Dynamic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AbstractBuilder.Sample;
    using ExpectedObjects;
    using Xunit;

    public class AbstractBuilderTests
    {
        [Fact]
        public void Build_DefaultSeed_BuildsDefaultObject()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Car.DefaultColor,
                NumDoors = default(int),
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_OneModification_BuildsAppliesModification()
        {
            // Arrange
            AbstractBuilder<Car> builder = new AbstractBuilder<Car>(() => new Car())
                .Set(x => x.Color = Color.Red.Name);

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Color.Red.Name,
                NumDoors = default(int),
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_MoreThanOneModifications_BuildsAppliesIncrementalModification()
        {
            // Arrange
            AbstractBuilder<Car> builder = new AbstractBuilder<Car>(() => new Car())
                .Set(x => x.Color = Color.Red.Name, x => x.Color = Color.Blue.Name);

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Color.Blue.Name,
                NumDoors = default(int),
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Set_NullModifications_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                builder.Set(null);
            });
        }

        [Fact]
        public void Set_EmptyModifications_ReturnsSimilarBuilder()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                builder.Set();
            });
        }

        [Fact]
        public void Set_BuilderWithoutSeedCtor_ThrowsMissingMethodException()
        {
            // Arrange
            var builder = new BuilderWithoutSeedCtor();

            // Act & Assert
            Assert.Throws<MissingMethodException>(() =>
            {
                builder.Set(x => x.NumDoors = 5);
            });
        }

        [Fact]
        public async Task BuildAsync_SecuentiallyBuilded_FollowsTheProperOrder()
        {
            // Arrange
            const int ListSize = 100;
            var builder = new AbstractBuilder<int[]>(() => new int[ListSize]);

            foreach (int index in Enumerable.Range(0, ListSize))
            {
                builder = builder.Set(x => x[index] = index);
            }

            // Act
            int[] actual = await builder.BuildAsync();

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(0, actual[0]);

            foreach (int index in Enumerable.Range(1, ListSize - 1))
            {
                Assert.Equal(expected: actual[index - 1] + 1, actual: actual[index]);
            }
        }

        [Fact]
        public async Task BuildAsync_Cancelled_ThrowsException()
        {
            // Arrange
            var builder = new AbstractBuilder<object>(() => new object());

            var cancellationToken = new CancellationToken(true);

            // Act
            await Assert.ThrowsAsync<TaskCanceledException>(() => builder.BuildAsync(cancellationToken));
        }

        [Fact]
        public async Task BuildAsync_CancelledDuringSeedStep_ThrowsException()
        {
            // Arrange
            var cnclTknSource = new CancellationTokenSource();

            bool isCreated = false;
            var builder = new AbstractBuilder<object>(() =>
            {
                cnclTknSource.Cancel();
                isCreated = true;
                return new object();
            });

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => builder.BuildAsync(cnclTknSource.Token));
            Assert.True(isCreated);
        }

        [Fact]
        public async Task BuildAsync_CancelledDuringSetStep_ThrowsException()
        {
            // Arrange
            var cnclTknSource = new CancellationTokenSource();

            bool isCreated = false;
            bool isModified = false;
            var builder = new AbstractBuilder<object>(() =>
            {
                isCreated = true;
                return new object();
            }).Set(x =>
            {
                cnclTknSource.Cancel();
                isModified = true;
            });

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => builder.BuildAsync(cnclTknSource.Token));
            Assert.True(isCreated);
            Assert.True(isModified);
        }
    }
}
