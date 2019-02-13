using System;

namespace QboxNext.Core.Utils
{
    public class Guard
    {
        public static void IsBefore(DateTime from, DateTime to, string p)
        {
            if (from > to)
                throw new ArgumentOutOfRangeException(p);
        }

        public static void IsNotNull(object obj, string p)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(p);
            }
        }

        public static void IsNotNullOrEmpty(string id, string p)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(id, p);
            }
        }

        public static void IsNotNullOrEmpty(byte[] data, string p)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException(p);
            }
        }

		public static void IsTrue(bool b, string p)
		{
			if (!b)
			{
				throw new ArgumentException(p);
			}
		}

	    public static void IsFalse(bool b, string p)
	    {
		    if (b)
		    {
			    throw new ArgumentException(p);
		    }
	    }
    }
}
