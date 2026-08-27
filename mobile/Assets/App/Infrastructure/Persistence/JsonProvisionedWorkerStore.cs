using System;
using System.IO;
using Newtonsoft.Json;

namespace SurakshaAR.Infrastructure.Persistence
{
    public sealed class JsonProvisionedWorkerStore
    {
        private readonly string path;

        public JsonProvisionedWorkerStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A path is required.", nameof(path));
            }

            this.path = path;
        }

        public string? Load()
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var document = JsonConvert.DeserializeObject<WorkerDocument>(File.ReadAllText(path));
            return string.IsNullOrWhiteSpace(document?.WorkerId) ? null : document.WorkerId;
        }

        public void Save(string workerId)
        {
            if (string.IsNullOrWhiteSpace(workerId))
            {
                throw new ArgumentException("A worker id is required.", nameof(workerId));
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(new WorkerDocument { WorkerId = workerId }));
        }

        public void Clear()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private sealed class WorkerDocument
        {
            public string WorkerId { get; set; } = string.Empty;
        }
    }
}
