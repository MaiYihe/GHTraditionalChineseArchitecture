using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 檩径权衡_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 檩径权衡_小式亭子 class.
        /// </summary>
        public 檩径权衡_无斗拱亭子()
          : base("檩径权衡_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.last;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("檐柱径D", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("金盘一侧与半径比", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("檐、金檩尺寸", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double D = 0; double jinPan = 0;
            if (!DA.GetData(0, ref D) || !DA.GetData(1, ref jinPan)) return;
            DA.SetData(0, new CircularProfile(0, 0, 0.9 * D / 2, jinPan));
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
            get { return new Guid("C618E9C7-9244-44F1-B607-FF4616CDC264"); }
        }
    }
}