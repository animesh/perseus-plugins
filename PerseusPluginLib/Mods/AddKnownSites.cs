using System;
using System.Collections.Generic;
using BaseLibS.Graph;
using BaseLibS.Num;
using BaseLibS.Param;
using BaseLibS.Parse.Misc;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Mods{
	public class AddKnownSites : IMatrixProcessing{
		public bool HasButton => false;

		public string Url
			=> "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Modifications:AddKnownSites";

		public Bitmap2 DisplayImage => null;
		public string Description => "Sites that are known in PhosphoSitePlus are indicated.";
		public string HelpOutput => "";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Add known sites";
		public string Heading => "Modifications";
		public bool IsActive => true;
		public float DisplayRank => 5;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<string> colChoice = mdata.StringColumnNames;
			int colInd = 0;
			for (int i = 0; i < colChoice.Count; i++){
				if (colChoice[i].ToUpper().Equals("UNIPROT")){
					colInd = i;
					break;
				}
			}
			int colSeqInd = 0;
			for (int i = 0; i < colChoice.Count; i++){
				if (colChoice[i].ToUpper().Equals("SEQUENCE WINDOW")){
					colSeqInd = i;
					break;
				}
			}
			string[] choice = PhosphoSitePlusParser.GetAllMods();
			int ind = ArrayUtils.IndexOf(choice, "Phosphorylation");
			return
				new Parameters(
					new SingleChoiceParam("Modification"){
						Value = ind,
						Values = choice,
						Help = "Select here the kind of modification for which information should be added."
					},
					new SingleChoiceParam("Uniprot column"){
						Value = colInd,
						Help = "Specify here the column that contains Uniprot identifiers.",
						Values = colChoice
					},
					new SingleChoiceParam("Sequence window"){
						Value = colSeqInd,
						Help = "Specify here the column that contains the sequence windows around the site.",
						Values = colChoice
					});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string mod = param.GetParam<int>("Modification").StringValue;
			string[] seqWins;
			string[] accs;
			string[] pubmedLtp;
			string[] pubmedMs2;
			string[] cstMs2;
			string[] species;
			PhosphoSitePlusParser.ParseKnownMod(mod, out seqWins, out accs, out pubmedLtp, out pubmedMs2, out cstMs2, out species);
			if (seqWins == null){
				processInfo.ErrString = "File does not exist.";
				return;
			}
			string[] up = mdata.StringColumns[param.GetParam<int>("Uniprot column").Value];
			string[][] uprot = new string[up.Length][];
			for (int i = 0; i < up.Length; i++){
				uprot[i] = up[i].Length > 0 ? up[i].Split(';') : new string[0];
			}
			string[] win = mdata.StringColumns[param.GetParam<int>("Sequence window").Value];
			Dictionary<string, List<int>> map = new Dictionary<string, List<int>>();
			for (int i = 0; i < seqWins.Length; i++){
				string acc = accs[i];
				if (!map.ContainsKey(acc)){
					map.Add(acc, new List<int>());
				}
				map[acc].Add(i);
			}
			string[] newCol = new string[uprot.Length];
			string[][] newCatCol = new string[uprot.Length][];
			string[][] originCol = new string[uprot.Length][];
			for (int i = 0; i < newCol.Length; i++){
				string[] win1 = TransformIl(win[i]).Split(';');
				HashSet<string> wins = new HashSet<string>();
				HashSet<string> origins = new HashSet<string>();
				foreach (string ux in uprot[i]){
					if (map.ContainsKey(ux)){
						List<int> n = map[ux];
						foreach (int ind in n){
							string s = seqWins[ind];
							if (Contains(win1, TransformIl(s.ToUpper().Substring(1, s.Length - 2)))){
								wins.Add(s);
								if (pubmedLtp[ind].Length > 0){
									origins.Add("LTP");
								}
								if (pubmedMs2[ind].Length > 0){
									origins.Add("HTP");
								}
								if (cstMs2[ind].Length > 0){
									origins.Add("CST");
								}
							}
						}
					}
				}
				if (wins.Count > 0){
					newCol[i] = StringUtils.Concat(";", ArrayUtils.ToArray(wins));
					newCatCol[i] = new[]{"+"};
					string[] x = ArrayUtils.ToArray(origins);
					Array.Sort(x);
					originCol[i] = x;
				} else{
					newCol[i] = "";
					newCatCol[i] = new string[0];
					originCol[i] = new string[0];
				}
			}
			mdata.AddStringColumn("PhosphoSitePlus window", "", newCol);
			mdata.AddCategoryColumn("Known site", "", newCatCol);
			mdata.AddCategoryColumn("Origin", "", originCol);
		}

		public static void ParseKnownSites(string filename){}

		public static bool Contains(IEnumerable<string> wins, string x){
			foreach (string win in wins){
				if (Contains(win, x)){
					return true;
				}
			}
			return false;
		}

		private static bool Contains(string wins, string s){
			if (wins.Length == s.Length){
				return wins.Equals(s);
			}
			return wins.Length > s.Length ? CenterEquals(wins, s) : CenterEquals(s, wins);
		}

		private static bool CenterEquals(string wins, string s){
			int offset = wins.Length/2 - s.Length/2;
			return wins.Substring(offset, wins.Length - 2*offset).Equals(s);
		}

		public static string TransformIl(string p0){
			List<char> result = new List<char>();
			foreach (char c in p0){
				result.Add(c == 'L' ? 'I' : c);
			}
			return new string(result.ToArray());
		}
	}
}