﻿using BasicLib.Param;

namespace PerseusApi{
	public interface IUpload : IActivity{
		/// <summary>
		/// Define here the parameters that determine the specifics of the upload.
		/// </summary>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(ref string errString);
	}
}