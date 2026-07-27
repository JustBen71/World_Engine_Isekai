using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores contamination exposures for future disease or toxicity systems.
/// </summary>
public sealed record ContaminationExposureComponent(
    IReadOnlyCollection<ContaminationExposure> Exposures) : IComponent;
