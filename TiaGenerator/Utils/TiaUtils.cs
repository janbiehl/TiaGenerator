namespace TiaGenerator.Utils
{
	public static class TiaUtils
	{
		/// <summary>
		/// Get the block groups from a block group path
		/// </summary>
		/// <param name="blockGroupPath">The path for the block group</param>
		/// <param name="separator">The separator used for separating the single parts</param>
		/// <returns>The separated block groups in the order of the input</returns>
		public static string[] GetBlockGroups(string blockGroupPath, char separator = '/')
		{
			return blockGroupPath.Split(separator);
		}
	}
}