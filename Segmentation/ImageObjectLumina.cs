using SharpAccessory.Imaging.Classification;
using SharpAccessory.Imaging.Segmentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation
{
    class ImageObjectLumina : ImageObject
    {

        private List<ImageObject> nearCellCores = new List<ImageObject>();

        public ImageObjectLumina(uint id): base(id)
        {

        }
        public ImageObjectLumina(uint id, Contour contour) : base(id, contour)
        {

        }
        public ImageObjectLumina(uint id, Class cls) : base(id, cls)
        {

        }
        public ImageObjectLumina(uint id, Contour contour, Class cls) : base(id, contour, cls)
        {

        }

        public bool addNearCellCores(ImageObject cellCore)
        {
            foreach(ImageObject cellCoreInList in nearCellCores)
            {
                if (cellCoreInList.Id == cellCore.Id)
                    return false; //Zellkern schon aufgenommen
            }
            nearCellCores.Add(cellCore);
            return true;
        }

        public int countNearCellCores()
        {
            return this.nearCellCores.Count;
        }
    }
}
