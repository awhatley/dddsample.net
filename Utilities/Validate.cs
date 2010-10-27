using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainDrivenDelivery.Utilities
{
    public static class Validate
    {
        public static void notNull<T>(T obj, string message)
            where T : class 
        {
            if(obj == null)
                throw new ArgumentNullException(null, message);            
        }

        public static void isTrue(bool condition, string message)
        {
            if(!condition)
                throw new ArgumentException(message);
        }

        public static void notNull<T>(T obj)
            where T : class
        {
            if(obj == null)
                throw new ArgumentNullException();
        }

        public static void notEmpty<T>(IEnumerable<T> collection)
        {
            if(collection == null)
                throw new ArgumentNullException();

            if(!collection.Any())
                throw new ArgumentException();
        }

        public static void notEmpty<T>(IEnumerable<T> collection, string message)
        {
            if(collection == null)
                throw new ArgumentNullException(null, message);

            if(!collection.Any())
                throw new ArgumentException(message);
        }

        public static void noNullElements<T>(IEnumerable<T> collection)
            where T : class
        {
            if(collection == null)
                throw new ArgumentNullException();

            if(collection.Any(x => x == null))
                throw new ArgumentNullException();
        }

        public static void noNullElements<T>(IEnumerable<T> collection, string message)
            where T : class
        {
            if(collection == null)
                throw new ArgumentNullException(null, message);

            if(collection.Any(x => x == null))
                throw new ArgumentNullException(null, message);
        }
    }
}