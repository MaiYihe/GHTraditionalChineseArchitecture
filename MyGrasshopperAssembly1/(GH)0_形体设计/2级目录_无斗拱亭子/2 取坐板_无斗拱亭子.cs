using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取坐板_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取坐板_小式亭子 class.
        /// </summary>
        public 取坐板_无斗拱亭子()
          : base("取坐板_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("定位基线", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("坐板宽", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("坐板外偏", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("端部坐板延申", "", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("移除第n项", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基面", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            double zuoBanWaiPian = 0.0;
            double zuoBanWidth = 0.0;
            double chuTou = 0.0;
            int n = 0;
            if (!DA.GetDataList(0, baseCurves) ||
                !DA.GetData(1, ref zuoBanWidth) ||
                !DA.GetData(2, ref zuoBanWaiPian) ||
                !DA.GetData(3, ref chuTou) ||
                !DA.GetData(4, ref n)) return;

            if (n < 0 || n > baseCurves.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "索引错误！");
                return;
            }
            if (chuTou < 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "延申值不能小于0!");
                return;
            }

            BaseCurve removedBaseCurve = baseCurves[n];
            baseCurves.RemoveAt(n);
            Curve leftCurve = Curve.JoinCurves(baseCurves.Select(x => x.curve).ToArray())[0];
            Curve extentedCurve = leftCurve.DuplicateCurve();
            if (chuTou > 0)
            {
                List<Curve> tmpCurves = leftCurve.DuplicateSegments().ToList();
                Vector3d v0 = new Vector3d(tmpCurves[0].PointAtStart - tmpCurves[0].PointAtEnd);
                v0.Unitize();
                Transform xform = Transform.Translation(v0 * chuTou);
                Point3d newStart = new Point3d(tmpCurves[0].PointAtStart);
                newStart.Transform(xform);

                Vector3d v1 = new Vector3d(tmpCurves[tmpCurves.Count - 1].PointAtEnd - tmpCurves[tmpCurves.Count - 1].PointAtStart);
                v1.Unitize();
                Transform yform = Transform
                    .Translation(v1 * chuTou);
                Point3d newEnd = new Point3d(tmpCurves[tmpCurves.Count - 1].PointAtEnd);
                newEnd.Transform(yform);

                tmpCurves[0] = new Line(newStart, tmpCurves[0].PointAtEnd).ToNurbsCurve();
                tmpCurves[tmpCurves.Count - 1] = new Line(tmpCurves[tmpCurves.Count - 1].PointAtStart, newEnd).ToNurbsCurve();

                extentedCurve = Curve.JoinCurves(tmpCurves.ToArray())[0];
            }


            Curve waiPianInsideCurve = default(Curve);
            if (zuoBanWaiPian == 0)
            {
                waiPianInsideCurve = extentedCurve;
            }
            else
            {
                waiPianInsideCurve = extentedCurve.Offset(Plane.WorldXY
                , zuoBanWaiPian, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];
            }

            Curve outSideCurve = waiPianInsideCurve.Offset(Plane.WorldXY
                , zuoBanWidth, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)[0];

            Curve[] insideSegments = waiPianInsideCurve.DuplicateSegments();
            Curve[] outSideSegments = outSideCurve.DuplicateSegments();


            List<Brep> zuoBanBreps = insideSegments
                .Select((x, i) => Brep.CreateFromLoft(new Curve[] { x, outSideSegments[i] }
                , Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0])
                .ToList();

            List<BaseCurve> sortedBaseCurves = SortedBaseCurveByPoint(baseCurves,
                zuoBanBreps
                .Select(x => AreaMassProperties.Compute(x).Centroid).ToList());
            List<BaseFace> zuoBanBaseFaces = zuoBanBreps
                .Select((x, i) => new BaseFace(x, baseCurves[i].referencePlane, baseCurves[i].positiveDirection))
                .ToList();


            int itemCount = zuoBanBaseFaces.Count;
            BaseFace baseFace0 = zuoBanBaseFaces[(itemCount + ((n - 1) % itemCount)) % itemCount];
            BaseFace baseFace1 = zuoBanBaseFaces[(itemCount + ((n - 2) % itemCount)) % itemCount];
            List<BaseFace> subList0 = new List<BaseFace> { baseFace0 };
            List<BaseFace> subList1 = new List<BaseFace> { baseFace1 };
            List<BaseFace> remainingBaseFaces = zuoBanBaseFaces
                .Where((x, i) => i != (itemCount + ((n - 1) % itemCount)) % itemCount && i != (itemCount + ((n - 2) % itemCount)) % itemCount)
                .ToList();
            List<List<BaseFace>> baseFacesList = new List<List<BaseFace>>();
            baseFacesList.Add(subList0);
            baseFacesList.Add(subList1);
            baseFacesList.Add(remainingBaseFaces);
            GH_Structure<IGH_Goo> tree = listEdit.TurnDoubleListToDataTree(baseFacesList);

            //DA.SetDataList(0, insideSegments);
            DA.SetDataTree(0, tree);
        }

        private List<BaseCurve> SortedBaseCurveByPoint(List<BaseCurve> baseCurves, List<Point3d> tragets)
        {
            List<BaseCurve> sortedBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < tragets.Count; i++)
            {
                BaseCurve tmpBaseCurve = baseCurves
                    .OrderBy(x => x.referencePlane.Origin.DistanceTo(tragets[i]))
                    .First();
                sortedBaseCurves.Add(tmpBaseCurve);
            }
            return sortedBaseCurves;
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
            get { return new Guid("463418C4-3EB3-45BD-8625-52B78D32C13C"); }
        }
    }
}