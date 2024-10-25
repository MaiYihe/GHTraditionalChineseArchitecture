using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class 取轴 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取轴 class.
        /// </summary>
        public 取轴()
          : base("取轴", "Nickname",
              "将BaseCurves转化为轴",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("定位轴", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            if (!DA.GetDataList(0, baseCurves)) return;
            List<BaseAxis> baseAxes = new List<BaseAxis>();
            for(int i =0;i<baseCurves.Count;i++)
            {
                baseAxes.Add(ConvertBaseCurveToBaseAxis(baseCurves[i]));
            }

            DA.SetDataList(0, baseAxes);
        }
        private BaseAxis ConvertBaseCurveToBaseAxis(BaseCurve baseCurve)
        {
            Vector3d tmpV = new Vector3d(baseCurve.curve.PointAtEnd - baseCurve.curve.PointAtStart);
            tmpV.Unitize();
            BaseAxis baseAxis = new BaseAxis(baseCurve.curve, baseCurve.extrudePlane
                , tmpV);
            return baseAxis;
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
            get { return new Guid("F48964DF-4CF9-4213-9404-8E75E9C68DDA"); }
        }
    }
}