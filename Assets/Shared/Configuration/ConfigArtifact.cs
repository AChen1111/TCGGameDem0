using System;

namespace AChen.Configuration
{
    [Serializable]
    public sealed class ConfigArtifact
    {
        public string category;
        public string address;
        public string format;
        public string path;
        public long size;
        public string sha256;
    }
}
