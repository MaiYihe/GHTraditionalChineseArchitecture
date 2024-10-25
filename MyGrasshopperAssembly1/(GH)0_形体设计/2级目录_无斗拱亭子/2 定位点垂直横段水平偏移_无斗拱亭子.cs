using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using IOComponents;
using Rhino.Collections;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using UtilityElement;
using System.Linq;

namespace MyGrasshopperAssembly1
{
    public class 定位点垂直横段水平偏移_无斗拱亭子 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 横段偏移 class.
        /// </summary>
        public 定位点垂直横段水平偏移_无斗拱亭子()
          : base("定位点垂直横段水平偏移_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.last;
        private static readonly ParamDefinition[] inputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "定位点",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true,
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "偏移值",
                NickName = "垂直横段方向，XY平面水平偏移",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true,
            }),
        };
        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "偏移后定位点",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.list,
                Optional = false
            }),
        };

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Polyline polyline = new Polyline();double offset = 0.0;
            DA.GetData(0,ref polyline);
            DA.GetData(1,ref offset);

            List<Curve> curveList = new List<Curve>();
            for (int i = 0; i < polyline.Count - 1; i++)
            {
                curveList.Add(new Line(polyline[i], polyline[i + 1]).ToNurbsCurve() as Curve);
            }
            curveList.Add(new Line(polyline[polyline.Count - 1], polyline[0]).ToNurbsCurve() as Curve);

            List<Point3d> middlePoint3Ds = curveList.Select(curve => curve.PointAt(0.5)).ToList();
            double scaleFactor = (middlePoint3Ds[0].DistanceTo(new Point3d(0, 0, 0))+ offset) / middlePoint3Ds[0].DistanceTo(new Point3d(0,0,0));

            Polyline offsetPolyline = polyline.Duplicate();
            offsetPolyline.Transform(Transform.Scale(Plane.WorldXY, scaleFactor, scaleFactor,0));
            DA.SetData(0, offsetPolyline);
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
            get { return new Guid("373818C4-B22A-451B-A4A5-0D6053974CEE"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}