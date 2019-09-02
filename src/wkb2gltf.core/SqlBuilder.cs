using System;

namespace Wkb2Gltf
{
    public class SqlBuilder
    {
        public static string GetOptionalColumnsSql(string colorcolumn = "", string attributecolumn = "")
        {
            if (colorcolumn == String.Empty && attributecolumn == String.Empty) {
                return String.Empty;
            }

            var res = (colorcolumn != String.Empty ? ", " + colorcolumn : string.Empty);
            res += (attributecolumn != String.Empty ? ", " + attributecolumn : string.Empty);
            return res;
        }

    }
}
