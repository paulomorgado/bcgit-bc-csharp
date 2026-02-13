#if !NETFRAMEWORK
using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace Org.BouncyCastle.Utilities
{
    internal static class Spans
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CopyFrom<T>(this Span<T> output, ReadOnlySpan<T> input)
        {
            input.Slice(0, output.Length).CopyTo(output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<T> FromNullable<T>(T[]? array)
        {
            return array == null ? Span<T>.Empty : array.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<T> FromNullable<T>(T[]? array, int start)
        {
            return array == null ? Span<T>.Empty : array.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<T> FromNullableReadOnly<T>(T[]? array)
        {
            return array == null ? Span<T>.Empty : array.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<T> FromNullableReadOnly<T>(T[]? array, int start)
        {
            return array == null ? Span<T>.Empty : array.AsSpan(start);
        }
    }
}
#endif

#if NETSTANDARD2_0
namespace System.Buffers
{
    public delegate void SpanAction<T, in TArg>(Span<T> span, TArg arg);
 
    public delegate void ReadOnlySpanAction<T, in TArg>(ReadOnlySpan<T> span, TArg arg);
}
#endif
