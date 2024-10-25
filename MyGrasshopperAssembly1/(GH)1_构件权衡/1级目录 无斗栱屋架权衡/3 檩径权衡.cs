using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 檩径权衡 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 柱径权衡 class.
        /// </summary>
        public 檩径权衡()
          : base("檩径权衡_屋架无斗栱", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("柱径D", "D，默认单位为m", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("金盘一侧与半径比", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("檐、金、脊檩尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("扶脊木尺寸", "", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Double D = 0.0, jinPan = 0.0;
            CircularProfile yanJinJiDimension, fuJiMuDimension;
            DA.GetData(0, ref D);
            DA.GetData(1, ref jinPan);

            yanJinJiDimension = new CircularProfile(0, 0, D / 2, jinPan);
            fuJiMuDimension = new CircularProfile(0, 0, (0.8 * D) / 2, jinPan);

            DA.SetData(0, yanJinJiDimension);
            DA.SetData(1, fuJiMuDimension);
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
            get { return new Guid("ED2A407E-C088-4195-977F-F83186DCC4C5"); }
        }
    }
}