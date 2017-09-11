using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// We need several types defined in OpenGL namespace,
// but we don't use any GL function in this file.
using OpenGL;

namespace objview
{
    public class ObjMesh
    {
        #region public methods

        public static IMesh FromFile(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return FromStream(stream);
            }
        }

        public static IMesh FromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, true))
            {
                return FromTextReader(reader);
            }
        }

        public static IMesh FromTextReader(TextReader reader)
        {
            var instance = new ObjMesh();
            instance.Read(reader);
            return instance.Build();
        }

        #endregion

        protected ObjMesh()
        {
        }

        #region Reading raw obj data

        protected class VertexInfo
        {
            public int V;
            public int Vt;
            public int Vn;
        }

        protected List<Vertex3f> ObjV = new List<Vertex3f>();

        protected List<Vertex2f> ObjVt = new List<Vertex2f>();

        protected List<Vertex3f> ObjVn = new List<Vertex3f>();

        protected List<VertexInfo[]> ObjF = new List<VertexInfo[]>();

        protected enum Key
        {
            V,
            VT,
            VN,
            F,
            G,
            MTLLIB,
            USEMTL,
            S,
            O,
            P,
            L,
        }

        protected static string[] Keywords
            = Enum.GetNames(typeof(Key)).Select(name => name.ToLowerInvariant()).ToArray();

        protected void Read(TextReader reader)
        {
            for (;;)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (line.Length < 1) continue;
                if (line[0] == '#') continue;

                switch (GetObjKey(line))
                {
                    case Key.V:
                        ObjV.Add(GetVertex3f(line));
                        break;
                    case Key.VT:
                        ObjVt.Add(GetVertex2f(line));
                        break;
                    case Key.VN:
                        ObjVn.Add(GetVertex3f(line));
                        break;
                    case Key.F:
                        ObjF.Add(GetVertexInfoList(line));
                        break;
                    case Key.G:
                    case Key.MTLLIB:
                    case Key.USEMTL:
                    case Key.S:
                    case Key.O:
                    case Key.P:
                    case Key.L:
                        break;
                }
            }
        }

        protected static Key GetObjKey(string line)
        {
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (line.StartsWith(Keywords[i]))
                {
                    var c = line[Keywords[i].Length];
                    if (c == ' ' || c == '\t') return (Key)i;
                }
            }
            throw new Exception(string.Format("unrecognized obj line: {0}", line));
        }

        protected static char[] DELIMITERS = { ' ', '\t' };

        protected static Vertex2f GetVertex2f(string line)
        {
            var items = line.Split(DELIMITERS, 4, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 3) throw new Exception(string.Format("invalid obj line: {0}", line));
            return new Vertex2f(
                float.Parse(items[1], CultureInfo.InvariantCulture),
                float.Parse(items[2], CultureInfo.InvariantCulture));
        }

        protected static Vertex3f GetVertex3f(string line)
        {
            var items = line.Split(DELIMITERS, 5, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 4) throw new Exception(string.Format("invalid obj line: {0}", line));
            return new Vertex3f(
                float.Parse(items[1], CultureInfo.InvariantCulture),
                float.Parse(items[2], CultureInfo.InvariantCulture),
                float.Parse(items[3], CultureInfo.InvariantCulture));
        }

        protected VertexInfo[] GetVertexInfoList(string line)
        {
            var items = line.Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length < 4) throw new Exception(string.Format("invalid obj line: {0}", line));
            var a = new VertexInfo[items.Length - 1];
            for (int i = 1; i < items.Length; i++)
            {
                a[i - 1] = ParseVertexInfo(items[i]);
            }
            return a;
        }

        protected static char[] SLANT = { '/' };

        protected VertexInfo ParseVertexInfo(string info)
        {
            var items = info.Split(SLANT, 4, StringSplitOptions.None);
            switch (items.Length)
            {
                case 1:
                    return new VertexInfo()
                    {
                        V = GetIndex(items[0], ObjV.Count)
                    };
                case 2:
                    return new VertexInfo()
                    {
                        V = GetIndex(items[0], ObjV.Count),
                        Vt = GetIndex(items[1], ObjVt.Count)
                    };
                case 3:
                    return new VertexInfo()
                    {
                        V = GetIndex(items[0], ObjV.Count),
                        Vt = GetIndex(items[1], ObjVt.Count),
                        Vn = GetIndex(items[2], ObjVn.Count)
                    };
                default:
                    throw new Exception();
            }
        }

        protected static int GetIndex(string value, int count)
        {
            if (value == "") return 0;
            var n = int.Parse(value);
            if (n >= 0) return n;
            n += count + 1;
            if (n > 0) return n;
            throw new Exception();
        }

        #endregion

        #region IMesh building

        protected List<MeshVertex> Vertices;

        protected List<int> Faces;

        protected HashSet<EdgeInfo> Edges;

        protected IMesh Build()
        {
            Vertices = new List<MeshVertex>(ObjV.Count * 4);
            Faces = new List<int>(ObjF.Count * 3);
            Edges = new HashSet<EdgeInfo>();

            foreach (var f in ObjF)
            {
                var p = AddMeshVertex(f[0]);
                var q = AddMeshVertex(f[1]);
                AddMeshEdge(p, q);
                for (int i = 2; i < f.Length; i++)
                {
                    var r = AddMeshVertex(f[i]);
                    AddMeshTriangle(p, q, r);
                    AddMeshEdge(q, r);
                    q = r;
                }
                AddMeshEdge(q, p);
            }

            return new MeshContainer(Vertices.ToArray(), Faces.ToArray(), GetEdgesArray());
        }

        protected int AddMeshVertex(VertexInfo info)
        {
            var v = new MeshVertex();
            v.Coord = info.V <= 0 ? Vertex3f.Zero : ObjV[info.V - 1];
            v.Normal = info.Vn <= 0 ? Vertex3f.Zero : ObjVn[info.Vn - 1];
            v.TexCoord = info.Vt <= 0 ? Vertex2f.Zero : ObjVt[info.Vt - 1];

            var i = Vertices.Count;
            Vertices.Add(v);
            return i;
        }

        protected void AddMeshTriangle(int i, int j, int k)
        {
            Faces.Add(i);
            Faces.Add(j);
            Faces.Add(k);
        }

        protected void AddMeshEdge(int i, int j)
        {
            Edges.Add(new EdgeInfo(Vertices[i].Coord, Vertices[j].Coord, i, j));
        }

        protected int[] GetEdgesArray()
        {
            var array = new int[Edges.Count * 2];
            int i = 0;
            foreach (var e in Edges)
            {
                array[i++] = e.IA;
                array[i++] = e.IB;
            }
            return array;
        }

        protected class EdgeInfo
        {
            private readonly Vertex3f A, B;

            public readonly int IA, IB;

            private readonly int HashCode;

            public EdgeInfo(Vertex3f a, Vertex3f b, int ia, int ib)
            {
                A = a;
                B = b;
                IA = ia;
                IB = ib;
                HashCode = a.GetHashCode() + b.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EdgeInfo)) return false;
                var e = obj as EdgeInfo;
                return (A == e.A && B == e.B) || (A == e.B && B == e.A);
            }

            public override int GetHashCode()
            {
                return HashCode;
            }
        }

        #endregion
    }
}
