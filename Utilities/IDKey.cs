namespace DomainDrivenDelivery.Utilities
{
    /// <summary>
    /// Wrap an identity key (Object.GetHashCode()) so that an object can only be Equal() to itself.
    /// This is necessary to disambiguate the occasional duplicate identityHashCodes that can occur.
    /// </summary>
    internal sealed class IDKey
    {
        private readonly object value;
        private readonly int id;

        public IDKey(object _value)
        {
            id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_value);
            value = _value;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object other)
        {
            if(!(other is IDKey))
            {
                return false;
            }
            
            IDKey idKey = (IDKey)other;
            if(id != idKey.id)
            {
                return false;
            }
            
            // Note that identity equals is used.
            return value == idKey.value;
        }
    }
}