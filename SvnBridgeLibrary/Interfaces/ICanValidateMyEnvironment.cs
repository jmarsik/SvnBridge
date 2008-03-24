namespace SvnBridge.Interfaces
{
	/// <summary>
	/// Implementors of this interface will be called on startup to validate the 
	/// operation environment.
	/// </summary>
	public interface ICanValidateMyEnvironment
	{
		/// <summary>
		/// Validates the environment, should throw an exception 
		/// if the environment is not set up correctly
		/// </summary>
		void ValidateEnvironment();
	}
}