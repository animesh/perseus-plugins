﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BaseLibS.Graph;
using BaseLibS.Graph.Image;
using BaseLibS.Num;
using BaseLibS.Param;
using PerseusApi.Matrix;
using PerseusPluginLib.Filter;

namespace PerseusPluginLib.Utils{
	public static class PerseusPluginUtils{
		public static SingleChoiceParam GetFilterModeParam(bool column){
			return new SingleChoiceParam("Filter mode"){
				Values = new[]{"Reduce matrix", column ? "Add categorical column" : "Add categorical row"}
			};
		}

		private static SingleChoiceParam GetModeParam1(){
			return new SingleChoiceParam("Mode"){
				Values = new[]{"Remove matching rows", "Keep matching rows"},
				Help =
					"If 'Remove matching rows' is selected, rows having the value specified above will be removed while " +
					"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen."
			};
		}

		private static SingleChoiceParam GetModeParam2(){
			return new SingleChoiceParam("Mode"){
				Values = new[]{"Mark matching rows", "Mark non-matching rows"},
				Help =
					"If 'Mark matching rows' is selected, rows having the value specified above will be indicated with a '+' in the output column. " +
					"If 'Keep matching rows' is selected, the opposite will happen."
			};
		}

		internal static SingleChoiceWithSubParams GetFilterModeParamNew(){
			SingleChoiceWithSubParams p = new SingleChoiceWithSubParams("Filter mode"){
				Values = new[]{"Reduce matrix", "Add categorical column", "Split matrix"},
				SubParams =
					new List<Parameters>(new[]{new Parameters(GetModeParam1()), new Parameters(GetModeParam2()), new Parameters()})
			};
			return p;
		}

		public static void FilterRows(IMatrixData mdata, Parameters parameters, int[] rows){
			bool reduceMatrix = GetReduceMatrix(parameters);
			if (reduceMatrix){
				mdata.ExtractRows(rows);
			} else{
				Array.Sort(rows);
				string[][] col = new string[mdata.RowCount][];
				for (int i = 0; i < col.Length; i++){
					bool contains = Array.BinarySearch(rows, i) >= 0;
					col[i] = contains ? new[]{"Keep"} : new[]{"Discard"};
				}
				mdata.AddCategoryColumn("Filter", "", col);
			}
		}

		private static bool GetReduceMatrix(Parameters parameters){
			return parameters.GetParam<int>("Filter mode").Value == 0;
		}

		public static void FilterColumns(IMatrixData mdata, Parameters parameters, int[] cols){
			bool reduceMatrix = GetReduceMatrix(parameters);
			if (reduceMatrix){
				mdata.ExtractColumns(cols);
			} else{
				Array.Sort(cols);
				string[][] row = new string[mdata.ColumnCount][];
				for (int i = 0; i < row.Length; i++){
					bool contains = Array.BinarySearch(cols, i) >= 0;
					row[i] = contains ? new[]{"Keep"} : new[]{"Discard"};
				}
				mdata.AddCategoryRow("Filter", "", row);
			}
		}

		internal static void ReadValuesShouldBeParams(Parameters param, out FilteringMode filterMode, out double threshold,
			out double threshold2){
			ParameterWithSubParams<int> x = param.GetParamWithSubParams<int>("Values should be");
			Parameters subParams = x.GetSubParameters();
			int shouldBeIndex = x.Value;
			threshold = double.NaN;
			threshold2 = double.NaN;
			switch (shouldBeIndex){
				case 0:
					filterMode = FilteringMode.Valid;
					break;
				case 1:
					filterMode = FilteringMode.GreaterThan;
					threshold = subParams.GetParam<double>("Minimum").Value;
					break;
				case 2:
					filterMode = FilteringMode.GreaterEqualThan;
					threshold = subParams.GetParam<double>("Minimum").Value;
					break;
				case 3:
					filterMode = FilteringMode.LessThan;
					threshold = subParams.GetParam<double>("Maximum").Value;
					break;
				case 4:
					filterMode = FilteringMode.LessEqualThan;
					threshold = subParams.GetParam<double>("Maximum").Value;
					break;
				case 5:
					filterMode = FilteringMode.Between;
					threshold = subParams.GetParam<double>("Minimum").Value;
					threshold2 = subParams.GetParam<double>("Maximum").Value;
					break;
				case 6:
					filterMode = FilteringMode.Outside;
					threshold = subParams.GetParam<double>("Minimum").Value;
					threshold2 = subParams.GetParam<double>("Maximum").Value;
					break;
				default:
					throw new Exception("Should not happen.");
			}
		}

		public static SingleChoiceWithSubParams GetValuesShouldBeParam(){
			return new SingleChoiceWithSubParams("Values should be"){
				Values = new[]{"Valid", "Greater than", "Greater or equal", "Less than", "Less or equal", "Between", "Outside"},
				SubParams =
					new[]{
						new Parameters(new Parameter[0]),
						new Parameters(new Parameter[]
						{new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."}}),
						new Parameters(new Parameter[]
						{new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."}}),
						new Parameters(new Parameter[]
						{new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}}),
						new Parameters(new Parameter[]
						{new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}}),
						new Parameters(new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."},
							new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}),
						new Parameters(new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."},
							new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."})
					},
				ParamNameWidth = 50,
				TotalWidth = 731
			};
		}

		internal static bool IsValid(double data, double threshold, double threshold2, FilteringMode filterMode){
			switch (filterMode){
				case FilteringMode.Valid:
					return !double.IsNaN(data) && !double.IsNaN(data);
				case FilteringMode.GreaterThan:
					return data > threshold;
				case FilteringMode.GreaterEqualThan:
					return data >= threshold;
				case FilteringMode.LessThan:
					return data < threshold;
				case FilteringMode.LessEqualThan:
					return data <= threshold;
				case FilteringMode.Between:
					return data >= threshold && data <= threshold2;
				case FilteringMode.Outside:
					return data < threshold || data > threshold2;
			}
			throw new Exception("Never get here.");
		}

		internal static void NonzeroFilter1(bool rows, int minValids, bool percentage, IMatrixData mdata, Parameters param,
			double threshold, double threshold2, FilteringMode filterMode){
			if (rows){
				List<int> valids = new List<int>();
				for (int i = 0; i < mdata.RowCount; i++){
					int count = 0;
					for (int j = 0; j < mdata.ColumnCount; j++){
						if (IsValid(mdata.Values.Get(i, j), threshold, threshold2, filterMode)){
							count++;
						}
					}
					if (Valid(count, minValids, percentage, mdata.ColumnCount)){
						valids.Add(i);
					}
				}
				FilterRows(mdata, param, valids.ToArray());
			} else{
				List<int> valids = new List<int>();
				for (int j = 0; j < mdata.ColumnCount; j++){
					int count = 0;
					for (int i = 0; i < mdata.RowCount; i++){
						if (IsValid(mdata.Values.Get(i, j), threshold, threshold2, filterMode)){
							count++;
						}
					}
					if (Valid(count, minValids, percentage, mdata.RowCount)){
						valids.Add(j);
					}
				}
				FilterColumns(mdata, param, valids.ToArray());
			}
		}

		internal static bool Valid(int count, int minValids, bool percentage, int total){
			if (percentage){
				return count*100 >= minValids*total;
			}
			return count >= minValids;
		}

		public static Parameter GetMinValuesParam(bool rows){
			return new SingleChoiceWithSubParams("Min. valids"){
				Values = new[]{"Number", "Percentage"},
				SubParams =
					new[]{
						new Parameters(new IntParam("Min. number of values", 3){
							Help =
								"If a " + (rows ? "row" : "column") +
								" has less than the specified number of valid values it will be discarded in the output."
						}),
						new Parameters(new IntParam("Min. percentage of values", 70){
							Help =
								"If a " + (rows ? "row" : "column") +
								" has less than the specified percentage of valid values it will be discarded in the output."
						}),
					},
				ParamNameWidth = 50,
				TotalWidth = 731
			};
		}

		public static Parameter GetMinValuesParamOld(bool rows){
			return new IntParam("Min. number of values", 3){
				Help =
					"If a " + (rows ? "row" : "column") +
					" has less than the specified number of valid values it will be discarded in the output."
			};
		}

		public static int GetMinValids(Parameters param, out bool percentage){
			ParameterWithSubParams<int> p = param.GetParamWithSubParams<int>("Min. valids");
			percentage = p.Value == 1;
			return p.GetSubParameters().GetParam<int>(percentage ? "Min. percentage of values" : "Min. number of values").Value;
		}

		public static string[][] CollapseCatCol(string[][] catCol, int[][] collapse){
			string[][] result = new string[collapse.Length][];
			for (int i = 0; i < collapse.Length; i++){
				result[i] = CollapseCatCol(catCol, collapse[i]);
			}
			return result;
		}

		private static string[] CollapseCatCol(IList<string[]> catCol, IEnumerable<int> collapse){
			HashSet<string> all = new HashSet<string>();
			foreach (int x in collapse){
				all.UnionWith(catCol[x]);
			}
			string[] y = ArrayUtils.ToArray(all);
			Array.Sort(y);
			return y;
		}

		public static float[] CollapseNumCol(float[] numCol, int[][] collapse){
			float[] result = new float[collapse.Length];
			for (int i = 0; i < collapse.Length; i++){
				result[i] = CollapseNumCol(numCol, collapse[i]);
			}
			return result;
		}

		private static float CollapseNumCol(IList<float> numCol, IEnumerable<int> collapse){
			List<float> all = new List<float>();
			foreach (int x in collapse){
				if (!float.IsNaN(numCol[x]) && !float.IsInfinity(numCol[x])){
					all.Add(numCol[x]);
				}
			}
			float y = ArrayUtils.Median(all.ToArray());
			return y;
		}

		public static double[] CollapseNumCol(double[] numCol, int[][] collapse){
			double[] result = new double[collapse.Length];
			for (int i = 0; i < collapse.Length; i++){
				result[i] = CollapseNumCol(numCol, collapse[i]);
			}
			return result;
		}

		private static double CollapseNumCol(IList<double> numCol, IEnumerable<int> collapse){
			List<double> all = new List<double>();
			foreach (int x in collapse){
				if (!double.IsNaN(numCol[x]) && !double.IsInfinity(numCol[x])){
					all.Add(numCol[x]);
				}
			}
			double y = ArrayUtils.Median(all.ToArray());
			return y;
		}

		public static int[][] GetMainColIndices(IList<string[]> groupCol, string[] groupNames){
			int[][] colInds = new int[groupNames.Length][];
			for (int i = 0; i < colInds.Length; i++){
				colInds[i] = GetMainColIndices(groupCol, groupNames[i]);
			}
			return colInds;
		}

		private static int[] GetMainColIndices(IList<string[]> groupCol, string groupName){
			List<int> result = new List<int>();
			for (int i = 0; i < groupCol.Count; i++){
				string[] w = groupCol[i];
				Array.Sort(w);
				if (Array.BinarySearch(w, groupName) >= 0){
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOfCol(IMatrixData data, string categoryName, string value){
			int index = GetIndexOfCol(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.ColumnCount; i++){
				string[] s = data.GetCategoryRowEntryAt(index, i);
				foreach (string s1 in s){
					if (s1.Equals(value)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOfCol(IMatrixData data, string categoryName, HashSet<string> values){
			int index = GetIndexOfCol(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.ColumnCount; i++){
				string[] s = data.GetCategoryRowEntryAt(index, i);
				foreach (string s1 in s){
					if (values.Contains(s1)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOf(IMatrixData data, string categoryName, string value){
			int index = GetIndexOf(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.RowCount; i++){
				string[] s = data.GetCategoryColumnEntryAt(index, i);
				foreach (string s1 in s){
					if (s1.Equals(value)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOf(IMatrixData data, string categoryName, HashSet<string> values){
			int index = GetIndexOf(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.RowCount; i++){
				string[] s = data.GetCategoryColumnEntryAt(index, i);
				foreach (string s1 in s){
					if (values.Contains(s1)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int GetIndexOf(IMatrixData data, string categoryName){
			for (int i = 0; i < data.CategoryColumnNames.Count; i++){
				if (data.CategoryColumnNames[i].Equals(categoryName)){
					return i;
				}
			}
			return -1;
		}

		public static int GetIndexOfCol(IMatrixData data, string categoryName){
			for (int i = 0; i < data.CategoryRowNames.Count; i++){
				if (data.CategoryRowNames[i].Equals(categoryName)){
					return i;
				}
			}
			return -1;
		}

		public static List<string[][]> GetCategoryColumns(IMatrixData mdata, IList<int> inds){
			List<string[][]> result = new List<string[][]>();
			foreach (int ind in inds){
				result.Add(mdata.GetCategoryColumnAt(ind));
			}
			return result;
		}

		public static List<string[][]> GetCategoryColumns(IMatrixData mdata){
			List<string[][]> result = new List<string[][]>();
			for (int index = 0; index < mdata.CategoryColumnCount; index++){
				result.Add(mdata.GetCategoryColumnAt(index));
			}
			return result;
		}

		public static List<string[][]> GetCategoryRows(IMatrixData mdata, IList<int> inds){
			List<string[][]> result = new List<string[][]>();
			foreach (int ind in inds){
				result.Add(mdata.GetCategoryRowAt(ind));
			}
			return result;
		}

		public static List<string[][]> GetCategoryRows(IMatrixData mdata){
			List<string[][]> result = new List<string[][]>();
			for (int index = 0; index < mdata.CategoryRowCount; index++){
				result.Add(mdata.GetCategoryRowAt(index));
			}
			return result;
		}

		public static string[][] CalcPvalueSignificance(double[] pvals, double threshold){
			string[][] result = new string[pvals.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = pvals[i] <= threshold ? new[]{"+"} : new string[0];
			}
			return result;
		}

		public static string[][] CalcBenjaminiHochbergFdr(double[] pvals, double threshold, int n, out double[] fdrs){
			fdrs = new double[pvals.Length];
			int[] o = ArrayUtils.Order(pvals);
			int lastind = -1;
			for (int i = 0; i < n; i++){
				double fdr = Math.Min(1, pvals[o[i]]*n/(1.0 + i));
				fdrs[o[i]] = fdr;
				if (fdr <= threshold){
					lastind = i;
				}
			}
			string[][] result = new string[pvals.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = new string[0];
			}
			for (int i = 0; i <= lastind; i++){
				result[o[i]] = new[]{"+"};
			}
			return result;
		}

		public static Bitmap2 GetImage(string file){
			Assembly thisExe = Assembly.GetExecutingAssembly();
			Stream file1 = thisExe.GetManifestResourceStream("PerseusPluginLib.img." + file);
			if (file1 == null){
				return null;
			}
			Bitmap2 bm = Image2.ReadImage(file1);
			file1.Close();
			return bm;
		}
	}
}