using System.Text.RegularExpressions;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class ProcessTextColumns : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap2 DisplayImage => null;
		public string Description => "Values in string columns can be manipulated according to a regular expression.";
		public string HelpOutput => "";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Process text column";
		public string Heading => "Rearrange";
		public bool IsActive => true;
		public float DisplayRank => 22;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;

		public string Url
			=> "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Rearrange:ProcessTextColumns";

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string regexStr = param.GetParam<string>("Regular expression").Value;
			Regex regex = new Regex(regexStr);
			int[] inds = param.GetParam<int[]>("Columns").Value;
			bool keepColumns = param.GetParam<bool>("Keep original columns").Value;
			bool semicolons = param.GetParam<bool>("Strings separated by semicolons are independent").Value;
		    string replacement = param.GetParam<string>("Replacement string").Value;
			foreach (int col in inds){
				ProcessColMatch(mdata, regex, replacement, col, keepColumns, semicolons);
			}
		}

		private static void ProcessColMatch(IDataWithAnnotationColumns mdata, Regex regex, string replacement, int col, bool keepColumns, bool semicolons){
			string[] values = new string[mdata.RowCount];
			for (int row = 0; row < mdata.RowCount; row++){
				string fullString = mdata.StringColumns[col][row];
				string[] inputParts = semicolons ? fullString.Split(';') : new[]{fullString};
			    values[row] = RegexMatchOrReplace(inputParts[0]);
				for (int i = 1; i < inputParts.Length; i++)
				{
				    values[row] += ";" + RegexMatchOrReplace(inputParts[i]);
				}
			}
			if (keepColumns){
				mdata.AddStringColumn(mdata.StringColumnNames[col], null, values);
			} else{
				mdata.StringColumns[col] = values;
			}

		    string RegexMatchOrReplace(string input)
		    {
		        if (string.IsNullOrEmpty(replacement))
		        {
		            return regex.Match(input).Groups[1].ToString();
		        }
		        return regex.Replace(input, replacement);
		    }
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
                new Parameters(new MultiChoiceParam("Columns")
                {
                    Values = mdata.StringColumnNames
                }, 
                new StringParam("Regular expression", "^([^;]+)"),
                new StringParam("Replacement string", "")
                {
                    Help = "Replacement string. Leave empty for matching."
                },
                new BoolParam("Keep original columns", false),
                new BoolParam("Strings separated by semicolons are independent", false));
        }
    }
}