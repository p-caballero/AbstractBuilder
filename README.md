# AbstractBuilder

A NuGet package that contains a builder pattern for testing in C#.

## How to add it

To use this package, simply install the NuGet **AbstractBuilder**. It has no external dependencies.

    dotnet add package AbstractBuilder

Alternatively, you can visit the NuGet webpage for **AbstractBuilder**: https://www.nuget.org/packages/AbstractBuilder/.

## Getting started

When you have to create a test, you ofen reuse objects with the same information. Instead of copying and pasting, you can create a method to build those test objects multiple times.

To simplify this process and follow the principle of single responsibility, builders are used. The classical approach is that one builder contains only one object. However, **AbstractBuilder** modifies the pattern to create a new builder every time a modification is added, and the final object is not created until the `Build` method is called.

Consequently, a builder from AbstractBuilder can be declared in your test class only once and reused in all the tests you need.

```csharp

    MyBuilder builder = new MyBuilder()
        .WithArms(2)
        .WithLegs(2);

    Person john = builder.WithName("John").Build();
    Person peter = builder.WithName("Peter").Build();

```

In the previous example, we make two changes and create two `Person` objects.

## How can I use it? - Way 1: Heritage

To use the `AbstractBuilder` class, you need to inherit from it and specify the generic type as the result. The build steps can vary in complexity, as shown in this example.

```csharp

    public MyBuilder : AbstractBuilder<Person>
    {
        public MyBuilder WithName(string name)
        {
            return Set<MyBuilder>(x => x.Name = name);
        }

        protected override Person CreateDefault()
        {
            return new Person {
                IsAlive = true
            };
        }
    }

```

## How can I use it? - Way 2: Heritage (before version 1.7.0)

In previous versions of AbstractBuilder, the mandatory constructor should be provided, regardless of its visibility. The current version is backward compatible, and you can still use it. However, it is recommended to move to _Way 1_ to simplify your builders.

Your builder requires a public constructor (the default constructor in the example) and another constructor with the seed (the visibility does not matter). In this example, we can set the "*Name*" property with the `WithName` method. We should always use the `Set<>` method to create new methods that modify the builder.

```csharp

    public MyBuilder : AbstractBuilder<Person>
    {
        // DEFAULT CONSTRUCTOR
        public MyBuilder() : this(CreateDefaultValue)
        {
        }

        // MANDATORY CONSTRUCTOR (it can be private or protected)
        private MyBuilder(Func<Person> seedFunc) : base(seedFunc)
        {
        }

        public MyBuilder WithName(string name)
        {
            return Set<MyBuilder>(x => x.Name = name);
        }
        
        private static Result CreateDefaultValue()
        {
            return new Person {
                IsAlive = true
            };
        }
    }
```

## How can I use it? - Way 3: Using the abstract builder itself

You can use it directly without any restrictions. However, you will need to declare everything and you cannot reuse the builder.

```csharp

    var builder = new AbstractBuilder<Person>()
        .Set(x => x.Arms = 2)
        .Set(x => x.Legs = 2);

    Person john = builder.Set(x => x.Name = "John").Build();
    Person peter = builder.Set(x => x.Name = "Peter").Build();

```

## Can we aggregate some modifications in one call?

We can do either of these: call the lambda action multiple times or use brackets.

```csharp

    var builder1 = new AbstractBuilder<Person>().Set(x => x.Arms = 2, x => x.Legs = 2)

    var builder2 = new AbstractBuilder<Person>().Set(x => {
        x.Arms = 2;
        x.Legs = 2;
    });

```

## Asyncronous build

The process is the same, but with the synchronous method `BuildAsync`. The cancellation token can be accessed by passing an argument of type `BuilderContext`.

```csharp

    var builderContext = new BuilderContext() { CancellationToken = myCancellationToken };

    var builder = new AbstractBuilder<Person>()
        .Set((x, ctx) => x.Arms = 2)
        .Set((x, ctx) => x.Legs = 2);

    Person john = await builder.Set(x => x.Name = "John").BuildAsync(builderContext);
    Person peter = await builder.Set(x => x.Name = "Peter").BuildAsync(builderContext);

```

## BuilderContext

You can inherit from `BuilderContext` to pass your own arguments, besides the `CancellationToken`, if you need them.

If your builder has multiple constructors, the one with the `BuilderContext` parameter has the highest priority.

You can use any version of the `Set` method interchangeably, as the builder internally converts them to the same operation. However, if you omit the context parameter, you will not be able to access it for that method.

```csharp

    var builderContext = new MyBuilderContext() { Multiplier = 5 };

    var builder = new AbstractBuilder<Person>()
        .Set((x, ctx) => x.Arms = 2 * ((MyBuilderContext)ctx).Multiplier)
        .Set(x => x.Legs = 2);

    Person mutantJohn = await builder.Set(x => x.Name = "John").BuildAsync(builderContext);
    Person mutantPeter = await builder.Set(x => x.Name = "Peter").BuildAsync(builderContext);

```

## Builder for records

[Records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) cannot use this generic builder pattern because their properties are read-only after the object is created. To achieve a similar result, you should use `RecordBuilder`.

```csharp
    public record Point(double X, double Y, double Z); 

    public PointBuilder : RecordBuilder<Point>
    {
        public PointBuilder WithCoordinateAlpha()
        {
            return Set<PointBuilder>(p => p.X, () => 10)
                  .Set<PointBuilder>(p => p.Y, () => 20);
        }
    }

```

The `Set` method assigns a value to the chosen parameter. If no value is given, it uses the default value of the parameter or in the worst case, the default value of the type.

This builder supports both record class and record struct.

```csharp

    var builder = new PointBuilder()
        .WithCoordinateAlpha();

    Point alpha = builder.Build();
    Point beta = builder.Set<PointBuilder>(x => x.Z, () => 10).Build();
    Point charlie = builder.Set<PointBuilder>(x => x.Z, () => 20).Build();

```

In the previous example, `alpha` was _(10,20,0)_, `beta` was _(10,20,10)_ and `charlie` was _(10,20,20)_.

## Simplifying tests

### Create a static field

You can create the builder as a static field in your test class and reuse it in your tests.

```csharp
    using AbstractBuilder;
    using Xunit;
    
    public class BookCiteDomainServiceTests
    {
        private static BookCiteBuilder BookCiteBuilder = new();

        public MyTests()
        {
            _service = new BookCiteDomainService();
        }

        [Theory]
        [InlineData("Arthur", "Conan Doyle")]
        [InlineData("Camilo", "José Cela")]
        public void PerformAction_SomeConditions_ExpectedResults(string name, string surname)
        {
            // Arrange
            BookCite bookCite = BookCiteBuilder
                .WithAuthor("Arthur", );

            // Act
            var actual = _service.PerformAction(bookCite);

            // Assert
            // Checking of the expected values
        }

        [Fact]
        public void PerformAction_SomeOtherConditions_OtherExpectedResults()
        {
            // Arrange
            BookCite bookCite = BookCiteBuilder
                .WithAuthor("Arthur", null);

            // Act
            var actual = _service.PerformAction(bookCite);

            // Assert
            // Checking of the expected values
        }
    }
```

### Injecting builders with IClassFixture in xUnit

If you are afraid of adding `new` everywhere in the code, you can use `IClassFixture` to inject it.

```csharp
    using AbstractBuilder;
    using Xunit;
    
    public class BookCiteDomainServiceTests : IClassFixture<BookCiteBuilder>
    {
        private BookCiteBuilder _bookCiteBuilder;

        public MyTests(BookCiteBuilder bookCiteBuilder)
        {
            bookCiteBuilder = _bookCiteBuilder;
            _service = new BookCiteDomainService();
        }

        [Fact]
        public void PerformAction_SomeOtherConditions_OtherExpectedResults()
        {
            // Arrange
            BookCite bookCite = BookCiteBuilder
                .WithAuthor("Arthur", null);

            // Act
            var actual = _service.PerformAction(bookCite);

            // Assert
            // Checking of the expected values
        }
    }
```

---

**_Enjoy t3st1ng!_**
