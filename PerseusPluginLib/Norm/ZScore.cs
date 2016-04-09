using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLibS.Num;
using BaseLibS.Param;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Norm{
	public class ZScore : IMatrixProcessing{
		public bool HasButton{
			get { return true; }
		}

		public Bitmap DisplayImage{
			get { return Resources.zscoreButton_Image; }
		}

		public string Name{
			get { return "Z-score"; }
		}

		public string Heading{
			get { return "Normalization"; }
		}

		public bool IsActive{
			get { return true; }
		}

		public float DisplayRank{
			get { return -10; }
		}

		public string HelpOutput{
			get { return "Normalized expression matrix."; }
		}

		public string[] HelpSupplTables{
			get { return new string[0]; }
		}

		public int NumSupplTables{
			get { return 0; }
		}

		public string[] HelpDocuments{
			get { return new string[0]; }
		}

		public int NumDocuments{
			get { return 0; }
		}

		public string Url{
			get { return "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Normalization:ZScore"; }
		}

		public string Description{
			get{
				return
					"The mean of each row/column is subtracted from each value. The result is then divided by the standard deviation of the row/column.";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return int.MaxValue;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			Parameters rowParams =
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Grouping"){
						Values = ArrayUtils.Concat(new[]{"<No grouping>"}, mdata.CategoryRowNames),
						Help = "The z-scoring will be done separately in groups if a grouping is specified here."
					}
				});
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Matrix access"){
						Values = new[]{"Rows", "Columns"},
						ParamNameWidth = 136,
						TotalWidth = 731,
						SubParams = new[]{rowParams, new Parameters()},
						Help = "Specifies if the z-scoring is performed on the rows or the columns of the matrix."
					},
					new BoolParam("Use median"), new BoolParam("Report mean and std. dev.")
				});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			ParameterWithSubParams<int> access = param.GetParamWithSubParams<int>("Matrix access");
			bool rows = access.Value == 0;
			int groupInd;
			if (rows){
				groupInd = access.GetSubParameters().GetParam<int>("Grouping").Value - 1;
			} else{
				groupInd = -1;
			}
			bool report = param.GetParam<bool>("Report mean and std. dev.").Value;
			bool median = param.GetParam<bool>("Use median").Value;
			if (groupInd < 0){
				double[] means;
				double[] stddevs;
				Zscore(rows, mdata, processInfo.NumThreads, report, median, out means, out stddevs);
				if (report){
					if (rows){
						mdata.AddNumericColumn("Mean", "Mean", means);
						mdata.AddNumericColumn("Std. dev.", "Std. dev.", stddevs);
					} else{
						mdata.AddNumericRow("Mean", "Mean", means);
						mdata.AddNumericRow("Std. dev.", "Std. dev.", stddevs);
					}
				}
			} else{
				string[][] catRow = mdata.GetCategoryRowAt(groupInd);
				foreach (string[] t in catRow){
					if (t.Length > 1){
						processInfo.ErrString = "The groups are overlapping.";
						return;
					}
				}
				string[] groupVals = ArrayUtils.UniqueValuesPreserveOrder(catRow);
				double[][] means;
				double[][] stddevs;
				ZscoreGroups(mdata, catRow, processInfo.NumThreads, report, median, groupVals, out means, out stddevs);
				if (report){
					for (int i = 0; i < groupVals.Length; i++){
						mdata.AddNumericColumn("Mean " + groupVals[i], "Mean", means[i]);
						mdata.AddNumericColumn("Std. dev. " + groupVals[i], "Std. dev.", stddevs[i]);
					}
				}
			}
		}

		private static void ZscoreGroups(IMatrixData data, IList<string[]> catRow, int nthreads, bool report, bool median,
			IList<string> groupVals, out double[][] means, out double[][] stddevs){
			means = new double[groupVals.Count][];
			stddevs = new double[groupVals.Count][];
			for (int index = 0; index < groupVals.Count; index++){
				string groupVal = groupVals[index];
				int[] inds = GetIndices(catRow, groupVal);
				ZscoreGroup(data, inds, nthreads, report, median, out means[index], out stddevs[index]);
			}
		}

		private static void ZscoreGroup(IMatrixData data, IList<int> inds, int nthreads, bool report, bool median,
			out double[] means, out double[] stddevs){
			means = null;
			stddevs = null;
			if (report){
				means = new double[data.RowCount];
				stddevs = new double[data.RowCount];
			}
			double[] doubles = means;
			double[] stddevs1 = stddevs;
			new ThreadDistributor(nthreads, data.RowCount, i => Calc3(i, data, inds, doubles, stddevs1, report, median)).Start();
		}

		private static void Calc3(int i, IMatrixData data, IList<int> inds, IList<double> means, IList<double> stddevs,
			bool report, bool median){
			double[] vals = new double[inds.Count];
			for (int j = 0; j < inds.Count; j++){
				double q = data.Values[i, inds[j]];
				vals[j] = q;
			}
			double stddev;
			double mean = ArrayUtils.MeanAndStddev(vals, out stddev, median);
			foreach (int t in inds){
				data.Values[i, t] = (float) ((data.Values[i, t] - mean)/stddev);
			}
			if (report){
				means[i] = mean;
				stddevs[i] = stddev;
			}
		}

		internal static int[] GetIndices(IList<string[]> catRow, string groupVal){
			List<int> result = new List<int>();
			for (int i = 0; i < catRow.Count; i++){
				Array.Sort(catRow[i]);
				if (Array.BinarySearch(catRow[i], groupVal) >= 0){
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		private static void Zscore(bool rows, IMatrixData data, int nthreads, bool report, bool median, out double[] means,
			out double[] stddevs){
			means = null;
			stddevs = null;
			if (rows){
				if (report){
					means = new double[data.RowCount];
					stddevs = new double[data.RowCount];
				}
				double[] doubles = means;
				double[] stddevs1 = stddevs;
				new ThreadDistributor(nthreads, data.RowCount, i => Calc1(i, data, doubles, stddevs1, report, median)).Start();
			} else{
				if (report){
					means = new double[data.ColumnCount];
					stddevs = new double[data.ColumnCount];
				}
				double[] doubles = means;
				double[] stddevs1 = stddevs;
				new ThreadDistributor(nthreads, data.ColumnCount, j => Calc2(j, data, doubles, stddevs1, report, median)).Start();
			}
		}

		private static void Calc1(int i, IMatrixData data, IList<double> means, IList<double> stddevs, bool report,
			bool median){
			double[] vals = new double[data.ColumnCount];
			for (int j = 0; j < data.ColumnCount; j++){
				vals[j] = data.Values[i, j];
			}
			double stddev;
			double mean = ArrayUtils.MeanAndStddev(vals, out stddev, median);
			for (int j = 0; j < data.ColumnCount; j++){
				data.Values[i, j] = (float) ((data.Values[i, j] - mean)/stddev);
			}
			if (report){
				means[i] = mean;
				stddevs[i] = stddev;
			}
		}

		private static void Calc2(int j, IMatrixData data, IList<double> means, IList<double> stddevs, bool report,
			bool median){
			double[] vals = new double[data.RowCount];
			for (int i = 0; i < data.RowCount; i++){
				vals[i] = data.Values[i, j];
			}
			double stddev;
			double mean = ArrayUtils.MeanAndStddev(vals, out stddev, median);
			for (int i = 0; i < data.RowCount; i++){
				data.Values[i, j] = (float) ((data.Values[i, j] - mean)/stddev);
			}
			if (report){
				means[j] = mean;
				stddevs[j] = stddev;
			}
		}
	}
}