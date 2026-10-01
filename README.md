<p align="center">
  <a>
    <img src="https://github.com/CADindustries/container/blob/main/logos/package-custom.png" alt="Abdrakov.Solutions logo" width="256" height="256">
  </a>
</p>
<h1 align="center">Hypocrite.Container</h1>  

#### Pure Container:
[![Nuget](https://img.shields.io/nuget/v/Hypocrite.Container.svg)](http://nuget.org/packages/Hypocrite.Container)
[![Nuget](https://img.shields.io/nuget/dt/Hypocrite.Container.svg)](http://nuget.org/packages/Hypocrite.Container)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/CrackAndDie/Hypocrite.Container/blob/main/LICENSE)

#### Prism Adapter:
[![Nuget](https://img.shields.io/nuget/v/Hypocrite.Container.Prism.svg)](http://nuget.org/packages/Hypocrite.Container.Prism)
[![Nuget](https://img.shields.io/nuget/dt/Hypocrite.Container.Prism.svg)](http://nuget.org/packages/Hypocrite.Container.Prism)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/CrackAndDie/Hypocrite.Container/blob/main/LICENSE)

#### Prism.Avalonia Adapter:
[![Nuget](https://img.shields.io/nuget/v/Hypocrite.Container.AvaloniaPrism.svg)](http://nuget.org/packages/Hypocrite.Container.AvaloniaPrism)
[![Nuget](https://img.shields.io/nuget/dt/Hypocrite.Container.AvaloniaPrism.svg)](http://nuget.org/packages/Hypocrite.Container.AvaloniaPrism)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/CrackAndDie/Hypocrite.Container/blob/main/LICENSE)

<h2>About:</h2>  

Lightning fast mutable/dynamic DI/IOC container. It may be slower than prebuilt/sealed containers in some cases but it is more flexible. 

<h2>Usage:</h2>  

<h3>Manual usage:</h3>  

You can manually use container to register and resolve types when needed:  
```csharp
public class TestClass
{
    public int A { get; set; }
    public string B { get; set; }
}

// somewhere
ILightContainer lightContainer = new LightContainer();
lightContainer.Register<TestClass, TestClass>();

var classInstance = lightContainer.Resolve<TestClass>();
```

<p align="center">
  <a>
    <img width="797" height="302" alt="image" src="https://github.com/user-attachments/assets/9c7a34ec-33d6-4a6b-b898-401a2c6b7d39" />
  </a>
</p>
<h5 align="center">(Benchmark name "PureResolveType")</h5>   

<h3>Manual usage (Singleton):</h3>  

You can manually use container to register and resolve types in singleton scope:  
```csharp
lightContainer.RegisterSingleton<TestClass, TestClass>();
```

<p align="center">
  <a>
    <img width="788" height="304" alt="image" src="https://github.com/user-attachments/assets/a99651f5-d056-4a7c-97ec-d7837685d271" />
  </a>
</p>
<h5 align="center">(Benchmark name "PureResolveSingleton")</h5>   

<h3>Manual usage (Instance):</h3>  

You can manually use container to register and resolve instances:  
```csharp
lightContainer.RegisterInstance<TestClass>(new TestClass());
```

<p align="center">
  <a>
    <img width="789" height="299" alt="image" src="https://github.com/user-attachments/assets/339d1f2c-cb58-4e3d-9fe2-22ffb8dec4a3" />
  </a>
</p>
<h5 align="center">(Benchmark name "PureResolveInstance")</h5>   

<h3>Manual usage (Factory):</h3>  

You can manually use container to register and resolve types via factories:  
```csharp
lightContainer.RegisterFactory<TestClass>((container, type, name) => new TestClass());
```

<p align="center">
  <a>
    <img width="786" height="299" alt="image" src="https://github.com/user-attachments/assets/03416923-aca5-43c0-9c8e-fb9edf2739af" />
  </a>
</p>
<h5 align="center">(Benchmark name "PureResolveFactory")</h5>   

<h3>Attribute injections:</h3>  

All the registered shite could be resolved via *Injection* attribute (use the attribute only for properties and fields) like this:
```csharp
private class NormalClass
{
    [Injection]
    InjectedClass TestClass { get; set; }
    [Injection]
    AnotherInjectedClass _anotherTestClass;
}
```
<p align="center">
  <a>
    <img width="791" height="299" alt="image" src="https://github.com/user-attachments/assets/107a3f68-63c4-48c9-8942-1a9dbdcbfd1d" />
  </a>
</p>
<h5 align="center">(Benchmark name "ResolveTypeWithParamsInj")</h5>  

<h3>Constructor injections:</h3>  

Parametrised constructors could be used with *Hypocrite.Container*. For example after registering and resolving the class  
```csharp
private class NormalClass
{
    private InjectedClass _testClass;
    private int _a;
    private string _b;

    [Injection]
    public NormalClass(InjectedClass testClass, int a, string b = "awd")
    {
        _testClass = testClass;
        _a = a;
        _b = b;
    }
}
```
the *testClass* parameter would be resolved as usual (if it is not registered in the container then an instance of it would be created); the *a* parameter would have **default type** value (for Int32 is 0); the *b* parameter would have its **default parameter** value (in this case is "awd").   

<p align="center">
  <a>
    <img width="786" height="305" alt="image" src="https://github.com/user-attachments/assets/5ca3334c-3f13-4289-b60e-ee95ccb0eb65" />
  </a>
</p>
<h5 align="center">(Benchmark name "ResolveTypeWithCtorInj")</h5>  


<h3>Inheritance injections:</h3>  

The classed from which Your class is inherited would also be prepared for injections:  
```csharp
private class InjectedClass
{
    internal int A { get; set; }
}

private class BaseClass
{
    [Injection]
    protected InjectedClass TestClass { get; set; }
}

private class NormalClass : BaseClass
{
}
```
So in this case after *NormalClass* registration and resolve, the *TestClass* property would also be injected.   

<p align="center">
  <a>
    <img width="782" height="301" alt="image" src="https://github.com/user-attachments/assets/9e2b4a2f-7b82-457f-9d2b-e010b2d8ca52" />
  </a>
</p>
<h5 align="center">(Benchmark name "ResolveTypeWithInheritedInj")</h5>  

<h3>Recursive injections:</h3>  

There could be two classes that require injection of each other:
```csharp
private class FirstClass
{
    [Injection]
    SecondClass InjectedClass { get; set; }
}

private class SecondClass
{
    [Injection]
    FirstClass InjectedClass { get; set; }
}
```
And this would work as expected!  
<p align="center">
  <a>
    <img width="788" height="303" alt="image" src="https://github.com/user-attachments/assets/c515a91d-06a0-4e9b-bd8a-f16d7757effd" />
  </a>
</p>
<h5 align="center">(Benchmark name "ResolveRecursiveInj")</h5>  


## Other   

### Why is it slower than [Stylet](https://github.com/canton7/Stylet) container?  

Light container could be slower than Stylet container in some cases because Stylet container requires to be created via builder and it does some IOC things at it's build time. After Stylet container creation it could not be used for new registrations, only for resolves.  

### What was benchmark hardware?  

CPU: 12th Gen Intel(R) Core(TM) i5-12400F (2.50 GHz)   
GPU: GeForce RTX 5060  
RAM: 32 Gb, 3200 MT/s  
