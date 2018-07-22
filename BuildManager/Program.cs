using Model;
using System;
using System.Threading.Tasks;

namespace BuildManager
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //string input;
            //try
            //{
            //    do
            //    {
            //        Console.WriteLine("Type in a build number and press enter to run the build");
            //        Console.WriteLine("Press enter on it's own to quit");
            //        input = Console.ReadLine();
            //        if (!string.IsNullOrEmpty(input))
            //        {
            //            string[] ary = input.Split(' ');
            //            BuildRunRequest request = new BuildRunRequest {
            //                Build = ary[0],
            //                Image = ary[1]
            //            };
            //            Console.WriteLine("Please wait ...");
            //            BuildController controller = new BuildController();
            //            await controller.KickOffBuild(request);
            //            Console.WriteLine("OK");
            //            Console.WriteLine("===================================================");
            //        }
            //    } while (!string.IsNullOrEmpty(input));

            //} catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    Console.WriteLine(ex.StackTrace);
            //}
            //finally
            //{
            //    Console.ReadLine();
            //}
        }


    }
}
