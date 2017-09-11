using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace objview
{
    public interface IMesh
    {
        MeshVertex[] Vertices { get; }
        int[] Faces { get; }
        int[] Wires { get; }
    }
}
