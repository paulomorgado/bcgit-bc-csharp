using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyCastle.Crypto.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [SimpleJob(RuntimeMoniker.Net481)]
    [HideColumns(Column.Error, Column.StdDev, Column.Median)]
    public class AesEngineBenchmarks
    {
        private KeyParameter key;
        private IBlockCipher aesForInit;
        private IBlockCipher aesForEncrypt;
        private byte[] output = new byte[256];
        private byte[] iv = new byte[] { 0x59, 0x5B, 0x69, 0x9B, 0xBD, 0x3B, 0xC0, 0xDF, 0x26, 0x06, 0x20, 0x93, 0xC1, 0xAD, 0x8F, 0x73 };

        [IterationSetup]
        public void IterationSetup()
        {
            key = new KeyParameter(new byte[] { 0x23, 0x48, 0x29, 0x00, 0x84, 0x67, 0xbe, 0x18, 0x6c, 0x3d, 0xe1, 0x4a, 0xae, 0x72, 0xd6, 0x2c });

            aesForInit = AesUtilities.CreateEngine();

            aesForEncrypt = AesUtilities.CreateEngine();
            aesForEncrypt.Init(true, key);
        }

        [Benchmark]
        public void Init()
        {
            aesForInit.Init(true, key);
        }

        [Benchmark]
        public void ProcessBlock()
        {
            aesForEncrypt.ProcessBlock(iv, 0, output, 16);
        }
    }
}
