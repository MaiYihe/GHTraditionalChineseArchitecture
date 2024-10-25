using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Utility;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取仔角梁面_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取仔角梁线 class.
        /// </summary>
        public 取仔角梁面_无斗拱亭子()
          : base("取仔角梁面_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("角梁顶部线", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("仔角梁平段出头", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("仔角梁截面高", "", "", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("仔角梁面", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> jiaoLiangTopBaseCurves = new List<BaseCurve>();
            double zaiJiaoLiangHeight = 0;
            double zaiJiaoLiangPingChuTou = 0;
            if (!DA.GetDataList(0, jiaoLiangTopBaseCurves) ||
                !DA.GetData(2, ref zaiJiaoLiangHeight) ||
                !DA.GetData(1, ref zaiJiaoLiangPingChuTou)) return;

            //得到XY平面上的sample
            BaseCurve sample = jiaoLiangTopBaseCurves[0].Duplicate();
            Plane plainPlane = PlaneUtility.PlaneToPlanes(sample.referencePlane)[1];
            double angle = -Vector3d.VectorAngle(Vector3d.ZAxis, jiaoLiangTopBaseCurves[0].positiveDirection);
            plainPlane.Transform(Transform.Rotation(angle, plainPlane.ZAxis, plainPlane.Origin));
            sample.Transform(Transform.PlaneToPlane(plainPlane, Plane.WorldXY));
            //平段出头
            Curve pingCurve = new Line(sample.curve.PointAtEnd, -Vector3d.XAxis, zaiJiaoLiangPingChuTou).ToNurbsCurve();
            //合并线
            List<Curve> curvesToJoin = new List<Curve>();
            curvesToJoin.Add(pingCurve); curvesToJoin.Add(sample.curve);
            Curve[] joinedCurves = Curve.JoinCurves(curvesToJoin);
            if (joinedCurves.Count() != 1) return;
            //偏移线
            if (zaiJiaoLiangHeight == 0) return;
            Curve[] offsetedCurves = joinedCurves[0].Offset(Plane.WorldXY, zaiJiaoLiangHeight, DocumentTolerance(), CurveOffsetCornerStyle.Sharp);
            if (offsetedCurves.Count() != 1) return;
            //所有线
            Curve[] curves = { joinedCurves[0], offsetedCurves[0] };

            //放样
            Brep loftedBrep = Brep.CreateFromLoft(curves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            //修正平面
            Plane fixedPlane = Plane.WorldXY.Clone();
            fixedPlane.Transform(Transform.Rotation(-angle, fixedPlane.ZAxis, fixedPlane.Origin));
            BaseFace baseFace = new BaseFace(loftedBrep, fixedPlane, Vector3d.ZAxis);

            //返回Transform
            List<Transform> backTransformList = jiaoLiangTopBaseCurves
                .Select(x => Transform.PlaneToPlane(fixedPlane, PlaneUtility.PlaneToPlanes(x.referencePlane)[1]))
                .ToList();
            List<BaseFace> backBaseFaces = new List<BaseFace>();
            for (int i = 0; i < backTransformList.Count; i++)
            {
                BaseFace tmpBaseFace = baseFace.Duplicate();
                tmpBaseFace.Transform(backTransformList[i]);
                backBaseFaces.Add(tmpBaseFace);
            }

            //得到各自的BoudingBox
            Plane tmpPlane = PlaneUtility.PlaneToPlanes(sample.referencePlane)[0];
            BoundingBox nowBoxOriginal = sample.curve.GetBoundingBox(tmpPlane);
            BoundingBox nowBox0 = joinedCurves[0].GetBoundingBox(tmpPlane);
            BoundingBox nowBox1 = offsetedCurves[0].GetBoundingBox(tmpPlane);
            BoundingBox unionBox = BoundingBox.Union(nowBox0, nowBox1);

            //得到原件的截面高与长
            double height = unionBox.Max.X;
            double width = unionBox.Max.Y;

            //位移
            List<Transform> transforms = new List<Transform>();
            for (int i = 0; i < jiaoLiangTopBaseCurves.Count(); i++)
            {
                Vector3d tmpVector = new Vector3d(jiaoLiangTopBaseCurves[i].positiveDirection);
                tmpVector.Unitize();
                tmpVector = tmpVector * zaiJiaoLiangHeight;
                transforms.Add(Transform.Translation(tmpVector));
            }

            DA.SetDataList(0, backBaseFaces);
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
            get { return new Guid("7256C7E3-EC90-4D05-BE07-5484156F69C5"); }
        }
    }
}