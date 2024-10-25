using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YhmAssembly
{
    internal class OutputParamsManager
    {
        GH_Component _gh_Component;
        GH_Component gh_Component
        {
            get { return _gh_Component; }
            set
            {
                _gh_Component = value;
                outputs = _gh_Component.Params.Output;
            }
        }

        List<IGH_Param> outputs { get; set; }//当gh_Component被set时关联生成

        public string[] nickNames { get; set; }//输出nickName列表
        List<object> outputData { get; set; }//输出值列表
        public IGH_DataAccess DA { get; set; }//数据库

        //Construct，构造输出
        public OutputParamsManager()
        {
        }
        public OutputParamsManager(ref GH_Component gh_Component, List<(string, object)> dataList)
        {
            this.gh_Component = gh_Component;

            string[] nickNames = dataList.Select(item => item.Item1).ToArray();//nickName列表
            var outputData = dataList.Select(item => item.Item2).ToList();//输出值列表
            this.nickNames = nickNames;
            this.outputData = outputData;
        }
        //清空输出参数
        public void ClearOutputs()
        {
            while (gh_Component.Params.Output.Count > 0)
            {
                gh_Component.Params.UnregisterOutputParameter(gh_Component.Params.Output[checked(0)]);
            }
            gh_Component.Params.OnParametersChanged();
        }

        //添加输出参数项，会清空所有项再重新添加，不使用DA添加输出参数
        public void ResetOutputsParams()
        {
            ClearOutputs();
            for (int i = 0; i < nickNames.Count(); i++)
            {
                //生成输出项
                IGH_Param new_param = CreateParameter(GH_ParameterSide.Output, i);
                gh_Component.Params.RegisterOutputParam(new_param);
                gh_Component.Params.Output[i].NickName = nickNames[i];
                //为输出项赋值
                outputs[i].ClearData();
                if (listEdit.IsList(outputData[i].GetType()))
                {
                    IList tmpDataList = outputData[i] as IList;
                    outputs[i].AddVolatileDataList(new GH_Path(0), tmpDataList);
                }
                else
                outputs[i].AddVolatileData(new GH_Path(0), 0, outputData[i]);
            }
            gh_Component.Params.OnParametersChanged();
        }


        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }
        public void ShowOutputsParams()
        {
            for (int i = 0; i < nickNames.Count(); i++)
            {
                if (listEdit.IsList(outputData[i].GetType()))
                {
                    IList tmpDataList = outputData[i] as IList;
                    DA.SetDataList(i, tmpDataList);
                }
                else
                    DA.SetData(i, outputData[i]);

            }
        }



        Dictionary<string, object> dataDictionary { get; set; }//数据字典:nickName,值
        //Construct，查询对映输出
        public OutputParamsManager(ref GH_Component gh_Component, ref IGH_DataAccess DA)
        {
            this.gh_Component = gh_Component;
            this.DA = DA;
            this.outputs = gh_Component.Params.Output;
            this.nickNames = outputs.Select(item => item.NickName).ToArray();//nickName列表
        }
        public OutputParamsManager(ref GH_Component gh_Component)
        {
            this.gh_Component = gh_Component;
            this.outputs = gh_Component.Params.Output;
            this.nickNames = outputs.Select(item => item.NickName).ToArray();//nickName列表
        }
        public void AddToDictionary(params List<(string, object)>[] lists)
        {
            Dictionary<string, object> dataDictionary = new Dictionary<string, object>();
            Dictionary<string, object> tempDictionary = new Dictionary<string, object>();
            foreach (var list in lists)
            {
                tempDictionary = list.ToDictionary(item => item.Item1, item => item.Item2);
                dataDictionary = dataDictionary.Union(tempDictionary).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);//在Union 方法中，重复的元素只会出现一次
            }
            this.dataDictionary = dataDictionary;
        }
        public void LookupDictionaryAndOutput()
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                object outputResult;
                if (dataDictionary.TryGetValue(nickNames[i], out outputResult))
                {
                    DA.SetData(i, outputResult);
                }
            }
        }
        public static void FlattenOutput(ref GH_Component gH_Component)
        {
            var gH_OutputList = gH_Component.Params.Output;
            foreach (var OutputItem in gH_OutputList)
            {
                OutputItem.VolatileData.Flatten();
            }
        }//将Output各项拍平


        
    }
}
