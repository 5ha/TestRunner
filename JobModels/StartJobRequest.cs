namespace JobModels
{
    /// <summary>
    /// This class is used as the entry point to start off a job run
    /// </summary>
    public class StartJobRequest
    {
        /// <summary>
        /// The image that containes the test runner
        /// </summary>
        public string TestRunImage { get; set; }

        /// <summary>
        /// The command that lists/runs the tests
        /// </summary>
        public string TestRunCommand { get; set; }

        /// <summary>
        /// The compose file setting up the environment to run the tests
        /// </summary>
        public string Yaml { get; set; }
    }
}
