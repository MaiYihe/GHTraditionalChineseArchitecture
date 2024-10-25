using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using YhmAssembly;
using System.Linq;
using IOComponents;
using Grasshopper.Kernel.Parameters;
using UtilityElement.ReferenceDatum;
using Grasshopper.GUI.Canvas;
using Grasshopper;
using static Grasshopper.Kernel.GH_ComponentParamServer;
using UtilityElement;

namespace MyGrasshopperAssembly1
{
    public class 取步架_无斗拱亭子 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 步架权衡_小式亭子 class.
        /// </summary>
        public 取步架_无斗拱亭子()
          : base("取步架_无斗拱亭子", "Nickname",
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
            new ParamDefinition(new Param_GenericObject
            {
                Name = "比例",
                NickName = "以‘:’分隔,外檐到内檐",
                Description = "",
                Access = GH_ParamAccess.item,
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
            GH_Component currentComponent = this;
            List<BaseCurve> baseCurves = new List<BaseCurve>(); string R1 = "";
            if (!DA.GetDataList(0, baseCurves) || !DA.GetData(1, ref R1)) return;
            List<double> R1_1 = R1.Split(':').Select(s => double.Parse(s)).ToList();//脊步架、金步架、檐步架
            if (R1_1.Count() == 1 || R1_1.Count() > 3)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "比例错误！");
                return;
            }
            //起始点startPoints
            List<Point3d> startPoints = baseCurves.Select(x => x.curve.PointAtStart).ToList();
            //得到中心点averagePoint
            Point3d averagePoint = Point3dUtility.Point3dAverage(
                baseCurves.Select(x => x.curve.PointAtStart)
                .ToList());
            //得到offset最长距离向量offsetLongestVectorList
            List<Vector3d> offsetLongestVectorList =
                baseCurves.Select(x => new Vector3d(averagePoint - x.curve.PointAt(0.5))).ToList();
            //脊步架、金步架、檐步架的占总步架长的比
            List<double> rateList = R1_1.Select(x => x / R1_1.Sum()).ToList();
            //得各步架
            List<double> buJia = rateList
                .Select(x => x * offsetLongestVectorList[0].Length)
                .ToList();

            //得到点offset最长距离向量offsetLongestVectorPointList
            List<Vector3d> offsetLongestVectorPointList =
                baseCurves.Select(x => new Vector3d(averagePoint - x.curve.PointAtStart)).ToList();

            //取要移动的项，每一项对于前面所有项求和
            List<double> moveRate = rateList.Select((x, i) => rateList.Take(i + 1).Sum()).ToList();
            moveRate.RemoveAt(moveRate.Count - 1);
            //完成baseCurve位移
            List<List<BaseCurve>> movedBaseCurvesList = new List<List<BaseCurve>>();
            for (int i = 0; i < moveRate.Count; i++)
            {
                List<Point3d> tmpP = new List<Point3d>();
                //位移向量、位移
                List<Vector3d> moveVector = offsetLongestVectorPointList
                    .Select(x => x * moveRate[i])
                    .ToList();
                for (int j = 0; j < startPoints.Count; j++)
                {
                    Point3d tmpPItem = startPoints[j];
                    tmpPItem.Transform(Transform.Translation(moveVector[j]));
                    tmpP.Add(tmpPItem);
                }
                tmpP.Add(tmpP[0]);
                //由点生成线，转化为BaseCurve
                List<Curve> tmpCurves = new List<Curve>();
                for (int j = 0; j < tmpP.Count - 1; j++)
                {
                    Curve tmpCurve = new Line(tmpP[j], tmpP[j + 1]).ToNurbsCurve();
                    tmpCurves.Add(tmpCurve);
                }
                List<BaseCurve> tmpBaseCurves = BaseCurveUtility.CurvesToBaseCurves(tmpCurves);
                movedBaseCurvesList.Add(tmpBaseCurves);
            }


            //设置输出
            List<(string, Object)> dataList = new List<(string, object)>();
            Action action0 = () =>
            {
                dataList = new List<(string, object)>
                {
                    ("脊步架", buJia[0]),
                    ("檐步架", buJia[1]),
                    ("金檩横段线",movedBaseCurvesList[0]),
                };
            };

            Action action1 = () =>
            {
                dataList = new List<(string, object)>
                {
                    ("脊步架", buJia[0]),
                    ("金步架", buJia[1]),
                    ("檐步架", buJia[2]),
                    ("上金檩横段线",movedBaseCurvesList[1]),
                    ("下金檩横段线",movedBaseCurvesList[0]),
                };
            };

            GH_Canvas canvas = Instances.ActiveCanvas;
            if (Instances.ActiveCanvas != null) canvas = Instances.ActiveCanvas;
            SourceAndTarget sourceAndTarget = new SourceAndTarget(ref canvas, currentComponent);
            IGH_Param source = sourceAndTarget.source;
            IGH_Param target = sourceAndTarget.target;
            //如果参数变化，就更新输出
            OutputParamsManager outputParamsManager = new OutputParamsManager(ref currentComponent, dataList);
            if (IsParameterChanged || (source != null && target != null))
            {
                if (R1_1.Count == 2)
                {
                    action0.Invoke();
                }
                else if (R1_1.Count == 3)
                {
                    action1.Invoke();
                }
                outputParamsManager = new OutputParamsManager(ref currentComponent, dataList);
                outputParamsManager.ResetOutputsParams();
            }
            else
            {
                if (R1_1.Count == 2)
                {
                    action0.Invoke();
                }
                else if (R1_1.Count == 3)
                {
                    action1.Invoke();
                }
                outputParamsManager = new OutputParamsManager(ref currentComponent, dataList);
                outputParamsManager.DA = DA;
                //运行时直接查询输出
                outputParamsManager.ShowOutputsParams();
            }

            OutputParamsManager.FlattenOutput(ref currentComponent);
            //运行的最后将响应调整成false
        }

        private bool IsParameterChanged { get; set; } = false;
        //参数变化事件
        public event ParameterChangedEventHandler ParameterChanged;
        protected override void AfterSolveInstance()
        {
            // 在 AfterSolveInstance 方法中取消订阅事件
            base.AfterSolveInstance();
            ParameterChanged -= OnParameterChanged;
            IsParameterChanged = false;
        }
        protected override void BeforeSolveInstance()
        {
            // 在 BeforeSolveInstance 方法中订阅事件
            base.BeforeSolveInstance();
            ParameterChanged += OnParameterChanged;
        }
        private void OnParameterChanged(object sender, GH_ParamServerEventArgs e)
        {
            IsParameterChanged = true;
            //一旦参数变化就调用，更新输出端
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
            get { return new Guid("80AD343E-016B-4F04-B617-A9184BEFEC42"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}