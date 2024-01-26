namespace AbstractBuilder
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AbstractBuilder.Examples.Builders;
    using AbstractBuilder.Examples.Entities;
    using ExpectedObjects;
    using Xunit;
    using CtrType = Examples.Builders.BuilderWithBothCtors.CtrType;

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
        public void Build_CancelledBefore_ThrowsOperationCanceledException()
        {
            // Arrange
            var builder = new AbstractBuilder<object>(() => new object());

            var builderContext = new BuilderContext
            {
                CancellationToken = new CancellationToken(true)
            };

            // Act
            Assert.Throws<OperationCanceledException>(() => builder.Build(builderContext));
        }

        [Fact]
        public void Build_CancelledDuringSeedStepWithoutSet_DoesntDetectCancellation()
        {
            // Arrange
            var cnclTknSource = new CancellationTokenSource();

            var builderContext = new BuilderContext
            {
                CancellationToken = cnclTknSource.Token
            };

            bool isCreated = false;
            var builder = new AbstractBuilder<object>(() =>
            {
                cnclTknSource.Cancel();
                isCreated = true;
                return new object();
            });

            // Act
            builder.Build(builderContext);
            Assert.True(isCreated);
        }

        [Fact]
        public void Build_CancelledDuringSetStep_DoesntDetectCancellation()
        {
            // Arrange
            var cnclTknSource = new CancellationTokenSource();

            var builderContext = new BuilderContext
            {
                CancellationToken = cnclTknSource.Token
            };

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
            builder.Build(builderContext);
            Assert.True(isCreated);
            Assert.True(isModified);
        }

        [Fact]
        public void Set_NullModifications_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                builder.Set((Action<Car>[])null);
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
                builder.Set(Array.Empty<Action<Car>>());
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
        public async Task BuildAsync_CancelledBefore_ThrowsOperationCanceledException()
        {
            // Arrange
            var builder = new AbstractBuilder<object>(() => new object());

            var builderContext = new BuilderContext
            {
                CancellationToken = new CancellationToken(true)
            };

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => builder.BuildAsync(builderContext));
        }

        [Fact]
        public async Task BuildAsync_CancelledDuringSeedStepWithoutSet_DoesntDetectCancellation()
        {
            // Arrange
            var cnclTknSource = new CancellationTokenSource();

            var builderContext = new BuilderContext
            {
                CancellationToken = cnclTknSource.Token
            };

            bool isCreated = false;
            var builder = new AbstractBuilder<object>(() =>
            {
                cnclTknSource.Cancel();
                isCreated = true;
                return new object();
            });

            // Act
            await builder.BuildAsync(builderContext);
            Assert.True(isCreated);
        }

        [Fact]
        public async Task BuildAsync_CancelledDuringSetStep_DoesntDetectCancellation()
        {
            // Arrange
            var cnclTknSource = new CancellationTokenSource();

            var builderContext = new BuilderContext
            {
                CancellationToken = cnclTknSource.Token
            };

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
            await builder.BuildAsync(builderContext);
            Assert.True(isCreated);
            Assert.True(isModified);
        }

        [Fact]
        public void Ctor_OnlyCtor_HeritageCall()
        {
            // Arrange
            BuilderWithBothCtors.Ctors.Clear();

            // Act
            var actual = new BuilderWithBothCtors();

            // Asset
            Assert.NotNull(actual);
            Assert.Equal(new[] { CtrType.SeedNoCtx, CtrType.EmptyNoCtx }, BuilderWithBothCtors.Ctors);
        }
        
        [Fact]
        public void Ctor_SetNoCtxSetNoCtx_SetUsesSeedCtx()
        {
            // Arrange
            BuilderWithBothCtors.Ctors.Clear();

            // Act
            var actual = new BuilderWithBothCtors()
                .Set(x => x.Model = FerraryModels.Ferrari208Gts)
                .Set(x => x.Color = Color.Red.Name);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(new[] { CtrType.SeedNoCtx, CtrType.EmptyNoCtx, CtrType.SeedCtx, CtrType.SeedCtx}, BuilderWithBothCtors.Ctors);
        }

        [Fact]
        public void Ctor_SetCtxSetCtx_SetUsesSeedCtx()
        {
            // Arrange
            BuilderWithBothCtors.Ctors.Clear();

            // Act
            var actual = new BuilderWithBothCtors()
                .Set((x, context) => x.Model = FerraryModels.Ferrari208Gts)
                .Set((x, context) => x.Color = Color.Red.Name);

            // Asset
            Assert.NotNull(actual);
            Assert.Equal(new[] { CtrType.SeedNoCtx, CtrType.EmptyNoCtx, CtrType.SeedCtx, CtrType.SeedCtx}, BuilderWithBothCtors.Ctors);
        }
    }
}
