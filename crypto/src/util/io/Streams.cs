using System;
using System.IO;
#if !NETFRAMEWORK
using System.Buffers;
using System.Runtime.InteropServices;
#endif
#if NETCOREAPP1_0_OR_GREATER || NET45_OR_GREATER || NETSTANDARD1_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Org.BouncyCastle.Utilities.IO
{
    public static class Streams
    {
        private static readonly int MaxStackAlloc = Platform.Is64BitProcess ? 4096 : 1024;

        public static int DefaultBufferSize => MaxStackAlloc;

        public static void CopyTo(Stream source, Stream destination)
        {
            CopyTo(source, destination, DefaultBufferSize);
        }

        public static void CopyTo(Stream source, Stream destination, int bufferSize)
        {
            int bytesRead;
#if NET6_0_OR_GREATER
            if (bufferSize <= MaxStackAlloc)
            {
                Span<byte> buffer = stackalloc byte[bufferSize];
                while ((bytesRead = source.Read(buffer)) != 0)
                {
                    destination.Write(buffer[..bytesRead]);
                }
                return;
            }
#endif
            {
#if NETFRAMEWORK
                byte[] buffer = new byte[bufferSize];
#else
                byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
#endif
                    while ((bytesRead = source.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        destination.Write(buffer, 0, bytesRead);
                    }
#if !NETFRAMEWORK
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
                }
#endif
            }
        }

#if NETCOREAPP1_0_OR_GREATER || NET45_OR_GREATER || NETSTANDARD1_0_OR_GREATER
        public static Task CopyToAsync(Stream source, Stream destination)
        {
            return CopyToAsync(source, destination, DefaultBufferSize);
        }

        public static Task CopyToAsync(Stream source, Stream destination, int bufferSize)
        {
            return CopyToAsync(source, destination, bufferSize, CancellationToken.None);
        }

        public static Task CopyToAsync(Stream source, Stream destination, CancellationToken cancellationToken)
        {
            return CopyToAsync(source, destination, DefaultBufferSize, cancellationToken);
        }

        public static async Task CopyToAsync(Stream source, Stream destination, int bufferSize,
            CancellationToken cancellationToken)
        {
            int bytesRead;
            byte[] buffer = new byte[bufferSize];
#if !NETFRAMEWORK
            while ((bytesRead = await ReadAsync(source, new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false)) != 0)
            {
                await WriteAsync(destination, new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
            }
#else
			while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
			{
				await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
			}
#endif
        }
#endif

        public static void Drain(Stream inStr)
        {
            CopyTo(inStr, Stream.Null, DefaultBufferSize);
        }

        /// <summary>Write the full contents of inStr to the destination stream outStr.</summary>
        /// <param name="inStr">Source stream.</param>
        /// <param name="outStr">Destination stream.</param>
        /// <exception cref="IOException">In case of IO failure.</exception>
        public static void PipeAll(Stream inStr, Stream outStr)
        {
            PipeAll(inStr, outStr, DefaultBufferSize);
        }

        /// <summary>Write the full contents of inStr to the destination stream outStr.</summary>
        /// <param name="inStr">Source stream.</param>
        /// <param name="outStr">Destination stream.</param>
        /// <param name="bufferSize">The size of temporary buffer to use.</param>
        /// <exception cref="IOException">In case of IO failure.</exception>
        public static void PipeAll(Stream inStr, Stream outStr, int bufferSize)
        {
            CopyTo(inStr, outStr, bufferSize);
        }

        /// <summary>
        /// Pipe all bytes from <c>inStr</c> to <c>outStr</c>, throwing <c>StreamFlowException</c> if greater
        /// than <c>limit</c> bytes in <c>inStr</c>.
        /// </summary>
        /// <param name="inStr">
        /// A <see cref="Stream"/>
        /// </param>
        /// <param name="limit">
        /// A <see cref="System.Int64"/>
        /// </param>
        /// <param name="outStr">
        /// A <see cref="Stream"/>
        /// </param>
        /// <returns>The number of bytes actually transferred, if not greater than <c>limit</c></returns>
        /// <exception cref="IOException"></exception>
        public static long PipeAllLimited(Stream inStr, long limit, Stream outStr)
        {
            return PipeAllLimited(inStr, limit, outStr, DefaultBufferSize);
        }

        public static long PipeAllLimited(Stream inStr, long limit, Stream outStr, int bufferSize)
        {
            var limited = new LimitedInputStream(inStr, limit);
            CopyTo(limited, outStr, bufferSize);
            return limit - limited.CurrentLimit;
        }

        public static byte[] ReadAll(Stream inStr)
        {
            MemoryStream buf = new MemoryStream();
            PipeAll(inStr, buf);
            return buf.ToArray();
        }

        [Obsolete("Will be removed")]
        public static byte[] ReadAll(MemoryStream inStr)
        {
            return inStr.ToArray();
        }

        public static byte[] ReadAllLimited(Stream inStr, int limit)
        {
            MemoryStream buf = new MemoryStream();
            PipeAllLimited(inStr, limit, buf);
            return buf.ToArray();
        }

#if !NETFRAMEWORK
        public static Task<int> ReadAsync(Stream source, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return source.ReadAsync(array.Array!, array.Offset, array.Count, cancellationToken);
            }

            return ReadAsyncCompletion(source, buffer, cancellationToken);
        }

        private static async Task<int> ReadAsyncCompletion(Stream source, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            byte[] localBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int result = await source.ReadAsync(localBuffer, 0, buffer.Length, cancellationToken);
                localBuffer.AsSpan(0, result).CopyTo(buffer.Span);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(localBuffer, clearArray: true);
            }
        }
#endif

        public static int ReadFully(Stream inStr, byte[] buf)
        {
            return ReadFully(inStr, buf, 0, buf.Length);
        }

        public static int ReadFully(Stream inStr, byte[] buf, int off, int len)
        {
            int totalRead = 0;
            while (totalRead < len)
            {
                int numRead = inStr.Read(buf, off + totalRead, len - totalRead);
                if (numRead < 1)
                    break;
                totalRead += numRead;
            }
            return totalRead;
        }

#if !NETFRAMEWORK
        public static int ReadFully(Stream inStr, Span<byte> buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int numRead = inStr.Read(buffer.Slice(totalRead));
                if (numRead < 1)
                    break;
                totalRead += numRead;
            }
            return totalRead;
        }
#endif

        public static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            int available = buffer.Length - offset;
            if ((offset | available) < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            int remaining = available - count;
            if ((count | remaining) < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
        }

#if NETCOREAPP1_0_OR_GREATER || NET45_OR_GREATER || NETSTANDARD1_0_OR_GREATER
        internal static async Task WriteAsyncCompletion(Task writeTask, byte[] localBuffer)
        {
            try
            {
                await writeTask.ConfigureAwait(false);
            }
            finally
            {
                Array.Clear(localBuffer, 0, localBuffer.Length);
            }
        }

        internal static Task WriteAsyncDirect(Stream destination, byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            destination.Write(buffer, offset, count);
            return Task.CompletedTask;
        }
#endif

#if !NETFRAMEWORK
        public static Task WriteAsync(Stream destination, ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return destination.WriteAsync(array.Array!, array.Offset, array.Count, cancellationToken);
            }

            return WriteAsyncCompletion(destination, buffer, cancellationToken);
        }

        private static async Task WriteAsyncCompletion(Stream destination, ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken)
        {
            byte[] localBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.Span.CopyTo(localBuffer);
                await destination.WriteAsync(localBuffer, 0, buffer.Length).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(localBuffer, clearArray: true);
            }
        }

        internal static
#if NET6_0_OR_GREATER
            ValueTask
#else
            Task
#endif
            WriteAsyncDirect(Stream destination, ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return
#if NET6_0_OR_GREATER
                    ValueTask
#else
                    Task
#endif
                        .FromCanceled(cancellationToken);

            destination.Write(buffer.Span);
            return
#if NET6_0_OR_GREATER
                ValueTask
#else
                Task
#endif
                    .CompletedTask;
        }
#endif

        /// <exception cref="IOException"></exception>
        public static int WriteBufTo(MemoryStream buf, byte[] output, int offset)
        {
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            if (buf.TryGetBuffer(out var buffer))
            {
                buffer.CopyTo(output, offset);
                return buffer.Count;
            }
#endif

            int size = Convert.ToInt32(buf.Length);
            buf.WriteTo(new MemoryStream(output, offset, size));
            return size;
        }
    }
}

#if !NETFRAMEWORK && (!NETCOREAPP3_1_OR_GREATER && !NETSTANDARD2_1_OR_GREATER)
namespace System.IO
{
    /// <summary>
    /// Extension methods for Stream to support Span&lt;byte&gt; on platforms that don't natively support it.
    /// </summary>
    internal static class StreamSpanExtensions
    {
        private static readonly int MaxStackAlloc = Platform.Is64BitProcess ? 4096 : 1024;

        /// <summary>
        /// Extension method to read into a Span&lt;byte&gt; on Stream for platforms that don't natively support it.
        /// Uses ArrayPool for buffer management.
        /// </summary>
        public static int Read(this Stream stream, Span<byte> buffer)
        {
            byte[] tmp = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int numRead = stream.Read(tmp, 0, buffer.Length);
                if (numRead > 0)
                {
                    tmp.AsSpan(0, numRead).CopyTo(buffer);
                }
                return numRead;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tmp, clearArray: true);
            }
        }

        /// <summary>
        /// Extension method to write from a ReadOnlySpan&lt;byte&gt; on Stream for platforms that don't natively support it.
        /// Uses ArrayPool for buffer management.
        /// </summary>
        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 0)
                return;

            byte[] tmp = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(tmp);
                stream.Write(tmp, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tmp, clearArray: true);
            }
        }
    }
}
#endif

