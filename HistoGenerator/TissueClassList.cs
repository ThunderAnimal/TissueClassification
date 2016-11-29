using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoGenerator
{
    class TissueAnnotaionList
    {
        List<TissueClass> tissueList;

        public TissueAnnotaionList()
        {
            this.tissueList = new List<TissueClass>();
        }

        public void addTissueAnnotaion(TissueClass annotaion)
        {
            this.tissueList.Add(annotaion);
        }
        
    }
}
