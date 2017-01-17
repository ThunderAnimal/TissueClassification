/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.Adapters.PicNetML {
	using System.Collections.Generic;
	using java.io;
	using java.util;
	using weka.core;
	using weka.core.converters;
	using Attribute=weka.core.Attribute;
	/// <summary>
	/// Fassade für WEKA
	/// </summary>
	public static class Util {
		/// <summary>
		/// erzeugt einen WEKA-kompatiblen Datensatz
		/// </summary>
		/// <param name="classNames">Aufzählung der Klassennamen die im Datensatz verwendet werden sollen</param>
		/// <param name="featureNames">Namen der Merkmale</param>
		/// <param name="name">Name des Datensatzes</param>
		/// <param name="classAttributeName">>Name für das Klassenattribut</param>
		/// <returns></returns>
		public static Instances CreateInstances(IEnumerable<string> classNames,IEnumerable<string> featureNames,string name="data",string classAttributeName="class"){
			var features=new ArrayList();
			foreach(var featureName in featureNames){
				var attribute=new Attribute(featureName);
				features.add(attribute);
			}
			Attribute classAttribute=null;
			if(null!=classNames){
				var classNamesList=new List<string>(classNames);
				classNamesList.Sort();
				var classValues=new ArrayList();
				foreach(var className in classNamesList) classValues.add(className);
				classAttribute=new Attribute(classAttributeName,classValues);
				features.add(classAttribute);
			}
			var dataset=new Instances(name,features,1);
			if(null!=classAttribute) dataset.setClass(classAttribute);
			return dataset;
		}
		public static Instances LoadInstancesFromWekaArff(string fileName){
			var bufferedReader=new BufferedReader(new FileReader(fileName));
			var arffReader=new ArffLoader.ArffReader(bufferedReader);
			var instances=arffReader.getData();
			bufferedReader.close();
			instances.setClassIndex(instances.numAttributes()-1);
			return instances;
		}
	}
}
