namespace ComposeManager.Config
{
    public class AppSettings
    {
        /// <summary>
        /// The number of simultaneous threads running jobs
        /// </summary>
        public int ProcessCount { get; set; }

        /// <summary>
        /// The maximu number of minutes composed will be allowed to run before being 'downed'
        /// </summary>
        public int MaxComposeExecutionTimeInMinutes { get; set; }

        /// <summary>
        /// The local folder from which compose files will be executed
        /// Each process will execute compose in its own folder being a subfolder in this path
        /// </summary>
        public string YamlBasePath { get; set; }
    }
}
