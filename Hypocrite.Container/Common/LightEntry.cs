namespace Hypocrite.Container.Common
{
	public struct LightEntry<TValue>
	{
		public int HashCode;
		public string Name;
		public TValue Value;
		public int Next;

		public override string ToString()
		{
			return $"Name: {Name}, Val: {Value}, Next: {Next}";
		}
	}

    public struct LightLightEntry<TValue>
    {
        public int HashCode;
        public TValue Value;
        public int Next;

        public override string ToString()
        {
            return $"Val: {Value}, Next: {Next}";
        }
    }
}
