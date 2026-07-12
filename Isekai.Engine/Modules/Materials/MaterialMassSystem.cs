using Isekai.Engine.Core.System;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Calculates entity mass from material composition and material definitions.
/// </summary>
public sealed class MaterialMassSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<MaterialCompositionComponent>())
        {
            var composition = entity.GetComponent<MaterialCompositionComponent>();
            var mass = 0.0;

            foreach (var quantity in composition.Materials)
            {
                if (quantity.Volume < 0)
                {
                    throw new InvalidMaterialCompositionException("Material volume cannot be negative.");
                }

                var material = context.World.Definitions.Get<MaterialDefinition>(quantity.Material.Id);
                mass += material.Density * quantity.Volume;
            }

            entity.SetComponent(new MaterialMassComponent(mass));
        }
    }
}
