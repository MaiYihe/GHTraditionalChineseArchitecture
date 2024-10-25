using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class 取正身椽面_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取正身檐椽面_小式亭子 class.
        /// </summary>
        public 取正身椽面_无斗拱亭子()
          : base("取正身椽面_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("正身檐椽顶部线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("正身椽顶部线", "", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("顶部正身椽面", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("底部正身椽面", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> zhengShenChuanBaseCurve = new List<BaseCurve>();
            GH_Structure<IGH_Goo> tree = new GH_Structure<IGH_Goo>();
            if (!DA.GetDataList(0, zhengShenChuanBaseCurve) ||
                !DA.GetDataTree(1, out tree)) return;
            //n边形
            int n = tree.get_Branch(0).Count / 2;
            //up每边有singleCount个
            int upSingleCount = tree.DataCount / n;
            int downSingleCount = upSingleCount + 2;

            ///得到upBaseCurves和downBaseCurves
            //path数量
            int pathCount = tree.PathCount;
            List<BaseCurve> upBaseCurves = new List<BaseCurve>();
            //椽奇数情况
            if (tree.get_Branch(pathCount - 1).Count < tree.get_Branch(0).Count)
            {
                for (int i = 0; i < pathCount - 1; i++)
                {
                    IList<IGH_Goo> branch = (IList<IGH_Goo>)tree.get_Branch(i);
                    upBaseCurves.Add((BaseCurve)branch[0]);
                    upBaseCurves.Add((BaseCurve)branch[1]);
                }
                IList<IGH_Goo> tmpBranch = (IList<IGH_Goo>)tree.get_Branch(pathCount - 1);
                upBaseCurves.Add((BaseCurve)tmpBranch[0]);
            }
            //椽偶数情况
            else if (tree.get_Branch(pathCount - 1).Count == tree.get_Branch(0).Count)
            {
                for (int i = 0; i < pathCount; i++)
                {
                    IList<IGH_Goo> branch = (IList<IGH_Goo>)tree.get_Branch(i);
                    upBaseCurves.Add((BaseCurve)branch[0]);
                    upBaseCurves.Add((BaseCurve)branch[1]);
                }
            }
            List<BaseCurve> downBaseCurves = zhengShenChuanBaseCurve.Take(downSingleCount).ToList();

            //分别生成Surface
            Curve downCurve0 = downBaseCurves[0].curve;
            Curve downCurve1 = downBaseCurves[downBaseCurves.Count - 1].curve;
            Point3d downP0 = downCurve0.PointAtStart;
            Point3d downP1 = downCurve0.PointAtEnd;
            Point3d downP2 = downCurve1.PointAtEnd;
            Point3d downP3 = downCurve1.PointAtStart;
            Surface downSurface = NurbsSurface.CreateFromCorners(downP0, downP1, downP2, downP3);
            BaseFace downBaseFace = BaseFaceUtility.SurfaceToBaseSurface(downSurface);


            BaseFace upBaseFace = new BaseFace();
            //椽奇数情况
            if (tree.get_Branch(pathCount - 1).Count < tree.get_Branch(0).Count)
            {
                Point3d upP1 = upBaseCurves[upBaseCurves.Count - 1].curve.PointAtStart;
                Surface upSurface = NurbsSurface.CreateFromCorners(downP0, upP1, downP3);
                upBaseFace = BaseFaceUtility.SurfaceToBaseSurface(upSurface);
            }
            //椽偶数情况
            else if (tree.get_Branch(pathCount - 1).Count == tree.get_Branch(0).Count)
            {
                Point3d upP1 = upBaseCurves[upBaseCurves.Count - 1].curve.PointAtStart;
                Point3d upP2 = upBaseCurves[upBaseCurves.Count - 2].curve.PointAtStart;
                Surface upSurface = NurbsSurface.CreateFromCorners(downP0, upP2, upP1, downP3);
                upBaseFace = BaseFaceUtility.SurfaceToBaseSurface(upSurface);
            }


            //返回路径
            IList<IGH_Goo> backTree = (IList<IGH_Goo>)tree.get_Branch(0);
            IList<IGH_Goo> oddItems = backTree.Where((item, index) => index % 2 != 0).ToList();
            List<BaseCurve> oddBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < oddItems.Count; i++)
            {
                oddBaseCurves.Add((BaseCurve)oddItems[i]);
            }
            List<Transform> backTransform = oddBaseCurves
                .Select((x, i) => Transform.PlaneToPlane(oddBaseCurves[0].referencePlane, x.referencePlane))
                .ToList();

            List<BaseFace> upBaseFaces = new List<BaseFace> ();
            List<BaseFace> downBaseFaces = new List<BaseFace>();
            for (int i = 0;i < backTransform.Count;i++)
            {
                BaseFace tmpUpBaseFace = upBaseFace.Duplicate();
                BaseFace tmpDownBaseFace = downBaseFace.Duplicate();
                tmpUpBaseFace.Transform(backTransform[i]);
                tmpDownBaseFace.Transform(backTransform[i]);

                upBaseFaces.Add(tmpUpBaseFace);
                downBaseFaces.Add(tmpDownBaseFace);
            }

            DA.SetDataList(0, upBaseFaces);
            DA.SetDataList(1, downBaseFaces);
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
            get { return new Guid("BEAAE1DA-6211-411D-8A4F-28608E0E19F0"); }
        }
    }
}