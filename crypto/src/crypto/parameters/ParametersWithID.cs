using System;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ParametersWithID
        : ICipherParameters
    {
#if !NETFRAMEWORK
        public static ParametersWithID Create<TState>(ICipherParameters parameters, int idLength, TState state,
            System.Buffers.SpanAction<byte, TState> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ParametersWithID result = new ParametersWithID(parameters, idLength);
            action(result.m_id, state);
            return result;
        }
#endif

        private readonly ICipherParameters m_parameters;
        private readonly byte[] m_id;

        public ParametersWithID(ICipherParameters parameters, byte[] id)
        {
            // NOTE: 'parameters' may be null to imply key re-use
            m_parameters = parameters;
            m_id = Arrays.CopyBuffer(id);
        }

        public ParametersWithID(ICipherParameters parameters, byte[] id, int idOff, int idLen)
        {
            // NOTE: 'parameters' may be null to imply key re-use
            m_parameters = parameters;
            m_id = Arrays.CopySegment(id, idOff, idLen);
        }

#if !NETFRAMEWORK
        public ParametersWithID(ICipherParameters parameters, ReadOnlySpan<byte> id)
        {
            // NOTE: 'parameters' may be null to imply key re-use
            m_parameters = parameters;
            m_id = id.ToArray();
        }
#endif

#if !NETFRAMEWORK
        private ParametersWithID(ICipherParameters parameters, int idLength)
        {
            // NOTE: 'parameters' may be null to imply key re-use
            m_parameters = parameters;
            m_id = Arrays.CreateBuffer<byte>(idLength);
        }
#endif

        public void CopyIDTo(byte[] buf, int off, int len) => Arrays.CopyBufferToSegment(m_id, buf, off, len);

        public byte[] GetID() => Arrays.InternalCopyBuffer(m_id);

        public int IDLength => m_id.Length;

#if !NETFRAMEWORK
        internal ReadOnlySpan<byte> InternalID => m_id;
#endif

        public ICipherParameters Parameters => m_parameters;
    }
}
