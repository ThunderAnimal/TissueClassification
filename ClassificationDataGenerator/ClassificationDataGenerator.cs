using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Glaukopis.SharpAccessoryIntegration;
using Glaukopis.SlideProcessing;
using Segmentation;
using SharpAccessory.Imaging.Filters;
using SharpAccessory.Threading;
using VMscope.InteropCore.VirtualMicroscopy;

namespace ClassificatioDataGenerator
{
    internal static class ClassificationDataGenerator
    {
        /// <summary>
        /// Config Variablen
        /// </summary>
        private static bool isSave = false; //Save Pictures
        private static int abort = 0; //Abort after x Anotions for each Dataset 0- disable

        private static void Main(string[] args)
        {
            //Eingabe Parameter
            String srcSlides = args[0];
            String outCsv = args[1];
            String outputPics = args[2];

            //Start der Verarbeitung
            var startProcTime = DateTime.Now;
            Console.WriteLine("Start: " + startProcTime);
            Console.WriteLine("Verarbeite Daten...");

            //Nötige Ordner ertsellen 
            if (!Directory.Exists(outputPics))
                Directory.CreateDirectory(outputPics);
            if (!Directory.Exists(outCsv))
                Directory.CreateDirectory(outCsv);

            //Annotaionen und Histowerte holen und in liste sammeln 
            var annotationList = GetAnnotaions(srcSlides, outputPics);

            //Liste anpassen
            annotationList.cleanUp();
            annotationList.sort();

            //Ergebnisse in CSV schreiben
            Console.WriteLine("\nSpeicher Daten...");
            annotationList.writeToCsv(outCsv + @"\data.csv");
            annotationList.groubByOnClassAndOthers("Fettgewebe").writeToCsv(outCsv + @"\dataFettgeweebe.csv");
            annotationList.groubByOnClassAndOthers("Tumor").writeToCsv(outCsv + @"\dataTumor.csv");
            annotationList.groubByOnClassAndOthers("Entzündung").writeToCsv(outCsv + @"\dataEntzuendung.csv");
            annotationList.groubByOnClassAndOthers("Gefäß").writeToCsv(outCsv + @"\dataGefaess.csv");
            annotationList.groubByOnClassAndOthers("Mikrokalk").writeToCsv(outCsv + @"\dataMikrokalk.csv");
            annotationList.groubByOnClassAndOthers("Nerv").writeToCsv(outCsv + @"\dataNerv.csv");
            annotationList.groubByOnClassAndOthers("Stroma").writeToCsv(outCsv + @"\dataStroma.csv");
            annotationList.groubByOnClassAndOthers("DCIC").writeToCsv(outCsv + @"\dataDCIC.csv");
            annotationList.groubByOnClassAndOthers("Kalk").writeToCsv(outCsv + @"\dataKalk.csv");
            annotationList.groubByOnClassAndOthers("Normales Mammaepithel").writeToCsv(outCsv + @"\dataNormalesMammaepithel.csv");
            Console.WriteLine("Results saved to:  {0} ", outCsv);

            //Ender der Verarbeitung
            var endProcTime = DateTime.Now;
            Console.WriteLine("\n############ FINISHED ###############");
            Console.WriteLine("Ende: " + endProcTime);

            //FINISH
            Console.Write("Benötigte Zeit: {0:F} Minuten", endProcTime.Subtract(startProcTime).TotalMinutes);
            Console.ReadKey();
        }
        
        private static TissueAnnotaionList GetAnnotaions(String srcSlides, String outputPics)
        {
            TissueAnnotaionList annotationList = new TissueAnnotaionList();
            int i = 0;
            //Get Anotaions from silde
            foreach (var slideName in Util.GetSlideFilenames(new String[] { srcSlides }))
            {
                using (var slideCache = new SlideCache(slideName))
                {
                    
                    Parallel.ForEach(slideCache.Slide.GetAnnotations(), (annotation) =>
                    {
                        if (abort != 0)
                        {
                            if (i >= abort)
                            {
                                return;
                            }
                            i++;
                        }

                        //Annotaions Bitmap extrahieren
                        var contained = new List<IAnnotation>();
                        foreach (var candidate in slideCache.Slide.GetAnnotations())
                        {
                            if (candidate == annotation) continue;
                            var rectangle = annotation.BoundingBox;
                            rectangle.Intersect(candidate.BoundingBox);
                            if (rectangle.IsEmpty) continue;
                            if (annotation.BoundingBox.Contains(candidate.BoundingBox)) contained.Add(candidate);
                        }

                        using (Bitmap annotationBitmap = annotation.Extract(1, contained))
                        {

                            TissueAnnotationClass tissueAnnotation = new TissueAnnotationClass(annotation.Id, annotation.Name, slideCache.SlideName);

                            //Werte Berechnen
                            tissueAnnotation = tissueAnnotation.ComputeFeatureValues(annotationBitmap);

                            //Zur Liste hinzufügren
                            annotationList.add(tissueAnnotation);

                            //FOR DEBUG Save Image
                            if (isSave)
                            {
                                annotationBitmap.Save(outputPics + "\\" + tissueAnnotation + ".png");
                            }

                            Console.WriteLine(tissueAnnotation + " exc:" + contained.Count);
                        }
                    });
                    if(abort != 0 && i >= abort)
                        return annotationList;
                    }
            }
            return annotationList;
        }
    }
}
