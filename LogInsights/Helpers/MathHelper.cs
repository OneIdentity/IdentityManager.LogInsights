using System;
using System.Collections.Generic;
using System.Linq;


namespace LogInsights.Helpers
{
    public static class MathHelper<T> where T:IComparable
    {
        public static T Within(T Val, T A, T B)
        {
            if (Val.CompareTo(A) < 0 /*Val < A*/) 
                return A;

            if (Val.CompareTo(B) > 0 /*Val > B*/)
                return B;

            return Val;
        }

        public static bool isWithin(T Val, T A, T B)
        {
            return ((Val.CompareTo(A) >= 0 /*Val >= A*/) && (Val.CompareTo(B) <= 0 /*Val <= B*/));
        }
    }
}
