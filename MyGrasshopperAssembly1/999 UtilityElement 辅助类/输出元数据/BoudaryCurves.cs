using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annotation
{
    internal class BoudaryCurves
    {
        //由内而外
        List<LevelCurves> LevelCurvesList { get; set; }

        //Construct
        public BoudaryCurves(params LevelCurves[] LevelCurvesList)
        {
            if (LevelCurvesList.Count() > 4) return;
            else
            {
                List<LevelCurves> levelCurvesList = new List<LevelCurves>();
                for (int i = 0; i < LevelCurvesList.Length; i++)
                {
                    levelCurvesList.Add(LevelCurvesList[i]);
                }
            }
        }
    }
}
