using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using System.Linq;
using OutPutElement;
using Rhino.Geometry.Intersect;
using Rhino;

namespace MyGrasshopperAssembly1
{
    public class 取衬头木面_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取衬头木面 class.
        /// </summary>
        public 取衬头木面_无斗拱亭子()
          : base("取衬头木面_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.last;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("角梁图元信息", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("正身檐椽线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("檐檩顶部线", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("衬头木高", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("衬头木厚", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("衬头木面", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryInformation> jiaoLiangGI = new List<GeometryInformation>();
            List<BaseCurve> zhengShenYanChuanBaseCurves = new List<BaseCurve>();
            List<BaseCurve> yanLingBaseCurves = new List<BaseCurve>();
            double height = 0.0; double width = 0.0;
            if (!DA.GetDataList(0, jiaoLiangGI) ||
                !DA.GetDataList(1, zhengShenYanChuanBaseCurves) ||
                !DA.GetDataList(2, yanLingBaseCurves) ||
                !DA.GetData(3, ref height) ||
                !DA.GetData(4, ref width)) return;
            if (height <= 0 || width <= 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "参数错误！");
                return;
            }
            List<BaseCurve> coreBaseCurves = BaseCurveUtility.PolygonShapeGetInternalIntersectionLine(yanLingBaseCurves);
            //取一项
            BaseCurve coreBaseCurve = coreBaseCurves[0];
            Plane jiaoLiangSidePlane = FindjiaoLiangSidePlane(jiaoLiangGI[0]);
            Plane yanChuanPlane = CurveUtility.GetPlanes(zhengShenYanChuanBaseCurves[0].curve, 1)[1];
            //pointA
            Point3d pointA = Intersection
                .CurvePlane(coreBaseCurve.curve, yanChuanPlane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0]
                .PointA;
            Curve curve = new Line(pointA, coreBaseCurve.curve.PointAtStart).ToNurbsCurve();
            Vector3d moveVecror = coreBaseCurve.referencePlane.YAxis;
            moveVecror.Unitize(); moveVecror = -moveVecror * width / 2;
            curve.Transform(Transform.Translation(moveVecror));

            Point3d pointAA = curve.PointAtStart;
            double t = 0.0;
            Line tmpLine = new Line(curve.PointAtStart, curve.PointAtEnd);
            Intersection.LinePlane(tmpLine, jiaoLiangSidePlane, out t);

            Point3d pointB = tmpLine.PointAt(t);
            Plane tmpPlane = CurveUtility.GetPlanes(coreBaseCurve.curve, 0)[2];
            Vector3d v = tmpPlane.YAxis;
            v.Unitize();
            v = v * height;
            Transform xform = Transform.Translation(v);
            Point3d pointC = pointB;
            pointC.Transform(xform);

            //创建BaseFace
            Surface surface = NurbsSurface.CreateFromCorners(pointAA, pointB, pointC);
            Plane referencePlane = new Plane();
            surface.TryGetPlane(out referencePlane);
            BaseFace baseFace = new BaseFace(surface.ToBrep(), referencePlane, referencePlane.ZAxis);

            //原路返回
            List<BaseFace> baseFaces = new List<BaseFace> { baseFace };
            Transform mirror = Transform.Scale(CurveUtility.GetPlanes(coreBaseCurve.curve, 0.5)[2], 1, 1, -1);
            BaseFace mirroredBaseFace = baseFace.Duplicate();
            mirroredBaseFace.Transform(mirror);
            baseFaces.Add(mirroredBaseFace);

            List<BaseFace> backBaseFaces = new List<BaseFace>();
            List<Transform> backTransformList = jiaoLiangGI
                .Select(x => Transform.PlaneToPlane(jiaoLiangGI[0].referencePlane, x.referencePlane))
                .ToList();
            List<BaseFace> baseFacesAdded = new List<BaseFace>();
            for (int i = 1; i < backTransformList.Count; i++)
            {
                baseFacesAdded.Clear();
                baseFacesAdded = baseFaces.Select(x => x.Duplicate()).ToList();
                for (int j = 0; j < baseFacesAdded.Count; j++)
                {
                    baseFacesAdded[j].Transform(backTransformList[i]);
                }
                backBaseFaces.AddRange(baseFacesAdded);
            }
            DA.SetDataList(0, backBaseFaces);
        }


        private Plane FindjiaoLiangSidePlane(GeometryInformation jiaoLiangGI)
        {
            List<AreaMassProperties> ampList = jiaoLiangGI.brep.Surfaces.Select(x => AreaMassProperties.Compute(x)).ToList();
            List<Point3d> centerPoints = ampList.Select(x => x.Centroid).ToList();
            List<Point3d> subPoints = centerPoints
                .Select(x => PlaneUtility.subWorldPlaneForPoint3d(jiaoLiangGI.referencePlane, x))
                .ToList();
            Surface jiaoLiangSideSurface = subPoints
                .Zip(jiaoLiangGI.brep.Surfaces, (subP, originSurface) => new { p0 = subP, p1 = originSurface })
                .OrderBy(x => x.p0.Y)
                .Select(x => x.p1)
                .Last();
            Plane jiaoLiangSidePlane = new Plane();
            jiaoLiangSideSurface.TryGetPlane(out jiaoLiangSidePlane);
            return jiaoLiangSidePlane;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7D59E9E8-392B-4E4D-867F-2C1B1EF973F9"); }
        }
    }
}