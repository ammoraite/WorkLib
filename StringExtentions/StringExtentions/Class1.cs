namespace StringExtentions
{
    public static class StringExtension
    {
        /// <summary>
        /// Возращает массив типа int из строки в которой они разделены любым не числовым символом
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
       public static int[] GetNumbers (this string s )
        {
            string NT = string.Join ("", s.Trim ( ).Select (x =>
            {
                if (char.IsNumber (x))
                {
                    return x;
                }
                return ' ';
            })).Trim ( );
            while (NT.Contains ("  ")) { NT=NT.Replace ("  ", " "); }
            return NT.Split (" ").Select (x => int.Parse (x)).ToArray ( );
        }
    }
}