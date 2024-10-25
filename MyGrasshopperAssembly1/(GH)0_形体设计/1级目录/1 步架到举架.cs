using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Commands;
using Rhino.Geometry;

namespace MyGrasshopperAssembly1
{
    public class 步架到举架 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public 步架到举架()
          : base("步架到举架", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("规格","GG","九檩、七檩、五檩",GH_ParamAccess.item);
            pManager.AddNumberParameter("柱高","CH","...",GH_ParamAccess.item);
            pManager.AddNumberParameter("步架", "BJ", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("举高","JG","...",GH_ParamAccess.list);
            pManager.AddNumberParameter("通举高", "TJG", "...", GH_ParamAccess.list);
            pManager.AddNumberParameter("柱总高", "ZZG", "...", GH_ParamAccess.list);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Double> nine = new List<Double> {0,0.5,0.65,0.75,0.9};//九檩抬高
            List<Double> seven = new List<Double> {0,0.5,0.7,0.8};//七檩抬高
            List<Double> five = new List<Double> {0,0.5,0.7};//五檩抬高

            String GG = "";//规格
            Double CH = new Double();//柱高
            Double BJ = new Double();//步架
            DA.GetData(0,ref GG);//规格
            DA.GetData(1,ref CH);//柱高
            DA.GetData(2,ref BJ);//步架

            List<Double> JG = new List<Double>();//举高
            List<Double> TJG = new List<Double>();//通举高
            List<Double> ZZG = new List<Double>();//柱总高
            Double runningSum = 0.0;

            if (GG == "九檩")
            {
                JG = nine.Select(x => x*BJ).ToList();
                runningSum = 0.0;
                TJG = JG.Aggregate(new List<Double>(), (result, value) => {
                    runningSum += value;
                    result.Add(runningSum);
                    return result;
                });
                ZZG.AddRange(TJG.Select(x => x+CH).ToList());
            }
            else if (GG == "七檩")
            {
                JG = seven.Select(x => x * BJ).ToList();
                TJG = JG.Aggregate(new List<Double>(), (result, value) => {
                    runningSum += value;
                    result.Add(runningSum);
                    return result;
                });
                ZZG.AddRange(TJG.Select(x => x + CH).ToList());
            }
            else if (GG == "五檩")
            {
                JG = five.Select(x => x * BJ).ToList();
                TJG = JG.Aggregate(new List<Double>(), (result, value) => {
                    runningSum += value;
                    result.Add(runningSum);
                    return result;
                });
                ZZG.AddRange(TJG.Select(x => x + CH).ToList());
            }

            DA.SetDataList(0,JG);
            DA.SetDataList(1,TJG);
            DA.SetDataList(2,ZZG);//柱总高
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
            get { return new Guid("D0D7B388-9EB6-4640-BFF1-1647A481D8D7"); }
        }
    }
}