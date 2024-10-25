using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Rhino;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YhmAssembly
{
    internal class InputParamsManager
    {
        GH_Component _gh_Component;
        GH_Component gh_Component
        {
            get { return _gh_Component; }
            set
            {
                _gh_Component = value;
                inputs = _gh_Component.Params.Input;
            }
        }

        List<IGH_Param> inputs { get; set; }//当gh_Component被set时关联生成
        List<object> inputData { get; set; }//输入值列表
        public string[] nickNames { get; set; }//输入nickName列表
        IGH_DataAccess DA { get; set; }//数据库

        //Construct
        public InputParamsManager()
        {
        }
        public InputParamsManager(ref GH_Component gh_Component, List<(string, object)> dataList)
        {
            this.gh_Component = gh_Component;
            string[] nickNames = dataList.Select(item => item.Item1).ToArray();//nickName列表
            var inputData = dataList.Select(item => item.Item2).ToList();//输入值列表
            this.nickNames = nickNames;
            this.inputData = inputData;
        }
        public InputParamsManager(ref GH_Component gh_Component)
        {
            this.gh_Component = gh_Component;
        }
        //清空输入参数
        private void ClearInputs()
        {
            while (inputs.Count > 0)
            {
                gh_Component.Params.UnregisterInputParameter(inputs[checked(0)]);
            }
            gh_Component.Params.OnParametersChanged();
        }

        //添加输出参数项，会清空所有项再重新添加，不使用DA添加输出参数
        public void ResetInputsParams()
        {
            ClearInputs();
            for (int i = 0; i < nickNames.Count(); i++)
            {
                //生成输入项
                IGH_Param new_param = CreateParameter(GH_ParameterSide.Input, i);
                new_param.Optional = true;
                gh_Component.Params.RegisterInputParam(new_param);
                inputs[i].NickName = nickNames[i];
            }
            gh_Component.Params.OnParametersChanged();
        }        
        IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }
    }
}
