using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;

namespace BouncyCastle.Crypto.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [SimpleJob(RuntimeMoniker.Net481)]
    [HideColumns(Column.Error, Column.StdDev, Column.Median, Column.Job)]
    public class CloneImplementationComparison
    {
        [Params(16, 128, 1024)]
        public int ArraySize;

        private byte[] sourceArray;

        [GlobalSetup]
        public void Setup()
        {
            sourceArray = new byte[ArraySize];
            for (int i = 0; i < ArraySize; i++)
                sourceArray[i] = (byte)(i & 0xFF);
        }

        [Benchmark(Baseline = true)]
        public byte[] ArrayClone()
        {
            return (byte[])sourceArray.Clone();
        }

        [Benchmark]
        public byte[] NewArrayBlockCopy()
        {
#if NET5_0_OR_GREATER
            byte[] temp = GC.AllocateUninitializedArray<byte>(sourceArray.Length);
#else
            byte[] temp = new byte[sourceArray.Length];
#endif
            Buffer.BlockCopy(sourceArray, 0, temp, 0, sourceArray.Length);
            return temp;
        }
    }
}

