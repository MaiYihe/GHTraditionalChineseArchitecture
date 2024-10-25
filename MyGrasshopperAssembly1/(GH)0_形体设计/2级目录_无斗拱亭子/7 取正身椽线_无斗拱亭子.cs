using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using System.Linq;
using YhmAssembly;
using UtilityElement;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace MyGrasshopperAssembly1
{
    public class 取正身椽线_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取正身椽 class.
        /// </summary>
        public 取正身椽线_无斗拱亭子()
          : base("取正身椽线_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("戗线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("举架顶点", "", "", GH_ParamAccess.item);            
            pManager.AddGenericParameter("正身檐椽线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("正身椽线", "", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> qiangBaseCurve = new List<BaseCurve>();
            Point3d juJiaPoint = new Point3d();
            List<BaseCurve> zhengShenChuanBaseCurve = new List<BaseCurve>();
            if (!DA.GetDataList(0, qiangBaseCurve)||
                !DA.GetData(1, ref juJiaPoint) ||
                !DA.GetDataList(2, zhengShenChuanBaseCurve)) return;
            int n = qiangBaseCurve.Count;//多边形边数

            //分成n份
            List<List<BaseCurve>> chunks = new List<List<BaseCurve>>();
            int chunkSize = zhengShenChuanBaseCurve.Count / n;
            for (int i = 0; i < n; i++)
            {
                List<BaseCurve> chunk = zhengShenChuanBaseCurve.Skip(i * chunkSize).Take(chunkSize).ToList();
                chunks.Add(chunk);
            }
            //取出一份，取出第一项与最后一项
            List<BaseCurve> chunkItem = chunks[0];
            Curve firstCurve = chunkItem[0].curve;
            Curve lastCurve = chunkItem[chunkItem.Count - 1].curve;
            //创建投影的三角平面
            Surface surface = NurbsSurface.CreateFromCorners(firstCurve.PointAtStart, juJiaPoint, lastCurve.PointAtStart);
            //将第一份第一项的Curve投影到XY平面上
            Curve projectedCurve = Curve.ProjectToPlane(firstCurve, Plane.WorldXY);
            Plane plane = CurveUtility.GetPlanes(projectedCurve, 0)[2];
            Point3d closestPoint = plane.ClosestPoint(Plane.WorldXY.Origin);
            //创建延申到原点位置的newCurve
            Point3d movedStartPoint = projectedCurve.PointAtStart;
            movedStartPoint.Transform(Transform.Translation(new Vector3d(Plane.WorldXY.Origin - closestPoint)));
            Curve newCurve = new Line(movedStartPoint, projectedCurve.PointAtEnd).ToNurbsCurve();
            //创建该份所有对映的newCurve
            List<Transform> xformList = chunkItem
                .Select(x => Transform.PlaneToPlane(chunkItem[0].referencePlane, x.referencePlane))
                .ToList();
            List<Curve> newCurves = new List<Curve>();
            for (int i = 0; i < xformList.Count; i++)
            {
                Curve tmpCurve = newCurve.DuplicateCurve();
                tmpCurve.Transform(xformList[i]);
                newCurves.Add(tmpCurve);
            }
            List<Curve> projectedCurves = new List<Curve>();
            for (int i = 0; i < newCurves.Count; i++)
            {
                Curve[] tmpCurves = Curve
                    .ProjectToBrep(newCurves[i], surface.ToBrep(), Vector3d.ZAxis, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (tmpCurves.Count() > 0) projectedCurves.AddRange(tmpCurves);
            }

            //转化为双重列表mergedLists，输出为树
            List<BaseCurve> baseCurves = BaseCurveUtility.CurvesToBaseCurves(projectedCurves);
            List<List<BaseCurve>> mergedLists = new List<List<BaseCurve>>();
            for (int i = 0; i < baseCurves.Count() / 2; i++)
            {
                List<BaseCurve> mergedList = new List<BaseCurve>();
                mergedList.Add(baseCurves[i]);
                mergedList.Add(baseCurves[baseCurves.Count() - 1 - i]);
                mergedLists.Add(mergedList);
            }
            // 如果 baseCurves 的数量是奇数，则将中间的项单独合并成一个列表
            if (baseCurves.Count() % 2 != 0)
            {
                List<BaseCurve> mergedList = new List<BaseCurve>();
                mergedList.Add(baseCurves[baseCurves.Count() / 2]);
                mergedLists.Add(mergedList);
            }

            //原路返回
            List<List<BaseCurve>> backLists = new List<List<BaseCurve>>();
            List<Transform> transforms = chunks.Select(x => Transform.PlaneToPlane(chunks[0][0].referencePlane, x[0].referencePlane)).ToList();

            for (int i = 0; i < mergedLists.Count(); i++)
            {
                List<BaseCurve> tmpMergedList = new List<BaseCurve>();
                for (int j = 0; j < transforms.Count(); j++)
                {
                    List<BaseCurve> tmptmpMergedList = mergedLists[i].Select(x => x.Duplicate()).ToList();
                    foreach (BaseCurve item in tmptmpMergedList)
                    {
                        item.Transform(transforms[j]);
                    }
                    tmpMergedList.AddRange(tmptmpMergedList);
                }
                backLists.Add(tmpMergedList);
            }


            GH_Structure<IGH_Goo> tree = listEdit.TurnDoubleListToDataTree(backLists);
            DA.SetDataTree(0, tree);
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
            get { return new Guid("E366F417-FE3E-4090-9A55-AB24E240B858"); }
        }
    }
}