using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HistoGenerator
{
    class TissueAnnotaionList
    {
        List<TissueClass> tissueList;

        public TissueAnnotaionList()
        {
            this.tissueList = new List<TissueClass>();
        }
        
        public void add(TissueClass annotaion)
        {
            this.tissueList.Add(annotaion);
        }
        public TissueClass getElement(int index)
        {
            return this.tissueList.ElementAt(index);
        }
        public void sort()
        {
            tissueList.Sort();
        }
        public TissueAnnotaionList groubByOnClassAndOthers(String ClassName)
        {
            TissueAnnotaionList newList = new TissueAnnotaionList();
            foreach(TissueClass annotaion in this.tissueList)
            {
                TissueClass newAnnotaion = annotaion.Clone();
                if (!newAnnotaion.getAnnotaionClass().Equals(ClassName))
                {
                    newAnnotaion.setAnnotaionClassOther();
                }
                newList.add(newAnnotaion);
            }
            newList.sort();
            return newList;
        }

        public void writeToCsv(String path)
        {

            File.WriteAllText(path,"Class;H-Q25;H-Median;H-Q75;E-Q25;E-Median;E-Q75\n",System.Text.Encoding.UTF8);
            foreach(TissueClass annotaion in tissueList)
            {
                File.AppendAllText(path,annotaion.getCsvFormat() + "\n");
            }
        }
        
    }
}
