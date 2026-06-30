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
  Interfaces/
  Exceptions/
  Utilities/

Isekai.Engine.Tests/
  Core/
  TestDoubles/
```

`Isekai.Engine` contient le Core du moteur.
`Isekai.Engine.Tests` contient les tests unitaires xUnit.

## Lancer les tests avec Rider

1. Ouvrez `World_Engine_Isekai.sln`.
2. Attendez que Rider restaure les packages NuGet.
3. Dans l'explorateur de projet, ouvrez `Isekai.Engine.Tests`.
4. Faites clic droit sur le projet `Isekai.Engine.Tests`.
5. Cliquez sur `Run Unit Tests`.

Resultat attendu :

```text
61 tests passed
0 failed
```

## Lancer les tests en ligne de commande

Si le SDK .NET est installe dans le `PATH`, executez :

```powershell
dotnet restore .\World_Engine_Isekai.sln
dotnet test .\World_Engine_Isekai.sln --no-restore
```

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
