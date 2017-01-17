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

            File.WriteAllText(path, this.getCsvHeadForamt() + "\n", System.Text.Encoding.UTF8);
            foreach(TissueAnnotationClass annotaion in tissueList)
            {
                File.AppendAllText(path,annotaion.getCsvFormat() + "\n");
            }
        }
        private string getCsvHeadForamt()
        {
            return
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33}",
                    "Class",
                    "H - Q25",
                    "H - Median",
                    "H - Q75",
                    "E - Q25",
                    "E - Median",
                    "E - Q75",
                    "CountCores",
                    "CountLumina",
                    "MidCoresSize",
                    "MeanCoresSize",
                    "Q25CoresSize",
                    "Q75CoresSize",
                    "MidLuminaSize",
                    "MeanLuminaSize",
                    "Q25LuminaSize",
                    "Q75LuminaSize",
                    "DensityCores",
                    "MidFormFactorCores",
                    "MeanFormFactorCores",
                    "Q25FormFactorCores",
                    "Q75FormFactorCores",
                    "MidFormFactorLuminaWithSize",
                    "MeanFormFactorLuminaWithSize",
                    "Q25FormFactorLuminaWithSize",
                    "Q75FormFactorLuminaWithSize",
                    "MidDensityLuminaCoresInNear",
                    "MeanDensityLuminaCoresInNear",
                    "Q25DensityLuminaCoresInNear",
                    "Q75DensityLuminaCoresInNear",
                    "MidDensityFormFactorLuminaCoresInNear",
                    "MeanDensityFormFactorLuminaCoresInNear",
                    "Q25DensityFormFactorLuminaCoresInNear",
                    "Q75DensityFormFactorLuminaCoresInNear");
        }

    }
}
