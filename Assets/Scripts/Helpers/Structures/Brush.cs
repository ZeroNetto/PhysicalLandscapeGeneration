using System.Collections.Generic;

namespace Helpers.Structires
{
    public struct Brush
    {
        public List<int> IndexOffsets { get; set; }
        public List<float> Weights { get; set; }
    }
}