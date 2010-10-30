using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace DomainDrivenDelivery.Utilities
{
    /// <summary>
    /// Assists in implementing <see cref="Object.GetHashCode"/> methods.
    /// </summary>
    public class HashCodeBuilder : Builder<int>
    {
        private static readonly ThreadLocal<HashSet<IDKey>> REGISTRY = new ThreadLocal<HashSet<IDKey>>(() => new HashSet<IDKey>());

        private static void reflectionAppend(object obj, IReflect type, HashCodeBuilder builder, bool useTransients, IEnumerable<string> excludedFields)
        {
            if(REGISTRY.Value.Contains(new IDKey(obj)))
                return;

            try
            {
                REGISTRY.Value.Add(new IDKey(obj));
                var values = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(field => excludedFields == null || !excludedFields.Contains(field.Name))
                    .Where(field => useTransients || !field.IsNotSerialized)
                    .Select(field => field.GetValue(obj));

                foreach(var value in values)
                    builder.append(value);
            }

            finally
            {
                REGISTRY.Value.Remove(new IDKey(obj));
            }
        }

        public static int reflectionHashCode(object obj)
        {
            return reflectionHashCode(obj, true, null);
        }

        public static int reflectionHashCode(object obj, bool useTransients)
        {
            return reflectionHashCode(obj, useTransients, null);
        }

        public static int reflectionHashCode(object obj, IEnumerable<string> excludedFields)
        {
            return reflectionHashCode(obj, true, excludedFields);
        }

        public static int reflectionHashCode(object obj, bool useTransients, IEnumerable<string> excludedFields)
        {
            if(obj == null)
                throw new ArgumentNullException("obj", "The object to build a hash code for must not be null");

            var builder = new HashCodeBuilder(17, 37);

            for(var type = obj.GetType(); type != null && type != typeof(object); type = type.BaseType)
                reflectionAppend(obj, type, builder, useTransients, excludedFields);

            return builder.toHashCode();
        }

        private readonly int iConstant;
        private int iTotal = 0;

        public HashCodeBuilder()
        {
            iConstant = 37;
            iTotal = 17;
        }

        public HashCodeBuilder(int initialNonZeroOddNumber, int multiplierNonZeroOddNumber)
        {
            if(initialNonZeroOddNumber == 0)
                throw new ArgumentOutOfRangeException("initialNonZeroOddNumber", initialNonZeroOddNumber,
                    "HashCodeBuilder requires a non zero initial value");

            if(initialNonZeroOddNumber % 2 == 0)
                throw new ArgumentOutOfRangeException("initialNonZeroOddNumber", initialNonZeroOddNumber,
                    "HashCodeBuilder requires an odd initial value");

            if(multiplierNonZeroOddNumber == 0)
                throw new ArgumentOutOfRangeException("multiplierNonZeroOddNumber", multiplierNonZeroOddNumber,
                    "HashCodeBuilder requires a non zero multiplier");

            if(multiplierNonZeroOddNumber % 2 == 0)
                throw new ArgumentOutOfRangeException("multiplierNonZeroOddNumber", multiplierNonZeroOddNumber,
                    "HashCodeBuilder requires an odd multiplier");
            
            iConstant = multiplierNonZeroOddNumber;
            iTotal = initialNonZeroOddNumber;
        }

        public HashCodeBuilder append(bool value)
        {
            iTotal = iTotal * iConstant + (value ? 0 : 1);
            return this;
        }

        public HashCodeBuilder append(bool[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(byte value)
        {
            iTotal = iTotal * iConstant + value;
            return this;
        }

        public HashCodeBuilder append(byte[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(char value)
        {
            iTotal = iTotal * iConstant + value;
            return this;
        }

        public HashCodeBuilder append(char[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(double value)
        {
            return append(BitConverter.DoubleToInt64Bits(value));
        }

        public HashCodeBuilder append(double[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(float value)
        {
            iTotal = iTotal * iConstant + BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
            return this;
        }

        public HashCodeBuilder append(float[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(int value)
        {
            iTotal = iTotal * iConstant + value;
            return this;
        }

        public HashCodeBuilder append(int[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(long value)
        {
            iTotal = iTotal * iConstant + ((int)(value ^ (value >> 32)));
            return this;
        }

        public HashCodeBuilder append(long[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(Object obj)
        {
            if(obj == null)
            {
                iTotal = iTotal * iConstant;

            }
            else
            {
                if(obj.GetType().IsArray)
                {
                    // 'Switch' on type of array, to dispatch to the correct handler
                    // This handles multi dimensional arrays
                    if(obj is long[])
                    {
                        append((long[])obj);
                    }
                    else if(obj is int[])
                    {
                        append((int[])obj);
                    }
                    else if(obj is short[])
                    {
                        append((short[])obj);
                    }
                    else if(obj is char[])
                    {
                        append((char[])obj);
                    }
                    else if(obj is byte[])
                    {
                        append((byte[])obj);
                    }
                    else if(obj is double[])
                    {
                        append((double[])obj);
                    }
                    else if(obj is float[])
                    {
                        append((float[])obj);
                    }
                    else if(obj is bool[])
                    {
                        append((bool[])obj);
                    }
                    else
                    {
                        // Not an array of primitives
                        append((object[])obj);
                    }
                }
                else
                {
                    iTotal = iTotal * iConstant + obj.GetHashCode();
                }
            }
            return this;
        }

        public HashCodeBuilder append(Object[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder append(short value)
        {
            iTotal = iTotal * iConstant + value;
            return this;
        }

        public HashCodeBuilder append(short[] array)
        {
            if(array == null)
            {
                iTotal = iTotal * iConstant;
            }
            else
            {
                for(int i = 0; i < array.Length; i++)
                {
                    append(array[i]);
                }
            }
            return this;
        }

        public HashCodeBuilder appendSuper(int superHashCode)
        {
            iTotal = iTotal * iConstant + superHashCode;
            return this;
        }

        public int toHashCode()
        {
            return iTotal;
        }

        public int build()
        {
            return toHashCode();
        }

        public override int GetHashCode()
        {
            return toHashCode();
        }
    }
}