using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annotation
{
    internal class View
    {
        List<BoudaryCurves> BoudaryCurvesList { get; set; }

        public View(BoudaryCurves[] boudaryCurvesList)
        {
            if (boudaryCurvesList.Count() > 4) return;
            else
            {
                List<BoudaryCurves> BoudaryCurvesList = new List<BoudaryCurves>();
                for (int i = 0; i < boudaryCurvesList.Count(); i++)
                {
                    this.BoudaryCurvesList.Add(boudaryCurvesList[i]);
                }
            }
        }

    }
}
