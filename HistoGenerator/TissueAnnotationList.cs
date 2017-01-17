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
        List<TissueAnnotationClass> tissueList;

        public TissueAnnotaionList()
        {
            this.tissueList = new List<TissueAnnotationClass>();
        }
        
        public void add(TissueAnnotationClass annotaion)
        {
            this.tissueList.Add(annotaion);
        }
        public TissueAnnotationClass getElement(int index)
        {
            return this.tissueList.ElementAt(index);
        }
        public void sort()
        {
            tissueList.Sort();
        }
        public void cleanUp()
        {
            List<TissueAnnotationClass> toDelete = new List<TissueAnnotationClass>();
            foreach(TissueAnnotationClass annotation in this.tissueList)
            {
                if(annotation.getAnnotaionClass().Equals("") ||
                    annotation.getAnnotaionClass().Equals("."))
                {
                    toDelete.Add(annotation);
                }
            }
            foreach (TissueAnnotationClass annotation in toDelete)
            {
                this.tissueList.Remove(annotation);
            }
        }
        public TissueAnnotaionList groubByOnClassAndOthers(String ClassName)
        {
            TissueAnnotaionList newList = new TissueAnnotaionList();
            foreach(TissueAnnotationClass annotaion in this.tissueList)
            {
                TissueAnnotationClass newAnnotaion = annotaion.Clone();
                if (!newAnnotaion.getAnnotaionClass().Equals(ClassName))
                {
                    newAnnotaion.setAnnotaionClassOther();
                }
                newList.add(newAnnotaion);
            }
            newList.sort();
            return newList;
        }

        public void writeToCsv(string path)
        {

            File.WriteAllText(path, TissueAnnotationClass.getCsvHeadForamt() + "\n", System.Text.Encoding.UTF8);
            foreach(TissueAnnotationClass annotaion in tissueList)
            {
                File.AppendAllText(path,annotaion.getCsvFormat() + "\n");
            }
        }
        
    }
}
