namespace Alphabet.Application.Common.Mappings;

/// <summary>
/// Implemented by DTOs that configure AutoMapper mappings.
/// </summary>
public interface IMapFrom<T>
{
    void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap(typeof(T), GetType());
    }
}
