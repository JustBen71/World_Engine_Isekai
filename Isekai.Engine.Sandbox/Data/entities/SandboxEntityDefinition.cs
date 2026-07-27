namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model describing one sandbox entity.
/// </summary>
public sealed record SandboxEntityDefinition(
    string Name,
    string Glyph,
    string? Material,
    string? Body,
    double Volume,
    SandboxMaterialData[]? Materials,
    SandboxPositionData? Position,
    SandboxVelocityData? Velocity,
    double Temperature,
    SandboxThermalSensitivityData? ThermalSensitivity,
    SandboxBloodData? Blood,
    SandboxNaturalRecoveryData? NaturalRecovery,
    SandboxInjuryData[]? Injuries,
    SandboxCompositeData? Composite,
    SandboxContactSurfaceData? ContactSurface,
    SandboxImpactResistanceData? ImpactResistance,
    SandboxImpactInjuryProfileData? ImpactInjuryProfile,
    SandboxImpactRequestData? Impact,
    SandboxGripCapabilityData? GripCapability,
    SandboxHeldEntityData[]? HeldEntities,
    SandboxHandledImpactData? HandledImpact,
    SandboxBodyImpactCapabilityData? BodyImpactCapability,
    SandboxBodyContactSurfaceData[]? BodyContactSurfaces,
    SandboxBodyImpactData? BodyImpact,
    SandboxNeedsData? Needs,
    SandboxDietData? Diet,
    SandboxWaterToleranceData? WaterTolerance,
    SandboxConsumableResourceData? Consumable,
    SandboxWaterSourceData? WaterSource,
    SandboxConsumeIntentData? ConsumeIntent,
    SandboxDrinkIntentData? DrinkIntent);
