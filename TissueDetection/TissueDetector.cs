/*
 * @author Sebastian Lohmann
 */
namespace TissueDetection{
	using System;
	using System.Drawing;
	using System.Linq;
    using System.Collections;
	using Glaukopis.SlideProcessing;
	using SharpAccessory.Imaging.Processors;
	internal class TissueDetector{
		private static void Main(string[] args){
            const int COLOR_WHITE = 200;
            const double IS_WHITE = 0.8;

            foreach (var slideName in Util.GetSlideFilenames(new string[] { args[0] })){
				using(var slideCache=new SlideCache(slideName)){
                    // scale=1 -> baselayer , sollte so klein wie möglich sein um die Rechenzeit zu minimieren
                    // targetSize Größe der zu prozessierenden Bilder, hängt von der hardware ab und bestimmt die Auflösung der Heatmap, für niedrige scale-Werte sollte auch die Größe reduziert werden
                    // Auflösung: Breite=tissueSlidePartitioner.Columns Höhe=tissueSlidePartitioner.Rows
                    // var: implizit typisiert, tatsächlich stark typisiert da der Compiler den Typ kennt; RMT->Goto To Definition
                    // Empfehlung: http://shop.oreilly.com/product/0636920040323.do die 5.0 gibt es auch als pdf im Internet
                    var tissueSlidePartitioner=new SlidePartitioner<bool>(slideCache.Slide,0.5f,new Size(500,500)); //Nicht unbedingt auf dem Baselayerr arbeiten zB. 1f --> 0.1
					using(var overViewImage=slideCache.Slide.GetImagePart(0,0,slideCache.Slide.Size.Width,slideCache.Slide.Size.Height,tissueSlidePartitioner.Columns,tissueSlidePartitioner.Rows)){
						//TODO falls die Gewebeerkennung auf dem Übersichtsbild stattfinden soll, dann hier

						slideCache.SetImage("overview",overViewImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\...
					}
					foreach(var tile in tissueSlidePartitioner.Values){
						using(var tileImage=slideCache.Slide.GetImagePart(tile)){
							var containsTissue=false;
							#region hier sollte containsTissue richtig bestimmt werden
							var bitmapProcessor=new BitmapProcessor(tileImage);//liefert schnelleren Zugriff auf die Pixel-Werte, alternativ auch SharpAccessory.Imaging.Processors.GrayscaleProcessor

                            ///eventuel Farbtiefe ermitteln
                            int countWhite = 0;
                            int countNonWhite = 0;
                            bool isWhite = false;

                            for (var y = 0; y < bitmapProcessor.Height; y++)
                            {
                                for (var x = 0; x < bitmapProcessor.Width; x++)
                                {                                  
                                    var r = bitmapProcessor.GetRed(x, y);
                                    var g = bitmapProcessor.GetGreen(x, y);
                                    var b = bitmapProcessor.GetBlue(x, y);

                                    var grauwert =  (int)(r + g + b)/3;
                                    if (grauwert > COLOR_WHITE)
                                        countWhite++;
                                    else
                                        countNonWhite++;

                                }
                            }
                            //Calculate Imnage is White or not
                            isWhite = (countWhite / (countWhite + countNonWhite)) >= IS_WHITE;
                                                        
							bitmapProcessor.Dispose();
							if(0==tile.Index.Y) containsTissue=false;//oberste Reihe sollte kein Gewebe enthalten
							//if(slideCache.Slide.GetAnnotationsInArea(tile.SourceArea).Any()) containsTissue=true;//Kacheln mit Annotationen enthalten Gewebe
                            if (!isWhite) containsTissue = true;
                            #endregion

                            tile.Data = containsTissue;//Wert zur Kachel speichern

                            //Only for Debug
                            var saveImage =true;
							if(saveImage){
                                string path = args[1] + @"\" + slideCache.SlideName + @"\";
                                if (containsTissue)
                                    path += @"isTissue\";
                                else
                                    path += @"noTissue\";

                                tileImage.Save(path + tile.Index.X + "-" + tile.Index.Y + ".png");
                                //var tileCache = slideCache.GetTileCache(tile.Index);
                                //tileCache.SetImage("rgb",tileImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\[Index]\... ansonsten tileImage.Save("[uri].png")
							}
                            //
                            Console.WriteLine(slideCache.SlideName+"-"+tile.Index+" done - containsTissue: " + containsTissue.ToString());
						}
					}
                    //true wird zu grün, false zu rot; syntax ist lambda (=>) mit einem conditional operator (?)
                    Func<bool, Color> f = b =>
                    {
                        if (b) return Color.Red;
                        else return Color.Green;
                    };
                    //kann auch Funcktion f verwendet werden
					using(var heatMap=tissueSlidePartitioner.GenerateHeatMap(b=>b?Color.Red:Color.Green)) slideCache.SetImage("tissueHeatMap",heatMap);
				}
			}
		}
	}
}