﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.LogReader
{
    /// <summary>
    /// This proxy extends each log entry with its context information. 
    /// </summary>
    public class LogContextReader : LogReader
    {
        private LogContextReader(ILogReader baseReader, int countPreviousContextEntries, int countNextContextEntries)
        {
            m_BaseReader = baseReader ?? throw new ArgumentNullException(nameof(baseReader));
            m_Enumerator = m_BaseReader.ReadAsync().GetAsyncEnumerator();
            m_CountPreviousContextEntries = Math.Max(0, countPreviousContextEntries);
            m_CountNextContextEntries = Math.Max(0, countNextContextEntries);

        }

        public static async Task<LogContextReader> CreateAsync(ILogReader baseReader, int countPreviousContextEntries, int countNextContextEntries)
        {
            var reader = new LogContextReader(baseReader, countPreviousContextEntries, countNextContextEntries);

            await reader._PreloadAsync().ConfigureAwait(false);

            return reader;
        }

        private async Task _PreloadAsync()
        {
            int i = 0;
            var buffer = new List<LogEntry>();

            // read the candidate and the next entries
            while (await m_Enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                buffer.Add(m_Enumerator.Current);

                if (++i == m_CountNextContextEntries+1) 
                    break;
            }

            // empty stream?
            if (buffer.Count == 0)
            {
                m_Candidate = null;
                return;
            }

            m_Candidate = buffer[0];

            // remove the candidate from the successors
            buffer.RemoveAt(0);

            m_Next = new _RingBuffer<LogEntry>(m_CountNextContextEntries, buffer.ToArray());
            m_Previous = new _RingBuffer<LogEntry>(m_CountPreviousContextEntries, null);
        }


        protected override async IAsyncEnumerable<LogEntry> OnReadAsync(CancellationToken ct)
        {
            if (m_Candidate == null)
                yield break;

            while (await m_Enumerator.MoveNextAsync(ct).ConfigureAwait(false))
            {
                yield return _FinalizeCandidate(m_Candidate);

                m_Previous.Add(m_Candidate);
                m_Candidate = m_Next.Pop();
                m_Next.Add(m_Enumerator.Current);
            }

            yield return _FinalizeCandidate(m_Candidate);

            foreach (var entry in m_Next.ToArray())
            {
                m_Previous.Add(m_Candidate);
                m_Candidate = m_Next.Pop();
                yield return _FinalizeCandidate(m_Candidate);
            }
        }

        private LogEntry _FinalizeCandidate(LogEntry candidate)
        {
            candidate.NextEntries = m_Next.ToArray();
            candidate.PreviousEntries = m_Previous.ToArray();
            return candidate;
        }

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        public override string Display => m_BaseReader.Display;

        private class _RingBuffer<T> : IEnumerable<T>
        {
            private readonly int m_Length;
            private readonly Queue<T> m_Buffer; // TODO real ringe buffer based on an array

            internal _RingBuffer(int length, T[] initialData)
            {
                m_Length = length;
                m_Buffer = new Queue<T>(initialData ?? Enumerable.Empty<T>());
            }

            internal void Add(T entry)
            {
                m_Buffer.Enqueue(entry);
                
                while (m_Buffer.Count > m_Length)
                    m_Buffer.Dequeue();
            }

            internal T Pop()
            {
                m_Buffer.TryDequeue(out var result);
                return result;
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                return m_Buffer.GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        private _RingBuffer<LogEntry> m_Previous;
        private _RingBuffer<LogEntry> m_Next;
        private LogEntry m_Candidate;
        private readonly int m_CountPreviousContextEntries;
        private readonly int m_CountNextContextEntries;
        private readonly ILogReader m_BaseReader;
        private int m_Position;
        private IAsyncEnumerator<LogEntry> m_Enumerator;
    }
}