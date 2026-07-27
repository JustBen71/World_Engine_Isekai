namespace Isekai.Engine.Sandbox.Scenarios;

/// <summary>
/// Lists the sandbox scenarios available from the console launcher.
/// </summary>
public static class SandboxScenarioRegistry
{
    private static readonly SandboxScenario[] Scenarios =
    [
        new(
            "woodcutting",
            "Coupe de bois",
            "Un humain tient une hache os-silex et coupe un jeune arbre jusqu'a rupture du tronc.",
            "woodcutting.json",
            30),
        new(
            "deer-kicks-human",
            "Cerf contre humain",
            "Un cerf frappe un humain au torse avec ses sabots jusqu'a destruction d'une partie vitale.",
            "deer_kicks_human.json",
            45),
        new(
            "free-movement",
            "Deplacements libres",
            "Les entites mobiles se deplacent sur la carte avec des vitesses deterministes.",
            "free_movement.json",
            35),
        new(
            "steel-axe",
            "Hache acier contre arbre",
            "Compare une hache acier-chene sur le meme jeune arbre avec une usure faible.",
            "steel_axe_vs_tree.json",
            25),
        new(
            "bleeding",
            "Hemorragie non traitee",
            "Un humain avec une plaie massive perd son sang jusqu'a un etat fatal.",
            "bleeding_until_death.json",
            60),
        new(
            "natural-recovery",
            "Recuperation naturelle",
            "Un cerf avec une blessure legere recupere progressivement grace a sa regeneration.",
            "natural_recovery.json",
            40),
        new(
            "temperature-comfort",
            "Confort thermique",
            "Humain et cerf reagissent a une ambiance froide pendant que les objets physiques refroidissent.",
            "temperature_comfort.json",
            35),
        new(
            "composite-wear",
            "Usure composite",
            "Une hache os-silex frappe une pierre pour verifier l'usure de sa tete et de son assemblage.",
            "composite_wear.json",
            30),
        new(
            "thermal-materials",
            "Materiaux et temperature",
            "Eau, fer et pierre convergent vers l'ambiance selon leurs proprietes thermiques.",
            "thermal_materials.json",
            35)
    ];

    /// <summary>
    /// Gets all available scenarios.
    /// </summary>
    public static IReadOnlyCollection<SandboxScenario> All => Scenarios;

    /// <summary>
    /// Finds a scenario by id or returns null.
    /// </summary>
    public static SandboxScenario? FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Scenarios.FirstOrDefault(scenario =>
            string.Equals(scenario.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
