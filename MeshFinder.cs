using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
public class MeshFinder
{
    public static MeshFinder Instance { get; private set; } = new MeshFinder();
    public class TirangleApi
    {
        // Start is called before the first frame update
        public static TirangleApi Instance { get; private set;} = new TirangleApi();
        public Polygon2D Triangulate(PSLG pslg)  {
            if (pslg.vertices.Count == 0)
            {
                Debug.LogError("No vertices passed to triangle. hole count: " + pslg.holes.Count + ", vert count: " + pslg.vertices.Count);
                return new Polygon2D(new int[] { }, new Vector2[] { });
            }
            else 
            {
                // Write poly file
                WritePolyFile(pslg);
            
                // Execute Triangle
                ExecuteTriangle();
            
                // Read outout
                Vector2[] vertices = ReadVerticesFile();
                int[] triangles = ReadTrianglesFile();
            
                // CleanUp();

                return new Polygon2D(triangles, vertices);
            }
        }

        void WritePolyFile(PSLG pslg)
        { 
            try
            {
                string polyFilePath = "C:\\triangle\\polygon.poly";
                if (File.Exists(polyFilePath))
                {
                    File.Delete(polyFilePath);
                }
                using (StreamWriter sw = File.CreateText(polyFilePath))
                {
                    sw.WriteLine("# polygon.poly");
                    sw.WriteLine("# generated by Unity Triangle API");
                    sw.WriteLine("#");
                    // Vertices
                    sw.WriteLine(pslg.GetNumberOfSegments() + " 2 0 1");
                    sw.WriteLine("# The polyhedrons.");
                    int boundaryMarker = 2;
                    int i;
                    for (i = 0; i < pslg.vertices.Count; i++)
                    {
                        if (i != 0 && pslg.boundaryMarkersForPolygons.Contains(i))
                        {
                            boundaryMarker++;
                        }
                        sw.WriteLine(i + 1 + "\t" + pslg.vertices[i].x + "\t" + pslg.vertices[i].y + "\t" + boundaryMarker);
                    }
                    int offset = i;
                    for (i = 0; i < pslg.holes.Count; i++)
                    {
                        sw.WriteLine("# Hole #" + (i + 1));
                        int j;
                        for (j = 0; j < pslg.holes[i].vertices.Count; j++)
                        {
                            sw.WriteLine((offset + j + 1) + "\t" + pslg.holes[i].vertices[j].x + "\t" + pslg.holes[i].vertices[j].y + "\t" + (boundaryMarker + i + 1));
                        }
                        offset += j;
                    }
                    // Line segments
                    sw.WriteLine();
                    sw.WriteLine("# Line segments.");
                    sw.WriteLine(pslg.GetNumberOfSegments() + " 1");
                    sw.WriteLine("# The polyhedrons.");
                    boundaryMarker = 2;
                    for (i = 0; i < pslg.segments.Count; i++)
                    {
                        if (i != 0 && pslg.boundaryMarkersForPolygons.Contains(i))
                        {
                            boundaryMarker++;
                        }
                        sw.WriteLine(i + 1 + "\t" + (pslg.segments[i][0] + 1) + "\t" + (pslg.segments[i][1] + 1) + "\t" + boundaryMarker);
                    }
                    offset = i;
                    for (i = 0; i < pslg.holes.Count; i++)
                    {
                        sw.WriteLine("# Hole #" + (i + 1));
                        int j;
                        for (j = 0; j < pslg.holes[i].segments.Count; j++)
                        {
                            sw.WriteLine((offset + j + 1) + "\t" + (offset + 1 + pslg.holes[i].segments[j][0]) + "\t" + (offset + 1 + pslg.holes[i].segments[j][1]) + "  " + (boundaryMarker + i + 1));
                        }
                        offset += j;
                    }
                    // Holes
                    sw.WriteLine();
                    sw.WriteLine("# Holes.");
                    sw.WriteLine(0);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        void ExecuteTriangle()
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "C:\\triangle\\triangle.exe";
                process.StartInfo.Arguments = "-pPq0 C:\\triangle\\polygon.poly";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                
                //string output = process.StandardOutput.ReadToEnd();
                //Debug.Log(output);
                
                process.WaitForExit();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        Vector2[] ReadVerticesFile()
        {
            Vector2[] vertices = null;
            try
            {
                string outputVerticesFile = "C:\\triangle\\polygon.1.node";

                StreamReader sr = File.OpenText(outputVerticesFile);

                string line = sr.ReadLine();
                int n = line.IndexOf("  ");
                int nVerts = int.Parse(line.Substring(0, n));
                vertices = new Vector2[nVerts];
                int whileCount = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    int index = -1;
                    float x = 0f;
                    float y = 0f;
                    int c = 0;
                    if (!line.Contains("#"))
                    {
                        string[] stringBits = line.Split(' ');

                        foreach (string s in stringBits)
                        {
                            if (s != "" && s != " ")
                            {
                                if (c == 0)
                                    index = int.Parse(s);
                                else if (c == 1)
                                    x = float.Parse(s);
                                else if (c == 2)
                                    y = float.Parse(s);

                                c++;
                            }
                        }
                    }
                    if (index != -1)
                    {
                        vertices[index - 1] = new Vector2(x, y);
                    }
                    whileCount++;
                    if (whileCount > 1000)
                    {
                        Debug.LogError("Stuck in while loop");
                        break;
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return vertices;
        }
        private int[] ReadTrianglesFile()
        {
            List<int> triList = null;
            try
            {
                string outputTrianglesFile = "C:\\triangle\\polygon.1.ele";
                using (StreamReader sr = File.OpenText(outputTrianglesFile))
                {
                    string line = sr.ReadLine();
                    int n = line.IndexOf("  ");
                    int nTriangles = int.Parse(line.Substring(0, n));
                    //int[] triangles = new int[nTriangles * 3];
                    triList = new List<int>(nTriangles * 3);
                    int count = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        int index = -1;
                        int c = 0;
                        int[] tri = new int[3];
                        if (!line.Contains("#"))
                        {
                            string[] stringBits = line.Split(' ');

                            foreach (string s in stringBits)
                            {
                                if (s != "" && s != " ")
                                {
                                    if (c == 0)
                                        index = int.Parse(s);
                                    else if (c == 1)
                                        tri[0] = int.Parse(s) - 1;
                                    else if (c == 2)
                                        tri[1] = int.Parse(s) - 1;
                                    else if (c == 3)
                                        tri[2] = int.Parse(s) - 1;

                                    c++;
                                }
                            }
                        }
                        if (index != -1)
                        {
                            triList.AddRange(tri);
                        }
                        count++;
                        if (count > 1000)
                        {
                            Debug.LogError("Stuck in while loop");
                            break;
                        }
                    }
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return triList.ToArray();
        }
        private void CleanUp()
        {
            try
            {
                File.Delete("C:\\triangle\\polygon.1.ele");
                File.Delete("C:\\triangle\\polygon.1.node");
                File.Delete("C:\\triangle\\polygon.poly");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public class Polygon2D {
        public int[] triangles;
        public Vector2[] vertices;
        public Polygon2D(int[] triangle, Vector2[] vertices)
        {
            this.triangles = triangle;
            this.vertices = vertices;
        }
    }
    public class PSLG {
        public List<Vector2> vertices;
        public List<int[]> segments;
        public List<PSLG> holes;
        public List<int> boundaryMarkersForPolygons;
        public PSLG()
        {
            vertices = new List<Vector2>();
            segments = new List<int[]>();
            holes = new List<PSLG>();
            boundaryMarkersForPolygons = new List<int>();
        }
        public PSLG(List<Vector2> vertices) : this()
        {
            this.AddVertexLoop(vertices);
        }
        public void AddVertexLoop(List<Vector2> vertices)
        {
            if (vertices.Count < 3)
            {
                Debug.Log("A vertex loop cannot have less than three vertices " + vertices.Count);
            }
            else
            {
                this.vertices.AddRange(vertices);
                int segmentOffset = segments.Count;
                boundaryMarkersForPolygons.Add(segments.Count);
                for(int i = 0; i < vertices.Count - 1; i++)
                {
                    segments.Add(new int[] { i + segmentOffset, i + 1 + segmentOffset });
                }
                segments.Add(new int[] { vertices.Count - 1 + segmentOffset, segmentOffset });
            }
        }
        public int GetNumberOfSegments()
        {
            int offset = vertices.Count;
            foreach (PSLG hole in holes)
            {
                offset += hole.segments.Count;
            }
            return offset;
        }
    }
}