using System;
using System.Collections.Generic;
using System.Text;

namespace objview
{
    public interface IModel : IDisposable
    {
        float BoundingRadius { get; }

        void Draw();
    }
}
