using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取横段内交线_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取横段内交线_小式亭子 class.
        /// </summary>
        public 取横段内交线_无斗拱亭子()
          : base("取横段内交线_无斗拱亭子", "Nickname",
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("交线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();          
            if (!DA.GetDataList(0, baseCurves)) return;
            List<BaseCurve> coreBaseCurves = GetCoreBaseCurves(baseCurves);

            DA.SetDataList(0, coreBaseCurves);
        }

        public static List<BaseCurve> GetCoreBaseCurves(List<BaseCurve> baseCurves)
        {
            List<BaseCurve> coreBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < baseCurves.Count; i++)
            {
                List<BaseCurve> tmpCutters = baseCurves.Select(x => x.Duplicate()).ToList();
                tmpCutters.RemoveAt(i);
                coreBaseCurves.Add(BaseCurveUtility.CurvesSplitAndGetCoreBaseCurve(tmpCutters.Select(x => x.curve).ToArray(), baseCurves[i]));
            }
            return coreBaseCurves;
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
            get { return new Guid("75D3E30A-BBD1-438E-AA41-398A7ADB676E"); }
        }
    }
}