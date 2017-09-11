using System;
using System.Collections.Generic;
using System.Text;

namespace objview
{
    class MeshContainer : IMesh
    {
        public MeshVertex[] Vertices { get; private set; }

        public int[] Faces { get; private set; }

        public int[] Wires { get; private set; }

        public MeshContainer(MeshVertex[] vertices, int[] faces, int[] wires)
        {
            Vertices = vertices;
            Faces = faces;
            Wires = wires;
        }
    }
}
