using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityElement.Metadeta.Dimensions;
using UtilityElement;
using UtilityElement.Metadeta;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using Rhino;
using Rhino.Geometry.Collections;

namespace TimberElement
{
    internal class FaceTimber : TimberElement
    {
        //属性
        public new bool isExist
        {
            get
            {
                base.isExist = false;
                if (width > 0) base.isExist = true;
                return base.isExist;
            }
        }//判断存在与否
        public BaseFace baseFace { get; set; }
        public double width { get; set; }

        private Brep timberBrep;
        public Brep TimberBrep
        {
            get
            {
                //挤出edge
                if (baseFace == null || width <= 0) return null;
                BrepFace brepFace = baseFace.brep.Faces[0];

                //Curve[] edges = baseFace.brep.DuplicateNakedEdgeCurves(true, true);
                //Curve[] tmpEdges = Curve.JoinCurves(edges);
                //Curve edgeClosedCurve = tmpEdges[0];
                //if (edgeClosedCurve.IsClosed == false) return null;
                //Vector3d vector3D = baseFace.positiveDirection;
                //vector3D.Unitize();
                //vector3D = vector3D * width;
                //edgeClosedCurve.Transform(Transform.Translation(-vector3D / 2));
                Vector3d vector3D = baseFace.positiveDirection;
                vector3D.Unitize();
                vector3D = vector3D * width;
                brepFace.Transform(Transform.Translation(-vector3D / 2));
                if (brepFace == null) return null;
                Point3d centriod = brepFace.ToNurbsSurface().PointAt(0.5, 0.5);
                Curve pathCuvre = new Line(centriod, vector3D, width).ToNurbsCurve();
                Brep brep = brepFace.CreateExtrusion(pathCuvre, true);
                brep.Transform(Transform.Translation(-vector3D / 2));
                Brep timberBrep = brep;

                //Surface edgeSurface = Surface.CreateExtrusion(edgeClosedCurve, vector3D);
                //Brep edgeBrep = edgeSurface.ToBrep();
                ////上下的brep
                //Brep brep0 = baseFace.brep.DuplicateBrep();
                //Surface tmpSurface = baseFace.brep.DuplicateBrep().Surfaces[0];
                //tmpSurface.Reverse(1);
                //Brep brep1 = tmpSurface.ToBrep();
                //brep0.Transform(Rhino.Geometry.Transform.Translation(vector3D / 2));
                //brep1.Transform(Rhino.Geometry.Transform.Translation(-vector3D / 2));

                //Brep[] breps = new Brep[] { brep0, edgeBrep, brep1 };
                //Brep[] tmpBreps = Brep.JoinBreps(breps, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                //if (tmpBreps.Count() != 1) return null;
                //timberBrep = tmpBreps[0];

                return timberBrep;
            }
            set { timberBrep = value; }
        }

        public new string name
        {
            get { return base.name; }
            set { base.name = value; }
        }//构件名称
        public new string description
        {
            get
            {
                base.description = string.Format("");
                return base.description;
            }
        }//为父类的描述赋值并输出
        //构造函数
        public FaceTimber(BaseFace baseFace, double width, string name)
        {
            this.baseFace = baseFace;
            this.width = width;
            this.name = name;
        }

    }
}
