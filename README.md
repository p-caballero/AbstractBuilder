# AbstractBuilder

NuGet that contains a builder pattern for testing in C#

## How to add it

Just install the NuGet **AbstractBuilder**. It doesn't have any kind of external dependencies.

    dotnet add package AbstractBuilder

Or just go to NuGet web page: https://www.nuget.org/packages/AbstractBuilder/

## How does it works

We create a default builder and we request modifications to that builder, for every request we have a **new builder** (with all the previous modifications and the new ones). Finally when we call to Build method we create the object.

```csharp

    MyBuilder builder = new MyBuilder()
        .WithArms(2)
        .WithLegs(2);

    Person john = builder.WithName("John").Build();
    Person peter = builder.WithName("Peter").Build();

```

In the previous example we share 2 modifications and we create 2 instances of the class `Person`.

## How can I use it? - Way 1: Heritage

You just have to inherit from **AbstractBuilder** class where the generic type is the result. The build steps can be as simple as this example or as complex as you want.

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

Your builder needs a public constructor (in the example it is the default constructor) and it always needs a constructor with the seed (the visibility is not important).
In this example we can set the property "*Name*" with the method `WithName`. We way of creating new methods that affects to the builder should be using always the method `Set`.

## How can I use it? - Way 2: Using the abstract builder itself

I don't want to be restrictive so you can use it directly. The problem is that you will have to declare everything and you will not reuse the builder.

```csharp

    var builder = new AbstractBuilder<Person>()
        .Set(x => x.Arms = 2)
        .Set(x => x.Legs = 2);

    Person john = builder.Set(x => x.Name = "John").Build();
    Person peter = builder.Set(x => x.Name = "Peter").Build();

```

## Could we aggregate some modifications in one call?

Yes, we can. We can call multiple times to the lambda action or surround with brackets.

```csharp

    var builder1 = new AbstractBuilder<Person>().Set(x => x.Arms = 2, x => x.Legs = 2)

    var builder2 = new AbstractBuilder<Person>().Set(x => {
        x.Arms = 2;
        x.Legs = 2;
    });

```

## Asyncronous build

The process is same but with the syncronous method BuildAsync. The cancellation token can be accesible passing the argument of type BuilderContext.

```csharp

    var builderContext = new BuilderContext() { CancellationToken = myCancellationToken };

    var builder = new AbstractBuilder<Person>()
        .Set((x, ctx) => x.Arms = 2)
        .Set((x, ctx) => x.Legs = 2);

    Person john = await builder.Set(x => x.Name = "John").BuildAsync(builderContext);
    Person peter = await builder.Set(x => x.Name = "Peter").BuildAsync(builderContext);

```

## BuilderContext

You can inherit from **BuilderContext** to pass you own arguments if you need them apart form the **CancellationToken**.

When there are more than one constructor in our builder, the priority is for the constructor with the BuilderContext.

You can use indistinctly the different version of the method **Set**, the builder internally converts them into the same operation. If you don't use the version with the context then it will not be accessibe just for that method.

```csharp

    var builderContext = new MyBuilderContext() { Multiplier = 5 };

    var builder = new AbstractBuilder<Person>()
        .Set((x, ctx) => x.Arms = 2 * ((MyBuilderContext)ctx).Multiplier)
        .Set(x => x.Legs = 2);

    Person mutantJohn = await builder.Set(x => x.Name = "John").BuildAsync(builderContext);
    Person mutantPeter = await builder.Set(x => x.Name = "Peter").BuildAsync(builderContext);

```

## Builder for records

[Records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) cannot follow this genenic builder pattern due to the properties are read-only after the creation of the object. If you want to follow a similar approach you should use `RecordBuilder`.

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

Using the method `Set` we can add the value for the selected parameter. If you don't provide a value then it will try to use the default value of that parameter and in the worst case, the default value for that type.

This builder works with **record class** and **record struct**.

```csharp

    var builder = new PointBuilder()
        .WithCoordinateAlpha();

    Point alpha = builder.Build();
    Point beta = builder.Set<PointBuilder>(x => x.Z, () => 10).Build();
    Point charlie = builder.Set<PointBuilder>(x => x.Z, () => 20).Build();

```

In the previous example, alpha was _(10,20,0)_, beta was _(10,20,10)_ and charlie was _(10,20,20)_.