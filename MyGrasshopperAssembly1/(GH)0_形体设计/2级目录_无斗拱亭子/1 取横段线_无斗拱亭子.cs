using System;
using System.Collections.Generic;
using GH_IO.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using IOComponents;
using Rhino.Collections;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using System.Linq;

namespace MyGrasshopperAssembly1
{
    public class 取横段线_无斗拱亭子 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 取横段线_小式亭子 class.
        /// </summary>
        public 取横段线_无斗拱亭子()
          : base("取横段线_无斗拱亭子", "Nickname",
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
                Access = GH_ParamAccess.list,
                Optional = true,
            }),
        };
        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "横段线",
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
            List<Point3d> point3Ds = new List<Point3d>();
            if (!DA.GetDataList(0, point3Ds)) return;
            point3Ds.Add(point3Ds[0]);
            //PolylineCurve polylineCurve = polyline.ToPolylineCurve();
            List<Curve> curveList = new List<Curve>();
            for (int i = 0; i < point3Ds.Count() - 1; i++)
            {
                curveList.Add(new Line(point3Ds[i], point3Ds[i + 1]).ToNurbsCurve() as Curve);
            }
            List<BaseCurve> baseCurveList = BaseCurveUtility.CurvesToBaseCurves(curveList);

            DA.SetDataList(0, baseCurveList);
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
            get { return new Guid("E38F74C4-8FEF-4314-A0C9-4D9E61CB6ACF"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}