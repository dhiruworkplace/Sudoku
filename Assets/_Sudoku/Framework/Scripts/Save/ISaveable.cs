using System.Collections.Generic;

namespace ArtboxGames
{
	public interface ISaveable
	{
		string	SaveId		{ get; }
		bool	ShouldSave	{ get; set; }

		Dictionary<string, object> Save();
	}
}
