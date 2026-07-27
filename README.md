# World Engine Isekai

World Engine est un moteur de simulation C#/.NET independant de Unity et Unreal.
Ce depot contient actuellement les fondations du Core et les tests unitaires associes.

## Prerequis systeme

Pour travailler sur le projet et lancer les tests unitaires, installez :

- JetBrains Rider 2026.1 ou plus recent.
- .NET SDK 10 ou plus recent.
- Git.

Rider peut embarquer son propre SDK .NET. Si les tests se lancent depuis Rider, il n'est pas obligatoire que la commande `dotnet` soit disponible dans le terminal systeme. Pour lancer les tests en ligne de commande, le SDK .NET doit etre installe et accessible dans le `PATH`.

## Recuperer le projet

Clonez le depot avec Git :

```powershell
git clone <url-du-repo>
cd World_Engine_Isekai
```

Ouvrez ensuite la solution dans Rider :

```text
World_Engine_Isekai.sln
```

## Structure principale

```text
Isekai.Engine/
  Core/
    Definitions/
  Data/
    Definitions/
  Modules/
    Body/
    Composition/
    Healing/
    Injuries/
    Materials/
    Temperature/
    Terrain/
    Vitals/
  Interfaces/
  Exceptions/
  Utilities/

Isekai.Engine.Tests/
  Core/
  TestDoubles/

Isekai.Engine.Sandbox/
  Components/
  Data/
    definitions/
    entities/
    scenarios/
  Scenarios/
  Systems/
  Terminal/
  World/
```

`Isekai.Engine` contient le Core du moteur.
`Isekai.Engine.Tests` contient les tests unitaires xUnit.
`Isekai.Engine.Sandbox` contient une application console de validation manuelle separee du moteur. Elle utilise `Spectre.Console` pour le rendu terminal colore.

## Lancer les tests avec Rider

1. Ouvrez `World_Engine_Isekai.sln`.
2. Attendez que Rider restaure les packages NuGet.
3. Dans la liste des configurations, selectionnez `Tests - All`.
4. Lancez `Run` pour executer tous les tests unitaires.
5. Selectionnez ensuite `Tests - Sandbox Scenarios`.
6. Lancez `Run` pour verifier uniquement les scenarios sandbox.

Configurations disponibles :

- `Tests - All` : execute toute la suite xUnit du projet `Isekai.Engine.Tests`.
- `Tests - Sandbox Scenarios` : execute les regressions `SandboxScenarioRegressionTests`.

Resultat attendu :

```text
165 tests passed
0 failed
```

## Lancer les tests en ligne de commande

Si le SDK .NET est installe dans le `PATH`, executez :

```powershell
dotnet restore .\World_Engine_Isekai.sln
dotnet test .\World_Engine_Isekai.sln --no-restore
```

## Lancer la sandbox console

La sandbox est un projet separe du moteur. Elle sert a verifier manuellement que le moteur peut charger des donnees, creer un monde, executer des ticks et afficher un etat observable.

Depuis Rider :

1. Ouvrez `World_Engine_Isekai.sln`.
2. Dans la liste des configurations, selectionnez `Sandbox Console`.
3. Lancez `Run`.

Configuration disponible :

- `Sandbox Console` : lance la sandbox avec `--ticks=30` et affiche le menu de scenarios.
- `Sandbox Console Trace` : lance la sandbox avec `--ticks=10 --trace` et genere `debug.log`.

Depuis PowerShell :

```powershell
dotnet run --project .\Isekai.Engine.Sandbox\Isekai.Engine.Sandbox.csproj
```

Options :

```powershell
dotnet run --project .\Isekai.Engine.Sandbox\Isekai.Engine.Sandbox.csproj -- --ticks=10
dotnet run --project .\Isekai.Engine.Sandbox\Isekai.Engine.Sandbox.csproj -- --scenario=woodcutting --ticks=30
dotnet run --project .\Isekai.Engine.Sandbox\Isekai.Engine.Sandbox.csproj -- --scenario=woodcutting --ticks=10 --trace
```

Sans `--scenario`, la console affiche un menu interactif.
Les scenarios disponibles sont :

```text
woodcutting          Coupe de bois
deer-kicks-human    Cerf contre humain
free-movement       Deplacements libres
steel-axe           Hache acier contre arbre
bleeding            Hemorragie non traitee
natural-recovery    Recuperation naturelle
temperature-comfort Confort thermique
composite-wear      Usure composite
thermal-materials   Materiaux et temperature
thermal-zones       Zones thermiques locales
terrain-basic       Terrain de base
```

La sandbox charge :

```text
Isekai.Engine.Sandbox/Data/definitions/materials.json
Isekai.Engine.Sandbox/Data/definitions/bodies.json
Isekai.Engine.Sandbox/Data/definitions/injuries.json
Isekai.Engine.Sandbox/Data/scenarios/*.json
```

Elle affiche une grille terminal avec des entites, leurs positions, leur masse calculee par `MaterialMassSystem`, leur temperature, leur integrite corporelle et leurs blessures eventuelles.
Le rendu affiche aussi une carte coloree avec emojis de terrain, ainsi qu'un panneau d'informations en temps reel a droite :

- tick et frame courants ;
- temperature moyenne de la carte ;
- temperature minimum et maximum ;
- nombre d'entites ;
- position, masse, temperature, confort thermique, integrite corporelle et blessures de chaque entite.

Les memes fichiers de scenario sont executes par `SandboxScenarioRegressionTests`.
Ces tests servent de garde-fous : si une formule du moteur change brutalement, un scenario de regression doit le signaler.

Le scenario `thermal-zones` montre une ambiance locale simulee par la sandbox :
une zone froide, une zone temperee et une zone chaude. Il ne depend pas d'un module `Terrain` complet.

Legende terrain :

- prairie : 🌾
- foret : 🌳
- eau : 🌊
- montagne : ⛰️

Les entites de demonstration sont chargees depuis JSON. Le fichier contient notamment :

- jeune arbre ;
- humain ;
- cerf ;
- eau ;
- fer ;
- pierre.

L'humain et le cerf ont une sensibilite thermique. Le jeune arbre, l'eau, le fer et la pierre peuvent avoir une temperature physique, mais ne ressentent pas la temperature.

## Activer le debug.log imbrique

Le Core contient un logger de diagnostic optionnel. Par defaut, aucun log n'est ecrit.

Pour tracer les entrees et sorties des fonctions Core dans `debug.log`, utilisez un niveau de log 10 :

```csharp
using Isekai.Engine.Core;
using Isekai.Engine.Diagnostics;

using var logger = new FileTraceLogger("debug.log", EngineLogLevels.FullTrace);
var world = new WorldState(logger);

var entity = world.CreateEntity();
world.Tick(TimeSpan.FromSeconds(1));
```

Le fichier produit une imbrication lisible :

```text
WorldState.CreateEntity => {
  Entity.ctor => {
  } <= Entity.ctor
} <= WorldState.CreateEntity
WorldState.Tick => {
  SimulationTime.Advance => {
  } <= SimulationTime.Advance
} <= WorldState.Tick
```

`debug.log` est ignore par Git.

## Definitions data-driven

Le Core contient un socle neutre pour les futures donnees :

- `DefinitionId` represente un identifiant stable venant des donnees.
- `IDefinition` est le contrat minimal d'une definition data-driven.
- `DefinitionRegistry` stocke les definitions par identifiant.

Ce socle ne contient aucune definition specifique au gameplay. Il sert a eviter les chaines brutes partout dans le moteur et a recevoir les definitions chargees par la couche Data.

Exemple :

```csharp
using Isekai.Engine.Core.Definitions;

var registry = new DefinitionRegistry();
var id = DefinitionId.From("example.definition");

registry.Register(new ExampleDefinition(id));
var definition = registry.Get<ExampleDefinition>(id);
```

Une definition doit seulement exposer son identifiant :

```csharp
public sealed record ExampleDefinition(DefinitionId Id) : IDefinition;
```

La couche `Data/Definitions` charge maintenant des definitions JSON neutres. Chaque definition JSON doit avoir au minimum :

- `id` : identifiant stable de definition.
- `type` : type JSON enregistre dans `DefinitionTypeRegistry`.

Exemple de fichier :

```json
{
  "definitions": [
    {
      "type": "example.definition",
      "id": "example.one"
    }
  ]
}
```

Exemple de chargement :

```csharp
var types = new DefinitionTypeRegistry();
types.Register<ExampleDefinition>("example.definition");

var loader = new JsonDefinitionLoader(types);
var registry = loader.LoadDirectory("Data/definitions");
```

Pipeline recommande :

```csharp
var types = new DefinitionTypeRegistry();
types.RegisterMaterialDefinitions();

var pipeline = new DefinitionLoadPipeline(
    new JsonDefinitionLoader(types),
    new IDefinitionValidator[]
    {
        new MaterialDefinitionValidator()
    });

var registry = pipeline.LoadDirectory("Data/definitions");
```

Le pipeline charge les JSON, valide les references, execute les validateurs de modules, puis fige le registre.

Le loader accepte un objet unique, un tableau, ou un objet racine avec une propriete `definitions`.

Erreurs gerees :

- fichier ou dossier introuvable ;
- JSON invalide ;
- champ `id` ou `type` manquant ;
- type de definition inconnu ;
- identifiant duplique ;
- type .NET incompatible lors de la recuperation.

## References entre definitions

Une definition peut referencer une autre definition avec `DefinitionReference<TDefinition>`.
Cela evite les chaines brutes et permet de valider que la cible existe avec le bon type.

Exemple C# :

```csharp
public sealed record ExampleDefinition(DefinitionId Id) : IDefinition;

public sealed record OwnerDefinition(
    DefinitionId Id,
    DefinitionReference<ExampleDefinition> Target) : IDefinition;
```

Exemple JSON :

```json
{
  "definitions": [
    {
      "type": "example.definition",
      "id": "example.one"
    },
    {
      "type": "owner.definition",
      "id": "owner.one",
      "target": "example.one"
    }
  ]
}
```

Validation :

```csharp
var result = registry.ValidateReferences();

if (!result.IsValid)
{
    // result.Errors contient toutes les erreurs trouvees.
}

registry.EnsureReferencesValid();
```

`ValidateReferences()` regroupe les erreurs sans lancer d'exception.
`EnsureReferencesValid()` lance une `DefinitionValidationException` contenant toutes les erreurs.

Apres chargement et validation, le registre peut etre fige :

```csharp
registry.EnsureReferencesValid();
registry.Freeze();
```

Une fois fige, le registre reste lisible mais refuse tout nouvel enregistrement.
Cette regle stabilise les definitions avant le debut de la simulation.

## Module Materials

Le premier module universel disponible est `Materials`.
Il ne contient pas de gameplay : il modelise seulement des donnees physiques generales.

Types principaux :

- `MaterialDefinition` : definition data-driven d'un materiau.
- `MaterialQuantity` : volume d'un materiau dans une entite.
- `MaterialCompositionComponent` : composition materielle d'une entite.
- `MaterialMassComponent` : masse calculee en kilogrammes.
- `MaterialMassSystem` : calcule la masse depuis les compositions et definitions.
- `MaterialDefinitionValidator` : valide les donnees de materiaux avant simulation.

Exemple JSON :

```json
{
  "type": "material.definition",
  "id": "material.iron",
  "density": 7870,
  "specificHeatCapacity": 450,
  "thermalConductivity": 80
}
```

Exemple d'enregistrement des types :

```csharp
var types = new DefinitionTypeRegistry();
types.RegisterMaterialDefinitions();
```

Exemple d'utilisation :

```csharp
var entity = world.CreateEntity();

entity.AddComponent(new MaterialCompositionComponent(new[]
{
    new MaterialQuantity(
        DefinitionReference<MaterialDefinition>.From("material.iron"),
        Volume: 0.5)
}));

world.RegisterSystem(new MaterialMassSystem());
world.Tick(TimeSpan.FromSeconds(1));

var mass = entity.GetComponent<MaterialMassComponent>();
```

## Convention de modules

Les modules du moteur exposent une integration commune avec `IEngineModule` :

```csharp
public interface IEngineModule
{
    string Name { get; }
    void RegisterDefinitionTypes(DefinitionTypeRegistry registry);
    IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators();
    void RegisterSystems(WorldState world);
}
```

Objectif :

- enregistrer facilement les types JSON d'un module ;
- ajouter ses validateurs ;
- brancher ses systemes dans `WorldState` ;
- eviter de coder chaque module de maniere speciale dans la sandbox ou dans un futur jeu.

Modules actuels :

- `BodyModule`
- `BodyCapabilitiesModule`
- `CompositionModule`
- `HealingModule`
- `HandlingModule`
- `ImpactModule`
- `InjuryModule`
- `MaterialModule`
- `TemperatureModule`
- `TerrainModule`
- `VitalsModule`

## Module Composition

Le module `Composition` modelise des entites composites sans logique de craft.
Il sert a dire qu'une entite est formee de pieces physiques importantes, tout en laissant les materiaux integres dans `MaterialCompositionComponent`.

Types principaux :

- `CompositeComponent` : liste les pieces importantes d'une entite composite.
- `CompositePart` : reference une entite piece avec un role libre et un marqueur structurel.
- `CompositeMembershipComponent` : indique qu'une entite est utilisee par un composite.
- `CompositeIntegrityComponent` : etat structurel calcule du composite.
- `CompositeMassComponent` : masse totale calculee du composite et de ses pieces.
- `CompositeMembershipSystem` : synchronise les memberships des pieces.
- `CompositeIntegritySystem` : calcule si les pieces structurelles existent encore et dans quel etat.
- `CompositeMassSystem` : additionne la masse des pieces et des materiaux integres.

Exemple conceptuel :

```text
Hache os-silex
  CompositeComponent:
    body -> Os long
    head -> Silex taille
    binding -> Liane jungle
```

Le moteur ne sait pas que c'est une hache. Il sait seulement que ces entites forment un composite.

Pour un cas comme une soudure ou du metal fondu, on ne cree pas forcement une entite separee.
Le metal integre reste represente par `MaterialCompositionComponent` sur le composite :

```text
Hache acier-chene
  CompositeComponent:
    handle -> Manche chene
    head -> Tete acier
  MaterialCompositionComponent:
    iron
```

## Module Handling

Le module `Handling` modelise le fait qu'une entite puisse tenir et manier une autre entite.
Il ne connait pas les humains, les orcs, les arbres animes ou les armes : il relie seulement un porteur, un objet tenu et une action physique.

Types principaux :

- `GripCapabilityComponent` : force maximale, force de manipulation et precision d'un porteur.
- `HeldEntity` : entite tenue, slot libre et qualite de prise.
- `HeldEntitiesComponent` : liste des entites tenues par un porteur.
- `HandledImpactComponent` : action d'impact cadencee avec un objet tenu.
- `HandledImpactSystem` : genere un `ImpactRequestComponent` seulement quand un coup doit partir.

Exemple conceptuel :

```text
Humain
  GripCapability:
    maxGripForce: 70
    manipulationForce: 45
    precision: 0.9
  HeldEntities:
    right_hand -> Hache os-silex
  HandledImpact:
    heldEntity: Hache os-silex
    contactEntity: Silex taille
    targetEntity: Jeune arbre
    ticksBetweenImpacts: 3
```

La force de l'impact vient donc du porteur, pas de la hache.
La hache apporte sa composition, sa masse, sa surface de contact et sa durabilite.

## Module BodyCapabilities

Le module `BodyCapabilities` modelise ce qu'une entite peut faire avec ses propres parties du corps.
Il sert notamment aux contacts corporels autorises : coup de sabot, coup de poing, coup de tete, morsure ou coup de cou pour une creature qui possede cette capacite.

Types principaux :

- `BodyImpactCapabilityComponent` : force corporelle maximale, force d'impact et precision.
- `BodyContactSurface` : role autorise, partie du corps utilisee et proprietes de contact.
- `BodyContactSurfacesComponent` : surfaces corporelles autorisees, chacune liee a une partie du corps.
- `BodyImpactComponent` : action d'impact cadencee avec une surface corporelle autorisee.
- `BodyImpactSystem` : genere un `ImpactRequestComponent` depuis le corps de l'acteur.

Exemple de contact corporel :

```text
Cerf
  BodyImpactCapability:
    maxForce: 120
    impactForce: 95
    precision: 1.0
  BodyContactSurfaces:
    front_hoof -> front_legs
  BodyImpact:
    contactRole: front_hoof
    targetEntity: Humain
    targetBodyPart: right_leg
```

Un role non declare ne peut pas etre utilise.
Une partie du corps detruite ne peut plus produire ce contact.
Les parties du corps n'ont pas de durabilite d'outil : elles ont une integrite corporelle et peuvent recevoir des blessures via les modules `Body`, `Impact` et `Injuries`.

## Module Impact

Le module `Impact` modelise un contact physique sans logique d'attaque, de minage ou de gameplay.
Il utilise des coefficients numeriques ou `1.0` represente une base standard.

Types principaux :

- `ContactSurfaceComponent` : durete, tranchant, penetration, tenue du tranchant et aire de contact d'une forme.
- `ImpactResistanceComponent` : resistance d'une cible a l'impact.
- `ImpactRequestComponent` : demande de resolution d'un impact.
- `ImpactResultComponent` : resultat calcule sur l'entite source.
- `ImpactResolvedEvent` : evenement publie lorsqu'un impact est resolu.
- `ImpactResolutionSystem` : transforme force + surface + resistance en ratio et resultat.
- `ImpactToBodyDamageSystem` : applique le resultat d'impact sur l'integrite d'une partie de corps ciblee.
- `ImpactContactWearSystem` : use la partie de l'entite utilisee comme surface de contact.
- `ImpactInjuryProfileComponent` : configure comment une cible transforme certains impacts en blessures.
- `ImpactInjuryBridgeSystem` : cree ou aggrave une blessure a partir d'un impact resolu.

Exemple conceptuel :

```text
Tete acier
  ContactSurface:
    hardness: 2.0
    sharpness: 2.4
    penetration: 1.8
    edgeRetention: 2.5
    contactArea: 0.01

Silex taille
  ContactSurface:
    hardness: 1.2
    sharpness: 1.8
    penetration: 1.4
    edgeRetention: 0.5
    contactArea: 0.012
```

Le materiau donne la base physique generale, mais la forme de l'entite donne ses proprietes de contact.
Une plaque de metal et une tete d'outil peuvent donc partager un materiau tout en ayant des surfaces tres differentes.

Le pont `Impact -> Injury` reste configurable par entite.
Un humain peut transformer une fracture du torse en blessure moderement hemorragique, tandis qu'un arbre transforme une coupe du tronc en fissure sans sang.

Dans la sandbox, l'humain tient la hache os-silex et frappe le tronc du jeune arbre pendant les ticks.
Le resultat d'impact reste generique (`KCut:1.68`) et le systeme reduit progressivement l'integrite du tronc.
La tete en silex a son propre corps, donc elle s'use aussi et fait baisser l'integrite composite de la hache.

## Module Body

Le module `Body` modelise une structure physique optionnelle pour une entite.
Une entite peut avoir un corps detaille, un corps simple, ou aucun corps.

Types principaux :

- `BodyDefinition` : definition data-driven d'un corps.
- `BodyPartDefinition` : definition d'une partie du corps.
- `BodyComponent` : assigne un corps a une entite.
- `BodyStateComponent` : etat runtime des parties.
- `BodyIntegrityComponent` : integrite globale entre 0 et 1.
- `BodyInitializationSystem` : initialise les parties depuis la definition.
- `BodyIntegritySystem` : calcule l'integrite globale.

Exemple JSON :

```json
{
  "type": "body.definition",
  "id": "body.human.basic",
  "parts": [
    {
      "id": "torso",
      "name": "Torso",
      "material": "material.flesh",
      "maxIntegrity": 100
    }
  ]
}
```

Dans la sandbox, le jeune arbre a un corps simple, l'humain et le cerf ont un corps plus detaille.
Les objets comme l'eau, le fer ou la pierre n'ont pas de corps.

## Module Injuries

Le module `Injuries` modelise des blessures runtime appliquees a des parties de corps.
Il reste volontairement generique : il ne decide pas comment une blessure apparait, il applique seulement les donnees deja presentes sur l'entite.
Une blessure ne se soigne pas automatiquement. La recuperation vient d'un composant de recuperation naturelle ou d'un systeme de soin separe.

Types principaux :

- `InjuryDefinition` : definition data-driven d'un type de blessure.
- `InjuryState` : blessure runtime ciblee sur une partie de corps, avec severite et hemorragie.
- `InjuryComponent` : liste des blessures presentes sur une entite.
- `InjuryApplicationSystem` : applique la perte d'integrite sur les parties touchees.
- `BleedingSystem` : transforme les hemorragies en perte de sang pour les entites qui ont du sang.
- `InjuryDefinitionValidator` : valide les donnees de blessures avant simulation.

Exemple JSON :

```json
{
  "type": "injury.definition",
  "id": "injury.generic.tissue_damage",
  "name": "entaille",
  "integrityLossPerSeverityPerSecond": 0.35,
  "defaultBleedingSeverity": "Minor"
}
```

Une entite doit avoir un `BodyStateComponent` pour que les blessures affectent son corps.
Une pierre, une ressource ou un objet sans corps peut ignorer totalement ce module.
Dans la sandbox, le jeune arbre, l'humain et le cerf montrent des blessures initiales chargees depuis JSON.

## Module Vitals

Le module `Vitals` modelise l'etat vital sans barre de vie.
Une entite vivante peut avoir du sang et des parties vitales definies dans son corps.

Types principaux :

- `BloodComponent` : volume de sang courant et maximum.
- `VitalStateComponent` : indique si l'entite est vivante et la raison de mort si elle ne l'est plus.
- `VitalStateSystem` : detecte la mort par manque de sang ou destruction d'une partie vitale.

Exemples de roles vitaux dans un body JSON :

```json
{
  "id": "head",
  "name": "Head",
  "material": "material.flesh",
  "maxIntegrity": 35,
  "vitalRole": "brain"
}
```

## Module Healing

Le module `Healing` separe recuperation naturelle et soin actif.
Cela permet par exemple a un cerf de recuperer plus vite naturellement qu'un humain.

Types principaux :

- `NaturalRecoveryComponent` : recuperation propre a l'entite blessee.
- `HealingCapabilityComponent` : capacite d'une entite a soigner une autre entite.
- `HealingTargetComponent` : cible actuellement soignee.
- `NaturalRecoverySystem` : reduit les blessures selon les capacites naturelles de l'entite.
- `TreatmentSystem` : applique un soin externe depuis une entite soigneuse vers une cible.

## Module Terrain

Le module `Terrain` modelise la structure spatiale persistante du monde.
Une cellule de terrain n'est pas une entite ECS : elle ne possede pas de dictionnaire de composants, pas d'identifiant runtime d'entite et pas de cycle de vie d'entite.

Responsabilites du module :

- espace et grille ;
- coordonnees de cellules ;
- conversion depuis une position continue ;
- altitude en metres ;
- type physique de sol ;
- voisins deterministes ;
- regions spatiales et regions actives de simulation.

Le terrain ne calcule pas les biomes, la meteo, la temperature ressentie, la vegetation, l'IA ou le gameplay.
Ces phenomenes pourront utiliser le terrain sans lui appartenir.

Types principaux :

- `WorldPosition` : position continue en metres.
- `PositionComponent` : position generique d'une entite.
- `TerrainCellCoordinate` : coordonnee discrete d'une cellule.
- `TerrainCell` : vue immutable d'une cellule avec altitude et sol.
- `TerrainGrid` : grille compacte stockee par couches.
- `SoilDefinition` : definition data-driven d'un type de sol.
- `TerrainGridDefinition` : dimensions et profil de generation d'une grille.
- `TerrainService` : acces terrain injecte explicitement aux consommateurs.
- `ActiveSimulationRegionRegistry` : regions importantes en memoire.
- `SimpleTerrainGenerator` : generation deterministe plate, pente ou colline centrale.

Representation memoire :

```text
TerrainGrid
  elevationLayer: double[]
  soilLayer: DefinitionId[]
  index = y * WidthInCells + x
```

`GetCell` retourne une copie immutable de la cellule.
Les modifications passent par `SetElevation` et `SetSoil`, ce qui evite de croire qu'une copie modifie directement la grille.

Les voisins sont retournes dans un ordre stable :

```text
4 voisins: Nord, Est, Sud, Ouest
8 voisins: Nord, Nord-Est, Est, Sud-Est, Sud, Sud-Ouest, Ouest, Nord-Ouest
```

La pente est un ratio :

```text
(elevationTo - elevationFrom) / horizontalDistanceMeters
```

Integration avec les entites :

```csharp
entity.AddComponent(new PositionComponent(new WorldPosition(19, 24, 0)));
var coordinate = terrainService.GetCellAt(entity.GetComponent<PositionComponent>().Position);
```

Integration avec `Temperature` :

```text
WorldPosition
      v
TerrainGrid
      v
TerrainCellCoordinate
      v
AmbientTemperatureProvider
      v
TemperatureExchangeSystem
```

Le choix actuel est de ne pas stocker de temperature ambiante dans `TerrainCell`.
`Terrain` fournit seulement la position et la cellule. Un provider de temperature, ou plus tard un module `Climate`, pourra utiliser cette cellule pour retourner une ambiance locale.

Le scenario sandbox `terrain-basic` affiche une grille 10x10 generee en pente, quelques cellules, les voisins d'une cellule et la cellule correspondant a chaque entite positionnee.

## Module Temperature

Le module `Temperature` est le module universel responsable de la chaleur et de la temperature.
Il modelise la temperature physique, la temperature ambiante lue depuis une source abstraite et le ressenti thermique sans gameplay specifique.

Il ne depend pas du module `Body` : un corps vivant, un objet, une matiere, un composite ou une future couche environnementale peuvent avoir une temperature.
Le module `Terrain` fournit seulement la structure spatiale du monde. Le calcul thermique reste dans `Temperature`.

Types principaux :

- `AmbientTemperatureComponent` : temperature ambiante globale simple, conservee pour les tests et scenarios existants.
- `IAmbientTemperatureProvider` : source abstraite de temperature ambiante autour d'une entite.
- `ComponentAmbientTemperatureProvider` : fournisseur par defaut qui lit `AmbientTemperatureComponent`.
- `TemperatureComponent` : temperature physique actuelle d'une entite, en degres Celsius.
- `ThermalSensitivityComponent` : plage de temperature confortable d'une entite sensible.
- `ThermalComfortComponent` : resultat calcule du confort thermique.
- `TemperatureExchangeSystem` : fait tendre progressivement la temperature physique vers l'ambiance fournie.
- `ThermalComfortSystem` : calcule le confort seulement pour les entites sensibles.

Flux de responsabilites :

```text
AmbientTemperatureProvider
          ↓
TemperatureExchangeSystem
          ↓
TemperatureComponent
          ↓
ThermalComfortSystem
          ↓
ThermalComfortComponent
```

Exemple :

```csharp
entity.AddComponent(new TemperatureComponent(36.8));
entity.AddComponent(new ThermalSensitivityComponent(36.0, 38.0));
```

Une entite comme un baton, un arbre ou une pierre peut avoir `TemperatureComponent` sans avoir `ThermalSensitivityComponent`.
Elle change alors de temperature, mais ne ressent rien.

Un humain ou un cerf peut avoir les deux composants.
Le moteur calcule alors un `ThermalComfortComponent`.

La temperature ambiante peut venir du composant global existant ou, plus tard, d'un provider spatial qui lit la position d'une entite puis demande la temperature de la cellule correspondante.
Le module ne connait pas les biomes, le climat, les saisons, le jour/nuit, l'altitude ou l'humidite.

## Regles de mutation pendant un Tick

Pendant `world.Tick(deltaTime)`, les systemes sont executes dans l'ordre d'enregistrement et observent le meme ensemble d'entites pendant toute la boucle.

Ordre complet d'un tick :

```text
Tick
  Begin Event Queue
  Advance Time
  Execute Systems
  Flush Queued Events
  Apply Deferred World Mutations
  End Event Queue
End Tick
```

Les evenements publies par les systemes pendant un tick sont mis en file. Ils sont dispatches apres l'execution de tous les systemes, dans l'ordre ou ils ont ete publies. Si un handler publie un nouvel evenement pendant le flush, ce nouvel evenement est ajoute a la fin de la file et sera lui aussi dispatche de maniere deterministe.

Les mutations structurelles du `WorldState` sont differees :

- `CreateEntity()` cree l'objet `Entity`, mais l'enregistre dans le monde apres la fin du tick.
- `RemoveEntity(entityId)` demande une suppression, mais l'applique apres la fin du tick.
- Les mutations differees sont appliquees dans l'ordre ou elles ont ete demandees.

Cette regle evite qu'un systeme modifie la collection d'entites pendant qu'un autre systeme est en cours d'execution.

Il est interdit d'enregistrer un systeme pendant un tick. Il est aussi interdit de demarrer un nouveau tick pendant qu'un tick est deja en cours.

## Depannage

Si `dotnet` n'est pas reconnu dans PowerShell, installez le SDK .NET 10 puis redemarrez le terminal.

Si Rider indique qu'un pack de ciblage est manquant, verifiez que le SDK installe supporte bien `net10.0`.

Si les tests ne sont pas detectes, verifiez que le projet `Isekai.Engine.Tests` a bien restaure ses packages NuGet, notamment :

- `xunit`
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`

Les dossiers `bin/` et `obj/` sont generes automatiquement et ignores par Git.
