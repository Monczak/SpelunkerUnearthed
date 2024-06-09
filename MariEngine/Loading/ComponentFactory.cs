using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using MariEngine.Logging;

namespace MariEngine.Loading;

public class ComponentFactory
{
    private readonly DependencyStorage dependencyStorage = new();

    public ComponentFactory AddDependency<T>(T dependency) where T : class
    {
        dependencyStorage.AddDependency(dependency);
        return this;
    }

    public class DependencyStorage
    {
        private readonly List<(Type type, object dependency)> dependencies = [];

        public void AddDependency<T>(T dependency) where T : class
        {
            dependencies.Add((typeof(T), dependency));
        }

        public T GetDependency<T>() where T : class => GetDependency(typeof(T)) as T;

        public object GetDependency(Type type) =>
            dependencies.FirstOrDefault(pair => pair.type.IsAssignableTo(type)).dependency;
    }

    public class ComponentBuilder(DependencyStorage dependencyStorage, Type componentType, params object[] args)
    {
        private Dictionary<PropertyInfo, object> specialValues = new();

        public ComponentBuilder SetSpecial(string specialName, object value)
        {
            var specialPropInfo = componentType
                .GetProperties()
                .FirstOrDefault(prop => prop.Name == specialName &&
                                        prop.CustomAttributes.Any(a => a.AttributeType == typeof(SpecialAttribute)));
            
            if (specialPropInfo is null)
            {
                throw new ComponentLoadingException(
                    $"Could not find a special property or field with the name {specialName}.");
            }

            specialValues[specialPropInfo] = value;
            return this;
        }

        public Component Build()
        {
            if (!componentType.IsAssignableTo(typeof(Component)))
                throw new ArgumentException($"{componentType.Name} is not a component type.");
        
            // Take the first constructor that has [Inject] params
            var constructorInfo = componentType.GetConstructors()
                .FirstOrDefault(c => c.GetParameters()
                    .Any(p => p.CustomAttributes
                        .Any(a => a.AttributeType == typeof(InjectAttribute))
                    )
                );
                
            // If there are no constructors that take [Inject] params, take the first constructor
            if (constructorInfo is null)
            {
                constructorInfo = componentType.GetConstructors()
                    .FirstOrDefault();
            }

            if (constructorInfo is null)
            {
                throw new ComponentLoadingException("The provided component type does not have a constructor? Weird");
            }

            var constructorParameters = constructorInfo.GetParameters();
            var constructorParameterValues = new object[constructorParameters.Length];

            var paramIndex = 0;
            for (var i = 0; i < constructorParameterValues.Length; i++)
            {
                if (constructorParameters[i].CustomAttributes.Any(a => a.AttributeType == typeof(InjectAttribute)))
                {
                    var dependency = dependencyStorage.GetDependency(constructorParameters[i].ParameterType);
                    if (dependency is null)
                        throw new ComponentLoadingException($"Could not inject required dependency {constructorParameters[i].Name} for component {componentType.Name}.");
                    constructorParameterValues[i] = dependency;
                }
                else
                {
                    if (paramIndex >= args.Length)
                        throw new ComponentLoadingException(
                            $"Not enough arguments provided for component {componentType.Name}.");
                    constructorParameterValues[i] = args[paramIndex++];
                }
            }

            if (paramIndex != args.Length)
            {
                throw new ComponentLoadingException($"Too many arguments provided for component {componentType.Name}.");
            }

            try
            {
                return constructorInfo.Invoke(constructorParameterValues) as Component;
            }
            catch (ArgumentException e)
            {
                throw new ComponentLoadingException(e.Message);
            }
        }
    }

    public ComponentBuilder CreateBuilder<T>(params object[] args) where T : Component => CreateBuilder(typeof(T), args);

    public ComponentBuilder CreateBuilder(Type componentType, params object[] args) => new(dependencyStorage, componentType, args);
}