using Hypocrite.Container;

namespace Hypocrite.Checks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var container = new LightContainer();
            container.RegisterSingleton<IService1, Service1>();
            container.RegisterSingleton<IService2, Service2>();
            container.RegisterInstance<IService3>(new Service3());

            var sssss = container.Resolve<IService3>();
            var b = 3 + 4;
            Console.ReadKey();
        }
    }

    public interface IService1 { }
    public class Service1 : IService1 { }

    public interface IService2 { }
    public class Service2 : IService2 
    {
        [Injection]
        public IService1 Service1 { get; set; }
    }

    public interface IService3 { }
    public class Service3 : IService3
    {
        [Injection]
        public IService2 Service2 { get; set; }
    }
}
