namespace JobModels
{
    public class TestRequestDescription
    {
        /// <summary>
        /// The image that containes the test runner
        /// </summary>
        public string TestRunImage { get; set; }

        /// <summary>
        /// The command that lists/runs the tests
        /// </summary>
        public string TestRunCommand { get; set; }
    }
}
