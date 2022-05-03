using Godot;
using System.Collections.Generic;
using System;

namespace Framework.Utils
{

    public class RayCastLine
    {
        public Vector3 p1 { get; set; }
        public Vector3 p2 { get; set; }
        public float Time { get; set; }
    }

    class LineDrawer3d : MeshInstance3D
    {
        public float DrawTime { get; set; } = 1.2f;
        List<RayCastLine> lines = new List<RayCastLine>();

        public void AddLine(Vector3 p1, Vector3 p2)
        {
            lines.Add(new RayCastLine { p1 = p1, p2 = p2, Time = DrawTime });
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            this.Mesh = new ImmediateMesh();
            var mat = new StandardMaterial3D();
            mat.AlbedoColor = Colors.Red;
            mat.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
            this.MaterialOverride = mat;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            var mesh = this.Mesh as ImmediateMesh;
            mesh.ClearSurfaces();

            foreach (var line in lines.ToArray())
            {
                if (line.Time <= 0f)
                {
                    this.lines.Remove(line);
                }
                else
                {
                    line.Time -= delta;
                }
            }

            if (this.lines.Count > 0)
            {
                mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
                foreach (var line in lines.ToArray())
                {
                    mesh.SurfaceAddVertex(line.p1);
                    mesh.SurfaceAddVertex(line.p2);
                }

                mesh.SurfaceEnd();
            }
        }
    }
}