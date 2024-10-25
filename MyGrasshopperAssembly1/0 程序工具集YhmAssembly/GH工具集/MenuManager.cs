using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry.SpatialTrees;
using Rhino;

namespace YhmAssembly
{
    internal class MenuManager
    {
        GH_Component gh_Component { get; set; }
        public ToolStripDropDown menu { get; set; }
        private string[] _menuItemNames;
        public string[] menuItemNames
        {
            get { return _menuItemNames; }
            set
            {
                _menuItemNames = value;
                for (int i = 0; i < _menuItemNames.Length; i++)
                {
                    GH_DocumentObject.Menu_AppendItem(menu, menuItemNames[i], null, false, @checked: false);
                }
                eventHandlers = new EventHandler[_menuItemNames.Length];
                menuState = Enumerable.Repeat(true, _menuItemNames.Length).ToArray();
            }
        }//创建menuItemNames的同时创建出GH中的菜单与事件处理委托、menuState

        //menuState与trueButton关联，设置trueButton即设置menuState
        public bool[] menuState { get; set; }

        private int _trueButton = -1;//初始值为-1，表示未调用
        public int trueButton
        {
            get { return _trueButton; }
            set
            {
                if (value < 0 || value > menu.Items.Count - 1 - correctNum)
                {
                    return;
                }
                _trueButton = value;
                menuState = menuState.Select((x, i) => i != _trueButton).ToArray();
            }
        }
        EventHandler[] eventHandlers { get; set; }//事件处理委托
        int correctNum = 5;//修正index的系数

        //Construct
        public MenuManager(ref GH_Component gh_Component, ref ToolStripDropDown menu)
        {
            this.gh_Component = gh_Component;
            this.menu = menu;
        }

        public void UpdateMenu()
        {
            for (int i = 0; i < eventHandlers.Count(); i++)
            {
                menu.Items[i + correctNum].Enabled = menuState[i];
            }
        }

        public void SetMenuItemAction(int index, params Action[] actionList)
        {
            menu.Items[index + correctNum].Click -= eventHandlers[index];
            if (index >= 0 && index < eventHandlers.Length)
            {
                eventHandlers[index] = (s, eventArgs) =>
                {
                    foreach (Action action in actionList) action.Invoke();

                    trueButton = index;
                    UpdateMenu();//不知为何不起作用
                };
            }
            menu.Items[index + correctNum].Click += eventHandlers[index];

        }//为菜单第index项设置动态操作方法（菜单设置）
    }
}
