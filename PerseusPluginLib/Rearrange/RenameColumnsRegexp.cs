using System;
using System.Linq;
using System.Text.RegularExpressions;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class RenameColumnsRegexp : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap2 DisplayImage => null;

		public string Description
			=> "Rename expression columns with the help of matching part of the name by a regular expression.";

		public string HelpOutput => "";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Rename columns [reg. ex.]";
		public string Heading => "Rearrange";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;

		public string Url
			=> "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Rearrange:RenameColumnsRegexp";

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
            ref IDocumentData[] documents, ProcessInfo processInfo)
        {
            var parameter = param.GetParamWithSubParams<int>("Column type");
			Tuple<Regex, string> vals = parameter.GetSubParameters().GetParam<Tuple<Regex, string>>("Regex").Value;
		    Regex pattern = vals.Item1;
		    string replacementStr = vals.Item2;
            switch (parameter.Value)
            {
                case 0:
                    for (int i = 0; i < mdata.ColumnCount; i++)
                    {
                        mdata.ColumnNames[i] = pattern.Replace(mdata.ColumnNames[i], replacementStr);
                    }
                    break;
                case 1:
                    for (int i = 0; i < mdata.StringColumnCount; i++)
                    {
                        mdata.StringColumnNames[i] = pattern.Replace(mdata.StringColumnNames[i], replacementStr);
                    }
                    break;
                case 2:
                    for (int i = 0; i < mdata.NumericColumnCount; i++)
                    {
                        mdata.NumericColumnNames[i] = pattern.Replace(mdata.NumericColumnNames[i], replacementStr);
                    }
                    break;
                case 3:
                    for (int i = 0; i < mdata.CategoryColumnCount; i++)
                    {
                        mdata.CategoryColumnNames[i] = pattern.Replace(mdata.CategoryColumnNames[i], replacementStr);
                    }
                    break;
                case 4:
                    for (int i = 0; i < mdata.CategoryColumnCount; i++)
                    {
                        mdata.MultiNumericColumnNames[i] = pattern.Replace(mdata.MultiNumericColumnNames[i], replacementStr);
                    }
                    break;
            }
	        if (mdata.ColumnNames.Count > mdata.ColumnNames.Distinct().Count()
	            || mdata.StringColumnNames.Count > mdata.StringColumnNames.Distinct().Count()
	            || mdata.NumericColumnNames.Count > mdata.NumericColumnNames.Distinct().Count()
	            || mdata.CategoryColumnNames.Count > mdata.CategoryColumnNames.Distinct().Count()
	            || mdata.MultiNumericColumnNames.Count > mdata.MultiNumericColumnNames.Distinct().Count())
	        {
		        processInfo.ErrString = "Column naming not unique. Please change the pattern accordingly.";
	        }
		}

        public Parameters GetParameters(IMatrixData mdata, ref string errorString)
        {
            var parameter = new SingleChoiceWithSubParams("Column type")
            {
                Values = new [] {"Main", "Text", "Numeric", "Category", "Multi numeric"},
                SubParams = new []
                {
                new RegexReplaceParam("Regex", new Regex("Column (.*)"), "$1", mdata.ColumnNames),
                new RegexReplaceParam("Regex", new Regex("Column (.*)"), "$1", mdata.StringColumnNames),
                new RegexReplaceParam("Regex", new Regex("Column (.*)"), "$1", mdata.NumericColumnNames),
                new RegexReplaceParam("Regex", new Regex("Column (.*)"), "$1", mdata.CategoryColumnNames),
                new RegexReplaceParam("Regex", new Regex("Column (.*)"), "$1", mdata.MultiNumericColumnNames),
                }.Select(param => new Parameters(param)).ToArray()
            };
            return new Parameters(parameter);
        }
	}
}