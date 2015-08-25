using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimLibraryAddin
{
    public class RevitVariants
    {
#if Revit2016
        public const string Variant = "Revit 2016";
#elif Revit2015
        public const string Variant = "Revit 2015";
#endif
    }
}
