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
    public class 取正身椽面_重檐_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取正身椽面_重檐_无斗拱亭子 class.
        /// </summary>
        public 取正身椽面_重檐_无斗拱亭子()
          : base("取正身椽面_重檐_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("正身檐椽顶部线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("正身椽面", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> zhengShenChuanBaseCurves = new List<BaseCurve>();
            if (!DA.GetDataList(0, zhengShenChuanBaseCurves)) return;
            List<Point3d> startPoints = zhengShenChuanBaseCurves.Select(x => x.curve.PointAtStart).ToList();
            if (startPoints.Count < 2)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "椽数错误！");
                return;
            }
            //每边有singleCount个
            int SingleCount = 2;
            Vector3d lastVector = new Vector3d(startPoints[1] - startPoints[0]);
            for (int i = 2; i < startPoints.Count; i++)
            {
                Vector3d tmpVector = new Vector3d(startPoints[i] - startPoints[i - 1]);
                if (Vector3d.VectorAngle(lastVector, tmpVector) < 0.01)
                {
                    SingleCount++;
                }
                else break;
                lastVector = tmpVector;
            }
            BaseCurve baseCurve0 = zhengShenChuanBaseCurves[0];
            BaseCurve baseCurve1 = zhengShenChuanBaseCurves[SingleCount - 1];
            Surface surface = NurbsSurface.CreateFromCorners(baseCurve0.curve.PointAtStart
                , baseCurve0.curve.PointAtEnd, baseCurve1.curve.PointAtEnd
                , baseCurve1.curve.PointAtStart);
            BaseFace baseFace = BaseFaceUtility.SurfaceToBaseSurface(surface);

            List<List<BaseCurve>> chunks = zhengShenChuanBaseCurves
                .Select((x, i) => new { Value = x, Index = i })
                .GroupBy(item => item.Index / SingleCount)
                .Select(group => group.Select(item => item.Value).ToList())
                .ToList();
            List<BaseCurve> firstBaseCurves = chunks.Select(x => x[0]).ToList();
            List<Transform> backTransform = firstBaseCurves
                .Select(x => Transform.PlaneToPlane(x.referencePlane, firstBaseCurves[0].referencePlane))
                .ToList();
            List<BaseFace> baseFaces = new List<BaseFace>();
            for (int i = 0; i < backTransform.Count; i++)
            {
                BaseFace tmpBaseFace = baseFace.Duplicate();
                tmpBaseFace.Transform(backTransform[i]);
                baseFaces.Add(tmpBaseFace);
            }

            DA.SetDataList(0, baseFaces);
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
            get { return new Guid("120FF657-7F08-48DB-8C5B-95ACF8600CBA"); }
        }
    }
}