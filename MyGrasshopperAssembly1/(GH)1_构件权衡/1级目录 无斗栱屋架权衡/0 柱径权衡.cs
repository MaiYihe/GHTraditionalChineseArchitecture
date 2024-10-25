using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 柱径权衡 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 柱径权衡 class.
        /// </summary>
        public 柱径权衡()
          : base("柱径权衡_屋架无斗栱", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("柱径D", "D", "默认单位为m", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("檐柱尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("金柱尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("中柱尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("山柱尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("重檐金柱尺寸", "", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        Double cun = 0.032;//单位是m
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Double D = 0.0;
            CircularProfile yanZhuDimension, jinZhuDimension, zhongZhuDimension,shanZhuDimension,chongYanJinZhuDimension;
            DA.GetData(0,ref D);

            yanZhuDimension = new CircularProfile(0, 0, D / 2, 0);
            jinZhuDimension = new CircularProfile(0, 0, (D + cun) / 2, 0);
            zhongZhuDimension = new CircularProfile(0, 0, (D + 2 * cun) / 2, 0);
            shanZhuDimension = new CircularProfile(0, 0, (D + 2 * cun) / 2, 0); 
            chongYanJinZhuDimension = new CircularProfile(0, 0, (D + 2 * cun) / 2, 0);

            DA.SetData(0, yanZhuDimension);
            DA.SetData(1, jinZhuDimension);
            DA.SetData(2, zhongZhuDimension);
            DA.SetData(3, shanZhuDimension);
            DA.SetData(4, chongYanJinZhuDimension);
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
            get { return new Guid("ED2A407E-C088-4195-977F-F83186DCC4C3"); }
        }
    }
}