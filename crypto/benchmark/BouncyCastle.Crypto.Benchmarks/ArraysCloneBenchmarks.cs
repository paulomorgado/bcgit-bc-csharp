using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using Org.BouncyCastle.Utilities;
using Microsoft.VSDiagnostics;

namespace BouncyCastle.Crypto.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [SimpleJob(RuntimeMoniker.Net481)]
    [HideColumns(Column.Error, Column.StdDev, Column.Median)]
    [CPUUsageDiagnoser]
    public class ArraysCloneBenchmarks
    {
        private byte[] smallArray16;
        private byte[] mediumArray128;
        private byte[] largeArray1024;
        private int[] intArray128;
        private long[] longArray128;
        private byte[] resultSmall;
        private byte[] resultMedium;
        private byte[] resultLarge;
        private int[] resultInt;
        private long[] resultLong;
        [GlobalSetup]
        public void Setup()
        {
            smallArray16 = new byte[16];
            mediumArray128 = new byte[128];
            largeArray1024 = new byte[1024];
            intArray128 = new int[128];
            longArray128 = new long[128];
            for (int i = 0; i < 16; i++)
                smallArray16[i] = (byte)i;
            for (int i = 0; i < 128; i++)
            {
                mediumArray128[i] = (byte)i;
                intArray128[i] = i;
                longArray128[i] = i;
            }

            for (int i = 0; i < 1024; i++)
                largeArray1024[i] = (byte)(i & 0xFF);
        }

        [Benchmark]
        public void CloneSmall16Bytes()
        {
            resultSmall = Arrays.Clone(smallArray16);
        }

        [Benchmark]
        public void CloneMedium128Bytes()
        {
            resultMedium = Arrays.Clone(mediumArray128);
        }

        [Benchmark]
        public void CloneLarge1024Bytes()
        {
            resultLarge = Arrays.Clone(largeArray1024);
        }

        [Benchmark]
        public void CloneInt128Elements()
        {
            resultInt = Arrays.Clone(intArray128);
        }

        [Benchmark]
        public void CloneLong128Elements()
        {
            resultLong = Arrays.Clone(longArray128);
        }
    }
}