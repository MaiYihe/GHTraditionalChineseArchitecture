using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class 横段线转竖段线_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 横段线转竖段线_无斗拱亭子 class.
        /// </summary>
        public 横段线转竖段线_无斗拱亭子()
          : base("横段线转竖段线_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("横段线", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("出头高", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("是否贯通", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("竖段线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> hengDuanBaseCurves = new List<BaseCurve>();
            double chuTou = 0.0;
            bool isPierceThrough = false;
            if (!DA.GetDataList(0, hengDuanBaseCurves) ||
                !DA.GetData(1, ref chuTou) ||
                !DA.GetData(2, ref isPierceThrough)) return;
            if (isPierceThrough == false && chuTou < 0.001)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "输出为空！");
                return;
            }

            List<Point3d> startPoints = hengDuanBaseCurves
                .Select(x => x.curve.PointAtStart)
                .ToList();
            List<Point3d> proStartPoints = new List<Point3d>();
            if (isPierceThrough == true)
            {
                Transform pro = Transform.PlanarProjection(Plane.WorldXY);
                for (int i = 0; i < startPoints.Count; i++)
                {
                    Point3d tmpPoint = startPoints[i];
                    tmpPoint.Transform(pro);
                    proStartPoints.Add(tmpPoint);
                }
            }
            else
            {
                proStartPoints = startPoints;
            }

            Transform chuTouTrans = Transform.Translation(new Vector3d(0, 0, chuTou));
            List<Point3d> endPoints = startPoints
                .Select(x => new Point3d(x))
                .ToList();
            List<Point3d> transEndPoints = new List<Point3d>();
            for (int i = 0; i < endPoints.Count; i++)
            {               
                Point3d tmpPoint = endPoints[i];
                tmpPoint.Transform(chuTouTrans);
                transEndPoints.Add(tmpPoint);
            }

            List<BaseCurve> baseCurves = proStartPoints
                .Select((x, i) => BaseCurveUtility.CurveToBaseCurve(new Line(x, transEndPoints[i]).ToNurbsCurve()))
                .ToList();
            baseCurves = 取竖段线_无斗拱亭子.AdjuestStraightBaseCurvesToCenter(baseCurves);


            DA.SetDataList(0, baseCurves);
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
            get { return new Guid("54CE3CF1-CE7E-4230-AF0B-72A893A84AB4"); }
        }
    }
}