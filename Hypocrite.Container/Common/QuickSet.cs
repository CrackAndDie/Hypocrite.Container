﻿using System;

namespace Hypocrite.Container.Common
{
	public class QuickSet<TValue> where TValue : class
	{
		#region Fields
		private const int HashMask = 0x7FFFFFFF;

		private int _prime;
		private int[] Buckets;
		private LightEntry<TValue>[] Entries;
		public int Count { get; private set; }
		#endregion


		#region Constructors
		public QuickSet()
		{
			var size = Primes[_prime];
			Buckets = new int[size];
			Entries = new LightEntry<TValue>[size];

			for (int i = 0; i < Buckets.Length; i++)
				Buckets[i] = -1;
		}
		#endregion


		#region Public Methods
		public TValue Get(int hashCode, string name)
		{
			var targetBucket = (hashCode & HashMask) % Buckets.Length;
			bool notEmptyName = name.Length > 0;
			bool notEmptyCode = hashCode != 0;

            var i = Buckets[targetBucket];
			LightEntry<TValue> candidate;
            while (i >= 0)
			{
                candidate = Entries[i];
				if ((notEmptyCode && candidate.HashCode != hashCode) || (notEmptyName && candidate.Name != name)) 
				{
					i = candidate.Next;
                    continue; 
				}
				return candidate.Value;
			}

			return null;
		}

		public bool AddOrReplace(int hashCode, string name, TValue value)
		{
			var collisions = 0;
			var targetBucket = (hashCode & HashMask) % Buckets.Length;

			// Check for the existing 
			for (var i = Buckets[targetBucket]; i >= 0; i = Entries[i].Next)
			{
				var candidate = Entries[i];
				if (candidate.HashCode != hashCode || !string.IsNullOrWhiteSpace(name) && candidate.Name != name || !Equals(candidate.Value, value))
				{
					collisions++;
					continue;
				}

				// Already exists and replacing
				candidate.Value = value;
				return false;
			}

			// Expand if required
			if (Count >= Buckets.Length || collisions > Buckets.Length / 2)
			{
				Expand();
				targetBucket = (hashCode & HashMask) % Buckets.Length;
			}

			// Add registration
			ref var entry = ref Entries[Count];
			entry.HashCode = hashCode;
			entry.Name = name;
			entry.Value = value;
			entry.Next = Buckets[targetBucket];
			Buckets[targetBucket] = Count++;

			return true;
		}

        public void Clear()
        {
            Buckets = null;
            Entries = null;
        }
        #endregion

        #region Implementation
        private void Expand()
		{
			var entries = Entries;

			_prime += 1;

			var size = Primes[_prime];
			Buckets = new int[size];
			Entries = new LightEntry<TValue>[size];

			for (int i = 0; i < Buckets.Length; i++)
				Buckets[i] = -1;

			Array.Copy(entries, 0, Entries, 0, Count);
			for (var i = 0; i < Count; i++)
			{
				var hashCode = Entries[i].HashCode & HashMask;
				if (hashCode < 0) continue;

				var bucket = hashCode % Buckets.Length;
				Entries[i].Next = Buckets[bucket];
				Buckets[bucket] = i;
			}
		}

		public static readonly int[] Primes = {
			11, 37, 71, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597,
			1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
			25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751,
			225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
			1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

		#endregion
	}

    public class QuickQuickSet<TValue> where TValue : class
    {
        #region Fields
        private const int HashMask = 0x7FFFFFFF;

        private int _prime;
        private int[] Buckets;
        private LightLightEntry<TValue>[] Entries;
        public int Count { get; private set; }
        #endregion


        #region Constructors
        public QuickQuickSet()
        {
            var size = Primes[_prime];
            Buckets = new int[size];
            Entries = new LightLightEntry<TValue>[size];

            for (int i = 0; i < Buckets.Length; i++)
                Buckets[i] = -1;
        }
        #endregion


        #region Public Methods
        public TValue Get(int hashCode)
        {
            var targetBucket = (hashCode & HashMask) % Buckets.Length;
            bool notEmptyCode = hashCode != 0;

            var i = Buckets[targetBucket];
            LightLightEntry<TValue> candidate;
            while (i >= 0)
            {
                candidate = Entries[i];
                if ((notEmptyCode && candidate.HashCode != hashCode))
                {
                    i = candidate.Next;
                    continue;
                }
                return candidate.Value;
            }

            return null;
        }

        public bool AddOrReplace(int hashCode, TValue value)
        {
            var collisions = 0;
            var targetBucket = (hashCode & HashMask) % Buckets.Length;

            // Check for the existing 
            for (var i = Buckets[targetBucket]; i >= 0; i = Entries[i].Next)
            {
                var candidate = Entries[i];
                if (candidate.HashCode != hashCode || !Equals(candidate.Value, value))
                {
                    collisions++;
                    continue;
                }

                // Already exists and replacing
                candidate.Value = value;
                return false;
            }

            // Expand if required
            if (Count >= Buckets.Length || collisions > Buckets.Length / 2)
            {
                Expand();
                targetBucket = (hashCode & HashMask) % Buckets.Length;
            }

            // Add registration
            ref var entry = ref Entries[Count];
            entry.HashCode = hashCode;
            entry.Value = value;
            entry.Next = Buckets[targetBucket];
            Buckets[targetBucket] = Count++;

            return true;
        }

        public void Clear()
        {
            Buckets = null;
            Entries = null;
        }
        #endregion

        #region Implementation
        private void Expand()
        {
            var entries = Entries;

            _prime += 1;

            var size = Primes[_prime];
            Buckets = new int[size];
            Entries = new LightLightEntry<TValue>[size];

            for (int i = 0; i < Buckets.Length; i++)
                Buckets[i] = -1;

            Array.Copy(entries, 0, Entries, 0, Count);
            for (var i = 0; i < Count; i++)
            {
                var hashCode = Entries[i].HashCode & HashMask;
                if (hashCode < 0) continue;

                var bucket = hashCode % Buckets.Length;
                Entries[i].Next = Buckets[bucket];
                Buckets[bucket] = i;
            }
        }

        public static readonly int[] Primes = {
            11, 37, 71, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597,
            1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
            25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751,
            225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

        #endregion
    }
}
