using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using Component = MariEngine.Components.Component;

namespace MariEngine.Loading;

public class ComponentFactory
{
    private readonly DependencyStorage dependencyStorage = new();

    public ComponentFactory AddDependency<T>(T dependency) where T : class
    {
        dependencyStorage.AddDependency(dependency);
        return this;
    }
    
    public ComponentFactory AddDependency(Type type, object dependency)
    {
        dependencyStorage.AddDependency(type, dependency);
        return this;
    }

    public class DependencyStorage
    {
        private readonly List<(Type type, object dependency)> dependencies = [];

        public DependencyStorage AddDependency<T>(T dependency) where T : class => AddDependency(typeof(T), dependency);

        public DependencyStorage AddDependency(Type type, object dependency)
        {
            dependencies.Add((type, dependency));
            return this;
        }

        public T GetDependency<T>() where T : class => GetDependency(typeof(T)) as T;

        public object GetDependency(Type type) =>
            dependencies.FirstOrDefault(pair => pair.type.IsAssignableTo(type)).dependency;

        public DependencyStorage Clone()
        {
            var storage = new DependencyStorage();
            foreach (var (type, dependency) in dependencies)
                storage.AddDependency(type, dependency);
            return storage;
        }
    }

    public abstract class BaseComponentBuilder<TComponent, TBuilder>(
        DependencyStorage dependencyStorage,
        Type componentType,
        params object[] args) where TBuilder : BaseComponentBuilder<TComponent, TBuilder>
    {
        private readonly Dictionary<PropertyInfo, object> specialValues = new();
        private readonly Dictionary<string, string> resources = new();

        private Type proxyType;
        private object proxyObj;

        public TBuilder WithSpecial(string specialName, object value)
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
            return (TBuilder)this;
        }
        
        public TBuilder WithResource(string resourceParamName, string resourceId)
        {
            resources[resourceParamName.ToLower()] = resourceId;
            return (TBuilder)this;
        }

        public TBuilder WithProxy<TProxy>(TProxy proxyObj) => WithProxy(typeof(TProxy), proxyObj);

        public TBuilder WithProxy(Type proxyType, object proxyObj)
        {
            this.proxyType = proxyType;
            this.proxyObj = proxyObj;
            return (TBuilder)this;
        }

        public TComponent Build(Entity ownerEntity = null, TileEntity ownerTileEntity = null)
        {
            if (!componentType.IsAssignableTo(typeof(TComponent)))
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
                throw new ComponentLoadingException("The provided component type does not have a constructor? Weird.");
            }

            var constructorParameters = constructorInfo.GetParameters();
            var constructorParameterValues = new object[constructorParameters.Length];

            var paramIndex = 0;
            for (var i = 0; i < constructorParameterValues.Length; i++)
            {
                if (constructorParameters[i].IsOptional)
                    continue;
                
                if (constructorParameters[i].CustomAttributes.Any(a => a.AttributeType == typeof(InjectAttribute)))
                {
                    var dependency = dependencyStorage.GetDependency(constructorParameters[i].ParameterType);
                    if (dependency is null)
                        throw new ComponentLoadingException($"Could not inject required dependency {constructorParameters[i].Name} for component {componentType.Name}.");
                    constructorParameterValues[i] = dependency;
                }
                else if (constructorParameters[i].ParameterType.IsAssignableTo(typeof(IResource)) 
                         && constructorParameters[i].CustomAttributes.Any(a => a.AttributeType == typeof(InjectResourceAttribute)))
                {
                    var resourceLoaderType = ServiceRegistry.Services
                        .Select(pair => pair.Key)
                        .Where(t => t.IsAssignableTo(typeof(IResourceLoaderService)) && !t.IsAbstract)
                        .FirstOrDefault(t =>
                            t.BaseType!.GenericTypeArguments[0].IsAssignableTo(constructorParameters[i].ParameterType));
                    if (resourceLoaderType is null)
                        throw new ComponentLoadingException($"No resource loader service is registered for resource of type {constructorParameters[i].ParameterType.Name}.");

                    var resourceLoader = (IResourceLoaderService)ServiceRegistry.Get(resourceLoaderType);
                    var resource = resourceLoader.Get(resources[constructorParameters[i].Name!.ToLower()]);
                    constructorParameterValues[i] = resource;
                }
                else
                {
                    if (paramIndex >= args.Length)
                        throw new ComponentLoadingException($"Not enough arguments provided for component {componentType.Name}.");

                    constructorParameterValues[i] = args[paramIndex++];
                }
            }

            if (paramIndex != args.Length)
            {
                throw new ComponentLoadingException($"Too many arguments provided for component {componentType.Name}.");
            }

            try
            {
                var component = constructorInfo.Invoke(constructorParameterValues);

                if (ownerEntity is not null)
                {
                    typeof(Component).GetProperty("OwnerEntity")?.SetValue(component, ownerEntity);
                }
                
                if (ownerTileEntity is not null)
                {
                    typeof(TileEntityComponent).GetProperty("OwnerEntity")?.SetValue(component, ownerTileEntity);
                }

                foreach (var (propInfo, value) in specialValues)
                {
                    var converter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                    propInfo.SetValue(component, converter.ConvertFrom(value));
                }

                if (proxyType is not null)
                {
                    var proxyBuildableComponentType = (typeof(TComponent) switch
                    {
                        var t when t.IsAssignableTo(typeof(Component)) => typeof(Component<>),
                        var t when t.IsAssignableTo(typeof(TileEntityComponent)) => typeof(TileEntityComponent<>),
                        _ => throw new ArgumentException($"{typeof(TComponent).Name} is not a component type.")
                    }).MakeGenericType(proxyType);
                    
                    var buildMethod = proxyBuildableComponentType.GetMethod("Build");
                    if (buildMethod is null)
                        throw new ComponentLoadingException($"Component {proxyBuildableComponentType.Name} has no Build method? Weird.");
                    
                    buildMethod.Invoke(component, [proxyObj]);
                }

                return (TComponent)component;
            }
            catch (ArgumentException e)
            {
                throw new ComponentLoadingException(e.Message);
            }
        }
    }

    public class TileEntityComponentBuilder(
        Tilemap tilemap,
        DependencyStorage dependencyStorage,
        Type componentType,
        params object[] args) : BaseComponentBuilder<TileEntityComponent, TileEntityComponentBuilder>(dependencyStorage.Clone().AddDependency(tilemap), componentType, args);

    public class ComponentBuilder(DependencyStorage dependencyStorage, Type componentType, params object[] args)
        : BaseComponentBuilder<Component, ComponentBuilder>(dependencyStorage, componentType, args);
    

    public ComponentBuilder CreateComponentBuilder<T>(params object[] args) where T : Component => CreateComponentBuilder(typeof(T), args);

    public ComponentBuilder CreateComponentBuilder(Type componentType, params object[] args) => new(dependencyStorage, componentType, args);
    
    public TileEntityComponentBuilder CreateTileEntityComponentBuilder<T>(Tilemap tilemap, params object[] args) where T : TileEntityComponent => CreateTileEntityComponentBuilder(tilemap, typeof(T), args);

    public TileEntityComponentBuilder CreateTileEntityComponentBuilder(Tilemap tilemap, Type componentType, params object[] args) => new(tilemap, dependencyStorage, componentType, args);
}