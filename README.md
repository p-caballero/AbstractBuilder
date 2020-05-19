# AbstractBuilder

NuGet that contains a builder pattern for testing in C#

## How to add it

Just install the NuGet **AbstractBuilder**. It doesn't have any kind of external dependencies.

    dotnet add package AbstractBuilder

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

        // MANDATORY CONSTRUCTOR
        protected MyBuilder(Func<Person> seedFunc) : base(seedFunc)
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

## How can I use it? - Way 2: Using the abstract class itself

I don't want to be restrictive so you can use it directly. The problem is that you will have to declare everything and you will not reuse the builder.

```csharp

    var builder = new AbstractClass<Person>()
        .Set(x => x.Arms = 2)
        .Set(x => x.Legs = 2);

    Person john = builder.Set(x => x.Name = "John").Build();
    Person peter = builder.Set(x => x.Name = "Peter").Build();

```

## Could we aggregate some modifications in one call?

Yes, we can. We can call multiple times to the lambda action or surround with brackets.

```csharp

    var builder1 = new AbstractClass<Person>().Set(x => x.Arms = 2, x => x.Legs = 2)

    var builder2 = new AbstractClass<Person>().Set(x => {
        x.Arms = 2;
        x.Legs = 2;
    });

```
