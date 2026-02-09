using BenchmarkDotNet.Running;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace BouncyCastle.Crypto.Benchmarks
{
    static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
