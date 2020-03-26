/// <summary>
/// Collection of game statistics
/// </summary>
public class GameStatistics
{
	#region Public Properties

	/// <summary>
	/// Total achieved kills
	/// </summary>
	public int TotalKills { get; private set; }

	/// <summary>
	/// Total deaths
	/// </summary>
	public int TotalDeaths => TotalPlayerDeaths + TotalEnviromentalDeaths;

	/// <summary>
	/// Temporary count for totla deaths - usually it get reset on map change
	/// </summary>
	public int TemporaryDeathCount;

	/// <summary>
	/// Total deaths caused by enviroment
	/// </summary>
	public int TotalEnviromentalDeaths { get; private set; }

	/// <summary>
	/// Total deaths caused by players
	/// </summary>
	public int TotalPlayerDeaths { get; private set; }

	#endregion

	#region Public Methods

	/// <summary>
	/// Reset temporary death count back to zero
	/// </summary>
	public void ResetTemporaryDeathCount() => TemporaryDeathCount = 0;

	/// <summary>
	/// Increment total kills by 1
	/// </summary>
	public void IncrementKillCount() => TotalKills++;

	/// <summary>
	/// Increment enviromental deaths by 1
	/// </summary>
	public void IncrementEnviromentalDeathCount()
	{
		TotalEnviromentalDeaths++;
		TemporaryDeathCount++;
	}

	/// <summary>
	/// Increment player deaths by 1
	/// </summary>
	public void IncrementPlayerDeathCount()
	{
		TotalEnviromentalDeaths++;
		TemporaryDeathCount++;
	}

	#endregion
}
