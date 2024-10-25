using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using IOComponents;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper;

namespace MyGrasshopperAssembly1
{
    public class 横段内交线_小式亭子 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 取内交线 class.
        /// </summary>
        public 横段内交线_小式亭子()
          : base("横段内交线_小式亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.last;
        private static readonly ParamDefinition[] inputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "横段线",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.list,
                Optional = true,
            }),
        };
        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
        };


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> inputBaseCurves = new List<BaseCurve>();
            if (!DA.GetDataList(0, inputBaseCurves)) return;
            if (inputBaseCurves == null) return;
            double tolerance = 0.001;

            List<BaseCurve> resultBaseCurves = new List<BaseCurve>();
            var groupedCurves = inputBaseCurves
                .Where(baseCurve => baseCurve != null && baseCurve.curve != null)
                .GroupBy(baseCurve => Math.Round(baseCurve.curveLength / tolerance) * tolerance)
                .OrderBy(group => group.Key)
                .ToList();
            if (groupedCurves.Count > 1)
            {
                //取出第一组，短组
                List<BaseCurve> shortBaseCurves = groupedCurves.FirstOrDefault()
                    .Select(x => x.Duplicate())
                    .ToList();

                //短组缩放生成cutter
                List<BaseCurve> cutterBaseCurves = groupedCurves.FirstOrDefault()
                    .Select(x => x.Duplicate())
                    .ToList();
                foreach (BaseCurve cutterBaseCurve in cutterBaseCurves)
                {
                    cutterBaseCurve.Scale1D(1.2, 0.5);
                }
                List<Curve> cutters = cutterBaseCurves
                    .Select(x => x.curve)
                    .ToList();

                //取出第二组，长组
                List<BaseCurve> longBaseCurves = groupedCurves.ElementAtOrDefault(1)
                    .Select(x => x.Duplicate())
                    .ToList();

                //剪切
                for (int i = 0; i < longBaseCurves.Count; i++)
                {
                    Point3d center = longBaseCurves[i].curve.PointAt(0.5);
                    List<BaseCurve> tmpBaseCurves = BaseCurveUtility.CurvesCutBaseCurve(cutters.ToArray(), longBaseCurves[i]);
                    BaseCurve closestCurve = tmpBaseCurves
                        .OrderBy(x => x.curve.PointAt(0.5).DistanceTo(center))
                        .First();

                    resultBaseCurves.Add(closestCurve);
                }
                resultBaseCurves.AddRange(shortBaseCurves);

            }
            

            var resultGroupedCurves = resultBaseCurves
                .Where(baseCurve => baseCurve != null && baseCurve.curve != null)
                .GroupBy(baseCurve => Math.Round(baseCurve.curveLength / tolerance) * tolerance)
                .OrderBy(group => group.Key)
                .ToList();

            //输出管理
            GH_Component currentComponent = this;
            ParamsManerger paramsManerger = new ParamsManerger(ref currentComponent);
            List<string> tmpS = new List<string>();
            for (int i = 0; i < resultGroupedCurves.Count(); i++)
            {
                tmpS.Add("横段线");
            }
            List<string[]> outputNickNamesList = new List<string[]>();
            outputNickNamesList.Add(tmpS.ToArray());
            paramsManerger.outputNickNames = outputNickNamesList;
            paramsManerger.OutputSwitch(0);

            //将Group转化为列表
            List<List<BaseCurve>> valuesList = new List<List<BaseCurve>>();
            for (int i = 0; i < resultGroupedCurves.Count; i++)
            {
                List<BaseCurve> values = resultGroupedCurves[i].ToList();
                valuesList.Add(values);
            }
            for (int i = 0; i < valuesList.Count; i++)
            {
                try
                {
                    DA.SetDataList(i, valuesList[i]);
                }
                catch (Exception ex)
                {
                    for (int j = 0; j < valuesList[i].Count(); j++)
                    {
                        currentComponent.Params.Output[i].AddVolatileData(new GH_Path(0), j, valuesList[i][j]);
                    }
                }
            }
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
            get { return new Guid("812E936C-A287-4D64-B03E-CA652BD96976"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}