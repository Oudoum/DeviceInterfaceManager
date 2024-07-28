using System.Text;
using System.Text.Json.Serialization;

namespace DeviceInterfaceManager.Models.Modifiers;

[JsonDerivedType(typeof(Transformation), typeDiscriminator: nameof(Transformation))]
[JsonDerivedType(typeof(Comparison), typeDiscriminator: nameof(Comparison))]
[JsonDerivedType(typeof(Interpolation), typeDiscriminator: nameof(Interpolation))]
[JsonDerivedType(typeof(Padding), typeDiscriminator: nameof(Padding))]
[JsonDerivedType(typeof(Substring), typeDiscriminator: nameof(Substring))]
public interface IModifier : IActive
{
    public void Apply(ref StringBuilder value);
}