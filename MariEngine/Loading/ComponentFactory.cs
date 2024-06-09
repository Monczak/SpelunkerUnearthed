using System.Linq;
using MariEngine.Components;

namespace MariEngine.Loading;

public class ComponentFactory(DependencyInjector injector)
{
    // public T Build<T>(params object[] args) where T : Component
    // {
    //     var componentType = typeof(T);
    //     var constructorInfo = componentType.GetConstructors()
    //         .FirstOrDefault(c => c.GetParameters()
    //             .Any(p => p.CustomAttributes
    //                 .Any(a => a.AttributeType == typeof(InjectAttribute))
    //             )
    //         );
    // }
}