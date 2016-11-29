using System;
using System.Drawing;
using System.Linq;
using System.Collections;
using Glaukopis.SlideProcessing;
using SharpAccessory.Imaging.Processors;
using System.Threading.Tasks;
using System.IO;
using Glaukopis.SlideProcessing;
using VMscope.InteropCore.VirtualMicroscopy;

namespace HistoGenerator
{
    internal static class HistoGenerator
    {
        private static void Main(string[] args)
        {
            TissueAnnotaionList annotationList;

            annotationList = getAnnotaions();

            //TODO --> In CSV Datei schreiben

            Console.WriteLine("############ FINISHED ###############");
            Console.ReadKey();
        }
        
        private static TissueAnnotaionList getAnnotaions()
        {
            TissueAnnotaionList annotationList = new TissueAnnotaionList();

            //TODO --> anotaion raus holen (siehe AnotaionExtraktor)

            //TODO --> die H und E werte von Annotaions berechen (siehe TileProcessor)

            //TODO --> Annotaions List füllen


            return annotationList;
        }

    }
}
