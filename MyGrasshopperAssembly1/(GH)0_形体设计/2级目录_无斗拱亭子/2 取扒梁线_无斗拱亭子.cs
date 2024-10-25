using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types.Transforms;
using IOComponents;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using Grasshopper.GUI.Canvas;
using Grasshopper;
using GH_IO.Serialization;

namespace MyGrasshopperAssembly1
{
    public class 取扒梁线_无斗拱亭子 : ZuiComponent
    {
        public 取扒梁线_无斗拱亭子()
          : base("取扒梁线_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.last;
        private static readonly ParamDefinition[] inputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "檐檩横段线",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.list,
                Optional = true,
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "金檩横段线",
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

        int lastLeft = -1;//初始化值为-1
        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("LastLeft", lastLeft); // 保存参数值
            return base.Write(writer);
        }

        // 从 Grasshopper 文件加载参数值
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetInt32("LastLeft", ref lastLeft); // 从文件中读取参数值
            return base.Read(reader);
        }

        ParamsManerger paramsManerger { get; set; }
        protected override void BeforeSolveInstance()
        {
            GH_Component currentComponent = this;
            ParamsManerger paramsManerger = new ParamsManerger(ref currentComponent);
            List<string[]> inputNickNamesList = new List<string[]>
            {
                new string[] { "檐檩横段线", "金檩横段线", "位置索引" },//偶
                new string[] { "檐檩横段线", "金檩横段线", "开端/起点" }//奇
            };

            List<string[]> outputNickNamesList = new List<string[]>
            {
                new string[] { "扒梁长段线", "扒梁短段线"  },//偶
                new string[] { "扒梁线"  }//奇
            };
            paramsManerger.inputNickNames = inputNickNamesList;
            paramsManerger.outputNickNames = outputNickNamesList;
            this.paramsManerger = paramsManerger;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Component currentComponent = this;
            List<BaseCurve> yanLingBaseCurves = new List<BaseCurve>();
            List<BaseCurve> jinLingBaseCurves = new List<BaseCurve>();
            //只有完整输入时才会运行后续
            if (!DA.GetDataList(0, yanLingBaseCurves)) return;
            if (!DA.GetDataList(1, jinLingBaseCurves)) return;
            int edgeCount = yanLingBaseCurves.Count;

            //偶数输出
            List<BaseCurve> upAndDownBaseCurves = new List<BaseCurve>();
            List<BaseCurve> rightAndLeftBaseCurves = new List<BaseCurve>();
            //奇数输出
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            int currentLeft = edgeCount % 2;

            Object _index = default(Object); int index = 0;//偶数
            Object _startOrEnd = default(Object); int startOrEnd = 0;//奇数

            Action action0 = () =>
            {
                try
                {
                    if (!DA.GetData(2, ref _index)) return;
                    GH_Convert.ToInt32(_index, out index, GH_Conversion.Both);
                    if (index > yanLingBaseCurves.Count - 1 || index < 0)
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "索引错误！");
                        return;
                    }

                    EvenSituation(yanLingBaseCurves, jinLingBaseCurves, edgeCount, index, out upAndDownBaseCurves, out rightAndLeftBaseCurves);
                    DA.SetDataList(0, upAndDownBaseCurves);
                    DA.SetDataList(1, rightAndLeftBaseCurves);
                }
                catch (Exception ex)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "");
                }

            };//偶数
            Action action1 = () =>
            {
                try
                {
                    if (!DA.GetData(2, ref _startOrEnd)) return;
                    GH_Convert.ToInt32(_startOrEnd, out startOrEnd, GH_Conversion.Both);
                    if (startOrEnd < 0 || startOrEnd > 1)
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "开端/起点请输出0或1！");
                        return;
                    }
                    OddSituation(yanLingBaseCurves, jinLingBaseCurves, startOrEnd, out baseCurves);
                    DA.SetDataList(0, baseCurves);
                }
                catch (Exception ex)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "");
                }
            };//奇数


            //内容性质不变
            if (currentLeft == lastLeft)
            {
                if (currentLeft == 0)
                {
                    action0.Invoke();
                }
                else if (currentLeft == 1)
                {
                    action1.Invoke();
                }
            }
            //内容性质变化（偶数->奇数 或 奇数->偶数）
            else
            {
                lastLeft = currentLeft;
                if (currentLeft == 0)
                {
                    //切到currentLeft，currentLeft=0就切换到偶数个
                    paramsManerger.InputSwitch(0);
                    paramsManerger.OutputSwitch(0);
                    action0.Invoke();
                }

                else if (currentLeft == 1)
                {
                    //切到currentLeft，currentLeft=1就切换到奇数个
                    paramsManerger.InputSwitch(1);
                    paramsManerger.OutputSwitch(1);
                    this.Params.OnParametersChanged();
                    action1.Invoke();
                }
            }
        }

        protected override void AfterSolveInstance()
        {
            GH_Component currentComponent = this;
            int n = currentComponent.Params.Output.Count;
            for (int i = 0; i < n; i++)
            {
                currentComponent.Params.Output[i].VolatileData.Flatten();
            }
        }

        private void EvenSituation(List<BaseCurve> yanLingBaseCurves, List<BaseCurve> jinLingBaseCurves, int edgeCount, int index,
            out List<BaseCurve> upAndDownBaseCurves, out List<BaseCurve> rightAndLeftBaseCurves)
        {
            //偶数边形
            List<Point3d> point3Ds = yanLingBaseCurves.Select(x => x.curve.PointAtStart).ToList();//檐檩线多边形顶点
            Point3d center = Point3dUtility.Point3dAverage(point3Ds);//中心
            double yanLingOursideRadius = point3Ds[0].DistanceTo(center);//檐檩线外切圆半径
            double jinLingInsideRadius = center.DistanceTo(jinLingBaseCurves[0].curve.PointAt(0.5)); //金檩内切圆半径
            double jinLingOutsideRadius = center.DistanceTo(jinLingBaseCurves[0].curve.PointAtStart);//金檩外切圆半径

            //center所在XY平面得到井字线
            List<Curve> upAndDownCurves = new List<Curve>();
            List<Curve> rightAndLeftCurves = new List<Curve>();
            Curve c01 = new Line(new Point3d(-yanLingOursideRadius, 0, 0), new Point3d(yanLingOursideRadius, 0, 0)).ToNurbsCurve();
            Curve c02 = c01.DuplicateCurve();

            //井字线长端，上下端
            c01.Transform(Transform.Translation(new Vector3d(0, jinLingInsideRadius, center.Z)));
            c02.Transform(Transform.Translation(new Vector3d(0, -jinLingInsideRadius, center.Z)));
            upAndDownCurves.Add(c01); upAndDownCurves.Add(c02);

            //井字线短端，左右端
            Curve c11 = new Line(new Point3d(0, -jinLingInsideRadius, 0), new Point3d(0, jinLingInsideRadius, 0)).ToNurbsCurve();
            Curve c12 = c11.DuplicateCurve();

            if (edgeCount % 3 == 0)//多边形是3的倍数
            {
                c11.Transform(Transform.Translation(new Vector3d(jinLingOutsideRadius, 0, center.Z)));
                c12.Transform(Transform.Translation(new Vector3d(-jinLingOutsideRadius, 0, center.Z)));
            }
            else//4的倍数
            {
                c11.Transform(Transform.Translation(new Vector3d(jinLingInsideRadius, 0, center.Z)));
                c12.Transform(Transform.Translation(new Vector3d(-jinLingInsideRadius, 0, center.Z)));
            }
            rightAndLeftCurves.Add(c11); rightAndLeftCurves.Add(c12);

            upAndDownBaseCurves = BaseCurveUtility.CurvesToBaseCurves(upAndDownCurves);
            rightAndLeftBaseCurves = BaseCurveUtility.CurvesToBaseCurves(rightAndLeftCurves);

            //下侧井字线与第index根金檩的Curve对映上
            Transform xform = Transform.PlaneToPlane(
                upAndDownBaseCurves[1].referencePlane,
                CurveUtility.GetPlanes(jinLingBaseCurves[index].curve, 0.5)[0]);

            //得到旋转位移后的基线
            for (int i = 0; i < 2; i++)
            {
                upAndDownBaseCurves[i].Transform(xform);
                rightAndLeftBaseCurves[i].Transform(xform);
            }
            Curve[] joinedCurves = Curve.JoinCurves(yanLingBaseCurves.Select(x => x.curve).ToArray()
                , DocumentTolerance());

            //得到剪切后的基线
            if (joinedCurves.Count() != 1)
            {
                return;
            }
            for (int i = 0; i < upAndDownBaseCurves.Count(); i++)
            {
                List<BaseCurve> tmpBaseCurves = BaseCurveUtility.CurveCutBaseCurve(joinedCurves[0], upAndDownBaseCurves[i]);
                BaseCurve[] closestBaseCurve = tmpBaseCurves
                    .GroupBy(x => x.curve.PointAt(0.5).DistanceTo(center))
                    .OrderBy(g => g.Key)
                    .First().ToArray();
                upAndDownBaseCurves[i] = closestBaseCurve[0];
            }
        }

        private void OddSituation(List<BaseCurve> yanLingBaseCurves, List<BaseCurve> jinLingBaseCurves, int startOrEnd
            , out List<BaseCurve> baseCurves)
        {
            List<Point3d> point3Ds = yanLingBaseCurves.Select(x => x.curve.PointAtStart).ToList();//檐檩线多边形顶点
            Point3d center = Point3dUtility.Point3dAverage(point3Ds);//中心
            double yanLingOursideRadius = point3Ds[0].DistanceTo(center);//檐檩线外切圆半径

            //得到joinedCurve
            Curve[] joinedCurves = Curve.JoinCurves(yanLingBaseCurves.Select(x => x.curve).ToArray()
                , DocumentTolerance());

            baseCurves = new List<BaseCurve>();//输出

            //得到剪切后的基线
            if (joinedCurves.Count() != 1) return;

            //0表示start延申，1表示end延申
            List<BaseCurve> scaledBaseCurves = jinLingBaseCurves
                .Select(x => x.Duplicate())
                .ToList();
            foreach (BaseCurve baseCurve in scaledBaseCurves)
            {
                baseCurve.Scale1D((yanLingOursideRadius + baseCurve.curveLength) / baseCurve.curveLength, startOrEnd);
            }


            foreach (BaseCurve scaledBaseCurve in scaledBaseCurves)
            {
                List<BaseCurve> tmpBaseCurves = BaseCurveUtility.CurveCutBaseCurve(joinedCurves[0], scaledBaseCurve);
                BaseCurve[] closestBaseCurve = tmpBaseCurves
                   .GroupBy(x => x.curve.PointAt(0.5).DistanceTo(center))
                   .OrderBy(g => g.Key)
                   .First().ToArray();
                baseCurves.Add(closestBaseCurve[0]);
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
            get { return new Guid("0394A550-55D4-4127-B8C4-20DF73789296"); } 
        }

        protected override ParamDefinition[] Inputs => inputs;

protected override ParamDefinition[] Outputs => outputs;
        
    }
}