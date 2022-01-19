using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack
{
    public static class Utils
    {
        public static string FindCommonPath(IEnumerable<string> paths)
        {
            var pathParts = paths.Select(a => Path.GetFullPath(a).Split('\\').ToArray()).ToArray();
            string commonPath = "";
            var curI = 0;
            bool isOver = false;
            while (!isOver)
            {
                foreach (var pathPart in pathParts)
                {
                    if (curI >= pathPart.Length || pathParts[0][curI] != pathPart[curI])
                    {
                        isOver = true;
                        break;
                    }
                }
                if (!isOver)
                    commonPath = Path.Combine(commonPath, pathParts[0][curI]);
                curI++;
            }
            return commonPath;
        }

    }
}
