using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DomainDrivenDelivery.Utilities
{
    public class EqualsBuilder : Builder<bool>
    {

        private static readonly ThreadLocal<HashSet<Tuple<IDKey, IDKey>>> REGISTRY =
            new ThreadLocal<HashSet<Tuple<IDKey, IDKey>>>(() => new HashSet<Tuple<IDKey, IDKey>>());

        static Tuple<IDKey, IDKey> getRegisterPair(Object lhs, Object rhs)
        {
            IDKey left = new IDKey(lhs);
            IDKey right = new IDKey(rhs);
            return Tuple.Create(left, right);
        }

        static bool isRegistered(Object lhs, Object rhs)
        {
            Tuple<IDKey, IDKey> pair = getRegisterPair(lhs, rhs);
            Tuple<IDKey, IDKey> swappedPair = Tuple.Create(pair.Item1, pair.Item2);

            return REGISTRY.Value.Contains(pair) || REGISTRY.Value.Contains(swappedPair);
        }

        private bool value = true;

        public static bool reflectionEquals(Object lhs, Object rhs)
        {
            return reflectionEquals(lhs, rhs, true, null);
        }

        public static bool reflectionEquals(Object lhs, Object rhs, IEnumerable<String> excludedFields)
        {
            return reflectionEquals(lhs, rhs, true, excludedFields);
        }

        public static bool reflectionEquals(Object lhs, Object rhs, bool testTransients)
        {
            return reflectionEquals(lhs, rhs, testTransients, null);
        }

        public static bool reflectionEquals(Object lhs, Object rhs, bool testTransients, IEnumerable<String> excludedFields)
        {
            if(lhs == rhs)
            {
                return true;
            }
            if(lhs == null || rhs == null)
            {
                return false;
            }
            // Find the leaf class since there may be transients in the leaf 
            // class or in classes between the leaf and root.
            // If we are not testing transients or a subclass has no ivars, 
            // then a subclass can test equals to a superclass.
            var lhsClass = lhs.GetType();
            var rhsClass = rhs.GetType();
            Type testClass;
            if(lhsClass.IsInstanceOfType(rhs))
            {
                testClass = lhsClass;
                if(!rhsClass.IsInstanceOfType(lhs))
                {
                    // rhsClass is a subclass of lhsClass
                    testClass = rhsClass;
                }
            }
            else if(rhsClass.IsInstanceOfType(lhs))
            {
                testClass = rhsClass;
                if(!lhsClass.IsInstanceOfType(rhs))
                {
                    // lhsClass is a subclass of rhsClass
                    testClass = lhsClass;
                }
            }
            else
            {
                // The two classes are not related.
                return false;
            }
            EqualsBuilder equalsBuilder = new EqualsBuilder();
            try
            {
                reflectionAppend(lhs, rhs, testClass, equalsBuilder, testTransients, excludedFields);
                while(testClass.BaseType != null)
                {
                    testClass = testClass.BaseType;
                    reflectionAppend(lhs, rhs, testClass, equalsBuilder, testTransients, excludedFields);
                }
            }
            catch(ArgumentException e)
            {
                // In this case, we tried to test a subclass vs. a superclass and
                // the subclass has ivars or the ivars are transient and 
                // we are testing transients.
                // If a subclass has ivars that we are trying to test them, we get an
                // exception and we know that the objects are not equal.
                return false;
            }
            return equalsBuilder.value;
        }

        private static void reflectionAppend(
            Object lhs,
            Object rhs,
            Type type,
            EqualsBuilder builder,
            bool useTransients,
            IEnumerable<string> excludedFields)
        {

            if(isRegistered(lhs, rhs))
            {
                return;
            }

            try
            {
                REGISTRY.Value.Add(Tuple.Create(new IDKey(lhs), new IDKey(rhs)));
                var values = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(field => excludedFields == null || !excludedFields.Contains(field.Name))
                    .Where(field => useTransients || !field.IsNotSerialized)
                    .Select(field => new {
                        lhs = field.GetValue(lhs),
                        rhs = field.GetValue(rhs)
                    });

                foreach(var value in values)
                    builder.append(value.lhs, value.rhs);
            }

            finally
            {
                REGISTRY.Value.Remove(Tuple.Create(new IDKey(lhs), new IDKey(rhs)));
            }
        }

        public EqualsBuilder appendSuper(bool superEquals)
        {
            if(value == false)
            {
                return this;
            }
            value = superEquals;
            return this;
        }

        public EqualsBuilder append(Object lhs, Object rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            var lhsClass = lhs.GetType();
            if(!lhsClass.IsArray)
            {
                // The simple case, not an array, just test the element
                value = lhs.Equals(rhs);
            }
            else if(lhs.GetType() != rhs.GetType())
            {
                // Here when we compare different dimensions, for example: a bool[][] to a bool[] 
                this.setEquals(false);
            }
            // 'Switch' on type of array, to dispatch to the correct handler
            // This handles multi dimensional arrays of the same depth
            else if(lhs is long[])
            {
                append((long[])lhs, (long[])rhs);
            }
            else if(lhs is int[])
            {
                append((int[])lhs, (int[])rhs);
            }
            else if(lhs is short[])
            {
                append((short[])lhs, (short[])rhs);
            }
            else if(lhs is char[])
            {
                append((char[])lhs, (char[])rhs);
            }
            else if(lhs is byte[])
            {
                append((byte[])lhs, (byte[])rhs);
            }
            else if(lhs is double[])
            {
                append((double[])lhs, (double[])rhs);
            }
            else if(lhs is float[])
            {
                append((float[])lhs, (float[])rhs);
            }
            else if(lhs is bool[])
            {
                append((bool[])lhs, (bool[])rhs);
            }
            else
            {
                // Not an array of primitives
                append((Object[])lhs, (Object[])rhs);
            }
            return this;
        }

        public EqualsBuilder append(long lhs, long rhs)
        {
            if(value == false)
            {
                return this;
            }
            value = (lhs == rhs);
            return this;
        }

        public EqualsBuilder append(int lhs, int rhs)
        {
            if(value == false)
            {
                return this;
            }
            value = (lhs == rhs);
            return this;
        }

        public EqualsBuilder append(short lhs, short rhs)
        {
            if(value == false)
            {
                return this;
            }
            value = (lhs == rhs);
            return this;
        }

        public EqualsBuilder append(char lhs, char rhs)
        {
            if(value == false)
            {
                return this;
            }
            value = (lhs == rhs);
            return this;
        }

        public EqualsBuilder append(byte lhs, byte rhs)
        {
            if(value == false)
            {
                return this;
            }
            value = (lhs == rhs);
            return this;
        }

        public EqualsBuilder append(double lhs, double rhs)
        {
            if(value == false)
            {
                return this;
            }
            return append(BitConverter.DoubleToInt64Bits(lhs), BitConverter.DoubleToInt64Bits(rhs));
        }

        public EqualsBuilder append(float lhs, float rhs)
        {
            if(value == false)
            {
                return this;
            }
            return append(BitConverter.ToInt32(BitConverter.GetBytes(lhs), 0),
                BitConverter.ToInt32(BitConverter.GetBytes(rhs), 0));
        }

        public EqualsBuilder append(bool lhs, bool rhs)
        {
            if(value == false)
            {
                return this;
            }
            value = (lhs == rhs);
            return this;
        }

        public EqualsBuilder append(Object[] lhs, Object[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(long[] lhs, long[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(int[] lhs, int[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(short[] lhs, short[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(char[] lhs, char[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(byte[] lhs, byte[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }


        public EqualsBuilder append(double[] lhs, double[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(float[] lhs, float[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public EqualsBuilder append(bool[] lhs, bool[] rhs)
        {
            if(value == false)
            {
                return this;
            }
            if(lhs == rhs)
            {
                return this;
            }
            if(lhs == null || rhs == null)
            {
                this.setEquals(false);
                return this;
            }
            if(lhs.Length != rhs.Length)
            {
                this.setEquals(false);
                return this;
            }
            for(int i = 0; i < lhs.Length && value; ++i)
            {
                append(lhs[i], rhs[i]);
            }
            return this;
        }

        public bool build()
        {
            return value;
        }

        public bool isEquals()
        {
            return value;
        }

        protected void setEquals(bool isEquals)
        {
            this.value = isEquals;
        }

        public void reset()
        {
            this.value = true;
        }
    }
}