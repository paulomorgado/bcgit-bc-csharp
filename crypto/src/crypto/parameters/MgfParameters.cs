using System;
#if !NETFRAMEWORK
using System.Buffers;
#endif

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    /// <remarks>Parameters for mask derivation functions.</remarks>
    public sealed class MgfParameters
        : IDerivationParameters
    {
#if !NETFRAMEWORK
        public static MgfParameters Create<TState>(int length, TState state, SpanAction<byte, TState> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            MgfParameters result = new MgfParameters(length);
            action(result.m_seed, state);
            return result;
        }
#endif

        private readonly byte[] m_seed;

        public MgfParameters(byte[] seed)
        {
            m_seed = Arrays.CopyBuffer(seed);
        }

        public MgfParameters(byte[] seed, int off, int len)
        {
            m_seed = Arrays.CopySegment(seed, off, len);
        }

#if !NETFRAMEWORK
        private MgfParameters(int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            m_seed = new byte[length];
        }
#endif

        public void CopySeedTo(byte[] buf, int off, int len) => Arrays.CopyBufferToSegment(m_seed, buf, off, len);

        public byte[] GetSeed() => Arrays.InternalCopyBuffer(m_seed);

        [Obsolete("Use 'CopySeedTo' instead")]
        public void GetSeed(byte[] buffer, int offset) => m_seed.CopyTo(buffer, offset);

#if !NETFRAMEWORK
        public void GetSeed(Span<byte> output)
        {
            m_seed.CopyTo(output);
        }
#endif

#if !NETFRAMEWORK
        internal ReadOnlySpan<byte> InternalSeed => m_seed;
#endif

        public int SeedLength => m_seed.Length;
    }
}
