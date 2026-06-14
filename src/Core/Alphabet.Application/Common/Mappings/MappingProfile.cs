using System.Reflection;
using AutoMapper;

namespace Alphabet.Application.Common.Mappings;

/// <summary>
/// Applies mappings declared through <see cref="IMapFrom{T}" />.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);
        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        var types = assembly.GetExportedTypes()
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType))
            .ToArray();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(mappingMethodName)
                ?? type.GetInterface(mapFromType.Name)?.GetMethod(mappingMethodName);

            methodInfo?.Invoke(instance, [this]);
        }
    }
}
