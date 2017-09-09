using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace objview
{
    public struct MeshVertex
    {
        public Vertex3f Coord;
        public Vertex3f Normal;
        public Vertex2f TexCoord;

        public static readonly MeshVertex Zero;

        public const int Size = Vertex3f.Size * 2 + Vertex2f.Size;
        public const int CoordOffset = 0;
        public const int NormalOffset = Vertex3f.Size;
        public const int TexCoordOffset = Vertex3f.Size * 2;

        public override bool Equals(Object a)
        {
            return (a != null) && (a is MeshVertex) && ((MeshVertex)a == this);
        }

        public static bool operator ==(MeshVertex a, MeshVertex b)
        {
            return a.Coord == b.Coord &&
                a.Normal == b.Normal &&
                a.TexCoord == b.TexCoord;
        }

        public static bool operator !=(MeshVertex a, MeshVertex b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Coord.GetHashCode() + Normal.GetHashCode();
        }
    }
}
