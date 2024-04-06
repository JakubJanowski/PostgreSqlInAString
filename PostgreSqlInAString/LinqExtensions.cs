using System;
using System.Collections.Generic;

namespace PostgreSqlInAString {
    internal static class LinqExtensions {
        // Implementation based on .NET 6+ LINQ state as of commit 1cc0fcd1b06c52eacdf52dc6fc72fa86b3a17014

        /// <summary>Returns the maximum value in a generic sequence according to a specified key selector function.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>The value with the maximum key in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No key extracted from <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="System.IComparable{TKey}" /> interface.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source" /> sequence is empty and <typeparamref name="TSource" />'s default value is not null.</exception>
        /// <remarks>
        /// <para>If <typeparamref name="TKey" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => MaxBy(source, keySelector, null);

        /// <summary>Returns the maximum value in a generic sequence according to a specified key selector function.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">The <see cref="IComparer{TKey}" /> to compare keys.</param>
        /// <returns>The value with the maximum key in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No key extracted from <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{TKey}" /> interface.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source" /> sequence is empty and <typeparamref name="TSource" />'s default value is not null.</exception>
        /// <remarks>
        /// <para>If <typeparamref name="TKey" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source is null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector is null) {
                throw new ArgumentNullException(nameof(keySelector));
            }

            comparer = comparer ?? Comparer<TKey>.Default;

            using (IEnumerator<TSource> e = source.GetEnumerator()) {

                if (!e.MoveNext()) {
                    if (default(TSource) == null) {
                        return default;
                    } else {
                        throw new InvalidOperationException("Sequence contains no elements");
                    }
                }

                TSource value = e.Current;
                TKey key = keySelector(value);

                if (default(TKey) == null) {
                    if (key == null) {
                        TSource firstValue = value;

                        do {
                            if (!e.MoveNext()) {
                                // All keys are null, surface the first element.
                                return firstValue;
                            }

                            value = e.Current;
                            key = keySelector(value);
                        }
                        while (key == null);
                    }

                    while (e.MoveNext()) {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (nextKey != null && comparer.Compare(nextKey, key) > 0) {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                } else {
                    if (comparer == Comparer<TKey>.Default) {
                        while (e.MoveNext()) {
                            TSource nextValue = e.Current;
                            TKey nextKey = keySelector(nextValue);
                            if (Comparer<TKey>.Default.Compare(nextKey, key) > 0) {
                                key = nextKey;
                                value = nextValue;
                            }
                        }
                    } else {
                        while (e.MoveNext()) {
                            TSource nextValue = e.Current;
                            TKey nextKey = keySelector(nextValue);
                            if (comparer.Compare(nextKey, key) > 0) {
                                key = nextKey;
                                value = nextValue;
                            }
                        }
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>The value with the minimum key in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No key extracted from <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="System.IComparable{TKey}" /> interface.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source" /> sequence is empty and <typeparamref name="TSource" />'s default value is not null.</exception>
        /// <remarks>
        /// <para>If <typeparamref name="TKey" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => MinBy(source, keySelector, comparer: null);

        /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">The <see cref="IComparer{TKey}" /> to compare keys.</param>
        /// <returns>The value with the minimum key in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No key extracted from <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{TKey}" /> interface.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source" /> sequence is empty and <typeparamref name="TSource" />'s default value is not null.</exception>
        /// <remarks>
        /// <para>If <typeparamref name="TKey" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source is null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector is null) {
                throw new ArgumentNullException(nameof(keySelector));
            }

            comparer = comparer ?? Comparer<TKey>.Default;

            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (!e.MoveNext()) {
                    if (default(TSource) == null) {
                        return default;
                    } else {
                        throw new InvalidOperationException("Sequence contains no elements");
                    }
                }

                TSource value = e.Current;
                TKey key = keySelector(value);

                if (default(TKey) == null) {
                    if (key == null) {
                        TSource firstValue = value;

                        do {
                            if (!e.MoveNext()) {
                                // All keys are null, surface the first element.
                                return firstValue;
                            }

                            value = e.Current;
                            key = keySelector(value);
                        }
                        while (key == null);
                    }

                    while (e.MoveNext()) {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (nextKey != null && comparer.Compare(nextKey, key) < 0) {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                } else {
                    if (comparer == Comparer<TKey>.Default) {
                        while (e.MoveNext()) {
                            TSource nextValue = e.Current;
                            TKey nextKey = keySelector(nextValue);
                            if (Comparer<TKey>.Default.Compare(nextKey, key) < 0) {
                                key = nextKey;
                                value = nextValue;
                            }
                        }
                    } else {
                        while (e.MoveNext()) {
                            TSource nextValue = e.Current;
                            TKey nextKey = keySelector(nextValue);
                            if (comparer.Compare(nextKey, key) < 0) {
                                key = nextKey;
                                value = nextValue;
                            }
                        }
                    }
                }

                return value;
            }
        }
    }
}
