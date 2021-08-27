using System;
using System.Collections.Generic;
using System.Text;

namespace todozulu.Texts.Helpers
{
    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }

        private NullScope() { }
    }
}
