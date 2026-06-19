using System.Globalization;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Robust.Shared.Physics.Serialization;

public sealed class NonNegativeFloatSerializer : ITypeSerializer<float, ValueDataNode>
{
    public ValidationNode Validate(
        ISerializationManager serializationManager,
        ValueDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        if (!float.TryParse(node.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
            return new ErrorNode(node, $"Failed parsing float value: {node.Value}");

        if (value < 0f)
            return new ErrorNode(node, "Value must be non-negative.");

        return new ValidatedValueNode(node);
    }

    public float Read(
        ISerializationManager serializationManager,
        ValueDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<float>? instanceProvider = null)
    {
        var value = float.Parse(node.Value, CultureInfo.InvariantCulture);

        if (value < 0f)
            throw new InvalidMappingException("Value must be non-negative.");

        return value;
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        float value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        return new ValueDataNode(value.ToString(CultureInfo.InvariantCulture));
    }
}
