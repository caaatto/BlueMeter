using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Analyze.Exceptions
{
    public class DataTamperedException : Exception
    {
        public DataTamperedException() : base() { }
        public DataTamperedException(string? message) : base(message) { }
    }
}
