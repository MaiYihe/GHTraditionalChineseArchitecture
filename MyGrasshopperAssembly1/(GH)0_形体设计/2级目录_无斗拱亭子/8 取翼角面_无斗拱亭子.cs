using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using UtilityElement;

namespace MyGrasshopperAssembly1
{
    public class 取翼角面_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取翼角面_小式亭子 class.
        /// </summary>
        public 取翼角面_无斗拱亭子()
          : base("取翼角面_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("翼角檐椽线", "", "", GH_ParamAccess.tree);
            pManager.AddCurveParameter("翼角檐椽尾曲线", "", "", GH_ParamAccess.item);
            pManager.AddCurveParameter("翼角檐椽顶线", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("翼角面", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> tree = new GH_Structure<IGH_Goo>();
            Curve bottomCurve = default(Curve);
            Curve topCurve = default(Curve);
            if (!DA.GetDataTree(0, out tree) ||
                !DA.GetData(1, ref bottomCurve) ||
                !DA.GetData(2, ref topCurve)) return;

            //取出树枝中的每个第一项baseCurves
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            IList<IGH_Goo> branch = (IList<IGH_Goo>)tree.get_Branch(0);
            foreach (IGH_Goo item in branch)
            {
                baseCurves.Add((BaseCurve)item);
            }
            //奇数项，用于原路返回
            List<BaseCurve> oddItems = baseCurves
                .Where((item, index) => index % 2 != 0)
                .ToList();
            //前两项连线，取mirror平面
            Curve lianXian = new Line(baseCurves[0].curve.PointAt(0.5), baseCurves[1].curve.PointAt(0.5)).ToNurbsCurve();
            Plane mirrorPlane = CurveUtility.GetPlanes(lianXian, 0.5)[2];
            Transform mirror = Transform.Mirror(mirrorPlane);


            Curve curve0 = new Line(bottomCurve.PointAtStart, topCurve.PointAtStart).ToNurbsCurve();
            Curve curve1 = new Line(bottomCurve.PointAtEnd, topCurve.PointAtStart).ToNurbsCurve();
            List<Curve> curves = new List<Curve> { curve0, curve1, bottomCurve };
            int m = 0;
            Surface surface = NurbsSurface.CreateNetworkSurface(curves.ToArray(), 0
             , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
             , RhinoDoc.ActiveDoc.PageAngleToleranceRadians, out m);
            BaseFace baseFace = BaseFaceUtility.SurfaceToBaseSurface(surface);


            //镜像
            BaseFace mirroredBaseFace = baseFace.Duplicate();
            mirroredBaseFace.Transform(mirror);
            List<List<BaseFace>> backBaseFacesList = new List<List<BaseFace>>();
            List<BaseFace> backBaseFaces0 = new List<BaseFace>();
            List<BaseFace> backBaseFaces1 = new List<BaseFace>();
            //原路返回
            List<Transform> backTransformList = oddItems
                .Select(x => Transform.PlaneToPlane(oddItems[0].referencePlane, x.referencePlane))
                .ToList();
            for (int i = 0; i < backTransformList.Count; i++)
            {
                BaseFace tmpBaseFace = baseFace.Duplicate();
                BaseFace tmpMirroredBaseFace = mirroredBaseFace.Duplicate();
                tmpBaseFace.Transform(backTransformList[i]);
                tmpMirroredBaseFace.Transform(backTransformList[i]);

                tmpMirroredBaseFace.referencePlane = new Plane(tmpMirroredBaseFace.referencePlane.Origin
                    , -tmpMirroredBaseFace.referencePlane.XAxis, tmpMirroredBaseFace.referencePlane.YAxis);
                backBaseFaces0.Add(tmpBaseFace);
                backBaseFaces1.Add(tmpMirroredBaseFace);
            }
            backBaseFacesList.Add(backBaseFaces0);
            backBaseFacesList.Add(backBaseFaces1);

            GH_Structure<IGH_Goo> treeResult = listEdit.TurnDoubleListToDataTree(backBaseFacesList);
            DA.SetDataTree(0, treeResult);
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
            get { return new Guid("231EA050-033D-409C-BBA2-B6413AAFE29D"); }
        }
    }
}