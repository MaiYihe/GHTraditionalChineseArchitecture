using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using YhmAssembly;
using System.Linq;
using Grasshopper;
using GH_IO.Serialization;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using IOComponents;

namespace MyGrasshopperAssembly1
{
    public class 檩向线分类_无斗栱屋架 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public 檩向线分类_无斗栱屋架()
          : base("檩向线分类_无斗栱屋架", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        private static readonly ParamDefinition[] inputs = new ParamDefinition[1]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "檩向线",
                NickName = "LXX",
                Description = "The point to search from",
                Access = GH_ParamAccess.item,
                Optional = true
            }),
        };

        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "-",
                NickName = "-",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true
            }),
        };
        public override GH_Exposure Exposure => GH_Exposure.secondary;//暴露在二级标题下

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.DisableGapLogic();
            GH_Structure<IGH_Goo> gH_StructureInput = (GH_Structure<IGH_Goo>)this.Params.Input[0].VolatileData;//输入树
            if (gH_StructureInput == null || gH_StructureInput.IsEmpty || gH_StructureInput.PathCount != 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "数据结构错误！");
                return;
            }
            checked
            {
                int outputNum = this.Params.Output.Count;
                for (int i = 0; i < outputNum; i++)
                {
                    GH_Structure<IGH_Goo> gH_Structure2 = (GH_Structure<IGH_Goo>)this.Params.Output[i].VolatileData;
                    gH_Structure2.Clear();
                    SetOutputTree(i, gH_StructureInput);
                }
            }
            UpdateOutputs();
        }

        bool isCombine = true;
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("IsCombine", isCombine); // 保存参数值
            return base.Write(writer);
        }

        // 从 Grasshopper 文件加载参数值
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("IsCombine", ref isCombine); // 从文件中读取参数值
            return base.Read(reader);
        }

        /// <summary>
        /// 对菜单与输出项的扩展与收紧进行操作
        /// </summary>
        //wire连线判断：判断wire是否更新，判断是否是对当前实例的更新，若是则运行WhetherMirrorItem

        private object sender { get; set; }
        private MouseEventArgs e { get; set; }
        public void GetEvents(object sender, MouseEventArgs e)
        {
            this.sender = sender;
            this.e = e;
        }
        GH_Canvas canvas;
        public override void CreateAttributes()
        {
            Action action = default(Action);
            base.CreateAttributes();
            if (Instances.ActiveCanvas != null) canvas = Instances.ActiveCanvas;
            else return;
            GH_Component currentComponent = this;
            SourceAndTarget sourceAndTarget = new SourceAndTarget(ref canvas, currentComponent);
            IGH_Param source = sourceAndTarget.source;
            IGH_Param target = sourceAndTarget.target;
            if (source != null && target != null)
            {
                action = () => WhetherMirrorItem();//传递委托
                WireConnectionDetect wireConnection = new WireConnectionDetect(ref canvas, sender, e, ref action);
            }
        }

        private void WhetherMirrorItem()
        {
            if (isCombine) MergeMirrorItem();
            else ExpandMirrorItem();
        }//初始化时调用

        //menu事件判断
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_DocumentObject.Menu_AppendSeparator(menu);
            GH_DocumentObject.Menu_AppendItem(menu, "Update", UpdateOutputs);
            GH_DocumentObject.Menu_AppendItem(menu, "合并对称项", MergeMirrorItem, !isCombine, @checked: false);
            GH_DocumentObject.Menu_AppendItem(menu, "展开对称项", ExpandMirrorItem, isCombine, @checked: false);
        }//点击Menu会触发的事件
        private void MergeMirrorItem()
        {
            isCombine = true;
            AdjustWholeComponent(isCombine);
            //ExpireSolution(recompute: true);
        }
        private void ExpandMirrorItem()
        {
            isCombine = false;
            AdjustWholeComponent(isCombine);
            //ExpireSolution(recompute: true);
        }
        private void MergeMirrorItem(object sender, EventArgs e)
        {
            isCombine = true;
            AdjustWholeComponent(isCombine);
            //ExpireSolution(recompute: true);
        }
        private void ExpandMirrorItem(object sender, EventArgs e)
        {
            isCombine = false;
            AdjustWholeComponent(isCombine);
            //ExpireSolution(recompute: true);
        }
        private void UpdateOutputs()
        {
            AdjustWholeComponent(isCombine);
        }
        private void UpdateOutputs(object sender, EventArgs e)
        {
            AdjustWholeComponent(isCombine);
            //ExpireSolution(recompute: true);
        }
        private void AdjustWholeComponent(bool isCombine)
        {
            GH_Structure<IGH_Goo> gH_StructureInput = (GH_Structure<IGH_Goo>)this.Params.Input[0].VolatileData;//输入树            
            checked//如果不使用 checked 关键字，当 this.Params.Output.Count 的值比 Int32.MaxValue 大，并且 1 被减去后的结果比 Int32.MinValue 小的时候，整数溢出就会发生
            {
                AutoCreateTree(isCombine);//自动创建输出项Output[]
                int num = this.Params.Input[0].VolatileData.DataCount;//输入项的数量
                if (num % 2 != 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "檩向线数非奇数！");
                    return;
                }
                NegIndexList<String> negIndexList = new NegIndexList<String>();
                List<String> normalList = new List<String>();
                int watershed = 0;
                int middleItemIndex = num / 2;//中间项的index，即脊檩的index
                //存入negIndexList，脊檩的NickName
                negIndexList.PositiveAdd("脊檩");
                //存入negIndexList，金檩的NickName
                for (int i = 1; i < num - 1; i++)
                {
                    // 设置不同的 NickName
                    if (i - middleItemIndex < 0)
                    {
                        negIndexList.NegtiveAdd("金檩" + (i - 1).ToString());
                    }
                    else if (i - middleItemIndex > 0)
                    {
                        negIndexList.PositiveAdd("金檩" + (i - middleItemIndex - 1).ToString());
                    }
                    else if (i - middleItemIndex == 0) continue;
                }
                //存入negIndexList，檐檩的NickName
                negIndexList.PositiveAdd("檐檩");
                negIndexList.NegtiveAdd("檐檩");
                normalList = negIndexList.ToNormal(out watershed);//negIndexList转化为正常列表

                if (isCombine)
                {
                    for (int i = 0; i < watershed; i++)
                    {
                        this.Params.Output[i].NickName = normalList[i];//改NickNmae
                        SetOutputTree(i, gH_StructureInput);
                        AddOutputTree(i, num - i - 1, gH_StructureInput);
                    }
                    this.Params.Output[watershed].NickName = normalList[watershed];//改NickNmae
                    SetOutputTree(watershed, gH_StructureInput);
                    ReverseOutput();
                }
                else
                {
                    for (int i = 0; i < num; i++)
                    {
                        this.Params.Output[i].NickName = normalList[i];//改NickNmae
                        SetOutputTree(i, gH_StructureInput);
                    }
                }
                FlattenOutput();
            }
        }
        private void SetOutputTree(int index, GH_Structure<IGH_Goo> gH_StructureInput)
        {
            GH_Structure<IGH_Goo> gH_StructureGraft = gH_StructureInput.Duplicate();//复制输入树
            GH_GraftMode gH_GraftMode = GH_GraftMode.None;
            gH_StructureGraft.Graft(gH_GraftMode);//Graft输入树
            if (index >= gH_StructureGraft.PathCount)
            {
                this.Params.Output[index].NickName = "{ }";
                return;
            }
            GH_Structure<IGH_Goo> gH_StructureOutput = (GH_Structure<IGH_Goo>)this.Params.Output[index].VolatileData;//输出树，第index个 
            gH_StructureOutput.Clear();
            gH_StructureOutput.AppendRange(gH_StructureGraft.Branches[index], gH_StructureGraft.get_Path(index));//设置第index个输出树的值
        }//由输入树与对映index设置输出树
        private void AddOutputTree(int index, int addedItemIndex, GH_Structure<IGH_Goo> gH_StructureInput)
        {
            GH_Structure<IGH_Goo> gH_StructureGraft = gH_StructureInput.Duplicate();//复制输入树
            GH_GraftMode gH_GraftMode = GH_GraftMode.None;
            gH_StructureGraft.Graft(gH_GraftMode);//Graft输入树
            GH_Structure<IGH_Goo> gH_StructureOutput = (GH_Structure<IGH_Goo>)this.Params.Output[index].VolatileData;
            gH_StructureOutput.AppendRange(gH_StructureGraft.Branches[addedItemIndex], gH_StructureGraft.get_Path(addedItemIndex));//设置第index个输出树的值            
        }//添加
        private void ReverseOutput()
        {
            var gH_OutputList = this.Params.Output;
            List<String> newNickNames = gH_OutputList.Select(outPutItem => outPutItem.NickName).ToList();
            newNickNames.Reverse();//新的nickName列表

            List<List<IGH_Goo>> branchList = new List<List<IGH_Goo>>();
            List<List<GH_Path>> pathList = new List<List<GH_Path>>();
            for (int i = 0; i < gH_OutputList.Count; i++)
            {
                //新的branchList列表
                int tmpInt = gH_OutputList[i].VolatileData.AllData(true).Count();
                List<IGH_Goo> outPutItem = gH_OutputList[i].VolatileData.AllData(true).ToList();
                branchList.Add(outPutItem);
                //新的pathList列表
                IList<GH_Path> outPutItemPath = gH_OutputList[i].VolatileData.Paths;
                List<GH_Path> pathItem = new List<GH_Path>(outPutItemPath);
                pathList.Add(pathItem);
            }
            branchList.Reverse();
            pathList.Reverse();

            int count = gH_OutputList.Count;//输出参数的数量
            for (int i = 0; i < count; i++)
            {
                GH_Structure<IGH_Goo> gH_StructureOutput = (GH_Structure<IGH_Goo>)this.Params.Output[i].VolatileData;
                gH_StructureOutput.Clear();


                for (int j = 0; j < pathList[i].Count; j++)
                {// 创建包含单个元素的列表
                    List<IGH_Goo> singleItemBranchList = new List<IGH_Goo>();
                    singleItemBranchList.Add(branchList[i][j]);
                    gH_StructureOutput.AppendRange(singleItemBranchList, pathList[i][j]);
                }
                gH_OutputList[i].NickName = newNickNames[i];
            }
        }//反转输出Output的NickName与数据
        private void FlattenOutput()
        {
            var gH_OutputList = this.Params.Output;
            foreach (var OutputItem in gH_OutputList)
            {
                OutputItem.VolatileData.Flatten();
            }
        }//将Output各项拍平
        private void AutoCreateTree(bool isCombine)
        {
            int dataCount = this.Params.Input[0].VolatileData.DataCount;
            int initialCount = this.Params.Output.Count; // 获取初始输出参数数量
            if (!isCombine && this.Params.Output.Count == dataCount)
            {
                return;
            }

            RecordUndoEvent("檩向线分类");
            int finalCount = 0;
            // 添加或删除输出参数，具体取决于 isCombine 的值
            if (!isCombine)
            {
                finalCount = dataCount;
                while (this.Params.Output.Count < finalCount)
                {
                    IGH_Param newParam = CreateParameter(GH_ParameterSide.Output, this.Params.Output.Count);
                    this.Params.RegisterOutputParam(newParam);
                }

                while (this.Params.Output.Count > finalCount)
                {
                    this.Params.UnregisterOutputParameter(this.Params.Output[this.Params.Output.Count - 1]);
                }
            }
            else
            {
                finalCount = dataCount / 2 + 1;
                while (this.Params.Output.Count < finalCount)
                {
                    IGH_Param newParam = CreateParameter(GH_ParameterSide.Output, this.Params.Output.Count);
                    this.Params.RegisterOutputParam(newParam);
                }

                while (this.Params.Output.Count > finalCount)
                {
                    this.Params.UnregisterOutputParameter(this.Params.Output[this.Params.Output.Count - 1]);
                }
            }

            //更新后的输出参数数量
            if (initialCount != finalCount)
            {
                this.Params.OnParametersChanged();
                VariableParameterMaintenance();
                ExpireSolution(recompute: true);//用于通知 Grasshopper 进行重新计算
            }//保证输出参数变化时GH才重新进行运算
        }//自动创建输出项Output[]
        public override IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
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
            get { return new Guid("1F1BEE44-7441-4CCC-8DA5-56C0B6CDF64F"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}