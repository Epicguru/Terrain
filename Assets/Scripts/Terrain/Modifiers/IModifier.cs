
namespace Terrain.Modifiers
{
    public interface IModifier<T>
    {
        bool RequiresReapplication { get; }

        void ApplyMod(T target);
    }
}
