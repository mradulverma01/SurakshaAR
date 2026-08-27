using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SurakshaAR.Domain.Catalog;
using SurakshaAR.Domain.Training;

namespace SurakshaAR.Infrastructure.Catalog
{
    public sealed class JsonTrainingCatalog : ITrainingCatalog
    {
        private readonly string directory;

        public JsonTrainingCatalog(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("A catalog directory is required.", nameof(directory));
            }

            this.directory = directory;
        }

        public async Task<ScenarioBundle> Get(string moduleId, int? version = null)
        {
            ValidateModuleId(moduleId);
            var path = version.HasValue
                ? Path.Combine(directory, moduleId + ".v" + version.Value + ".json")
                : LatestPath(moduleId);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The scenario bundle is not installed.", path);
            }

            ScenarioDocument? document;
            using (var reader = new StreamReader(path))
            {
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                document = JsonConvert.DeserializeObject<ScenarioDocument>(json);
            }

            if (document == null || !string.Equals(document.Id, moduleId, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The scenario document identity does not match the requested module.");
            }

            return document.ToBundle();
        }

        public async Task<IReadOnlyList<ScenarioBundle>> List()
        {
            if (!Directory.Exists(directory))
            {
                return Array.Empty<ScenarioBundle>();
            }

            var files = Directory.GetFiles(directory, "*.v*.json");
            var latestByModule = files
                .Select(path => new { Path = path, FileName = Path.GetFileNameWithoutExtension(path) })
                .Where(entry => entry.FileName.Contains(".v"))
                .Select(entry =>
                {
                    var fileName = entry.FileName;
                    var separator = fileName.LastIndexOf(".v", StringComparison.Ordinal);
                    var moduleId = fileName.Substring(0, separator);
                    var versionText = fileName.Substring(separator + 2);
                    return new { entry.Path, ModuleId = moduleId, VersionText = versionText };
                })
                .Where(entry => int.TryParse(entry.VersionText, out _))
                .GroupBy(entry => entry.ModuleId, StringComparer.Ordinal)
                .Select(group => group.OrderByDescending(entry => int.Parse(entry.VersionText)).First());

            var bundles = new List<ScenarioBundle>();
            foreach (var entry in latestByModule)
            {
                bundles.Add(await Get(entry.ModuleId).ConfigureAwait(false));
            }

            return bundles.OrderBy(bundle => bundle.Id, StringComparer.Ordinal).ToArray();
        }

        private string LatestPath(string moduleId)
        {
            var candidates = Directory.Exists(directory)
                ? Directory.GetFiles(directory, moduleId + ".v*.json")
                : Array.Empty<string>();

            if (candidates.Length == 0)
            {
                return Path.Combine(directory, moduleId + ".missing.json");
            }

            return candidates
                .Select(path => new { Path = path, Version = ParseVersion(path, moduleId) })
                .OrderByDescending(candidate => candidate.Version)
                .First()
                .Path;
        }

        private static int ParseVersion(string path, string moduleId)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var prefix = moduleId + ".v";
            return fileName.StartsWith(prefix, StringComparison.Ordinal)
                && int.TryParse(fileName.Substring(prefix.Length), out var version)
                    ? version
                    : 0;
        }

        private static void ValidateModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId)
                || moduleId.Any(character => !char.IsLetterOrDigit(character) && character != '_' && character != '-'))
            {
                throw new ArgumentException("Module ids may contain letters, digits, underscores, and hyphens.", nameof(moduleId));
            }
        }

        public sealed class ScenarioDocument
        {
            public string Id { get; set; } = string.Empty;

            public int Version { get; set; }

            public int PassScore { get; set; }

            public SceneDocument Scene { get; set; } = new SceneDocument();

            public List<InteractionDocument> Interactions { get; set; } = new List<InteractionDocument>();

            public List<StepDocument> Steps { get; set; } = new List<StepDocument>();

            public ScenarioBundle ToBundle()
            {
                return new ScenarioBundle(
                    Id,
                    Version,
                    PassScore,
                    Steps.Select(step => step.ToStep()).ToArray(),
                    Scene.ToReference(),
                    Interactions.Select(interaction => interaction.ToDefinition()).ToArray());
            }
        }

        public sealed class SceneDocument
        {
            public string SceneId { get; set; } = string.Empty;

            public string PrefabId { get; set; } = string.Empty;

            public ScenarioSceneReference ToReference()
            {
                return new ScenarioSceneReference(SceneId, PrefabId);
            }
        }

        public sealed class InteractionDocument
        {
            public string Id { get; set; } = string.Empty;

            public string Kind { get; set; } = string.Empty;

            public string ActionKind { get; set; } = string.Empty;

            public string TargetId { get; set; } = string.Empty;

            public decimal Threshold { get; set; }

            public List<string> OrderedWaypoints { get; set; } = new List<string>();

            public ScenarioInteractionDefinition ToDefinition()
            {
                if (!Enum.TryParse<SemanticInteractionKind>(Kind, true, out var kind))
                {
                    throw new InvalidDataException("The interaction kind is not supported.");
                }

                return new ScenarioInteractionDefinition(Id, kind, ActionKind, TargetId, Threshold, OrderedWaypoints);
            }
        }

        public sealed class StepDocument
        {
            public string Id { get; set; } = string.Empty;

            public int Score { get; set; }

            public string? CueKey { get; set; }

            public List<ActionDocument> Accept { get; set; } = new List<ActionDocument>();

            public List<WrongActionDocument> WrongActions { get; set; } = new List<WrongActionDocument>();

            public ScenarioStep ToStep()
            {
                return new ScenarioStep(
                    Id,
                    Score,
                    Accept.Select(action => new AcceptedAction(action.Kind, action.TargetId)).ToArray(),
                    WrongActions.Select(action => action.ToWrongAction()).ToArray(),
                    CueKey);
            }
        }

        public class ActionDocument
        {
            public string Kind { get; set; } = string.Empty;

            public string TargetId { get; set; } = string.Empty;
        }

        public sealed class WrongActionDocument : ActionDocument
        {
            public int Penalty { get; set; }

            public bool Critical { get; set; }

            public WrongAction ToWrongAction()
            {
                return new WrongAction(Kind, TargetId, Penalty, Critical);
            }
        }
    }
}
