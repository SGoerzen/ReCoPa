using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReCoPa.Network;

// ------------------------------------------------------------
    // Framing (TCP event protocol)
    // Frame: [int32 bodyLen BE][uint16 eventLen BE][event UTF8][payload UTF8]
    // ------------------------------------------------------------
    internal static class Framing
    {
        public static async Task WriteMessageAsync(
            NetworkStream stream,
            string eventName,
            string payload,
            int maxMessageBytes,
            TimeSpan sendTimeout,
            CancellationToken ct)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));
            payload ??= string.Empty;

            var evBytes = Encoding.UTF8.GetBytes(eventName);
            if (evBytes.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(eventName), "eventName too long.");

            var plBytes = Encoding.UTF8.GetBytes(payload);

            int bodyLen = 2 + evBytes.Length + plBytes.Length;
            if (bodyLen <= 0 || bodyLen > maxMessageBytes)
                throw new InvalidDataException($"Message too large: {bodyLen} > {maxMessageBytes}");

            byte[] frame = new byte[4 + bodyLen];
            BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(0, 4), bodyLen);
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(4, 2), (ushort)evBytes.Length);

            Buffer.BlockCopy(evBytes, 0, frame, 6, evBytes.Length);
            Buffer.BlockCopy(plBytes, 0, frame, 6 + evBytes.Length, plBytes.Length);

            using var linked = CreateTimeoutCts(sendTimeout, ct);
            await stream.WriteAsync(frame, 0, frame.Length, linked.Token).ConfigureAwait(false);
            await stream.FlushAsync(linked.Token).ConfigureAwait(false);
        }

        public static async Task<(string EventName, string Payload)> ReadMessageAsync(
            NetworkStream stream,
            int maxMessageBytes,
            TimeSpan receiveTimeout,
            CancellationToken ct)
        {
            byte[] lenBuf = new byte[4];
            await ReadExactAsync(stream, lenBuf, receiveTimeout, ct).ConfigureAwait(false);

            int bodyLen = BinaryPrimitives.ReadInt32BigEndian(lenBuf.AsSpan());
            if (bodyLen <= 0 || bodyLen > maxMessageBytes)
                throw new InvalidDataException($"Invalid body length {bodyLen} (limit {maxMessageBytes}).");

            byte[] body = new byte[bodyLen];
            await ReadExactAsync(stream, body, receiveTimeout, ct).ConfigureAwait(false);

            ushort evLen = BinaryPrimitives.ReadUInt16BigEndian(body.AsSpan(0, 2));
            if (evLen == 0 || 2 + evLen > bodyLen)
                throw new InvalidDataException("Invalid event name length.");

            string ev = Encoding.UTF8.GetString(body, 2, evLen);
            string payload = Encoding.UTF8.GetString(body, 2 + evLen, bodyLen - (2 + evLen));
            return (ev, payload);
        }

        private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, TimeSpan timeout, CancellationToken ct)
        {
            int offset = 0;
            using var linked = CreateTimeoutCts(timeout, ct);

            while (offset < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, linked.Token).ConfigureAwait(false);
                if (read == 0) throw new EndOfStreamException("Remote closed connection.");
                offset += read;
            }
        }

        private static CancellationTokenSource CreateTimeoutCts(TimeSpan timeout, CancellationToken ct)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (timeout > TimeSpan.Zero) cts.CancelAfter(timeout);
            return cts;
        }
    }