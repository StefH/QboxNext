using System.Collections;

namespace QboxNext.Qboxes.Parsing.Elements
{
    public class BitArrayUtility
    {
        public BitArrayUtility()
        {
        }

        public static int GetIntFromBitArray(ICollection bitArray)
        {
            var array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }
    }
}