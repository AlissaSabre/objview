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
    public class ObjMesh : IMesh
    {
        public MeshVertex[] Vertices { get; protected set; }

        public int[] Faces { get; protected set; }

        protected class VertexInfo
        {
            public int V;
            public int Vt;
            public int Vn;
        }

        protected class RawData
        {
            public List<Vertex3f> V = new List<Vertex3f>();
            public List<Vertex2f> Vt = new List<Vertex2f>();
            public List<Vertex3f> Vn = new List<Vertex3f>();
            public List<VertexInfo[]> F = new List<VertexInfo[]>();
        }

        public static ObjMesh FromFile(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return FromStream(stream);
            }
        }

        public static ObjMesh FromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, true))
            {
                return FromTextReader(reader);
            }
        }

        public static ObjMesh FromTextReader(TextReader reader)
        {
            var obj = new RawData();
            for (;;)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (line.Length < 1) continue;
                if (line[0] == '#') continue;

                switch (GetObjKey(line))
                {
                    case Key.V:
                        obj.V.Add(GetVertex3f(line));
                        break;
                    case Key.VT:
                        obj.Vt.Add(GetVertex2f(line));
                        break;
                    case Key.VN:
                        obj.Vn.Add(GetVertex3f(line));
                        break;
                    case Key.F:
                        obj.F.Add(GetVertexInfoList(line, obj));
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

            var vertices = new List<MeshVertex>(obj.V.Count * 2);
            var faces = new List<int>(obj.F.Count * 4);
            foreach (var f in obj.F)
            {
                var p = Index(f[0], obj, vertices);
                var q = Index(f[1], obj, vertices);
                for (int i = 2; i < f.Length; i++)
                {
                    faces.Add(p);
                    faces.Add(q);
                    faces.Add(q = Index(f[i], obj, vertices));
                }
            }

            return new ObjMesh() { Vertices = vertices.ToArray(), Faces = faces.ToArray() };
        }

        protected static int Index(VertexInfo info, RawData data, List<MeshVertex> vertices)
        {
            var v = new MeshVertex();
            v.Coord = info.V <= 0 ? Vertex3f.Zero : data.V[info.V - 1];
            v.Normal = info.Vn <= 0 ? Vertex3f.Zero : data.Vn[info.Vn - 1];
            v.TexCoord = info.Vt <= 0 ? Vertex2f.Zero : data.Vt[info.Vt - 1];
#if true
            var i = vertices.Count;
            vertices.Add(v);
#else
            var i = vertices.IndexOf(v);
            if (i < 0)
            {
                i = vertices.Count;
                vertices.Add(v);
            }
#endif
            return i;
        }

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

        // protected static Key[] Keys = Enum.GetValues(typeof(Key)) as Key[];

        protected static string[] Keywords = (Enum.GetValues(typeof(Key)) as Key[]).Select(k => k.ToString().ToLowerInvariant()).ToArray();

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

        protected static VertexInfo[] GetVertexInfoList(string line, RawData data)
        {
            var items = line.Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length < 4) throw new Exception(string.Format("invalid obj line: {0}", line));
            var a = new VertexInfo[items.Length - 1];
            for (int i = 1; i < items.Length; i++)
            {
                a[i - 1] = ParseVertexInfo(items[i], data);
            }
            return a;
        }

        protected static char[] SLANT = { '/' };

        protected static VertexInfo ParseVertexInfo(string info, RawData data)
        {
            var items = info.Split(SLANT, 4, StringSplitOptions.None);
            switch (items.Length)
            {
                case 1:
                    return new VertexInfo()
                    {
                        V = GetIndex(items[0], data.V.Count)
                    };
                case 2:
                    return new VertexInfo()
                    {
                        V = GetIndex(items[0], data.V.Count),
                        Vt = GetIndex(items[1], data.Vt.Count)
                    };
                case 3:
                    return new VertexInfo()
                    {
                        V = GetIndex(items[0], data.V.Count),
                        Vt = GetIndex(items[1], data.Vt.Count),
                        Vn = GetIndex(items[2], data.Vn.Count)
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
    }
}
